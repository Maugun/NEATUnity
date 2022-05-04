using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace NEAT
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

            _genomes = new List<Genome>();
            for (int i = 0; i < _config.startPopulation; ++i)
                AddGenomeToList(Genome.GenomeRandomWeights(startingGenome, _r));

            BestFitness = -1f;
            BestGenome = null;
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

            // Get Genomes Fitness
            //foreach (Genome genome in _genomes)
            //    genome.Fitness = EvaluateGenome(genome);

            // Sort Genomes by Fitness (Desc order)
            _genomes = _genomes.OrderByDescending(o => o.Fitness).ToList();
            BestGenome = _genomes[0];
            BestFitness = _genomes[0].Fitness;
        }

        public void RemoveOldGenomes(List<int> excludeList)
        {
            OrderGenomeByFitness();
            int maxToKeep = ((_config.maxPopulation * _config.percentageToKeep) / 100) - 1;
            for (int i = _genomes.Count - 1; i > maxToKeep; --i)
            {
                if (!excludeList.Contains(_genomes[i].Id))
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
            if (dad != null && mom != null)
            {
                // Crossover between Mom & Dad
                if (mom.Fitness >= dad.Fitness)
                    child = Genome.Crossover(mom, dad, _r, _config.disabledConnectionInheritChance);
                else
                    child = Genome.Crossover(dad, mom, _r, _config.disabledConnectionInheritChance);
            }
            else
            {
                int maxToKeep = ((_currentPop * _config.percentageToKeep) / 100) - 1;
                mom = _genomes[UnityEngine.Random.Range(0, maxToKeep)];
                dad = _genomes[UnityEngine.Random.Range(0, maxToKeep)];

                // Crossover between Mom & Dad
                if (mom.Fitness >= dad.Fitness)
                    child = Genome.Crossover(mom, dad, _r, _config.disabledConnectionInheritChance);
                else
                    child = Genome.Crossover(dad, mom, _r, _config.disabledConnectionInheritChance);
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
    }
}
