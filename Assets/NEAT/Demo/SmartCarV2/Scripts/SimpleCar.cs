using UnityEngine;

namespace NEAT.Demo.SmartCarV2
{
    public class SimpleCar : MonoBehaviour
    {
        public float accelForce = 1f;
        public float steeringForce = 1f;
        private Rigidbody _rb;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void Move(float steering, float accel)
        {
            transform.Rotate(0f, steering * steeringForce, 0f);
            _rb.velocity = transform.forward * accel * accelForce;
        }
    }
}
