using UnityEngine;

namespace NEAT.Demo.SmartCarV2
{
    public class SimpleCar : MonoBehaviour
    {
        public float _accelForce = 1f;
        public float _steeringForce = 1f;
        private Rigidbody _rb;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void Move(float steering, float accel)
        {
            transform.Rotate(0f, steering * _steeringForce, 0f);
            _rb.velocity = transform.forward * accel * _accelForce;
        }
    }
}
