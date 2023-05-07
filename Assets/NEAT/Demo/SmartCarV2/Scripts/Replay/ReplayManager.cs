using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEAT.Demo.SmartCarV2
{
    public class ReplayManager : MonoBehaviour
    {
        public UIManager ui;
        public LevelGenerator levelGenerator;
        public CameraController cameraController;
        public NEATManager neatManager;
        public Transform spawn;

        public float GenerationTimer { get; set; }

        private SaveManager _saveManager;
        private Save _save;

        private List<Transform> _creatureList;
        private Quaternion _spawnRotation;
        private GameObject _creaturePrefab;
        private int _deadCreatureNumber;
        private List<int> _deadIdList;

        private int _generationNumber;
        private int _replayGenerationNumber;
        private int _currentLevelNumber;

        private NeuralNetwork _currentNN;

        void Start()
        {
            _save = null;
            _creatureList = new List<Transform>();
            GenerationTimer = 0f;
            _saveManager = new SaveManager();
            _creaturePrefab = neatManager.config.creaturePrefab;
            _generationNumber = 0;
            _deadCreatureNumber = 0;
            _deadIdList = new List<int>();
            _replayGenerationNumber = 0;
            _currentLevelNumber = 0;
            _currentNN = null;
        }

        void Update()
        {
            GenerationTimer += Time.deltaTime;
            ui.UpdateTimer(GenerationTimer);
        }

        public void StartReplay(string saveId)
        {
            _save = _saveManager.GetSaveFromSaveId(saveId);
            Debug.Log("Levels: " + _save.levels.Length);
            Debug.Log("Genomes: " + _save.genomes.Length);
            _creaturePrefab.GetComponent<DemoCarController>().timeAttack = _save.data.isTimeAttack;
            GetGenerationNumber();
            LoadLevel(_save.levels[_currentLevelNumber]);
            SpawnCreature();
            AddNNToCreature();
            StartReplay();
        }

        private void GetGenerationNumber()
        {
            int index = _replayGenerationNumber >= _save.generationNumbers.Length - 1 ? _save.generationNumbers.Length - 1 : _replayGenerationNumber;
            _generationNumber = _save.generationNumbers[index];
            ui.UpdateCurrentGenerationNumber(_generationNumber);
        }

        private void LoadLevel(Level level)
        {
            levelGenerator.LoadLevel(level);
            spawn = levelGenerator.Spawn;
            SetSpawnRotation(levelGenerator.SpawnRotation);
            cameraController.CenterCameraOnMap(levelGenerator.width - 1, levelGenerator.height - 1, levelGenerator.tileSize);
        }

        private void SpawnCreature()
        {
            CreatureNeuralNetwork.BestNN = null;
            GameObject creature = (GameObject)Instantiate(_creaturePrefab, spawn.position, _spawnRotation);
            creature.GetComponent<DemoCarController>().Init(DemoCarController.SimulationType.Replay);
            creature.GetComponent<CreatureNeuralNetwork>().Reset(0);
            creature.SetActive(false);
            _creatureList.Add(creature.transform);
        }

        private void AddNNToCreature()
        {
            int index = _replayGenerationNumber >= _save.genomes.Length - 1 ? _save.genomes.Length - 1 : _replayGenerationNumber;
            _currentNN = new NeuralNetwork(_save.genomes[index], (NEAT.Neuron.ActivationType)_save.data.activationType, _save.data.bias, _save.data.timeOut);
            _creatureList[0].GetComponent<CreatureNeuralNetwork>().NeuralNetwork = _currentNN;
        }

        private void StartReplay()
        {
            foreach (Transform creature in _creatureList)
            {
                creature.GetComponent<CreatureNeuralNetwork>().IsInit = true;
                creature.gameObject.SetActive(true);
            }
            GenerationTimer = 0f;
        }

        private void StartNextGeneration()
        {
            _replayGenerationNumber++;
            UpdateUI();
            GetGenerationNumber();
            ResetDeadCreatures();
            LoadNextLevel();
            ResetCreatures();
            AddNNToCreature();
            StartReplay();
        }

        private void LoadNextLevel()
        {
            if (_currentLevelNumber == _save.data.levelSwitch.Count - 1) return;
            if (_generationNumber <= _save.data.levelSwitch[_currentLevelNumber + 1]) return;
            _currentLevelNumber++;
            LoadLevel(_save.levels[_currentLevelNumber]);
            ui.IncreaseLevelNumber();
            ui.UpdateLevelUI();
        }

        private void UpdateUI()
        {
            ui.UpdateGenerationLog(string.Format(
                "Generation: {0} | Fitness: {1} | Checkpoints : {2} | Time: {3}",
                _generationNumber,
               _currentNN.Fitness,
                CreatureNeuralNetwork.BestNN.CheckpointPassed,
                CreatureNeuralNetwork.BestNN.Time
            ));
            ui.UpdateBrainGraph(_currentNN);
        }

        private void ResetCreatures()
        {
            CreatureNeuralNetwork.BestNN = null;

            for (int i = 0; i < _creatureList.Count; i++)
            {
                Transform creature = _creatureList[i];
                creature.GetComponent<CreatureNeuralNetwork>().Reset(i);
                creature.GetComponent<DemoCarController>().Reset();
                creature.position = spawn.position;
                creature.rotation = _spawnRotation;
                creature.gameObject.SetActive(false);
            }
        }

        public void AddDeadCreature(int id)
        {
            if (_deadIdList.Contains(id)) return;

            _deadIdList.Add(id);
            _deadCreatureNumber++;
            if (_deadCreatureNumber == _creatureList.Count) StartNextGeneration();
        }

        private void ResetDeadCreatures()
        {
            _deadCreatureNumber = 0;
            _deadIdList.Clear();
        }

        public void SetSpawnRotation(Quaternion spawnRotation)
        {
            _spawnRotation = spawnRotation;
        }
    }
}
