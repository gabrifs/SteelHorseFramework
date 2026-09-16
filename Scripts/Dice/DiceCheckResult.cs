namespace SteelHorse.Framework.Dice
{
    // Outcome of a DiceRoller.RollCheck call - carries the raw roll alongside the
    // resolved total/success so callers don't need to redo the DC comparison themselves.
    public readonly struct DiceCheckResult
    {
        public int Roll { get { return _roll; } }
        public int Bonus { get { return _bonus; } }
        public int Total { get { return _total; } }
        public int TargetDC { get { return _targetDC; } }
        public bool Success { get { return _success; } }

        private readonly int _roll;
        private readonly int _bonus;
        private readonly int _total;
        private readonly int _targetDC;
        private readonly bool _success;

        public DiceCheckResult(int roll, int bonus, int total, int targetDC, bool success)
        {
            _roll = roll;
            _bonus = bonus;
            _total = total;
            _targetDC = targetDC;
            _success = success;
        }
    }
}
