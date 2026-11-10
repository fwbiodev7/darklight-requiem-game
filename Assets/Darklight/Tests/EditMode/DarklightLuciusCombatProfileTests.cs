using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightLuciusCombatProfileTests
    {
        readonly List<UnityEngine.Object> owned=new();
        [TearDown] public void Dispose()
        {foreach(var item in owned)if(item!=null)UnityEngine.Object.DestroyImmediate(item);owned.Clear();}
        AttackDefinition[] Bases()
        {
            var sword=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Darklight/Data/Weapons/Requiem.asset");
            Assert.That(sword,Is.Not.Null);Assert.That(sword.loadout.lights,Has.Length.EqualTo(3));
            foreach(var attack in sword.loadout.lights)Assert.That(attack,Is.Not.Null);
            return sword.loadout.lights;
        }
        static AttackDefinition[] Slots(CombatLoadout loadout)=>loadout.lights.Concat(new[]{
            loadout.heavy,loadout.charged,loadout.counter,loadout.skill,loadout.aerial,loadout.weaponSpecial,
            loadout.pauseBranch,loadout.heavyBranch,loadout.swapEntry,loadout.airFinisher}).ToArray();

        [Test] public void ExactlyThreeClonesServeEveryInputAlias()
        {
            var source=Bases();var loadout=DarklightLuciusCombatProfile.Create(source,owned.Add);
            Assert.That(owned,Has.Count.EqualTo(3));Assert.That(loadout.lights,Has.Length.EqualTo(3));
            Assert.That(Slots(loadout).Distinct().Count(),Is.EqualTo(3));
            foreach(var slot in Slots(loadout))Assert.That(loadout.lights,Does.Contain(slot));
            for(int i=0;i<3;i++)Assert.That(loadout.lights[i],Is.Not.SameAs(source[i]));
            Assert.That(loadout.heavy,Is.SameAs(loadout.lights[1]));Assert.That(loadout.charged,Is.SameAs(loadout.heavy));
            Assert.That(loadout.counter,Is.SameAs(loadout.lights[2]));Assert.That(loadout.skill,Is.SameAs(loadout.lights[0]));
        }

        [Test] public void StableKeysAndThreeSwordPatternsHaveNoFourthVariation()
        {
            Assert.That(DarklightLuciusCombatProfile.VariationCount,Is.EqualTo(3));
            Assert.That(new[]{DarklightLuciusCombatProfile.SwordHorizontal,DarklightLuciusCombatProfile.SwordVertical,
                DarklightLuciusCombatProfile.SwordThrust},Is.EqualTo(new[]{"SwordHorizontal","SwordVertical","SwordThrust"}));
            var loadout=DarklightLuciusCombatProfile.Create(Bases(),owned.Add);
            Assert.That(loadout.lights.Select(a=>a.animationKey),Is.EqualTo(new[]{"SwordHorizontal","SwordVertical","SwordThrust"}));
            Assert.That(loadout.lights.Select(a=>a.displayName),Is.EqualTo(new[]{"Corte Horizontal","Corte Vertical","Estocada"}));
            Assert.That(loadout.lights.Select(a=>a.hitShape),Is.EqualTo(new[]{HitShape.Sweep,HitShape.Sector,HitShape.Thrust}));
        }

        [Test] public void NativeFabiusSourcesStayIntactAndAllOtherCombatFieldsAreCopied()
        {
            var source=Bases();var before=source.Select(a=>EditorJsonUtility.ToJson(a)).ToArray();
            var loadout=DarklightLuciusCombatProfile.Create(source,owned.Add);
            for(int i=0;i<3;i++)
            {
                Assert.That(EditorJsonUtility.ToJson(source[i]),Is.EqualTo(before[i]),source[i].name);
                // Normalize only the four intentional identity/pattern changes
                // on a disposable comparison copy, then compare every field.
                var expected=UnityEngine.Object.Instantiate(source[i]);owned.Add(expected);
                expected.name=loadout.lights[i].name;expected.displayName=loadout.lights[i].displayName;
                expected.animationKey=loadout.lights[i].animationKey;expected.hitShape=loadout.lights[i].hitShape;
                Assert.That(JsonUtility.ToJson(loadout.lights[i]),Is.EqualTo(JsonUtility.ToJson(expected)),source[i].name);
            }
            // The caller may tune a session clone later without mutating Fabius.
            loadout.lights[0].damage+=1;
            Assert.That(source.Select(a=>EditorJsonUtility.ToJson(a)),Is.EqualTo(before));
        }

        [Test] public void RecreatingTheSameContractKeepsThreeKeysWithoutImplementingAnotherForm()
        {
            var source=Bases();var first=DarklightLuciusCombatProfile.Create(source,owned.Add);
            var second=DarklightLuciusCombatProfile.Create(source,owned.Add);
            Assert.That(owned,Has.Count.EqualTo(6));Assert.That(Slots(second).Distinct().Count(),Is.EqualTo(3));
            Assert.That(second.lights.Select(a=>a.animationKey),Is.EqualTo(first.lights.Select(a=>a.animationKey)));
            for(int i=0;i<3;i++)Assert.That(second.lights[i],Is.Not.SameAs(first.lights[i]));
        }

        [Test] public void InvalidInputsFailBeforeAllocatingAttackDefinitions()
        {
            Assert.Throws<ArgumentException>(()=>DarklightLuciusCombatProfile.Create(null,owned.Add));
            Assert.Throws<ArgumentException>(()=>DarklightLuciusCombatProfile.Create(new AttackDefinition[4],owned.Add));
            Assert.Throws<ArgumentException>(()=>DarklightLuciusCombatProfile.Create(new AttackDefinition[3],owned.Add));
            Assert.Throws<ArgumentNullException>(()=>DarklightLuciusCombatProfile.Create(Bases(),null));
            Assert.That(owned,Is.Empty);
        }
    }
}
