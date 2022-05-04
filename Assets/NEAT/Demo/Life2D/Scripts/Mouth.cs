using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEAT.Demo.Life2D
{
    public class Mouth : MonoBehaviour
    {
        [SerializeField]
        private Creature _creatureScript;                                                       // Creature Script

        void Start()
        {
            _creatureScript = transform.parent.GetComponent<Creature>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Plant plant = collision.transform.GetComponent<Plant>();
            if (plant != null)
            {
                _creatureScript.Eat(plant._energy);
                _creatureScript.RemoveRedColor();
                plant.Eaten();
            }
        }

        // private void OnTriggerStay2D(Collider2D collision)
        // {
        //     Creature creature = collision.transform.GetComponent<Creature>();
        //     if (creature != null)
        //     {
        //         _creatureScript.Eat(creature.Bitten(), true);
        //         _creatureScript.AddRedColor();
        //     }
        // }
    }
}
