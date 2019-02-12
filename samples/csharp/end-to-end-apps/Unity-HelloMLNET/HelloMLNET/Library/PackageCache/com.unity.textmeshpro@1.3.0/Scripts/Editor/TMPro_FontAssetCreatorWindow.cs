using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.IO;
using Object = UnityEngine.Object;

namespace TMPro.EditorUtilities
{
    public class TMPro_FontAssetCreatorWindow : EditorWindow
    {
        [MenuItem("Window/TextMeshPro/Font Asset Creator", false, 2025)]
        public static void ShowFontAtlasCreatorWindow()
        {
            var window = GetWindow<TMPro_FontAssetCreatorWindow>();
            window.titleContent = new GUIContent("Font Asset Creator");
            window.Focus();

            // Make sure TMP Essential Resources have been imported.
            window.CheckEssentialResources();
        }


        public static void ShowFontAtlasCreatorWindow(Font sourceFontFile)
        {
            var window = GetWindow<TMPro_FontAssetCreatorWindow>();

            window.titleContent = new GUIContent("Font Asset Creator");
            window.Focus();

            // Override selected font asset
            window.ClearGeneratedData();
            window.m_SelectedFontAsset = null;
            window.m_LegacyFontAsset = null;
            window.m_SourceFontFile = sourceFontFile;

            // Make sure TMP Essential Resources have been imported.
            window.CheckEssentialResources();
        }


        public static void ShowFontAtlasCreatorWindow(TMP_FontAsset fontAsset)
        {
            var window = GetWindow<TMPro_FontAssetCreatorWindow>();

            window.titleContent = new GUIContent("Font Creator");
            window.Focus();

            // Clear any previously generated data
            window.ClearGeneratedData();
            window.m_LegacyFontAsset = null;

            // Load font asset creation settings if we have valid settings
            if (!string.IsNullOrEmpty(fontAsset.creationSettings.sourceFontFileGUID))
            {
                window.LoadFontCreationSettings(fontAsset.creationSettings);
                window.m_SavedFontAtlas = fontAsset.atlas;
            }
            else
            {
                window.m_WarningMessage = "Font Asset [" + fontAsset.name + "] does not contain any previous \"Font Asset Creation Settings\". This usually means [" + fontAsset.name + "] was created before this new functionality was added.";
                window.m_SourceFontFile = null;
                window.m_LegacyFontAsset = fontAsset;
            }

            // Even if we don't have any saved generation settings, we still want to pre-select the source font file.
            window.m_SelectedFontAsset = fontAsset;

            // Make sure TMP Essential Resources have been imported.
            window.CheckEssentialResources();
        }
        
        [System.Serializable]
        class FontAssetCreationSettingsContainer
        {
            public List<FontAssetCreationSettings> fontAssetCreationSettings;
        }
        
        FontAssetCreationSettingsContainer m_FontAssetCreationSettingsContainer;
        
        //static readonly string[] m_FontCreationPresets = new string[] { "Recent 1", "Recent 2", "Recent 3", "Recent 4" };
        int m_FontAssetCreationSettingsCurrentIndex = 0;
        //private bool m_IsFontAssetOpenForEdit = false;
        const string k_FontAssetCreationSettingsContainerKey = "TextMeshPro.FontAssetCreator.RecentFontAssetCreationSettings.Container";
        const string k_FontAssetCreationSettingsCurrentIndexKey = "TextMeshPro.FontAssetCreator.RecentFontAssetCreationSettings.CurrentIndex";
        const float k_TwoColumnControlsWidth = 335f;

        // Diagnostics
        System.Diagnostics.Stopwatch m_StopWatch;
        
        string[] m_FontSizingOptions = { "Auto Sizing", "Custom Size" };
        int m_PointSizeSamplingMode;
        string[] m_FontResolutionLabels = { "16","32", "64", "128", "256", "512", "1024", "2048", "4096", "8192" };
        int[] m_FontAtlasResolutions = { 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };
        string[] m_FontCharacterSets = { "ASCII", "Extended ASCII", "ASCII Lowercase", "ASCII Uppercase", "Numbers + Symbols", "Custom Range", "Unicode Range (Hex)", "Custom Characters", "Characters from File" };
        enum FontPackingModes { Fast = 0, Optimum = 4 };
        FontPackingModes m_PackingMode = FontPackingModes.Fast;

        int m_CharacterSetSelectionMode;

        string m_CharacterSequence = "";
        string m_OutputFeedback = "";
        string m_WarningMessage;
        const string k_OutputNameLabel = "Font: ";
        const string k_OutputSizeLabel = "Pt. Size: ";
        const string k_OutputCountLabel = "Characters packed: ";
        int m_CharacterCount;
        Vector2 m_ScrollPosition;
        Vector2 m_OutputScrollPosition;
        
        bool m_IsRepaintNeeded;
        
        float m_RenderingProgress;
        bool m_IsRenderingDone;
        bool m_IsProcessing;
        bool m_IsGenerationDisabled;
        bool m_IsGenerationCancelled;

        Object m_SourceFontFile;
        TMP_FontAsset m_SelectedFontAsset;
        TMP_FontAsset m_LegacyFontAsset;
        TMP_FontAsset m_ReferencedFontAsset;

        TextAsset m_CharacterList;
        int m_PointSize;

        int m_Padding = 5;
        FaceStyles m_FontStyle = FaceStyles.Normal;
        float m_FontStyleValue = 2;
        RenderModes m_RenderMode = RenderModes.DistanceField16;
        int m_AtlasWidth = 512;
        int m_AtlasHeight = 512;

        FT_FaceInfo m_FontFaceInfo;
        FT_GlyphInfo[] m_FontGlyphInfo;
        byte[] m_TextureBuffer;
        Texture2D m_FontAtlas;
        Texture2D m_SavedFontAtlas;
        
        bool m_IncludeKerningPairs;
        int[] m_KerningSet;
        
        bool m_Locked;
        bool m_IsFontAtlasInvalid;

        public void OnEnable()
        {
            minSize = new Vector2(315, minSize.y);

            // Used for Diagnostics
            m_StopWatch = new System.Diagnostics.Stopwatch();

            // Initialize & Get shader property IDs.
            ShaderUtilities.GetShaderPropertyIDs();

            // Load last selected preset if we are not already in the process of regenerating an existing font asset (via the Context menu)
            if (EditorPrefs.HasKey(k_FontAssetCreationSettingsContainerKey))
            {
                if (m_FontAssetCreationSettingsContainer == null)
                    m_FontAssetCreationSettingsContainer = JsonUtility.FromJson<FontAssetCreationSettingsContainer>(EditorPrefs.GetString(k_FontAssetCreationSettingsContainerKey));

                if (m_FontAssetCreationSettingsContainer.fontAssetCreationSettings != null && m_FontAssetCreationSettingsContainer.fontAssetCreationSettings.Count > 0)
                {
                    // Load Font Asset Creation Settings preset.
                    if (EditorPrefs.HasKey(k_FontAssetCreationSettingsCurrentIndexKey))
                        m_FontAssetCreationSettingsCurrentIndex = EditorPrefs.GetInt(k_FontAssetCreationSettingsCurrentIndexKey);

                    LoadFontCreationSettings(m_FontAssetCreationSettingsContainer.fontAssetCreationSettings[m_FontAssetCreationSettingsCurrentIndex]);
                }
            }

            // Debug Link to received message from Native Code
            //TMPro_FontPlugin.LinkDebugLog(); // Link with C++ Plugin to get Debug output
        }

