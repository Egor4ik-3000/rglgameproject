using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nesteruk.MdxConsole;
using Console = Nesteruk.MdxConsole.Console;
using System.Threading;
using System.Windows.Forms;
using _9ROGUE_FORMS.Subject;
using System.Drawing;
using _9ROGUE_FORMS.Surroundings;
using _9ROGUE_FORMS.Items;

namespace _9ROGUE_FORMS
{
    class Processing
    {
        private Console c;
        private char lastpressedKey { get; set; }
        public Level Level { get; set; }
        public string[,] Display { get; set; }
        public char[,] Display_char { get; set; }
        public string[,] Display_Player_Stats { get; set; }
        public List<LevelObject> toDisplay_1 { get; set; }
        public List<LevelObject> toDisplay_2 { get; set; }
        public List<LevelObject> toDisplay_3 { get; set; }
        public List<LevelObject> toDisplay_4 { get; set; }
        public Keys pressedKey { get; set; }
        public int Turn_Count { get; set; }
        public Random rnd { get; set; }
        private bool inspect { get; set; }
        private bool move { get; set; }
        private bool attack { get; set; }
        private bool lvlup { get; set; }
        private bool open { get; set; }
        private bool inventory { get; set; }
        private List<string> nextTurnStrings { get; set; }
        private bool End { get; set; }

        private readonly ReaderWriterLockSlim myLock = new ReaderWriterLockSlim();

        private bool ConsoleCreated
        {
            get
            {
                myLock.EnterReadLock();
                bool result = c.Created;
                myLock.ExitReadLock();
                return result;
            }
        }

        public Processing(Level level)
        {
            inspect = false;
            move = false;
            attack = false;
            lvlup = false;
            open = false;
            inventory = false;

            End = false;

            nextTurnStrings = new List<string>();

            Turn_Count = 0;
            Level = level;
            Level.Player.Moved += Next_Turn;
            Level.Player.Attacked += Next_Turn;
            Level.Player.Restored += Next_Turn;

            Display = new string[15, 15];
            Display_char = new char[15, 15];

            toDisplay_1 = new List<LevelObject>();
            toDisplay_2 = new List<LevelObject>();
            toDisplay_3 = new List<LevelObject>();
            toDisplay_4 = new List<LevelObject>();

            Display_Player_Stats = new string[15, 5];

            rnd = new Random();

            using (c = Console.NewConsole(67, 35))
            {
                c.Text = "9ROGUE";
                c.Show();
                c.KeyDown += KeyDown;

                // Add main viewport that displays the level (1):
                Nesteruk.MdxConsole.Buffer main = new Nesteruk.MdxConsole.Buffer(15, 15);
                Viewport vp_main = new Viewport(main, new Size(main.Size.Width, main.Size.Height),
                                           new Point(0, 0), new Point(0, 0));

                // Add player general stats viewport (2):
                Nesteruk.MdxConsole.Buffer player_stats_buffer = new Nesteruk.MdxConsole.Buffer(25, 9);
                Viewport vp_player_stats = new Viewport(player_stats_buffer, new Size(player_stats_buffer.Size.Width, player_stats_buffer.Size.Height),
                                           new Point(0, 0), new Point(0, 20));

                // Add messages box viewport (3):
                Nesteruk.MdxConsole.Buffer messages_buffer = new Nesteruk.MdxConsole.Buffer(40, 34);
                Viewport vp_messages = new Viewport(messages_buffer, new Size(messages_buffer.Size.Width, messages_buffer.Size.Height),
                                           new Point(0, 0), new Point(26, 0));

                c.Viewports.Add(vp_main);
                c.Viewports.Add(vp_player_stats);
                c.Viewports.Add(vp_messages);
                //c.Viewports.Add(vp_dialogue);

                Font currentFont = c.TexManager.DefaultFont;
                Font italic = new Font(currentFont, FontStyle.Italic);
                Font bold = new Font(currentFont, FontStyle.Bold);
                c.TexManager.AddPreset(italic, Color.Transparent, Color.White);
                c.TexManager.AddPreset(bold, Color.Transparent, Color.White);
                c.TexManager.AddPreset(currentFont, Color.Transparent, Color.Green);
                c.TexManager.AddPreset(currentFont, Color.White, Color.Red);

                c.Viewports[3].Buffer.Write("You find yourself in a vast, dark dungeon \n");
                c.Viewports[3].Buffer.Write("To get out you will have to defeat each of 9 Guards securing the exit \n");
                c.Viewports[3].Buffer.Write("You make the first step into the darkness... \n");

                for (int i = 0; i < 15; i++)
                {
                    for (int j = 0; j < 15; j++)
                    {
                        Display[i, j] = "";
                    }
                }

                foreach (Creature creature in Level.Creatures)
                {
                    NearObjectsUpdate(creature);
                }

                for (int y = 0; y < 15; y++)
                {
                    for (int x = 0; x < 15; x++)
                    {
                        c.Viewports[1].Buffer.Write(Display[y, x]);
                    }
                    c.Viewports[1].Buffer.Write("\n");
                }

                while (ConsoleCreated)
                {
                    c.Render();
                    Application.DoEvents();
                }
            }
        }

