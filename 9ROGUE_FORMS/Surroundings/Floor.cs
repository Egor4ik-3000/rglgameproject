using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _9ROGUE_FORMS.Surroundings
{
    class Floor : Surrounding
    {
        public Floor() : base()
        {
            Name = "floor";
            Description = "nothing special";
            Figure = ".";
            FigureColor = ConsoleColor.DarkGray;
            Clip = false;
        }
    }
}
