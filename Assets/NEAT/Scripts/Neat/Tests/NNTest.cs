using UnityEngine;

namespace NEAT.Test
{
    public class NNTest
    {
        public NNTest() { }

        public static void Test1()
        {
            Genome genome = new Genome();
            NeuralNetwork net;
            float[] input;
            float[] output;
            string logs = "======================TEST 1===================================\n\n";

            genome.AddNodeGene(new NodeGene(NodeGene.TYPE.INPUT, 0));
            genome.AddNodeGene(new NodeGene(NodeGene.TYPE.OUTPUT, 1));
            genome.AddConnectionGene(new ConnectionGene(0, 1, 0.5f, true, 0));

            net = new NeuralNetwork(genome, Neuron.ActivationType.SIGMOID, false);
            input = new float[] { 1f };
            for (int i = 0; i < 3; i++)
            {
                output = net.FeedForward(input);
                logs += "output is of length=" + output.Length + " and has output[0]=" + output[0] + " expecting 0.9192\n";
            }

            Debug.Log(logs);
        }

        public static void Test2()
        {
            Genome genome = new Genome();
            NeuralNetwork net;
            float[] input;
            float[] output;
            string logs = "======================TEST 2===================================\n\n";

            genome.AddNodeGene(new NodeGene(NodeGene.TYPE.INPUT, 0));
            genome.AddNodeGene(new NodeGene(NodeGene.TYPE.OUTPUT, 1));
            genome.AddConnectionGene(new ConnectionGene(0, 1, 0.1f, true, 0));

            net = new NeuralNetwork(genome, Neuron.ActivationType.SIGMOID, false);
            input = new float[] { -0.5f };
            for (int i = 0; i < 3; i++)
            {
                output = net.FeedForward(input);
                logs += "output is of length=" + output.Length + " and has output[0]=" + output[0] + " expecting 0.50973\n";
            }

            Debug.Log(logs);
        }

        public static void Test3()
        {
            Genome genome = new Genome();
            NeuralNetwork net;
            float[] input;
            float[] output;
            string logs = "======================TEST 3===================================\n\n";

            genome = new Genome();
            genome.AddNodeGene(new NodeGene(NodeGene.TYPE.INPUT, 0));
            genome.AddNodeGene(new NodeGene(NodeGene.TYPE.OUTPUT, 1));
            genome.AddNodeGene(new NodeGene(NodeGene.TYPE.HIDDEN, 2));
            genome.AddConnectionGene(new ConnectionGene(0, 2, 0.4f, true, 0));
            genome.AddConnectionGene(new ConnectionGene(2, 1, 0.7f, true, 1));

            net = new NeuralNetwork(genome, Neuron.ActivationType.SIGMOID, false);
            input = new float[] { 0.9f };
            for (int i = 0; i < 3; i++)
            {
                output = net.FeedForward(input);
                logs += "output is of length=" + output.Length + " and has output[0]=" + output[0] + " expecting 0.9524\n";
            }

            Debug.Log(logs);
        }

        public static void Test4()
        {
            Genome genome = new Genome();
            NeuralNetwork net;
            float[] input;
            float[] output;
            string logs = "======================TEST 4===================================\n\n";

            genome = new Genome();
            genome.AddNodeGene(new NodeGene(NodeGene.TYPE.INPUT, 0));
            genome.AddNodeGene(new NodeGene(NodeGene.TYPE.INPUT, 1));
            genome.AddNodeGene(new NodeGene(NodeGene.TYPE.INPUT, 2));
            genome.AddNodeGene(new NodeGene(NodeGene.TYPE.OUTPUT, 3));
            genome.AddNodeGene(new NodeGene(NodeGene.TYPE.HIDDEN, 4));
            genome.AddConnectionGene(new ConnectionGene(0, 4, 0.4f, true, 0));
            genome.AddConnectionGene(new ConnectionGene(1, 4, 0.7f, true, 1));
            genome.AddConnectionGene(new ConnectionGene(2, 4, 0.1f, true, 2));
            genome.AddConnectionGene(new ConnectionGene(4, 3, 1f, true, 3));

            net = new NeuralNetwork(genome, Neuron.ActivationType.SIGMOID, false);
            input = new float[] { 0.5f, 0.75f, 0.90f };
            for (int i = 0; i < 3; i++)
            {
                output = net.FeedForward(input);
                logs += "output is of length=" + output.Length + " and has output[0]=" + output[0] + " expecting 0.9924\n";
            }

            Debug.Log(logs);
        }
    }
}
