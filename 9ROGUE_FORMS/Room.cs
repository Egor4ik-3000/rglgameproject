using _9ROGUE_FORMS.Surroundings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _9ROGUE_FORMS
{
    class Room
    {
        public int D1_Pos_x { get; set; }
        public int D1_Pos_y { get; set; }
        public int D2_Pos_x { get; set; }
        public int D2_Pos_y { get; set; }
        public int Center_x { get; set; }
        public int Center_y { get; set; }
        public int Length { get; set; }
        public int Width { get; set; }
        public List<LevelObject> Doors { get; set; }

        public List<LevelObject> Objects { get; set; }

        public Room(int d1_pos_x, int d1_pos_y)
        {
            D1_Pos_x = d1_pos_x;
            D1_Pos_y = d1_pos_y;
            Objects = new List<LevelObject>();
            Doors = new List<LevelObject>();
        }

        public void Generate()
        {
            Random rnd = new Random();
            Length = (2 * rnd.Next(3, 6)) - 1;
            Width = (2 * rnd.Next(3, 6)) - 1;
            int z = rnd.Next(0, 2);
            if (z == 0)
            {
                Center_x = ((Length - 1) / 2) + D1_Pos_x;
            }
            else
            {
                Center_x = ((Length - 1) / 2) + D1_Pos_x - (Length - 1);
            }
            Center_y = (Width - 1) / 2 + D1_Pos_y - rnd.Next(1, Width - 2);
            // Стены по горизонтали:
            // Horizontal walls:
            for (int l = 0; l < Length; l++)
            {
                Wall wall_up = new Surroundings.Wall() { Position_x = Convert.ToInt32(Center_x - ((Length - 1) * 0.5)) + l, Position_y = Convert.ToInt32(Center_y - ((Width - 1) * 0.5)) };
                Wall wall_down = new Surroundings.Wall() { Position_x = Convert.ToInt32(Center_x - ((Length - 1) * 0.5)) + l, Position_y = Convert.ToInt32(Center_y + ((Width - 1) * 0.5)) };
                Objects.Add(wall_up);
                Objects.Add(wall_down);
            }
            // Стены по вертикали:
            // Vertical walls:
            for (int w = 0; w < Width; w++)
            {

                Wall wall_left = new Surroundings.Wall() { Position_x = Convert.ToInt32(Center_x - ((Length - 1) * 0.5)), Position_y = Convert.ToInt32(Center_y - ((Width - 1) * 0.5)) + w };
                Wall wall_rigth = new Surroundings.Wall() { Position_x = Convert.ToInt32(Center_x + ((Length - 1) * 0.5)), Position_y = Convert.ToInt32(Center_y - ((Width - 1) * 0.5)) + w };
                Objects.Add(wall_left);
                Objects.Add(wall_rigth);
            }
            // Заполнение полом:
            // Populating with floor:
            for (int w = 0; w < Width - 2; w++)
            {
                for (int l = 0; l < Length - 2; l++)
                {
                    Floor floor = new Floor() { Position_x = Convert.ToInt32(Center_x - (((Length - 1) * 0.5) - 1)) + l, Position_y = Convert.ToInt32(Center_y - (((Width - 1) * 0.5) - 1)) + w };
                    Objects.Add(floor);
                }
            }
            // Добавление дверей (ПОКА ТОЛЬКО 2):
            // Adding doors (ONLY 2 DOORS FOR NOW):

            // Первая дверь:
            // The first door:

            Objects.RemoveAll(x => x.Position_x == D1_Pos_x && x.Position_y == D1_Pos_y);
            Objects.Add(new Door() { Position_x = D1_Pos_x, Position_y = D1_Pos_y });


            // Делаем контейнер всех стен комнаты, чтобы выбрать из них случаный для замены на дверь:
            // We create a container for all walls the room has to choose a random wall and replace it with a door:

            List<LevelObject> walls = Objects.FindAll(x => x.GetType() == (new Wall()).GetType());
            bool check = false;
            while (!check)
            {
                LevelObject wallToDoor = walls[(rnd.Next(0, walls.Count))];
                // Проверяем, нет ли там уже двери и не выбрана ли стена в углу:
                // Check if there is a door already and if the chosen wall is in the corner
                if (
                    (((wallToDoor.Position_x == Convert.ToInt32(Center_x - ((Length - 1) * 0.5)))) && (wallToDoor.Position_y == Convert.ToInt32(Center_y - ((Width - 1) * 0.5)))) ||
                 (((wallToDoor.Position_x == Convert.ToInt32(Center_x + ((Length - 1) * 0.5)))) && (wallToDoor.Position_y == Convert.ToInt32(Center_y - ((Width - 1) * 0.5)))) ||
                  (((wallToDoor.Position_x == Convert.ToInt32(Center_x + ((Length - 1) * 0.5)))) && (wallToDoor.Position_y == Convert.ToInt32(Center_y + ((Width - 1) * 0.5)))) ||
                   (((wallToDoor.Position_x == Convert.ToInt32(Center_x - ((Length - 1) * 0.5)))) && (wallToDoor.Position_y == Convert.ToInt32(Center_y + ((Width - 1) * 0.5))))
                    )
                {
                    check = false;
                    continue;
                }
                //// Chech if the door leads to a wall:
                if ((Objects.Find(x => (x.Position_x == wallToDoor.Position_x && x.Position_y != wallToDoor.Position_y)).GetType() != (new Door()).GetType()))
                {

                    Door door = new Door() { Position_x = wallToDoor.Position_x, Position_y = wallToDoor.Position_y, IsUsed = false };
                    Objects.RemoveAll(x => x.Position_x == wallToDoor.Position_x && x.Position_y == wallToDoor.Position_y);
                    Objects.Add(door);
                    //Doors.Add(door);
                    D2_Pos_x = door.Position_x;
                    D2_Pos_y = door.Position_y;
                    check = true;
                }
                else
                {
                    check = false;
                }
            }
        }
    }
}
