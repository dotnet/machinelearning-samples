using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;



namespace TMPro.EditorUtilities
{

    [CustomEditor(typeof(TMP_SpriteAsset))]
    public class TMP_SpriteAssetEditor : Editor
    {
        struct UI_PanelState
        {
            public static bool spriteAssetInfoPanel = true;
            public static bool fallbackSpriteAssetPanel = true;
            public static bool spriteInfoPanel;
        }

        int m_moveToIndex;
        int m_selectedElement = -1;
        int m_page;

        const string k_UndoRedo = "UndoRedoPerformed";

        string m_searchPattern;
        List<int> m_searchList;
        bool m_isSearchDirty;

        SerializedProperty m_spriteAtlas_prop;
        SerializedProperty m_material_prop;
        SerializedProperty m_spriteInfoList_prop;
        ReorderableList m_fallbackSpriteAssetList;

        bool isAssetDirty;

        float m_xOffset;
        float m_yOffset;
        float m_xAdvance;
        float m_scale;

        public void OnEnable()
        {
            m_spriteAtlas_prop = serializedObject.FindProperty("spriteSheet");
            m_material_prop = serializedObject.FindProperty("material");
            m_spriteInfoList_prop = serializedObject.FindProperty("spriteInfoList");

            // Fallback TMP Sprite Asset list
            m_fallbackSpriteAssetList = new ReorderableList(serializedObject, serializedObject.FindProperty("fallbackSpriteAssets"), true, true, true, true);

            m_fallbackSpriteAssetList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = m_fallbackSpriteAssetList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
            };

