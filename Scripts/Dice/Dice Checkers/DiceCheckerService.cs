namespace SteelHorse.Framework.Dice
{
    // Lazily-constructed access point for the two IDiceChecker implementations. Deliberately
    // not registered in ServiceLocator - dice checking is optional/situational rather than a
    // true cross-project framework service, so it's a standalone static instead (same idea as
    // BeneathTheEternalFlame.Dungeon.DungeonLog, just lazy instead of eager).
    public static class DiceCheckerService
    {
        public static IDiceChecker Target { get { return _target ??= new TargetDiceChecker(); } }
        public static IDiceChecker Chance { get { return _chance ??= new ChanceDiceChecker(); } }

        private static IDiceChecker _target;
        private static IDiceChecker _chance;
    }
}
