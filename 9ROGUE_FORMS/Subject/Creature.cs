using _9ROGUE_FORMS.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _9ROGUE_FORMS.Subject
{
    class Creature : Subject
    {
        public List<LevelObject> NearObjects { get; set; }
        // Level:
        // Уровень:
        public int Lvl { get; set; }

        // Exp:
        // Опыт:
        public int Exp { get; set; }


        // Attributes:
        // Основные характеристики:
        public int Strength { get; set; }
        public int Intelligence { get; set; }
        public int Dexterity { get; set; }
        public int Endurance { get; set; }

        // Health:
        // Очки здоровья:
        public double HP { get; set; }
        public int HP_max { get; set; }

        // Habitat (borders of free-move zone):
        // Границы, внутри которых creature(монстр) свободно перемещается:
        public int Habitat_x_min { get; set; }
        public int Habitat_x_max { get; set; }
        public int Habitat_y_min { get; set; }
        public int Habitat_y_max { get; set; }

        // Trigger (does the creature follows the player)
        // Триггер (определяет, следует ли монстр за игроком):
        public bool IsTriggered { get; set; }

        // Follow borders:
        // Границы, в пределах которых монстр следует за игроком
        public int Follow_x_min { get; set; }
        public int Follow_x_max { get; set; }
        public int Follow_y_min { get; set; }
        public int Follow_y_max { get; set; }

        // Inventory:
        // Инвентарь:
        public Inventory Inventory { get; set; }

        // Active weapon:
        // "Надетое" оружие:
        public Weapon Active_Weapon { get; set; }

        // Active armor:
        // Надетая броня:
        public Armor Active_Armor { get; set; }

        public bool IsAlive { get; set; }

        public int Spawn_Position_x { get; set; }
        public int Spawn_Position_y { get; set; }



        public Creature()
        {
            NearObjects = new List<LevelObject>();
            Lvl = 1;
            HP_max = 1;
            HP = 1;
            Strength = 1;
            Intelligence = 1;
            Dexterity = 1;
            Endurance = 1;
            Inventory = new Inventory();
            IsAlive = true;
            Active_Weapon = new Weapon();
            Active_Armor = new Armor();
        }

        public virtual void Move(int Position_x_new, int Position_y_new)
        {
            Position_x = Position_x_new;
            Position_y = Position_y_new;
        }

        public virtual List<string> Attack(Creature creature, Random rnd)
        { 

            List<string> result = new List<string>();
            result.Add(String.Format(">{0} attacks {1}\n", Name, creature.Name));

            double damage = Strength * rnd.Next(2, 5) * 0.1 + Active_Weapon.Damage * 0.4 - creature.Active_Armor.Protection * 0.2;
            double сrit_chance = Endurance * 1.5;
            double creature_dodge_chance = creature.Endurance * 0.9;

            if (rnd.Next(Convert.ToInt32(100 / сrit_chance)) == (Convert.ToInt32(100 / (сrit_chance)) / 2))
            {
                damage *= 2;
                result.Add(String.Format(">{0} manages to get a critical hit on {1}!\n", Name, creature.Name));
            }

            if (rnd.Next(Convert.ToInt32(100 / creature_dodge_chance)) == (Convert.ToInt32(100 / (creature_dodge_chance)) / 2))
            {
                damage = 0;
                result.Add(String.Format(">{0} dodges {1}'s attack!\n", creature.Name, Name));
            }

            creature.HP -= damage;

            result.Add(String.Format(">{0} loses {1} hp\n", creature.Name, Decimal.Round(Convert.ToDecimal(damage), 2)));
            if (creature.HP <= 0)
            {
                result.Add(String.Format("\n>{0} passes away\n", creature.Name));
                Exp += Convert.ToInt32(creature.Exp / 4);
                result.Add(String.Format("\n>{0} receives {1} exp\n", Name, Convert.ToInt32(creature.Exp / 4)));
                if (rnd.Next(4) == 0)
                {
                    Inventory.Poisons.Add(new Poison(rnd));
                    result.Add(String.Format("\n{0} had a health poison! Now it belongs to {1}\n", creature.Name, Name));
                }
            }
            result.Add("\n");
            return result;
        }

        // Движение к точке:
        public void MoveTo(int to_position_x, int to_position_y, bool IsComingBack)
        {
            int dif_x = Position_x - to_position_x;
            int dif_y = Position_y - to_position_y;
            // Move left:
            if (dif_x > 0 && dif_y == 0)
            {
                if (MovementCheck( Position_x - 1, Position_y, IsComingBack))
                {
                    Move(Position_x - 1, Position_y);
                }
                return;
            }
            // Move right:
            if (dif_x < 0 && dif_y == 0)
            {
                if (MovementCheck( Position_x + 1, Position_y, IsComingBack))
                {
                    Move(Position_x + 1, Position_y);
                }
                return;
            }
            // Move down:
            if (dif_x == 0 && dif_y < 0)
            {
                if (MovementCheck( Position_x, Position_y + 1, IsComingBack))
                {
                    Move(Position_x, Position_y + 1);
                }
                return;
            }
            // Move up:
            if (dif_x == 0 && dif_y > 0)
            {
                if (MovementCheck( Position_x, Position_y - 1, IsComingBack))
                {
                    Move(Position_x, Position_y - 1);
                }
                return;
            }
            // Move up-left:
            if (dif_x > 0 && dif_y > 0)
            {
                if (MovementCheck( Position_x - 1, Position_y - 1, IsComingBack))
                {
                    Move(Position_x - 1, Position_y - 1);
                }
                return;
            }
            // Move up-right:
            if (dif_x < 0 && dif_y > 0)
            {
                if (MovementCheck( Position_x + 1, Position_y - 1, IsComingBack))
                {
                    Move(Position_x + 1, Position_y - 1);
                }
                return;
            }
            // Move down-right:
            if (dif_x < 0 && dif_y < 0)
            {
                if (MovementCheck( Position_x + 1, Position_y + 1, IsComingBack))
                {
                    Move(Position_x + 1, Position_y + 1);
                }
                return;
            }
            // Move down-left:
            if (dif_x > 0 && dif_y < 0)
            {
                if (MovementCheck( Position_x - 1, Position_y + 1, IsComingBack))
                {
                    Move(Position_x - 1, Position_y + 1);
                }
                return;
            }
        }

        // MovementCheck:
        public bool MovementCheck(int Position_x_new, int Position_y_new)
        {
            bool result = true;

            foreach (LevelObject obj in NearObjects)
            {
                if (obj.Position_x == Position_x_new && obj.Position_y == Position_y_new && obj.Clip == true)
                {
                    result = false;
                    break;
                }
            }

            if (Position_x_new < Habitat_x_min || Position_x_new > Habitat_x_max || Position_y_new < Habitat_y_min || Position_y_new > Habitat_y_max)
            {
                result = false;
            }

            return result;
        }

        public bool MovementCheck(int Position_x_new, int Position_y_new, bool IsComingBack)
        {
            bool result = true;

            foreach (LevelObject obj in NearObjects)
            {
                if (obj.Position_x == Position_x_new && obj.Position_y == Position_y_new && obj.Clip == true)
                {
                    result = false;
                    break;
                }
            }

            if (IsComingBack == false)
            {
                if (Position_x_new < Habitat_x_min || Position_x_new > Habitat_x_max || Position_y_new < Habitat_y_min || Position_y_new > Habitat_y_max)
                {
                    result = false;
                }
            }

            return result;
        }

    }
}
