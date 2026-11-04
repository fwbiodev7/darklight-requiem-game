// DARKLIGHT: REQUIEM — Transformation Safety & Architecture Adapter
// Antigravity Support Engineering — 2026-11-04
// Audited against canonical forms: Fabius Normal, Ruptura, Eclipse Absoluto, Lucius Normal, Nexo Fenrir.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.Transformations
{
    public enum CanonicalForm
    {
        FabiusNormal = 0,
        Ruptura = 1,
        EclipseAbsoluto = 2,
        LuciusNormal = 3,
        NexoFenrir = 4
    }

    [Serializable]
    public struct TransformationModifier
    {
        public float damageMultiplier;
        public float attackSpeedMultiplier;
        public float movementSpeedMultiplier;
        public float defenseMultiplier;
        public float strikeRangeBonus;
        public float duration;
        public float energyCost;

        public static TransformationModifier DefaultFor(CanonicalForm form)
        {
            switch (form)
            {
                case CanonicalForm.Ruptura:
                    return new TransformationModifier
                    {
                        damageMultiplier = 1.5f,
                        attackSpeedMultiplier = 1.04f,
                        movementSpeedMultiplier = 1.08f,
                        defenseMultiplier = 1.12f,
                        strikeRangeBonus = 0f,
                        duration = 180f,
                        energyCost = 60f
                    };
                case CanonicalForm.EclipseAbsoluto:
                    return new TransformationModifier
                    {
                        damageMultiplier = 2.0f,
                        attackSpeedMultiplier = 1.32f,
                        movementSpeedMultiplier = 1.18f,
                        defenseMultiplier = 0.75f,
                        strikeRangeBonus = 0.15f,
                        duration = 180f,
                        energyCost = 60f
                    };
                case CanonicalForm.NexoFenrir:
                    return new TransformationModifier
                    {
                        damageMultiplier = 1.8f,
                        attackSpeedMultiplier = 1.25f,
                        movementSpeedMultiplier = 1.25f,
                        defenseMultiplier = 0.85f,
                        strikeRangeBonus = 0.2f,
                        duration = 120f,
                        energyCost = 70f
                    };
                default:
                    return new TransformationModifier
                    {
                        damageMultiplier = 1f,
                        attackSpeedMultiplier = 1f,
                        movementSpeedMultiplier = 1f,
                        defenseMultiplier = 1f,
                        strikeRangeBonus = 0f,
                        duration = 0f,
                        energyCost = 0f
                    };
            }
        }
    }

    public interface ITransformationStateController
    {
        CanonicalForm CurrentForm { get; }
        bool IsTransforming { get; }
        float RemainingTime { get; }
        bool CanTransform(CanonicalForm targetForm, float currentEnergy, bool isGrounded, bool isNeutral);
        bool RequestTransform(CanonicalForm targetForm, ref float currentEnergy);
        void RevertToNormal(bool instant = false);
        void Tick(float deltaTime);
    }

    public sealed class TransformationStateController : ITransformationStateController
    {
        public CanonicalForm CurrentForm { get; private set; } = CanonicalForm.FabiusNormal;
        public bool IsTransforming { get; private set; }
        public float RemainingTime { get; private set; }
        public TransformationModifier ActiveModifier { get; private set; }

        public event Action<CanonicalForm, CanonicalForm> OnFormChanged;
        public event Action<CanonicalForm> OnFormEnded;

        private float transitionTimer;
        private CanonicalForm pendingForm;

        public bool CanTransform(CanonicalForm targetForm, float currentEnergy, bool isGrounded, bool isNeutral)
        {
            if (IsTransforming) return false;
            if (!isNeutral) return false;
            if (targetForm == CurrentForm) return false;

            if (targetForm == CanonicalForm.FabiusNormal || targetForm == CanonicalForm.LuciusNormal)
                return true;

            var mod = TransformationModifier.DefaultFor(targetForm);
            return currentEnergy >= mod.energyCost;
        }

        public bool RequestTransform(CanonicalForm targetForm, ref float currentEnergy)
        {
            if (IsTransforming) return false;

            if (targetForm == CanonicalForm.FabiusNormal || targetForm == CanonicalForm.LuciusNormal)
            {
                RevertToNormal();
                return true;
            }

            var mod = TransformationModifier.DefaultFor(targetForm);
            if (currentEnergy < mod.energyCost) return false;

            currentEnergy -= mod.energyCost;
            pendingForm = targetForm;
            IsTransforming = true;
            transitionTimer = 1.8f; // Standard canonical enter duration
            ActiveModifier = mod;
            RemainingTime = mod.duration;

            return true;
        }

        public void RevertToNormal(bool instant = false)
        {
            var prev = CurrentForm;
            CurrentForm = (prev == CanonicalForm.NexoFenrir || prev == CanonicalForm.LuciusNormal) 
                ? CanonicalForm.LuciusNormal 
                : CanonicalForm.FabiusNormal;

            RemainingTime = 0f;
            IsTransforming = false;
            transitionTimer = 0f;
            ActiveModifier = TransformationModifier.DefaultFor(CurrentForm);

            OnFormEnded?.Invoke(prev);
            OnFormChanged?.Invoke(prev, CurrentForm);
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f) return;

            if (IsTransforming)
            {
                transitionTimer -= deltaTime;
                if (transitionTimer <= 0f)
                {
                    IsTransforming = false;
                    var prev = CurrentForm;
                    CurrentForm = pendingForm;
                    OnFormChanged?.Invoke(prev, CurrentForm);
                }
                return;
            }

            if (CurrentForm != CanonicalForm.FabiusNormal && CurrentForm != CanonicalForm.LuciusNormal)
            {
                RemainingTime -= deltaTime;
                if (RemainingTime <= 0f)
                {
                    RevertToNormal();
                }
            }
        }
    }
}

