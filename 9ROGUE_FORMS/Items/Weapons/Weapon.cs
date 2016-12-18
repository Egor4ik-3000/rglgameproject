using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _9ROGUE_FORMS.Items
{
    class Weapon : Item
    {
        public int Damage { get; set; }


        public Weapon()
        {
            Requirements = new List<int>();
        }
    }
}
