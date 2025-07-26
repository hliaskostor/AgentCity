using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCity
{
    using System.Collections.Generic;
    using System.Drawing;
    using System;
   
        public class Agent
        {
      AgentCity agentCity;
            public int Id { get; private set; }
            public int Energy { get; set; }
            public int totalGold { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public Color Color { get; private set; }


            public List<int> Plan { get; set; } = new List<int>();

            private int planIndex;
            public List<(int X, int Y)> stepsHistory { get; set; }
            public bool IsTrading { get; set; }
            int energyPotValue;

            public Agent(int id, int maxEnergy, int startX, int startY, Color color, int energyPotValue)
            {
                Id = id;
                Energy = maxEnergy;
                X = startX;
                Y = startY;
                Color = color;

                planIndex = 0;
                stepsHistory = new List<(int X, int Y)>();
                this.energyPotValue = energyPotValue;
                Plan = new List<int>();

            }

            public void Move(int newX, int newY)
            {
                X = newX;
                Y = newY;
                stepsHistory.Add((newX, newY));
            }
            public void StartTrading()
            {
                IsTrading = true;
            }

            public void StopTrading()
            {
                IsTrading = false;
            }

            public void AddEnergy(int value)
            {
                Energy = Math.Min(100, Energy + value);
            }

            public void EnergySpent(int value)
            {
                Energy = Math.Max(0, Energy - value);
            }

            public bool newEnergyPot(int priceGold)
            {
                if (totalGold >= priceGold)
                {
                    totalGold -= priceGold;
                    return true;
                }
                return false;
            }


            public void sellEnergy(Agent buyer, int potGoldValue, int energyPotGoldValue)
            {
                if (Energy >= energyPotValue)
                {
                    Energy -= energyPotValue;
                    buyer.AddEnergy(energyPotValue);
                }
            }

        }
    }

