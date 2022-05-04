using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

namespace NEAT.Demo.SmartCarV2
{
    public class DemoCarController : MonoBehaviour
    {
        private SimpleCar _demoCarController;                                                   // Demo Car Controller
        private CarController _unityCarController;                                              // Unity Car Controller
        private CreatureNeuralNetwork _creatureNN;                                              // Creature Neural Network

        public bool IsInit { get; set; }                                                        // Is Initialize ?
        private float _h = 0f;                                                                  // H
        private float _v = 0f;                                                                  // V

        public float deathTimer = 5f;                                                           // Death Timer
        public float viewAngle = 160f;                                                          // View Angle
        public int viewRayNb = 9;                                                               // View Ray Number
        public bool addBackRay = true;                                                          // Add a Back Ray ?
        public int rayCastLen = 12;                                                             // RayCast Length
        public bool drawEyes = true;                                                            // Draw Eyes ?
        public string hitLayerName = "Wall";                                                    // Hit Layer Name

        [SerializeField]
        private LayerMask _sensorMask;                                                          // Defines the layer of the walls ("Wall")
        private LineRenderer _lr;                                                               // Line Renderer for RayCast Visualization

        private float _timer = 0f;                                                              // Timer
        private float _oldFitness = 0f;                                                         // Old Fitness
        private NEATManager _NEATManager;                                                       // NEATManager
        private float[] _previousFrameInputs = null;                                            // Previous Frame Inputs

        void Start()
        {
            _NEATManager = GameObject.Find("NEATManager").GetComponent<NEATManager>();
            _demoCarController = GetComponent<SimpleCar>();
            _unityCarController = GetComponent<CarController>();
            _creatureNN = GetComponent<CreatureNeuralNetwork>();
            _lr = GetComponent<LineRenderer>();
            _lr.positionCount = (viewRayNb + (addBackRay ? 1 : 0)) * 2;
            IsInit = false;
        }

        void FixedUpdate()
        {
            if (!IsInit && _creatureNN.IsInit)
            {
                _timer = 0f;
                _oldFitness = 0f;
                IsInit = true;
            }

            if (!IsInit) return;

            _timer += Time.deltaTime;
            if (_timer >= deathTimer)
            {
                Die();
                return;
            }

            // NN Actions
            Brain(_previousFrameInputs);

            //if (_creatureNN.Id == 0)
            //    Debug.Log("(h, v) = (" + _h + ", " + _v + ")");

            // Move
            if (_demoCarController != null)
                _demoCarController.Move(_h, _v);
            else
                _unityCarController.Move(_h, _v, _v, 0f);

            // Inputs
            _previousFrameInputs = Eyes(drawEyes);
            //Debug.Log(string.Join(",", _previousFrameInputs));
        }

        private void Brain(float[] inputs)
        {
            // Check inputs
            if (inputs == null)
                return;

            // Feed UNN
            float[] outputs = _creatureNN.NeuralNetwork.FeedForward(inputs);

            // Set Output
            _v = outputs[0];
            _h = outputs[1];
        }

        private float[] Eyes(bool draw)
        {
            // Draw
            _lr.enabled = draw ? true : false;

            float[] inputs = new float[viewRayNb + (addBackRay ? 1 : 0)];

            // Cast view angles ray 
            float stepAngleSize = viewAngle / (viewRayNb - 1);
            for (int i = 0; i < viewRayNb; i++)
            {
                float angle = 0f - (viewAngle / 2) + (stepAngleSize * i);
                Vector3 rayDirection = Quaternion.Euler(0f, angle, 0f) * transform.forward;
                Vector3 lineDirection = draw ? Quaternion.Euler(0f, angle, 0f) * Vector3.forward : Vector3.zero;
                inputs[i] = CastRay(rayDirection, lineDirection, (i * 2) + 1, draw);
            }

            // Cast back ray
            if (addBackRay) inputs[viewRayNb] = CastRay(-transform.forward, -Vector3.forward, (viewRayNb * 2) + 1, draw);
            return inputs;
        }

        public float CastRay(Vector3 rayDirection, Vector3 lineDirection, int index, bool draw)
        {
            // Set LR point to center
            if (draw) _lr.SetPosition(index - 1, Vector3.zero);

            // Cast a ray
            RaycastHit hit;
            float dist = Physics.Raycast(transform.position, rayDirection, out hit, rayCastLen, _sensorMask) ? hit.distance : rayCastLen;
            if (draw) _lr.SetPosition(index, dist * lineDirection);
            return dist;
        }

        public void OnCheckPoint()
        {
            _creatureNN.NeuralNetwork.Fitness += 1;
            _timer = 0f;
            //Debug.Log(_creatureNN.Id + " Fitness : " + _creatureNN.NeuralNetwork.Fitness);
        }

        public void Die()
        {
            _creatureNN.IsInit = false;
            IsInit = false;
            //Debug.Log(_creatureNN.Id + " DEAD");
            gameObject.SetActive(false);                                                        // Make sure the car is inactive
            _NEATManager.AddDeadCreature(_creatureNN.Id);                                       // Tell the Evolution Manager that the car is dead
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer(hitLayerName))
                Die();
        }
    }
}
