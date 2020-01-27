using UnityEngine;

namespace NEAT.Demo.Tools
{
    public class TimeScale : MonoBehaviour
    {
        [Range(0f, 100f)]
        public float _timeScale = 1.0f;

        private void Update()
        {
            Time.timeScale = _timeScale;
        }
    }
}
