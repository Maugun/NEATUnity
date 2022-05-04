using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEAT.Demo.Life2D
{
    public class PlantSpawner : MonoBehaviour
    {
        [Header("Prefab")]
        public GameObject _ground;                                      // Ground Prefab
        public GameObject _plantPrefab;                                 // Prefab to Spawn
        public float _scale = 0.2f;                                     // Scale of the Prefab

        [Header("Spawner")]
        public int _maxPop = 500;                                       // Max Population Number
        public float _spawnInterval = 0.1f;                             // Spawn Interval

        public int CurrentPop { get; set; }                             // Current Population
        private float _size;                                            // Spawn cube size
        private float _clock = 0f;

        void Start()
        {
            CurrentPop = 0;
            _size = _ground.transform.localScale.x - .5f;
            while (CurrentPop < _maxPop)
                Spawn();
        }

        void Update()
        {
            if (CurrentPop < _maxPop && (_clock >= _spawnInterval))
            {
                Spawn();
                _clock = 0f;
            }
            _clock += Time.deltaTime;
        }

        public void Spawn()
        {
            Vector2 spawnPosition = GetSpawnPosition();
            GameObject plant = Instantiate(_plantPrefab, spawnPosition, Quaternion.identity, transform);
            plant.transform.localScale = new Vector3(_scale, _scale, 1);
            plant.GetComponent<Plant>().PlantSpawner = this;
            CurrentPop += 1;
        }

        private Vector2 GetSpawnPosition()
        {
            Vector2 randPos = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            return (randPos * _size / 2) + (Vector2)transform.position;
        }

        public void AddDead()
        {
            CurrentPop -= 1;
        }
    }
}
