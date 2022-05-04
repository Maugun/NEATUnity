using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

namespace NEAT
{
    public abstract class Evaluator
    {
        private InnovationCounter _nodeInnovation;                                                  // Node Innovation
        private InnovationCounter _connectionInnovation;                                            // Connection Innovation
        private NEATConfig _config;                                                                 // Config

        private readonly Random _r = new Random();                                                  // Random

        private readonly int _addConnectionMutationMaxTries;                                        // Max Tries to attemp Add Connection Mutation

        private List<Genome> _genomes;                                                              // Genome List
        private List<Genome> _nextEvaluationGenomes;                                                // Next Evaluation Genome List

        private List<Species> _species;                                                             // Species

        private Dictionary<Genome, Species> _genomesSpecies;                                        // Genomes, Species Dictionary
        public float BestFitness { get; set; }                                                      // Best Fitness
        public Genome BestGenome { get; set; }                                                      // Best Genome
        public int GenerationNumber { get; set; }                                                   // Generation Number

        private string _mutationLogs = "";                                                          // Mutation Logs

        public Evaluator(NEATConfig config, Genome startingGenome, InnovationCounter nodeInnovation, InnovationCounter connectionInnovation)
        {
            _config = config;
            GenerationNumber = 0;
            _nodeInnovation = nodeInnovation;
            _connectionInnovation = connectionInnovation;

            _genomes = new List<Genome>();
            for (int i = 0; i < _config.populationSize; ++i)
                _genomes.Add(Genome.GenomeRandomWeights(startingGenome, _r));

            _nextEvaluationGenomes = new List<Genome>();

            _species = new List<Species>();

            _genomesSpecies = new Dictionary<Genome, Species>();

            BestFitness = -1f;
            BestGenome = null;
        }

        /// <summary>
        /// Evaluate Genomes
        /// </summary>
        public void Evaluate()
        {
            GenerationNumber++;
            ResetForEvaluation();

            // Get Genomes Fitness
            foreach (Genome genome in _genomes)
                genome.Fitness = EvaluateGenome(genome);

            // Sort Genomes by Fitness (Desc order)
            _genomes = _genomes.OrderByDescending(o => o.Fitness).ToList();

            //string log = "";
            //foreach (Genome g in _genomes)
            //    log += g.Fitness + "\n";
            //Debug.Log(log);

            if (_config.species)
            {
                // Create Species
                SpeciesCreation();

                GetGenomesFitness();
                KeepBestGenomeForNextEvaluation();
            }
            else
            {
                // Get Best Genome & Fitness
                BestGenome = _genomes[0];
                BestFitness = _genomes[0].Fitness;

                // Keep Best Genomes for Next Evaluation
                int maxToKeep = ((_config.populationSize * _config.percentageToKeep) / 100) - 1;
                for (int i = 0; i <= maxToKeep; i++)
                    _nextEvaluationGenomes.Add(_genomes[i]);
            }

            Breed();
        }

        /// <summary>
        /// Evaluate a Genome
        /// </summary>
        /// <param name="genome">Genome to Evaluate</param>
        /// <returns></returns>
        protected abstract float EvaluateGenome(Genome genome);

        /// <summary>
        /// Reset for Evaluation
        /// </summary>
        private void ResetForEvaluation()
        {
            foreach (Species species in _species) species.Reset(_r);
            _genomesSpecies.Clear();
            _nextEvaluationGenomes.Clear();
            BestFitness = -1f;
            BestGenome = null;
        }

