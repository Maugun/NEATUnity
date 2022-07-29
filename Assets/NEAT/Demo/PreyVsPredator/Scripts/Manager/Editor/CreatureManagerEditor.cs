using UnityEditor;
using UnityEngine;

namespace NEAT.Demo.PreyVsPredator
{
    [CustomEditor(typeof(CreatureManager))]
    public class CreatureManagerEditor : Editor
    {
        CreatureManager manager;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (manager.config != null)
                DrawConfigEditor(manager.config);
        }

        void DrawConfigEditor(Object config)
        {
            Editor editor = CreateEditor(config);
            editor.OnInspectorGUI();
        }

        private void OnEnable()
        {
            manager = (CreatureManager)target;
        }
    }
}


