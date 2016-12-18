using _9ROGUE_FORMS.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _9ROGUE_FORMS.Subject
{
    class ItemContainer : Subject
    { 
        public List<Weapon> Weapons { get; set; }
        public List<Armor> Armors { get; set; }
        public ItemContainer()
        {
            Name = "Item Container";
            Description = "a small chest with something inside of it";
            Figure = "+";
            Weapons = new List<Weapon>();
            Armors = new List<Armor>();
        }
    }
}
