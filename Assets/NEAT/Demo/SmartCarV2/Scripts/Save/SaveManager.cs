using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

namespace NEAT.Demo.SmartCarV2
{
    public class SaveManager
    {
        public const string SAVE_FOLDER = "Saves";
        public const string GENOME_FOLDER = "Genomes";
        public const string LEVEL_FOLDER = "Levels";

        private string currentSave;

        public SaveManager()
        {
            currentSave = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        }

        #region JSON Genome
        public void GenomeToJson(Genome genome, InnovationCounter nodeInnovation, InnovationCounter connectInnovation, int generationNumber = 0, int levelNumber = 0)
        {
            genome.PrepareToJson(nodeInnovation, connectInnovation);
            string json = JsonConvert.SerializeObject(genome);
            string path = $"{Application.persistentDataPath}/{SAVE_FOLDER}/{currentSave}/{GENOME_FOLDER}/{currentSave}-{levelNumber}-{generationNumber}.json";

            if (File.Exists(path))
            {
                Debug.Log($"Genome already exists at {path}");
                return;
            }

            FileInfo file = new System.IO.FileInfo(path);
            file.Directory.Create();
            File.WriteAllText(path, json);
        }

        public Genome GenomeFromJson(string path)
        {
            Genome genome = JsonConvert.DeserializeObject<Genome>(File.ReadAllText(path));
            genome.InitFromJson();
            return genome;
        }
        #endregion

        #region JSON Level
        public void LevelToJson(Level level, int levelNumber = 0)
        {
            string json = JsonConvert.SerializeObject(level);
            string path = $"{Application.persistentDataPath}/{SAVE_FOLDER}/{currentSave}/{LEVEL_FOLDER}/{currentSave}-{levelNumber}.json";

            if (File.Exists(path))
            {
                Debug.Log($"Level already exists at {path}");
                return;
            }

            FileInfo file = new System.IO.FileInfo(path);
            file.Directory.Create();
            File.WriteAllText(path, json);
        }

        public Level LevelFromJson(string path)
        {
            return JsonConvert.DeserializeObject<Level>(File.ReadAllText(path));
        }
        #endregion

        #region JSON SaveData
        public void SaveDataToJson(SaveData data)
        {
            string json = JsonConvert.SerializeObject(data);
            string path = $"{Application.persistentDataPath}/{SAVE_FOLDER}/{currentSave}/{currentSave}-data.json";
            FileInfo file = new System.IO.FileInfo(path);
            file.Directory.Create();
            File.WriteAllText(path, json);
        }

        public SaveData SaveDataFromJson(string path)
        {
            return JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(path));
        }
        #endregion

        #region Saves
        public static string[] GetAllSaveIds()
        {
            string[] paths = Directory.GetDirectories($"{Application.persistentDataPath}/{SAVE_FOLDER}/");
            string[] ids = new string[paths.Length];

            for (int i = 0; i < paths.Length; i++)
            {
                string[] parts = paths[i].Split('/');
                string id = parts[parts.Length - 1];
                ids[i] = id;
            }
            return ids;
        }

        public int[] GetGenerationNumbers(string saveId)
        {
            string[] paths = Directory.GetFiles($"{Application.persistentDataPath}/{SAVE_FOLDER}/{saveId}/{GENOME_FOLDER}/", "*.json");
            Array.Sort(paths);
            List<int> generationNumbers = new List<int>();

            foreach (string path in paths)
            {
                string[] parts = path.Split('/');
                string[] fileNameParts = parts[parts.Length - 1].Split('-');
                string generationNumberString = fileNameParts[fileNameParts.Length - 1].Replace(".json", "");
                generationNumbers.Add(int.Parse(generationNumberString));
            }

            int[] result = generationNumbers.ToArray();
            Array.Sort(result);

            return result;
        }

        public Genome[] GetAllGenomesFromSaveId(string saveId)
        {
            string[] paths = Directory.GetFiles($"{Application.persistentDataPath}/{SAVE_FOLDER}/{saveId}/{GENOME_FOLDER}/", "*.json");
            paths = paths.OrderBy(s => int.Parse(s.Split('-')[2].Replace(".json", ""))).ToArray();

            Genome[] genomes = new Genome[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                genomes[i] = GenomeFromJson(paths[i]);
            }
            return genomes;
        }

        public Level[] GetAllLevelsFromSaveId(string saveId)
        {
            string[] paths = Directory.GetFiles($"{Application.persistentDataPath}/{SAVE_FOLDER}/{saveId}/{LEVEL_FOLDER}/", "*.json");
            paths = paths.OrderBy(s => int.Parse(s.Split('-')[1].Replace(".json", ""))).ToArray();

            Level[] levels = new Level[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                levels[i] = LevelFromJson(paths[i]);
            }
            return levels;
        }

        public SaveData GetSaveDataFromSaveId(string saveId)
        {
            string[] paths = Directory.GetFiles($"{Application.persistentDataPath}/{SAVE_FOLDER}/{saveId}/", $"{saveId}-data.json");
            return SaveDataFromJson(paths[0]);
        }

        public Save GetSaveFromSaveId(string saveId)
        {
            Level[] levels = GetAllLevelsFromSaveId(saveId);
            Genome[] genomes = GetAllGenomesFromSaveId(saveId);
            SaveData data = GetSaveDataFromSaveId(saveId);
            int[] generationNumbers = GetGenerationNumbers(saveId);

            return new Save(saveId, genomes, levels, data, generationNumbers);
        }

    }
    #endregion
}

