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

        public static DiceCheckResult RollCheck(int faces, int bonus, int targetDC)
        {
            int roll = DiceRoll(faces);
            return BuildResult(roll, faces, bonus, targetDC);
        }

        // Rolls twice and keeps the higher result for the check.
        public static DiceCheckResult AdvantageRollCheck(int faces, int bonus, int targetDC)
        {
            int roll = Mathf.Max(DiceRoll(faces), DiceRoll(faces));
            return BuildResult(roll, faces, bonus, targetDC);
        }

        // Rolls twice and keeps the lower result for the check.
        public static DiceCheckResult DisadvantageRollCheck(int faces, int bonus, int targetDC)
        {
            int roll = Mathf.Min(DiceRoll(faces), DiceRoll(faces));
            return BuildResult(roll, faces, bonus, targetDC);
        }

        private static DiceCheckResult BuildResult(int roll, int faces, int bonus, int targetDC)
        {
            int total = roll + bonus;
            bool success = total >= targetDC;
            bool isCriticalSuccess = roll == faces;
            bool isCriticalFailure = roll == 1;

            return new DiceCheckResult(roll, bonus, total, targetDC, success, isCriticalSuccess, isCriticalFailure);
        }
    }
}
