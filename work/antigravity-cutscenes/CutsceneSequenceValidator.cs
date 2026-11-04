// DARKLIGHT: REQUIEM — Cutscene Sequence Validator
// Validates references, states, and conditions before execution.

using System.Collections.Generic;

namespace Darklight.Cutscenes
{
    /// <summary>
    /// Validates cutscene sequences and their dependencies.
    /// </summary>
    public static class CutsceneSequenceValidator
    {
        /// <summary>
        /// Validate a sequence thoroughly: references, state conditions, events.
        /// </summary>
        public static CutsceneValidationResult Validate(ICutsceneSequence sequence, CutsceneContext context)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            if (sequence == null)
            {
                errors.Add("Sequence is null");
                return new CutsceneValidationResult { Valid = false, Errors = errors.ToArray(), Warnings = warnings.ToArray() };
            }

            if (string.IsNullOrEmpty(sequence.Id))
                errors.Add("Sequence has no Id");

            if (string.IsNullOrEmpty(sequence.DisplayName))
                warnings.Add("Sequence has no DisplayName");

            // Context validation
            if (context == null)
            {
                errors.Add("CutsceneContext is null");
            }
            else
            {
                if (context.InputGate == null)
                    warnings.Add("No InputGate provided — input will not be blocked");

                if (context.Camera == null)
                    warnings.Add("No ICutsceneCamera provided — camera control unavailable");

                if (context.EventBridge == null)
                    warnings.Add("No EventBridge provided — events will not be emitted");
            }

            // Sequence self-validation
            var selfValidation = sequence.Validate();
            if (!selfValidation.Valid)
            {
                errors.AddRange(selfValidation.Errors);
            }
            if (selfValidation.Warnings != null)
                warnings.AddRange(selfValidation.Warnings);

            return new CutsceneValidationResult
            {
                Valid = errors.Count == 0,
                Errors = errors.ToArray(),
                Warnings = warnings.ToArray()
            };
        }

        /// <summary>
        /// Validate a batch of sequences for a chapter (e.g., all cutscenes in chapter 1).
        /// </summary>
        public static Dictionary<string, CutsceneValidationResult> ValidateBatch(
            IEnumerable<ICutsceneSequence> sequences, CutsceneContext context)
        {
            var results = new Dictionary<string, CutsceneValidationResult>();
            var seenIds = new HashSet<string>();

            foreach (var seq in sequences)
            {
                var result = Validate(seq, context);

                // Check for duplicate IDs
                if (!string.IsNullOrEmpty(seq?.Id))
                {
                    if (!seenIds.Add(seq.Id))
                    {
                        var errors = new List<string>(result.Errors) { $"Duplicate sequence ID: {seq.Id}" };
                        result = new CutsceneValidationResult { Valid = false, Errors = errors.ToArray(), Warnings = result.Warnings };
                    }
                }

                results[seq?.Id ?? "null"] = result;
            }

            return results;
        }
    }
}

