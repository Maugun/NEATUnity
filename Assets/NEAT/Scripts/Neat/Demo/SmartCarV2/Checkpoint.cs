using System.Collections.Generic;
using UnityEngine;

namespace NEAT.Demo.SmartCarV2
{
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField]
        public string _layerHitName = "Player";                                                     // The name of the layer set on each car
        public int Id { get; set; }                                                                 // CP Id
        public static int _totalCp = 0;                                                             // Number of CP

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer(_layerHitName))                     // If this object is a car
            {
                DemoCarController car = other.transform.root.GetComponent<DemoCarController>();
                CreatureNeuralNetwork NN = other.transform.root.GetComponent<CreatureNeuralNetwork>();

                if ((NN.NeuralNetwork.Fitness == Id) || (NN.NeuralNetwork.Fitness % _totalCp == Id))
                    car.OnCheckPoint();
            }
        }
    }
}
