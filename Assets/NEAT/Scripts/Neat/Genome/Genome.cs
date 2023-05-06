using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Random = System.Random;

namespace NEAT
{
    [System.Serializable]
    public class Genome
    {
        public static int genomeIdCounter = 0;                                                      // Genome Id Counter

        public Dictionary<int, NodeGene> Nodes { get; set; }                                        // Nodes Dictionary
        public Dictionary<int, ConnectionGene> Connections { get; set; }                            // Connections Dictionary
        [JsonIgnore] public float Fitness { get; set; }                                             // Genome Fitness
        [JsonIgnore] public int Id { get; set; }                                                    // Genome Id
        [JsonIgnore] public Vector2 NewWeightRange { get; set; }                                    // New Weight Range Values
        public float PerturbingProbability { get; set; }                                            // Perturbing Probability => rest of this number is the probability of a new weight

        /* JSON */
        public float _newWeightRangeX;                                                               // Newtonsoft.Json can't serialize Vector2
        public float _newWeightRangeY;                                                               // Newtonsoft.Json can't serialize Vector2
        public int _nodeInnovation;
        public int _connectionInnovation;

        public Genome() { }

        public Genome(Vector2 newWeightRange = default(Vector2), float perturbingProbability = 0.9f)
        {
            NewWeightRange = newWeightRange;
            PerturbingProbability = perturbingProbability;
            Nodes = new Dictionary<int, NodeGene>();
            Connections = new Dictionary<int, ConnectionGene>();
            Fitness = 0f;
            Id = Genome.genomeIdCounter;
            Genome.genomeIdCounter++;
        }

        public Genome(Genome genome)
        {
            NewWeightRange = genome.NewWeightRange;
            PerturbingProbability = genome.PerturbingProbability;
            Nodes = new Dictionary<int, NodeGene>();
            Connections = new Dictionary<int, ConnectionGene>();
            Fitness = genome.Fitness;
            Id = Genome.genomeIdCounter;
            Genome.genomeIdCounter++;

            // Nodes Deep Copy
            foreach (int nodeId in genome.Nodes.Keys)
                Nodes.Add(nodeId, new NodeGene(genome.Nodes[nodeId]));

            // Connections Deep Copy
            foreach (int connectionId in genome.Connections.Keys)
                Connections.Add(connectionId, new ConnectionGene(genome.Connections[connectionId]));
        }

        public static Genome GenomeRandomWeights(Genome genome, Random r)
        {
            Genome newGenome = new Genome(genome);

            foreach (ConnectionGene connection in newGenome.Connections.Values)
                connection.Weight = UnityEngine.Random.Range(genome.NewWeightRange.x, genome.NewWeightRange.y);

            return newGenome;
        }

        #region MUTATION =================================================================================================#
        /// <summary>
        /// Mutate Weights
        /// </summary>
        /// <param name="r">Random</param>
        public void WeightsMutation(Random r)
        {
            foreach (ConnectionGene connection in Connections.Values)
            {
                float weight = connection.Weight;
                float randomWeight = UnityEngine.Random.Range(NewWeightRange.x, NewWeightRange.y);

                weight = (float)r.NextDouble() < PerturbingProbability ? weight * randomWeight : randomWeight;
                connection.Weight = weight;
            }
        }

