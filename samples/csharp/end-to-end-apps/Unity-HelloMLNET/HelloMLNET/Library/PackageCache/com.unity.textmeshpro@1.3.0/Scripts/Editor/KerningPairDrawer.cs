using UnityEngine;
using UnityEditor;
using System.Collections;


namespace TMPro.EditorUtilities
{

    [CustomPropertyDrawer(typeof(KerningPair))]
    public class KerningPairDrawer : PropertyDrawer
    {
        private bool isEditingEnabled = false;
        private bool isSelectable = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty prop_FirstGlyph = property.FindPropertyRelative("m_FirstGlyph");
            SerializedProperty prop_FirstGlyphAdjustment = property.FindPropertyRelative("m_FirstGlyphAdjustments");
            SerializedProperty prop_SecondGlyph = property.FindPropertyRelative("m_SecondGlyph");
            SerializedProperty prop_SecondGlyphAdjustment = property.FindPropertyRelative("m_SecondGlyphAdjustments");

            position.yMin += 2;

            // We get Rect since a valid position may not be provided by the caller.
            GUILayoutUtility.GetRect(position.width, 45);

            float width = position.width / 2;
            float padding = 5.0f;

            Rect rect;

            isEditingEnabled = GUI.enabled;
            isSelectable = label.text == "Selectable" ? true : false;

            // First Glyph
            GUI.enabled = isEditingEnabled;
            if (isSelectable)
            {
                bool prevGuiState = GUI.enabled;
                GUI.enabled = true;
                rect = new Rect(position.x, position.y, 40, 18);
                EditorGUI.LabelField(rect, "Char:", TMP_UIStyleManager.label);

                rect = new Rect(position.x + 35f, position.y, 30, 18);
                EditorGUI.LabelField(rect, "<color=#FFFF80>" + (char)prop_FirstGlyph.intValue + "</color>", TMP_UIStyleManager.label);

                // Display ASCII decimal value
                rect = new Rect(position.x + 60f, position.y, 30, 18);
                EditorGUI.LabelField(rect, "ID:", TMP_UIStyleManager.label);

                rect = new Rect(position.x + 80f, position.y, 40, 18);
                EditorGUI.LabelField(rect, "<color=#FFFF80>" + prop_FirstGlyph.intValue + "</color>", TMP_UIStyleManager.label);
                GUI.enabled = prevGuiState;
            }
            else
            {
                rect = new Rect(position.x, position.y, width / 2 * 0.8f - padding, 18);

                string glyph = EditorGUI.TextArea(rect, "" + (char)prop_FirstGlyph.intValue);
                if (GUI.changed && glyph != "")
                {
                    GUI.changed = false;
                    prop_FirstGlyph.intValue = glyph[0];
                }

                rect.x += width / 2 * 0.8f;
                EditorGUI.PropertyField(rect, prop_FirstGlyph, GUIContent.none);

            }

            GUI.enabled = isEditingEnabled;
            EditorGUIUtility.labelWidth = 25f;

            rect = new Rect(position.x, position.y + 20, width * 0.5f - padding, 18);
            EditorGUI.PropertyField(rect, prop_FirstGlyphAdjustment.FindPropertyRelative("xPlacement"), new GUIContent("OX"));

            rect.x += width * 0.5f;
            EditorGUI.PropertyField(rect, prop_FirstGlyphAdjustment.FindPropertyRelative("yPlacement"), new GUIContent("OY"));

            rect.x = position.x;
            rect.y += 20;
            EditorGUI.PropertyField(rect, prop_FirstGlyphAdjustment.FindPropertyRelative("xAdvance"), new GUIContent("AX"));

            //rect.x += width * 0.5f;
            //EditorGUI.PropertyField(rect, prop_FirstGlyphAdjustment.FindPropertyRelative("yAdvance"), new GUIContent("AY"));


            // Second Glyph
            GUI.enabled = isEditingEnabled;
            if (isSelectable)
            {
                bool prevGuiState = GUI.enabled;
                GUI.enabled = true;
                rect = new Rect(position.width / 2 + 20, position.y, 40f, 18);
                EditorGUI.LabelField(rect, "Char:", TMP_UIStyleManager.label);

                rect = new Rect(rect.x + 35f, position.y, 30, 18);
                EditorGUI.LabelField(rect, "<color=#FFFF80>" + (char)prop_SecondGlyph.intValue + "</color>", TMP_UIStyleManager.label);

                // Display ASCII decimal value
                rect = new Rect(rect.x + 25f, position.y, 30, 18);
                EditorGUI.LabelField(rect, "ID:", TMP_UIStyleManager.label);

                rect = new Rect(rect.x + 20f, position.y, 40, 18);
                EditorGUI.LabelField(rect, "<color=#FFFF80>" + prop_SecondGlyph.intValue + "</color>", TMP_UIStyleManager.label);
                GUI.enabled = prevGuiState;
            }
            else
            {
                rect = new Rect(position.width / 2 + 20, position.y, width / 2 * 0.8f - padding, 18);

                string glyph = EditorGUI.TextArea(rect, "" + (char)prop_SecondGlyph.intValue);
                if (GUI.changed && glyph != "")
                {
                    GUI.changed = false;
                    prop_SecondGlyph.intValue = glyph[0];
                }

                rect.x += width / 2 * 0.8f;
                EditorGUI.PropertyField(rect, prop_SecondGlyph, GUIContent.none);
            }

            GUI.enabled = isEditingEnabled;
            EditorGUIUtility.labelWidth = 25f;

            rect = new Rect(position.width / 2 + 20, position.y + 20, width * 0.5f - padding, 18);
            EditorGUI.PropertyField(rect, prop_SecondGlyphAdjustment.FindPropertyRelative("xPlacement"), new GUIContent("OX"));

            rect.x += width * 0.5f;
            EditorGUI.PropertyField(rect, prop_SecondGlyphAdjustment.FindPropertyRelative("yPlacement"), new GUIContent("OY"));

            rect.x = position.width / 2 + 20;
            rect.y += 20;
            EditorGUI.PropertyField(rect, prop_SecondGlyphAdjustment.FindPropertyRelative("xAdvance"), new GUIContent("AX"));

            //rect.x += width * 0.5f;
            //EditorGUI.PropertyField(rect, prop_SecondGlyphAdjustment.FindPropertyRelative("yAdvance"), new GUIContent("AY"));

        }


    }
}