        /// <summary>
        /// Species Creation
        /// </summary>
        private void SpeciesCreation()
        {
            // Loop in Genomes
            foreach (Genome genome in _genomes)
            {
                bool isNewSpecies = true;

                // Loop in Species
                foreach (Species species in _species)
                {
                    // If Genome Distance < Max Genome Distance => Genome is from this Species
                    if (Genome.GenomeDistance(genome, species.Genome, _config.disjointMutator, _config.excessMutator, _config.avgDiffMutator) < _config.maxGenomeDistance)
                    {
                        species.Genomes.Add(genome);
                        _genomesSpecies.Add(genome, species);
                        isNewSpecies = false;
                        break;
                    }
                }

                // If the Genome has no Species => Add it to a new Species
                if (isNewSpecies)
                {
                    Species newSpecies = new Species(genome);
                    _species.Add(newSpecies);
                    _genomesSpecies.Add(genome, newSpecies);
                }
            }

            // Remove Empty Species
            _species.RemoveAll(item => item.Genomes.Count == 0);

            // Order Species Genome by Fitness (Desc order)
            foreach (Species species in _species)
                species.Genomes = species.Genomes.OrderByDescending(o => o.Fitness).ToList();
        }

        /// <summary>
        /// Get Genomes Fitness
        /// </summary>
        private void GetGenomesFitness()
        {
            foreach (Genome genome in _genomes)
            {
                // Get Species of the Genome
                Species species = _genomesSpecies[genome];

                float fitness = EvaluateGenome(genome);
                float adjustedFitness = fitness / _genomesSpecies[genome].Genomes.Count;

                species.AddAdjustedFitness(adjustedFitness);
                genome.Fitness = adjustedFitness;
                species.Genomes.Add(new Genome(genome));
                if (fitness > BestFitness)
                {
                    BestFitness = fitness;
                    BestGenome = genome;
                }
            }
        }

        /// <summary>
        /// Keep Best Genome from each Species for next Evaluation
        /// </summary>
        private void KeepBestGenomeForNextEvaluation()
        {
            foreach (Species species in _species)
            {
                // Order from Best to Worst Genome
                species.Genomes = species.Genomes.OrderByDescending(o => o.Fitness).ToList();

                // Add Best Genome to next Evaluation Genome
                _nextEvaluationGenomes.Add(species.Genomes[0]);
            }
        }

        /// <summary>
        /// Breed
        /// </summary>
        private void Breed()
        {
            // Logs Setup
            int weightMutationNb = 0;
            int addConnectionNb = 0;
            int addNodeNb = 0;
            int enableDisableConnectionNb = 0;
            _mutationLogs = "";

            while (_nextEvaluationGenomes.Count() < _config.populationSize)
            {
                Genome child = null;
                Genome mom = null;
                Genome dad = null;

                // Get Child

                Species species = null;
                int maxToKeep = ((_config.populationSize * _config.percentageToKeep) / 100) - 1;
                if (_config.species) species = GetRandomSpecies(_r);
                if (_config.crossover)
                {
                    if (species != null)
                    {
                        mom = GetRandomGenome(species, _r);
                        dad = GetRandomGenome(species, _r);
                    }
                    else
                    {
                        mom = _genomes[UnityEngine.Random.Range(0, maxToKeep)];
                        dad = _genomes[UnityEngine.Random.Range(0, maxToKeep)];
                    }

                    // Crossover between Mom & Dad
                    child = mom.Fitness >= dad.Fitness ? Genome.Crossover(mom, dad, _r, _config.disabledConnectionInheritChance) : Genome.Crossover(dad, mom, _r, _config.disabledConnectionInheritChance);
                }
                else
                {
                    child = species != null ? new Genome(GetRandomGenome(species, _r)) : new Genome(_genomes[UnityEngine.Random.Range(0, maxToKeep)]);
                }

                // Weights Mutation
                if ((float)_r.NextDouble() < _config.mutationRate)
                {
                    child.WeightsMutation(_r);
                    weightMutationNb++;
                }

                if (_config.genomeMutations)
                {
                    // Add Connection Mutation
                    if ((float)_r.NextDouble() < _config.addConnectionRate)
                    {
                        // If for Logs
                        if (child.AddConnectionMutation(_r, _connectionInnovation, 10)) addConnectionNb++;
                    }

                    // Add Node Mutation
                    if ((float)_r.NextDouble() < _config.addNodeRate)
                    {
                        child.AddNodeMutation(_r, _connectionInnovation, _nodeInnovation);
                        addNodeNb++;
                    }

                    // Enable/Disable a Random Connection
                    if ((float)_r.NextDouble() < _config.enableDisableRate)
                    {
                        // If for Logs
                        if (child.EnableOrDisableRandomConnection()) enableDisableConnectionNb++;
                    }
                }

                // Add Child to Next Evaluation Genomes
                _nextEvaluationGenomes.Add(child);
            }

            _mutationLogs += string.Format(
                "Weights Mutation: {0}, Add Connection: {1}, Add Node: {2}, Enable/Disable Connection: {3}\nCrossover is {4}, Genome Mutations is {5}, Species is {6}",
                weightMutationNb,
                addConnectionNb,
                addNodeNb,
                enableDisableConnectionNb,
                _config.crossover,
                _config.genomeMutations,
                _config.species
            );

            _genomes.Clear();
            _genomes = new List<Genome>(_nextEvaluationGenomes);
            _nextEvaluationGenomes = new List<Genome>();
        }

