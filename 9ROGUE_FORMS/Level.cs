using _9ROGUE_FORMS.Items;
using _9ROGUE_FORMS.Items.Weapons;
using _9ROGUE_FORMS.Subject;
using _9ROGUE_FORMS.Surroundings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _9ROGUE_FORMS
{
    class Level
    {
        // Each Level is 125+id*25x125+id*25

        public int ID { get; set; }
        public List<LevelObject> Objects { get; set; }
        public Player Player { get; set; }
        public List<Creature> Creatures { get; set; }
        public List<ItemContainer> ItemContainers { get; set; }
        public List<Room> Rooms { get; set; }
        public int LevelLength { get; set; }
        public Random rnd { get; set; }

        public Level(int id)
        {
            ID = id;
            Objects = new List<LevelObject>();
            Rooms = new List<Room>();
            Creatures = new List<Creature>();
            ItemContainers = new List<ItemContainer>();
            LevelLength = (150 + (25 * id));
            rnd = new Random();
        }

        public void Generator()
        {
            //Console.WriteLine("Loading, Please Wait");
            Objects.Clear();
            //int GeneratorPos_x = rnd.Next(0, LevelLength);
            //int GeneratorPos_y = rnd.Next(0, LevelLength);
            int GeneratorPos_x = LevelLength / 2;
            int GeneratorPos_y = LevelLength / 2;
            // Variable Floors determines how many floor elements there are:
            int Floors = 0;
            // We cover the whole level with walls (stones or whatever is under the ground):
            for (int l = 0; l < LevelLength; l++)
            {
                for (int w = 0; w < LevelLength; w++)
                {
                    Objects.Add(new Wall() { Position_x = l, Position_y = w });
                }
            }
            // Than we add single floor elements until 80% of the level is floor
            while (Floors < Objects.Count * 0.8)
            {
                int newPos_x = 0;
                int newPos_y = 0;
                // Determine the next position:
                int k = rnd.Next(0, 4);
                // 
                Up:
                if (k == 0)
                {
                    newPos_x = GeneratorPos_x;
                    newPos_y = GeneratorPos_y - 1;
                }
                // Move Down:
                if (k == 1)
                {
                    newPos_x = GeneratorPos_x;
                    newPos_y = GeneratorPos_y + 1;
                }
                // Move Right:
                if (k == 2)
                {
                    newPos_x = GeneratorPos_x + 1;
                    newPos_y = GeneratorPos_y;
                }
                // Move Left:
                if (k == 3)
                {
                    newPos_x = GeneratorPos_x - 1;
                    newPos_y = GeneratorPos_y;
                }

                if (LevelLength - newPos_x < 28 || newPos_x < 28 || LevelLength - newPos_y < 28 || newPos_y < 28)
                { continue; }

                // Check if there is a room:
                bool roomCheck = false;
                foreach (Room room in Rooms)
                {
                    if (room.Objects.Any(x => (x.Position_x == newPos_x && x.Position_y == newPos_y)))
                    {
                        roomCheck = true;
                        break;
                    }
                }

                if (roomCheck)
                {
                    continue;
                }

                k = rnd.Next(0, 11);
                // Generate floor
                if (k < 11)
                {
                    if (PutFloor(newPos_x, newPos_y))
                    {
                        GeneratorPos_x = newPos_x;
                        GeneratorPos_y = newPos_y;
                        Floors++;
                    }
                }
            }

            // Generating from 9 to 23 rooms:

            List<LevelObject> floors = Objects.FindAll(x => x.GetType() == (new Floor()).GetType());
     
            for (int i = 0; i < rnd.Next(9, 24);)
            {
                LevelObject floorToChange = floors[rnd.Next(0, floors.Count)];
                if (floorToChange.Position_x > 22 && floorToChange.Position_y > 22 && LevelLength - floorToChange.Position_x > 22 && LevelLength - floorToChange.Position_y > 22)
                {
                    // Check if the is no evident way to the first door:
                    if (Objects.Find(obj => (obj.Position_x == floorToChange.Position_x + 1 && obj.Position_y == floorToChange.Position_y)).GetType() == (new Wall()).GetType())
                    {
                        continue;
                    }
                    if (Objects.Find(obj => (obj.Position_x == floorToChange.Position_x - 1 && obj.Position_y == floorToChange.Position_y)).GetType() == (new Wall()).GetType())
                    {
                        continue;
                    }
                    if (Objects.Find(obj => (obj.Position_x == floorToChange.Position_x && obj.Position_y == floorToChange.Position_y + 1)).GetType() == (new Wall()).GetType())
                    {
                        continue;
                    }
                    if (Objects.Find(obj => (obj.Position_x == floorToChange.Position_x && obj.Position_y == floorToChange.Position_y - 1)).GetType() == (new Wall()).GetType())
                    {
                        continue;
                    }
                    Room room = GenRoom(floorToChange.Position_x, floorToChange.Position_y);
                    if (!floors.Any(x => x.Position_x == room.D2_Pos_x && x.Position_y == room.D2_Pos_y))
                    {
                        continue;
                    }
                    // Check if it intersects with an existing room:
                    bool cont = false;
                    foreach (Room room_to_check in Rooms)
                    {
                        foreach (LevelObject obj in room_to_check.Objects)
                        {
                            if (room.Objects.Any(x => x.Position_x == obj.Position_x && x.Position_y == obj.Position_y))
                            {
                                cont = true;
                                break;
                            }
                        }
                        if (cont)
                        {
                            break;
                        }
                    }

                    // Check if the second door leads to nowhere:
                    if (Objects.Find(obj => (obj.Position_x == room.D2_Pos_x + 1 && obj.Position_y == room.D2_Pos_y)).GetType() == (new Wall()).GetType())
                    {
                        cont = true;
                    }
                    if (Objects.Find(obj => (obj.Position_x == room.D2_Pos_x - 1 && obj.Position_y == room.D2_Pos_y)).GetType() == (new Wall()).GetType())
                    {
                        cont = true;
                    }
                    if (Objects.Find(obj => (obj.Position_x == room.D2_Pos_x && obj.Position_y == room.D2_Pos_y + 1)).GetType() == (new Wall()).GetType())
                    {
                        cont = true;
                    }
                    if (Objects.Find(obj => (obj.Position_x == room.D2_Pos_x && obj.Position_y == room.D2_Pos_y - 1)).GetType() == (new Wall()).GetType())
                    {
                        cont = true;
                    }

                    if (cont)
                    {
                        continue;
                    }


                    Rooms.Add(room);
                    foreach (LevelObject obj in room.Objects)
                    {
                        Objects.RemoveAll(x => x.Position_x == obj.Position_x && x.Position_y == obj.Position_y);
                        Objects.Add(obj);
                    }
                    i++;
                }
                else
                {
                    continue;
                }

            }
        }

        public bool PutFloor(int pos_x, int pos_y)
        {
            bool check = false;
            // Check if the Generator does not leave the level bounds
            if (Objects.Any(x => x.Position_x == pos_x && x.Position_y == pos_y))
            {
                // Delete the existing element
                Objects.RemoveAll(x => x.Position_x == pos_x && x.Position_y == pos_y);
                // Put floor there:
                Objects.Add(new Floor() { Position_x = pos_x, Position_y = pos_y });
                check = true;
            }
            return check;
        }


        public Room GenRoom(int pos_x, int pos_y)
        {
            Room room = new Room(pos_x, pos_y);
            room.Generate();

            return room;

        }
        
        // THE GAME SUFFERS A SERIOUS LACK OF OPIMIZATION
        // So this method will now delete all border (useless in gameplay) walls:
        public void Clear()
        {
            LevelObject obj = new LevelObject();
            bool con = false;
            for (int x = 0; x < LevelLength; x++)
            {
                for (int y = 0; y < LevelLength; y++)
                {
                    obj.Position_x = x;
                    obj.Position_y = y;
                    if (obj.GetType() == (new Wall()).GetType())
                    {
                        Objects.RemoveAll(o => o.Position_x == obj.Position_x && o.Position_y == obj.Position_y);
                    }
                    else
                    {
                        con = true;
                        break;
                    }
                }
                if (con)
                {
                    break;
                }
            }

            for (int x = 0; x < LevelLength; x++)
            {
                for (int y = 0; y < LevelLength; y++)
                {
                    obj.Position_x = LevelLength - x;
                    obj.Position_y = LevelLength - y;
                    if (obj.GetType() == (new Wall()).GetType())
                    {
                        Objects.RemoveAll(o => o.Position_x == obj.Position_x && o.Position_y == obj.Position_y);
                    }
                    else
                    {
                        con = true;
                        break;
                    }
                }
                if (con)
                {
                    break;
                }
            }
        }

        public void Place()
        {
            foreach (Room room in Rooms)
            {
                if (rnd.Next(0, 6) > 1)
                {
                    ItemContainer itemContainer = new ItemContainer();
                    itemContainer.Position_x = room.Center_x;
                    itemContainer.Position_y = room.Center_y;
                    int i = rnd.Next(1, 101);
                        if ( i > 0 && i <= 20 )
                        {
                            itemContainer.Weapons.Add(new WoodenDagger());
                        }
                        if (i > 20 && i <= 35)
                        {
                            itemContainer.Weapons.Add(new IronSword());
                        }
                        if (i > 35 && i <= 45)
                        {
                            itemContainer.Weapons.Add(new GlassDagger());
                        }
                        if (i > 45 && i <= 50)
                        {
                            itemContainer.Weapons.Add(new SilverSpear());
                        }

                    i = rnd.Next(1, 101);

                    if (i > 0 && i <= 20)
                        {
                            itemContainer.Armors.Add(new LeatherArmor());
                        }
                        if (i > 20 && i <= 35)
                        {
                            itemContainer.Armors.Add(new IronArmor());
                        }
                        if (i > 35 && i <= 50)
                        {
                            itemContainer.Armors.Add(new GlassArmor());
                        }
                        if (i > 50 && i <= 60)
                        {
                            itemContainer.Armors.Add(new SteelArmor());
                        }
                        if (i > 60 && i <= 65)
                        {
                            itemContainer.Armors.Add(new SilverArmor());
                        }

                    ItemContainers.Add(itemContainer);
                    Objects.Add(itemContainer);
                }
            }
        }

        public void Spawn(Player player)
        {
            List<LevelObject> floors = Objects.FindAll(x => x.Name == "floor");
            // Spawn Player:
            while(true)
            {
                int p = rnd.Next(0, floors.Count);
                if (ItemContainers.Any(i => i.Position_x == floors[p].Position_x && i.Position_y == floors[p].Position_y))
                {
                    continue;
                }
                else
                {
                    player.Position_x = floors[p].Position_x;
                    player.Position_y = floors[p].Position_y;
                    Player = player;
                    Objects.Add(Player);
                    Creatures.Add(Player);
                    break;
                }
            }

            // Spawn Skeletons:
            for (int i = 0; i < 20; )
            {
                int s = rnd.Next(0, floors.Count);
                if (Creatures.Any(obj => obj.Position_x == floors[s].Position_x && obj.Position_y == floors[s].Position_y) || ItemContainers.Any(c => c.Position_x == floors[s].Position_x && c.Position_y == floors[s].Position_y))
                {
                    continue;
                }
                else
                {
                    Skeleton skeleton = new Skeleton(ID);
                    skeleton.Position_x = floors[s].Position_x;
                    skeleton.Position_y = floors[s].Position_y;
                    skeleton.Spawn_Position_x = skeleton.Position_x;
                    skeleton.Spawn_Position_y = skeleton.Position_y;
                    skeleton.Habitat_x_max = skeleton.Position_x + 2;
                    skeleton.Habitat_x_min = skeleton.Position_x - 2;
                    skeleton.Habitat_y_max = skeleton.Position_y + 2;
                    skeleton.Habitat_y_min = skeleton.Position_y - 2;
                    skeleton.Follow_x_max = skeleton.Position_x + 6;
                    skeleton.Follow_x_min = skeleton.Position_x - 6;
                    skeleton.Follow_y_max = skeleton.Position_y + 6;
                    skeleton.Follow_y_min = skeleton.Position_y - 6;

                    Objects.Add(skeleton);
                    Creatures.Add(skeleton);
                    i++;
                }
            }
            // Spawn Skeleton Warriors:
            for (int i = 0; i < 10;)
            {
                int z = rnd.Next(0, floors.Count);
                if (Creatures.Any(obj => obj.Position_x == floors[z].Position_x && obj.Position_y == floors[z].Position_y) || ItemContainers.Any(c => c.Position_x == floors[z].Position_x && c.Position_y == floors[z].Position_y) || Math.Abs(floors[z].Position_x - Player.Position_x) < 30 || Math.Abs(floors[z].Position_y - Player.Position_y) < 30)
                {
                    continue;
                }
                else
                {
                    SkeletonWarrior skeleton = new SkeletonWarrior();
                    skeleton.Position_x = floors[z].Position_x;
                    skeleton.Position_y = floors[z].Position_y;
                    skeleton.Spawn_Position_x = skeleton.Position_x;
                    skeleton.Spawn_Position_y = skeleton.Position_y;
                    skeleton.Habitat_x_max = skeleton.Position_x + 2;
                    skeleton.Habitat_x_min = skeleton.Position_x - 2;
                    skeleton.Habitat_y_max = skeleton.Position_y + 2;
                    skeleton.Habitat_y_min = skeleton.Position_y - 2;
                    skeleton.Follow_x_max = skeleton.Position_x + 4;
                    skeleton.Follow_x_min = skeleton.Position_x - 4;
                    skeleton.Follow_y_max = skeleton.Position_y + 4;
                    skeleton.Follow_y_min = skeleton.Position_y - 4;
                    Objects.Add(skeleton);
                    Creatures.Add(skeleton);
                    i++;
                }
            }
            // Spawn Guards:
            for (int i = 0; i < 9; i++)
            {
                Guard guard = new Guard();
                guard.Position_x = Rooms[i].Center_x - 1;
                guard.Position_y = Rooms[i].Center_y - 1;
                guard.Spawn_Position_x = guard.Position_x;
                guard.Spawn_Position_y = guard.Position_y;
                guard.Habitat_x_max = (guard.Spawn_Position_x + 1) + (Rooms[i].Length) / 2 - 1;
                guard.Habitat_x_min = (guard.Spawn_Position_x + 1) - (Rooms[i].Length) / 2 + 1;
                guard.Habitat_y_max = (guard.Spawn_Position_y + 1) + (Rooms[i].Width) / 2 - 1;
                guard.Habitat_y_min = (guard.Spawn_Position_y + 1) - (Rooms[i].Width) / 2 + 1;
                guard.Follow_x_max = guard.Habitat_x_max;
                guard.Follow_y_max = guard.Habitat_y_max;
                guard.Follow_x_min = guard.Habitat_x_min;
                guard.Follow_y_min = guard.Habitat_y_min;
                Objects.Add(guard);
                Creatures.Add(guard);
            }
        }
    }
}
