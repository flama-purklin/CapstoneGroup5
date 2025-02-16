using LLMUnity;
using System;
using UnityEditor;
using UnityEngine;
using static LLMUnity.LLM;

namespace LLMUnity
{
    public class GPUAccelerationEditor : Editor
    {
        [CustomPropertyDrawer(typeof(LLMAttribute))]
        private class LLMDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                SerializedObject serializedObject = property.serializedObject;
                SerializedProperty gpuAccelProp = serializedObject.FindProperty("gpuAcceleration");

                if (property.name == "_numGPULayers")
                {
                    EditorGUI.BeginProperty(position, label, property);

                    using (new EditorGUI.DisabledGroupScope(gpuAccelProp.enumValueIndex != (int)GPUAccelerationMode.Manual))
                    {
                        position.x += 15;
                        position.width -= 15;
                        EditorGUI.PropertyField(position, property, label);
                    }

                    EditorGUI.EndProperty();
                }
                else
                {
                    EditorGUI.PropertyField(position, property, label);
                }
            }
        }
    }
}
