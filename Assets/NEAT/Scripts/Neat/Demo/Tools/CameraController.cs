using UnityEngine;

namespace NEAT.Demo.SmartCarV2
{
    public class CameraController : MonoBehaviour
    {
        public float _addToZ = 5f;

        public void CenterCameraOnMap(int x, int y, float tileSize)
        {
            float xPos = ((x + 1) / 2) * tileSize;
            float zPos = (y / 2) * tileSize;
            float yPos = (xPos > zPos) ? xPos + _addToZ : zPos + _addToZ;
            Vector3 newPosition = new Vector3(xPos, yPos, zPos);
            transform.position = newPosition;
        }
    }
}
