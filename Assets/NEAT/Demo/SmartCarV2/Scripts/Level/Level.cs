using System.Collections.Generic;

namespace NEAT.Demo.SmartCarV2
{
    [System.Serializable]
    public class Level
    {
        public int xMax;
        public int yMax;
        public bool isCircuit;
        public bool spawnOnlyFirstInList;

        public List<int[]> path;
        public Dictionary<int, int> tileIndexes;


        public Level() { }

        public Level(int xMax, int yMax, bool isCircuit, bool spawnOnlyFirstInList)
        {
            this.xMax = xMax;
            this.yMax = yMax;
            this.isCircuit = isCircuit;
            this.spawnOnlyFirstInList = spawnOnlyFirstInList;
            tileIndexes = new Dictionary<int, int>();
        }
    }
}