        public void OnDisable()
        {
            //Debug.Log("TextMeshPro Editor Window has been disabled.");

            // Cancel font asset generation just in case one is in progress.
            TMPro_FontPlugin.SendCancellationRequest(CancellationRequestType.WindowClosed);

            // Destroy Engine only if it has been initialized already
            TMPro_FontPlugin.Destroy_FontEngine();
            
            if (m_FontAtlas != null && EditorUtility.IsPersistent(m_FontAtlas) == false)
            {
                //Debug.Log("Destroying font_Atlas!");
                DestroyImmediate(m_FontAtlas);
            }
            
            if (File.Exists("Assets/TextMesh Pro/Glyph Report.txt"))
            {
                File.Delete("Assets/TextMesh Pro/Glyph Report.txt");
                File.Delete("Assets/TextMesh Pro/Glyph Report.txt.meta");

                AssetDatabase.Refresh();
            }

            // Save Font Asset Creation Settings Index
            SaveCreationSettingsToEditorPrefs(SaveFontCreationSettings());
            EditorPrefs.SetInt(k_FontAssetCreationSettingsCurrentIndexKey, m_FontAssetCreationSettingsCurrentIndex);

            // Unregister to event
            TMPro_EventManager.RESOURCE_LOAD_EVENT.Remove(ON_RESOURCES_LOADED);

            Resources.UnloadUnusedAssets();
        }


        // Event received when TMP resources have been loaded.
        void ON_RESOURCES_LOADED()
        {
            TMPro_EventManager.RESOURCE_LOAD_EVENT.Remove(ON_RESOURCES_LOADED);

            m_IsGenerationDisabled = false;
        }

        // Make sure TMP Essential Resources have been imported.
        void CheckEssentialResources()
        {
            if (TMP_Settings.instance == null)
            {
                if (m_IsGenerationDisabled == false)
                    TMPro_EventManager.RESOURCE_LOAD_EVENT.Add(ON_RESOURCES_LOADED);

                m_IsGenerationDisabled = true;
            }
        }


        public void OnGUI()
        {
            GUILayout.BeginHorizontal();
            DrawControls();
            if (position.width > position.height && position.width > k_TwoColumnControlsWidth)
            {
                DrawPreview();
            }
            GUILayout.EndHorizontal();
        }


        public void Update()
        {
            if (m_IsRepaintNeeded)
            {
                //Debug.Log("Repainting...");
                m_IsRepaintNeeded = false;
                Repaint();
            }

            // Update Progress bar is we are Rendering a Font.
            if (m_IsProcessing)
            {
                m_RenderingProgress = TMPro_FontPlugin.Check_RenderProgress();

                m_IsRepaintNeeded = true;
            }

            // Update Feedback Window & Create Font Texture once Rendering is done.
            if (m_IsRenderingDone)
            {
                // Stop StopWatch
                m_StopWatch.Stop();
                Debug.Log("Font Atlas generation completed in: " + m_StopWatch.Elapsed.TotalMilliseconds.ToString("0.000 ms."));
                m_StopWatch.Reset();

                m_IsProcessing = false;
                m_IsRenderingDone = false;

                if (m_IsGenerationCancelled == false)
                {
                    UpdateRenderFeedbackWindow();
                    CreateFontTexture();
                }
                Repaint();
            }
        }


        /// <summary>
        /// Method which returns the character corresponding to a decimal value.
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        static int[] ParseNumberSequence(string sequence)
        {
            List<int> unicodeList = new List<int>();
            string[] sequences = sequence.Split(',');

            foreach (string seq in sequences)
            {
                string[] s1 = seq.Split('-');

                if (s1.Length == 1)
                    try
                    {
                        unicodeList.Add(int.Parse(s1[0]));
                    }
                    catch
                    {
                        Debug.Log("No characters selected or invalid format.");
                    }
                else
                {
                    for (int j = int.Parse(s1[0]); j < int.Parse(s1[1]) + 1; j++)
                    {
                        unicodeList.Add(j);
                    }
                }
            }

            return unicodeList.ToArray();
        }


        /// <summary>
        /// Method which returns the character (decimal value) from a hex sequence.
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        static int[] ParseHexNumberSequence(string sequence)
        {
            List<int> unicodeList = new List<int>();
            string[] sequences = sequence.Split(',');

            foreach (string seq in sequences)
            {
                string[] s1 = seq.Split('-');

                if (s1.Length == 1)
                    try
                    {
                        unicodeList.Add(int.Parse(s1[0], NumberStyles.AllowHexSpecifier));
                    }
                    catch
                    {
                        Debug.Log("No characters selected or invalid format.");
                    }
                else
                {
                    for (int j = int.Parse(s1[0], NumberStyles.AllowHexSpecifier); j < int.Parse(s1[1], NumberStyles.AllowHexSpecifier) + 1; j++)
                    {
                        unicodeList.Add(j);
                    }
                }
            }

            return unicodeList.ToArray();
        }

        void DrawControls()
        {
            GUILayout.Space(5f);

            if (position.width > position.height && position.width > k_TwoColumnControlsWidth)
            {
                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, GUILayout.Width(315));
            }
            else
            {
                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
            }
            
            GUILayout.Space(5f);

            GUILayout.Label(m_SelectedFontAsset != null ? string.Format("Creation Settings ({0})", m_SelectedFontAsset.name) : "Font Settings", EditorStyles.boldLabel);
            
            EditorGUIUtility.labelWidth = 125f;
            EditorGUIUtility.fieldWidth = 5f;
            
