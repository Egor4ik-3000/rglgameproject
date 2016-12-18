using _9ROGUE_FORMS.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _9ROGUE_FORMS.Subject
{
    class Player : Creature
    {
        public event Action Moved;
        public event Action Attacked;
        public event Action Restored;

        public Player(string name) : base()
        {
            Random rnd = new Random();
            Name = name;
            Figure = "@";
            FigureColor = ConsoleColor.DarkCyan;
            Exists = true;
            IsViewable = true;
            HP_max = 3;
            HP = 3;
            IsTriggered = false;
            Habitat_x_max = 325;
            Habitat_x_min = 0;
            Habitat_y_max = 325;
            Habitat_y_min = 0;

            Inventory.Weapons.Add(new WoodenDagger());
            Inventory.Armors.Add(new LeatherArmor());
            Inventory.Poisons.Add(new Poison(rnd));
            Inventory.Poisons.Add(new Poison(rnd));
            Inventory.Poisons.Add(new Poison(rnd));

            // Godmode:
            if (Name == "Sergey_Efremov")
            {
                HP_max = 99999;
                HP = 99999;
            }
        }

        public override void Move(int Position_x_new, int Position_y_new)
        {
            base.Move(Position_x_new, Position_y_new);
            if (Moved != null)
            {
                Moved.Invoke();
            }
        }

        public override List<string> Attack(Creature creature, Random rnd)
        {
            List<string> result = base.Attack(creature, rnd);
            if (Attacked != null)
            {
                //Attacked.Invoke();
            }
            return result;
        }

        public bool LvlUpCheck ()
        {
            bool result = false;
            // Gain new Level each 100 exp:
            if (Exp >= Lvl * 100)
            {
                result = true;
            }
            return result;
        }


        public void LvlUp (string Attribute_Name)
        {
            Lvl++;
            HP_max++;
            HP = HP_max;
            if (Attribute_Name == "S")
            {
                Strength++;
            }
            if (Attribute_Name == "I")
            {
                Intelligence++;
            }
            if (Attribute_Name == "D")
            {
                Dexterity++;
            }
            if (Attribute_Name == "E")
            {
                Endurance++;
            }
        }

        public string Restore_HP ()
        {
            string result = ">You accidently spill the ale you were going to drink\n";

            if (HP < HP_max)
            {
                if (Inventory.Poisons.Count > 0)
                {
                    double restore = Inventory.Poisons[0].Restore + Intelligence * 0.3;
                    HP += restore;
                    result = String.Format(">You drink some good ale. It restores {0} of your health\n", Decimal.Round(Convert.ToDecimal(restore), 2));
                    Inventory.Poisons.RemoveAt(0);
                }
                else
                {
                    result = ">You have no ale to drink\n";
                }
            }
            else
            {
                result = ">You do not feel like drinking ale right now\n";
            }

            return result;
            //if (Restored != null)
            //{
            //    Restored.Invoke();
            //}

        }

        public List<string> Take(ItemContainer itmC)
        {
            List<string> result = new List<string>();
            result.Add(">You found:\n");
            foreach (Item item in itmC.Armors)
            {
                result.Add(item.Name);
                result.Add("\n");
                Inventory.Armors.RemoveAll(a => a.Name == item.Name);
            }
            foreach (Item item in itmC.Weapons)
            {
                result.Add(item.Name);
                result.Add("\n");
                Inventory.Weapons.RemoveAll(w => w.Name == item.Name);
            }

            Inventory.Weapons.AddRange(itmC.Weapons);
            Inventory.Armors.AddRange(itmC.Armors);
            return result;
        }
    }
}
