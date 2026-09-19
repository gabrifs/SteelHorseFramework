namespace SteelHorse.Framework.Dice
{
    // Outcome of an IDiceChecker check call - carries the raw roll alongside the
    // resolved total/success so callers don't need to redo the DC comparison themselves.
    public readonly struct DiceCheckResult
    {
        public int Roll { get { return _roll; } }
        public int Bonus { get { return _bonus; } }
        public int Total { get { return _total; } }
        public int TargetDC { get { return _targetDC; } }
        public bool Success { get { return _success; } }
        public bool IsCritical { get { return _isCritical; } }
        public DiceCheckKind Kind { get { return _kind; } }

        private readonly int _roll;
        private readonly int _bonus;
        private readonly int _total;
        private readonly int _targetDC;
        private readonly bool _success;
        private readonly bool _isCritical;
        private readonly DiceCheckKind _kind;

        public DiceCheckResult(int roll, int bonus, int total, int targetDC, bool success, bool isCritical, DiceCheckKind kind)
        {
            _roll = roll;
            _bonus = bonus;
            _total = total;
            _targetDC = targetDC;
            _success = success;
            _isCritical = isCritical;
            _kind = kind;
        }

        // Human-readable breakdown of the check's math - meant for surfacing to the player so
        // a pass/fail doesn't feel like a black box. The math reads differently per Kind, since
        // Target and Chance checks apply bonus differently (see ChanceDiceChecker): a Target
        // check reads e.g. "10 + 5 = 15 vs. 12" (or "20! + 5 = 25 vs. 12" on a critical), a
        // Chance check reads e.g. "10 vs. 25 + 5 = 30" (bonus raises the chance, not the roll).
        public override string ToString()
        {
            string rollText = _isCritical ? $"{_roll}!" : _roll.ToString();

            switch (_kind)
            {
                case DiceCheckKind.Chance:
                    int baseChance = _targetDC - _bonus;
                    return $"{rollText} vs. {baseChance} + {_bonus} = {_targetDC}";

                case DiceCheckKind.Target:
                default:
                    return $"{rollText} + {_bonus} = {_total} vs. {_targetDC}";
            }
        }
    }
}
