using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NEAT.Demo.Life2D
{
    public class CameraMovements : MonoBehaviour
    {
        public Text _id;
        public Text _fitness;
        public Text _foodEaten;
        public Text _creatureBitten;
        public BrainGraph _brainGraph;

        public Transform Follow { get; set; }
        public Creature Creature { get; set; }

        void Start()
        {
            SetActive(false);
        }

        void Update()
        {
            if (Follow != null)
            {
                transform.position = new Vector3(Follow.position.x, Follow.position.y, transform.position.z);
                if (Creature != null)
                {
                    SetActive(true);
                    _id.text = "ID: " + Creature.Id;
                    _fitness.text = "F: " + Creature.CurrentFitness.ToString("0.00");
                    _foodEaten.text = "Eat: " + Creature.PlantEaten;
                    _creatureBitten.text = "Bite: " + Creature.CreatureBitten;
                }
                else
                {
                    SetActive(false);
                }
            }
            else
            {
                SetActive(false);
            }
        }

        void SetActive(bool active)
        {
            _id.gameObject.SetActive(active);
            _fitness.gameObject.SetActive(active);
            _foodEaten.gameObject.SetActive(active);
            _creatureBitten.gameObject.SetActive(active);
            _brainGraph.gameObject.SetActive(active);
        }
    }
}
