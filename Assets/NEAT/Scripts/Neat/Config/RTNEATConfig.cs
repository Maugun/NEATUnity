using System.Collections.Generic;
using UnityEngine;

namespace NEAT
{
    [CreateAssetMenu()]
    public class RTNEATConfig : ScriptableObject
    {
        [Header("Basic configuration")]
        public bool addConnectionOnCreation = false;                                        // Add Connection between node on Genome Creation
        public GameObject creaturePrefab;                                                   // Creature Prefab
        public int startPopulation = 100;                                                   // Start Population Size
        public int maxPopulation = 200;                                                     // Max Population
        public int minPopulation = 50;                                                      // Min Population
        [Range(0, 100)] public int percentageToKeep = 30;                                   // Percentage to Keep

        [Header("Crossover")]
        public bool crossover = true;                                                       // Is Crossover on ?
        [Range(0f, 1f)] public float disabledConnectionInheritChance = 0.75f;               // Disabled Connection Inherit Chance

        [Header("Neural Network")]
        public bool bias = true;                                                            // Is Bias ON ?
        public List<int> hiddenNodeStartNumberByLayer = new List<int>();                    // Hidden Nodes Number at simulation Start by Layer
        public int outputNodeNumber = 2;                                                    // Output Nodes Number
        public Neuron.ActivationType activationType = Neuron.ActivationType.TANH;           // ActivationType
        public int timeOut = 1000;                                                          // Time Out
        public Vector2 newWeightRange = new Vector2(-0.5f, 0.5f);                           // New Weight Range Values

        [Header("Genome Mutations")]
        public bool genomeMutations = true;                                                 // Is Genome Mutation ON ?
        [Range(0f, 1f)] public float addConnectionRate = 0.05f;                             // Add a Connection Rate
        [Range(0f, 1f)] public float addNodeRate = 0.03f;                                   // Add a Node Rate
        [Range(0f, 1f)] public float enableDisableRate = 0.04f;                             // Enable Disbale Rate

        [Header("Weight Mutations")]
        [Range(0f, 1f)] public float mutationRate = 0.8f;                                   // Mutation Rate
        [Range(0f, 1f)] public float perturbingProbability = 0.9f;                          // Perturbing Probability => rest of this number is the probability of a new weight

        [Header("Species")]
        public bool species = false;                                                        // Is Species ON ?
        public float maxGenomeDistance = 3.0f;                                              // Max Genome Distance inside a Species
        public float disjointMutator = 1.0f;                                                // Disjoint Gene Mutator
        public float excessMutator = 1.0f;                                                  // Excess Gene Mutator
        public float avgDiffMutator = 0.4f;                                                 // Average Difference Mutator
    }
}
