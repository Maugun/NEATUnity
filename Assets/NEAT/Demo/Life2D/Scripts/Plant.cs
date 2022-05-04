using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEAT.Demo.Life2D
{
    public class Plant : MonoBehaviour
    {
        [Header("Config")]
        public int _energy = 10;                                            // Energy that the Plant will give
        public string _layerHitName = "Creature";                           // Layer Hit Name

        public PlantSpawner PlantSpawner { get; set; }
        //public int Id { get; set; }

        //private void OnCollisionEnter2D(Collision2D other)
        //{
        //    if (other.gameObject.layer == LayerMask.NameToLayer(_layerHitName))     // If the object is a creature
        //    {
        //        Eaten();
        //    }
        //}

        public void Eaten()
        {
            PlantSpawner.AddDead();                                             // Tell the Spawner to spawn a new Plant
            GameObject.Destroy(gameObject);                                     // Destroy the Plant
        }
    }
}
