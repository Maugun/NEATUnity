using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace NEAT
{
    public class SaveGenome
    {
        public const string GENOME_FOLDER = "Genomes";

        public void ToJson(Genome genome, InnovationCounter nodeInnovation, InnovationCounter connectInnovation, int generationNumber = 0)
        {
            genome.PrepareToJson(nodeInnovation, connectInnovation);
            string json = JsonConvert.SerializeObject(genome);
            string timestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            string path = $"{Application.persistentDataPath}/{GENOME_FOLDER}/genome_{generationNumber}_{timestamp}.json";

            if (File.Exists(path))
            {
                Debug.Log($"Genome already exists at {path}");
                return;
            }

            FileInfo file = new System.IO.FileInfo(path);
            file.Directory.Create();
            File.WriteAllText(path, json);
        }

        public Genome FromJson(string path)
        {
            Genome genome = JsonConvert.DeserializeObject<Genome>(File.ReadAllText(path));
            genome.InitFromJson();
            return genome;
        }

        public string[] GetAllPathFromDirectory()
        {
            return Directory.GetFiles($"{Application.persistentDataPath}/{GENOME_FOLDER}/", "*.json");
        }
    }
}
