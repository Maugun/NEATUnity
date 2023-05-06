using UnityEngine;
using Random = System.Random;

namespace NEAT.Demo.Tools
{
    public class HamiltonianTest : MonoBehaviour
    {
        void Start()
        {
            HamiltonianPath hp = new HamiltonianPath(new Random());
            hp.Generate(9, 9, true);
            Debug.Log(hp.PathToString());
        }
    }
}
