using System;
using UnityEngine;

namespace NEAT.Test
{
    public class NeatTest : MonoBehaviour
    {
        public class EvalutorTestEvaluator : Evaluator
        {
            public EvalutorTestEvaluator(NEATConfig config, Genome startingGenome, InnovationCounter nodeInnovation, InnovationCounter connectionInnovation) : base(config, startingGenome, nodeInnovation, connectionInnovation) { }

            protected override float EvaluateGenome(Genome genome)
            {
                float weightSum = 0f;
                foreach (ConnectionGene cg in genome.Connections.Values)
                {
                    if (cg.IsEnable)
                        weightSum += Math.Abs(cg.Weight);
                }
                float difference = Math.Abs(weightSum - 100f);
                if (difference == 0)
                    difference = 0.001f;
                return (1000f / difference);
            }
        }

        private InnovationCounter _nodeInnovation = new InnovationCounter();
        private InnovationCounter _connectionInnovation = new InnovationCounter();
        private EvalutorTestEvaluator _eval = null;
        private bool _evaluatorTestFlag = true;
        public int _generationNumber = 20;
        public NEATConfig _config;

        void Start()
        {
            CrossoverTest();
            EvaluatorTestInit();
        }

        void Update()
        {
            if (_evaluatorTestFlag)
            {
                for (int i = 0; i < _generationNumber; i++)
                {
                    Debug.Log(EvaluatorTestUpdate(i));
                    if (i == (_generationNumber - 1))
                        _evaluatorTestFlag = false;
                }
            }
        }

        public void CrossoverTest()
        {
            // Parent 1
            Genome parent1 = new Genome();

            for (int i = 0; i < 3; i++)
            {
                NodeGene node = new NodeGene(NodeGene.TYPE.INPUT, i + 1);
                parent1.AddNodeGene(node);
            }

            parent1.AddNodeGene(new NodeGene(NodeGene.TYPE.OUTPUT, 4));
            parent1.AddNodeGene(new NodeGene(NodeGene.TYPE.HIDDEN, 5));

            parent1.AddConnectionGene(new ConnectionGene(1, 4, 1f, true, 1));
            parent1.AddConnectionGene(new ConnectionGene(2, 4, 1f, false, 2));
            parent1.AddConnectionGene(new ConnectionGene(3, 4, 1f, true, 3));
            parent1.AddConnectionGene(new ConnectionGene(2, 5, 1f, true, 4));
            parent1.AddConnectionGene(new ConnectionGene(5, 4, 1f, true, 5));
            parent1.AddConnectionGene(new ConnectionGene(1, 5, 1f, true, 8));

            // Parent 2 (More Fit)
            Genome parent2 = new Genome();
            for (int i = 0; i < 3; i++)
            {
                NodeGene node = new NodeGene(NodeGene.TYPE.INPUT, i + 1);
                parent2.AddNodeGene(node);
            }
            parent2.AddNodeGene(new NodeGene(NodeGene.TYPE.OUTPUT, 4));
            parent2.AddNodeGene(new NodeGene(NodeGene.TYPE.HIDDEN, 5));
            parent2.AddNodeGene(new NodeGene(NodeGene.TYPE.HIDDEN, 6));

            parent2.AddConnectionGene(new ConnectionGene(1, 4, 1f, true, 1));
            parent2.AddConnectionGene(new ConnectionGene(2, 4, 1f, false, 2));
            parent2.AddConnectionGene(new ConnectionGene(3, 4, 1f, true, 3));
            parent2.AddConnectionGene(new ConnectionGene(2, 5, 1f, true, 4));
            parent2.AddConnectionGene(new ConnectionGene(5, 4, 1f, false, 5));
            parent2.AddConnectionGene(new ConnectionGene(5, 6, 1f, true, 6));
            parent2.AddConnectionGene(new ConnectionGene(6, 4, 1f, true, 7));
            parent2.AddConnectionGene(new ConnectionGene(3, 5, 1f, true, 9));
            parent2.AddConnectionGene(new ConnectionGene(1, 6, 1f, true, 10));

            // Crossover
            Genome child = Genome.Crossover(parent2, parent1, new System.Random(), 0.75f);

            // Return Results
            Debug.Log(String.Format("PARENT 1\n{0}\n\nPARENT 2\n{1}\n\nCROSSOVER CHILD\n{2}", parent1.ToString(), parent2.ToString(), child.ToString()));
        }

        public void EvaluatorTestInit()
        {
            Genome genome = new Genome();
            int n1 = _nodeInnovation.GetNewInnovationNumber();
            int n2 = _nodeInnovation.GetNewInnovationNumber();
            int n3 = _nodeInnovation.GetNewInnovationNumber();
            genome.AddNodeGene(new NodeGene(NodeGene.TYPE.INPUT, n1));
            genome.AddNodeGene(new NodeGene(NodeGene.TYPE.INPUT, n2));
            genome.AddNodeGene(new NodeGene(NodeGene.TYPE.OUTPUT, n3));

            int c1 = _connectionInnovation.GetNewInnovationNumber();
            int c2 = _connectionInnovation.GetNewInnovationNumber();
            genome.AddConnectionGene(new ConnectionGene(n1, n3, 0.5f, true, c1));
            genome.AddConnectionGene(new ConnectionGene(n2, n3, 0.5f, true, c2));

            _eval = new EvalutorTestEvaluator(_config, genome, _nodeInnovation, _connectionInnovation);
        }

        public string EvaluatorTestUpdate(int genNb)
        {
            string logs = "";
            _eval.Evaluate();
            logs +=
                "Generation: " + (genNb + 1) +
                " | Best Fitness: " + _eval.BestFitness +
                " | Species Nb: " + _eval.GetSpeciesNumber() +
                " | Connections in best performer: " + _eval.BestGenome.Connections.Values.Count;

            float weightSum = 0;
            foreach (ConnectionGene cg in _eval.BestGenome.Connections.Values)
            {
                if (cg.IsEnable)
                    weightSum += Math.Abs(cg.Weight);
            }
            logs += " | Weight Sum: " + weightSum;

            if ((genNb + 1) % 5 == 0 || genNb == 0)
                logs += "\n\n" + _eval.BestGenome.ToString();

            logs += "\n\n===========================================\n\n";

            return logs;
        }
    }
}
