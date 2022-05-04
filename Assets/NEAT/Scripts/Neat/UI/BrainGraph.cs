using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NEAT
{
    public class BrainGraph : MonoBehaviour
    {
        [Header("Objects")]
        public RectTransform panelRec;
        public GameObject containerPrefab;
        public GameObject nodePrefab;
        public GameObject connectionPrefab;

        [Header("Objects")]
        public float nodeSize = 20f;
        public float connectionThickness = 10f;

        [Header("Config")]
        public int timeOut = 1000;

        private GameObject nodeContainer;
        private GameObject connectionContainer;

        private Dictionary<int, Neuron> neurons;                                                // Neurons of the genome by Ids
        private List<int> inputIds;                                                             // Ids of Input Neurons

        private Vector2 _panelSize;
        private List<GameObject> _nodes;
        private List<GameObject> _connections;
        private List<int> _createdNodes;
        private List<GraphNode> _graphNodes;
        private float _minXValue;
        private float _maxXValue;
        private float _minYValue;
        private float _maxYValue;
        private Dictionary<int, int> _stepYDict;

        private void Start()
        {
            _nodes = new List<GameObject>();
            _connections = new List<GameObject>();
            _createdNodes = new List<int>();
            _graphNodes = new List<GraphNode>();
            _stepYDict = new Dictionary<int, int>();

            // Size of the panel
            _panelSize = panelRec.sizeDelta;

            // Min, Max x & y values
            _minXValue = nodeSize;
            _maxXValue = _panelSize.x - nodeSize;
            _minYValue = nodeSize;
            _maxYValue = _panelSize.y - nodeSize;

            // Instantiate containers
            connectionContainer = Instantiate(containerPrefab, Vector3.zero, Quaternion.identity, transform);
            nodeContainer = Instantiate(containerPrefab, Vector3.zero, Quaternion.identity, transform);

            // Set anchored position to match the panel
            Vector2 anchoredPosition = new Vector2(_panelSize.x / 2, -_panelSize.y / 2);

            // Connection Container
            RectTransform connectionContainerRec = connectionContainer.GetComponent<RectTransform>();
            connectionContainerRec.anchoredPosition = anchoredPosition;
            connectionContainerRec.sizeDelta = _panelSize;

            // Node Container
            RectTransform nodeContainerRec = nodeContainer.GetComponent<RectTransform>();
            nodeContainerRec.anchoredPosition = anchoredPosition;
            nodeContainerRec.sizeDelta = _panelSize;
        }

        private GameObject AddNode(Vector2 anchoredPosition, Neuron.NeuronType type = Neuron.NeuronType.HIDDEN)
        {
            GameObject node = Instantiate(nodePrefab, Vector3.zero, Quaternion.identity, nodeContainer.transform);

            // Set node size & position
            RectTransform nodeRec = node.GetComponent<RectTransform>();
            nodeRec.sizeDelta = new Vector2(nodeSize, nodeSize);
            nodeRec.anchoredPosition = anchoredPosition;

            // Set color
            Image nodeSprite = node.GetComponent<Image>();
            nodeSprite.color = type == Neuron.NeuronType.HIDDEN ? Color.white : Color.green;

            _nodes.Add(node);

            return node;
        }

        private GameObject AddConnection(Vector2 nodePositionA, Vector2 nodePositionB, float weight)
        {
            GameObject connection = Instantiate(connectionPrefab, Vector3.zero, Quaternion.identity, connectionContainer.transform);
            RectTransform connectionRec = connection.GetComponent<RectTransform>();

            // Set color
            Color color = Color.white;
            if (weight > 0) color = Color.blue;
            if (weight < 0) color = Color.red;
            connection.GetComponent<Image>().color = color;

            // Calc distance between the 2 nodes & the directional vector
            float distance = Vector2.Distance(nodePositionA, nodePositionB);
            Vector2 direction = (nodePositionB - nodePositionA).normalized;

            // Set connection size & position
            connectionRec.sizeDelta = new Vector2(distance, connectionThickness);
            connectionRec.anchoredPosition = nodePositionA + direction * distance * .5f;

            // Rotate
            float angle = Vector2.SignedAngle(connection.transform.right, direction);
            connectionRec.transform.Rotate(Vector3.forward, angle);

            _connections.Add(connection);

            return connection;
        }

        public void CreateGraph()
        {
            // Create graphNode
            foreach (int id in inputIds)
                createNextNode(id);

            // Calc xMax, yMax, xStep & yStep
            int maxX = _stepYDict.Count + 1;
            int maxY = _stepYDict.Values.Max();
            float xStep = (_maxXValue - _minXValue) / maxX;
            float yStep = -((_maxYValue + _minYValue) / maxY);

            // Sort by Id (for outputs nodes)
            _graphNodes.Sort((x, y) => x.Id.CompareTo(y.Id));

            // Update outputs node X value to be on the far right
            foreach (GraphNode node in _graphNodes)
            {
                if (node.NeuronType == Neuron.NeuronType.OUTPUT)
                    node.X = maxX;
            }

            // Create nodes & connections
            foreach (GraphNode node in _graphNodes)
            {
                Vector2 nodePosition = new Vector2(node.X * xStep + _minXValue, node.Y * yStep - _minYValue);
                AddNode(nodePosition, node.NeuronType);

                int i = 0;
                foreach (GraphNode childNode in node.Connections)
                {
                    Vector2 childNodePosition = new Vector2(childNode.X * xStep + _minXValue, childNode.Y * yStep - _minYValue);
                    AddConnection(nodePosition, childNodePosition, node.Weights[i]);
                    i++;
                }
            }
        }

        private GraphNode createNextNode(int nodeId, int xStep = 0)
        {
            // If graphNode already exists return it
            if (_createdNodes.Contains(nodeId))
                return _graphNodes.Find(node => (node.Id == nodeId));

            // Add yStep if it doesn't exist
            if (_stepYDict.ContainsKey(xStep) == false)
                _stepYDict[xStep] = 0;

            // Create new graphNode
            float x = xStep;
            float y = _stepYDict[xStep];
            GraphNode node = new GraphNode(nodeId, neurons[nodeId].NeuronTypeValue, x, y);
            _graphNodes.Add(node);
            _createdNodes.Add(nodeId);

            // Update yStep then xStep
            _stepYDict[xStep] += 1;
            xStep++;

            // Create & add child graphNode
            int i = 0;
            foreach (int id in neurons[nodeId].OutputIds)
            {
                GraphNode nextNode = createNextNode(id, xStep);
                node.Connections.Add(nextNode);
                node.Weights.Add(neurons[nodeId].OutputWeights[i]);
                i++;
            }

            return node;
        }

        // private int MaxConnections(int id)
        // {
        //     if (_neurons[id].NeuronTypeValue == Neuron.NeuronType.OUTPUT)
        //         return 1;

        //     int[] outputs = _neurons[id].OutputIds;
        //     int max = 0;
        //     for (int i = 0; i < outputs.Length; i++)
        //     {
        //         int connections = MaxConnections(outputs[i]);
        //         max = connections > max ? connections + 1 : max;
        //     }
        //     return max;
        // }

        public void ClearGraph()
        {
            foreach (GameObject node in _nodes)
            {
                Destroy(node);
            }
            _nodes.Clear();

            foreach (GameObject connection in _connections)
            {
                Destroy(connection);
            }
            _connections.Clear();
            _stepYDict.Clear();
            _createdNodes.Clear();
        }

        public void SetNeuralNetwork(NeuralNetwork nn)
        {
            neurons = nn.GetNeurons();
            inputIds = nn.GetInputIds();
        }
    }

    public class GraphNode
    {
        public int Id { get; set; }
        public Neuron.NeuronType NeuronType { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public List<GraphNode> Connections { get; set; }
        public List<float> Weights { get; set; }

        public GraphNode(int id, Neuron.NeuronType neuronType, float x, float y)
        {
            Id = id;
            NeuronType = neuronType;
            X = x;
            Y = y;
            Connections = new List<GraphNode>();
            Weights = new List<float>();
        }

        public bool Equals(GraphNode other)
        {
            return this.Id == other.Id;
        }
    }
}
