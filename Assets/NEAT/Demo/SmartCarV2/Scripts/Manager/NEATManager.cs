using System.Collections.Generic;
using UnityEngine;

namespace NEAT.Demo.SmartCarV2
{
    public class NEATManager : MonoBehaviour
    {
        [Header("NEAT")]
        public Transform spawn;                                                                 // Spawn
        public bool isMultiLevel = false;                                                       // Is Multi Level ?
        public bool start = false;                                                              // Start ?
        public bool saveBestGenome = false;                                                     // Save Best Genome to Json ?
        public bool autoSaveBestGenome = true;                                                  // Auto Save Best Genome to Json ?
        public LevelGenerator levelGenerator;                                                   // Level Generator
        public UIManager ui;                                                                    // UI
        public NEATConfig config;                                                               // Config

        public float GenerationTimer { get; set; }                                              // Timer for current generation

        private List<Transform> _creatureList;                                                  // Creatures List
        private CreatureEvaluator _evaluator = null;                                            // Evaluator
        private Genome _startingGenome = null;                                                  // Starting Genome
        private int _deadCreatureNumber;                                                        // Dead Creature Number
        private List<int> _deadIdList;                                                          // List of Dead Creature's ids
        private Quaternion _spawnRotation;                                                      // SpawnRotation

        private bool _isNewLevel;
        private int _currentLevel;

        private InnovationCounter _nodeInnovation = new InnovationCounter();                    // Node Innovation Counter
        private InnovationCounter _connectionInnovation = new InnovationCounter();              // Connection Innovation Counter

        private SaveManager _saveManager;
        private float _bestSavedFitness;
        private SaveData _saveData;

        private void Start()
        {
            _creatureList = new List<Transform>();
            _deadCreatureNumber = 0;
            _deadIdList = new List<int>();
            GenerationTimer = 0f;
            _saveManager = new SaveManager();
            _currentLevel = 1;
            _saveData = new SaveData();
            _isNewLevel = true;
            _bestSavedFitness = 0f;
        }

        private void Update()
        {
            GenerationTimer += Time.deltaTime;
            ui.UpdateTimer(GenerationTimer);

            // Delay Start Simulation to let time to all Components to load
            if (start)
            {
                InitSaveData();
                SetStartingGenomeAndEvaluator();
                SaveLevelToJson();
                SpawnCreatures();
                AddNNToCreatures();
                ui.UpdateCurrentGenerationNumber(1);
                StartSimulation();
                start = false;
            }
        }

        private void SpawnCreatures()
        {
            CreatureNeuralNetwork.BestNN = null;

            // Spawn Creatures
            for (int i = 0; i < config.populationSize; i++)
            {
                GameObject creature = (GameObject)Instantiate(config.creaturePrefab, spawn.position, _spawnRotation);
                creature.GetComponent<DemoCarController>().Init(DemoCarController.SimulationType.NEAT);
                creature.GetComponent<CreatureNeuralNetwork>().Reset(i);
                creature.SetActive(false);
                _creatureList.Add(creature.transform);
            }
        }

        private void SetStartingGenomeAndEvaluator()
        {
            _startingGenome = _startingGenome == null ? CreateStartingGenome() : _startingGenome;
            Debug.Log("Starting Genome:\n" + _startingGenome.ToString());
            SetEvaluator(_startingGenome);
        }

