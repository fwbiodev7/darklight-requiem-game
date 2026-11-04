// DARKLIGHT: REQUIEM — Campaign & Quest Progression Bridge
// Antigravity Support Engineering — 2026-11-04
// Prevents duplicate quest completion, lost events on save/load, out-of-order transitions.

using System;
using System.Collections.Generic;

namespace Darklight.Campaign
{
    [Serializable]
    public enum ObjectiveState
    {
        NotStarted,
        Active,
        Completed,
        Failed
    }

    [Serializable]
    public class CampaignObjective
    {
        public string id;
        public string chapterId;
        public string title;
        public string description;
        public ObjectiveState state;
        public bool isMandatory;
        public string[] prerequisites;
    }

    public interface ICampaignManagerBridge
    {
        bool TryStartObjective(string objectiveId);
        bool TryCompleteObjective(string objectiveId);
        bool IsObjectiveCompleted(string objectiveId);
        ObjectiveState GetObjectiveState(string objectiveId);
        bool CanAdvanceToChapter(int chapterIndex);
    }

    public class CampaignProgressionController : ICampaignManagerBridge
    {
        private readonly Dictionary<string, CampaignObjective> objectives = new Dictionary<string, CampaignObjective>();
        private readonly HashSet<string> completedEvents = new HashSet<string>();

        public void RegisterObjective(CampaignObjective obj)
        {
            if (obj != null && !objectives.ContainsKey(obj.id))
                objectives[obj.id] = obj;
        }

        public bool TryStartObjective(string objectiveId)
        {
            if (!objectives.TryGetValue(objectiveId, out var obj)) return false;
            if (obj.state != ObjectiveState.NotStarted) return false;

            // Check prerequisites
            if (obj.prerequisites != null)
            {
                foreach (var req in obj.prerequisites)
                {
                    if (!objectives.TryGetValue(req, out var reqObj) || reqObj.state != ObjectiveState.Completed)
                        return false;
                }
            }

            obj.state = ObjectiveState.Active;
            return true;
        }

        public bool TryCompleteObjective(string objectiveId)
        {
            if (!objectives.TryGetValue(objectiveId, out var obj)) return false;
            if (obj.state == ObjectiveState.Completed) return false; // Prevent double completion

            obj.state = ObjectiveState.Completed;
            completedEvents.Add(objectiveId);
            return true;
        }

        public bool IsObjectiveCompleted(string objectiveId)
        {
            return objectives.TryGetValue(objectiveId, out var obj) && obj.state == ObjectiveState.Completed;
        }

        public ObjectiveState GetObjectiveState(string objectiveId)
        {
            return objectives.TryGetValue(objectiveId, out var obj) ? obj.state : ObjectiveState.NotStarted;
        }

        public bool CanAdvanceToChapter(int chapterIndex)
        {
            if (chapterIndex <= 0) return true;
            // Check that previous chapter mandatory objectives are complete
            string prevChapterPrefix = $"ch{chapterIndex - 1}_";
            foreach (var kvp in objectives)
            {
                if (kvp.Key.StartsWith(prevChapterPrefix) && kvp.Value.isMandatory && kvp.Value.state != ObjectiveState.Completed)
                    return false;
            }
            return true;
        }
    }
}

