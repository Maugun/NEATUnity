using UnityEngine;

namespace NEAT.Demo.SmartCarV2
{
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField]
        public string layerHitName = "Player";                                                      // The name of the layer set on each car
        public int Id { get; set; }                                                                 // CP Id
        public static int totalCp = 0;                                                              // Number of CP

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer(layerHitName))                      // If this object is a car
            {
                DemoCarController car = other.transform.root.GetComponent<DemoCarController>();
                if ((car.CreatureNN.CheckpointPassed == Id) || (car.CreatureNN.CheckpointPassed % totalCp == Id)) car.OnCheckPoint();
            }
        }
    }
}
