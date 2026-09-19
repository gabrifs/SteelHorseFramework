using System;
using UnityEngine;

namespace SteelHorse.Framework.Dice
{
    // Resolves a Dice roll as a pass/fail check against a target value - success needs the
    // roll (plus bonus) to be equal to or over value.
    public sealed class TargetDiceChecker : IDiceChecker
    {
        public event Action<DiceCheckResult> CheckResolved;

        public DiceCheckResult NormalRollCheck(Dice dice, int bonus, int value, bool allowCriticals)
        {
            int roll = dice.Roll();
            return BuildResult(roll, dice, bonus, value, allowCriticals);
        }

        // Rolls twice and keeps the higher result for the check.
        public DiceCheckResult AdvantageRollCheck(Dice dice, int bonus, int value, bool allowCriticals)
        {
            int roll = Mathf.Max(dice.Roll(), dice.Roll());
            return BuildResult(roll, dice, bonus, value, allowCriticals);
        }

        // Rolls twice and keeps the lower result for the check.
        public DiceCheckResult DisadvantageRollCheck(Dice dice, int bonus, int value, bool allowCriticals)
        {
            int roll = Mathf.Min(dice.Roll(), dice.Roll());
            return BuildResult(roll, dice, bonus, value, allowCriticals);
        }

        // With allowCriticals true, rolling the maximum possible total (every die in the Dice
        // maxed) is an automatic success and rolling the minimum possible total (every die a 1)
        // is an automatic failure, regardless of bonus/value - pass false to fall back to a
        // plain Total >= value comparison for checks that shouldn't have crits.
        public DiceCheckResult BuildResult(int roll, Dice dice, int bonus, int value, bool allowCriticals)
        {
            int total = roll + bonus;
            int maxRoll = dice.Faces * dice.Amount;
            int minRoll = dice.Amount;
            bool isCritical = allowCriticals && (roll == maxRoll || roll == minRoll);

            bool success = isCritical ? roll == maxRoll : total >= value;

            DiceCheckResult result = new DiceCheckResult(roll, bonus, total, value, success, isCritical, DiceCheckKind.Target);
            CheckResolved?.Invoke(result);

            return result;
        }
    }
}
