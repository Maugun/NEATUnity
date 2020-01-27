using NEAT.Demo.SmartCarV2;
using System.Collections.Generic;
using UnityEngine;

namespace NEAT
{
    public class NEATManager : MonoBehaviour
    {
        [Header("NEAT")]
        public Transform _spawn;                                                                // Spawn
        public bool _generateLevel = false;                                                     // Generate Level
        public bool _start = false;                                                             // Start ?
        public LevelGenerator _levelGenerator;                                                  // Level Generator
        public CameraController _cameraController;                                              // Camera Controller
        public GenerateLevelUI _ui;                                                             // UI
        public NEATConfig _config;                                                              // Config

        private List<Transform> _creatureList;                                                  // Creatures List
        private CreatureEvaluator _evaluator = null;                                            // Evaluator
        private Genome _startingGenome = null;                                                  // Starting Genome
        private int _deadCreatureNumber;                                                        // Dead Creature Number
        private List<int> _deadIdList;                                                          // List of Dead Creature's ids
        private int _generationNumber;                                                          // Generation Number
        private Quaternion _spawnRotation;                                                      // SpawnRotation

        private InnovationCounter _nodeInnovation = new InnovationCounter();                    // Node Innovation Counter
        private InnovationCounter _connectionInnovation = new InnovationCounter();              // Connection Innovation Counter

        private void Start()
        {
            _creatureList = new List<Transform>();
            _deadCreatureNumber = 0;
            _deadIdList = new List<int>();
            _generationNumber = 1;
        }

        private void Update()
        {
            // Delay Start Simulation to let time to all Components to load
            if (_start)
            {
                // Generate Level
                if (_generateLevel)
                {
                    _levelGenerator.GenerateLevel();
                    _spawn = _levelGenerator.Spawn;
                    _spawnRotation = _levelGenerator.SpawnRotation;
                    _cameraController.CenterCameraOnMap(_levelGenerator._width - 1, _levelGenerator._height - 1, _levelGenerator._tileSize);
                }

                CreateStartingGenomeAndEvaluator();
                SpawnCreatures();
                CreateNeuralNetworks();
                StartSimulation();
                _start = false;
            }
        }

        private void SpawnCreatures()
        {
            // Spawn Creatures
            for (int i = 0; i < _config._populationSize; ++i)
            {
                GameObject creature = (GameObject)Instantiate(_config._creaturePrefab, _spawn.position, _spawnRotation);
                creature.GetComponent<CreatureNeuralNetwork>().IsInit = false;
                creature.GetComponent<CreatureNeuralNetwork>().Id = i;
                creature.SetActive(false);
                _creatureList.Add(creature.transform);
            }
        }