        /// <summary>
        /// Add a new Connection between 2 Random Nodes
        /// </summary>
        /// <param name="r">Random</param>
        /// <param name="connectionInnovationCounter">Connection Innovation Counter</param>
        /// <param name="maxTries">Max Tries to attemp Mutation</param>
        /// <returns>Success</returns>
        public bool AddConnectionMutation(Random r, InnovationCounter connectionInnovationCounter, int maxTries = 10)
        {
            bool success = false;

            // Try maxTries times
            for (int tries = 0; tries < maxTries && !success; tries++)
            {
                // Get 2 Random Nodes & a Random Weight
                List<int> nodeInnovationIds = new List<int>(Nodes.Keys);
                NodeGene node1 = Nodes[nodeInnovationIds[r.Next(nodeInnovationIds.Count)]];
                NodeGene node2 = Nodes[nodeInnovationIds[r.Next(nodeInnovationIds.Count)]];

                // Sigmoid Weights
                // float weight = (float)r.NextDouble() * 2f - 1f;

                // Tanh Weights
                // float weight = UnityEngine.Random.Range(-0.5f, 0.5f);

                // Config Weights
                float weight = UnityEngine.Random.Range(NewWeightRange.x, NewWeightRange.y);

                // Is Reversed ?
                if ((node1.Type == NodeGene.TYPE.HIDDEN && node2.Type == NodeGene.TYPE.INPUT) ||
                    (node1.Type == NodeGene.TYPE.OUTPUT && node2.Type == NodeGene.TYPE.HIDDEN) ||
                    (node1.Type == NodeGene.TYPE.OUTPUT && node2.Type == NodeGene.TYPE.INPUT))
                {
                    NodeGene swap = node1;
                    node1 = node2;
                    node2 = swap;
                }

                // Is Connection Impossible ?
                if ((node1.Type == NodeGene.TYPE.INPUT && node2.Type == NodeGene.TYPE.INPUT) ||
                    (node1.Type == NodeGene.TYPE.OUTPUT && node2.Type == NodeGene.TYPE.OUTPUT) ||
                    (node1 == node2))
                    continue;

                // Does Connection already exists ?
                bool connectionExists = false;
                foreach (ConnectionGene c in Connections.Values)
                {
                    if ((c.InNode == node1.Id && c.OutNode == node2.Id) ||
                        (c.InNode == node2.Id && c.OutNode == node1.Id))
                    {
                        connectionExists = true;
                        break;
                    }
                }
                if (connectionExists) continue;

                // Circular structure ?
                bool connectionImpossible = false;
                List<int> nodeIds = new List<int>();                        // Node that need output from node2
                List<int> nodeToCheck = new List<int>();                    // Nodes to check
                foreach (int connectionId in Connections.Keys)              // Get all Connections w/ outNode == node2
                {
                    ConnectionGene connection = Connections[connectionId];
                    if (connection.InNode == node2.Id)                      // Connection comes from node2
                    {
                        nodeIds.Add(connection.OutNode);
                        nodeToCheck.Add(connection.OutNode);
                    }
                }
                while (nodeToCheck.Count > 0)                               // Get all Connections w/ outNode == nodeToCheck
                {
                    int nodeId = nodeToCheck[0];
                    foreach (int connectionId in Connections.Keys)
                    {
                        ConnectionGene connection = Connections[connectionId];
                        if (connection.InNode == nodeId)                    // Connection comes from nodeToCheck
                        {
                            nodeIds.Add(connection.OutNode);
                            nodeToCheck.Add(connection.OutNode);
                        }
                    }
                    nodeToCheck.RemoveAt(0);
                }
                foreach (int i in nodeIds)                                  // Loop through all Node Ids
                {
                    if (i == node1.Id)                                      // If one of the Node == node1 => Connection is Impossible
                    {
                        connectionImpossible = true;
                        break;
                    }
                }
                if (connectionImpossible) continue;

                // Add new Connection
                ConnectionGene newConnection = new ConnectionGene(
                    node1.Id,
                    node2.Id,
                    weight,
                    true,
                    connectionInnovationCounter.GetNewInnovationNumber()
                );
                Connections.Add(newConnection.InnovationId, newConnection);
                success = true;
            }

            return success;
        }

        /// <summary>
        /// Add a new Node to a Random Connection
        /// </summary>
        /// <param name="r">Random</param>
        /// <param name="connectionInnovationCounter">Connection Innovation Counter</param>
        /// <param name="nodeInnovationCounter">Node Innovation Counter</param>
        public void AddNodeMutation(Random r, InnovationCounter connectionInnovationCounter, InnovationCounter nodeInnovationCounter)
        {
            // Get enabled Connections
            List<ConnectionGene> enabledConnections = new List<ConnectionGene>();
            foreach (ConnectionGene c in Connections.Values)
            {
                if (c.IsEnable) enabledConnections.Add(c);
            }
            if (enabledConnections.Count == 0) return;

            // Get a Random Connection
            ConnectionGene connection = enabledConnections[r.Next(enabledConnections.Count)];
            NodeGene inNode = Nodes[connection.InNode];
            NodeGene outNode = Nodes[connection.OutNode];

            // Disbale Old Connection
            connection.IsEnable = false;

            // Create a node w/ IN & OUT Connection to replace old Connection
            NodeGene newNode = new NodeGene(NodeGene.TYPE.HIDDEN, nodeInnovationCounter.GetNewInnovationNumber());
            ConnectionGene inToNew = new ConnectionGene(inNode.Id, newNode.Id, 1f, true, connectionInnovationCounter.GetNewInnovationNumber()); // Weight of 1
            ConnectionGene newToOut = new ConnectionGene(newNode.Id, outNode.Id, connection.Weight, true, connectionInnovationCounter.GetNewInnovationNumber()); // Weight of old Connection

            // Add Node & Connections to the Genome
            Nodes.Add(newNode.Id, newNode);
            Connections.Add(inToNew.InnovationId, inToNew);
            Connections.Add(newToOut.InnovationId, newToOut);
        }

