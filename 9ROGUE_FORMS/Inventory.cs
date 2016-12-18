using _9ROGUE_FORMS.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _9ROGUE_FORMS
{
    class Inventory
    {
        public List<Weapon> Weapons { get; set; }
        public List<Armor> Armors { get; set; }
        public List<Poison> Poisons { get; set; }

        public List<string> Items { get; set; }

        public Inventory()
        {
            Items = new List<string>();
            Weapons = new List<Weapon>();
            Armors = new List<Armor>();
            Poisons = new List<Poison>();
        }
    }
}
