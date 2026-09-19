using System;

namespace SteelHorse.Framework.Dice
{
    // Resolves a Dice roll into a pass/fail DiceCheckResult against a caller-supplied bonus
    // and value, without knowing anything about the calling game's attributes or difficulty
    // systems. TargetDiceChecker and ChanceDiceChecker are the two implementations - see
    // their own files for what each represents. allowCriticals has no default here since
    // the two implementations disagree on what it should default to.
    public interface IDiceChecker
    {
        // Fired whenever a check resolves into a result - for UI/SFX reacting to a pass/fail
        // or a critical.
        event Action<DiceCheckResult> CheckResolved;

        DiceCheckResult NormalRollCheck(Dice dice, int bonus, int value, bool allowCriticals);

        // Rolls twice and keeps whichever result favors success - see the implementation for
        // which extreme that is.
        DiceCheckResult AdvantageRollCheck(Dice dice, int bonus, int value, bool allowCriticals);

        // Rolls twice and keeps whichever result favors failure - the inverse of AdvantageRollCheck.
        DiceCheckResult DisadvantageRollCheck(Dice dice, int bonus, int value, bool allowCriticals);

        // Resolves an already-known roll into a DiceCheckResult, without rolling dice itself -
        // exposed so a caller with its own roll doesn't have to go through NormalRollCheck/etc.
        DiceCheckResult BuildResult(int roll, Dice dice, int bonus, int value, bool allowCriticals);
    }
}