        /// <summary>
        /// Disable or Enable a Random Connection
        /// </summary>
        /// <param name="maxTries">Max Tries</param>
        /// <returns>Success</returns>
        public bool EnableOrDisableRandomConnection(int maxTries = 10)
        {
            // Get List of Connection
            List<ConnectionGene> connections = Connections.Values.ToList();
            if (connections.Count < 1) return false;

            // Try maxTries times
            for (int tries = 0; tries < maxTries; tries++)
            {
                // Get a Random Connection
                ConnectionGene connection = connections[UnityEngine.Random.Range(0, Connections.Values.Count)];
                int inNode = Nodes[connection.InNode].Id;
                int outNode = Nodes[connection.OutNode].Id;

                // If the Connection is Enable => Disable it, Else Enable it
                if (connection.IsEnable)
                {
                    int inNodeOutputNb = 0;
                    int outNodeInputNb = 0;
                    foreach (ConnectionGene c in connections)
                    {
                        //TODO
                        // Check if inNode Connection still have an Output Connection
                        if (c.InNode == outNode)
                            inNodeOutputNb++;

                        // Check if outNode Connection still have an Input Connection
                        if (c.OutNode == inNode)
                            outNodeInputNb++;
                    }

                    // If inNodeOutputNb && outNodeInputNb > 1 => Disable the Connection
                    if (inNodeOutputNb > 1 && outNodeInputNb > 1)
                    {
                        connection.IsEnable = false;
                        return true;
                    }
                }
                else
                {
                    // Enable the Connection
                    connection.IsEnable = true;
                    return true;
                }
            }

            return false;
        }
        #endregion =======================================================================================================#

        #region CROSSOVER ================================================================================================#
        /// <summary>
        /// Crossover 2 Genomes
        /// </summary>
        /// <param name="moreFitParent">Parent with the most Fitness of the 2 Parents</param>
        /// <param name="lessFitParent">Second Parent</param>
        /// <param name="r">Random</param>
        /// <param name="disabledConnectionInheritChance">Disabled Connection Inherit Chance</param>
        /// <returns>Child Genome</returns>
        public static Genome Crossover(Genome moreFitParent, Genome lessFitParent, Random r, float disabledConnectionInheritChance)
        {
            Genome child = new Genome(moreFitParent.NewWeightRange, moreFitParent.PerturbingProbability);

            // Add More Fit Parent Nodes to Child
            foreach (NodeGene moreFitParentNode in moreFitParent.Nodes.Values)
                child.AddNodeGene(new NodeGene(moreFitParentNode));

            // Add a mix of Parents Connections to Child
            foreach (ConnectionGene moreFitParentConnection in moreFitParent.Connections.Values)
            {
                ConnectionGene childConnectionGene = null;

                if (lessFitParent.Connections.ContainsKey(moreFitParentConnection.InnovationId))    // Matching Connection => Select randomly one of the parents Connection
                {
                    ConnectionGene lessFitParentConnection = lessFitParent.Connections[moreFitParentConnection.InnovationId];
                    bool disabled = !moreFitParentConnection.IsEnable || !lessFitParentConnection.IsEnable;
                    childConnectionGene = r.Next(100) < 50 ? new ConnectionGene(moreFitParentConnection) : new ConnectionGene(lessFitParentConnection);
                    if (disabled && ((float)r.NextDouble() < disabledConnectionInheritChance))
                        childConnectionGene.IsEnable = false;
                }
                else                                                                                // Disjoint or excess Connection => Select More Fit Parent Connection
                    childConnectionGene = new ConnectionGene(moreFitParentConnection);

                child.AddConnectionGene(childConnectionGene);
            }
            return child;
        }
        #endregion =======================================================================================================#

