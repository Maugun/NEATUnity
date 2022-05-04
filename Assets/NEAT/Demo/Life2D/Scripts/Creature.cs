using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NEAT.Demo.Life2D
{
    public class Creature : MonoBehaviour
    {
        [Header("Config")]
        public float _accelForce = 1f;                                      // Acceleration Force
        public float _turningForce = 1f;                                    // Turning Force
        public float _energy = 20f;                                         // Energy
        public float _energyInterval = 2f;                                  // Energy Interval
        public LayerMask _sensorMask;
        public LayerMask _wallSensorMask;
        public float _bitten = .5f;

        public NeuralNetwork NN { get; set; }                               // Neural Network
        public int Id { get; set; }                                         // Id
        public RTNEATManager Manager { get; set; }                          // RT NEAT Manager

        public static List<Creature> _currentCreatures = null;              // Current Creature List

        private Rigidbody2D _rb;                                            // RigidBody2D

        public float CurrentFitness { get; set; }
        public float PlantEaten { get; set; }
        public float CreatureBitten { get; set; }
        private float _clock = 0f;

        private float _viewAngle = 120f;
        private float _viewDistance = 10f;

        public SpriteRenderer _spriteRenderer;

        void Start()
        {
            if (_currentCreatures == null)
                _currentCreatures = new List<Creature>();
            _currentCreatures.Add(this);
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            CurrentFitness = 0;
            PlantEaten = 0;
            CreatureBitten = 0;
        }

        private void Update()
        {
            float time = Time.deltaTime;
            if (_clock >= _energyInterval)
            {
                _energy--;
                if (_energy <= 0)
                    Die();
                _clock = 0f;
            }
            float[] inputs = Eye();
            inputs = inputs.Concat(new float[] { _energy }).ToArray();
            Brain(inputs);
            _clock += time;
            NN.Genome.Fitness += time;
            CurrentFitness = NN.Genome.Fitness;
        }

        void OnMouseDown()
        {
            if (Manager.CameraMovements.Follow != transform)
            {
                FollowCreature();
            }
            else
            {
                Manager.CameraMovements.Follow = null;
                Manager.CameraMovements.Creature = null;
            }

        }

        public void FollowCreature()
        {
            if (Manager.CameraMovements.Follow != null)
                Manager.CameraMovements.Follow.GetComponent<Creature>().Draw(false);

            if (Manager.CameraMovements.Follow != transform)
            {
                Manager.CameraMovements.Follow = transform;
                Manager.CameraMovements.Creature = this;
                Manager.CameraMovements._brainGraph.ClearGraph();
                Manager.CameraMovements._brainGraph.SetNeuralNetwork(NN);
                Manager.CameraMovements._brainGraph.CreateGraph();
                Draw(true);
            }
        }

        public void Draw(bool draw)
        {
        }

        private void Die()
        {
            _currentCreatures.Remove(this);
            Manager.AddDeadCreature(this);
            Destroy(gameObject);
        }

        public void Eat(float energy, bool bite = false)
        {
            _energy += energy;

            if (bite)
                CreatureBitten += 1;
            else
                PlantEaten += 1;
        }

        private void Brain(float[] inputs)
        {
            float[] outputs = NN.FeedForward(inputs);
            Move(outputs[0], outputs[1]);
        }

        private void Move(float turning, float accel)
        {
            transform.Rotate(0f, 0f, turning * _turningForce);
            _rb.velocity = (Vector2)transform.up * accel * _accelForce;
        }

        private float[] Eye()
        {
            int foodNumber = 0;
            int creatureNumber = 0;
            int wallNumber = 0;
            float closestFoodDistance = 0f;
            float closestCreatureDistance = 0f;
            float closestWallDistance = 0f;
            float closestFoodAngle = 0f;
            float closestCreatureAngle = 0f;
            float creatureRedLevel = 0f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _viewDistance, _sensorMask);

            if (hits.Length > 0)
            {
                // Get limit Vector dir
                Vector3 limitDir = Quaternion.Euler(0f, 0f, _viewAngle / 2) * transform.up;

                foreach (Collider2D hit in hits)
                {
                    if (hit.transform != transform.parent)
                    {
                        Vector3 hitVectorDirection = hit.transform.position - transform.position;

                        // Get angle between up & hit position
                        float angle = Vector3.SignedAngle(transform.up, hitVectorDirection, Vector3.forward);

                        // If in View Angle
                        if (Math.Abs(angle) <= _viewAngle / 2)
                        {
                            Debug.DrawRay(transform.position, hitVectorDirection, Color.white);

                            float distance = Vector2.Distance(transform.position, hit.transform.position);

                            switch (hit.transform.parent.tag)
                            {
                                case "Food":
                                    foodNumber += 1;
                                    if (closestFoodDistance == 0f || distance < closestFoodDistance)
                                    {
                                        closestFoodDistance = distance;
                                        closestFoodAngle = angle;
                                    }
                                    break;
                                case "Creature":
                                    creatureNumber += 1;
                                    if (closestCreatureDistance == 0f || distance < closestCreatureDistance)
                                    {
                                        float green = hit.transform.parent.GetComponent<Creature>()._spriteRenderer.color.g;
                                        closestCreatureDistance = distance;
                                        closestCreatureAngle = angle;
                                        creatureRedLevel = 1f - green;
                                    }
                                    break;
                            }

                        }
                    }
                }
            }

            // Wall Detection
            RaycastHit2D wallHit = Physics2D.Raycast(transform.position, transform.up, _viewDistance, _wallSensorMask);
            if (wallHit)
            {
                Vector3 wallHitPos = wallHit.point;
                Debug.DrawRay(transform.position, wallHitPos - transform.position, Color.white);
                closestWallDistance = wallHit.distance;
                wallNumber++;
            }



            // Results
            float[] vision = { foodNumber, closestFoodDistance, closestFoodAngle, creatureNumber, closestCreatureDistance, closestCreatureAngle, /*creatureRedLevel,*/ closestWallDistance, wallNumber };
            //Debug.Log(closestWallDistance + " | " + wallNumber);
            return vision;
        }

        public float Bitten()
        {
            _energy -= _bitten;
            return _bitten;
        }

        public void AddRedColor()
        {
            float minus = _spriteRenderer.color.g - .01f >= 0 ? .01f : 0f;
            Color color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g - minus, _spriteRenderer.color.b - minus);
            _spriteRenderer.color = color;
        }

        public void RemoveRedColor()
        {
            float add = _spriteRenderer.color.g + .04f >= 1 ? .04f : 0f;
            Color color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g + add, _spriteRenderer.color.b + add);
            _spriteRenderer.color = color;
        }
    }
}
