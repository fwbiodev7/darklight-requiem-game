using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightLuciusSpriteBankTests
    {
        static DarklightLuciusSpriteCatalog Valid()=>new(){schemaVersion=1,characterId="Lucius",form="Normal",weapon="Sword",complete=false,
            pages=Enumerable.Range(0,8).Select(d=>new DarklightLuciusSpritePage{
                id="LuciusNormalIdle8V1_Idle_"+d,action="Idle",direction=d,
                resourcePath=DarklightLuciusSpriteBank.Root+"LuciusNormalIdle8V1",
                imageSha256=new string('a',64),sourceSha256=new string('b',64),nativeSize=new[]{1024,1024},
                pixelsPerUnit=128,spriteNames=new[]{"Redesign_LuciusNormalIdle8V1_"+d.ToString("00")},
                durationMs=new[]{350},loop=true,wholeFigures=true,status="visual-reviewed"}).ToArray()};
        [TearDown] public void Clear()=>DarklightLuciusSpriteBank.ClearCache();
        [Test] public void EightReviewedIdleViewsActivateWithoutClaimingCompleteKit()
        {var data=Valid();Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.True);Assert.That(data.complete,Is.False);}
        [Test] public void DuplicateContextsAndIdsAreRejected()
        {
            var data=Valid();data.pages[7].direction=6;Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages[7].id=data.pages[6].id;Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages=data.pages.Take(7).ToArray();Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
        }
        [TestCase("Lucien","Normal","Sword")][TestCase("Lucius","Fenrir","Sword")][TestCase("Lucius","Normal","Katana")]
        public void CharacterFormAndWeaponCannotAddressOtherBanks(string identity,string form,string weapon)
        {var data=Valid();data.characterId=identity;data.form=form;data.weapon=weapon;Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);}
        [TestCase("Darklight/SecondaryCharacters/Lucien")][TestCase("Darklight/LuciusNormal20261109/../Lucien")]
        [TestCase("Darklight/LuciusNormal20261109/Sheet.png")][TestCase("Darklight/LuciusNormal20261109/")]
        public void ResourcePathsStayWithinTheirOwnNormalizedPacket(string path)
        {var data=Valid();data.pages[0].resourcePath=path;Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);}
        [Test] public void ReviewHashesWholeFiguresAndNormalizedNamesAreRequired()
        {
            var data=Valid();data.complete=true;data.pages[0].status="candidate";Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False,"complete cannot approve candidate art");
            data=Valid();data.pages[0].wholeFigures=false;Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages[0].sourceSha256="missing";Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages[0].imageSha256=new string('z',64);Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages[0].spriteNames[0]="Redesign_Lucien_00";Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages[0].id="Lucius Idle";Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
        }
        [Test] public void DirectionsDensitiesTimingsAndActionsAreValidated()
        {
            var data=Valid();data.pages[0].direction=8;Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages[0].pixelsPerUnit=float.NaN;Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages[0].pixelsPerUnit=0;Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages[0].durationMs=new[]{0};Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages[0].durationMs=Array.Empty<int>();Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages[0].action="Unknown";Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
        }
        [Test] public void SharedFrameClockLoopsOrHoldsLastNativePose()
        {
            var page=Valid().pages[0];page.spriteNames=new[]{"Redesign_Lucius_A","Redesign_Lucius_B"};page.durationMs=new[]{100,200};
            Assert.That(DarklightLuciusSpriteBank.Frame(page,.15f),Is.EqualTo(1));
            Assert.That(DarklightLuciusSpriteBank.Frame(page,.35f),Is.Zero);
            page=Valid().pages[0];page.spriteNames=new[]{"Redesign_Lucius_A","Redesign_Lucius_B"};page.durationMs=new[]{100,200};page.loop=false;
            Assert.That(DarklightLuciusSpriteBank.Frame(page,10),Is.EqualTo(1));
        }
        [Test] public void InitialResourcesResolveEightNativeIdleViewsAndReportFallback()
        {
            var text=Resources.Load<TextAsset>(DarklightLuciusSpriteBank.Root+"catalog");Assert.That(text,Is.Not.Null);
            var data=JsonUtility.FromJson<DarklightLuciusSpriteCatalog>(text.text);
            Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.True);
            var pages=data.pages.Where(p=>p.action=="Idle").ToArray();Assert.That(pages,Has.Length.EqualTo(8));
            foreach(var page in pages)
            {
                Assert.That(page.spriteNames,Has.Length.EqualTo(1),"Initial kit provides static views, not an invented idle cycle");
                Assert.That(DarklightLuciusSpriteBank.TryPose("Idle",page.direction,5,out var idle,out bool fallback),Is.True);
                Assert.That(fallback,Is.False);Assert.That(idle.name,Is.EqualTo(page.spriteNames[0]));
                Assert.That(idle.texture.width,Is.EqualTo(page.nativeSize[0]));Assert.That(idle.texture.height,Is.EqualTo(page.nativeSize[1]));
                Assert.That(idle.pixelsPerUnit,Is.EqualTo(page.pixelsPerUnit).Within(.001f));
                Assert.That(DarklightLuciusSpriteBank.TryPose("Walk",page.direction,1,out var walk,out fallback),Is.True);
                Assert.That(fallback,Is.True);Assert.That(walk,Is.SameAs(idle));
            }
        }
    }
}
