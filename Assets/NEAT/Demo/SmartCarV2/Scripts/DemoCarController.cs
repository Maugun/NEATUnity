using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

namespace NEAT.Demo.SmartCarV2
{
    public class DemoCarController : MonoBehaviour
    {
        public bool IsInit { get; set; }                                                        // Is Initialize ?
        public CreatureNeuralNetwork CreatureNN { get; set; }                                   // Creature Neural Network

        public float deathTimer = 5f;                                                           // Death Timer
        public float viewAngle = 220f;                                                          // View Angle
        public int viewRayNb = 9;                                                               // View Ray Number
        public int rayCastLen = 20;                                                             // RayCast Length
        public bool addBackRay = true;                                                          // Add a Back Ray ?
        public bool drawEyes = true;                                                            // Draw Eyes ?
        public bool timeAttack = true;
        public string hitLayerName = "Wall";                                                    // Hit Layer Name

        [SerializeField]
        private LayerMask _sensorMask;                                                          // Defines the layer of the walls ("Wall")

        private SimpleCar _simpleCarController;                                                 // Simple Car Controller
        private CarController _unityCarController;                                              // Unity Car Controller
        private NEATManager _NEATManager;                                                       // NEATManager
        private LineRenderer _lr;                                                               // Line Renderer for RayCast Visualization
        private float _h;                                                                       // H
        private float _v;                                                                       // V
        private float[] _previousFrameInputs;                                                   // Previous Frame Inputs
        private float _timer;                                                                   // Timer

        void Start()
        {
            _NEATManager = GameObject.Find("NEATManager").GetComponent<NEATManager>();
            _simpleCarController = GetComponent<SimpleCar>();
            _unityCarController = GetComponent<CarController>();
            CreatureNN = GetComponent<CreatureNeuralNetwork>();
            _lr = GetComponent<LineRenderer>();
            _lr.positionCount = (viewRayNb + (addBackRay ? 1 : 0)) * 2;
            Reset();
        }

        public void Reset()
        {
            _h = 0f;
            _v = 0f;
            _previousFrameInputs = null;
            _timer = 0f;
            IsInit = false;
        }

        void FixedUpdate()
        {
            if (!IsInit && CreatureNN.IsInit)
            {
                _timer = 0f;
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
            if (_simpleCarController != null)
                _simpleCarController.Move(_h, _v);
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
            float[] outputs = CreatureNN.NeuralNetwork.FeedForward(inputs);

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
            CreatureNN.NeuralNetwork.Fitness += timeAttack ? 1 + (deathTimer - _timer) : 1;
            CreatureNN.CheckpointPassed++;
            CreatureNN.Time += _timer;
            _timer = 0f;
            if (CreatureNeuralNetwork.BestNN.NeuralNetwork.Fitness < CreatureNN.NeuralNetwork.Fitness) CreatureNeuralNetwork.BestNN = CreatureNN;
            //Debug.Log(_creatureNN.Id + " Fitness : " + _creatureNN.NeuralNetwork.Fitness);
        }

        public void Die()
        {
            CreatureNN.IsInit = false;
            IsInit = false;
            //Debug.Log(_creatureNN.Id + " DEAD");
            gameObject.SetActive(false);                                                        // Make sure the car is inactive
            _NEATManager.AddDeadCreature(CreatureNN.Id);                                       // Tell the Evolution Manager that the car is dead
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer(hitLayerName))
                Die();
        }
    }
}