        private void Next_Turn()
        {
            Turn_Count++;
            foreach (string str in nextTurnStrings)
            {
                c.Viewports[3].Buffer.Write(str);
            }

            // HP check:
            if (Level.Player.HP > Level.Player.HP_max)
            {
                Level.Player.HP = Level.Player.HP_max;
            }

            // Death Check:
            foreach (Creature creature in Level.Creatures)
            {
                if (creature.HP <= 0)
                {
                    creature.IsAlive = false;
                    Level.Objects.Remove(creature);
                }
            }
            if (Level.Player.IsAlive == false)
            {
                c.Viewports[3].Buffer.Write("\nYour journey has come to an end\nYou did not manage to get to the surface But one day there will be the one, who will accomplish what you could not\n[Press any key to leave]");

            }
            Level.Creatures.RemoveAll(cr => cr.IsAlive == false);

            if (!Level.Creatures.Any(c => c.GetType() == (new Guard()).GetType()))
            {
                c.Viewports[3].Buffer.Write("\n\n All Guards are dead. Now you can leave the dungeon!");
                End = true;
            }

            if (Level.Player.IsAlive == true)
            { 
            // Trigger check:
            foreach (Creature creature in Level.Creatures)
            {
                if (creature.GetType() != Level.Player.GetType())
                {
                    NearObjectsUpdate(creature);
                    if (creature.IsTriggered == false)
                    {
                        if (Math.Abs(Level.Player.Position_x - creature.Position_x) < 4 && Math.Abs(Level.Player.Position_y - creature.Position_y) < 4)
                        {
                            creature.IsTriggered = true;
                        }
                    }
                    else
                    {
                        if (Level.Player.Position_x < creature.Follow_x_min || Level.Player.Position_x > creature.Follow_x_max || Level.Player.Position_y < creature.Follow_y_min || Level.Player.Position_y > creature.Follow_y_max)
                        {
                            creature.IsTriggered = false;
                        }
                    }
                }
            }

                // Monsters actions (move/attack):
                foreach (Creature creature in Level.Creatures)
                {
                    if (creature.GetType() != Level.Player.GetType())
                    {
                        NearObjectsUpdate(creature);
                        if (creature.IsTriggered == true)
                        {
                            if (creature.NearObjects.Any(obj => obj.GetType() == Level.Player.GetType()))
                            {
                                List<string> attack_result = creature.Attack(Level.Creatures.Find(c => c.GetType() == Level.Player.GetType()), rnd);
                                foreach (string str in attack_result)
                                {
                                    c.Viewports[3].Buffer.Write(str);
                                }
                            }
                            else
                            {
                                creature.MoveTo(Level.Player.Position_x, Level.Player.Position_y, true);
                            }
                        }
                        else
                        {
                            // Возвращение в habitat:
                            if (creature.Position_x < creature.Habitat_x_min || creature.Position_x > creature.Habitat_x_max || creature.Position_y < creature.Habitat_y_min || creature.Position_y > creature.Habitat_y_max)
                            {
                                creature.MoveTo(creature.Spawn_Position_x, creature.Spawn_Position_y, true);
                            }
                            else
                            {
                                int Position_x_new = creature.Position_x + rnd.Next(-1, 2);
                                int Position_y_new = creature.Position_y + rnd.Next(-1, 2);
                                if (creature.MovementCheck(Position_x_new, Position_y_new))
                                {
                                    creature.Move(Position_x_new, Position_y_new);
                                }
                            }
                        }
                    }
                }
            }

            // Check if Level is Up:
            if (Level.Player.LvlUpCheck())
            {
                c.Viewports[3].Buffer.Write("\nCongratulations! You have gained a new Level!\nPress the right key to increase:\n");
                c.Viewports[3].Buffer.Write("[S]trength\n[I]ntelligence\n[D]exterity\n[E]ndurance\n\n");
                lvlup = true;
            }

            // Update Inventory.Items:
            Level.Player.Inventory.Items.Clear();
            foreach (Weapon weapon in Level.Player.Inventory.Weapons)
            {
                Level.Player.Inventory.Items.Add(weapon.Name);
            }
            foreach (Armor armor in Level.Player.Inventory.Armors)
            {
                Level.Player.Inventory.Items.Add(armor.Name);
            }
            // Update NearObjects (Player Only):
            NearObjectsUpdate(Level.Player);
            // Update Level Display:
            DisplayUpdate();
            // Update Player Stats Viewport:
            Display_Player_Stats_Update();

            nextTurnStrings = new List<string>();
        }

