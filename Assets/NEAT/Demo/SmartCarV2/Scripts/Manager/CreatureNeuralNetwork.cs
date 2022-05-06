using UnityEngine;

namespace NEAT.Demo.SmartCarV2
{
    public class CreatureNeuralNetwork : MonoBehaviour
    {
        public static CreatureNeuralNetwork BestNN { get; set; }

        public int Id { get; set; }
        public bool IsInit { get; set; }
        public int CheckpointPassed { get; set; }
        public NeuralNetwork NeuralNetwork { get; set; }
        public float Time { get; set; }

        public void Reset(int id)
        {
            Id = id;
            IsInit = false;
            CheckpointPassed = 0;
            NeuralNetwork = null;
            Time = 0;
            if (BestNN == null) BestNN = this;
        }
    }
}
