using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _9ROGUE_FORMS.Items
{
    class Armor : Item
    {
        public int Protection { get; set; }


        public Armor()
        {
            Requirements = new List<int>();
        }
    }
}
