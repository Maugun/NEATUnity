
namespace NEAT
{
    public class InnovationCounter
    {
        private int _currentInnovation = 0;

        public int GetNewInnovationNumber()
        {
            return _currentInnovation++;
        }
    }
}
