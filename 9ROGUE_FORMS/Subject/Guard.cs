using _9ROGUE_FORMS.Items;
using _9ROGUE_FORMS.Items.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _9ROGUE_FORMS.Subject
{
    class Guard : Creature
    {
        public Guard()
        {
            Name = "Skeleton Guard";

            Figure = "G";
            Random rnd = new Random();
            Exp = rnd.Next(700, 1301);
            Lvl = Convert.ToInt32(Math.Round(Convert.ToDecimal((Exp / 100)), MidpointRounding.AwayFromZero));
            HP_max = Lvl;
            HP = HP_max;
            Description = String.Format("a skeleton Guard. It will put its life to make you stay in the dungeon forever ({0} lvl)", Lvl);
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
                Active_Weapon = new SilverSpear();
            }
            else
            {
                Active_Weapon = new IronSword();
            }
        }
    }
}