        /// <summary>
        /// Get Random Species Biased By Adjusted Fitness (More chance of selecting a Species w/ an higher Adjusted Fitness)
        /// </summary>
        /// <param name="r">Random</param>
        /// <returns></returns>
        private Species GetRandomSpecies(Random r)
        {
            // Get Species Total Adjusted Fitness Sum
            double totalAdjustedFitnessSum = 0.0;
            foreach (Species species in _species)
                totalAdjustedFitnessSum += species.TotalAdjustedFitness;

            // Get Total Adjusted Fitness Selection Value
            double totalAdjustedFitnessSelection = r.NextDouble() * totalAdjustedFitnessSum;

            // Get Selected Species
            totalAdjustedFitnessSum = 0.0;
            foreach (Species species in _species)
            {
                totalAdjustedFitnessSum += species.TotalAdjustedFitness;
                if (totalAdjustedFitnessSum >= totalAdjustedFitnessSelection) return species;
            }

            // If no Species found return the Best one
            List<Species> speciesOrderedByFitnessDesc = _species.OrderByDescending(o => o.TotalAdjustedFitness).ToList();
            if (speciesOrderedByFitnessDesc.Count == 0) return null;
            return speciesOrderedByFitnessDesc[0];
        }

        /// <summary>
        /// Get Random Genome Biased By Adjusted Fitness (More chance of selecting a Genome w/ an higher Fitness)
        /// </summary>
        /// <param name="r">Random</param>
        /// <returns></returns>
        private Genome GetRandomGenome(Species species, Random r)
        {
            // Get Genomes Total Fitness Sum
            double totalFitnessSum = 0.0;
            foreach (Genome genome in species.Genomes)
                totalFitnessSum += genome.Fitness;

            // Get Total Fitness Selection Value
            double totalFitnessSelection = r.NextDouble() * totalFitnessSum;

            // Get Selected Genome
            totalFitnessSum = 0.0;
            foreach (Genome genome in species.Genomes)
            {
                totalFitnessSum += genome.Fitness;
                if (totalFitnessSum >= totalFitnessSelection) return genome;
            }

            // If no Genome found return the Best one
            List<Genome> genomesOrderedByFitnessDesc = species.Genomes.OrderByDescending(o => o.Fitness).ToList();
            return genomesOrderedByFitnessDesc[0];
        }

        /// <summary>
        /// Get Number of Species
        /// </summary>
        /// <returns></returns>
        public int GetSpeciesNumber()
        {
            return _species.Count;
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
        /// Get Mutation Logs
        /// </summary>
        /// <returns></returns>
        public string GetMutationLogs()
        {
            return _mutationLogs;
        }

        /// <summary>
        /// Get Generation Logs
        /// </summary>
        /// <returns></returns>
        public string GetGenerationLogs()
        {
            return string.Format(
                "Generation: {0} | Best Fitness: {1} | Species Nb: {2}\n{3} \n\nBest Genome:\n{4}",
                GenerationNumber,
                BestFitness,
                GetSpeciesNumber(),
                GetMutationLogs(),
                BestGenome.ToString()
            );
        }
    }
}
