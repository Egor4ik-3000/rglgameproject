using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _9ROGUE_FORMS.Surroundings
{
    class Door : Surrounding
    {
        public bool IsUsed { get; set; }
        public Door()
        {
            Name = "door";
            Description = "a wooden door built by Gnomes ages ago";
            Figure = "/";
            FigureColor = ConsoleColor.DarkYellow;
            Clip = false;
        }
    }
}
