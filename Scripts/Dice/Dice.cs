using System;
using UnityEngine;

namespace SteelHorse.Framework.Dice
{
    // Reusable definition of a die roll (e.g. a d20, or 3d6) - build one once for a specific
    // use (a weapon's damage die, a stat check's die, etc.) and reuse it instead of passing
    // raw faces/amount around every time it needs to be rolled.
    [Serializable]
    public class Dice
    {
        // Fired for every single die physically rolled, regardless of which Dice instance
        // rolled it or how it was rolled (a plain Roll(), or one inside a DiceChecker check) -
        // for SFX/animation reacting to a roll.
        public static event Action<int, int> DiceRolled;

        public int Faces { get { return _faces; } }
        public int Amount { get { return _amount; } }

        [SerializeField] private int _faces;
        [SerializeField] private int _amount = 1;

        public Dice()
        {
        }

        public Dice(int faces, int amount = 1)
        {
            _faces = faces;
            _amount = Mathf.Max(1, amount);
        }

        // Rolls this die (or dice, if Amount > 1) and returns the summed total.
        public int Roll()
        {
            int total = 0;

            for (int i = 0; i < _amount; i++)
            {
                int roll = UnityEngine.Random.Range(1, _faces + 1);
                DiceRolled?.Invoke(_faces, roll);
                total += roll;
            }

            return total;
        }

        // Gets this die for text display, following TTRPG convention.
        public string DiceDisplay()
        {
            return $"{Amount}d{Faces}";
        }
    }
}
