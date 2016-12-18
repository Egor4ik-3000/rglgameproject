using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _9ROGUE_FORMS.Surroundings
{
    class Wall : Surrounding
    {
        public Wall() : base()
        {
            Name = "wall";
            Description = "a natural stone wall";
            Figure = "#";
            FigureColor = ConsoleColor.DarkGray;
            Clip = true;
        }
    }
}
