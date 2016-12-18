using _9ROGUE_FORMS.Subject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _9ROGUE_FORMS.Items
{
    class Poison
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Restore { get; set; }

        public Poison(Random rnd)
        {
            Name = "Ale";
            Restore = rnd.Next(1, 4);
        }
    }
}
