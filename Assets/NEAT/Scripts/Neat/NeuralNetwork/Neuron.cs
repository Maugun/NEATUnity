using System;

namespace NEAT
{
    public class Neuron
    {
        public enum ActivationType
        {
            SIGMOID,                    // Output Range [0, 1]
            TANH                        // Output Range [-1, 1]
        };

        public enum NeuronType
        {
            INPUT,
            HIDDEN,
            OUTPUT
        };

        public float Output { get; set; }
        private float?[] _inputs;
        public int[] OutputIds { get; set; }
        public float[] OutputWeights { get; set; }
        public bool Bias { get; set; }
        public ActivationType ActivationTypeValue { get; set; }
        public NeuronType NeuronTypeValue { get; set; }

        public Neuron(NeuronType neuronType = NeuronType.HIDDEN, ActivationType activationType = ActivationType.SIGMOID, bool bias = true)
        {
            NeuronTypeValue = neuronType;
            ActivationTypeValue = activationType;
            Bias = bias;
            _inputs = new float?[0];
            OutputIds = new int[0];
            OutputWeights = new float[0];
        }

        /// <summary>
        /// Add an Output Connection
        /// </summary>
        /// <param name="outputID">Output Id</param>
        /// <param name="weight">Weight</param>
        public void AddOutputConnection(int outputID, float weight)
        {
            // Add OutputId to the array
            int[] newOutputIDs = new int[OutputIds.Length + 1];
            for (int i = 0; i < OutputIds.Length; i++)
                newOutputIDs[i] = OutputIds[i];

            // Add new value at the end
            newOutputIDs[OutputIds.Length] = outputID;
            OutputIds = newOutputIDs;

            // Add Weight to the array
            float[] newOutputWeights = new float[OutputWeights.Length + 1];
            for (int i = 0; i < OutputWeights.Length; i++)
                newOutputWeights[i] = OutputWeights[i];

            // Add new value at the end
            newOutputWeights[OutputWeights.Length] = weight;
            OutputWeights = newOutputWeights;
        }

        /// <summary>
        /// Add an Input Connection
        /// </summary>
        public void AddInputConnection()
        {
            float?[] newInputs = new float?[_inputs.Length + 1];
            for (int i = 0; i < newInputs.Length; i++)
                newInputs[i] = null;
            _inputs = newInputs;
        }

        /// <summary>
        /// Calculate the Neuron Output value
        /// </summary>
        /// <returns></returns>
        public float Calculate()
        {
            // Get the Sum of Inputs Weights
            float sum = 0f;
            foreach (float? input in _inputs)
                sum += input.Value;

            // Activation
            if (NeuronTypeValue == NeuronType.INPUT)
            {
                Output = sum;
            }
            else
            {
                // Bias
                if (Bias)
                    sum += 1f;

                switch (ActivationTypeValue)
                {
                    case ActivationType.SIGMOID:
                        Output = SigmoidActivation(sum);
                        break;
                    case ActivationType.TANH:
                        Output = TanhActivation(sum);
                        break;
                }
            }

            return Output;
        }

        /// <summary>
        /// Is Neuron ready to fire ?
        /// </summary>
        /// <returns></returns>
        public bool IsReady()
        {
            // Check if all Inputs are not null
            bool isReady = true;
            foreach (float? input in _inputs)
            {
                if (input == null)
                {
                    isReady = false;
                    break;
                }
            }
            return isReady;
        }

        /// <summary>
        /// Feed the first Input available 
        /// </summary>
        /// <param name="input"></param>
        /// <returns>True if success, False if no free Input</returns>
        public bool FeedInput(float input)
        {
            bool success = false;
            for (int i = 0; i < _inputs.Length; i++)
            {
                if (_inputs[i] == null)
                {
                    _inputs[i] = input;
                    success = true;
                    break;
                }
            }
            return success;
        }

        /// <summary>
        /// Reset Inputs & Output value
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < _inputs.Length; i++)
                _inputs[i] = null;
            Output = 0f;
        }

        /// <summary>
        /// Sigmoid Activation
        /// </summary>
        /// <param name="inputsSum">Sum of inputs</param>
        /// <returns></returns>
        private float SigmoidActivation(float inputsSum)
        {
            return (1f / (1f + (float)Math.Exp(/*-4.9d * */inputsSum)));
        }

        /// <summary>
        /// Tanh Activation
        /// </summary>
        /// <param name="inputsSum">Sum of inputs</param>
        /// <returns></returns>
        private float TanhActivation(float inputsSum)
        {
            return (float)Math.Tanh(inputsSum);
        }
    }
}