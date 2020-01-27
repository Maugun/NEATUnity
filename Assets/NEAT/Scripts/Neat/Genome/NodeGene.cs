using System;

namespace NEAT
{
    public class NodeGene
    {
        public enum TYPE
        {
            INPUT,
            HIDDEN,
            OUTPUT
        }

        public TYPE Type { get; set; }
        public int Id { get; set; }

        public NodeGene(TYPE type, int id)
        {
            Type = type;
            Id = id;
        }

        public NodeGene(NodeGene node)
        {
            Type = node.Type;
            Id = node.Id;
        }

        public override string ToString()
        {
            string type = "INPUT";
            if (Type == TYPE.HIDDEN)
                type = "HIDDEN";
            if (Type == TYPE.OUTPUT)
                type = "OUTPUT";

            return String.Format("[{0} | {1}]", Id, type);
        }
    }
}
