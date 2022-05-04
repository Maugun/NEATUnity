using UnityEngine;

namespace NEAT.Demo.SmartCarV2
{
    public class CreatureNeuralNetwork : MonoBehaviour
    {
        public NeuralNetwork NeuralNetwork { get; set; }
        public bool IsInit { get; set; }
        public int Id { get; set; }
    }
}
