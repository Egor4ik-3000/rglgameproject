using _9ROGUE_FORMS.Items;
using _9ROGUE_FORMS.Items.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _9ROGUE_FORMS.Subject
{
    class SkeletonWarrior : Creature
    {
        public SkeletonWarrior()
        {
            Name = "Skeleton Warrior";

            Figure = "$";
            Random rnd = new Random();
            Exp = rnd.Next(300, 800);
            Lvl = Convert.ToInt32(Math.Round(Convert.ToDecimal((Exp / 100)), MidpointRounding.AwayFromZero));
            HP_max = Lvl;
            HP = rnd.Next(1, HP_max + 1);
            Description = String.Format("a well-equipped skeleton. It used to be a good warrior in past ({0} lvl)", Lvl);
            for (int i = 0; i < Lvl; i++)
            {
                if (rnd.Next(2) == 1)
                {
                    Strength++;
                }
                else
                {
                    Dexterity++;
                }
            }
            if (rnd.Next(2) == 1)
            {
                Active_Weapon = new WoodenDagger();
            }
            else
            {
                Active_Weapon = new IronSword();
            }
        }
    }
}