        #region COUNT ====================================================================================================#
        /// <summary>
        /// Get Genome Distance between 2 Genomes
        /// </summary>
        /// <param name="genome1">Genome 1</param>
        /// <param name="genome2">Genome 2</param>
        /// <param name="disjointMutator">Disjoint Gene Mutator</param>
        /// <param name="excessMutator">Excess Gene Mutator</param>
        /// <param name="avgDiffMutator">Average Difference Mutator</param>
        /// <returns></returns>
        public static float GenomeDistance(Genome genome1, Genome genome2, float disjointMutator, float excessMutator, float avgDiffMutator)
        {
            int disjointGenes = CountDisjointGene(genome1, genome2);
            int excessGenes = CountExcessGene(genome1, genome2);
            float avgDiff = AvgWeightDiff(genome1, genome2);

            return disjointGenes * disjointMutator + excessGenes * excessMutator + avgDiff * avgDiffMutator;
        }

        /// <summary>
        /// Count Matching Node Genes between 2 Genomes
        /// </summary>
        /// <param name="genome1">Genome 1</param>
        /// <param name="genome2">Genome 2</param>
        /// <returns></returns>
        public static int CountMatchingNodeGenes(Genome genome1, Genome genome2)
        {
            int match = 0;

            // Get Node Max Id
            int maxNodeId = GetMaxInnovationId(new List<int>(genome1.Nodes.Keys), new List<int>(genome2.Nodes.Keys));

            // Count Matching Nodes
            for (int i = 0; i <= maxNodeId; i++)
            {
                if (genome1.Nodes.ContainsKey(i) && genome2.Nodes.ContainsKey(i))
                    match++;
            }

            return match;
        }

        /// <summary>
        /// Count Matching Connection Genes between 2 Genomes
        /// </summary>
        /// <param name="genome1">Genome 1</param>
        /// <param name="genome2">Genome 2</param>
        /// <returns></returns>
        public static int CountMatchingConnectionGenes(Genome genome1, Genome genome2)
        {
            int match = 0;

            // Get Connection Max Id
            int maxConnectionId = GetMaxInnovationId(new List<int>(genome1.Connections.Keys), new List<int>(genome2.Connections.Keys));

            // Count Matching Connections
            for (int i = 0; i <= maxConnectionId; i++)
            {
                if (genome1.Connections.ContainsKey(i) && genome2.Connections.ContainsKey(i))
                    match++;
            }

            return match;
        }

        /// <summary>
        /// Count Matching Genes between 2 Genomes
        /// </summary>
        /// <param name="genome1">Genome 1</param>
        /// <param name="genome2">Genome 2</param>
        /// <returns></returns>
        public static int CountMatchingGene(Genome genome1, Genome genome2)
        {
            return CountMatchingNodeGenes(genome1, genome2) + CountMatchingConnectionGenes(genome1, genome2);
        }

        /// <summary>
        /// Count Disjoint Node Genes between 2 Genomes
        /// </summary>
        /// <param name="genome1">Genome 1</param>
        /// <param name="genome2">Genome 2</param>
        /// <returns></returns>
        public static int CountDisjointNodeGenes(Genome genome1, Genome genome2)
        {
            int match = 0;

            // Get Nodes Max Id
            int genome1MaxNodeId = GetMaxInnovationId(new List<int>(genome1.Nodes.Keys));
            int genome2MaxNodeId = GetMaxInnovationId(new List<int>(genome2.Nodes.Keys));
            int maxNodeId = genome1MaxNodeId > genome2MaxNodeId ? genome1MaxNodeId : genome2MaxNodeId;

            // Count Disjoint Nodes
            for (int i = 0; i <= maxNodeId; i++)
            {
                if (!genome1.Nodes.ContainsKey(i) && genome2.Nodes.ContainsKey(i) && genome1MaxNodeId > i)
                    match++;
                else if (!genome2.Nodes.ContainsKey(i) && genome1.Nodes.ContainsKey(i) && genome2MaxNodeId > i)
                    match++;
            }

            return match;
        }

