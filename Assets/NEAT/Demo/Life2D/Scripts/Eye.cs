using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEAT.Demo.Life2D
{
    public class Eye : MonoBehaviour
    {
        public float _viewAngle = 160f;                                                         // View Angle
        public int _viewSegmentNumber = 5;                                                      // Number of View Segment
        public float _radius = 5f;                                                              // Radius
        public int _viewRayNb = 9;                                                              // View Ray Number
        public int _rayCastLen = 5;                                                             // RayCast Length
        public bool _draw = true;                                                               // Draw Eye ?
        public float _lrLineWidth = 0.25f;                                                      // Line Renderer Line Width
        public int _lrOrderZ = -1;                                                              // Use z value as LineRender Order in Layer
        public int InputStartId { get; set; }                                                   // Input Start Id

        [SerializeField]
        private LayerMask _sensorMask;                                                          // Defines the layer
        private LineRenderer _lr;                                                               // Line Renderer for RayCast Visualization
        private float[] _previousFrameInputs = null;                                            // Previous Frame Inputs
        private Creature _creatureScript;                                                       // Creature Script
        private float _segmentAngle;                                                            // Segment Angle

        void Start()
        {
            //_lr = GetComponent<LineRenderer>();
            //_lr.positionCount = _viewRayNb * 2;
            //_lr.startWidth = _lrLineWidth;
            //_lr.endWidth = _lrLineWidth;
            _creatureScript = transform.parent.GetComponent<Creature>();
            //_previousFrameInputs = new float[_viewRayNb];
            _previousFrameInputs = new float[_viewSegmentNumber * 3];
            ResetPreviousFrame();
            _segmentAngle = _viewAngle / _viewSegmentNumber;
        }

        void Update()
        {
            // Add Inputs
            //_creatureScript.AddInputs(_previousFrameInputs, InputStartId);

            // Get Inputs & Draw LR
            EyeSensor(_draw);
        }

        private void EyeSensor(bool draw)
        {
            //string str = "";
            ResetPreviousFrame();
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _radius, _sensorMask);

            if (hits.Length > 0)
            {
                // Get limit Vector dir
                Vector3 limitDir = Quaternion.Euler(0f, 0f, _viewAngle / 2) * transform.up;
                //str += "limDir = " + limitDir.ToString() + " | ";

                foreach (Collider2D hit in hits)
                {
                    if (hit.transform != transform.parent)
                    {
                        // Get angle between up & hit position
                        float angle = Vector3.Angle(transform.up, hit.transform.position - transform.position);
                        //str += "vangle = " + angle + " | ";

                        // If in View Angle
                        if (angle <= _viewAngle / 2)
                        {
                            // Get angle between limit Vector dir & hit position
                            angle = Vector3.Angle(limitDir, hit.transform.position - transform.position);
                            //str += "angle = " + angle + " | ";

                            // Segment == angle / segment angle
                            int segment = (int)Mathf.Floor(angle / _segmentAngle);
                            segment = (segment < _viewSegmentNumber) ? segment : _viewSegmentNumber - 1;
                            //str += "segment = " + segment + " | ";

                            if (hit.transform.tag == "Food")
                                _previousFrameInputs[segment * 3] += 1;
                            else if (hit.transform.tag == "Creature")
                                _previousFrameInputs[(segment * 3) + 1] += 1;
                            else if (hit.transform.tag == "Wall")
                                _previousFrameInputs[(segment * 3) + 2] += 1;
                        }
                    }
                }
            }
            //if (str != "")
            //    Debug.Log(str);
        }

        private void ResetPreviousFrame()
        {
            for (int i = 0; i < _previousFrameInputs.Length; ++i)
                _previousFrameInputs[i] = 0f;
        }

        //private float[] Eyes(bool draw)
        //{
        //    float[] inputs = new float[_viewRayNb * 2];
        //    _lr.positionCount = _viewRayNb * 2;

        //    // Update LR
        //    if (draw)
        //        _lr.enabled = true;
        //    else
        //        _lr.enabled = false;

        //    // Cast view angles ray 
        //    float stepAngleSize = _viewAngle / (_viewRayNb - 1);
        //    int y = 0;
        //    for (int i = 0; i < _viewRayNb; i++)
        //    {
        //        float angle = 0f - (_viewAngle / 2) + (stepAngleSize * i);
        //        Vector3 dir = Quaternion.Euler(0f, 0f, angle) * transform.up;
        //        Vector3 dirLine = (draw) ? (Quaternion.Euler(0f, 0f, angle) * Vector3.up) : Vector3.zero;
        //        float[] values = CastRay(dir, dirLine, (i * 2) + 1, draw);
        //        inputs[y] = values[0];
        //        inputs[y + 1] = values[1];
        //        y += 2;
        //        //Debug.DrawRay(transform.position, dir * _rayCastLen, Color.white);
        //    }
        //    return inputs;
        //}

        //public float[] CastRay(Vector3 rayDirection, Vector3 lineDirection, int index, bool draw)
        //{
        //    // Set LR point to center
        //    if (draw)
        //        _lr.SetPosition(index - 1, new Vector3(0f, 0f, _lrOrderZ));

        //    // Cast a ray
        //    RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, rayDirection, _rayCastLen, _sensorMask.value);
        //    RaycastHit2D trueHit = new RaycastHit2D();
        //    float distance = _rayCastLen;
        //    int type = 0;
        //    foreach (RaycastHit2D hit in hits)
        //    {
        //        if (hit.transform != transform.parent)
        //        {
        //            trueHit = hit;
        //            break;
        //        }

        //    }

        //    // Hit
        //    if (trueHit.collider != null)
        //    {
        //        distance = trueHit.distance;
        //        if (trueHit.transform.tag == "Food")
        //            type = 1;
        //        if (trueHit.transform.tag == "Creature")
        //            type = 2;
        //    }

        //    // Draw
        //    if (draw)
        //    {
        //        Vector3 position = (distance * (1 / transform.localScale.x)) * lineDirection;
        //        position.z = _lrOrderZ;
        //        _lr.SetPosition(index, position);
        //    }

        //    return new float[2] { distance, type };
        //}

        public float[] GetPreviousFrameInput()
        {
            return _previousFrameInputs;
        }
    }
}