        // Player Stats Buffer update:
        public void Display_Player_Stats_Update ()
        {
            for (int i = 0; i < 7; i++)
            {
                c.Viewports[2].Buffer.Write("\n");
            }
            c.Viewports[2].Buffer.Write(Level.Player.Name);
            c.Viewports[2].Buffer.Write(":");
            c.Viewports[2].Buffer.Write("\n");
            c.Viewports[2].Buffer.Write(String.Format("HP: {0} \n", Decimal.Round(Convert.ToDecimal(Level.Player.HP), 2)));
            c.Viewports[2].Buffer.Write(String.Format("Lvl: {0}  Exp: {1} \n", Level.Player.Lvl, Level.Player.Exp));
            c.Viewports[2].Buffer.Write(String.Format("Dungeon: {0}  Turn: {1} \n", Level.ID + 1, Turn_Count));
            c.Viewports[2].Buffer.Write(String.Format("[F1] - Controls"));
        }

        // Near Objects Update Method:
        private void NearObjectsUpdate(Creature creature)

        {

            creature.NearObjects.Clear();

            creature.NearObjects.AddRange(Level.Objects.FindAll(obj =>
            (obj.Position_x == creature.Position_x - 1 && obj.Position_y == creature.Position_y - 1)
            ||
            (obj.Position_x == creature.Position_x - 1 && obj.Position_y == creature.Position_y)
            ||
            (obj.Position_x == creature.Position_x - 1 && obj.Position_y == creature.Position_y + 1)
            ||
            (obj.Position_x == creature.Position_x && obj.Position_y == creature.Position_y + 1)
            ||
            (obj.Position_x == creature.Position_x + 1 && obj.Position_y == creature.Position_y + 1)
            ||
            (obj.Position_x == creature.Position_x + 1 && obj.Position_y == creature.Position_y)
            ||
            (obj.Position_x == creature.Position_x + 1 && obj.Position_y == creature.Position_y - 1)
            ||
            (obj.Position_x == creature.Position_x && obj.Position_y == creature.Position_y - 1)));
        }