        /// <summary>
        /// Count Disjoint Connection Genes between 2 Genomes
        /// </summary>
        /// <param name="genome1">Genome 1</param>
        /// <param name="genome2">Genome 2</param>
        /// <returns></returns>
        public static int CountDisjointConnectionGenes(Genome genome1, Genome genome2)
        {
            int match = 0;

            // Get Connections Max Id
            int genome1MaxConnectionId = GetMaxInnovationId(new List<int>(genome1.Connections.Keys));
            int genome2MaxConnectionId = GetMaxInnovationId(new List<int>(genome2.Connections.Keys));
            int maxConnectionId = genome1MaxConnectionId > genome2MaxConnectionId ? genome1MaxConnectionId : genome2MaxConnectionId;

            // Count Disjoint Connections
            for (int i = 0; i <= maxConnectionId; i++)
            {
                if (!genome1.Nodes.ContainsKey(i) && genome2.Nodes.ContainsKey(i) && genome1MaxConnectionId > i)
                    match++;
                else if (!genome2.Nodes.ContainsKey(i) && genome1.Nodes.ContainsKey(i) && genome2MaxConnectionId > i)
                    match++;
            }

            return match;
        }

        /// <summary>
        /// Count Disjoint Genes between 2 Genomes
        /// </summary>
        /// <param name="genome1">Genome 1</param>
        /// <param name="genome2">Genome 2</param>
        /// <returns></returns>
        public static int CountDisjointGene(Genome genome1, Genome genome2)
        {
            return CountDisjointNodeGenes(genome1, genome2) + CountDisjointConnectionGenes(genome1, genome2);
        }

        /// <summary>
        /// Count Excess Node Genes between 2 Genomes
        /// </summary>
        /// <param name="genome1">Genome 1</param>
        /// <param name="genome2">Genome 2</param>
        /// <returns></returns>
        public static int CountExcessNodeGenes(Genome genome1, Genome genome2)
        {
            int match = 0;

            // Get Nodes Max Id
            int genome1MaxNodeId = GetMaxInnovationId(new List<int>(genome1.Nodes.Keys));
            int genome2MaxNodeId = GetMaxInnovationId(new List<int>(genome2.Nodes.Keys));
            int maxNodeId = genome1MaxNodeId > genome2MaxNodeId ? genome1MaxNodeId : genome2MaxNodeId;

            // Count Excess Nodes
            for (int i = 0; i <= maxNodeId; i++)
            {
                if (!genome1.Nodes.ContainsKey(i) && genome2.Nodes.ContainsKey(i) && genome1MaxNodeId < i)
                    match++;
                else if (!genome2.Nodes.ContainsKey(i) && genome1.Nodes.ContainsKey(i) && genome2MaxNodeId < i)
                    match++;
            }

            return match;
        }

        /// <summary>
        /// Count Excess Connection Genes between 2 Genomes
        /// </summary>
        /// <param name="genome1">Genome 1</param>
        /// <param name="genome2">Genome 2</param>
        /// <returns></returns>
        public static int CountExcessConnectionGenes(Genome genome1, Genome genome2)
        {
            int match = 0;

            // Get Connections Max Id
            int genome1MaxConnectionId = GetMaxInnovationId(new List<int>(genome1.Connections.Keys));
            int genome2MaxConnectionId = GetMaxInnovationId(new List<int>(genome2.Connections.Keys));
            int maxConnectionId = genome1MaxConnectionId > genome2MaxConnectionId ? genome1MaxConnectionId : genome2MaxConnectionId;

            // Count Excess Connections
            for (int i = 0; i <= maxConnectionId; i++)
            {
                if (!genome1.Nodes.ContainsKey(i) && genome2.Nodes.ContainsKey(i) && genome1MaxConnectionId < i)
                    match++;
                else if (!genome2.Nodes.ContainsKey(i) && genome1.Nodes.ContainsKey(i) && genome2MaxConnectionId < i)
                    match++;
            }

            return match;
        }

