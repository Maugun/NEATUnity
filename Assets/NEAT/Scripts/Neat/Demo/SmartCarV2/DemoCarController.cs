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

        public float _deathTimer = 5f;                                                          // Death Timer
        public float _viewAngle = 160f;                                                         // View Angle
        public int _viewRayNb = 9;                                                              // View Ray Number
        public bool _addBackRay = true;                                                         // Add a Back Ray ?
        public int _rayCastLen = 12;                                                            // RayCast Length
        public bool _drawEyes = true;                                                           // Draw Eyes ?
        public string _hitLayerName = "Wall";                                                   // Hit Layer Name

        [SerializeField]
        private LayerMask _sensorMask;                                                          // Defines the layer of the walls ("Wall")
        private LineRenderer _lr;                                                               // Line Renderer for RayCast Visualization

        private float _timer = 0f;                                                              // Timer
        private float _oldFitness = 0f;                                                         // Old Fitness
        private NEATManager _NEATManager;                                                       // NEATManager

        void Start()
        {
            _NEATManager = GameObject.Find("NEATManager").GetComponent<NEATManager>();
            _demoCarController = GetComponent<SimpleCar>();
            _unityCarController = GetComponent<CarController>();
            _creatureNN = GetComponent<CreatureNeuralNetwork>();
            _lr = GetComponent<LineRenderer>();
            _lr.positionCount = (_viewRayNb + (_addBackRay ? 1 : 0)) * 2;
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

            if (IsInit)
            {
                _timer += Time.deltaTime;
                if (_timer >= _deathTimer)
                {
                    WallHit();                                                                  // Kill this car
                    return;
                }

                // NN Actions
                Brain(Eyes(false));

                //if (_creatureNN.Id == 0)
                //    Debug.Log("(h, v) = (" + _h + ", " + _v + ")");

                // Move
                if (_demoCarController != null)
                    _demoCarController.Move(_h, _v);
                else
                    _unityCarController.Move(_h, _v, _v, 0f);

                // Update LR
                if (_drawEyes)
                {
                    _lr.enabled = true;
                    Eyes(true);
                }
                else
                    _lr.enabled = false;
                    
            }
        }

        private void Brain(float[] inputs)
        {
            // Feed UNN
            float[] outputs = _creatureNN.NeuralNetwork.FeedForward(inputs);

            // Set Output
            _v = outputs[0];
            _h = outputs[1];
        }

        private float[] Eyes(bool draw)
        {
            float[] inputs = new float[_viewRayNb + (_addBackRay ? 1 : 0)];

            // Cast view angles ray 
            float stepAngleSize = _viewAngle / (_viewRayNb - 1);
            for (int i = 0; i < _viewRayNb; ++i)
            {
                float angle = 0f - (_viewAngle / 2) + (stepAngleSize * i);
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * transform.forward;
                Vector3 dirLine = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                inputs[i] = CastRay(dir, dirLine, (i*2) + 1, draw);
                //if (draw)
                //    Debug.DrawRay(transform.position, dir * _rayCastLen, Color.white);
            }

            // Cast back ray
            if (_addBackRay)
            {
                inputs[_viewRayNb] = CastRay(-transform.forward, -Vector3.forward, (_viewRayNb * 2) + 1, draw);
                //if (draw)
                //    Debug.DrawRay(transform.position, -transform.forward * _rayCastLen, Color.white);
            }
            return inputs;
        }

        public float CastRay(Vector3 rayDirection, Vector3 lineDirection, int index, bool draw)
        {
            // Set LR point to center
            if (draw)
                _lr.SetPosition(index - 1, Vector3.zero);

            // Cast a ray
            RaycastHit hit;
            if (Physics.Raycast(transform.position, rayDirection, out hit, _rayCastLen, _sensorMask))
            {
                // Return the distance of the hit in the line
                float dist = Vector3.Distance(hit.point, transform.position);
                if (draw)
                    _lr.SetPosition(index, dist * lineDirection);
                return dist;
            }
            else
            {
                // Return the maximum distance
                if (draw)
                    _lr.SetPosition(index, _rayCastLen * lineDirection);
                return _rayCastLen;
            }

        }

        public void OnCheckPoint()
        {
            _creatureNN.NeuralNetwork.Fitness += 1;
            _timer = 0f; 
            //Debug.Log(_creatureNN.Id + " Fitness : " + _creatureNN.NeuralNetwork.Fitness);
        }

        public void WallHit()
        {
            _creatureNN.IsInit = false;
            IsInit = false;
            //Debug.Log(_creatureNN.Id + " DEAD");
            gameObject.SetActive(false);                                                        // Make sure the car is inactive
            _NEATManager.AddDeadCreature(_creatureNN.Id);                                       // Tell the Evolution Manager that the car is dead
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer(_hitLayerName))
                WallHit(); // Kill
        }
    }
}