            EditorGUI.BeginDisabledGroup(m_IsProcessing);
            {
                // FONT TTF SELECTION
                EditorGUI.BeginChangeCheck();
                m_SourceFontFile = EditorGUILayout.ObjectField("Source Font File", m_SourceFontFile, typeof(Font), false) as Font;
                if (EditorGUI.EndChangeCheck())
                {
                    m_SelectedFontAsset = null;
                    m_IsFontAtlasInvalid = true;
                }

                // FONT SIZING
                EditorGUI.BeginChangeCheck();
                if (m_PointSizeSamplingMode == 0)
                {
                    m_PointSizeSamplingMode = EditorGUILayout.Popup("Sampling Point Size", m_PointSizeSamplingMode, m_FontSizingOptions);
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    m_PointSizeSamplingMode = EditorGUILayout.Popup("Sampling Point Size", m_PointSizeSamplingMode, m_FontSizingOptions, GUILayout.Width(225));
                    m_PointSize = EditorGUILayout.IntField(m_PointSize);
                    GUILayout.EndHorizontal();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    m_IsFontAtlasInvalid = true;
                }

                // FONT PADDING
                EditorGUI.BeginChangeCheck();
                m_Padding = EditorGUILayout.IntField("Padding", m_Padding);
                m_Padding = (int)Mathf.Clamp(m_Padding, 0f, 64f);
                if (EditorGUI.EndChangeCheck())
                {
                    m_IsFontAtlasInvalid = true;
                }

                // FONT PACKING METHOD SELECTION
                EditorGUI.BeginChangeCheck();
                m_PackingMode = (FontPackingModes)EditorGUILayout.EnumPopup("Packing Method", m_PackingMode);
                if (EditorGUI.EndChangeCheck())
                {
                    m_IsFontAtlasInvalid = true;
                }

                // FONT ATLAS RESOLUTION SELECTION
                GUILayout.BeginHorizontal();
                GUI.changed = false;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PrefixLabel("Atlas Resolution");
                m_AtlasWidth = EditorGUILayout.IntPopup(m_AtlasWidth, m_FontResolutionLabels, m_FontAtlasResolutions); //, GUILayout.Width(80));
                m_AtlasHeight = EditorGUILayout.IntPopup(m_AtlasHeight, m_FontResolutionLabels, m_FontAtlasResolutions); //, GUILayout.Width(80));
                if (EditorGUI.EndChangeCheck())
                {
                    m_IsFontAtlasInvalid = true;
                }

                GUILayout.EndHorizontal();


                // FONT CHARACTER SET SELECTION
                EditorGUI.BeginChangeCheck();
                bool hasSelectionChanged = false;
                m_CharacterSetSelectionMode = EditorGUILayout.Popup("Character Set", m_CharacterSetSelectionMode, m_FontCharacterSets);
                if (EditorGUI.EndChangeCheck())
                {
                    m_CharacterSequence = "";
                    hasSelectionChanged = true;
                    m_IsFontAtlasInvalid = true;

                    //Debug.Log("Resetting Sequence!");
                }

                switch (m_CharacterSetSelectionMode)
                {
                    case 0: // ASCII
                        //characterSequence = "32 - 126, 130, 132 - 135, 139, 145 - 151, 153, 155, 161, 166 - 167, 169 - 174, 176, 181 - 183, 186 - 187, 191, 8210 - 8226, 8230, 8240, 8242 - 8244, 8249 - 8250, 8252 - 8254, 8260, 8286";
                        m_CharacterSequence = "32 - 126, 160, 8203, 8230, 9633";
                        break;

                    case 1: // EXTENDED ASCII
                        m_CharacterSequence = "32 - 126, 160 - 255, 8192 - 8303, 8364, 8482, 9633";
                        // Could add 9632 for missing glyph
                        break;

                    case 2: // Lowercase
                        m_CharacterSequence = "32 - 64, 91 - 126, 160";
                        break;

                    case 3: // Uppercase
                        m_CharacterSequence = "32 - 96, 123 - 126, 160";
                        break;

                    case 4: // Numbers & Symbols
                        m_CharacterSequence = "32 - 64, 91 - 96, 123 - 126, 160";
                        break;

                    case 5: // Custom Range
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        GUILayout.Label("Enter a sequence of decimal values to define the characters to be included in the font asset or retrieve one from another font asset.", TMP_UIStyleManager.label);
                        GUILayout.Space(10f);

                        EditorGUI.BeginChangeCheck();
                        m_ReferencedFontAsset = EditorGUILayout.ObjectField("Select Font Asset", m_ReferencedFontAsset, typeof(TMP_FontAsset), false) as TMP_FontAsset;
                        if (EditorGUI.EndChangeCheck() || hasSelectionChanged)
                        {
                            if (m_ReferencedFontAsset != null)
                                m_CharacterSequence = TMP_EditorUtility.GetDecimalCharacterSequence(TMP_FontAsset.GetCharactersArray(m_ReferencedFontAsset));
                            m_IsFontAtlasInvalid = true;
                        }

                        // Filter out unwanted characters.
                        char chr = Event.current.character;
                        if ((chr < '0' || chr > '9') && (chr < ',' || chr > '-'))
                        {
                            Event.current.character = '\0';
                        }
                        GUILayout.Label("Character Sequence (Decimal)", EditorStyles.boldLabel);
                        EditorGUI.BeginChangeCheck();
                        m_CharacterSequence = EditorGUILayout.TextArea(m_CharacterSequence, TMP_UIStyleManager.textAreaBoxWindow, GUILayout.Height(120), GUILayout.ExpandWidth(true));
                        if (EditorGUI.EndChangeCheck())
                        {
                            m_IsFontAtlasInvalid = true;
                        }
                        
                        EditorGUILayout.EndVertical();
                        break;

                    case 6: // Unicode HEX Range
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        GUILayout.Label("Enter a sequence of Unicode (hex) values to define the characters to be included in the font asset or retrieve one from another font asset.", TMP_UIStyleManager.label);
                        GUILayout.Space(10f);

                        EditorGUI.BeginChangeCheck();
                        m_ReferencedFontAsset = EditorGUILayout.ObjectField("Select Font Asset", m_ReferencedFontAsset, typeof(TMP_FontAsset), false) as TMP_FontAsset;
                        if (EditorGUI.EndChangeCheck() || hasSelectionChanged)
                        {
                            if (m_ReferencedFontAsset != null)
                                m_CharacterSequence = TMP_EditorUtility.GetUnicodeCharacterSequence(TMP_FontAsset.GetCharactersArray(m_ReferencedFontAsset));
                            m_IsFontAtlasInvalid = true;
                        }

                        // Filter out unwanted characters.
                        chr = Event.current.character;
                        if ((chr < '0' || chr > '9') && (chr < 'a' || chr > 'f') && (chr < 'A' || chr > 'F') && (chr < ',' || chr > '-'))
                        {
                            Event.current.character = '\0';
                        }
                        GUILayout.Label("Character Sequence (Hex)", EditorStyles.boldLabel);
                        EditorGUI.BeginChangeCheck();
                        m_CharacterSequence = EditorGUILayout.TextArea(m_CharacterSequence, TMP_UIStyleManager.textAreaBoxWindow, GUILayout.Height(120), GUILayout.ExpandWidth(true));
                        if (EditorGUI.EndChangeCheck())
                        {
                            m_IsFontAtlasInvalid = true;
                        }

                        EditorGUILayout.EndVertical();
                        break;

                    case 7: // Characters from Font Asset
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        GUILayout.Label("Type the characters to be included in the font asset or retrieve them from another font asset.", TMP_UIStyleManager.label);
                        GUILayout.Space(10f);

                        EditorGUI.BeginChangeCheck();
                        m_ReferencedFontAsset = EditorGUILayout.ObjectField("Select Font Asset", m_ReferencedFontAsset, typeof(TMP_FontAsset), false) as TMP_FontAsset;
                        if (EditorGUI.EndChangeCheck() || hasSelectionChanged)
                        {
                            if (m_ReferencedFontAsset != null)
                                m_CharacterSequence = TMP_FontAsset.GetCharacters(m_ReferencedFontAsset);
                            m_IsFontAtlasInvalid = true;
                        }

                        EditorGUI.indentLevel = 0;
                        
                        GUILayout.Label("Custom Character List", EditorStyles.boldLabel);
                        EditorGUI.BeginChangeCheck();
                        m_CharacterSequence = EditorGUILayout.TextArea(m_CharacterSequence, TMP_UIStyleManager.textAreaBoxWindow, GUILayout.Height(120), GUILayout.ExpandWidth(true));
                        if (EditorGUI.EndChangeCheck())
                        {
                            m_IsFontAtlasInvalid = true;
                        }
                        EditorGUILayout.EndVertical();
                        break;

                    case 8: // Character List from File
                        EditorGUI.BeginChangeCheck();
                        m_CharacterList = EditorGUILayout.ObjectField("Character File", m_CharacterList, typeof(TextAsset), false) as TextAsset;
                        if (EditorGUI.EndChangeCheck())
                        {
                            m_IsFontAtlasInvalid = true;
                        }
                        if (m_CharacterList != null)
                        {
                            m_CharacterSequence = m_CharacterList.text;
                        }
                        break;
                }

                // FONT STYLE SELECTION
                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                m_FontStyle = (FaceStyles)EditorGUILayout.EnumPopup("Font Style", m_FontStyle, GUILayout.Width(225));
                m_FontStyleValue = EditorGUILayout.IntField((int)m_FontStyleValue);
                if (EditorGUI.EndChangeCheck())
                {
                    m_IsFontAtlasInvalid = true;
                }
                GUILayout.EndHorizontal();

                // Render Mode Selection
                EditorGUI.BeginChangeCheck();
                m_RenderMode = (RenderModes)EditorGUILayout.EnumPopup("Render Mode", m_RenderMode);
                if (EditorGUI.EndChangeCheck())
                {
                    //m_availableShaderNames = UpdateShaderList(font_renderMode, out m_availableShaders);
                    m_IsFontAtlasInvalid = true;
                }

                m_IncludeKerningPairs = EditorGUILayout.Toggle("Get Kerning Pairs", m_IncludeKerningPairs);

                EditorGUILayout.Space();
            }

