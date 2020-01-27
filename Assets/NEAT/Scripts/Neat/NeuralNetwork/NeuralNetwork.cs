using System.Collections.Generic;

namespace NEAT
{
    public class NeuralNetwork
    {
        public float Fitness { get; set; }                                                      // Fitness of the NN
        public Genome Genome { get; set; }                                                      // Genome of the NN

        private Dictionary<int, Neuron> _neurons;                                               // Neurons of the genome by Ids
        private List<int> _inputIds;                                                            // Ids of Input Neurons
        private List<int> _outputIds;                                                           // Ids of Output Neurons
        private List<Neuron> _unprocessedNeurons;                                               // List of unprocessed Neurons
        private int _timeOut;                                                                   // Time Out
        private bool _bias;                                                                     // Bias
        private Neuron.ActivationType _activationType;                                          // ActivationType

        public NeuralNetwork(Genome genome, Neuron.ActivationType activationType = Neuron.ActivationType.SIGMOID, bool bias = true, int timeOut = 1000)
        {
            _activationType = activationType;
            _bias = bias;
            _timeOut = timeOut;
            _inputIds = new List<int>();
            _outputIds = new List<int>();
            _neurons = new Dictionary<int, Neuron>();
            _unprocessedNeurons = new List<Neuron>();
            Genome = genome;
            Fitness = 0f;

            // Fill Neurons, Input & Outputs Ids w/ Nodes in Genome
            foreach (int nodeId in genome.Nodes.Keys)
            {
                NodeGene node = genome.Nodes[nodeId];
                Neuron neuron = null;

                // Input & Output Node
                if (node.Type == NodeGene.TYPE.INPUT)
                {
                    neuron = new Neuron(Neuron.NeuronType.INPUT, _activationType, false); // No bias on Input Node
                    neuron.AddInputConnection();
                    _inputIds.Add(nodeId);
                }
                else if (node.Type == NodeGene.TYPE.OUTPUT)
                {
                    neuron = new Neuron(Neuron.NeuronType.OUTPUT, _activationType, _bias);
                    _outputIds.Add(nodeId);
                }
                else
                {
                    neuron = new Neuron(Neuron.NeuronType.HIDDEN, _activationType, _bias);
                }

                // Add Neuron to the Network
                _neurons.Add(nodeId, neuron);
            }
            _inputIds.Sort();
            _outputIds.Sort();

            // Add Genome Connections to Neurons
            foreach (int connectionId in genome.Connections.Keys)
            {
                ConnectionGene connection = genome.Connections[connectionId];

                // Ignore Disabled Nodes
                if (!connection.IsEnable)
                    continue;

                // Add Output Connection to the Neuron of the emitting Node
                Neuron emittingNeuron = _neurons[connection.InNode];
                emittingNeuron.AddOutputConnection(connection.OutNode, connection.Weight);

                // Add Input Connection to the Neuron of the receiving Node
                Neuron receivingNeuron = _neurons[connection.OutNode];
                receivingNeuron.AddInputConnection();
            }
        }

        /// <summary>
        /// Feed Forward Neural Network
        /// </summary>
        /// <param name="inputs">Inputs Node</param>
        /// <returns>Outputs Node if success, null if error</returns>
        public float[] FeedForward(float[] inputs)
        {
            if (inputs.Length != _inputIds.Count)
                return null;

            // Reset Neurons & unprocessed Neurons
            foreach (int key in _neurons.Keys)
                _neurons[key].Reset();
            _unprocessedNeurons.Clear();

            // Add all Neurons to unprocessed Neurons
            _unprocessedNeurons.AddRange(_neurons.Values);

            // Feeding the Inputs Neuron & Inputs Neuron Receivers 
            for (int i = 0; i < inputs.Length; i++)
            {
                // Calculate Inputs Neuton because we already have the input value
                Neuron inputNeuron = _neurons[_inputIds[i]];
                inputNeuron.FeedInput(inputs[i]);
                inputNeuron.Calculate();

                // Feed Inputs Neuron Receivers
                for (int k = 0; k < inputNeuron.OutputIds.Length; k++)
                {
                    // Add the Input to the next Neuron w/ correct Weight for the Connection
                    Neuron receiver = _neurons[inputNeuron.OutputIds[k]];
                    receiver.FeedInput(inputNeuron.Output * inputNeuron.OutputWeights[k]);
                }
                _unprocessedNeurons.Remove(inputNeuron);
            }

            for (int timeout = 0; _unprocessedNeurons.Count > 0; ++timeout)
            {
                // Timeout / Can't solve the Network => return null
                if (timeout > _timeOut)
                    return null;
                    
                for (int i = 0; i < _unprocessedNeurons.Count; ++i)
                {
                    Neuron neuron = _unprocessedNeurons[i];

                    // If Neuron is ready => Calculate & Feed
                    if (neuron.IsReady())
                    {
                        neuron.Calculate();
                        for (int y = 0; y < neuron.OutputIds.Length; y++)
                        {
                            int receiverId = neuron.OutputIds[y];
                            float receiverValue = neuron.Output * neuron.OutputWeights[y];
                            _neurons[receiverId].FeedInput(receiverValue);
                        }
                        _unprocessedNeurons.RemoveAt(i);
                    }
                }
            }

            // Copy Output from Outputs Neuron to an array to return
            float[] outputs = new float[_outputIds.Count];
            _outputIds.Sort();
            for (int i = 0; i < _outputIds.Count; i++)
                outputs[i] = _neurons[_outputIds[i]].Output;

            return outputs;
        }

        
        public override string ToString()
        {
            string str = "";

            foreach (int neuronId in _neurons.Keys)
            {
                // Neuron
                string type = " | HIDDEN";
                if (_inputIds.Contains(neuronId))
                    type = " | INPUT";
                else if (_outputIds.Contains(neuronId))
                    type = " | OUTPUT";

                str += "[ " + neuronId + type + " ]\n";

                // Connections
                Neuron neuron = _neurons[neuronId];
                for (int i = 0; i < neuron.OutputIds.Length; ++i)
                    str += "==>" + neuron.OutputIds[i] + ", weight: " + neuron.OutputWeights[i] + "\n";
                str += "\n";
            }
            return str;
        }
    }
}