using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEAT.Demo.SmartCarV2
{
    public class Save
    {
        public string id;
        public Level[] levels;
        public Genome[] genomes;
        public SaveData data;
        public int[] generationNumbers;

        public Save(string id, Genome[] genomes, Level[] levels, SaveData data, int[] generationNumbers)
        {
            this.id = id;
            this.genomes = genomes;
            this.levels = levels;
            this.data = data;
            this.generationNumbers = generationNumbers;
        }
    }

    [System.Serializable]
    public class SaveData
    {
        public List<int> levelSwitch;
        public bool isTimeAttack;
        public int activationType;
        public bool bias;
        public int timeOut;

        public SaveData()
        {
            levelSwitch = new List<int>();
        }
    }
}