            EditorGUI.EndDisabledGroup();

            if (!string.IsNullOrEmpty(m_WarningMessage))
            {
                EditorGUILayout.HelpBox(m_WarningMessage, MessageType.Warning);
            }
            
            GUI.enabled = m_SourceFontFile != null && !m_IsProcessing && !m_IsGenerationDisabled; // Enable Preview if we are not already rendering a font.
            if (GUILayout.Button("Generate Font Atlas") && m_CharacterSequence.Length != 0 && GUI.enabled)
            {
                if (!m_IsProcessing && m_SourceFontFile != null)
                {
                    DestroyImmediate(m_FontAtlas);
                    m_FontAtlas = null;
                    m_OutputFeedback = string.Empty;
                    m_SavedFontAtlas = null;
                    int errorCode;

                    errorCode = TMPro_FontPlugin.Initialize_FontEngine(); // Initialize Font Engine
                    if (errorCode != 0)
                    {
                        if (errorCode == 0xF0)
                        {
                            //Debug.Log("Font Library was already initialized!");
                            errorCode = 0;
                        }
                        else
                            Debug.Log("Error Code: " + errorCode + "  occurred while Initializing the FreeType Library.");
                    }
                    
                    string fontPath = AssetDatabase.GetAssetPath(m_SourceFontFile); // Get file path of TTF Font.

                    if (errorCode == 0)
                    {
                        errorCode = TMPro_FontPlugin.Load_TrueType_Font(fontPath); // Load the selected font.

                        if (errorCode != 0)
                        {
                            if (errorCode == 0xF1)
                            {
                                //Debug.Log("Font was already loaded!");
                                errorCode = 0;
                            }
                            else
                                Debug.Log("Error Code: " + errorCode + "  occurred while Loading the [" + m_SourceFontFile.name + "] font file. This typically results from the use of an incompatible or corrupted font file.");
                        }
                    }

                    if (errorCode == 0)
                    {
                        if (m_PointSizeSamplingMode == 0) m_PointSize = 72; // If Auto set size to 72 pts.

                        errorCode = TMPro_FontPlugin.FT_Size_Font(m_PointSize); // Load the selected font and size it accordingly.
                        if (errorCode != 0)
                            Debug.Log("Error Code: " + errorCode + "  occurred while Sizing the font.");
                    }

                    // Define an array containing the characters we will render.
                    if (errorCode == 0)
                    {
                        int[] characterSet;
                        if (m_CharacterSetSelectionMode == 7 || m_CharacterSetSelectionMode == 8)
                        {
                            List<int> charList = new List<int>();

                            for (int i = 0; i < m_CharacterSequence.Length; i++)
                            {
                                // Check to make sure we don't include duplicates
                                if (charList.FindIndex(item => item == m_CharacterSequence[i]) == -1)
                                    charList.Add(m_CharacterSequence[i]);
                                else
                                {
                                    //Debug.Log("Character [" + characterSequence[i] + "] is a duplicate.");
                                }
                            }

                            characterSet = charList.ToArray();
                        }
                        else if (m_CharacterSetSelectionMode == 6)
                        {
                            characterSet = ParseHexNumberSequence(m_CharacterSequence);
                        }
                        else
                        {
                            characterSet = ParseNumberSequence(m_CharacterSequence);
                        }

                        m_CharacterCount = characterSet.Length;
                        
                        m_TextureBuffer = new byte[m_AtlasWidth * m_AtlasHeight];

                        m_FontFaceInfo = new FT_FaceInfo();

                        m_FontGlyphInfo = new FT_GlyphInfo[m_CharacterCount];
                        
                        int padding = m_Padding;

                        bool autoSizing = m_PointSizeSamplingMode == 0;

                        float strokeSize = m_FontStyleValue;
                        if (m_RenderMode == RenderModes.DistanceField16) strokeSize = m_FontStyleValue * 16;
                        if (m_RenderMode == RenderModes.DistanceField32) strokeSize = m_FontStyleValue * 32;

                        m_IsProcessing = true;
                        m_IsGenerationCancelled = false;

                        // Start Stop Watch
                        m_StopWatch = System.Diagnostics.Stopwatch.StartNew();

                        ThreadPool.QueueUserWorkItem(someTask =>
                        {
                            m_IsRenderingDone = false;

                            errorCode = TMPro_FontPlugin.Render_Characters(m_TextureBuffer, m_AtlasWidth, m_AtlasHeight, padding, characterSet, m_CharacterCount, m_FontStyle, strokeSize, autoSizing, m_RenderMode, (int)m_PackingMode, ref m_FontFaceInfo, m_FontGlyphInfo);
                            m_IsRenderingDone = true;
                        });

                    }

                    SaveCreationSettingsToEditorPrefs(SaveFontCreationSettings());
                }
            }

            // FONT RENDERING PROGRESS BAR
            GUILayout.Space(1);

            Rect progressRect = EditorGUILayout.GetControlRect(false, 20);

            bool isEnabled = GUI.enabled;
            GUI.enabled = true;
            EditorGUI.ProgressBar(progressRect, m_IsProcessing ? m_RenderingProgress : 0, "Generation Progress");
            progressRect.x = progressRect.x + progressRect.width - 20;
            progressRect.y += 1;
            progressRect.width = 20;
            progressRect.height = 16;

            GUI.enabled = m_IsProcessing;
            if (GUI.Button(progressRect, "X"))
            {
                TMPro_FontPlugin.SendCancellationRequest(CancellationRequestType.CancelInProgess);
                m_RenderingProgress = 0;
                m_IsProcessing = false;
                m_IsGenerationCancelled = true;
            }
            GUI.enabled = isEnabled;
            
            // FONT STATUS & INFORMATION
            GUISkin skin = GUI.skin;

            //GUI.skin = TMP_UIStyleManager.TMP_GUISkin;
            GUI.enabled = true;
            
            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(145));
            m_OutputScrollPosition = EditorGUILayout.BeginScrollView(m_OutputScrollPosition);
            EditorGUILayout.LabelField(m_OutputFeedback, TMP_UIStyleManager.label);
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUI.skin = skin;
            
            // SAVE TEXTURE & CREATE and SAVE FONT XML FILE
            GUI.enabled = m_FontAtlas != null && !m_IsProcessing;    // Enable Save Button if font_Atlas is not Null.
            
            EditorGUILayout.BeginHorizontal();
                
