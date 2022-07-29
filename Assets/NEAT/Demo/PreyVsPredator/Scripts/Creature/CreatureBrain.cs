using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEAT.Demo.PreyVsPredator
{
    public class CreatureBrain : MonoBehaviour
    {
        public int Id { get; set; }
        public bool IsInit { get; set; }
        public NeuralNetwork NeuralNetwork { get; set; }
        public float Time { get; set; }
        public int Eat { get; set; }
        public CreatureManager CreatureManager { get; set; }

        public void Reset()
        {
            Id = -1;
            IsInit = false;
            NeuralNetwork = null;
            Time = 0;
            Eat = 0;
        }

        public void Breed(Vector3 position)
        {
            CreatureManager.CreatureBreed(NeuralNetwork.Genome, position);
        }
    }
}
