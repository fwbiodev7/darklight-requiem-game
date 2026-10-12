namespace Darklight
{
    public static class DarklightWorldScale
    {
        public const float Environment=1.325f;
        public static float EnemyHeight(RequiemEnemyKind kind)=>(int)kind>=5?3.25f:kind==RequiemEnemyKind.Armored?3.1f:kind==RequiemEnemyKind.Crawler?2.2f:2.5f;
    }
}