            if (GUILayout.Button("Save") && GUI.enabled)
            {
                if (m_SelectedFontAsset == null)
                {
                    if (m_LegacyFontAsset != null)
                        SaveNewFontAssetWithSameName(m_LegacyFontAsset);
                    else
                        SaveNewFontAsset(m_SourceFontFile);
                }
                else
                {
                    // Save over exiting Font Asset
                    string filePath = Path.GetFullPath(AssetDatabase.GetAssetPath(m_SelectedFontAsset)).Replace('\\', '/');

                    if (m_RenderMode < RenderModes.DistanceField16) // ((int)m_RenderMode & 0x10) == 0x10)
                        Save_Normal_FontAsset(filePath);
                    else // ((RasterModes)m_RenderMode & RasterModes.Raster_Mode_SDF) == RasterModes.Raster_Mode_SDF || m_RenderMode == RenderModes.DistanceFieldAA)
                        Save_SDF_FontAsset(filePath);
                }
            }
            if (GUILayout.Button("Save as...") && GUI.enabled)
            {
                if (m_SelectedFontAsset == null)
                {
                    SaveNewFontAsset(m_SourceFontFile);
                }
                else
                {
                    SaveNewFontAssetWithSameName(m_SelectedFontAsset);
                }
            }
                
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            
            GUI.enabled = true; // Re-enable GUI

            GUILayout.Space(5);
            
            if (position.height > position.width || position.width < k_TwoColumnControlsWidth)
            {
                DrawPreview();
                GUILayout.Space(5);
            }
            
            EditorGUILayout.EndScrollView();

