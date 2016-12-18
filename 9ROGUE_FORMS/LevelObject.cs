using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _9ROGUE_FORMS
{
    class LevelObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Position_x { get; set; }
        public int Position_y { get; set; }
        public string Figure { get; set; }
        public ConsoleColor FigureColor { get; set; }
        public bool Exists { get; set; }
        public bool Clip { get; set; }
        public bool IsViewable { get; set; }
    }
}
