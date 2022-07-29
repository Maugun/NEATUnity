using System;
using UnityEngine;

namespace NEAT.Demo.PreyVsPredator
{
    public class PredatorController : MonoBehaviour
    {
        public CreatureBrain CreatureBrain { get; set; }
        public int Id { get; set; }
        public bool IsInit { get; set; }

        [Header("Movements")]
        [SerializeField] private float _accelerationForce = 2f;
        [SerializeField] private float _turningForce = 1f;

        [Header("Energy")]
        [SerializeField] private float _energyMax = 50f;
        [SerializeField] private float _energyGainFromEattingPrey = 25f;
        [SerializeField] private float _energyLostOnMovementMultiplicator = 1f;
        [SerializeField] private float _energyLostOvertime = 1f;

        [Header("Breed")]
        [SerializeField] private float _preyEatenToBreed = 3;

        [Header("Eat")]
        [SerializeField] private float _digestionTimer = 3f;

        [Header("Map")]
        [SerializeField] private int _size = 50;

        private Rigidbody2D _rigidBody;
        private CreatureEyes _eyes;
        private float _energy;
        private float _timer;
        private float _lastPreyEatenTimer;
        private float _preyEaten;
        private float _totalPreyEaten;

        private void Awake()
        {
            _rigidBody = transform.GetComponent<Rigidbody2D>();
            _eyes = transform.GetComponent<CreatureEyes>();
            CreatureBrain = transform.GetComponent<CreatureBrain>();
        }

        private void OnEnable()
        {
            Reset();
        }

        private void Reset()
        {
            IsInit = false;
            Id = -1;
            _energy = _energyMax;
            _timer = 0f;
            _lastPreyEatenTimer = 0f;
            _preyEaten = 0f;
            _totalPreyEaten = 0f;
        }

        private void FixedUpdate()
        {
            if (!CreatureBrain.IsInit || CreatureBrain.NeuralNetwork == null) return;

            Vector3 position = transform.position;
            float halfSize = _size / 2;
            if (position.x > halfSize) position.x = -halfSize;
            if (position.x < -halfSize) position.x = halfSize;
            if (position.y > halfSize) position.y = -halfSize;
            if (position.y < -halfSize) position.y = halfSize;
            transform.position = position;

            _timer += Time.fixedDeltaTime;
            _lastPreyEatenTimer += Time.fixedDeltaTime;
            CreatureBrain.Time += Time.fixedDeltaTime;

            float[] inputs = _eyes.See();
            float[] outputs = CreatureBrain.NeuralNetwork.FeedForward(inputs);
            Move(outputs[0], outputs[1]);
            EnergyManagement(outputs[1]);
            Breed();
        }

        private void Move(float turning, float acceleration)
        {
            transform.Rotate(0f, 0f, turning * _turningForce);
            _rigidBody.velocity = acceleration * _accelerationForce * (Vector2)transform.up;
            _rigidBody.angularVelocity = 0;
        }

        private void EnergyManagement(float acceleration)
        {
            // float absoluteAcceleration = Math.Abs(acceleration) * Time.deltaTime;
            // _energy -= absoluteAcceleration * _energyLostOnMovementMultiplicator + _energyLostOvertime * Time.fixedDeltaTime;
            _energy -= _energyLostOvertime * Time.fixedDeltaTime;

            if (_energy <= 0f) Die();
        }

        private void Die()
        {
            Debug.Log("Predator " + CreatureBrain.Id + " Died !");
            CreatureBrain.NeuralNetwork.Fitness = CreatureBrain.Time;
            CreatureBrain.CreatureManager.CreatureDied(gameObject);
        }

        private void Breed()
        {
            if (_preyEaten < _preyEatenToBreed) return;

            _preyEaten = 0f;
            CreatureBrain.Breed(transform.position);
        }

        private bool Eat()
        {
            if (_lastPreyEatenTimer < _digestionTimer) return false;

            _preyEaten++;
            _totalPreyEaten++;
            _energy += _energyGainFromEattingPrey;
            _lastPreyEatenTimer = 0f;
            return true;
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            if (other.gameObject.tag == "Prey" && Eat()) other.transform.GetComponent<PreyController>().Die();
        }
    }
}