            if (m_IsFontAtlasInvalid)
                ClearGeneratedData();
        }


        /// <summary>
        /// Clear the previously generated data.
        /// </summary>
        void ClearGeneratedData()
        {
            m_IsFontAtlasInvalid = false;

            if (m_FontAtlas != null)
            {
                DestroyImmediate(m_FontAtlas);
                m_FontAtlas = null;
            }
        
            m_SavedFontAtlas = null;

            m_OutputFeedback = string.Empty;
            m_WarningMessage = string.Empty;
        }


        /// <summary>
        /// Function to update the feedback window showing the results of the latest generation.
        /// </summary>
        void UpdateRenderFeedbackWindow()
        {
            m_PointSize = m_FontFaceInfo.pointSize;

            string colorTag = m_FontFaceInfo.characterCount == m_CharacterCount ? "<color=#C0ffff>" : "<color=#ffff00>";
            string colorTag2 = "<color=#C0ffff>";

            var missingGlyphReport = k_OutputNameLabel + "<b>" + colorTag2 + m_FontFaceInfo.name + "</color></b>";

            if (missingGlyphReport.Length > 60)
                missingGlyphReport += "\n" + k_OutputSizeLabel + "<b>" + colorTag2 + m_FontFaceInfo.pointSize + "</color></b>";
            else
                missingGlyphReport += "  " + k_OutputSizeLabel + "<b>" + colorTag2 + m_FontFaceInfo.pointSize + "</color></b>";

            missingGlyphReport += "\n" + k_OutputCountLabel + "<b>" + colorTag + m_FontFaceInfo.characterCount + "/" + m_CharacterCount + "</color></b>";

            // Report missing requested glyph
            missingGlyphReport += "\n\n<color=#ffff00><b>Missing Characters</b></color>";
            missingGlyphReport += "\n----------------------------------------";
            
            m_OutputFeedback = missingGlyphReport;

            for (int i = 0; i < m_CharacterCount; i++)
            {
                if (m_FontGlyphInfo[i].x == -1)
                {
                    missingGlyphReport += "\nID: <color=#C0ffff>" + m_FontGlyphInfo[i].id + "\t</color>Hex: <color=#C0ffff>" + m_FontGlyphInfo[i].id.ToString("X") + "\t</color>Char [<color=#C0ffff>" + (char)m_FontGlyphInfo[i].id + "</color>]";

                    if (missingGlyphReport.Length < 16300)
                        m_OutputFeedback = missingGlyphReport;
                }
            }

            if (missingGlyphReport.Length > 16300)
                m_OutputFeedback += "\n\n<color=#ffff00>Report truncated.</color>\n<color=#c0ffff>See</color> \"TextMesh Pro\\Glyph Report.txt\"";

            // Save Missing Glyph Report file
            if (Directory.Exists("Assets/TextMesh Pro"))
            {
                missingGlyphReport = System.Text.RegularExpressions.Regex.Replace(missingGlyphReport, @"<[^>]*>", string.Empty);
                File.WriteAllText("Assets/TextMesh Pro/Glyph Report.txt", missingGlyphReport);
                AssetDatabase.Refresh();
            }
        }


        void CreateFontTexture()
        {
            m_FontAtlas = new Texture2D(m_AtlasWidth, m_AtlasHeight, TextureFormat.Alpha8, false, true);

            Color32[] colors = new Color32[m_AtlasWidth * m_AtlasHeight];

            for (int i = 0; i < (m_AtlasWidth * m_AtlasHeight); i++)
            {
                byte c = m_TextureBuffer[i];
                colors[i] = new Color32(c, c, c, c);
            }
            // Clear allocation of 
            m_TextureBuffer = null;
            
            if (m_RenderMode == RenderModes.Raster || m_RenderMode == RenderModes.RasterHinted)
                m_FontAtlas.filterMode = FilterMode.Point;

            m_FontAtlas.SetPixels32(colors, 0);
            m_FontAtlas.Apply(false, true);
        }


        /// <summary>
        /// Open Save Dialog to provide the option save the font asset using the name of the source font file. This also appends SDF to the name if using any of the SDF Font Asset creation modes.
        /// </summary>
        /// <param name="sourceObject"></param>
        void SaveNewFontAsset(Object sourceObject)
        {
            string filePath;
            
            // Save new Font Asset and open save file requester at Source Font File location.
            string saveDirectory = new FileInfo(AssetDatabase.GetAssetPath(sourceObject)).DirectoryName;

            if (m_RenderMode < RenderModes.DistanceField16) // ((int)m_RenderMode & 0x10) == 0x10)
            {
                filePath = EditorUtility.SaveFilePanel("Save TextMesh Pro! Font Asset File", saveDirectory, sourceObject.name, "asset");

                if (filePath.Length == 0)
                    return;

                Save_Normal_FontAsset(filePath);
            }
            else if (m_RenderMode >= RenderModes.DistanceField16) // ((RasterModes)m_RenderMode & RasterModes.Raster_Mode_SDF) == RasterModes.Raster_Mode_SDF || m_RenderMode == RenderModes.DistanceFieldAA)
            {
                filePath = EditorUtility.SaveFilePanel("Save TextMesh Pro! Font Asset File", saveDirectory, sourceObject.name + " SDF", "asset");

                if (filePath.Length == 0)
                    return;

                Save_SDF_FontAsset(filePath);
            }
        }


        /// <summary>
        /// Open Save Dialog to provide the option to save the font asset under the same name.
        /// </summary>
        /// <param name="sourceObject"></param>
        void SaveNewFontAssetWithSameName(Object sourceObject)
        {
            string filePath;

            // Save new Font Asset and open save file requester at Source Font File location.
            string saveDirectory = new FileInfo(AssetDatabase.GetAssetPath(sourceObject)).DirectoryName;

            filePath = EditorUtility.SaveFilePanel("Save TextMesh Pro! Font Asset File", saveDirectory, sourceObject.name, "asset");

            if (filePath.Length == 0)
                return;

            if (m_RenderMode < RenderModes.DistanceField16) // ((int)m_RenderMode & 0x10) == 0x10)
            {
                Save_Normal_FontAsset(filePath);
            }
            else if (m_RenderMode >= RenderModes.DistanceField16) // ((RasterModes)m_RenderMode & RasterModes.Raster_Mode_SDF) == RasterModes.Raster_Mode_SDF || m_RenderMode == RenderModes.DistanceFieldAA)
            {
                Save_SDF_FontAsset(filePath);
            }
        }


        void Save_Normal_FontAsset(string filePath)
        {
            filePath = filePath.Substring(0, filePath.Length - 6); // Trim file extension from filePath.

            string dataPath = Application.dataPath;

            if (filePath.IndexOf(dataPath, System.StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                Debug.LogError("You're saving the font asset in a directory outside of this project folder. This is not supported. Please select a directory under \"" + dataPath + "\"");
                return;
            }

            string relativeAssetPath = filePath.Substring(dataPath.Length - 6);
            string tex_DirName = Path.GetDirectoryName(relativeAssetPath);
            string tex_FileName = Path.GetFileNameWithoutExtension(relativeAssetPath);
            string tex_Path_NoExt = tex_DirName + "/" + tex_FileName;

            // Check if TextMeshPro font asset already exists. If not, create a new one. Otherwise update the existing one.
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath(tex_Path_NoExt + ".asset", typeof(TMP_FontAsset)) as TMP_FontAsset;
            if (fontAsset == null)
            {
                //Debug.Log("Creating TextMeshPro font asset!");
                fontAsset = ScriptableObject.CreateInstance<TMP_FontAsset>(); // Create new TextMeshPro Font Asset.
                AssetDatabase.CreateAsset(fontAsset, tex_Path_NoExt + ".asset");

                //Set Font Asset Type
                fontAsset.fontAssetType = TMP_FontAsset.FontAssetTypes.Bitmap;

                // Reference to the source font file
                //font_asset.sourceFontFile = font_TTF as Font;

                // Add FaceInfo to Font Asset
                FaceInfo face = GetFaceInfo(m_FontFaceInfo, 1);
                fontAsset.AddFaceInfo(face);

                // Add GlyphInfo[] to Font Asset
                TMP_Glyph[] glyphs = GetGlyphInfo(m_FontGlyphInfo, 1);
                fontAsset.AddGlyphInfo(glyphs);

                // Get and Add Kerning Pairs to Font Asset
                if (m_IncludeKerningPairs)
                {
                    string fontFilePath = AssetDatabase.GetAssetPath(m_SourceFontFile);
                    KerningTable kerningTable = GetKerningTable(fontFilePath, (int)face.PointSize);
                    fontAsset.AddKerningInfo(kerningTable);
                }


                // Add Font Atlas as Sub-Asset
                fontAsset.atlas = m_FontAtlas;
                m_FontAtlas.name = tex_FileName + " Atlas";

                AssetDatabase.AddObjectToAsset(m_FontAtlas, fontAsset);

                // Create new Material and Add it as Sub-Asset
                Shader default_Shader = Shader.Find("TextMeshPro/Bitmap"); // m_shaderSelection;
                Material tmp_material = new Material(default_Shader);
                tmp_material.name = tex_FileName + " Material";
                tmp_material.SetTexture(ShaderUtilities.ID_MainTex, m_FontAtlas);
                fontAsset.material = tmp_material;

                AssetDatabase.AddObjectToAsset(tmp_material, fontAsset);

            }
            else
            {
                // Find all Materials referencing this font atlas.
                Material[] material_references = TMP_EditorUtility.FindMaterialReferences(fontAsset);

                // Destroy Assets that will be replaced.
                DestroyImmediate(fontAsset.atlas, true);

                //Set Font Asset Type
                fontAsset.fontAssetType = TMP_FontAsset.FontAssetTypes.Bitmap;

                // Add FaceInfo to Font Asset
                FaceInfo face = GetFaceInfo(m_FontFaceInfo, 1);
                fontAsset.AddFaceInfo(face);

                // Add GlyphInfo[] to Font Asset
                TMP_Glyph[] glyphs = GetGlyphInfo(m_FontGlyphInfo, 1);
                fontAsset.AddGlyphInfo(glyphs);

                // Get and Add Kerning Pairs to Font Asset
                if (m_IncludeKerningPairs)
                {
                    string fontFilePath = AssetDatabase.GetAssetPath(m_SourceFontFile);
                    KerningTable kerningTable = GetKerningTable(fontFilePath, (int)face.PointSize);
                    fontAsset.AddKerningInfo(kerningTable);
                }

                // Add Font Atlas as Sub-Asset
                fontAsset.atlas = m_FontAtlas;
                m_FontAtlas.name = tex_FileName + " Atlas";

                // Special handling due to a bug in earlier versions of Unity.
                m_FontAtlas.hideFlags = HideFlags.None;
                fontAsset.material.hideFlags = HideFlags.None;

                AssetDatabase.AddObjectToAsset(m_FontAtlas, fontAsset);

                // Assign new font atlas texture to the existing material.
                fontAsset.material.SetTexture(ShaderUtilities.ID_MainTex, fontAsset.atlas);

                // Update the Texture reference on the Material
                for (int i = 0; i < material_references.Length; i++)
                {
                    material_references[i].SetTexture(ShaderUtilities.ID_MainTex, m_FontAtlas);
                }
            }

            // Save Font Asset creation settings
            m_SelectedFontAsset = fontAsset;
            m_LegacyFontAsset = null;
            fontAsset.creationSettings = SaveFontCreationSettings();

            AssetDatabase.SaveAssets();

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(fontAsset));  // Re-import font asset to get the new updated version.

            //EditorUtility.SetDirty(font_asset);
            fontAsset.ReadFontDefinition();

            AssetDatabase.Refresh();

            m_FontAtlas = null;

            // NEED TO GENERATE AN EVENT TO FORCE A REDRAW OF ANY TEXTMESHPRO INSTANCES THAT MIGHT BE USING THIS FONT ASSET
            TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, fontAsset);
        }


        void Save_SDF_FontAsset(string filePath)
        {
            filePath = filePath.Substring(0, filePath.Length - 6); // Trim file extension from filePath.

            string dataPath = Application.dataPath;

            if (filePath.IndexOf(dataPath, System.StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                Debug.LogError("You're saving the font asset in a directory outside of this project folder. This is not supported. Please select a directory under \"" + dataPath + "\"");
                return;
            }

            string relativeAssetPath = filePath.Substring(dataPath.Length - 6);
            string tex_DirName = Path.GetDirectoryName(relativeAssetPath);
            string tex_FileName = Path.GetFileNameWithoutExtension(relativeAssetPath);
            string tex_Path_NoExt = tex_DirName + "/" + tex_FileName;


            // Check if TextMeshPro font asset already exists. If not, create a new one. Otherwise update the existing one.
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(tex_Path_NoExt + ".asset");
            if (fontAsset == null)
            {
                //Debug.Log("Creating TextMeshPro font asset!");
                fontAsset = ScriptableObject.CreateInstance<TMP_FontAsset>(); // Create new TextMeshPro Font Asset.
                AssetDatabase.CreateAsset(fontAsset, tex_Path_NoExt + ".asset");

                // Reference to the source font file
                //font_asset.sourceFontFile = font_TTF as Font;

                //Set Font Asset Type
                fontAsset.fontAssetType = TMP_FontAsset.FontAssetTypes.SDF;

                //if (m_destination_Atlas != null)
                //    m_font_Atlas = m_destination_Atlas;

                // If using the C# SDF creation mode, we need the scale down factor.
                int scaleDownFactor = 1; // ((RasterModes)m_RenderMode & RasterModes.Raster_Mode_SDF) == RasterModes.Raster_Mode_SDF || m_RenderMode == RenderModes.DistanceFieldAA ? 1 : font_scaledownFactor;

                // Add FaceInfo to Font Asset
                FaceInfo face = GetFaceInfo(m_FontFaceInfo, scaleDownFactor);
                fontAsset.AddFaceInfo(face);

                // Add GlyphInfo[] to Font Asset
                TMP_Glyph[] glyphs = GetGlyphInfo(m_FontGlyphInfo, scaleDownFactor);
                fontAsset.AddGlyphInfo(glyphs);

                // Get and Add Kerning Pairs to Font Asset
                if (m_IncludeKerningPairs)
                {
                    string fontFilePath = AssetDatabase.GetAssetPath(m_SourceFontFile);
                    KerningTable kerningTable = GetKerningTable(fontFilePath, (int)face.PointSize);
                    fontAsset.AddKerningInfo(kerningTable);
                }

                // Add Line Breaking Rules
                //LineBreakingTable lineBreakingTable = new LineBreakingTable();
                //

                // Add Font Atlas as Sub-Asset
                fontAsset.atlas = m_FontAtlas;
                m_FontAtlas.name = tex_FileName + " Atlas";

                AssetDatabase.AddObjectToAsset(m_FontAtlas, fontAsset);

                // Create new Material and Add it as Sub-Asset
                Shader default_Shader = Shader.Find("TextMeshPro/Distance Field"); //m_shaderSelection;
                Material tmp_material = new Material(default_Shader);

                tmp_material.name = tex_FileName + " Material";
                tmp_material.SetTexture(ShaderUtilities.ID_MainTex, m_FontAtlas);
                tmp_material.SetFloat(ShaderUtilities.ID_TextureWidth, m_FontAtlas.width);
                tmp_material.SetFloat(ShaderUtilities.ID_TextureHeight, m_FontAtlas.height);

                int spread = m_Padding + 1;
                tmp_material.SetFloat(ShaderUtilities.ID_GradientScale, spread); // Spread = Padding for Brute Force SDF.

                tmp_material.SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.normalStyle);
                tmp_material.SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);

                fontAsset.material = tmp_material;

                AssetDatabase.AddObjectToAsset(tmp_material, fontAsset);

            }
            else
            {
                // Find all Materials referencing this font atlas.
                Material[] material_references = TMP_EditorUtility.FindMaterialReferences(fontAsset);

                // Destroy Assets that will be replaced.
                DestroyImmediate(fontAsset.atlas, true);

                //Set Font Asset Type
                fontAsset.fontAssetType = TMP_FontAsset.FontAssetTypes.SDF;

                int scaleDownFactor = 1; // ((RasterModes)m_RenderMode & RasterModes.Raster_Mode_SDF) == RasterModes.Raster_Mode_SDF || m_RenderMode == RenderModes.DistanceFieldAA ? 1 : font_scaledownFactor;
                // Add FaceInfo to Font Asset  
                FaceInfo face = GetFaceInfo(m_FontFaceInfo, scaleDownFactor);
                fontAsset.AddFaceInfo(face);

                // Add GlyphInfo[] to Font Asset
                TMP_Glyph[] glyphs = GetGlyphInfo(m_FontGlyphInfo, scaleDownFactor);
                fontAsset.AddGlyphInfo(glyphs);

                // Get and Add Kerning Pairs to Font Asset
                if (m_IncludeKerningPairs)
                {
                    string fontFilePath = AssetDatabase.GetAssetPath(m_SourceFontFile);
                    KerningTable kerningTable = GetKerningTable(fontFilePath, (int)face.PointSize);
                    fontAsset.AddKerningInfo(kerningTable);
                }

                // Add Font Atlas as Sub-Asset
                fontAsset.atlas = m_FontAtlas;
                m_FontAtlas.name = tex_FileName + " Atlas";

                // Special handling due to a bug in earlier versions of Unity.
                m_FontAtlas.hideFlags = HideFlags.None;
                fontAsset.material.hideFlags = HideFlags.None;

                AssetDatabase.AddObjectToAsset(m_FontAtlas, fontAsset);

                // Assign new font atlas texture to the existing material.
                fontAsset.material.SetTexture(ShaderUtilities.ID_MainTex, fontAsset.atlas);

                // Update the Texture reference on the Material
                for (int i = 0; i < material_references.Length; i++)
                {
                    material_references[i].SetTexture(ShaderUtilities.ID_MainTex, m_FontAtlas);
                    material_references[i].SetFloat(ShaderUtilities.ID_TextureWidth, m_FontAtlas.width);
                    material_references[i].SetFloat(ShaderUtilities.ID_TextureHeight, m_FontAtlas.height);

                    int spread = m_Padding + 1;
                    material_references[i].SetFloat(ShaderUtilities.ID_GradientScale, spread); // Spread = Padding for Brute Force SDF.

                    material_references[i].SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.normalStyle);
                    material_references[i].SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);
                }
            }

            // Saving File for Debug
            //var pngData = destination_Atlas.EncodeToPNG();
            //File.WriteAllBytes("Assets/Textures/Debug Distance Field.png", pngData);

            // Save Font Asset creation settings
            m_SelectedFontAsset = fontAsset;
            m_LegacyFontAsset = null;
            fontAsset.creationSettings = SaveFontCreationSettings();

            AssetDatabase.SaveAssets();

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(fontAsset));  // Re-import font asset to get the new updated version.

            fontAsset.ReadFontDefinition();

            AssetDatabase.Refresh();

            m_FontAtlas = null;

            // NEED TO GENERATE AN EVENT TO FORCE A REDRAW OF ANY TEXTMESHPRO INSTANCES THAT MIGHT BE USING THIS FONT ASSET
            TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, fontAsset);
        }


        /// <summary>
        /// Internal method to save the Font Asset Creation Settings
        /// </summary>
        /// <returns></returns>
        FontAssetCreationSettings SaveFontCreationSettings()
        {
            FontAssetCreationSettings settings = new FontAssetCreationSettings();

            //settings.sourceFontFileName = m_SourceFontFile.name;
            settings.sourceFontFileGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_SourceFontFile));
            settings.pointSizeSamplingMode = m_PointSizeSamplingMode;
            settings.pointSize = m_PointSize;
            settings.padding = m_Padding;
            settings.packingMode = (int)m_PackingMode;
            settings.atlasWidth = m_AtlasWidth;
            settings.atlasHeight = m_AtlasHeight;
            settings.characterSetSelectionMode = m_CharacterSetSelectionMode;
            settings.characterSequence = m_CharacterSequence;
            settings.referencedFontAssetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_ReferencedFontAsset));
            settings.referencedTextAssetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_CharacterList));
            settings.fontStyle = (int)m_FontStyle;
            settings.fontStyleModifier = m_FontStyleValue;
            settings.renderMode = (int)m_RenderMode;
            settings.includeFontFeatures = m_IncludeKerningPairs;

            return settings;
        }

        /// <summary>
        /// Internal method to load the Font Asset Creation Settings
        /// </summary>
        /// <param name="settings"></param>
        void LoadFontCreationSettings(FontAssetCreationSettings settings)
        {
            m_SourceFontFile = AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(settings.sourceFontFileGUID));
            m_PointSizeSamplingMode  = settings.pointSizeSamplingMode;
            m_PointSize = settings.pointSize;
            m_Padding = settings.padding;
            m_PackingMode = (FontPackingModes)settings.packingMode;
            m_AtlasWidth = settings.atlasWidth;
            m_AtlasHeight = settings.atlasHeight;
            m_CharacterSetSelectionMode = settings.characterSetSelectionMode;
            m_CharacterSequence = settings.characterSequence;
            m_ReferencedFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(settings.referencedFontAssetGUID));
            m_CharacterList = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(settings.referencedTextAssetGUID));
            m_FontStyle = (FaceStyles)settings.fontStyle;
            m_FontStyleValue = settings.fontStyleModifier;
            m_RenderMode = (RenderModes)settings.renderMode;
            m_IncludeKerningPairs = settings.includeFontFeatures;
        }


        /// <summary>
        /// Save the latest font asset creation settings to EditorPrefs.
        /// </summary>
        /// <param name="settings"></param>
        void SaveCreationSettingsToEditorPrefs(FontAssetCreationSettings settings)
        {
            // Create new list if one does not already exist
            if (m_FontAssetCreationSettingsContainer == null)
            {
                m_FontAssetCreationSettingsContainer = new FontAssetCreationSettingsContainer();
                m_FontAssetCreationSettingsContainer.fontAssetCreationSettings = new List<FontAssetCreationSettings>();
            }

            // Add new creation settings to the list
            m_FontAssetCreationSettingsContainer.fontAssetCreationSettings.Add(settings);

            // Since list should only contain the most 4 recent settings, we remove the first element if list exceeds 4 elements.
            if (m_FontAssetCreationSettingsContainer.fontAssetCreationSettings.Count > 4)
                m_FontAssetCreationSettingsContainer.fontAssetCreationSettings.RemoveAt(0);

            m_FontAssetCreationSettingsCurrentIndex = m_FontAssetCreationSettingsContainer.fontAssetCreationSettings.Count - 1;

            // Serialize list to JSON
            string serializedSettings = JsonUtility.ToJson(m_FontAssetCreationSettingsContainer, true);

            EditorPrefs.SetString(k_FontAssetCreationSettingsContainerKey, serializedSettings);
        }

        void DrawPreview()
        {
            Rect pixelRect;
            if (position.width > position.height && position.width > k_TwoColumnControlsWidth)
            {
                float minSide = Mathf.Min(position.height - 15f, position.width - k_TwoColumnControlsWidth);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(minSide));

                pixelRect = GUILayoutUtility.GetRect(minSide, minSide, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                pixelRect = GUILayoutUtility.GetAspectRect(1f);
            }
            
            if (m_FontAtlas != null)
            {
                EditorGUI.DrawTextureAlpha(pixelRect, m_FontAtlas, ScaleMode.StretchToFill);
            }
            else if (m_SavedFontAtlas != null)
            {
                EditorGUI.DrawTextureAlpha(pixelRect, m_SavedFontAtlas, ScaleMode.StretchToFill);
            }

            EditorGUILayout.EndVertical();
        }


        // Convert from FT_FaceInfo to FaceInfo
        static FaceInfo GetFaceInfo(FT_FaceInfo ftFace, int scaleFactor)
        {
            FaceInfo face = new FaceInfo();

            face.Name = ftFace.name;
            face.PointSize = (float)ftFace.pointSize / scaleFactor;
            face.Padding = ftFace.padding / scaleFactor;
            face.LineHeight = ftFace.lineHeight / scaleFactor;
            face.CapHeight = 0;
            face.Baseline = 0;
            face.Ascender = ftFace.ascender / scaleFactor;
            face.Descender = ftFace.descender / scaleFactor;
            face.CenterLine = ftFace.centerLine / scaleFactor;
            face.Underline = ftFace.underline / scaleFactor;
            face.UnderlineThickness = ftFace.underlineThickness == 0 ? 5 : ftFace.underlineThickness / scaleFactor; // Set Thickness to 5 if TTF value is Zero.
            face.strikethrough = (face.Ascender + face.Descender) / 2.75f;
            face.strikethroughThickness = face.UnderlineThickness;
            face.SuperscriptOffset = face.Ascender;
            face.SubscriptOffset = face.Underline;
            face.SubSize = 0.5f;
            //face.CharacterCount = ft_face.characterCount;
            face.AtlasWidth = ftFace.atlasWidth / scaleFactor;
            face.AtlasHeight = ftFace.atlasHeight / scaleFactor;

            return face;
        }


        // Convert from FT_GlyphInfo[] to GlyphInfo[]
        TMP_Glyph[] GetGlyphInfo(FT_GlyphInfo[] ftGlyphs, int scaleFactor)
        {
            List<TMP_Glyph> glyphs = new List<TMP_Glyph>();
            List<int> kerningSet = new List<int>();

            for (int i = 0; i < ftGlyphs.Length; i++)
            {
                TMP_Glyph g = new TMP_Glyph();

                g.id = ftGlyphs[i].id;
                g.x = ftGlyphs[i].x / scaleFactor;
                g.y = ftGlyphs[i].y / scaleFactor;
                g.width = ftGlyphs[i].width / scaleFactor;
                g.height = ftGlyphs[i].height / scaleFactor;
                g.xOffset = ftGlyphs[i].xOffset / scaleFactor;
                g.yOffset = ftGlyphs[i].yOffset / scaleFactor;
                g.xAdvance = ftGlyphs[i].xAdvance / scaleFactor;

                // Filter out characters with missing glyphs.
                if (g.x == -1)
                    continue;

                glyphs.Add(g);
                kerningSet.Add(g.id);
            }

            m_KerningSet = kerningSet.ToArray();

            return glyphs.ToArray();
        }


        // Get Kerning Pairs
        public KerningTable GetKerningTable(string fontFilePath, int pointSize)
        {
            KerningTable kerningInfo = new KerningTable();
            kerningInfo.kerningPairs = new List<KerningPair>();

            // Temporary Array to hold the kerning pairs from the Native Plug-in.
            FT_KerningPair[] kerningPairs = new FT_KerningPair[7500];

            int kpCount = TMPro_FontPlugin.FT_GetKerningPairs(fontFilePath, m_KerningSet, m_KerningSet.Length, kerningPairs);

            for (int i = 0; i < kpCount; i++)
            {
                // Proceed to add each kerning pairs.
                KerningPair kp = new KerningPair((uint)kerningPairs[i].ascII_Left, (uint)kerningPairs[i].ascII_Right, kerningPairs[i].xAdvanceOffset * pointSize);

                // Filter kerning pairs to avoid duplicates
                int index = kerningInfo.kerningPairs.FindIndex(item => item.firstGlyph == kp.firstGlyph && item.secondGlyph == kp.secondGlyph);

                if (index == -1)
                    kerningInfo.kerningPairs.Add(kp);
                else
                    if (!TMP_Settings.warningsDisabled) Debug.LogWarning("Kerning Key for [" + kp.firstGlyph + "] and [" + kp.secondGlyph + "] is a duplicate.");

            }

            return kerningInfo;
        }
    }
}