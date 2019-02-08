using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;



namespace TMPro.EditorUtilities
{

    [CustomPropertyDrawer(typeof(TMP_FontWeights))]
    public class FontWeightDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty prop_regular = property.FindPropertyRelative("regularTypeface");
            SerializedProperty prop_italic = property.FindPropertyRelative("italicTypeface");

            float width = position.width;

            position.width = EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(position, label);

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // NORMAL FACETYPE
            if (label.text[0] == '4') GUI.enabled = false;
            position.x += position.width; position.width = (width - position.width) / 2;
            EditorGUI.PropertyField(position, prop_regular, GUIContent.none);

            // ITALIC FACETYPE
            GUI.enabled = true;
            position.x += position.width;
            EditorGUI.PropertyField(position, prop_italic, GUIContent.none);

            EditorGUI.indentLevel = oldIndent;
        }
    }



    [CustomEditor(typeof(TMP_FontAsset))]
    public class TMP_FontAssetEditor : Editor
    {
        private struct UI_PanelState
        {
            public static bool fontSubAssetsPanel = true;
            public static bool fontWeightPanel = true;
            public static bool fallbackFontAssetPanel = true;
            public static bool glyphInfoPanel = false;
            public static bool kerningInfoPanel = false;
        }

        private struct Warning
        {
            public bool isEnabled;
            public double expirationTime;
        }

        private int m_CurrentGlyphPage = 0;
        private int m_CurrentKerningPage = 0;

        private int m_SelectedGlyphRecord = -1;
        private int m_SelectedAdjustmentRecord = -1;

        private string m_dstGlyphID;
        private const string k_placeholderUnicodeHex = "<i>Unicode Hex ID</i>";
        private string m_unicodeHexLabel = k_placeholderUnicodeHex;

        private Warning m_AddGlyphWarning;


        private string m_GlyphSearchPattern;
        private List<int> m_GlyphSearchList;

        private string m_KerningTableSearchPattern;
        private List<int> m_KerningTableSearchList;
        
        private bool m_isSearchDirty;

        private const string k_UndoRedo = "UndoRedoPerformed";

        private SerializedProperty font_atlas_prop;
        private SerializedProperty font_material_prop;

        private SerializedProperty fontWeights_prop;

        //private SerializedProperty fallbackFontAssets_prop;
        private ReorderableList m_list;

        private SerializedProperty font_normalStyle_prop;
        private SerializedProperty font_normalSpacing_prop;

        private SerializedProperty font_boldStyle_prop;
        private SerializedProperty font_boldSpacing_prop;

        private SerializedProperty font_italicStyle_prop;
        private SerializedProperty font_tabSize_prop;

        private SerializedProperty m_fontInfo_prop;
        private SerializedProperty m_glyphInfoList_prop;

        private SerializedProperty m_kerningInfo_prop;
        private KerningTable m_kerningTable;
        private SerializedProperty m_kerningPairs_prop;

        private SerializedProperty m_kerningPair_prop;

        private TMP_FontAsset m_fontAsset;

        private Material[] m_materialPresets;

        private bool isAssetDirty = false;

        private int errorCode;

        private System.DateTime timeStamp;

        public void OnEnable()
        {
            font_atlas_prop = serializedObject.FindProperty("atlas");
            font_material_prop = serializedObject.FindProperty("material");

            fontWeights_prop = serializedObject.FindProperty("fontWeights");

            m_list = new ReorderableList(serializedObject, serializedObject.FindProperty("fallbackFontAssets"), true, true, true, true);

            m_list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = m_list.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
            };

            m_list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Fallback Font Asset List");
            };

            font_normalStyle_prop = serializedObject.FindProperty("normalStyle");
            font_normalSpacing_prop = serializedObject.FindProperty("normalSpacingOffset");

            font_boldStyle_prop = serializedObject.FindProperty("boldStyle");
            font_boldSpacing_prop = serializedObject.FindProperty("boldSpacing");

            font_italicStyle_prop = serializedObject.FindProperty("italicStyle");
            font_tabSize_prop = serializedObject.FindProperty("tabSize");

            m_fontInfo_prop = serializedObject.FindProperty("m_fontInfo");
            m_glyphInfoList_prop = serializedObject.FindProperty("m_glyphInfoList");
            m_kerningInfo_prop = serializedObject.FindProperty("m_kerningInfo");
            m_kerningPair_prop = serializedObject.FindProperty("m_kerningPair");
            m_kerningPairs_prop = m_kerningInfo_prop.FindPropertyRelative("kerningPairs");

            m_fontAsset = target as TMP_FontAsset;
            m_kerningTable = m_fontAsset.kerningInfo;

            m_materialPresets = TMP_EditorUtility.FindMaterialReferences(m_fontAsset);

            m_GlyphSearchList = new List<int>();
            m_KerningTableSearchList = new List<int>();
        }


        public override void OnInspectorGUI()
        {
            // Check Warnings


            //Debug.Log("OnInspectorGUI Called.");
            Event currentEvent = Event.current;

            serializedObject.Update();

            // TextMeshPro Font Info Panel
            Rect rect = EditorGUILayout.GetControlRect();

            
            GUI.Label(rect, "Face Info", EditorStyles.boldLabel);

            rect.x += rect.width - 130f;
            rect.width = 130f;

            if (GUI.Button(rect, "Update Atlas Texture"))
            {
                TMPro_FontAssetCreatorWindow.ShowFontAtlasCreatorWindow(target as TMP_FontAsset);
            }


            EditorGUI.indentLevel = 1;

            GUI.enabled = false; // Lock UI

            float labelWidth = EditorGUIUtility.labelWidth;
            float fieldWidth = EditorGUIUtility.fieldWidth;

            EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("Name"), new GUIContent("Font Source"));
            EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("PointSize"));

            GUI.enabled = true;
            EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("Scale"));
            EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("LineHeight"));

            EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("Ascender"));
            EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("CapHeight"));
            EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("Baseline"));
            EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("Descender"));
            EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("Underline"), new GUIContent("Underline Offset"));
            EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("strikethrough"), new GUIContent("Strikethrough Offset"));
            EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("SuperscriptOffset"));
            EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("SubscriptOffset"));
            
            SerializedProperty subSize_prop = m_fontInfo_prop.FindPropertyRelative("SubSize");
            EditorGUILayout.PropertyField(subSize_prop, new GUIContent("Super / Subscript Size"));
            subSize_prop.floatValue = Mathf.Clamp(subSize_prop.floatValue, 0.25f, 1f);
            

            GUI.enabled = false;
            //EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("Padding"));

            //GUILayout.label("Atlas Size");
            EditorGUI.indentLevel = 1;
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("Padding"));
            EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("AtlasWidth"), new GUIContent("Width"));
            EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("AtlasHeight"), new GUIContent("Height"));
            
            GUI.enabled = true;

            EditorGUILayout.Space();

            EditorGUI.indentLevel = 0;
            UI_PanelState.fontSubAssetsPanel = EditorGUILayout.Foldout(UI_PanelState.fontSubAssetsPanel, new GUIContent("Font Sub-Assets"), true, TMP_UIStyleManager.boldFoldout);

            if (UI_PanelState.fontSubAssetsPanel)
            {
                GUI.enabled = false;
                EditorGUI.indentLevel = 1;
                EditorGUILayout.PropertyField(font_atlas_prop, new GUIContent("Font Atlas"));
                EditorGUILayout.PropertyField(font_material_prop, new GUIContent("Font Material"));
                GUI.enabled = true;
                EditorGUILayout.Space();
            }
            
            string evt_cmd = Event.current.commandName; // Get Current Event CommandName to check for Undo Events

            // FONT SETTINGS
            EditorGUI.indentLevel = 0;
            UI_PanelState.fontWeightPanel = EditorGUILayout.Foldout(UI_PanelState.fontWeightPanel, new GUIContent("Font Weights", "The Font Assets that will be used for different font weights and the settings used to simulate a typeface when no asset is available."), true, TMP_UIStyleManager.boldFoldout);

            if (UI_PanelState.fontWeightPanel)
            {
                EditorGUIUtility.labelWidth *= 0.75f;
                EditorGUIUtility.fieldWidth *= 0.25f;

                EditorGUILayout.BeginVertical();
                EditorGUI.indentLevel = 1;
                rect = EditorGUILayout.GetControlRect(true);
                rect.x += EditorGUIUtility.labelWidth;
                rect.width = (rect.width - EditorGUIUtility.labelWidth) / 2f;
                GUI.Label(rect, "Normal Style", EditorStyles.boldLabel);
                rect.x += rect.width;
                GUI.Label(rect, "Italic Style", EditorStyles.boldLabel);
                
                EditorGUI.indentLevel = 1;

                //EditorGUILayout.PropertyField(fontWeights_prop.GetArrayElementAtIndex(1), new GUIContent("100 - Thin"));
                //EditorGUILayout.PropertyField(fontWeights_prop.GetArrayElementAtIndex(2), new GUIContent("200 - Extra-Light"));
                //EditorGUILayout.PropertyField(fontWeights_prop.GetArrayElementAtIndex(3), new GUIContent("300 - Light"));
                EditorGUILayout.PropertyField(fontWeights_prop.GetArrayElementAtIndex(4), new GUIContent("400 - Regular"));
                //EditorGUILayout.PropertyField(fontWeights_prop.GetArrayElementAtIndex(5), new GUIContent("500 - Medium"));
                //EditorGUILayout.PropertyField(fontWeights_prop.GetArrayElementAtIndex(6), new GUIContent("600 - Demi-Bold"));
                EditorGUILayout.PropertyField(fontWeights_prop.GetArrayElementAtIndex(7), new GUIContent("700 - Bold"));
                //EditorGUILayout.PropertyField(fontWeights_prop.GetArrayElementAtIndex(8), new GUIContent("800 - Heavy"));
                //EditorGUILayout.PropertyField(fontWeights_prop.GetArrayElementAtIndex(9), new GUIContent("900 - Black"));

                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(font_normalStyle_prop, new GUIContent("Normal Weight"));
                font_normalStyle_prop.floatValue = Mathf.Clamp(font_normalStyle_prop.floatValue, -3.0f, 3.0f);
                if (GUI.changed || evt_cmd == k_UndoRedo)
                {
                    GUI.changed = false;

                    // Modify the material property on matching material presets.
                    for (int i = 0; i < m_materialPresets.Length; i++)
                        m_materialPresets[i].SetFloat("_WeightNormal", font_normalStyle_prop.floatValue);
                }

                EditorGUILayout.PropertyField(font_boldStyle_prop, new GUIContent("Bold Weight"));
                font_boldStyle_prop.floatValue = Mathf.Clamp(font_boldStyle_prop.floatValue, -3.0f, 3.0f);
                if (GUI.changed || evt_cmd == k_UndoRedo)
                {
                    GUI.changed = false;

                    // Modify the material property on matching material presets.
                    for (int i = 0; i < m_materialPresets.Length; i++)
                        m_materialPresets[i].SetFloat("_WeightBold", font_boldStyle_prop.floatValue);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(font_normalSpacing_prop, new GUIContent("Spacing Offset"));
                font_normalSpacing_prop.floatValue = Mathf.Clamp(font_normalSpacing_prop.floatValue, -100, 100);
                if (GUI.changed || evt_cmd == k_UndoRedo)
                {
                    GUI.changed = false;
                }

                EditorGUILayout.PropertyField(font_boldSpacing_prop, new GUIContent("Bold Spacing"));
                font_boldSpacing_prop.floatValue = Mathf.Clamp(font_boldSpacing_prop.floatValue, 0, 100);
                if (GUI.changed || evt_cmd == k_UndoRedo)
                {
                    GUI.changed = false;
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(font_italicStyle_prop, new GUIContent("Italic Style"));
                font_italicStyle_prop.intValue = Mathf.Clamp(font_italicStyle_prop.intValue, 15, 60);
                
                EditorGUILayout.PropertyField(font_tabSize_prop, new GUIContent("Tab Multiple"));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUIUtility.labelWidth = 0;
            EditorGUIUtility.fieldWidth = 0;
            
            // FALLBACK FONT ASSETS
            EditorGUI.indentLevel = 0;
            UI_PanelState.fallbackFontAssetPanel = EditorGUILayout.Foldout(UI_PanelState.fallbackFontAssetPanel, new GUIContent("Fallback Font Assets", "Select the Font Assets that will be searched and used as fallback when characters are missing from this font asset."), true, TMP_UIStyleManager.boldFoldout);

            if (UI_PanelState.fallbackFontAssetPanel)
            {
                EditorGUIUtility.labelWidth = 120;
                EditorGUI.indentLevel = 0;

                m_list.DoLayoutList();
                EditorGUILayout.Space();
            }

            // GLYPH INFO TABLE
            #region Glyph Table
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUIUtility.fieldWidth = fieldWidth;
            EditorGUI.indentLevel = 0;

            UI_PanelState.glyphInfoPanel = EditorGUILayout.Foldout(UI_PanelState.glyphInfoPanel, new GUIContent("Glyph Table"), true, TMP_UIStyleManager.boldFoldout);

            if (UI_PanelState.glyphInfoPanel)
            {
                int arraySize = m_glyphInfoList_prop.arraySize;
                int itemsPerPage = 15;

                // Display Glyph Management Tools
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    // Search Bar implementation
                    #region DISPLAY SEARCH BAR
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUIUtility.labelWidth = 130f;
                        EditorGUI.BeginChangeCheck();
                        string searchPattern = EditorGUILayout.TextField("Glyph Search", m_GlyphSearchPattern, "SearchTextField");
                        if (EditorGUI.EndChangeCheck() || m_isSearchDirty)
                        {
                            if (string.IsNullOrEmpty(searchPattern) == false)
                            {
                                m_GlyphSearchPattern = searchPattern;

                                // Search Glyph Table for potential matches
                                SearchGlyphTable(m_GlyphSearchPattern, ref m_GlyphSearchList);
                            }
                            else
                                m_GlyphSearchPattern = null;

                            m_isSearchDirty = false;
                        }

                        string styleName = string.IsNullOrEmpty(m_GlyphSearchPattern) ? "SearchCancelButtonEmpty" : "SearchCancelButton";
                        if (GUILayout.Button(GUIContent.none, styleName))
                        {
                            GUIUtility.keyboardControl = 0;
                            m_GlyphSearchPattern = string.Empty;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    #endregion

                    // Display Page Navigation
                    if (!string.IsNullOrEmpty(m_GlyphSearchPattern))
                        arraySize = m_GlyphSearchList.Count;

                    DisplayPageNavigation(ref m_CurrentGlyphPage, arraySize, itemsPerPage);
                }
                EditorGUILayout.EndVertical();

                // Display Glyph Table Elements
                
                if (arraySize > 0)
                {
                    // Display each GlyphInfo entry using the GlyphInfo property drawer.
                    for (int i = itemsPerPage * m_CurrentGlyphPage; i < arraySize && i < itemsPerPage * (m_CurrentGlyphPage + 1); i++)
                    {
                        // Define the start of the selection region of the element.
                        Rect elementStartRegion = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));

                        int elementIndex = i;
                        if (!string.IsNullOrEmpty(m_GlyphSearchPattern))
                            elementIndex = m_GlyphSearchList[i];
                            
                        SerializedProperty glyphInfo = m_glyphInfoList_prop.GetArrayElementAtIndex(elementIndex);

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        
                        EditorGUI.BeginDisabledGroup(i != m_SelectedGlyphRecord);
                        {
                            EditorGUILayout.PropertyField(glyphInfo);
                        }
                        EditorGUI.EndDisabledGroup();

                        EditorGUILayout.EndVertical();

                        // Define the end of the selection region of the element.
                        Rect elementEndRegion = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));

                        // Check for Item selection
                        Rect selectionArea = new Rect(elementStartRegion.x, elementStartRegion.y, elementEndRegion.width, elementEndRegion.y - elementStartRegion.y);
                        if (DoSelectionCheck(selectionArea))
                        {
                            if (m_SelectedGlyphRecord == i)
                                m_SelectedGlyphRecord = -1;
                            else
                            {
                                m_SelectedGlyphRecord = i;
                                m_AddGlyphWarning.isEnabled = false;
                                m_unicodeHexLabel = k_placeholderUnicodeHex;
                                GUIUtility.keyboardControl = 0;
                            }
                        }

                        // Draw Selection Highlight and Glyph Options
                        if (m_SelectedGlyphRecord == i)
                        {
                            TMP_EditorUtility.DrawBox(selectionArea, 2f, new Color32(40, 192, 255, 255));

                            // Draw Glyph management options
                            Rect controlRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight * 1f);
                            float optionAreaWidth = controlRect.width * 0.6f;
                            float btnWidth = optionAreaWidth / 3;

                            Rect position = new Rect(controlRect.x + controlRect.width * .4f, controlRect.y, btnWidth, controlRect.height);

                            // Copy Selected Glyph to Target Glyph ID
                            GUI.enabled = !string.IsNullOrEmpty(m_dstGlyphID);
                            if (GUI.Button(position, new GUIContent("Copy to")))
                            {
                                GUIUtility.keyboardControl = 0;

                                // Convert Hex Value to Decimal
                                int dstGlyphID = TMP_TextUtilities.StringToInt(m_dstGlyphID);

                                //Add new glyph at target Unicode hex id.
                                if (!AddNewGlyph(elementIndex, dstGlyphID))
                                {
                                    m_AddGlyphWarning.isEnabled = true;
                                    m_AddGlyphWarning.expirationTime = EditorApplication.timeSinceStartup + 1;
                                }

                                m_dstGlyphID = string.Empty;
                                m_isSearchDirty = true;

                                TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, m_fontAsset);
                            }

                            // Target Glyph ID
                            GUI.enabled = true;
                            position.x += btnWidth;

                            GUI.SetNextControlName("GlyphID_Input");
                            m_dstGlyphID = EditorGUI.TextField(position, m_dstGlyphID);

                            // Placeholder text
                            EditorGUI.LabelField(position, new GUIContent(m_unicodeHexLabel, "The Unicode (Hex) ID of the duplicated Glyph"), TMP_UIStyleManager.label);

                            // Only filter the input when the destination glyph ID text field has focus.
                            if (GUI.GetNameOfFocusedControl() == "GlyphID_Input")
                            {
                                m_unicodeHexLabel = string.Empty;

                                //Filter out unwanted characters.
                                char chr = Event.current.character;
                                if ((chr < '0' || chr > '9') && (chr < 'a' || chr > 'f') && (chr < 'A' || chr > 'F'))
                                {
                                    Event.current.character = '\0';
                                }
                            }
                            else
                                m_unicodeHexLabel = k_placeholderUnicodeHex;


                            // Remove Glyph
                            position.x += btnWidth;
                            if (GUI.Button(position, "Remove"))
                            {
                                GUIUtility.keyboardControl = 0;

                                RemoveGlyphFromList(elementIndex);

                                isAssetDirty = true;
                                m_SelectedGlyphRecord = -1;
                                m_isSearchDirty = true;
                                break;
                            }

                            if (m_AddGlyphWarning.isEnabled && EditorApplication.timeSinceStartup < m_AddGlyphWarning.expirationTime)
                            {
                                EditorGUILayout.HelpBox("The Destination Glyph ID already exists", MessageType.Warning);
                            }

                        }
                    }
                }

                DisplayPageNavigation(ref m_CurrentGlyphPage, arraySize, itemsPerPage);

                EditorGUILayout.Space();
            }
            #endregion


            // KERNING TABLE PANEL
            #region Kerning Table
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUIUtility.fieldWidth = fieldWidth;
            EditorGUI.indentLevel = 0;

            UI_PanelState.kerningInfoPanel = EditorGUILayout.Foldout(UI_PanelState.kerningInfoPanel, new GUIContent("Glyph Adjustment Table"), true, TMP_UIStyleManager.boldFoldout);

            if (UI_PanelState.kerningInfoPanel)
            {
                int arraySize = m_kerningPairs_prop.arraySize;
                int itemsPerPage = 20;

                // Display Kerning Pair Management Tools
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    // Search Bar implementation
                    #region DISPLAY SEARCH BAR
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUIUtility.labelWidth = 150f;
                        EditorGUI.BeginChangeCheck();
                        string searchPattern = EditorGUILayout.TextField("Adjustment Pair Search", m_KerningTableSearchPattern, "SearchTextField");
                        if (EditorGUI.EndChangeCheck() || m_isSearchDirty)
                        {
                            if (string.IsNullOrEmpty(searchPattern) == false)
                            {
                                m_KerningTableSearchPattern = searchPattern;

                                // Search Glyph Table for potential matches
                                SearchKerningTable(m_KerningTableSearchPattern, ref m_KerningTableSearchList);
                            }
                            else
                                m_KerningTableSearchPattern = null;

                            m_isSearchDirty = false;
                        }

                        string styleName = string.IsNullOrEmpty(m_KerningTableSearchPattern) ? "SearchCancelButtonEmpty" : "SearchCancelButton";
                        if (GUILayout.Button(GUIContent.none, styleName))
                        {
                            GUIUtility.keyboardControl = 0;
                            m_KerningTableSearchPattern = string.Empty;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    #endregion

                    // Display Page Navigation
                    if (!string.IsNullOrEmpty(m_KerningTableSearchPattern))
                        arraySize = m_KerningTableSearchList.Count;

                    DisplayPageNavigation(ref m_CurrentKerningPage, arraySize, itemsPerPage);
                }
                EditorGUILayout.EndVertical();


                //Rect pos;
                //pos = EditorGUILayout.GetControlRect(false, 20);

                //pos.x += 5;
                //EditorGUI.LabelField(pos, "First Glyph", TMP_UIStyleManager.TMP_GUISkin.label);
                //pos.x += 100;
                //EditorGUI.LabelField(pos, "Adjustment Values", TMP_UIStyleManager.TMP_GUISkin.label);

                //pos.x = pos.width / 2 + 5;
                //EditorGUI.LabelField(pos, "Second Glyph", TMP_UIStyleManager.TMP_GUISkin.label);
                //pos.x += 100;
                //EditorGUI.LabelField(pos, "Adjustment Values", TMP_UIStyleManager.TMP_GUISkin.label);

                if (arraySize > 0)
                {
                    // Display each GlyphInfo entry using the GlyphInfo property drawer.
                    for (int i = itemsPerPage * m_CurrentKerningPage; i < arraySize && i < itemsPerPage * (m_CurrentKerningPage + 1); i++)
                    {
                        // Define the start of the selection region of the element.
                        Rect elementStartRegion = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));

                        int elementIndex = i;
                        if (!string.IsNullOrEmpty(m_KerningTableSearchPattern))
                            elementIndex = m_KerningTableSearchList[i];

                        SerializedProperty kerningInfo = m_kerningPairs_prop.GetArrayElementAtIndex(elementIndex);
                        
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        EditorGUI.BeginDisabledGroup(i != m_SelectedAdjustmentRecord);
                        {
                            EditorGUILayout.PropertyField(kerningInfo, new GUIContent("Selectable"));
                        }
                        EditorGUI.EndDisabledGroup();

                        EditorGUILayout.EndVertical();

                        // Define the end of the selection region of the element.
                        Rect elementEndRegion = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));

                        // Check for Item selection
                        Rect selectionArea = new Rect(elementStartRegion.x, elementStartRegion.y, elementEndRegion.width, elementEndRegion.y - elementStartRegion.y);
                        if (DoSelectionCheck(selectionArea))
                        {
                            if (m_SelectedAdjustmentRecord == i)
                            {
                                m_SelectedAdjustmentRecord = -1;
                            }
                            else
                            {
                                m_SelectedAdjustmentRecord = i;
                                GUIUtility.keyboardControl = 0;
                            }
                        }

                        // Draw Selection Highlight and Kerning Pair Options
                        if (m_SelectedAdjustmentRecord == i)
                        {
                            TMP_EditorUtility.DrawBox(selectionArea, 2f, new Color32(40, 192, 255, 255));

                            // Draw Glyph management options
                            Rect controlRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight * 1f);
                            float optionAreaWidth = controlRect.width;
                            float btnWidth = optionAreaWidth / 4;

                            Rect position = new Rect(controlRect.x + controlRect.width - btnWidth, controlRect.y, btnWidth, controlRect.height);

                            // Remove Kerning pair
                            GUI.enabled = true;
                            if (GUI.Button(position, "Remove"))
                            {
                                GUIUtility.keyboardControl = 0;

                                m_kerningTable.RemoveKerningPair(i);
                                m_fontAsset.ReadFontDefinition();

                                isAssetDirty = true;
                                m_SelectedAdjustmentRecord = -1;
                                m_isSearchDirty = true;
                                break;
                            }
                        }
                    }
                }

                DisplayPageNavigation(ref m_CurrentKerningPage, arraySize, itemsPerPage);

                GUILayout.Space(5);

                // Add new kerning pair
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(m_kerningPair_prop);
                }
                EditorGUILayout.EndVertical();

                if (GUILayout.Button("Add New Kerning Pair"))
                {
                    int firstGlyph = m_kerningPair_prop.FindPropertyRelative("m_FirstGlyph").intValue;
                    int secondGlyph = m_kerningPair_prop.FindPropertyRelative("m_SecondGlyph").intValue;

                    GlyphValueRecord firstGlyphAdjustments = GetGlyphAdjustments(m_kerningPair_prop.FindPropertyRelative("m_FirstGlyphAdjustments"));
                    GlyphValueRecord secondGlyphAdjustments = GetGlyphAdjustments(m_kerningPair_prop.FindPropertyRelative("m_SecondGlyphAdjustments"));

                    errorCode = m_kerningTable.AddGlyphPairAdjustmentRecord((uint)firstGlyph, firstGlyphAdjustments, (uint)secondGlyph, secondGlyphAdjustments);

                    // Sort Kerning Pairs & Reload Font Asset if new kerning pair was added.
                    if (errorCode != -1)
                    {
                        m_kerningTable.SortKerningPairs();
                        m_fontAsset.ReadFontDefinition();
                        serializedObject.ApplyModifiedProperties();
                        isAssetDirty = true;
                        m_isSearchDirty = true;
                    }
                    else
                    {
                        timeStamp = System.DateTime.Now.AddSeconds(5);
                    }

                    // Clear Add Kerning Pair Panel
                    // TODO
                }

                if (errorCode == -1)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Kerning Pair already <color=#ffff00>exists!</color>", TMP_UIStyleManager.label);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    if (System.DateTime.Now > timeStamp)
                        errorCode = 0;
                }
            }
            #endregion


            if (serializedObject.ApplyModifiedProperties() || evt_cmd == k_UndoRedo || isAssetDirty)
            {
                TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, m_fontAsset);

                isAssetDirty = false;
                EditorUtility.SetDirty(target);
            }


            // Clear selection if mouse event was not consumed. 
            GUI.enabled = true;
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
                m_SelectedAdjustmentRecord = -1;

        }



        void DisplayPageNavigation(ref int currentPage, int arraySize, int itemsPerPage)
        {
            Rect pagePos = EditorGUILayout.GetControlRect(false, 20);
            pagePos.width /= 3;

            int shiftMultiplier = Event.current.shift ? 10 : 1; // Page + Shift goes 10 page forward

            // Previous Page
            GUI.enabled = currentPage > 0;

            if (GUI.Button(pagePos, "Previous Page"))
                currentPage -= 1 * shiftMultiplier;


            // Page Counter
            GUI.enabled = true;
            pagePos.x += pagePos.width;
            int totalPages = (int)(arraySize / (float)itemsPerPage + 0.999f);
            GUI.Label(pagePos, "Page " + (currentPage + 1) + " / " + totalPages, TMP_UIStyleManager.centeredLabel);

            // Next Page
            pagePos.x += pagePos.width;
            GUI.enabled = itemsPerPage * (currentPage + 1) < arraySize;

            if (GUI.Button(pagePos, "Next Page"))
                currentPage += 1 * shiftMultiplier;

            // Clamp page range
            currentPage = Mathf.Clamp(currentPage, 0, arraySize / itemsPerPage);

            GUI.enabled = true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcGlyphID"></param>
        /// <param name="dstGlyphID"></param>
        bool AddNewGlyph(int srcIndex, int dstGlyphID)
        {
            // Make sure Destination Glyph ID doesn't already contain a Glyph
            if (m_fontAsset.characterDictionary.ContainsKey(dstGlyphID))
                return false;

            // Add new element to glyph list.
            m_glyphInfoList_prop.arraySize += 1;

            // Get a reference to the source glyph.
            SerializedProperty sourceGlyph = m_glyphInfoList_prop.GetArrayElementAtIndex(srcIndex);

            int dstIndex = m_glyphInfoList_prop.arraySize - 1;

            // Get a reference to the target / destination glyph.
            SerializedProperty targetGlyph = m_glyphInfoList_prop.GetArrayElementAtIndex(dstIndex);

            CopySerializedProperty(sourceGlyph, ref targetGlyph);

            // Update the ID of the glyph
            targetGlyph.FindPropertyRelative("id").intValue = dstGlyphID;

            serializedObject.ApplyModifiedProperties();

            m_fontAsset.SortGlyphs();

            m_fontAsset.ReadFontDefinition();

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="glyphID"></param>
        void RemoveGlyphFromList(int index)
        {
            if (index > m_glyphInfoList_prop.arraySize)
                return;

            m_glyphInfoList_prop.DeleteArrayElementAtIndex(index);

            serializedObject.ApplyModifiedProperties();

            m_fontAsset.ReadFontDefinition();
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

        GlyphValueRecord GetGlyphAdjustments (SerializedProperty property)
        {
            GlyphValueRecord record;
            record.xPlacement = property.FindPropertyRelative("xPlacement").floatValue;
            record.yPlacement = property.FindPropertyRelative("yPlacement").floatValue;
            record.xAdvance = property.FindPropertyRelative("xAdvance").floatValue;
            record.yAdvance = property.FindPropertyRelative("yAdvance").floatValue;

            return record;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        void CopySerializedProperty(SerializedProperty source, ref SerializedProperty target)
        {
            // TODO : Should make a generic function which copies each of the properties.
            target.FindPropertyRelative("id").intValue = source.FindPropertyRelative("id").intValue;
            target.FindPropertyRelative("x").floatValue = source.FindPropertyRelative("x").floatValue;
            target.FindPropertyRelative("y").floatValue = source.FindPropertyRelative("y").floatValue;
            target.FindPropertyRelative("width").floatValue = source.FindPropertyRelative("width").floatValue;
            target.FindPropertyRelative("height").floatValue = source.FindPropertyRelative("height").floatValue;
            target.FindPropertyRelative("xOffset").floatValue = source.FindPropertyRelative("xOffset").floatValue;
            target.FindPropertyRelative("yOffset").floatValue = source.FindPropertyRelative("yOffset").floatValue;
            target.FindPropertyRelative("xAdvance").floatValue = source.FindPropertyRelative("xAdvance").floatValue;
            target.FindPropertyRelative("scale").floatValue = source.FindPropertyRelative("scale").floatValue;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        void SearchGlyphTable (string searchPattern, ref List<int> searchResults)
        {
            if (searchResults == null) searchResults = new List<int>();

            searchResults.Clear();

            int arraySize = m_glyphInfoList_prop.arraySize;

            for (int i = 0; i < arraySize; i++)
            {
                SerializedProperty sourceGlyph = m_glyphInfoList_prop.GetArrayElementAtIndex(i);

                int id = sourceGlyph.FindPropertyRelative("id").intValue;

                // Check for potential match against a character.
                if (searchPattern.Length == 1 && id == searchPattern[0])
                    searchResults.Add(i);

                // Check for potential match against decimal id
                if (id.ToString().Contains(searchPattern))
                    searchResults.Add(i);

                if (id.ToString("x").Contains(searchPattern))
                    searchResults.Add(i);

                if (id.ToString("X").Contains(searchPattern))
                    searchResults.Add(i);
            }
        }

        void SearchKerningTable(string searchPattern, ref List<int> searchResults)
        {
            if (searchResults == null) searchResults = new List<int>();

            searchResults.Clear();

            int arraySize = m_kerningPairs_prop.arraySize;

            for (int i = 0; i < arraySize; i++)
            {
                SerializedProperty sourceGlyph = m_kerningPairs_prop.GetArrayElementAtIndex(i);

                int firstGlyph = sourceGlyph.FindPropertyRelative("m_FirstGlyph").intValue;
                int secondGlyph = sourceGlyph.FindPropertyRelative("m_SecondGlyph").intValue;

                if (searchPattern.Length == 1)
                {
                    if (firstGlyph == searchPattern[0])
                    {
                        searchResults.Add(i);
                        continue;
                    }

                    if (secondGlyph == searchPattern[0])
                    {
                        searchResults.Add(i);
                        continue;
                    }
                }

                if (searchPattern.Length == 2)
                {
                    if (firstGlyph == searchPattern[0] && secondGlyph == searchPattern[1])
                    {
                        searchResults.Add(i);
                        continue;
                    }
                }

                if (firstGlyph.ToString().Contains(searchPattern))
                {
                    searchResults.Add(i);
                    continue;
                }

                //if (firstGlyph.ToString("x").Contains(searchPattern))
                //{
                //    searchResults.Add(i);
                //    continue;
                //}

                //if (firstGlyph.ToString("X").Contains(searchPattern))
                //{
                //    searchResults.Add(i);
                //    continue;
                //}

                if (secondGlyph.ToString().Contains(searchPattern))
                {
                    searchResults.Add(i);
                    continue;
                }

                //if (secondGlyph.ToString("x").Contains(searchPattern))
                //{
                //    searchResults.Add(i);
                //    continue;
                //}

                //if (secondGlyph.ToString("X").Contains(searchPattern))
                //{
                //    searchResults.Add(i);
                //    continue;
                //}
            }
        }
    }
}