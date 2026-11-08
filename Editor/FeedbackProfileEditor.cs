using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Astra.Pulse.Editor
{
    /// <summary>
    /// Draws a <see cref="FeedbackProfileSO"/> with an "Add Entry" type menu: Unity's default inspector cannot
    /// create instances in a <c>[SerializeReference]</c> list.
    /// </summary>
    [CustomEditor(typeof(FeedbackProfileSO))]
    public sealed class FeedbackProfileEditor : UnityEditor.Editor
    {
        private const string EntriesField = "_entries";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));

            if (SerializationUtility.HasManagedReferencesWithMissingTypes(target))
            {
                EditorGUILayout.HelpBox(
                    "Some entries reference a type that no longer exists. They show as Empty and are skipped by BuildRequest.",
                    MessageType.Warning);
            }

            SerializedProperty entries = serializedObject.FindProperty(EntriesField);
            int removeAt = -1;
            for (int i = 0; i < entries.arraySize; i++)
            {
                if (DrawEntry(entries.GetArrayElementAtIndex(i)))
                    removeAt = i;
            }

            if (removeAt >= 0)
                entries.DeleteArrayElementAtIndex(removeAt);

            Rect addRect = EditorGUILayout.GetControlRect();
            if (EditorGUI.DropdownButton(addRect, new GUIContent("Add Entry"), FocusType.Keyboard))
                ShowAddMenu(addRect);

            serializedObject.ApplyModifiedProperties();
        }

        private static bool DrawEntry(SerializedProperty entry)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            object value = entry.managedReferenceValue;
            EditorGUILayout.LabelField(FeedbackEntryTypes.GetDisplayName(value?.GetType()), EditorStyles.boldLabel);
            bool remove = GUILayout.Button("Remove", GUILayout.Width(64f));
            EditorGUILayout.EndHorizontal();

            SerializedProperty child = entry.Copy();
            SerializedProperty end = entry.GetEndProperty();
            bool enterChildren = true;
            while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, end))
            {
                enterChildren = false;
                EditorGUILayout.PropertyField(child, true);
            }

            EditorGUILayout.EndVertical();
            return remove;
        }

        private void ShowAddMenu(Rect rect)
        {
            GenericMenu menu = new GenericMenu();
            IReadOnlyList<Type> types = FeedbackEntryTypes.GetAuthorable();
            Dictionary<string, int> nameCounts = new Dictionary<string, int>();
            foreach (Type type in types)
            {
                string name = FeedbackEntryTypes.GetDisplayName(type);
                nameCounts[name] = nameCounts.TryGetValue(name, out int count) ? count + 1 : 1;
            }

            foreach (Type type in types)
            {
                string name = FeedbackEntryTypes.GetDisplayName(type);
                string label = nameCounts[name] > 1 && !string.IsNullOrEmpty(type.Namespace)
                    ? name + " (" + type.Namespace + ")"
                    : name;
                Type captured = type;
                menu.AddItem(new GUIContent(label), false, () => AddEntry(captured));
            }

            if (types.Count == 0)
                menu.AddDisabledItem(new GUIContent("No [Serializable] IFeedbackEntry type"));

            menu.DropDown(rect);
        }

        private void AddEntry(Type type)
        {
            serializedObject.Update();
            SerializedProperty entries = serializedObject.FindProperty(EntriesField);
            int index = entries.arraySize;
            entries.arraySize = index + 1;
            entries.GetArrayElementAtIndex(index).managedReferenceValue = Activator.CreateInstance(type, true);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