        // Player View Field Update Method for Up-Left corner:
        // (Position by position)
        private void CheckPositionView_1 (LevelObject objToCheck)
        {
            if (objToCheck.IsViewable == true)
            {
                if (objToCheck.GetType() != (new Wall()).GetType() && objToCheck.GetType() != (new Door()).GetType())
                {
                    if (toDisplay_1.Any(x => x.Position_x == objToCheck.Position_x -1 && x.Position_y == objToCheck.Position_y))
                    {
                        toDisplay_1[toDisplay_1.FindIndex(x => x.Position_x == objToCheck.Position_x - 1 && x.Position_y == objToCheck.Position_y)].IsViewable = true;
                    }
                    if (toDisplay_1.Any(x => x.Position_x == objToCheck.Position_x && x.Position_y == objToCheck.Position_y - 1))
                    {
                        toDisplay_1[toDisplay_1.FindIndex(x => x.Position_x == objToCheck.Position_x && x.Position_y == objToCheck.Position_y - 1)].IsViewable = true;
                    }
                }
            }
        }

        // Player View Field Update Method for Up-Right corner:
        // (Position by position)
        private void CheckPositionView_2(LevelObject objToCheck)
        {
            if (objToCheck.IsViewable == true)
            {
                if (objToCheck.GetType() != (new Wall()).GetType() && objToCheck.GetType() != (new Door()).GetType())
                {
                    if (toDisplay_2.Any(x => x.Position_x == objToCheck.Position_x + 1 && x.Position_y == objToCheck.Position_y))
                    {
                        toDisplay_2[toDisplay_2.FindIndex(x => x.Position_x == objToCheck.Position_x + 1 && x.Position_y == objToCheck.Position_y)].IsViewable = true;
                    }
                    if (toDisplay_2.Any(x => x.Position_x == objToCheck.Position_x && x.Position_y == objToCheck.Position_y - 1))
                    {
                        toDisplay_2[toDisplay_2.FindIndex(x => x.Position_x == objToCheck.Position_x && x.Position_y == objToCheck.Position_y - 1)].IsViewable = true;
                    }
                }
            }
        }

        // Down-Right:
        private void CheckPositionView_3(LevelObject objToCheck)
        {
            if (objToCheck.IsViewable == true)
            {
                if (objToCheck.GetType() != (new Wall()).GetType() && objToCheck.GetType() != (new Door()).GetType())
                {
                    if (toDisplay_3.Any(x => x.Position_x == objToCheck.Position_x + 1 && x.Position_y == objToCheck.Position_y))
                    {
                        toDisplay_3[toDisplay_3.FindIndex(x => x.Position_x == objToCheck.Position_x + 1 && x.Position_y == objToCheck.Position_y)].IsViewable = true;
                    }
                    if (toDisplay_3.Any(x => x.Position_x == objToCheck.Position_x && x.Position_y == objToCheck.Position_y + 1))
                    {
                        toDisplay_3[toDisplay_3.FindIndex(x => x.Position_x == objToCheck.Position_x && x.Position_y == objToCheck.Position_y + 1)].IsViewable = true;
                    }
                }
            }
        }

        // Down-Left:
        private void CheckPositionView_4(LevelObject objToCheck)
        {
            if (objToCheck.IsViewable == true)
            {
                if (objToCheck.GetType() != (new Wall()).GetType() && objToCheck.GetType() != (new Door()).GetType())
                {
                    if (toDisplay_4.Any(x => x.Position_x == objToCheck.Position_x - 1 && x.Position_y == objToCheck.Position_y))
                    {
                        toDisplay_4[toDisplay_4.FindIndex(x => x.Position_x == objToCheck.Position_x - 1 && x.Position_y == objToCheck.Position_y)].IsViewable = true;
                    }
                    if (toDisplay_4.Any(x => x.Position_x == objToCheck.Position_x && x.Position_y == objToCheck.Position_y + 1))
                    {
                        toDisplay_4[toDisplay_4.FindIndex(x => x.Position_x == objToCheck.Position_x && x.Position_y == objToCheck.Position_y + 1)].IsViewable = true;
                    }
                }
            }
        }

