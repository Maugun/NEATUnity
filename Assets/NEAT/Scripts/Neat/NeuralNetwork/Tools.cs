namespace NEAT
{
    public class Tools
    {
        public static float NormalizeInput(float value, float inputMin, float inputMax)
        {
            return (value - inputMin) / (inputMax - inputMin);
        }
    }
}
