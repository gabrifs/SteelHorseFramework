using UnityEngine;

namespace SteelHorse.Framework.Dice
{
    // Game-agnostic dice roller: rolls a single die of an arbitrary face count and,
    // optionally, resolves it as a check against a target DC with a flat bonus.
    public static class DiceRoller
    {
        public static int DiceRoll(int faces)
        {
            return Random.Range(1, faces + 1);
        }

        public static int MultiDiceRoll(int faces, int amount)
        {
            int total = 0;

            for (int i = 0; i < amount; i++)
                total += DiceRoll(faces);

            return total;
        }

        public static DiceCheckResult RollCheck(int faces, int bonus, int targetDC, bool allowCriticals = true)
        {
            int roll = DiceRoll(faces);
            return BuildResult(roll, faces, bonus, targetDC, allowCriticals);
        }

        // Rolls twice and keeps the higher result for the check.
        public static DiceCheckResult AdvantageRollCheck(int faces, int bonus, int targetDC, bool allowCriticals = true)
        {
            int roll = Mathf.Max(DiceRoll(faces), DiceRoll(faces));
            return BuildResult(roll, faces, bonus, targetDC, allowCriticals);
        }

        // Rolls twice and keeps the lower result for the check.
        public static DiceCheckResult DisadvantageRollCheck(int faces, int bonus, int targetDC, bool allowCriticals = true)
        {
            int roll = Mathf.Min(DiceRoll(faces), DiceRoll(faces));
            return BuildResult(roll, faces, bonus, targetDC, allowCriticals);
        }

        // With allowCriticals (default true), rolling the max face is an automatic success and
        // rolling a 1 is an automatic failure, regardless of bonus/targetDC - pass false to fall
        // back to a plain Total >= targetDC comparison for checks that shouldn't have crits.
        private static DiceCheckResult BuildResult(int roll, int faces, int bonus, int targetDC, bool allowCriticals)
        {
            int total = roll + bonus;
            bool isCritical = allowCriticals && (roll == faces || roll == 1);

            bool success = isCritical ? roll == faces : total >= targetDC;

            return new DiceCheckResult(roll, bonus, total, targetDC, success, isCritical);
        }
    }
}
