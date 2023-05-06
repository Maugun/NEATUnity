
namespace NEAT
{
    public class InnovationCounter
    {
        public int CurrentInnovation { get; set; }

        public InnovationCounter(int currentInnovation = 0) { CurrentInnovation = currentInnovation; }

        public int GetNewInnovationNumber()
        {
            return CurrentInnovation++;
        }
    }
}
