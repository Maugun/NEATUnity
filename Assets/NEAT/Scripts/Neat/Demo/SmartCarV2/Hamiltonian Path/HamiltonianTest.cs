using UnityEngine;

namespace NEAT.Demo.Tools
{
    public class HamiltonianTest : MonoBehaviour
    {
        void Start()
        {
            HamiltonianPath hp = new HamiltonianPath();
            hp.Generate(9, 9, true);
            Debug.Log(hp.PathToString());
        }
    }
}
