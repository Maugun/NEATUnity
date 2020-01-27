using System.Collections.Generic;

namespace NEAT
{
    public class FitnessGenome : IComparer<FitnessGenome>
    {
        public Genome Genome { get; set; }
        public float Fitness { get; set; }

        public FitnessGenome(Genome genome, float fitness)
        {
            Genome = genome;
            Fitness = fitness;
        }

        public int Compare(FitnessGenome fg1, FitnessGenome fg2)
        {
            if (fg1.Fitness > fg2.Fitness)
                return 1;
            else if (fg1.Fitness < fg2.Fitness)
                return -1;
            else
                return 0;
        }
    }
}