        // Player Field of View Update:
        // (whole)
        private void DisplayUpdate()
        {

            foreach (LevelObject obj in Level.Objects)
            {
                if (obj != Level.Player)
                {
                    obj.IsViewable = false;
                }
            }

            // Up-Left:
            toDisplay_1.Clear();

            // Populating:
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    toDisplay_1.Add(Level.Objects.FindLast(obj => obj.Position_x == Level.Player.Position_x - x && obj.Position_y == Level.Player.Position_y - y));
                }
            }

            foreach (LevelObject obj in toDisplay_1)
            {
                CheckPositionView_1(obj);
            }

            // Up-Right:
            toDisplay_2.Clear();

            // Populating:
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    toDisplay_2.Add(Level.Objects.FindLast(obj => obj.Position_x == Level.Player.Position_x + x && obj.Position_y == Level.Player.Position_y - y));
                }
            }

            foreach (LevelObject obj in toDisplay_2)
            {
                CheckPositionView_2(obj);
            }

            // Down-Right:
            toDisplay_3.Clear();

            // Populating:
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    toDisplay_3.Add(Level.Objects.FindLast(obj => obj.Position_x == Level.Player.Position_x + x && obj.Position_y == Level.Player.Position_y + y));
                }
            }

            foreach (LevelObject obj in toDisplay_3)
            {
                CheckPositionView_3(obj);
            }

            // Down-Left:
            toDisplay_4.Clear();

            // Populating:
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    toDisplay_4.Add(Level.Objects.FindLast(obj => obj.Position_x == Level.Player.Position_x - x && obj.Position_y == Level.Player.Position_y + y));
                }
            }

            foreach (LevelObject obj in toDisplay_4)
            {
                CheckPositionView_4(obj);
            }

            // Once the program is done with each of 4 corners, it updates Display array:
            for (int y = Level.Player.Position_y - 7; y < Level.Player.Position_y + 8; y++)
            {
                for (int x = Level.Player.Position_x - 7; x < Level.Player.Position_x + 8; x++)
                {
                    if (Level.Objects.FindLast(obj => obj.Position_x == x && obj.Position_y == y).IsViewable == true)
                    {
                        Display[(y - Level.Player.Position_y + 7), (x - Level.Player.Position_x + 7)] = (Level.Objects.FindLast(obj => obj.Position_x == x && obj.Position_y == y)).Figure;
                    }
                    else
                    {
                        Display[(y - Level.Player.Position_y + 7), (x - Level.Player.Position_x + 7)] = " ";
                    }
                }
            }

            for (int y = 0; y < 15; y++)
            {
                for (int x = 0; x < 15; x++)
                {
                    Display_char[y, x] = Convert.ToChar(Display[y, x]);
                }
            }

            // Update screen
            c.Viewports[1].Buffer.ReFill(Display_char);
        }

        // Inspect:
        private string Inspect(Creature creature, int Position_x_check, int Position_y_check)
        {

            string result = ">You see: nothing";

            if (Position_x_check == creature.Position_x && Position_y_check == creature.Position_y)
            {
                result = ">You did not understand where to look, so you just squint your eyes";

            }
            else
            {
                string obj = creature.NearObjects.FindLast(x => x.Position_x == Position_x_check && x.Position_y == Position_y_check).Description;
                result = String.Format(">You see: {0}", obj);
            }
            return result;
        }

        // Processing pressed keys (Player actions):
        private void KeyDown(object sender, KeyEventArgs e)
        {
            if (Level.Player.IsAlive == true && End == false)
            {


                int Position_x_new = Level.Player.Position_x;
                int Position_y_new = Level.Player.Position_y;
                inspect = false;
                move = false;
                attack = false;
                open = false;

                if (inventory == true)
                {
                    int key = 999;
                    if (e.KeyCode == Keys.D0)
                    {
                        key = 0;
                    }
                    if (e.KeyCode == Keys.D1)
                    {
                        key = 1;
                    }
                    if (e.KeyCode == Keys.D2)
                    {
                        key = 2;
                    }
                    if (e.KeyCode == Keys.D3)
                    {
                        key = 3;
                    }
                    if (e.KeyCode == Keys.D4)
                    {
                        key = 4;
                    }
                    if (e.KeyCode == Keys.D5)
                    {
                        key = 5;
                    }
                    if (e.KeyCode == Keys.D6)
                    {
                        key = 6;
                    }
                    if (e.KeyCode == Keys.D7)
                    {
                        key = 5;
                    }
                    if (e.KeyCode == Keys.D8)
                    {
                        key = 8;
                    }
                    if (e.KeyCode == Keys.D9)
                    {
                        key = 9;
                    }

                    if (Level.Player.Inventory.Items.Count >= key)
                    {
                        if (Level.Player.Inventory.Weapons.Any(w => w.Name == Level.Player.Inventory.Items[key]))
                        {
                            if ((Level.Player.Inventory.Weapons.Find(w => w.Name == Level.Player.Inventory.Items[key])).Weight > Level.Player.Endurance)
                            {
                                c.Viewports[3].Buffer.Write(">It is too heavy for you\n");
                            }
                            else
                            {
                                Level.Player.Active_Weapon = Level.Player.Inventory.Weapons.Find(w => w.Name == Level.Player.Inventory.Items[key]);
                                c.Viewports[3].Buffer.Write(String.Format(">You have equipped {0}\n", Level.Player.Active_Weapon.Name));
                            }
                        }
                        if (Level.Player.Inventory.Armors.Any(w => w.Name == Level.Player.Inventory.Items[key]))
                        {
                            if ((Level.Player.Inventory.Armors.Find(w => w.Name == Level.Player.Inventory.Items[key])).Weight > Level.Player.Endurance)
                            {
                                c.Viewports[3].Buffer.Write(">It is too heavy for you\n");
                            }
                            else
                            {
                                Level.Player.Active_Armor = Level.Player.Inventory.Armors.Find(w => w.Name == Level.Player.Inventory.Items[key]);
                                c.Viewports[3].Buffer.Write(String.Format(">You have equipped {0}\n", Level.Player.Active_Armor.Name));
                            }
                        }
                        inventory = false;
                    }
                }

                // Controls Help:
                if (e.KeyCode == Keys.F1)
                {
                    inspect = false;
                    move = false;
                    attack = false;
                    c.Viewports[3].Buffer.Write("\n");
                    c.Viewports[3].Buffer.Write("[Controls]\n");
                    c.Viewports[3].Buffer.Write("[O] - Open Inventory\n");
                    c.Viewports[3].Buffer.Write("[P] - Character Info\n");
                    c.Viewports[3].Buffer.Write("[K], then point the direction - Attack\n");
                    c.Viewports[3].Buffer.Write("[L], then point the direction - Inspect\n");
                    c.Viewports[3].Buffer.Write("[N], then point the direction - Open (a chest)\n");
                    c.Viewports[3].Buffer.Write("[M] - Drink Health Poison\n");
                    c.Viewports[3].Buffer.Write("\n");
                }

                // Charachter Info:
                if (e.KeyCode == Keys.P)
                {
                    inspect = false;
                    move = false;
                    attack = false;
                    open = false;
                    List<string> info = new List<string>();
                    info.Add(String.Format("\n{0}:\n", Level.Player.Name));
                    info.Add(String.Format("Level: {0}\n", Level.Player.Lvl));
                    info.Add(String.Format("Exp: {0}\n", Level.Player.Exp));
                    info.Add(String.Format("Strength: {0}\n", Level.Player.Strength));
                    info.Add(String.Format("Intelligence: {0}\n", Level.Player.Intelligence));
                    info.Add(String.Format("Dexterity: {0}\n", Level.Player.Dexterity));
                    info.Add(String.Format("Endurace: {0}\n\n", Level.Player.Endurance));

                    foreach (string str in info)
                    {
                        c.Viewports[3].Buffer.Write(str);
                    }

                }

                // Show Inventory:
                if (e.KeyCode == Keys.O)
                {
                    inspect = false;
                    move = false;
                    attack = false;
                    open = false;
                    inventory = true;
                    List<string> inv = new List<string>();
                    int k = 0;
                    inv.Add("\n>You have:\n");
                    inv.Add(">Weapons:\n");
                    foreach (Weapon weapon in Level.Player.Inventory.Weapons)
                    {
                        if (!inv.Contains(weapon.Name))
                        {
                            string isEquipped = "";
                            if (Level.Player.Active_Weapon == weapon)
                            {
                                isEquipped = "(E)";
                            }
                            inv.Add(String.Format(">{0} [{1}] {2}\n", weapon.Name, k, isEquipped));
                            k++;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    inv.Add("\n>Armors:\n");
                    foreach (Armor armor in Level.Player.Inventory.Armors)
                    {
                        if (!inv.Contains(armor.Name))
                        {
                            string isEquipped = "";
                            if (Level.Player.Active_Armor == armor)
                            {
                                isEquipped = "(E)";
                            }
                            inv.Add(String.Format(">{0} [{1}] {2}\n", armor.Name, k, isEquipped));
                            k++;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    inv.Add(String.Format("\n>Ale: {0}\n\n", Level.Player.Inventory.Poisons.Count));

                    foreach (string str in inv)
                    {
                        c.Viewports[3].Buffer.Write(str);
                    }

                }

                // Wait:
                if ((e.KeyCode == Keys.S || e.KeyCode == Keys.NumPad5) && lvlup == false)
                {
                    inspect = false;
                    move = false;
                    attack = false;
                    open = false;
                    Next_Turn();
                }

                    // Movement:
                    if (e.KeyCode == Keys.Q || e.KeyCode == Keys.NumPad7)
                {
                    Position_x_new = Level.Player.Position_x - 1;
                    Position_y_new = Level.Player.Position_y - 1;
                    move = true;
                }
                if (e.KeyCode == Keys.W || e.KeyCode == Keys.NumPad8)
                {
                    Position_x_new = Level.Player.Position_x;
                    Position_y_new = Level.Player.Position_y - 1;
                    move = true;
                }
                if (e.KeyCode == Keys.E || e.KeyCode == Keys.NumPad9)
                {
                    Position_x_new = Level.Player.Position_x + 1;
                    Position_y_new = Level.Player.Position_y - 1;
                    move = true;

                }
                if (e.KeyCode == Keys.D || e.KeyCode == Keys.NumPad6)
                {
                    Position_x_new = Level.Player.Position_x + 1;
                    Position_y_new = Level.Player.Position_y;
                    move = true;
                }
                if (e.KeyCode == Keys.C || e.KeyCode == Keys.NumPad3)
                {
                    Position_x_new = Level.Player.Position_x + 1;
                    Position_y_new = Level.Player.Position_y + 1;
                    move = true;
                }
                if (e.KeyCode == Keys.X || e.KeyCode == Keys.NumPad2)
                {
                    Position_x_new = Level.Player.Position_x;
                    Position_y_new = Level.Player.Position_y + 1;
                    move = true;
                }
                if (e.KeyCode == Keys.Z || e.KeyCode == Keys.NumPad1)
                {
                    Position_x_new = Level.Player.Position_x - 1;
                    Position_y_new = Level.Player.Position_y + 1;
                    move = true;
                }
                if (e.KeyCode == Keys.A || e.KeyCode == Keys.NumPad4)
                {
                    Position_x_new = Level.Player.Position_x - 1;
                    Position_y_new = Level.Player.Position_y;
                    move = true;
                }

                
                // Inspect:
                // ( + inspect arg ex):
                if (pressedKey == Keys.L)
                {
                    if (e.KeyCode == Keys.L)
                    {
                        inspect = false;
                        move = false;
                    }
                    else
                    {
                        inspect = true;
                        move = false;
                    }
                }

                // Open:
                if (pressedKey == Keys.N)
                {
                    if (e.KeyCode == Keys.N)
                    {
                        open = false;
                        move = false;
                    }
                    else
                    {
                        open = true;
                        move = false;
                    }
                }

                // Attack:
                if (pressedKey == Keys.K)
                {
                    if (e.KeyCode == Keys.K)
                    {
                        attack = false;
                        move = false;
                    }
                    else
                    {
                        attack = true;
                        move = false;
                    }
                }

                // Action:

                if (e.KeyCode == Keys.M && lvlup == false && inspect == false && attack == false)
                {
                    nextTurnStrings.Add(Level.Player.Restore_HP());
                    Next_Turn();
                }

                if (open == true && lvlup == false)
                {
                    if (Level.Player.NearObjects.Any(obj => obj.Position_x == Position_x_new && obj.Position_y == Position_y_new && obj.GetType() == (new ItemContainer()).GetType()))
                    {
                        nextTurnStrings.AddRange(Level.Player.Take(Level.ItemContainers.Find(c => c.Position_x == Position_x_new && c.Position_y == Position_y_new)));
                        Level.ItemContainers.RemoveAll(c => c.Position_x == Position_x_new && c.Position_y == Position_y_new);
                        Level.Objects.Remove(Level.Objects.FindLast(c => c.Position_x == Position_x_new && c.Position_y == Position_y_new));
                        Next_Turn();
                    }
                    else
                    {
                        nextTurnStrings.Add("\n>You open an imaginary box and take imaginary nothing from it\n");
                        Next_Turn();
                    }
                }

                if (inspect == true && lvlup == false)
                {
                    c.Viewports[3].Buffer.Write(Inspect(Level.Player, Position_x_new, Position_y_new));
                    c.Viewports[3].Buffer.Write("\n\n");
                }

                if (attack == true && lvlup == false)
                {
                    if (Position_x_new == Level.Player.Position_x && Position_y_new == Level.Player.Position_y)
                    {
                        c.Viewports[3].Buffer.Write(">You attempt to injure yourself and make a small cut on a hand.\nIt is as far as you can go with self-harming\n\n");
                    }
                    else
                    {
                        if (!Level.Creatures.Any(cr => cr.Position_x == Position_x_new && cr.Position_y == Position_y_new))
                        {
                            c.Viewports[3].Buffer.Write(">You start slashing air in front of you, but nothing happens\n\n");
                        }
                        else
                        {
                            nextTurnStrings = Level.Player.Attack(Level.Creatures.Find(cr => cr.Position_x == Position_x_new && cr.Position_y == Position_y_new), rnd);
                            Next_Turn();
                        }
                    }
                }

                if (inspect == false && move == true && lvlup == false)
                {
                    if (Level.Player.MovementCheck(Position_x_new, Position_y_new))
                    {
                        Level.Player.Move(Position_x_new, Position_y_new);
                    }
                }

                if (lvlup == true)
                {
                    inspect = false;
                    move = false;
                    attack = false;
                    if (e.KeyCode == Keys.S)
                    {
                        Level.Player.LvlUp("S");
                        c.Viewports[3].Buffer.Write("Strength+\n");
                        lvlup = false;
                    }
                    if (e.KeyCode == Keys.I)
                    {
                        Level.Player.LvlUp("I");
                        c.Viewports[3].Buffer.Write("Intelligence+\n");
                        lvlup = false;
                    }
                    if (e.KeyCode == Keys.D)
                    {
                        Level.Player.LvlUp("D");
                        c.Viewports[3].Buffer.Write("Dexterity+\n");
                        lvlup = false;
                    }
                    if (e.KeyCode == Keys.E)
                    {
                        Level.Player.LvlUp("E");
                        c.Viewports[3].Buffer.Write("Endurance+\n");
                        lvlup = false;
                    }
                }

                pressedKey = e.KeyCode;
            }

            else
            {
                c.Close();
            }
        }
    }
}
