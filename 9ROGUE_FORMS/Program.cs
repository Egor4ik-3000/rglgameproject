// RELEASE



using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using Nesteruk.MdxConsole;
using Console = Nesteruk.MdxConsole.Console;
using System.Threading;
using _9ROGUE_FORMS.Subject;

namespace _9ROGUE_FORMS
{
    static class Program
    {
        public static Player Player;
        private static Console c;
        private static readonly ReaderWriterLockSlim myLock = new ReaderWriterLockSlim();
        private static bool ConsoleCreated
        {
            get
            {
                myLock.EnterReadLock();
                bool result = c.Created;
                myLock.ExitReadLock();
                return result;
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (c = Console.NewConsole(45, 30))
            {
                c.Text = "9ROGUE";
                c.Show();


                Font currentFont = c.TexManager.DefaultFont;
                Font italic = new Font(currentFont, FontStyle.Italic);
                Font bold = new Font(currentFont, FontStyle.Bold);
                c.TexManager.AddPreset(italic, Color.Transparent, Color.White);
                c.TexManager.AddPreset(bold, Color.Transparent, Color.White);
                c.TexManager.AddPreset(currentFont, Color.Transparent, Color.Green);
                c.TexManager.AddPreset(currentFont, Color.White, Color.Red);

                string playerName = "";
                List<string> toName = new List<string>();

                c.Viewports[0].Buffer.Editing = true;

                c.Write("Enter your name:\n");

                c.KeyPress += (sender, e) =>
                {
                    if (!char.IsControl(e.KeyChar) && c.Viewports[0].Buffer.Editing == true)
                    {
                        if (toName.Count < 15)
                        { 
                        c.Viewports[0].Buffer.Write(e.KeyChar.ToString());
                        toName.Add(e.KeyChar.ToString().Replace(' ', '_'));
                        }

                    }
                    if ((e.KeyChar == 'Y' || e.KeyChar == 'y') && c.Viewports[0].Buffer.Editing == false)
                    {
                        Player = new Player(playerName);
                        c.Close();
                        Run();

                    }
                };

                c.KeyDown += (sender, e) =>
                {
                    if (e.KeyCode == Keys.Enter && c.Viewports[0].Buffer.Editing == true && toName.Any(x => x != ""))
                    {
                        c.Viewports[0].Buffer.Write("\n");

                        char[] name = new char[toName.Count];
                        for (int i = 0; i < toName.Count; i++)
                        {
                            name[i] = Convert.ToChar(toName[i]);
                        }

                        playerName = new string (name);

                        c.Write("\n");
                        c.Write("Hello, ");
                        c.Write(playerName);
                        c.Write("!\n");
                        c.Write("Welcome to ");
                        c.WriteFormat("9ROGUE\n\n", 2);
                        c.WriteLine("Movement controls:\n");
                        c.WriteLine("Q W E              7 8 9");
                        c.WriteLine("A   D  or  numpad: 4   6");
                        c.WriteLine("Z X C              1 2 3\n");
                        c.WriteLine("[F1] (in-game) for additional controls");
                        c.WriteLine("\n");
                        c.WriteLine("Ready to start your journey? (Y)");
                        c.WriteLine("\n");
                        c.WriteLine("(Loading may take a while!)");

                        c.Viewports[0].Buffer.Editing = false;
                    }


                    if (e.KeyCode == Keys.Back && c.Viewports[0].Buffer.Editing == true)
                    {
                        if (toName.Count > 0)
                        {
                            c.Viewports[0].Buffer.Backspace();
                            toName.RemoveAt(toName.Count - 1);
                        }
                    }
                };

                while (ConsoleCreated)
                {
                    c.Render();
                    Application.DoEvents();
                }
            }
        }
        public static void Run()
        {
            Level level = new Level(0);
            level.Generator();
            level.Clear();
            level.Place();
            level.Spawn(Player);

            Processing process = new Processing(level);
        }
    }
}
