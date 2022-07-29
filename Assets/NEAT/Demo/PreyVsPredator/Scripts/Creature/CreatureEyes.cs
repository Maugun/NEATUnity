using UnityEngine;


namespace NEAT.Demo.PreyVsPredator
{
    public class CreatureEyes : MonoBehaviour
    {
        [Header("View")]
        [SerializeField] private int _rayNumber = 24;
        [SerializeField] private float _angle = 45f;
        [SerializeField] private float _distance = 10f;
        [SerializeField] private bool _hasBackRay = true;
        [SerializeField] private LayerMask _sensorMask;

        [Header("Debug")]
        [SerializeField] private bool _draw = true;

        public float[] See()
        {
            float[] inputs = new float[_rayNumber + (_hasBackRay ? 1 : 0)];

            // Cast view angles ray 
            float stepAngleSize = _angle / (_rayNumber - 1);
            for (int i = 0; i < _rayNumber; i++)
            {
                float angle = 0f - (_angle / 2) + (stepAngleSize * i);
                Vector3 rayDirection = Quaternion.Euler(0f, 0f, angle) * transform.up;
                inputs[i] = CastRay(rayDirection);
            }

            // Cast back ray
            if (_hasBackRay) inputs[_rayNumber] = CastRay(-transform.up);
            return inputs;
        }

        private float CastRay(Vector3 rayDirection)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, _distance, _sensorMask);
            float distance = hit.distance > 0 ? hit.distance : _distance;
            if (_draw) Debug.DrawLine(transform.position, transform.position + rayDirection * distance, Color.white);
            return distance;
        }
    }
}
