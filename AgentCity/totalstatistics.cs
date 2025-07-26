using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCity
{
    public class totalstatistics
    {
        public int AgentId { get; set; }
        public int Energy { get; set; }
        public int GoldCollected { get; set; }

        public totalstatistics(int agentId, int energy, int goldCollected)
        {
            AgentId = agentId;
            Energy = energy;
            GoldCollected = goldCollected;
        }
    }
}
