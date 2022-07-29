using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

namespace NEAT.Demo.PreyVsPredator
{
    public class EvaluatorRT
    {
        private InnovationCounter _nodeInnovation;                                                  // Node Innovation
        private InnovationCounter _connectionInnovation;                                            // Connection Innovation
        private RTNEATConfig _config;                                                               // Config

        private readonly Random _r = new Random();                                                  // Random

        private readonly int _addConnectionMutationMaxTries;                                        // Max Tries to attemp Add Connection Mutation

        private List<Genome> _genomes;                                                              // Genome List
        private int _currentPop;                                                                    // Current Population
        public float BestFitness { get; set; }                                                      // Best Fitness
        public Genome BestGenome { get; set; }                                                      // Best Genome

        private string _mutationLogs = "";                                                          // Mutation Logs

        public EvaluatorRT(RTNEATConfig config, Genome startingGenome, InnovationCounter nodeInnovation, InnovationCounter connectionInnovation)
        {
            _config = config;
            _nodeInnovation = nodeInnovation;
            _connectionInnovation = connectionInnovation;
            _currentPop = 0;
            BestFitness = -1f;
            BestGenome = null;

            _genomes = new List<Genome>();
            if (startingGenome.Connections.Count == 0)
            {
                for (int i = 0; i < _config.startPopulation; ++i)
                    AddGenomeToList(BreedFromItself(startingGenome));
                return;
            }
            for (int i = 0; i < _config.startPopulation; ++i)
                AddGenomeToList(Genome.GenomeRandomWeights(startingGenome, _r));

        }

        public Genome AddNewGenome(Genome genomeToAdd = null)
        {
            Genome genome = null;
            if (genomeToAdd == null)
            {
                OrderGenomeByFitness();
                genome = Breed();
            }
            else
                genome = Genome.GenomeRandomWeights(genomeToAdd, _r);

            AddGenomeToList(genome);
            return genome;
        }

        public Genome BreedFromParents(Genome mom, Genome dad)
        {
            Genome genome = Breed(mom, dad);
            AddGenomeToList(genome);
            return genome;
        }

        public Genome BreedFromItself(Genome mom)
        {
            Genome genome = Breed(mom);
            AddGenomeToList(genome);
            return genome;
        }

        public void AddGenomeToList(Genome genome)
        {
            _genomes.Add(genome);
            _currentPop++;
        }

        public void OrderGenomeByFitness()
        {
            // Reset
            BestFitness = -1f;
            BestGenome = null;

            // Sort Genomes by Fitness (Desc order)
            _genomes = _genomes.OrderByDescending(o => o.Fitness).ToList();
            BestGenome = _genomes[0];
            BestFitness = _genomes[0].Fitness;
        }

        public void CleanGenomes()
        {
            OrderGenomeByFitness();
            int maxToKeep = MaxToKeep();
            for (int i = _genomes.Count - 1; i > maxToKeep; --i)
            {
                _genomes.RemoveAt(i);
            }
            _currentPop = _genomes.Count;
        }

        /// <summary>
        /// Breed
        /// </summary>
        private Genome Breed(Genome mom = null, Genome dad = null)
        {
            Genome child = null;

            // Get Child
            int maxToKeep = MaxToKeep();
            if (_config.crossover)
            {
                if (dad != null && mom != null)
                {
                    // Crossover between Mom & Dad
                    child = mom.Fitness >= dad.Fitness ? Genome.Crossover(mom, dad, _r, _config.disabledConnectionInheritChance) : Genome.Crossover(dad, mom, _r, _config.disabledConnectionInheritChance);
                }
                else
                {
                    mom = mom != null ? mom : _genomes[UnityEngine.Random.Range(0, maxToKeep)];
                    dad = dad != null ? dad : _genomes[UnityEngine.Random.Range(0, maxToKeep)];

                    // Crossover between Mom & Dad
                    child = mom.Fitness >= dad.Fitness ? Genome.Crossover(mom, dad, _r, _config.disabledConnectionInheritChance) : Genome.Crossover(dad, mom, _r, _config.disabledConnectionInheritChance);
                }
            }
            else
            {
                child = mom != null ? new Genome(mom) : new Genome(_genomes[UnityEngine.Random.Range(0, maxToKeep)]);
            }

            // Weights Mutation
            if ((float)_r.NextDouble() < _config.mutationRate)
                child.WeightsMutation(_r);

            if (_config.genomeMutations)
            {
                // Add Connection Mutation
                if ((float)_r.NextDouble() < _config.addConnectionRate)
                    child.AddConnectionMutation(_r, _connectionInnovation, 10);

                // Add Node Mutation
                if ((float)_r.NextDouble() < _config.addNodeRate)
                    child.AddNodeMutation(_r, _connectionInnovation, _nodeInnovation);

                // Enable/Disable a Random Connection
                if ((float)_r.NextDouble() < _config.enableDisableRate)
                    child.EnableOrDisableRandomConnection();
            }

            child.Fitness = 0f;
            return child;
        }

        /// <summary>
        /// Get Genomes List
        /// </summary>
        /// <returns></returns>
        public List<Genome> GetGenomes()
        {
            return _genomes;
        }

        /// <summary>
        /// Get Max Population to Keep
        /// </summary>
        /// <returns></returns>
        public int MaxToKeep()
        {
            return ((_config.maxPopulation * _config.percentageToKeep) / 100) - 1;
        }
    }
}