        private void CreateStartingGenomeAndEvaluator()
        {
            // Create New Genome
            Genome genome = new Genome(_config._newWeightRange, _config._perturbingProbability);

            // Check there is at least 1 Input & 1 Output Nodes
            if (_config._inputNodeNumber == 0)
                _config._inputNodeNumber = 1;
            if (_config._outputNodeNumber == 0)
                _config._outputNodeNumber = 1;

            // Create Input Nodes
            for (int i = 0; i < _config._inputNodeNumber; ++i)
                genome.AddNodeGene(new NodeGene(NodeGene.TYPE.INPUT, _nodeInnovation.GetNewInnovationNumber()));

            // Create Hidden Nodes
            foreach (int nb in _config._hiddenNodeStartNumberByLayer)
            {
                for (int i = 0; i < nb; ++i)
                    genome.AddNodeGene(new NodeGene(NodeGene.TYPE.HIDDEN, _nodeInnovation.GetNewInnovationNumber()));
            }
            
            // Create Output Nodes
            for (int i = 0; i < _config._outputNodeNumber; ++i)
                genome.AddNodeGene(new NodeGene(NodeGene.TYPE.OUTPUT, _nodeInnovation.GetNewInnovationNumber()));

            // Create Connections
            if (_config._addConnectionOnCreation)
            {
                if (_config._hiddenNodeStartNumberByLayer.Count > 0)
                {
                    int inputStartId = 0;
                    int previousHiddenStatId = 0;
                    int previousHiddenStopId = 0;
                    int hiddenStartId = _config._inputNodeNumber;
                    int hiddenStopId = _config._inputNodeNumber;
                    int outputStartId = genome.Nodes.Count - _config._outputNodeNumber;

                    // Loop through Hidden Layers
                    for (int hiddenLayer = 0; hiddenLayer < _config._hiddenNodeStartNumberByLayer.Count; ++hiddenLayer)
                    {
                        // Get Hidden Start & Stop Ids
                        if (hiddenLayer > 0)
                            hiddenStartId += _config._hiddenNodeStartNumberByLayer[hiddenLayer - 1];
                        hiddenStopId += _config._hiddenNodeStartNumberByLayer[hiddenLayer];

                        // Loop through Nodes of the current Layer
                        for (int hiddenNodeId = hiddenStartId; hiddenNodeId < hiddenStopId; ++hiddenNodeId)
                        {
                            // If current Hidden Layer is the first Layer
                            if (hiddenLayer == 0)
                            {
                                // Add Connections from Inputs to First Hidden Layer
                                for (int inputNodeId = inputStartId; inputNodeId < _config._inputNodeNumber; ++inputNodeId)
                                    genome.AddConnectionGene(new ConnectionGene(inputNodeId, hiddenNodeId, Random.Range(-0.5f, 0.5f), true, _connectionInnovation.GetNewInnovationNumber()));
                            }
                            else
                            {
                                // Add Connections from previous Hidden Layer to current Hidden Layer
                                for (int previousHiddenNodeId = previousHiddenStatId; previousHiddenNodeId < previousHiddenStopId; ++previousHiddenNodeId)
                                    genome.AddConnectionGene(new ConnectionGene(previousHiddenNodeId, hiddenNodeId, Random.Range(-0.5f, 0.5f), true, _connectionInnovation.GetNewInnovationNumber()));

                                // If current Hidden Layer is the last Layer 
                                if (hiddenLayer + 1 == _config._hiddenNodeStartNumberByLayer.Count)
                                {
                                    // Add Connections from Last Hidden Layer to Outputs
                                    for (int outputNodeId = outputStartId; outputNodeId < genome.Nodes.Count; ++outputNodeId)
                                        genome.AddConnectionGene(new ConnectionGene(hiddenNodeId, outputNodeId, Random.Range(-0.5f, 0.5f), true, _connectionInnovation.GetNewInnovationNumber()));
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
                    int outputStartId = _config._inputNodeNumber;
                    int outputStopId = _config._inputNodeNumber + _config._outputNodeNumber;

                    // Loop Input Node
                    for (int inputNodeId = 0; inputNodeId < outputStartId; ++inputNodeId)
                    {
                        // Loop Output Node & add Connection between Input Node & Hidden Node
                        for (int outputNodeId = outputStartId; outputNodeId < outputStopId; ++outputNodeId)
                            genome.AddConnectionGene(new ConnectionGene(inputNodeId, outputNodeId, Random.Range(-1f, 2f), true, _connectionInnovation.GetNewInnovationNumber()));
                    }
                }
            }
            
            _startingGenome = genome;
            Debug.Log("Starting Genome:\n" + genome.ToString());

            // Create Evaluator
            _evaluator = new CreatureEvaluator(_config, genome, _nodeInnovation, _connectionInnovation);
        }

        private void CreateNeuralNetworks()
        {
            List<Genome> genomeList = _evaluator.GetGenomes();
            for (int i = 0; i < genomeList.Count; ++i)
                _creatureList[i].GetComponent<CreatureNeuralNetwork>().NeuralNetwork = new NeuralNetwork(genomeList[i], _config._activationType, _config._bias, _config._timeOut);
        }

        private void StartNewGeneration()
        {
            // Reset Dead Creature Number
            _deadCreatureNumber = 0;
            _deadIdList.Clear();

            // Get Genome Fitness from Creature NN
            GetGenomeFitness();

            // Evaluate Generation
            _evaluator.Evaluate();

            // Logs
            NeuralNetwork bestNN = new NeuralNetwork(_evaluator.BestGenome, _config._activationType, _config._bias, _config._timeOut);
            Debug.Log(_evaluator.GetGenerationLogs() + "\nBest Neural Network:\n" + bestNN.ToString());
            _ui.UpdateGenerationLog("Generation nb: " + _evaluator.GenerationNumber + " | Best Fitness: " + _evaluator.BestFitness);

            // Reset Creatures
            ResetCreatures();

            // Add NN to Creatures
            CreateNeuralNetworks();

            // Update Generation Number
            _generationNumber = _evaluator.GenerationNumber;

            // Start Simulation
            StartSimulation();
        }

        private void GetGenomeFitness()
        {
            foreach (Transform creature in _creatureList)
            {
                NeuralNetwork creatureNN = creature.GetComponent<CreatureNeuralNetwork>().NeuralNetwork;
                _evaluator.GetGenomes().Find(o => o.Id == creatureNN.Genome.Id).Fitness = creatureNN.Fitness;
            }
        }

        private void StartSimulation()
        {
            foreach (Transform creature in _creatureList)
            {
                creature.GetComponent<CreatureNeuralNetwork>().IsInit = true;
                creature.gameObject.SetActive(true);
            }
        }

        private void ResetCreatures()
        {
            foreach (Transform creature in _creatureList)
            {
                Destroy(creature.gameObject);
            }
            _creatureList.Clear();
            SpawnCreatures();
        }

        public void AddDeadCreature(int id)
        {
            if (!_deadIdList.Contains(id))                  // Protection
            {
                _deadIdList.Add(id);
                _deadCreatureNumber++;
                if (_deadCreatureNumber == _config._populationSize)
                {
                    //Debug.Log("TOTAL DEATH = " + _deadCreatureNumber);
                    StartNewGeneration();
                }
            }
            else
                Debug.LogWarningFormat("Creature {0} hit the wall multiple times !", id);
        }

        public class CreatureEvaluator : Evaluator
        {
            public CreatureEvaluator(
                NEATConfig config,
                Genome startingGenome,
                InnovationCounter nodeInnovation,
                InnovationCounter connectionInnovation
                ) : base(config, startingGenome, nodeInnovation, connectionInnovation) { }

            protected override float EvaluateGenome(Genome genome)
            {
                return genome.Fitness;
            }
        }

        public void SetSpawnRotation(Quaternion spawnRotation)
        {
            _spawnRotation = spawnRotation;
        }
    }
}
