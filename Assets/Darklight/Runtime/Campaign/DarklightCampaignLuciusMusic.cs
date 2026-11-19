using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;

namespace Darklight
{
    // Private local installation only. No commercial audio is embedded in Assets
    // or a distributable build. Playback uses the existing campaign event source.
    public static class DarklightLuciusMusicTrack
    {
        public const string FileName="LuciusBoss.mp3";
        public const string Sha256="f276067f7d821f4d71fd45f335f714605fd1f5a1e38784666c17bf9301f7d2ae";
        public static string LocalPath(string persistentRoot)=>Path.Combine(persistentRoot,"PrivateMusic",FileName);
        public static bool Select(bool lucius,bool opponentAlive,bool resolved,bool playerAlive,bool loading,bool frontEnd)
            =>lucius&&opponentAlive&&!resolved&&playerAlive&&!loading&&!frontEnd;
        public static bool CanPlay(bool active,bool silent)=>active&&!silent;
        public static bool MatchesSuppliedFile(string path)
        {
            if(!File.Exists(path))return false;
            using(var input=File.OpenRead(path))using(var hash=SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(input)).Replace("-","").ToLowerInvariant()==Sha256;
        }
    }

    public sealed partial class DarklightCampaign
    {
        AudioClip luciusBossTrack;
        DarklightEnemy luciusMusicEncounter;
        UnityWebRequest luciusMusicRequest;
        Coroutine luciusMusicLoad;
        int luciusMusicSample;
        bool luciusMusicOwned,luciusMusicEnding,luciusMusicUnavailable;
        public bool LuciusBossMusicHasPriority=>luciusMusicOwned&&!luciusMusicEnding&&music!=null&&music.clip==luciusBossTrack;

        // Called by the existing skin/event mixer, including while menus pause
        // the campaign. Phase changes retain the same opponent and playhead.
        public void EvaluateLuciusBossMusic(float dt)
        {
            if(music==null)return;
            if(luciusMusicOwned&&music.clip==luciusBossTrack&&music.isPlaying)luciusMusicSample=music.timeSamples;
            var opponent=CurrentBoss;
            bool selected=DarklightLuciusMusicTrack.Select(opponent!=null&&opponent.IsLucius,
                opponent!=null&&opponent.Health!=null&&!opponent.Health.IsDead,opponent!=null&&opponent.Resolved,
                health==null||!health.IsDead,PhaseLoading,CinematicFront||panel=="Victory"||panel=="GameOver");
            if(luciusMusicOwned&&music.clip!=luciusBossTrack)
            {
                // Resonance/story events may borrow this same source. Do not
                // stop their clip or leave the boss's loop setting on it.
                music.loop=false;luciusMusicOwned=luciusMusicEnding=false;
            }
            if(!selected)
            {
                luciusMusicEncounter=null;CancelLuciusMusicRequest();
                if(!Active){ReleaseLuciusBossMusic();return;}
                if(luciusMusicOwned&&!luciusMusicEnding)
                {luciusMusicEnding=true;SkinMusic?.FinishEvent();}
                if(luciusMusicOwned&&luciusMusicEnding&&(SkinMusic==null||SkinMusic.EventWeight<=0))
                    ResetLuciusMusicPlayback();
                if(!luciusMusicOwned)ReleaseLuciusBossMusic();
                return;
            }
            if(opponent!=luciusMusicEncounter)
            {
                // A restarted duel gets a fresh start; a new boss phase does not.
                ResetLuciusMusicPlayback();luciusMusicEncounter=opponent;
            }
            if(!Active)
            {if(luciusMusicOwned)music.Pause();return;}
            if(luciusBossTrack==null)
            {
                if(luciusMusicLoad==null&&!luciusMusicUnavailable)
                    luciusMusicLoad=StartCoroutine(LoadLuciusMusic());
                return;
            }
            if(!luciusMusicOwned)
            {
                // Preserve the existing higher-priority story/resonance event,
                // including its fade-out. Transform themes yield to this event.
                if(Resonance.Active>=0||firstAwakeningMusicPending||HasFirstAwakeningMusic&&music.isPlaying||
                    SkinMusic!=null&&SkinMusic.EventWeight>.0001f)return;
                music.Stop();music.clip=luciusBossTrack;music.loop=true;music.volume=0;
                if(luciusMusicSample>0)music.timeSamples=Mathf.Clamp(luciusMusicSample,0,luciusBossTrack.samples-1);
                luciusMusicOwned=true;luciusMusicEnding=false;SkinMusic?.BeginEvent();
            }
            if(!DarklightLuciusMusicTrack.CanPlay(Active,DarklightAutomation.Silent))
            {music.volume=0;music.Pause();return;}
            music.UnPause();if(!music.isPlaying)music.Play();
        }

        IEnumerator LoadLuciusMusic()
        {
            // Yield before doing local IO so the coroutine handle is assigned
            // even when a missing/private file ends the request immediately.
            yield return null;
            string path=DarklightLuciusMusicTrack.LocalPath(Application.persistentDataPath);
            bool valid=false;
            try{valid=DarklightLuciusMusicTrack.MatchesSuppliedFile(path);}
            catch(IOException){valid=false;}
            catch(UnauthorizedAccessException){valid=false;}
            if(!valid)
            {
                luciusMusicUnavailable=true;luciusMusicLoad=null;
                Debug.LogWarning("Private Lucius music is unavailable; keeping the existing campaign music.");yield break;
            }
            var request=UnityWebRequestMultimedia.GetAudioClip(new Uri(path).AbsoluteUri,AudioType.MPEG);
            ((DownloadHandlerAudioClip)request.downloadHandler).streamAudio=true;
            luciusMusicRequest=request;yield return request.SendWebRequest();
            if(request.result==UnityWebRequest.Result.Success)
            {
                luciusBossTrack=DownloadHandlerAudioClip.GetContent(request);
                if(luciusBossTrack!=null)luciusBossTrack.name="LuciusBossPrivate";
                else luciusMusicUnavailable=true;
            }
            else
            {
                luciusMusicUnavailable=true;
                Debug.LogWarning("Private Lucius music could not be decoded; keeping the existing campaign music.");
            }
            request.Dispose();luciusMusicRequest=null;luciusMusicLoad=null;
        }
        void CancelLuciusMusicRequest()
        {
            if(luciusMusicRequest!=null){luciusMusicRequest.Abort();luciusMusicRequest.Dispose();luciusMusicRequest=null;}
            if(luciusMusicLoad!=null){StopCoroutine(luciusMusicLoad);luciusMusicLoad=null;}
        }
        void ResetLuciusMusicPlayback()
        {
            if(luciusMusicOwned&&music!=null&&music.clip==luciusBossTrack)
            {music.Stop();music.clip=null;music.loop=false;}
            luciusMusicOwned=luciusMusicEnding=false;luciusMusicSample=0;
        }
        public void ReleaseLuciusBossMusic()
        {
            CancelLuciusMusicRequest();ResetLuciusMusicPlayback();luciusMusicEncounter=null;
            if(luciusBossTrack!=null)Destroy(luciusBossTrack);
            luciusBossTrack=null;luciusMusicUnavailable=false;
        }
    }
}
