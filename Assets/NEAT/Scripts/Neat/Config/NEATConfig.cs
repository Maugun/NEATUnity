using System.Collections.Generic;
using UnityEngine;

namespace NEAT
{
    [CreateAssetMenu()]
    public class NEATConfig : ScriptableObject
    {
        [Header("Basic configuration")]
        public bool _addConnectionOnCreation = false;                                       // Add Connection between node on Genome Creation
        public GameObject _creaturePrefab;                                                  // Creature Prefab
        public int _populationSize = 150;                                                   // Population Size
        [Range(0, 100)] public int _percentageToKeep = 30;                                  // Percentage to Keep

        [Header("Crossover")]
        public bool _crossover = true;                                                      // Is Crossover ON ?
        [Range(0f, 1f)] public float _disabledConnectionInheritChance = 0.75f;              // Disabled Connection Inherit Chance

        [Header("Neural Network")]
        public bool _bias = true;                                                           // Is Bias ON ?
        public int _inputNodeNumber = 6;                                                    // Input Nodes Number
        public List<int> _hiddenNodeStartNumberByLayer = new List<int>();                   // Hidden Nodes Number at simulation Start by Layer
        public int _outputNodeNumber = 2;                                                   // Output Nodes Number
        public Neuron.ActivationType _activationType = Neuron.ActivationType.SIGMOID;       // ActivationType
        public int _timeOut = 1000;                                                         // Time Out
        public Vector2 _newWeightRange = new Vector2(-0.5f, 0.5f);                          // New Weight Range Values

        [Header("Genome Mutations")]
        public bool _genomeMutations = true;                                                // Is Genome Mutation ON ?
        [Range(0f, 1f)] public float _addConnectionRate = 0.05f;                            // Add a Connection Rate
        [Range(0f, 1f)] public float _addNodeRate = 0.03f;                                  // Add a Node Rate
        [Range(0f, 1f)] public float _enableDisableRate = 0.04f;                            // Enable Disbale Rate

        [Header("Weight Mutations")]
        [Range(0f, 1f)] public float _mutationRate = 0.8f;                                  // Mutation Rate
        [Range(0f, 1f)] public float _perturbingProbability = 0.9f;                         // Perturbing Probability => rest of this number is the probability of a new weight

        [Header("Species")]
        public bool _species = true;                                                        // Is Species ON ?
        public float _maxGenomeDistance = 3.0f;                                             // Max Genome Distance inside a Species
        public float _disjointMutator = 1.0f;                                               // Disjoint Gene Mutator
        public float _excessMutator = 1.0f;                                                 // Excess Gene Mutator
        public float _avgDiffMutator = 0.4f;                                                // Average Difference Mutator
    }
}
