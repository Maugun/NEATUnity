using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NEAT.Demo.Life2D
{
    public class RTNEATManager : MonoBehaviour
    {
        [Header("NEAT")]
        public RTNEATConfig _config;                                                            // Config
        public int _removeOldGenomeIntervale = 20;                                              // Remove Old Genome Frame

        [Header("Spawner")]
        public GameObject _ground;                                                              // Ground
        public float _spawnInterval = 0.1f;                                                     // Spawn Interval

        private List<NeuralNetwork> _nnToSpawn = null;                                          // Neural Network to Spawn
        private List<int> _currentGenomeIds = null;                                             // Current Genome Id List
        private Genome _startingGenome = null;                                                  // Starting Genome
        private EvaluatorRT _evaluator = null;                                                  // Evaluator
        private int _currentPopulation = 0;                                                     // Current Population
        public CameraMovements CameraMovements { get; set; }                                    // Camera Movements
        public List<Creature> _currentCreatures = null;
        public PlantSpawner _plantSpawner = null;

        private InnovationCounter _nodeInnovation = new InnovationCounter();                    // Node Innovation Counter
        private InnovationCounter _connectionInnovation = new InnovationCounter();              // Connection Innovation Counter

        private float _size;                                                                    // Spawn square size
        private int _inputNodeNumber = 9; //32
        private int _currentCreatureToFollow = 0;
        private float _clock = 0f;
        private float _clockSpawn = 0f;
        private int _generation = 0;

        void Start()
        {
            _size = _ground.transform.localScale.x - .5f;
            _nnToSpawn = new List<NeuralNetwork>();
            _currentGenomeIds = new List<int>();
            _currentCreatures = new List<Creature>();
            CameraMovements = Camera.main.GetComponent<CameraMovements>();
            CreateStartingGenomeAndEvaluator();
            CreateStartNeuralNetworkList();
        }

        void Update()
        {
            if (_nnToSpawn.Count > 0 &&
                _currentPopulation < _config.maxPopulation &&
                (_clockSpawn >= _spawnInterval))
            {
                Spawn();
                _clockSpawn = 0f;
            }
            else if (_nnToSpawn.Count == 0 && _currentPopulation < _config.minPopulation)
                AddNeuralNetworkToSpawn();

            if (_clock >= _removeOldGenomeIntervale)
            {
                _evaluator.RemoveOldGenomes(_currentGenomeIds);
                _generation++;
                Debug.LogFormat("Best Fitness: {0} | Current Population: {1} | Plant: {2} | Generation: {3}\n\nBest Genome:\n{4}", _evaluator.BestFitness, _currentPopulation, _plantSpawner.CurrentPop, _generation, _evaluator.BestGenome.ToString());
                _clock = 0f;
            }
            _clock += Time.deltaTime;
            _clockSpawn += Time.deltaTime;
        }

        private void AddNeuralNetworkToSpawn()
        {
            NeuralNetwork NN = new NeuralNetwork(_evaluator.AddNewGenome(), _config.activationType, _config.bias, _config.timeOut);
            _nnToSpawn.Add(NN);
            _currentGenomeIds.Add(NN.Genome.Id);
        }

        private void Spawn()
        {
            Vector2 spawnPosition = GetSpawnPosition();
            GameObject creature = Instantiate(_config.creaturePrefab, spawnPosition, Quaternion.identity, transform);
            Creature creatureScript = creature.GetComponent<Creature>();
            _currentCreatures.Add(creatureScript);
            creatureScript.NN = _nnToSpawn[0];
            _nnToSpawn.RemoveAt(0);
            creatureScript.Id = creatureScript.NN.Genome.Id;
            creatureScript.Manager = this;
            _currentPopulation++;
        }

        public void AddDeadCreature(Creature creature)
        {
            Genome genome = creature.NN.Genome;
            UpdateFitness(genome);
            _currentGenomeIds.Remove(genome.Id);
            _currentPopulation--;
            _currentCreatures.Remove(creature);
        }

        public void UpdateFitness(Genome genome)
        {
            _evaluator.GetGenomes().Find(o => o.Id == genome.Id).Fitness = genome.Fitness;
        }

        private Vector2 GetSpawnPosition()
        {
            // TODO Avoid to spawn in other Creature
            Vector2 randPos = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            return (randPos * _size / 2) + (Vector2)transform.position;
        }

        private void CreateStartNeuralNetworkList()
        {
            List<Genome> genomeList = _evaluator.GetGenomes();
            foreach (Genome genome in genomeList)
            {
                _nnToSpawn.Add(new NeuralNetwork(genome, _config.activationType, _config.bias, _config.timeOut));
                _currentGenomeIds.Add(genome.Id);
            }
        }

        private void CreateStartingGenomeAndEvaluator()
        {
            // Create New Genome
            Genome genome = new Genome(_config.newWeightRange, _config.perturbingProbability);

            // Check there is at least 1 Input & 1 Output Nodes
            if (_inputNodeNumber == 0)
                _inputNodeNumber = 1;
            if (_config.outputNodeNumber == 0)
                _config.outputNodeNumber = 1;

            // Create Input Nodes
            for (int i = 0; i < _inputNodeNumber; ++i)
                genome.AddNodeGene(new NodeGene(NodeGene.TYPE.INPUT, _nodeInnovation.GetNewInnovationNumber()));

            // Create Hidden Nodes
            foreach (int nb in _config.hiddenNodeStartNumberByLayer)
            {
                for (int i = 0; i < nb; ++i)
                    genome.AddNodeGene(new NodeGene(NodeGene.TYPE.HIDDEN, _nodeInnovation.GetNewInnovationNumber()));
            }

            // Create Output Nodes
            for (int i = 0; i < _config.outputNodeNumber; ++i)
                genome.AddNodeGene(new NodeGene(NodeGene.TYPE.OUTPUT, _nodeInnovation.GetNewInnovationNumber()));

            // Create Connections
            if (_config.addConnectionOnCreation)
            {
                if (_config.hiddenNodeStartNumberByLayer.Count > 0)
                {
                    int inputStartId = 0;
                    int previousHiddenStatId = 0;
                    int previousHiddenStopId = 0;
                    int hiddenStartId = _inputNodeNumber;
                    int hiddenStopId = _inputNodeNumber;
                    int outputStartId = genome.Nodes.Count - _config.outputNodeNumber;

                    // Loop through Hidden Layers
                    for (int hiddenLayer = 0; hiddenLayer < _config.hiddenNodeStartNumberByLayer.Count; ++hiddenLayer)
                    {
                        // Get Hidden Start & Stop Ids
                        if (hiddenLayer > 0)
                            hiddenStartId += _config.hiddenNodeStartNumberByLayer[hiddenLayer - 1];
                        hiddenStopId += _config.hiddenNodeStartNumberByLayer[hiddenLayer];

                        // Loop through Nodes of the current Layer
                        for (int hiddenNodeId = hiddenStartId; hiddenNodeId < hiddenStopId; ++hiddenNodeId)
                        {
                            // If current Hidden Layer is the first Layer
                            if (hiddenLayer == 0)
                            {
                                // Add Connections from Inputs to First Hidden Layer
                                for (int inputNodeId = inputStartId; inputNodeId < _inputNodeNumber; ++inputNodeId)
                                    genome.AddConnectionGene(new ConnectionGene(inputNodeId, hiddenNodeId, Random.Range(-0.5f, 0.5f), true, _connectionInnovation.GetNewInnovationNumber()));
                            }
                            else
                            {
                                // Add Connections from previous Hidden Layer to current Hidden Layer
                                for (int previousHiddenNodeId = previousHiddenStatId; previousHiddenNodeId < previousHiddenStopId; ++previousHiddenNodeId)
                                    genome.AddConnectionGene(new ConnectionGene(previousHiddenNodeId, hiddenNodeId, Random.Range(-0.5f, 0.5f), true, _connectionInnovation.GetNewInnovationNumber()));

                                // If current Hidden Layer is the last Layer 
                                if (hiddenLayer + 1 == _config.hiddenNodeStartNumberByLayer.Count)
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
                    int outputStartId = _inputNodeNumber;
                    int outputStopId = _inputNodeNumber + _config.outputNodeNumber;

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
            _evaluator = new EvaluatorRT(_config, genome, _nodeInnovation, _connectionInnovation);
        }

        public void FollowNextCreature()
        {
            _currentCreatureToFollow = _currentCreatureToFollow + 1 >= _currentCreatures.Count ? 0 : _currentCreatureToFollow + 1;
            _currentCreatures[_currentCreatureToFollow].FollowCreature();
        }

        public void FollowPreviousCreature()
        {
            _currentCreatureToFollow = _currentCreatureToFollow - 1 < 0 ? 0 : _currentCreatureToFollow - 1;
            _currentCreatures[_currentCreatureToFollow].FollowCreature();
        }

        public void FollowCurrentBestCreature()
        {
            List<Creature> orderedCreatureList = _currentCreatures.OrderByDescending(o => o.CurrentFitness).ToList();
            orderedCreatureList[0].FollowCreature();
        }
    }
}
