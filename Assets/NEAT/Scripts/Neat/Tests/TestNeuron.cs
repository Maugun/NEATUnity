using UnityEngine;

namespace NEAT.Test
{
    public class TestNeuron
    {
        public TestNeuron() { }

        public static void Test()
        {
            string logs = "";

            logs += "========TEST 1========\n\n";
            Neuron test1 = new Neuron(Neuron.NeuronType.HIDDEN, Neuron.ActivationType.SIGMOID, false);
            test1.AddInputConnection();
            test1.AddInputConnection();
            test1.AddInputConnection();

            test1.FeedInput(1f);
            logs += "Neuron reports that isReady()=" + test1.IsReady() + ", and is expected to report that isReady()=false\n";
            test1.FeedInput(1f);
            logs += "Neuron reports that isReady()=" + test1.IsReady() + ", and is expected to report that isReady()=false\n";
            test1.FeedInput(1f);
            logs += "Neuron reports that isReady()=" + test1.IsReady() + ", and is expected to report that isReady()=true\n";
            logs += "Sum=3\n";

            logs += "Calculating...\n";
            float output = test1.Calculate();
            logs += "Output: " + output + "\n\n";

            logs += "========TEST 2========\n\n";
            Neuron test2 = new Neuron(Neuron.NeuronType.HIDDEN, Neuron.ActivationType.SIGMOID, false);
            test2.AddInputConnection();
            test2.AddInputConnection();
            test2.AddInputConnection();

            test2.FeedInput(0f);
            test2.FeedInput(0.5f);
            test2.FeedInput(-0.5f);
            logs += "Sum=0\n";

            logs += "Calculating...\n";
            output = test2.Calculate();
            logs += "Output: " + output + "\n\n";

            logs += "========TEST 3========\n\n";
            Neuron test3 = new Neuron(Neuron.NeuronType.HIDDEN, Neuron.ActivationType.SIGMOID, false);
            test3.AddInputConnection();
            test3.AddInputConnection();
            test3.AddInputConnection();

            test3.FeedInput(-2f);
            test3.FeedInput(-2f);
            test3.FeedInput(-2f);
            logs += "Sum=-6\n";

            logs += "Calculating...\n";
            output = test3.Calculate();
            logs += "Output: " + output + "\n\n";

            logs += "========TEST 4========\n\n";
            Neuron test4 = new Neuron(Neuron.NeuronType.HIDDEN, Neuron.ActivationType.SIGMOID, false);
            test4.AddInputConnection();
            test4.AddInputConnection();
            test4.AddInputConnection();

            test4.FeedInput(-20f);
            test4.FeedInput(-20f);
            test4.FeedInput(-20f);
            logs += "Sum=-60\n";

            logs += "Calculating...\n";
            output = test4.Calculate();
            logs += "Output: " + output + "\n\n";

            Debug.Log(logs);
        }
    }
}