        private Genome CreateStartingGenome()
        {
            // Create New Genome
            Genome genome = new Genome(config.newWeightRange, config.perturbingProbability);

            // Check there is at least 1 Input & 1 Output Nodes
            if (config.inputNodeNumber == 0) config.inputNodeNumber = 1;
            if (config.outputNodeNumber == 0) config.outputNodeNumber = 1;

            // Create Input Nodes
            for (int i = 0; i < config.inputNodeNumber; i++)
                genome.AddNodeGene(new NodeGene(NodeGene.TYPE.INPUT, _nodeInnovation.GetNewInnovationNumber()));

            // Create Hidden Nodes
            foreach (int nb in config.hiddenNodeStartNumberByLayer)
            {
                for (int i = 0; i < nb; ++i)
                    genome.AddNodeGene(new NodeGene(NodeGene.TYPE.HIDDEN, _nodeInnovation.GetNewInnovationNumber()));
            }

            // Create Output Nodes
            for (int i = 0; i < config.outputNodeNumber; i++)
                genome.AddNodeGene(new NodeGene(NodeGene.TYPE.OUTPUT, _nodeInnovation.GetNewInnovationNumber()));

            // Create Connections
            if (config.addConnectionOnCreation)
            {
                if (config.hiddenNodeStartNumberByLayer.Count > 0)
                {
                    int inputStartId = 0;
                    int previousHiddenStatId = 0;
                    int previousHiddenStopId = 0;
                    int hiddenStartId = config.inputNodeNumber;
                    int hiddenStopId = config.inputNodeNumber;
                    int outputStartId = genome.Nodes.Count - config.outputNodeNumber;

                    // Loop through Hidden Layers
                    for (int hiddenLayer = 0; hiddenLayer < config.hiddenNodeStartNumberByLayer.Count; hiddenLayer++)
                    {
                        // Get Hidden Start & Stop Ids
                        if (hiddenLayer > 0) hiddenStartId += config.hiddenNodeStartNumberByLayer[hiddenLayer - 1];
                        hiddenStopId += config.hiddenNodeStartNumberByLayer[hiddenLayer];

                        // Loop through Nodes of the current Layer
                        for (int hiddenNodeId = hiddenStartId; hiddenNodeId < hiddenStopId; ++hiddenNodeId)
                        {
                            // If current Hidden Layer is the first Layer
                            if (hiddenLayer == 0)
                            {
                                // Add Connections from Inputs to First Hidden Layer
                                for (int inputNodeId = inputStartId; inputNodeId < config.inputNodeNumber; ++inputNodeId)
                                    genome.AddConnectionGene(new ConnectionGene(inputNodeId, hiddenNodeId, Random.Range(config.newWeightRange.x, config.newWeightRange.y), true, _connectionInnovation.GetNewInnovationNumber()));
                            }
                            else
                            {
                                // Add Connections from previous Hidden Layer to current Hidden Layer
                                for (int previousHiddenNodeId = previousHiddenStatId; previousHiddenNodeId < previousHiddenStopId; ++previousHiddenNodeId)
                                    genome.AddConnectionGene(new ConnectionGene(previousHiddenNodeId, hiddenNodeId, Random.Range(config.newWeightRange.x, config.newWeightRange.y), true, _connectionInnovation.GetNewInnovationNumber()));

                                // If current Hidden Layer is the last Layer 
                                if (hiddenLayer + 1 == config.hiddenNodeStartNumberByLayer.Count)
                                {
                                    // Add Connections from Last Hidden Layer to Outputs
                                    for (int outputNodeId = outputStartId; outputNodeId < genome.Nodes.Count; ++outputNodeId)
                                        genome.AddConnectionGene(new ConnectionGene(hiddenNodeId, outputNodeId, Random.Range(config.newWeightRange.x, config.newWeightRange.y), true, _connectionInnovation.GetNewInnovationNumber()));
                                }
                            }
                        }

                        // Save previous Hidden Layer Start & Stop Ids
                        previousHiddenStatId = hiddenStartId;
                        previousHiddenStopId = hiddenStopId;
                    }
                }
                else
                {
                    int outputStartId = config.inputNodeNumber;
                    int outputStopId = config.inputNodeNumber + config.outputNodeNumber;

                    // Loop Input Node
                    for (int inputNodeId = 0; inputNodeId < outputStartId; ++inputNodeId)
                    {
                        // Loop Output Node & add Connection between Input Node & Hidden Node
                        for (int outputNodeId = outputStartId; outputNodeId < outputStopId; ++outputNodeId)
                            genome.AddConnectionGene(new ConnectionGene(inputNodeId, outputNodeId, Random.Range(config.newWeightRange.x, config.newWeightRange.y), true, _connectionInnovation.GetNewInnovationNumber()));
                    }
                }
            }

            return genome;
        }

        private void SetEvaluator(Genome genome)
        {
            _evaluator = new CreatureEvaluator(config, genome, _nodeInnovation, _connectionInnovation);
        }

        private void AddNNToCreatures()
        {
            List<Genome> genomeList = _evaluator.GetGenomes();
            for (int i = 0; i < genomeList.Count; i++)
                _creatureList[i].GetComponent<CreatureNeuralNetwork>().NeuralNetwork = new NeuralNetwork(genomeList[i], config.activationType, config.bias, config.timeOut);
        }

        private void StartNewGeneration()
        {
            ResetDeadCreatures();
            GetGenomeFitnessFromCreatureNN();
            EvaluateCurrentGeneration();
            SaveGenomeToJson();
            LoadNextLevel();
            UpdateUI();
            ResetCreatures();
            AddNNToCreatures();
            StartSimulation();
        }

        private void EvaluateCurrentGeneration()
        {
            _evaluator.Evaluate();
        }

