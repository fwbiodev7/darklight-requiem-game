using System;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight
{
    sealed class DarklightLadyPresentation
    {
        const string Resource = "Darklight/OfficialCast/LadySupport";
        static readonly string[] Directions = { "S", "SW", "W", "NW", "N", "NE", "E", "SE" };
        static Sprite[,,] frames;
        static Vector2[] muzzles;
        [Serializable] sealed class MuzzleCalibration { public Vector2[] frames; }

        readonly SpriteRenderer art;
        readonly Sprite idle;
        Vector2 lastPosition;
        Vector2 walkHeading;
        float stride, defeatTime, motionGrace;
        int direction;

        public DarklightLadyPresentation(SpriteRenderer renderer, Vector2 position)
        {
            art = renderer;
            idle = renderer.sprite;
            lastPosition = position;
            if (frames != null) return;
            var named = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            foreach (Sprite sprite in Resources.LoadAll<Sprite>(Resource)) named[sprite.name] = sprite;
            frames = new Sprite[2, Directions.Length, 5];
            for (int action = 0; action < 2; action++)
                for (int way = 0; way < Directions.Length; way++)
                    for (int frame = 0; frame < 5; frame++)
                    {
                        string clip = action == 0 ? "Walk" : "Shot";
                        named.TryGetValue($"Lady_{clip}_{Directions[way]}_{frame}", out frames[action, way, frame]);
                    }
            TextAsset metadata = Resources.Load<TextAsset>(Resource + "/Muzzles");
            if (metadata != null) muzzles = JsonUtility.FromJson<MuzzleCalibration>(metadata.text)?.frames;
        }

        public void Present(Vector2 position, Vector2 velocity, CombatModel model, AttackDefinition attack,float delta)
        {
            Vector2 movement = position - lastPosition;
            lastPosition = position;
            if (model.State == CombatState.Attack)
            {
                stride = 0;
                motionGrace=0;
                direction = DarklightCharacterView.DirectionIndex(model.Facing);
                int phase = DarklightCastActionFrames.AttackPhase(model, attack);
                art.sprite = frames[1, direction, phase] ?? idle;
            }
            else if (velocity.sqrMagnitude > .01f)
            {
                if(movement.sqrMagnitude>.000001f)
                {
                    Vector2 heading=movement.normalized;
                    if(walkHeading==Vector2.zero||Vector2.Dot(walkHeading,heading)<0)
                        walkHeading=heading;
                    else walkHeading=Vector2.Lerp(walkHeading,heading,1-Mathf.Exp(-20f*delta)).normalized;
                    direction=DarklightCharacterView.DirectionIndex(walkHeading);
                    stride=Mathf.Repeat(stride+movement.magnitude,1.4f);
                    motionGrace=.12f;
                }
                else motionGrace=Mathf.Max(0,motionGrace-delta);

                // Frames rendered between physics steps keep the current foot placement.
                if(motionGrace>0)
                    art.sprite=frames[0,direction,Mathf.Min(4,(int)(stride/1.4f*5))]??idle;
                else PresentIdle();
            }
            else PresentIdle();
        }

        void PresentIdle()
        {
            stride=motionGrace=0;
            walkHeading=Vector2.zero;
            art.sprite=direction==0?idle:frames[1,direction,0]??idle;
        }

        public Vector2 Muzzle(Vector2 facing)
        {
            int way = DarklightCharacterView.DirectionIndex(facing);
            Vector2 offset = muzzles?.Length == Directions.Length ? muzzles[way] : Vector2.up * .65f;
            return art.transform.TransformPoint(offset);
        }

        public void Pause(Vector2 position) => lastPosition = position;
        public void ResetDefeat() { defeatTime=0;art.sprite=idle; }

        public void PresentDefeat(float delta)
        {
            defeatTime += delta;
            var sprite = DarklightCastActionFrames.Get("Lady", "Defeat", direction, DarklightCastActionFrames.FallPhase(defeatTime));
            if (sprite != null) art.sprite = sprite;
        }
    }
}
