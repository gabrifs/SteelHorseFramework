namespace SteelHorse.Framework.Dice
{
    // Outcome of a DiceRoller check call - carries the raw roll alongside the
    // resolved total/success so callers don't need to redo the DC comparison themselves.
    public readonly struct DiceCheckResult
    {
        public int Roll { get { return _roll; } }
        public int Bonus { get { return _bonus; } }
        public int Total { get { return _total; } }
        public int TargetDC { get { return _targetDC; } }
        public bool Success { get { return _success; } }
        public bool IsCritical { get { return _isCritical; } }

        private readonly int _roll;
        private readonly int _bonus;
        private readonly int _total;
        private readonly int _targetDC;
        private readonly bool _success;
        private readonly bool _isCritical;

        public DiceCheckResult(int roll, int bonus, int total, int targetDC, bool success, bool isCritical)
        {
            _roll = roll;
            _bonus = bonus;
            _total = total;
            _targetDC = targetDC;
            _success = success;
            _isCritical = isCritical;
        }

        // Human-readable breakdown of the check's math, e.g. "10 + 5 = 15 vs. 12" (or
        // "20! + 5 = 25 vs. 12" on a critical) - meant for surfacing to the player so a
        // pass/fail doesn't feel like a black box.
        public override string ToString()
        {
            string rollText = _isCritical ? $"{_roll}!" : _roll.ToString();
            return $"{rollText} + {_bonus} = {_total} vs. {_targetDC}";
        }
    }
}
