using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCity
{
    using System;
    using System.IO;

    public class City
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public char[,] Map { get; private set; }
        private bool[,] fixedMap;
        public City(string mapFile)
        {
            showMap(mapFile);
        }

        public void showMap(string mapFile)
        {
            string[] lines = File.ReadAllLines(mapFile);
            Height = lines.Length;


            Width = 0;
            foreach (var line in lines)
            {
                if (line.Length > Width)
                {
                    Width = line.Length;
                }
            }

            Map = new char[Width, Height];
            fixedMap = new bool[Width, Height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < lines[y].Length; x++)
                {
                    Map[x, y] = lines[y][x];
                }

                for (int x = lines[y].Length; x < Width; x++)
                {
                    Map[x, y] = ' ';
                }
            }
        }
        public void updateMap(string mapFile)
        {
            string[] lines = File.ReadAllLines(mapFile);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < lines[y].Length; x++)
                {

                    if (!fixedMap[x, y])
                    {
                        Map[x, y] = lines[y][x];
                    }
                }


                for (int x = lines[y].Length; x < Width; x++)
                {
                    if (!fixedMap[x, y])
                    {
                        Map[x, y] = ' ';
                    }
                }
            }
        }


        public void newMap()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Console.Write(Map[x, y]);
                }
                Console.WriteLine();
            }
        }
    }
}
