using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEAT.Demo.PreyVsPredator
{
    public class CreatureManager : MonoBehaviour
    {
        public enum CreatureType
        {
            PREY,
            PREDATOR
        }

        [Header("NEAT")]
        public RTNEATConfig config;
        [SerializeField] private int _inputNodeNumber = 25;

        [Header("Creature")]
        [SerializeField] private GameObject _creaturePrefab;

        [Header("Map")]
        [SerializeField] private int _size = 10;

        private bool IsInit;
        private InnovationCounter _nodeInnovation = new InnovationCounter();
        private InnovationCounter _connectionInnovation = new InnovationCounter();
        private Genome _startingGenome;
        private EvaluatorRT _evaluator;

        private List<GameObject> _inactiveCreatureList;
        private List<GameObject> _activeCreatureList;

        private void Awake()
        {
            _inactiveCreatureList = new List<GameObject>();
            _activeCreatureList = new List<GameObject>();
            IsInit = false;
            _startingGenome = null;
            _evaluator = null;
            InitSimulation();
        }

        private void Update()
        {
            //if (!IsInit) return;
        }

        private void InitSimulation()
        {
            CreateStartingGenomeAndEvaluator();
            GenerateInactiveCreatureList();
            GenerateActiveCreatureList();
        }

        private void CreateStartingGenomeAndEvaluator()
        {
            // Create New Genome
            Genome genome = new Genome(config.newWeightRange, config.perturbingProbability);

            // Check there is at least 1 Input & 1 Output Nodes
            if (_inputNodeNumber == 0)
                _inputNodeNumber = 1;
            if (config.outputNodeNumber == 0)
                config.outputNodeNumber = 1;

            // Create Input Nodes
            for (int i = 0; i < _inputNodeNumber; ++i)
                genome.AddNodeGene(new NodeGene(NodeGene.TYPE.INPUT, _nodeInnovation.GetNewInnovationNumber()));

            // Create Hidden Nodes
            foreach (int nb in config.hiddenNodeStartNumberByLayer)
            {
                for (int i = 0; i < nb; ++i)
                    genome.AddNodeGene(new NodeGene(NodeGene.TYPE.HIDDEN, _nodeInnovation.GetNewInnovationNumber()));
            }

            // Create Output Nodes
            for (int i = 0; i < config.outputNodeNumber; ++i)
                genome.AddNodeGene(new NodeGene(NodeGene.TYPE.OUTPUT, _nodeInnovation.GetNewInnovationNumber()));

            // Create Connections
            if (config.addConnectionOnCreation)
            {
                if (config.hiddenNodeStartNumberByLayer.Count > 0)
                {
                    int inputStartId = 0;
                    int previousHiddenStatId = 0;
                    int previousHiddenStopId = 0;
                    int hiddenStartId = _inputNodeNumber;
                    int hiddenStopId = _inputNodeNumber;
                    int outputStartId = genome.Nodes.Count - config.outputNodeNumber;

                    // Loop through Hidden Layers
                    for (int hiddenLayer = 0; hiddenLayer < config.hiddenNodeStartNumberByLayer.Count; ++hiddenLayer)
                    {
                        // Get Hidden Start & Stop Ids
                        if (hiddenLayer > 0)
                            hiddenStartId += config.hiddenNodeStartNumberByLayer[hiddenLayer - 1];
                        hiddenStopId += config.hiddenNodeStartNumberByLayer[hiddenLayer];

                        // Loop through Nodes of the current Layer
                        for (int hiddenNodeId = hiddenStartId; hiddenNodeId < hiddenStopId; hiddenNodeId++)
                        {
                            // If current Hidden Layer is the first Layer
                            if (hiddenLayer == 0)
                            {
                                // Add Connections from Inputs to First Hidden Layer
                                for (int inputNodeId = inputStartId; inputNodeId < _inputNodeNumber; inputNodeId++)
                                    genome.AddConnectionGene(new ConnectionGene(inputNodeId, hiddenNodeId, Random.Range(config.newWeightRange.x, config.newWeightRange.y), true, _connectionInnovation.GetNewInnovationNumber()));
                            }
                            else
                            {
                                // Add Connections from previous Hidden Layer to current Hidden Layer
                                for (int previousHiddenNodeId = previousHiddenStatId; previousHiddenNodeId < previousHiddenStopId; previousHiddenNodeId++)
                                    genome.AddConnectionGene(new ConnectionGene(previousHiddenNodeId, hiddenNodeId, Random.Range(config.newWeightRange.x, config.newWeightRange.y), true, _connectionInnovation.GetNewInnovationNumber()));

                                // If current Hidden Layer is the last Layer 
                                if (hiddenLayer + 1 == config.hiddenNodeStartNumberByLayer.Count)
                                {
                                    // Add Connections from Last Hidden Layer to Outputs
                                    for (int outputNodeId = outputStartId; outputNodeId < genome.Nodes.Count; outputNodeId++)
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
                    int outputStartId = _inputNodeNumber;
                    int outputStopId = _inputNodeNumber + config.outputNodeNumber;

                    // Loop Input Node
                    for (int inputNodeId = 0; inputNodeId < outputStartId; inputNodeId++)
                    {
                        // Loop Output Node & add Connection between Input Node & Hidden Node
                        for (int outputNodeId = outputStartId; outputNodeId < outputStopId; outputNodeId++)
                            genome.AddConnectionGene(new ConnectionGene(inputNodeId, outputNodeId, Random.Range(config.newWeightRange.x, config.newWeightRange.y), true, _connectionInnovation.GetNewInnovationNumber()));
                    }
                }
            }

            _startingGenome = genome;
            Debug.Log("Starting Genome:\n" + genome.ToString());

            // Create Evaluator
            _evaluator = new EvaluatorRT(config, genome, _nodeInnovation, _connectionInnovation);
        }

        private void GenerateActiveCreatureList()
        {
            List<Genome> genomeList = _evaluator.GetGenomes();
            foreach (Genome genome in genomeList)
            {
                GameObject creature = Instantiate(_creaturePrefab, Vector3.zero, Quaternion.identity);
                InitCreature(creature, genome, GetSpawnPosition());
            }
            _evaluator.CleanGenomes();
        }

        private void InitCreature(GameObject creature, Genome genome, Vector3 position)
        {
            CreatureBrain creatureBrain = creature.GetComponent<CreatureBrain>();
            creatureBrain.CreatureManager = this;
            creatureBrain.Reset();
            creatureBrain.NeuralNetwork = new NeuralNetwork(genome, config.activationType, config.bias, config.timeOut);
            creatureBrain.Id = genome.Id;
            creatureBrain.IsInit = true;
            creature.transform.position = position;
            creature.SetActive(true);
            _activeCreatureList.Add(creature);
        }

        private void GenerateInactiveCreatureList()
        {
            int inactiveTot = config.maxPopulation - config.startPopulation;
            for (int i = 0; i < inactiveTot; i++)
            {
                GameObject creature = Instantiate(_creaturePrefab, Vector3.zero, Quaternion.identity);
                CreatureBrain creatureBrain = creature.GetComponent<CreatureBrain>();
                creatureBrain.Reset();
                creature.SetActive(false);
                _inactiveCreatureList.Add(creature);
            }
        }

        public void CreatureDied(GameObject creature)
        {
            creature.SetActive(false);
            CreatureBrain creatureBrain = creature.GetComponent<CreatureBrain>();
            _evaluator.AddGenomeToList(creatureBrain.NeuralNetwork.Genome);
            creatureBrain.Reset();
            _activeCreatureList.Remove(creature);
            _inactiveCreatureList.Add(creature);
        }

        public void CreatureBreed(Genome genome, Vector3 position)
        {
            if (_inactiveCreatureList.Count == 0) return;
            Genome child = _evaluator.BreedFromItself(genome);
            GameObject creature = _inactiveCreatureList[0];
            _inactiveCreatureList.RemoveAt(0);
            InitCreature(creature, genome, position);
        }

        private Vector2 GetSpawnPosition()
        {
            // TODO Avoid to spawn in other Creature
            Vector2 randPos = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            return (randPos * _size / 2) + Vector2.zero;
        }

    }
}