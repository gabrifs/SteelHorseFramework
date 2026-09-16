using UnityEngine;

namespace SteelHorse.Framework.Dice
{
    // Game-agnostic dice roller: rolls a single die of an arbitrary face count and,
    // optionally, resolves it as a check against a target DC with a flat bonus.
    public static class DiceRoller
    {
        public static int Roll(int faces)
        {
            return Random.Range(1, faces + 1);
        }

        public static DiceCheckResult RollCheck(int faces, int bonus, int targetDC)
        {
            int roll = Roll(faces);
            int total = roll + bonus;
            bool success = total >= targetDC;

            return new DiceCheckResult(roll, bonus, total, targetDC, success);
        }
    }
}
