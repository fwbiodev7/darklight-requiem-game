using UnityEngine;

namespace Darklight
{
    public sealed partial class DarklightProgress
    {
        // Optional v2 fields: older saves keep their authored chapter checkpoint.
        public bool hasReturnPoint;
        public int returnChapter, returnCheckpoint;
        public Vector2 returnPoint;

        public bool RecordReturnPoint(int chapterIndex, int checkpointIndex, Vector2 point)
        {
            if (chapterIndex < 0 || chapterIndex >= ChapterCapacity || checkpointIndex < 0 || checkpointIndex > 4 || !Finite(point)) return false;
            returnChapter = chapterIndex;
            returnCheckpoint = checkpointIndex;
            returnPoint = point;
            hasReturnPoint = true;
            return true;
        }

        public bool TryReturnPoint(int chapterIndex, int checkpointIndex, out Vector2 point)
        {
            point = returnPoint;
            return hasReturnPoint && returnChapter == chapterIndex && returnCheckpoint == checkpointIndex &&
                returnChapter >= 0 && returnChapter < ChapterCapacity && returnCheckpoint >= 0 && returnCheckpoint <= 4 && Finite(point);
        }

        static bool Finite(Vector2 point) => !float.IsNaN(point.x) && !float.IsNaN(point.y) &&
            !float.IsInfinity(point.x) && !float.IsInfinity(point.y);
    }
}
