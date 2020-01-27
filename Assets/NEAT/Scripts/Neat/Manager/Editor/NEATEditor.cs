using UnityEditor;
using UnityEngine;

namespace NEAT
{
    [CustomEditor(typeof(NEATManager))]
    public class NEATEditor : Editor
    {
        NEATManager manager;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawConfigEditor(manager._config);
        }

        void DrawConfigEditor(Object config)
        {
            Editor editor = CreateEditor(config);
            editor.OnInspectorGUI();
        }

        private void OnEnable()
        {
            manager = (NEATManager)target;
        }
    }
}


