using System;
using UnityEngine;

namespace SteelHorse.Framework.Dice
{
    // Resolves a Dice roll as a flat percentage-style chance of success - the inverse of
    // TargetDiceChecker: success needs the roll to be equal to or under value (e.g. a 25%
    // chance rolled as NormalRollCheck(new Dice(100), bonus: 0, value: 25, ...)). Unlike
    // TargetDiceChecker, bonus raises the chance threshold instead of the roll - a positive
    // bonus should make success more likely, and since a lower roll is what succeeds here,
    // adding it to the roll would work against the caller instead of for them. Advantage/
    // Disadvantage and critical extremes are flipped from TargetDiceChecker's for the same
    // roll-under reason.
    public sealed class ChanceDiceChecker : IDiceChecker
    {
        public event Action<DiceCheckResult> CheckResolved;

        public DiceCheckResult NormalRollCheck(Dice dice, int bonus, int value, bool allowCriticals)
        {
            int roll = dice.Roll();
            return BuildResult(roll, dice, bonus, value, allowCriticals);
        }

        // Rolls twice and keeps the lower result - advantage favors success on a roll-under
        // check, so it keeps the roll closer to the minimum instead of the maximum.
        public DiceCheckResult AdvantageRollCheck(Dice dice, int bonus, int value, bool allowCriticals)
        {
            int roll = Mathf.Min(dice.Roll(), dice.Roll());
            return BuildResult(roll, dice, bonus, value, allowCriticals);
        }

        // Rolls twice and keeps the higher result.
        public DiceCheckResult DisadvantageRollCheck(Dice dice, int bonus, int value, bool allowCriticals)
        {
            int roll = Mathf.Max(dice.Roll(), dice.Roll());
            return BuildResult(roll, dice, bonus, value, allowCriticals);
        }

        // Succeeds when roll <= chance, where chance is value raised by bonus (rather than
        // Total being roll + bonus, as TargetDiceChecker does it - see the class comment).
        // With allowCriticals true, rolling the minimum possible total (every die a 1) is an
        // automatic success and rolling the maximum possible total (every die maxed) is an
        // automatic failure - the inverse of TargetDiceChecker's extremes, since a low roll is
        // what succeeds on a Chance check.
        public DiceCheckResult BuildResult(int roll, Dice dice, int bonus, int value, bool allowCriticals)
        {
            int total = roll;
            int maxRoll = dice.Faces * dice.Amount;
            int minRoll = dice.Amount;
            int chance = value + bonus;
            bool isCritical = allowCriticals && (roll == minRoll || roll == maxRoll);

            bool success = isCritical ? roll == minRoll : total <= chance;

            DiceCheckResult result = new DiceCheckResult(roll, bonus, total, chance, success, isCritical, DiceCheckKind.Chance);
            CheckResolved?.Invoke(result);

            return result;
        }
    }
}
