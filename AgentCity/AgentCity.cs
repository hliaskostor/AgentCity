using AgentCity;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Windows.Forms;

namespace AgentCity
{
    public partial class AgentCity : Form
    {
        City city;
     
        Agent[] agents;
        Random random;
        Timer time;
        int totalenergyPots = 10;
        int energyPotgold = 10;
        int energyPotValue = 20;
        int spentenergy = 1;
        int beginEnergy = 100;
        string stateFile = "state.txt";
        int backHome = 0;
        List<totalstatistics> Statistics = new List<totalstatistics>();
        Color[] colors;
        bool agentAnswer = false;
        Dictionary<int, bool> messageEnergy = new Dictionary<int, bool>();
        HashSet<(int, int)> buyEnergy = new HashSet<(int, int)>();
        int N = 100;
        int M = 100;
        int homeX = 0;
        int homeY = 0;
        Dictionary<int, (int, int)> agentPositions = new Dictionary<int, (int, int)>();
      
        public AgentCity()
        {
            InitializeComponent();
            random = new Random();
            time = new Timer();
            time.Interval = 1000;
            time.Tick += Timer_Tick;
         
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to refresh the city and agents?", "Confirm Refresh", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {

                city = new City("map.txt");
                viewAgents();
                loadEnergy();
                agentCity();

            }
        }
        private void resumeButton_Click(object sender, EventArgs e)
        {
            time.Start();
            showState();
        }
        private void AgentCity_Load(object sender, EventArgs e)
        {

            this.Size = new Size(1652, 780);
            agentPlans();
            city = new City("map.txt");
            viewAgents();
            loadEnergy();
            agentCity();
        }
        private void startButton_Click(object sender, EventArgs e)
        {
            time.Start();
        }
        private void stopButton_Click(object sender, EventArgs e)
        {
            time.Stop();
            agentStatistics();
            agentState();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            agentPosition();
            agentCity();
        }

        private void agentStatistics()
        {
                using (StreamWriter writer = new StreamWriter("statistics.txt"))
                {
                    foreach (var agent in agents)
                    {
                        writer.WriteLine($"Agent ID: {agent.Id}, Energy: {Math.Max(agent.Energy, 0)}, Gold Collected: {Math.Max(agent.totalGold, 0)}");
                        writer.WriteLine($"Steps: {agent.stepsHistory.Count} steps");


                        writer.WriteLine();
                    }
                } 
        }

