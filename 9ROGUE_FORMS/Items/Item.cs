using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _9ROGUE_FORMS.Items
{
    class Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Weight { get; set; }
        public List<int> Requirements { get; set; }
    }
}