        /// <summary>
        /// Count Excess Genes between 2 Genomes
        /// </summary>
        /// <param name="genome1">Genome 1</param>
        /// <param name="genome2">Genome 2</param>
        /// <returns></returns>
        public static int CountExcessGene(Genome genome1, Genome genome2)
        {
            return CountExcessNodeGenes(genome1, genome2) + CountExcessConnectionGenes(genome1, genome2);
        }

        /// <summary>
        /// Get Average Weight Difference between 2 Genomes
        /// </summary>
        /// <param name="genome1">Genome 1</param>
        /// <param name="genome2">Genome 2</param>
        /// <returns></returns>
        public static float AvgWeightDiff(Genome genome1, Genome genome2)
        {
            int match = 0;
            float weightDiff = 0f;

            // Get Connection Max Id
            int maxConnectionId = GetMaxInnovationId(new List<int>(genome1.Connections.Keys), new List<int>(genome2.Connections.Keys));

            // Count Weight Diff in Matching Connections
            for (int i = 0; i <= maxConnectionId; i++)
            {
                if (genome1.Connections.ContainsKey(i) && genome2.Connections.ContainsKey(i))
                {
                    match++;
                    weightDiff += Math.Abs(genome1.Connections[i].Weight - genome2.Connections[i].Weight);
                }
            }

            return weightDiff / match;
        }
        #endregion =======================================================================================================#

        #region STRING ===================================================================================================#
        public string NodesToString()
        {
            string nodesString = "";
            foreach (NodeGene node in Nodes.Values)
            {
                nodesString += node.ToString();
            }

            return nodesString;
        }

        public string ConnectionsToString()
        {
            string connectionsString = "";
            foreach (ConnectionGene connection in Connections.Values)
            {
                connectionsString += connection.ToString() + "\n";
            }

            return connectionsString;
        }

        public override string ToString()
        {
            return String.Format("{0}\n{1}", NodesToString(), ConnectionsToString());
        }
        #endregion =======================================================================================================#

        #region UTILS ====================================================================================================#
        /// <summary>
        /// Get Max Innovation Id
        /// </summary>
        /// <param name="innovationIds1">Innovation Id List 1</param>
        /// <param name="innovationIds2">Innovation Id List 2</param>
        /// <returns></returns>
        public static int GetMaxInnovationId(List<int> innovationIds1, List<int> innovationIds2)
        {
            // Get Max Innovation Id from both Lists
            int innovationIdMax1 = GetMaxInnovationId(innovationIds1);
            int innovationIdMax2 = GetMaxInnovationId(innovationIds2);

            // Return Max Innovation Id
            return innovationIdMax1 > innovationIdMax2 ? innovationIdMax1 : innovationIdMax2;
        }

        /// <summary>
        /// Get Max Innovation Id
        /// </summary>
        /// <param name="innovationIds">Innovation Id List</param>
        /// <returns></returns>
        public static int GetMaxInnovationId(List<int> innovationIds)
        {
            // Sort List
            innovationIds.Sort();

            // Return Max Innovation Id
            if (innovationIds.Count == 0) return 0;
            return innovationIds[innovationIds.Count - 1];
        }

        /// <summary>
        /// Add Node Gene
        /// </summary>
        /// <param name="node">Node</param>
        public void AddNodeGene(NodeGene node)
        {
            Nodes[node.Id] = node;
        }

        /// <summary>
        /// Add Connection Gene
        /// </summary>
        /// <param name="connection">Connection</param>
        public void AddConnectionGene(ConnectionGene connection)
        {
            Connections[connection.InnovationId] = connection;
        }

        /// <summary>
        /// Prepare To Json
        /// </summary>
        public void PrepareToJson(InnovationCounter nodeInnovation, InnovationCounter connectInnovation)
        {
            _newWeightRangeX = NewWeightRange.x;
            _newWeightRangeY = NewWeightRange.y;
            _nodeInnovation = nodeInnovation.CurrentInnovation;
            _connectionInnovation = connectInnovation.CurrentInnovation;
        }

        /// <summary>
        /// Init From Json
        /// </summary>
        public void InitFromJson()
        {
            NewWeightRange = new Vector2(_newWeightRangeX, _newWeightRangeY);
            Fitness = 0f;
            Id = Genome.genomeIdCounter;
            Genome.genomeIdCounter++;
        }
        #endregion =======================================================================================================#
    }
}
