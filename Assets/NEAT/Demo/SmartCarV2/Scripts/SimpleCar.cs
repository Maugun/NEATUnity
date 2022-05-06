using UnityEngine;

namespace NEAT.Demo.SmartCarV2
{
    public class SimpleCar : MonoBehaviour
    {
        public float accelForce = 1f;
        public float steeringForce = 1f;
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            Reset();
        }

        public void Move(float steering, float accel)
        {
            transform.Rotate(0f, steering * steeringForce, 0f);
            _rb.velocity = transform.forward * accel * accelForce;
        }

        public void Reset()
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }
}
