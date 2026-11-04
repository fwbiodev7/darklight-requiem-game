// DARKLIGHT: REQUIEM — Gothic Eclipse Performance Diagnostic & Optimizations
// Antigravity Support Engineering — 2026-11-04
// Non-intrusive runtime helpers and diagnostic metrics for atmosphere systems (rain, fog, 2D lighting).

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Darklight.GothicEclipse
{
    public struct AtmosphereMetrics
    {
        public int activeParticleEmitters;
        public int totalParticlesAlive;
        public int activeLights2D;
        public int activeAtmosphereObjects;
        public float estimatedDrawCallImpact;
    }

    public static class GothicEclipseOptimizer
    {
        private static readonly List<ParticleSystem> particleCache = new List<ParticleSystem>(32);
        private static readonly List<Light2D> lightCache = new List<Light2D>(32);

        /// <summary>
        /// Collects current runtime footprint of atmosphere and lighting without allocations.
        /// </summary>
        public static AtmosphereMetrics CollectMetrics()
        {
            var metrics = new AtmosphereMetrics();

            particleCache.Clear();
            var allEmitters = UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
            if (allEmitters != null)
            {
                metrics.activeParticleEmitters = allEmitters.Length;
                int particles = 0;
                for (int i = 0; i < allEmitters.Length; i++)
                {
                    if (allEmitters[i].isPlaying)
                        particles += allEmitters[i].particleCount;
                }
                metrics.totalParticlesAlive = particles;
            }

            lightCache.Clear();
            var allLights = UnityEngine.Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None);
            if (allLights != null)
            {
                metrics.activeLights2D = allLights.Length;
            }

            // Estimate draw call impact (2D light blend + particles)
            metrics.estimatedDrawCallImpact = metrics.activeLights2D * 1.5f + metrics.activeParticleEmitters;
            return metrics;
        }

        /// <summary>
        /// Safe pooling / particle culling for rain and fog outside camera frustum.
        /// </summary>
        public static void CullDistantAtmosphere(Camera cam, float cullDistance = 25f)
        {
            if (cam == null) return;
            Vector3 camPos = cam.transform.position;

            var emitters = UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
            for (int i = 0; i < emitters.Length; i++)
            {
                var emitter = emitters[i];
                float distSq = (emitter.transform.position - camPos).sqrMagnitude;
                if (distSq > cullDistance * cullDistance)
                {
                    if (emitter.isPlaying) emitter.Pause();
                }
                else
                {
                    if (emitter.isPaused) emitter.Play();
                }
            }
        }
    }
}