            m_fallbackSpriteAssetList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, new GUIContent("Fallback Sprite Asset List", "Select the Sprite Assets that will be searched and used as fallback when a given sprite is missing from this sprite asset."));
            };
        }


        public override void OnInspectorGUI()
        {

            //Debug.Log("OnInspectorGUI Called.");
            Event currentEvent = Event.current;
            string evt_cmd = currentEvent.commandName; // Get Current Event CommandName to check for Undo Events

            serializedObject.Update();

            // TEXTMESHPRO SPRITE INFO PANEL
            GUILayout.Label("Sprite Info", EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_spriteAtlas_prop , new GUIContent("Sprite Atlas"));
            if (EditorGUI.EndChangeCheck())
            {
                // Assign the new sprite atlas texture to the current material
                Texture2D tex = m_spriteAtlas_prop.objectReferenceValue as Texture2D;
                if (tex != null)
                {
                    Material mat = m_material_prop.objectReferenceValue as Material;
                    if (mat != null)
                        mat.mainTexture = tex;
                }
            }

            EditorGUILayout.PropertyField(m_material_prop, new GUIContent("Default Material"));
             
            EditorGUILayout.Space();

            // FALLBACK SPRITE ASSETS
            EditorGUI.indentLevel = 0;
            UI_PanelState.fallbackSpriteAssetPanel = EditorGUILayout.Foldout(UI_PanelState.fallbackSpriteAssetPanel, new GUIContent("Fallback Sprite Assets", "Select the Sprite Assets that will be searched and used as fallback when a given sprite is missing from this sprite asset."), true, TMP_UIStyleManager.boldFoldout);
            
            if (UI_PanelState.fallbackSpriteAssetPanel)
            {
                m_fallbackSpriteAssetList.DoLayoutList();
            }

            // SPRITE LIST
            EditorGUI.indentLevel = 0;

            UI_PanelState.spriteInfoPanel = EditorGUILayout.Foldout(UI_PanelState.spriteInfoPanel, "Sprite List", true, TMP_UIStyleManager.boldFoldout);

            if (UI_PanelState.spriteInfoPanel)
            {
                int arraySize = m_spriteInfoList_prop.arraySize;
                int itemsPerPage = 10; // (Screen.height - 292) / 80;

                // Display Glyph Management Tools
                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(true));
                {
                    // Search Bar implementation
                    #region DISPLAY SEARCH BAR
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUIUtility.labelWidth = 110f;
                        EditorGUI.BeginChangeCheck();
                        string searchPattern = EditorGUILayout.TextField("Sprite Search", m_searchPattern, "SearchTextField");
                        if (EditorGUI.EndChangeCheck() || m_isSearchDirty)
                        {
                            if (string.IsNullOrEmpty(searchPattern) == false)
                            {
                                //GUIUtility.keyboardControl = 0;
                                m_searchPattern = searchPattern.ToLower(System.Globalization.CultureInfo.InvariantCulture).Trim();

                                // Search Glyph Table for potential matches
                                SearchGlyphTable(m_searchPattern, ref m_searchList);
                            }

                            m_isSearchDirty = false;
                        }

                        string styleName = string.IsNullOrEmpty(m_searchPattern) ? "SearchCancelButtonEmpty" : "SearchCancelButton";
                        if (GUILayout.Button(GUIContent.none, styleName))
                        {
                            GUIUtility.keyboardControl = 0;
                            m_searchPattern = string.Empty;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    #endregion

                    // Display Page Navigation
                    if (!string.IsNullOrEmpty(m_searchPattern))
                        arraySize = m_searchList.Count;

                    // Display Page Navigation
                    DisplayGlyphPageNavigation(arraySize, itemsPerPage);
                }
                EditorGUILayout.EndVertical();

                if (arraySize > 0)
                {
                    // Display each SpriteInfo entry using the SpriteInfo property drawer.
                    for (int i = itemsPerPage * m_page; i < arraySize && i < itemsPerPage * (m_page + 1); i++)
                    {
                        // Define the start of the selection region of the element.
                        Rect elementStartRegion = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));

                        int elementIndex = i;
                        if (!string.IsNullOrEmpty(m_searchPattern))
                            elementIndex = m_searchList[i];

                        SerializedProperty spriteInfo = m_spriteInfoList_prop.GetArrayElementAtIndex(elementIndex);

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(75));
                        {
                            EditorGUI.BeginDisabledGroup(i != m_selectedElement);
                            {
                                EditorGUILayout.PropertyField(spriteInfo);
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        EditorGUILayout.EndVertical();

                        // Define the end of the selection region of the element.
                        Rect elementEndRegion = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));

                        // Check for Item selection
                        Rect selectionArea = new Rect(elementStartRegion.x, elementStartRegion.y, elementEndRegion.width, elementEndRegion.y - elementStartRegion.y);
                        if (DoSelectionCheck(selectionArea))
                        {
                            if (m_selectedElement == i)
                            {
                                m_selectedElement = -1;
                            }
                            else
                            {
                                m_selectedElement = i;
                                GUIUtility.keyboardControl = 0;
                            }
                        }

                        // Draw & Handle Section Area
                        if (m_selectedElement == i)
                        {
                            // Draw selection highlight
                            TMP_EditorUtility.DrawBox(selectionArea, 2f, new Color32(40, 192, 255, 255));

                            // Draw options to MoveUp, MoveDown, Add or Remove Sprites
                            Rect controlRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight * 1f);
                            controlRect.width /= 8;

                            // Move sprite up.
                            bool guiEnabled = GUI.enabled;
                            if (i == 0) { GUI.enabled = false; }
                            if (GUI.Button(controlRect, "Up"))
                            {
                                SwapSpriteElement(i, i - 1);
                            }
                            GUI.enabled = guiEnabled;

                            // Move sprite down.
                            controlRect.x += controlRect.width;
                            if (i == arraySize - 1) { GUI.enabled = false; }
                            if (GUI.Button(controlRect, "Down"))
                            {
                                SwapSpriteElement(i, i + 1);
                            }
                            GUI.enabled = guiEnabled;

                            // Move sprite to new index
                            controlRect.x += controlRect.width * 2;
                            //if (i == arraySize - 1) { GUI.enabled = false; }
                            m_moveToIndex = EditorGUI.IntField(controlRect, m_moveToIndex);
                            controlRect.x -= controlRect.width;
                            if (GUI.Button(controlRect, "Goto"))
                            {
                                MoveSpriteElement(i, m_moveToIndex);
                            }
                            //controlRect.x += controlRect.width;
                            GUI.enabled = guiEnabled;

                            // Add new Sprite
                            controlRect.x += controlRect.width * 4;
                            if (GUI.Button(controlRect, "+"))
                            {
                                m_spriteInfoList_prop.arraySize += 1;

                                int index = m_spriteInfoList_prop.arraySize - 1;

                                SerializedProperty spriteInfo_prop = m_spriteInfoList_prop.GetArrayElementAtIndex(index);

                                // Copy properties of the selected element
                                CopySerializedProperty(m_spriteInfoList_prop.GetArrayElementAtIndex(elementIndex), ref spriteInfo_prop);

                                spriteInfo_prop.FindPropertyRelative("id").intValue = index;
                                serializedObject.ApplyModifiedProperties();

                                m_isSearchDirty = true;
                            }

                            // Delete selected Sprite
                            controlRect.x += controlRect.width;
                            if (m_selectedElement == -1) GUI.enabled = false;
                            if (GUI.Button(controlRect, "-"))
                            {
                                m_spriteInfoList_prop.DeleteArrayElementAtIndex(elementIndex);

                                m_selectedElement = -1;
                                serializedObject.ApplyModifiedProperties();

                                m_isSearchDirty = true;

                                return;
                            }


                        }
                    }
                }

                DisplayGlyphPageNavigation(arraySize, itemsPerPage);

                EditorGUIUtility.labelWidth = 40f;
                EditorGUIUtility.fieldWidth = 20f;

                GUILayout.Space(5f);

                // GLOBAL TOOLS
                #region Global Tools
                GUI.enabled = true;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                Rect rect = EditorGUILayout.GetControlRect(false, 40);
               
                float width = (rect.width - 75f) / 4;
                EditorGUI.LabelField(rect, "Global Offsets & Scale", EditorStyles.boldLabel);
                
                
                rect.x += 70;
                bool old_ChangedState = GUI.changed;

                GUI.changed = false;
                m_xOffset = EditorGUI.FloatField(new Rect(rect.x + 5f + width * 0, rect.y + 20, width - 5f, 18), new GUIContent("OX:"), m_xOffset);
                if (GUI.changed) UpdateGlobalProperty("xOffset", m_xOffset);
                
                m_yOffset = EditorGUI.FloatField(new Rect(rect.x + 5f + width * 1, rect.y + 20, width - 5f, 18), new GUIContent("OY:"), m_yOffset);
                if (GUI.changed) UpdateGlobalProperty("yOffset", m_yOffset);
                
                m_xAdvance = EditorGUI.FloatField(new Rect(rect.x + 5f + width * 2, rect.y + 20, width - 5f, 18), new GUIContent("ADV."), m_xAdvance);
                if (GUI.changed) UpdateGlobalProperty("xAdvance", m_xAdvance);
                
                m_scale = EditorGUI.FloatField(new Rect(rect.x + 5f + width * 3, rect.y + 20, width - 5f, 18), new GUIContent("SF."), m_scale);
                if (GUI.changed) UpdateGlobalProperty("scale", m_scale);

                EditorGUILayout.EndVertical();
                #endregion

                GUI.changed = old_ChangedState;
                
            }


            if (serializedObject.ApplyModifiedProperties() || evt_cmd == k_UndoRedo || isAssetDirty)
            {
                isAssetDirty = false;
                EditorUtility.SetDirty(target);
                //TMPro_EditorUtility.RepaintAll(); // Consider SetDirty
            }

            // Clear selection if mouse event was not consumed. 
            GUI.enabled = true;
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
                m_selectedElement = -1;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="arraySize"></param>
        /// <param name="itemsPerPage"></param>
        void DisplayGlyphPageNavigation(int arraySize, int itemsPerPage)
        {
            Rect pagePos = EditorGUILayout.GetControlRect(false, 20);
            pagePos.width /= 3;

            int shiftMultiplier = Event.current.shift ? 10 : 1; // Page + Shift goes 10 page forward

            // Previous Page
            GUI.enabled = m_page > 0;

            if (GUI.Button(pagePos, "Previous Page"))
            {
                m_page -= 1 * shiftMultiplier;
                //m_isNewPage = true;
            }

            // Page Counter
            GUI.enabled = true;
            pagePos.x += pagePos.width;
            int totalPages = (int)(arraySize / (float)itemsPerPage + 0.999f);
            GUI.Label(pagePos, "Page " + (m_page + 1) + " / " + totalPages, TMP_UIStyleManager.centeredLabel);

            // Next Page
            pagePos.x += pagePos.width;
            GUI.enabled = itemsPerPage * (m_page + 1) < arraySize;

            if (GUI.Button(pagePos, "Next Page"))
            {
                m_page += 1 * shiftMultiplier;
                //m_isNewPage = true;
            }

            // Clamp page range
            m_page = Mathf.Clamp(m_page, 0, arraySize / itemsPerPage);

            GUI.enabled = true;
        }


        /// <summary>
        /// Method to update the properties of all sprites
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        void UpdateGlobalProperty(string property, float value)
        {
            int arraySize = m_spriteInfoList_prop.arraySize;

            for (int i = 0; i < arraySize; i++)
            {
                SerializedProperty spriteInfo = m_spriteInfoList_prop.GetArrayElementAtIndex(i);
                spriteInfo.FindPropertyRelative(property).floatValue = value;
            }

            GUI.changed = false;
        }

        // Check if any of the Style elements were clicked on.
        private bool DoSelectionCheck(Rect selectionArea)
        {
            Event currentEvent = Event.current;

            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    if (selectionArea.Contains(currentEvent.mousePosition) && currentEvent.button == 0)
                    {
                        currentEvent.Use();
                        return true;
                    }
                    break;
            }

            return false;
        }


        /// <summary>
        /// Swap the sprite item at the currently selected array index to another index.
        /// </summary>
        /// <param name="selectedIndex">Selected index.</param>
        /// <param name="newIndex">New index.</param>
        void SwapSpriteElement(int selectedIndex, int newIndex)
        {
            m_spriteInfoList_prop.MoveArrayElement(selectedIndex, newIndex);
            m_spriteInfoList_prop.GetArrayElementAtIndex(selectedIndex).FindPropertyRelative("id").intValue = selectedIndex;
            m_spriteInfoList_prop.GetArrayElementAtIndex(newIndex).FindPropertyRelative("id").intValue = newIndex;
            m_selectedElement = newIndex;
            m_isSearchDirty = true;
        }

        /// <summary>
        /// Move Sprite Element at selected index to another index and reorder sprite list.
        /// </summary>
        /// <param name="selectedIndex"></param>
        /// <param name="newIndex"></param>
        void MoveSpriteElement(int selectedIndex, int newIndex)
        {
            m_spriteInfoList_prop.MoveArrayElement(selectedIndex, newIndex);

            // Reorder Sprite Element Index
            for (int i = 0; i < m_spriteInfoList_prop.arraySize; i++)
                m_spriteInfoList_prop.GetArrayElementAtIndex(i).FindPropertyRelative("id").intValue = i;

            m_selectedElement = newIndex;
            m_isSearchDirty = true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        void CopySerializedProperty(SerializedProperty source, ref SerializedProperty target)
        {
            target.FindPropertyRelative("name").stringValue = source.FindPropertyRelative("name").stringValue;
            target.FindPropertyRelative("hashCode").intValue = source.FindPropertyRelative("hashCode").intValue;
            target.FindPropertyRelative("x").floatValue = source.FindPropertyRelative("x").floatValue;
            target.FindPropertyRelative("y").floatValue = source.FindPropertyRelative("y").floatValue;
            target.FindPropertyRelative("width").floatValue = source.FindPropertyRelative("width").floatValue;
            target.FindPropertyRelative("height").floatValue = source.FindPropertyRelative("height").floatValue;
            target.FindPropertyRelative("xOffset").floatValue = source.FindPropertyRelative("xOffset").floatValue;
            target.FindPropertyRelative("yOffset").floatValue = source.FindPropertyRelative("yOffset").floatValue;
            target.FindPropertyRelative("xAdvance").floatValue = source.FindPropertyRelative("xAdvance").floatValue;
            target.FindPropertyRelative("scale").floatValue = source.FindPropertyRelative("scale").floatValue;
            target.FindPropertyRelative("sprite").objectReferenceValue = source.FindPropertyRelative("sprite").objectReferenceValue;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        void SearchGlyphTable(string searchPattern, ref List<int> searchResults)
        {
            if (searchResults == null) searchResults = new List<int>();
            searchResults.Clear();

            int arraySize = m_spriteInfoList_prop.arraySize;

            for (int i = 0; i < arraySize; i++)
            {
                SerializedProperty sourceSprite = m_spriteInfoList_prop.GetArrayElementAtIndex(i);

                // Check for potential match against decimal id
                int id = sourceSprite.FindPropertyRelative("id").intValue;
                if (id.ToString().Contains(searchPattern))
                    searchResults.Add(i);

                // Check for potential match against name
                string name = sourceSprite.FindPropertyRelative("name").stringValue.ToLower(System.Globalization.CultureInfo.InvariantCulture).Trim();
                if (name.Contains(searchPattern))
                    searchResults.Add(i);
            }
        }

    }
}