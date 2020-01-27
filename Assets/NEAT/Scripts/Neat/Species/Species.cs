using System;
using System.Collections.Generic;

namespace NEAT
{
    public class Species
    {
        public Genome Genome { get; set; }
        public List<Genome> Genomes { get; set; }
        public List<FitnessGenome> FitnessPop { get; set; }
        public float TotalAdjustedFitness { get; set; }

        public Species(Genome genome)
        {
            Genome = genome;
            Genomes = new List<Genome>();
            FitnessPop = new List<FitnessGenome>();
            TotalAdjustedFitness = 0f;
        }

        public void AddAdjustedFitness(float adjustedFitness)
        {
            TotalAdjustedFitness += adjustedFitness;
        }

        public void Reset(Random r)
        {
            Genome = Genomes[r.Next(Genomes.Count)];
            Genomes.Clear();
            FitnessPop.Clear();
            TotalAdjustedFitness = 0f;
        }
    }
}