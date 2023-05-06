using System;

namespace NEAT
{
    [System.Serializable]
    public class ConnectionGene
    {
        public int InNode { get; set; }
        public int OutNode { get; set; }
        public float Weight { get; set; }
        public bool IsEnable { get; set; }
        public int InnovationId { get; set; }

        public ConnectionGene() { }

        public ConnectionGene(int inNode, int outNode, float weight, bool enable, int innovationId)
        {
            InNode = inNode;
            OutNode = outNode;
            Weight = weight;
            IsEnable = enable;
            InnovationId = innovationId;
        }

        public ConnectionGene(ConnectionGene connection)
        {
            InNode = connection.InNode;
            OutNode = connection.OutNode;
            Weight = connection.Weight;
            IsEnable = connection.IsEnable;
            InnovationId = connection.InnovationId;
        }

        public override string ToString()
        {
            return String.Format("[{0} | {1}->{2} | w: {3} | {4}]", InnovationId, InNode, OutNode, Weight, IsEnable ? "Enable" : "Disable");
        }
    }
}
