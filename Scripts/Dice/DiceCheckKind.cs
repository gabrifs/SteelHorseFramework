namespace SteelHorse.Framework.Dice
{
    // Which IDiceChecker produced a DiceCheckResult - lets the result format its own
    // breakdown correctly in ToString(), since Target and Chance checks apply bonus
    // differently (see ChanceDiceChecker).
    public enum DiceCheckKind
    {
        Target,
        Chance
    }
}