        public void viewAgents()
        {
            agents = new Agent[4];
            colors = new Color[] { Color.Blue, Color.Green, Color.Red, Color.Black };

            Dictionary<int, (int, int)> homePositions = new Dictionary<int, (int, int)>();
            for (int x = 0; x < city.Width; x++)
            {
                for (int y = 0; y < city.Height; y++)
                {
                    char tile = city.Map[x, y];
                    if (tile >= '1' && tile <= '4')
                    {
                        int agentId = tile - '0';
                        homePositions[agentId] = (x, y);
                    }
                }
            }

            for (int i = 0; i < agents.Length; i++)
            {
                int agentId = i + 1;
                if (homePositions.TryGetValue(agentId, out var homePosition))
                {
                    int startX = homePosition.Item1;
                    int startY = homePosition.Item2;
                    agents[i] = new Agent(agentId, beginEnergy, startX, startY, colors[i % colors.Length], energyPotValue);
                    agents[i].stepsHistory.Add((startX, startY));
                }
                else
                {
                    MessageBox.Show($"Home position for Agent {agentId} not found on the map.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private int AgentName(int x, int y)
        {

            char tile = city.Map[x, y];
            return tile - '0';
        }

        private void removeAgent(Agent agent)
        {
            if (agent.X >= 0 && agent.Y >= 0 && agent.X < city.Width && agent.Y < city.Height)
            {
                city.Map[agent.X, agent.Y] = ' ';
            }
            agent.X = -1;
            agent.Y = -1;
        }

        private void agentPlans()
        {
            try
            {
                if (File.Exists("plans.txt"))
                {
                    string[] lines = File.ReadAllLines("plans.txt");

                    int currentAgentId = -1;
                    List<int> currentAgentPlans = new List<int>();

                    foreach (var line in lines)
                    {
                        if (line.StartsWith("BEGINPLAN"))
                        {

                            currentAgentId = int.Parse(line.Split(' ')[1]);
                            currentAgentPlans.Clear();
                        }
                        else if (line.StartsWith("Building"))
                        {

                            int buildingNumber = int.Parse(line.Split(' ')[1]);
                            currentAgentPlans.Add(buildingNumber);
                        }
                        else if (line.StartsWith("ENDPLAN"))
                        {

                            var agent = agents.FirstOrDefault(a => a.Id == currentAgentId);
                            if (agent != null)
                            {
                                agent.Plan = currentAgentPlans.ToList();
                            }
                            else
                            {
                                MessageBox.Show($"Agent {currentAgentId} not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void agentPosition()
        {
            var shownMessages = new HashSet<(int, int)>();

            foreach (var agent in agents)
            {
                if (!messageEnergy.ContainsKey(agent.Id))
                {
                    messageEnergy[agent.Id] = false;
                }

                agent.EnergySpent(spentenergy);

                if (agent.Energy <= 0 && !messageEnergy[agent.Id])
                {
                    messageEnergy[agent.Id] = true;
                    bool hasGoldsLeft = city.Map.Cast<char>().Any(tile => tile == 'G');

                    if (hasGoldsLeft)
                    {
                        DialogResult result = MessageBox.Show($"Agent {agent.Id} has run out of energy. Do you want to buy an Energy Pot for {energyPotgold} gold?", "Buy Energy Pot", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            if (agent.totalGold >= energyPotgold)
                            {
                                agent.totalGold -= energyPotgold;
                                agent.AddEnergy(energyPotValue);
                                messageEnergy[agent.Id] = false;
                            }
                            else
                            {
                                MessageBox.Show($"Agent {agent.Id} does not have enough gold to buy an Energy Pot. The agent will be removed.", "Not Enough Gold", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                agent.Energy = 0;
                                removeAgent(agent);
                            }
                        }
                        else
                        {
                            MessageBox.Show($"The agent will be removed.", "Not Enough Gold", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            agent.Energy = 0;
                            removeAgent(agent);
                        }
                    }
                    else
                    {
                        agent.Energy = 0;
                        removeAgent(agent);
                        MessageBox.Show($"Agent {agent.Id} has run out of energy. There are no more golds to buy Energy Pots.", "No Golds Left", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                if (agent.X == -1 && agent.Y == -1)
                {
                    continue;
                }

                foreach (var agent2 in agents)
                {
                    if (agent != agent2 && agent2.X != -1 && agent2.Y != -1 &&
                        Math.Abs(agent.X - agent2.X) <= 1 && Math.Abs(agent.Y - agent2.Y) <= 1)
                    {
                        var agentPairKey = agent.Id < agent2.Id ? (agent.Id, agent2.Id) : (agent2.Id, agent.Id);

                        if (!shownMessages.Contains(agentPairKey) && !buyEnergy.Contains(agentPairKey))
                        {
                            shownMessages.Add(agentPairKey);
                            buyEnergy.Add(agentPairKey);

                            DialogResult result = MessageBox.Show($"Agent {agent2.Id} wants to trade an Energy Pot with Agent {agent.Id}. \nYes to Buy from Agent {agent.Id}.\n No to Sell to Agent {agent.Id}.", "Trade Energy Pot", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                            if (result == DialogResult.Yes)
                            {
                                if (agent.X != -1 && agent.Y != -1 && agent.Energy >= energyPotValue &&
                                    agent2.X != -1 && agent2.Y != -1 && agent2.totalGold >= energyPotgold)
                                {
                                    agent.Energy -= energyPotValue;
                                    agent.totalGold += energyPotgold;

                                    agent2.totalGold -= energyPotgold;
                                    agent2.AddEnergy(energyPotValue);

                                    MessageBox.Show($"Transaction Complete: Agent {agent.Id} sold an Energy Pot to Agent {agent2.Id}.", "Transaction Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else if (agent.Energy < energyPotValue)
                                {
                                    MessageBox.Show($"Agent {agent.Id} does not have enough energy to give at Agent{agent2.Id}.", "Insufficient Energy", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else if (agent2.totalGold < energyPotgold)
                                {
                                    MessageBox.Show($"Agent {agent2.Id} does not have enough gold to buy Energy Pot.", "Insufficient Gold", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            else if (result == DialogResult.No)
                            {
                                if (agent2.X != -1 && agent2.Y != -1 && agent2.Energy >= energyPotValue &&
                                    agent.X != -1 && agent.Y != -1 && agent.totalGold >= energyPotgold)
                                {
                                    agent2.Energy -= energyPotValue;
                                    agent2.totalGold += energyPotgold;

                                    agent.totalGold -= energyPotgold;
                                    agent.AddEnergy(energyPotValue);

                                    MessageBox.Show($"Transaction Complete: Agent {agent2.Id} sold an Energy Pot to Agent {agent.Id}.", "Transaction Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else if (agent2.Energy < energyPotValue)
                                {
                                    MessageBox.Show($"Agent {agent2.Id} does not have enough energy to give at Agent {agent.Id}.", "Insufficient Energy", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else if (agent.totalGold < energyPotgold)
                                {
                                    MessageBox.Show($"Agent {agent.Id} does not have enough gold to buy EnergyPot.", "Insufficient Gold", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                    }
                }

                if (agent.Energy > 0)
                {
                    int newX, newY;
                    do
                    {
                        newX = agent.X + random.Next(-4, 5);
                        newY = agent.Y + random.Next(-4, 5);
                        newX = Math.Max(0, Math.Min(newX, city.Width - 1));
                        newY = Math.Max(0, Math.Min(newY, city.Height - 1));
                    }
                    while ((newX == agent.X && newY == agent.Y) || (city.Map[newX, newY] != ' ' && city.Map[newX, newY] != 'E' && city.Map[newX, newY] != 'G' && city.Map[newX, newY] != '1' && city.Map[newX, newY] != '2' && city.Map[newX, newY] != '3' && city.Map[newX, newY] != '4'));
                    if (city.Map[newX, newY] == 'E')
                    {
                        agent.AddEnergy(energyPotValue); 
                        messageEnergy[agent.Id] = false;
                    }

                    if (city.Map[newX, newY] == 'G')
                    {
                        agent.totalGold += 10;
                    }

                    city.Map[agent.X, agent.Y] = ' ';
                    agent.Move(newX, newY);
                    city.Map[agent.X, agent.Y] = agent.Id.ToString()[0];

                    if (city.Map[newX, newY] == 'E' || city.Map[newX, newY] == 'G')
                    {
                        city.Map[newX, newY] = ' ';
                    }
                }

                if (agent.Id == 1 && agent.X == homeX && agent.Y == homeY && agent.Energy > 0)
                {
                    backHome++;
                }
            }

            if (backHome == 1)
            {
                time.Stop();
                agentStatistics();
                agentState();
            }

            infoAgents();
            agentCity();
        }
        private void answerAgent(Agent agent)
        {

            agent.Energy = 100;
            agent.totalGold = 0;


            city.Map[agent.X, agent.Y] = ' ';
            agent.X = homeX;
            agent.Y = homeY;
            city.Map[homeX, homeY] = agent.Id.ToString()[0];

            messageEnergy[agent.Id] = false;
        }

        private void loadEnergy()
        {
            for (int i = 0; i < totalenergyPots; i++)
            {
                int x, y;

                do
                {
                    x = random.Next(city.Width);
                    y = random.Next(city.Height);
                }
                while (city.Map[x, y] != ' ');

                if (agents.Any(agent => agent.X == x && agent.Y == y))
                {
                    continue;
                }

                city.Map[x, y] = 'E';
            }


            for (int i = 0; i < totalenergyPots; i++)
            {
                int x, y;

                do
                {
                    x = random.Next(city.Width);
                    y = random.Next(city.Height);
                }
                while (city.Map[x, y] != ' ');

                if (agents.Any(agent => agent.X == x && agent.Y == y))
                {
                    continue;
                }

                city.Map[x, y] = 'G';
            }



            if (!city.Map.Cast<char>().Any(tile => tile == 'G'))
            {
                DialogResult result = MessageBox.Show("All gold pots have been collected. Do you want to renew the city?", "Renew City", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    city = new City("map.txt");
                    loadEnergy();
                    agentCity();
                }
            }
        }
        private void agentCity()
        {
            if (city == null)
                return;

            int buildWidth = 42;
            int buildHeight = 42;

            int pictureBoxWidth = city.Width * buildWidth;
            int pictureBoxHeight = city.Height * buildHeight;

            pictureBox1.Width = pictureBoxWidth;
            pictureBox1.Height = pictureBoxHeight;

            Bitmap bitmap = new Bitmap(pictureBoxWidth, pictureBoxHeight);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                for (int y = 0; y < city.Height; y++)
                {
                    for (int x = 0; x < city.Width; x++)
                    {
                        char tile = city.Map[x, y];
                        Color color = colorBuildings(tile);
                        int currentTileWidth = buildWidth;
                        int currentTileHeight = buildHeight;
                        g.FillRectangle(new SolidBrush(color), x * buildWidth, y * buildHeight, currentTileWidth, currentTileHeight);

                        switch (tile)
                        {
                            case 'R':
                                if (color == Color.White)
                                {
                                    textSize(g, "Road", Font, Brushes.Black, x, y, buildWidth, buildHeight);
                                }
                                break;
                            case 'P':
                                if (color == Color.Green)
                                {
                                    textSize(g, "Park", Font, Brushes.White, x, y, currentTileWidth, currentTileHeight);
                                }
                                break;
                            case 'T':
                                if (color == Color.Red)
                                {
                                    textSize(g, "Post", Font, Brushes.White, x, y, buildWidth, buildHeight);
                                }
                                break;
                            case 'B':
                                if (color == Color.Orange)
                                {
                                    textSize(g, "Bank", Font, Brushes.Black, x, y, buildWidth, buildHeight);
                                }
                                break;
                            case 'M':
                                if (color == Color.Yellow)
                                {
                                    textSize(g, "Market", Font, Brushes.Black, x, y, buildWidth, buildHeight);
                                }
                                break;
                            case 'W':
                                if (color == Color.DarkGray)
                                {
                                    textSize(g, "Wall", Font, Brushes.White, x, y, buildWidth, buildHeight);
                                }
                                break;
                            case 'E':
                                if (color == Color.SkyBlue)
                                {
                                    textSize(g, "Energy \n Pot", Font, Brushes.Black, x, y, buildWidth, buildHeight);
                                }
                                break;
                            case 'G':
                                if (color == Color.Gold)
                                {
                                    textSize(g, "Gold \n Pot", Font, Brushes.Black, x, y, buildWidth, buildHeight);
                                }
                                break;
                            case 'h':
                                textSize(g, "Home \n Agent", Font, Brushes.White, x, y, buildWidth, buildHeight);
                                break;
                            default:
                                break;
                        }
                        foreach (var agent in agents)
                        {
                            if (agent.X == x && agent.Y == y)
                            {
                                int centerX = x * buildWidth + buildWidth / 2;
                                int centerY = y * buildHeight + buildHeight / 2;
                                int agentRadius = 15;
                                g.FillEllipse(new SolidBrush(agent.Color), centerX - agentRadius, centerY - agentRadius, agentRadius * 2, agentRadius * 2);
                                g.DrawString(agent.Id.ToString(), Font, Brushes.White, centerX - 5, centerY - 5);
                            }
                        }
                    }
                }
            }

            pictureBox1.Image = bitmap;
        }

        private void textSize(Graphics g, string text, Font font, Brush brush, int x, int y, int tileWidth, int tileHeight)
        {
            SizeF textSize = g.MeasureString(text, font);
            float textX = x * tileWidth + (tileWidth - textSize.Width) / 2;
            float textY = y * tileHeight + (tileHeight - textSize.Height) / 2;

            g.DrawString(text, font, brush, textX, textY);

        }

        private Color colorBuildings(char buildings)
        {
            switch (buildings)
            {
                case 'R':
                    return Color.White;
                case 'P':
                    return Color.Green;
                case 'T':
                    return Color.Red;
                case 'B':
                    return Color.Orange;
                case 'M':
                    return Color.Yellow;
                case 'h':
                    return Color.Brown;
                case 'W':
                    return Color.DarkGray;
                case 'E':
                    return Color.SkyBlue;
                case 'G':
                    return Color.Gold;
                default:
                    return Color.Gray;
            }
        }

        private void infoAgents()
        {
            if (agents == null) return;

            label1.Text = agents.Length > 0 ? $"Agent 1: Energy={Math.Max(agents[0].Energy, 0)}, Gold={Math.Max(agents[0].totalGold, 0)}, {stepsAgent(agents[0])}" : "Agent 1: Not Initialized";
            label2.Text = agents.Length > 1 ? $"Agent 2: Energy={Math.Max(agents[1].Energy, 0)}, Gold={Math.Max(agents[1].totalGold, 0)},  {stepsAgent(agents[1])}" : "Agent 2: Not Initialized";
            label3.Text = agents.Length > 2 ? $"Agent 3: Energy={Math.Max(agents[2].Energy, 0)}, Gold={Math.Max(agents[2].totalGold, 0)},  {stepsAgent(agents[2])}" : "Agent 3: Not Initialized";
            label4.Text = agents.Length > 3 ? $"Agent 4: Energy={Math.Max(agents[3].Energy, 0)}, Gold={Math.Max(agents[3].totalGold, 0)},   {stepsAgent(agents[3])}" : "Agent 4: Not Initialized";
        }

        private string stepsAgent(Agent agent)
        {
            int numberOfSteps = agent.stepsHistory.Count;
            return $"{numberOfSteps} steps";
        }
    
 
        private void agentState()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(stateFile))
                {
                    foreach (var agent in agents)
                    {
                        writer.WriteLine($"{agent.Id},{agent.X},{agent.Y},{agent.Energy},{agent.totalGold}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving agent state: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void showState()
        {
            try
            {
                if (File.Exists(stateFile))
                {
                    string[] lines = File.ReadAllLines(stateFile);
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split(',');
                        if (parts.Length == 5)
                        {
                            int agentId = int.Parse(parts[0]);
                            int x = int.Parse(parts[1]);
                            int y = int.Parse(parts[2]);
                            int energy = int.Parse(parts[3]);
                            int goldAgents = int.Parse(parts[4]);


                            Agent agent = agents.FirstOrDefault(a => a.Id == agentId);
                            if (agent != null)
                            {
                                agent.X = x;
                                agent.Y = y;
                                agent.Energy = energy;
                                agent.totalGold = goldAgents;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading agent state: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void exit_Click(object sender, EventArgs e)
        {
            agentState();
            this.Close();
        }

        private void viewButton_Click(object sender, EventArgs e)
        {
           
                if (File.Exists("statistics.txt"))
                {
                    string statisticsText = File.ReadAllText("statistics.txt");
                    MessageBox.Show(statisticsText, "Agent Statistics", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Agent statistics file not found.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            
          
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
