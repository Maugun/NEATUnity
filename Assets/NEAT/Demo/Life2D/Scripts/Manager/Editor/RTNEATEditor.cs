using UnityEditor;
using UnityEngine;

namespace NEAT.Demo.Life2D
{
    [CustomEditor(typeof(RTNEATManager))]
    public class RTNEATEditor : Editor
    {
        RTNEATManager manager;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (manager._config != null)
                DrawConfigEditor(manager._config);
        }

        void DrawConfigEditor(Object config)
        {
            Editor editor = CreateEditor(config);
            editor.OnInspectorGUI();
        }

        private void OnEnable()
        {
            manager = (RTNEATManager)target;
        }
    }
}

