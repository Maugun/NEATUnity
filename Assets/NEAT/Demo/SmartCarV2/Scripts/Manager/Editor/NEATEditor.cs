using UnityEditor;
using UnityEngine;

namespace NEAT.Demo.SmartCarV2
{
    [CustomEditor(typeof(NEATManager))]
    public class NEATEditor : Editor
    {
        NEATManager manager;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (manager.config != null) DrawConfigEditor(manager.config);
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