        private void GetGenomeFitnessFromCreatureNN()
        {
            foreach (Transform creature in _creatureList)
            {
                NeuralNetwork creatureNN = creature.GetComponent<CreatureNeuralNetwork>().NeuralNetwork;
                _evaluator.GetGenomes().Find(o => o.Id == creatureNN.Genome.Id).Fitness = creatureNN.Fitness;
            }
        }

        private void UpdateUI()
        {
            NeuralNetwork bestNN = new NeuralNetwork(_evaluator.BestGenome, config.activationType, config.bias, config.timeOut);
            Debug.Log(_evaluator.GetGenerationLogs() + "\nBest Neural Network:\n" + bestNN.ToString());
            ui.UpdateGenerationLog(string.Format(
                "Generation: {0} | Fitness: {1} | Checkpoints : {2} | Time: {3}",
                _evaluator.GenerationNumber,
                _evaluator.BestFitness,
                CreatureNeuralNetwork.BestNN.CheckpointPassed,
                CreatureNeuralNetwork.BestNN.Time
            ));
            ui.UpdateBrainGraph(bestNN);
            ui.UpdateCurrentGenerationNumber(_evaluator.GenerationNumber + 1);
        }

        private void StartSimulation()
        {
            foreach (Transform creature in _creatureList)
            {
                creature.GetComponent<CreatureNeuralNetwork>().IsInit = true;
                creature.gameObject.SetActive(true);
            }
            GenerationTimer = 0f;
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

        private void ResetDeadCreatures()
        {
            _deadCreatureNumber = 0;
            _deadIdList.Clear();
        }

        public void AddDeadCreature(int id)
        {
            if (_deadIdList.Contains(id)) // Protection
            {
                // Debug.LogWarningFormat("Creature {0} hit the wall multiple times !", id);
                return;
            }

            _deadIdList.Add(id);
            _deadCreatureNumber++;
            if (_deadCreatureNumber == config.populationSize)
            {
                // Debug.Log("TOTAL DEATH = " + _deadCreatureNumber);
                StartNewGeneration();
            }
        }

        private void LoadNextLevel()
        {
            _isNewLevel = false;
            if (!isMultiLevel || CreatureNeuralNetwork.BestNN.CheckpointPassed < Checkpoint.totalCp) return;

            ui.GenerateNextLevel();
            _currentLevel++;
            SaveLevelToJson();
            _isNewLevel = true;
            _bestSavedFitness = 0f;
        }

        public void SetSpawnRotation(Quaternion spawnRotation)
        {
            _spawnRotation = spawnRotation;
        }

        #region Save
        private void SaveGenomeToJson()
        {
            if (!saveBestGenome && !autoSaveBestGenome) return;
            if (!saveBestGenome && autoSaveBestGenome && _evaluator.BestFitness <= _bestSavedFitness && !_isNewLevel) return;
            _bestSavedFitness = _evaluator.BestFitness;
            _saveManager.GenomeToJson(_evaluator.BestGenome, _nodeInnovation, _connectionInnovation, _evaluator.GenerationNumber, _currentLevel);
        }

        // public void LoadGenomeFromJson()
        // {
        //     string[] paths = _saveManager.GetAllPathFromDirectory();
        //     _startingGenome = _saveManager.GenomeFromJson(paths[0]);
        //     _nodeInnovation.CurrentInnovation = _startingGenome._nodeInnovation;
        //     _connectionInnovation.CurrentInnovation = _startingGenome._connectionInnovation;
        // }

        public void SaveLevelToJson()
        {
            _saveManager.LevelToJson(levelGenerator.GetCurrentLevel(), _currentLevel);
            _saveData.levelSwitch.Add(_evaluator.GenerationNumber);
            _saveManager.SaveDataToJson(_saveData);
        }

        public void InitSaveData()
        {
            _saveData.activationType = (int)config.activationType;
            _saveData.bias = config.bias;
            _saveData.timeOut = config.timeOut;
        }

        public void IsTimeAttack(bool isTimeAttack)
        {
            _saveData.isTimeAttack = isTimeAttack;
        }
        #endregion

        public class CreatureEvaluator : Evaluator
        {
            public CreatureEvaluator(
                NEATConfig config,
                Genome startingGenome,
                InnovationCounter nodeInnovation,
                InnovationCounter connectionInnovation
                ) : base(config, startingGenome, nodeInnovation, connectionInnovation) { }

            protected override float EvaluateGenome(Genome genome) { return genome.Fitness; }
        }
    }
}
