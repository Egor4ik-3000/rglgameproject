using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _9ROGUE_FORMS.Subject
{
    class Skeleton : Creature
    {
        public Skeleton(int Level_ID)
        {
            Name = "Skeleton";

            Figure = "S";
            Random rnd = new Random();
            Exp = rnd.Next(100, 400);
            Lvl = Convert.ToInt32(Math.Round(Convert.ToDecimal((Exp / 100)), MidpointRounding.AwayFromZero));
            HP_max = Lvl;
            HP = rnd.Next(1, HP_max+1);
            Description = String.Format("a moving banch of bones. It tries to say something, but all you hear is 'Seshhhh' ({0} lvl)", Lvl);
            // Skeletons only Up their strength
            Strength = Lvl;
            // Skeletons move free in 5x5 (see Level.Spawn)
        }
    }
}
