//#define TMP_DEBUG_MODE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;


namespace TMPro
{
    /// <summary>
    /// Editable text input field.
    /// </summary>
    [AddComponentMenu("UI/TextMeshPro - Input Field", 11)]
    public class TMP_InputField : Selectable,
        IUpdateSelectedHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IPointerClickHandler,
        ISubmitHandler,
        ICanvasElement,
        IScrollHandler
    {

        // Setting the content type acts as a shortcut for setting a combination of InputType, CharacterValidation, LineType, and TouchScreenKeyboardType
        public enum ContentType
        {
            Standard,
            Autocorrected,
            IntegerNumber,
            DecimalNumber,
            Alphanumeric,
            Name,
            EmailAddress,
            Password,
            Pin,
            Custom
        }

        public enum InputType
        {
            Standard,
            AutoCorrect,
            Password,
        }

        public enum CharacterValidation
        {
            None,
            Digit,
            Integer,
            Decimal,
            Alphanumeric,
            Name,
            Regex,
            EmailAddress,
            CustomValidator
        }

        public enum LineType
        {
            SingleLine,
            MultiLineSubmit,
            MultiLineNewline
        }

        public delegate char OnValidateInput(string text, int charIndex, char addedChar);

        [Serializable]
        public class SubmitEvent : UnityEvent<string> { }

        [Serializable]
        public class OnChangeEvent : UnityEvent<string> { }

        [Serializable]
        public class SelectionEvent : UnityEvent<string> { }

        [Serializable]
        public class TextSelectionEvent : UnityEvent<string, int, int> { }

        protected TouchScreenKeyboard m_Keyboard;
        static private readonly char[] kSeparators = { ' ', '.', ',', '\t', '\r', '\n' };

        #region Exposed properties
        /// <summary>
        /// Text Text used to display the input's value.
        /// </summary>

        [SerializeField]
        protected RectTransform m_TextViewport;

        //Vector3[] m_ViewportCorners = new Vector3[4];

        [SerializeField]
        protected TMP_Text m_TextComponent;

        protected RectTransform m_TextComponentRectTransform;

        [SerializeField]
        protected Graphic m_Placeholder;

        [SerializeField]
        protected Scrollbar m_VerticalScrollbar;

        [SerializeField]
        protected TMP_ScrollbarEventHandler m_VerticalScrollbarEventHandler;
        //private bool m_ForceDeactivation;

        /// <summary>
        /// Used to keep track of scroll position
        /// </summary>
        private float m_ScrollPosition;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        protected float m_ScrollSensitivity = 1.0f;

        //[SerializeField]
        //protected TMP_Text m_PlaceholderTextComponent;

        [SerializeField]
        private ContentType m_ContentType = ContentType.Standard;

        /// <summary>
        /// Type of data expected by the input field.
        /// </summary>
        [SerializeField]
        private InputType m_InputType = InputType.Standard;

        /// <summary>
        /// The character used to hide text in password field.
        /// </summary>
        [SerializeField]
        private char m_AsteriskChar = '*';

        /// <summary>
        /// Keyboard type applies to mobile keyboards that get shown.
        /// </summary>
        [SerializeField]
        private TouchScreenKeyboardType m_KeyboardType = TouchScreenKeyboardType.Default;

        [SerializeField]
        private LineType m_LineType = LineType.SingleLine;

        /// <summary>
        /// Should hide mobile input.
        /// </summary>
        [SerializeField]
        private bool m_HideMobileInput = false;

        /// <summary>
        /// What kind of validation to use with the input field's data.
        /// </summary>
        [SerializeField]
        private CharacterValidation m_CharacterValidation = CharacterValidation.None;

        /// <summary>
        /// The Regex expression used for validating the text input.
        /// </summary>
        [SerializeField]
        private string m_RegexValue = string.Empty;

        /// <summary>
        /// The point sized used by the placeholder and input text object.
        /// </summary>
        [SerializeField]
        private float m_GlobalPointSize = 14;

        /// <summary>
        /// Maximum number of characters allowed before input no longer works.
        /// </summary>
        [SerializeField]
        private int m_CharacterLimit = 0;

        /// <summary>
        /// Event delegates triggered when the input field submits its data.
        /// </summary>
        [SerializeField]
        private SubmitEvent m_OnEndEdit = new SubmitEvent();

        /// <summary>
        /// Event delegates triggered when the input field submits its data.
        /// </summary>
        [SerializeField]
        private SubmitEvent m_OnSubmit = new SubmitEvent();

        /// <summary>
        /// Event delegates triggered when the input field is focused.
        /// </summary>
        [SerializeField]
        private SelectionEvent m_OnSelect = new SelectionEvent();

        /// <summary>
        /// Event delegates triggered when the input field focus is lost.
        /// </summary>
        [SerializeField]
        private SelectionEvent m_OnDeselect = new SelectionEvent();

        /// <summary>
        /// Event delegates triggered when the text is selected / highlighted.
        /// </summary>
        [SerializeField]
        private TextSelectionEvent m_OnTextSelection = new TextSelectionEvent();

        /// <summary>
        /// Event delegates triggered when text is no longer select / highlighted.
        /// </summary>
        [SerializeField]
        private TextSelectionEvent m_OnEndTextSelection = new TextSelectionEvent();

        /// <summary>
        /// Event delegates triggered when the input field changes its data.
        /// </summary>
        [SerializeField]
        private OnChangeEvent m_OnValueChanged = new OnChangeEvent();

        /// <summary>
        /// Custom validation callback.
        /// </summary>
        [SerializeField]
        private OnValidateInput m_OnValidateInput;

        [SerializeField]
        private Color m_CaretColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

        [SerializeField]
        private bool m_CustomCaretColor = false;

        [SerializeField]
        private Color m_SelectionColor = new Color(168f / 255f, 206f / 255f, 255f / 255f, 192f / 255f);

        /// <summary>
        /// Input field's value.
        /// </summary>

        [SerializeField]
        [TextArea(3, 10)]
        protected string m_Text = string.Empty;

        [SerializeField]
        [Range(0f, 4f)]
        private float m_CaretBlinkRate = 0.85f;

        [SerializeField]
        [Range(1, 5)]
        private int m_CaretWidth = 1;

        [SerializeField]
        private bool m_ReadOnly = false;

        [SerializeField]
        private bool m_RichText = true;

        #endregion

        protected int m_StringPosition = 0;
        protected int m_StringSelectPosition = 0;
        protected int m_CaretPosition = 0;
        protected int m_CaretSelectPosition = 0;

        private RectTransform caretRectTrans = null;
        protected UIVertex[] m_CursorVerts = null;
        private CanvasRenderer m_CachedInputRenderer;
        private Vector2 m_DefaultTransformPosition;
        private Vector2 m_LastPosition;

        [NonSerialized]
        protected Mesh m_Mesh;
        private bool m_AllowInput = false;
        //bool m_HasLostFocus = false;
        private bool m_ShouldActivateNextUpdate = false;
        private bool m_UpdateDrag = false;
        private bool m_DragPositionOutOfBounds = false;
        private const float kHScrollSpeed = 0.05f;
        private const float kVScrollSpeed = 0.10f;
        protected bool m_CaretVisible;
        private Coroutine m_BlinkCoroutine = null;
        private float m_BlinkStartTime = 0.0f;
        private Coroutine m_DragCoroutine = null;
        private string m_OriginalText = "";
        private bool m_WasCanceled = false;
        private bool m_HasDoneFocusTransition = false;

        private bool m_IsScrollbarUpdateRequired = false;
        private bool m_IsUpdatingScrollbarValues = false;

        private bool m_isLastKeyBackspace = false;
        private float m_ClickStartTime;
        private float m_DoubleClickDelay = 0.5f;

        // Doesn't include dot and @ on purpose! See usage for details.
        const string kEmailSpecialCharacters = "!#$%&'*+-/=?^_`{|}~";


        protected TMP_InputField()
        { }

        protected Mesh mesh
        {
            get
            {
                if (m_Mesh == null)
                    m_Mesh = new Mesh();
                return m_Mesh;
            }
        }

        /// <summary>
        /// Should the mobile keyboard input be hidden.
        /// </summary>
        public bool shouldHideMobileInput
        {
            set
            {
                SetPropertyUtility.SetStruct(ref m_HideMobileInput, value);
            }
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.tvOS:
                        return m_HideMobileInput;
                }

                return true;
            }
        }


        /// <summary>
        /// Input field's current text value.
        /// </summary>

        public string text
        {
            get
            {
                return m_Text;
            }
            set
            {
                if (this.text == value)
                    return;

                if (value == null) value = string.Empty;

                m_Text = value;

                //if (m_LineType == LineType.SingleLine)
                //    m_Text = m_Text.Replace("\n", "").Replace("\t", "");

                //// If we have an input validator, validate the input and apply the character limit at the same time.
                //if (onValidateInput != null || characterValidation != CharacterValidation.None)
                //{
                //    m_Text = "";
                //    OnValidateInput validatorMethod = onValidateInput ?? Validate;
                //    m_CaretPosition = m_CaretSelectPosition = value.Length;
                //    int charactersToCheck = characterLimit > 0 ? Math.Min(characterLimit - 1, value.Length) : value.Length;
                //    for (int i = 0; i < charactersToCheck; ++i)
                //    {
                //        char c = validatorMethod(m_Text, m_Text.Length, value[i]);
                //        if (c != 0)
                //            m_Text += c;
                //    }
                //}
                //else
                //{
                //    m_Text = characterLimit > 0 && value.Length > characterLimit ? value.Substring(0, characterLimit) : value;
                //}

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    SendOnValueChangedAndUpdateLabel();
                    return;
                }
#endif

                if (m_Keyboard != null)
                    m_Keyboard.text = m_Text;

                if (m_StringPosition > m_Text.Length)
                    m_StringPosition = m_StringSelectPosition = m_Text.Length;

                // Set RectTransform relative position to top of viewport.
                AdjustTextPositionRelativeToViewport(0);

                m_forceRectTransformAdjustment = true;

                SendOnValueChangedAndUpdateLabel();
            }
        }

        public bool isFocused
        {
            get { return m_AllowInput; }
        }

        public float caretBlinkRate
        {
            get { return m_CaretBlinkRate; }
            set
            {
                if (SetPropertyUtility.SetStruct(ref m_CaretBlinkRate, value))
                {
                    if (m_AllowInput)
                        SetCaretActive();
                }
            }
        }

        public int caretWidth { get { return m_CaretWidth; } set { if (SetPropertyUtility.SetStruct(ref m_CaretWidth, value)) MarkGeometryAsDirty(); } }

        public RectTransform textViewport { get { return m_TextViewport; } set { SetPropertyUtility.SetClass(ref m_TextViewport, value); } }

        public TMP_Text textComponent { get { return m_TextComponent; } set { SetPropertyUtility.SetClass(ref m_TextComponent, value); } }

        //public TMP_Text placeholderTextComponent { get { return m_PlaceholderTextComponent; } set { SetPropertyUtility.SetClass(ref m_PlaceholderTextComponent, value); } }

        public Graphic placeholder { get { return m_Placeholder; } set { SetPropertyUtility.SetClass(ref m_Placeholder, value); } }

        public Scrollbar verticalScrollbar
        {
            get { return m_VerticalScrollbar; }
            set
            {
                if (m_VerticalScrollbar != null)
                    m_VerticalScrollbar.onValueChanged.RemoveListener(OnScrollbarValueChange);

                SetPropertyUtility.SetClass(ref m_VerticalScrollbar, value);

                if (m_VerticalScrollbar)
                {
                    m_VerticalScrollbar.onValueChanged.AddListener(OnScrollbarValueChange);
                    
                }
            }
        }

        public float scrollSensitivity { get { return m_ScrollSensitivity; } set { if (SetPropertyUtility.SetStruct(ref m_ScrollSensitivity, value)) MarkGeometryAsDirty(); } }

        public Color caretColor { get { return customCaretColor ? m_CaretColor : textComponent.color; } set { if (SetPropertyUtility.SetColor(ref m_CaretColor, value)) MarkGeometryAsDirty(); } }

        public bool customCaretColor { get { return m_CustomCaretColor; } set { if (m_CustomCaretColor != value) { m_CustomCaretColor = value; MarkGeometryAsDirty(); } } }

        public Color selectionColor { get { return m_SelectionColor; } set { if (SetPropertyUtility.SetColor(ref m_SelectionColor, value)) MarkGeometryAsDirty(); } }

        public SubmitEvent onEndEdit { get { return m_OnEndEdit; } set { SetPropertyUtility.SetClass(ref m_OnEndEdit, value); } }

        public SubmitEvent onSubmit { get { return m_OnSubmit; } set { SetPropertyUtility.SetClass(ref m_OnSubmit, value); } }

        public SelectionEvent onSelect { get { return m_OnSelect; } set { SetPropertyUtility.SetClass(ref m_OnSelect, value); } }

        public SelectionEvent onDeselect { get { return m_OnDeselect; } set { SetPropertyUtility.SetClass(ref m_OnDeselect, value); } }

        public TextSelectionEvent onTextSelection { get { return m_OnTextSelection; } set { SetPropertyUtility.SetClass(ref m_OnTextSelection, value); } }

        public TextSelectionEvent onEndTextSelection { get { return m_OnEndTextSelection; } set { SetPropertyUtility.SetClass(ref m_OnEndTextSelection, value); } }

        public OnChangeEvent onValueChanged { get { return m_OnValueChanged; } set { SetPropertyUtility.SetClass(ref m_OnValueChanged, value); } }

        public OnValidateInput onValidateInput { get { return m_OnValidateInput; } set { SetPropertyUtility.SetClass(ref m_OnValidateInput, value); } }

        public int characterLimit { get { return m_CharacterLimit; } set { if (SetPropertyUtility.SetStruct(ref m_CharacterLimit, Math.Max(0, value))) UpdateLabel(); } }

        //public bool isInteractableControl { set { if ( } }

        /// <summary>
        /// Set the point size on both Placeholder and Input text object.
        /// </summary>
        public float pointSize
        {
            get { return m_GlobalPointSize; }
            set {
                    if (SetPropertyUtility.SetStruct(ref m_GlobalPointSize, Math.Max(0, value)))
                    {
                        SetGlobalPointSize(m_GlobalPointSize);
                        UpdateLabel();
                    }
                }
        }

        /// <summary>
        /// Sets the Font Asset on both Placeholder and Input child objects.
        /// </summary>
        public TMP_FontAsset fontAsset
        {
            get { return m_GlobalFontAsset; }
            set
            {
                if (SetPropertyUtility.SetClass(ref m_GlobalFontAsset, value))
                {
                    SetGlobalFontAsset(m_GlobalFontAsset);
                    UpdateLabel();
                }
            }
        }
        [SerializeField]
        protected TMP_FontAsset m_GlobalFontAsset;

        /// <summary>
        /// Determines if the whole text will be selected when focused.
        /// </summary>
        public bool onFocusSelectAll
        {
            get { return m_OnFocusSelectAll; }
            set { m_OnFocusSelectAll = value; }
        }
        [SerializeField]
        protected bool m_OnFocusSelectAll = true;
        protected bool m_isSelectAll;

        /// <summary>
        /// Determines if the text and caret position as well as selection will be reset when the input field is deactivated.
        /// </summary>
        public bool resetOnDeActivation
        {
            get { return m_ResetOnDeActivation; }
            set { m_ResetOnDeActivation = value; }
        }
        [SerializeField]
        protected bool m_ResetOnDeActivation = true;

        /// <summary>
        /// Controls whether the original text is restored when pressing "ESC".
        /// </summary>
        public bool restoreOriginalTextOnEscape
        {
            get { return m_RestoreOriginalTextOnEscape; }
            set { m_RestoreOriginalTextOnEscape = value; }
        }
        [SerializeField]
        private bool m_RestoreOriginalTextOnEscape = true;

        /// <summary>
        /// Is Rich Text editing allowed?
        /// </summary>
        public bool isRichTextEditingAllowed
        {
            get { return m_isRichTextEditingAllowed; }
            set { m_isRichTextEditingAllowed = value; }
        }
        [SerializeField]
        protected bool m_isRichTextEditingAllowed = true;


        // Content Type related

        public ContentType contentType { get { return m_ContentType; } set { if (SetPropertyUtility.SetStruct(ref m_ContentType, value)) EnforceContentType(); } }

        public LineType lineType { get { return m_LineType; } set { if (SetPropertyUtility.SetStruct(ref m_LineType, value)) SetTextComponentWrapMode(); SetToCustomIfContentTypeIsNot(ContentType.Standard, ContentType.Autocorrected); } }

        public InputType inputType { get { return m_InputType; } set { if (SetPropertyUtility.SetStruct(ref m_InputType, value)) SetToCustom(); } }

        public TouchScreenKeyboardType keyboardType { get { return m_KeyboardType; } set { if (SetPropertyUtility.SetStruct(ref m_KeyboardType, value)) SetToCustom(); } }

        public CharacterValidation characterValidation { get { return m_CharacterValidation; } set { if (SetPropertyUtility.SetStruct(ref m_CharacterValidation, value)) SetToCustom(); } }

        /// <summary>
        /// Sets the Input Validation to use a Custom Input Validation script.
        /// </summary>
        public TMP_InputValidator inputValidator
        {
            get { return m_InputValidator; }
            set {  if (SetPropertyUtility.SetClass(ref m_InputValidator, value)) SetToCustom(CharacterValidation.CustomValidator); }
        }
        [SerializeField]
        protected TMP_InputValidator m_InputValidator = null; 

        public bool readOnly { get { return m_ReadOnly; } set { m_ReadOnly = value; } }

        public bool richText { get { return m_RichText; } set { m_RichText = value; SetTextComponentRichTextMode(); } }

        // Derived property
        public bool multiLine { get { return m_LineType == LineType.MultiLineNewline || lineType == LineType.MultiLineSubmit; } }
        // Not shown in Inspector.
        public char asteriskChar { get { return m_AsteriskChar; } set { if (SetPropertyUtility.SetStruct(ref m_AsteriskChar, value)) UpdateLabel(); } }
        public bool wasCanceled { get { return m_WasCanceled; } }


        protected void ClampStringPos(ref int pos)
        {
            if (pos < 0)
                pos = 0;
            else if (pos > text.Length)
                pos = text.Length;
        }

        protected void ClampCaretPos(ref int pos)
        {
            if (pos < 0)
                pos = 0;
            else if (pos > m_TextComponent.textInfo.characterCount - 1)
                pos = m_TextComponent.textInfo.characterCount - 1;
        }

        /// <summary>
        /// Current position of the cursor.
        /// Getters are public Setters are protected
        /// </summary>

        protected int caretPositionInternal { get { return m_CaretPosition + Input.compositionString.Length; } set { m_CaretPosition = value; ClampCaretPos(ref m_CaretPosition); } }
        protected int stringPositionInternal { get { return m_StringPosition + Input.compositionString.Length; } set { m_StringPosition = value; ClampStringPos(ref m_StringPosition); } }

        protected int caretSelectPositionInternal { get { return m_CaretSelectPosition + Input.compositionString.Length; } set { m_CaretSelectPosition = value; ClampCaretPos(ref m_CaretSelectPosition); } }
        protected int stringSelectPositionInternal { get { return m_StringSelectPosition + Input.compositionString.Length; } set { m_StringSelectPosition = value; ClampStringPos(ref m_StringSelectPosition); } }

        private bool hasSelection { get { return stringPositionInternal != stringSelectPositionInternal; } }
        private bool m_isSelected;
        private bool isStringPositionDirty;
        private bool m_forceRectTransformAdjustment;

        /// <summary>
        /// Get: Returns the focus position as thats the position that moves around even during selection.
        /// Set: Set both the anchor and focus position such that a selection doesn't happen
        /// </summary>
        public int caretPosition
        {
            get { return caretSelectPositionInternal; }
            set { selectionAnchorPosition = value; selectionFocusPosition = value; isStringPositionDirty = true; }
        }

        /// <summary>
        /// Get: Returns the fixed position of selection
        /// Set: If Input.compositionString is 0 set the fixed position
        /// </summary>
        public int selectionAnchorPosition
        {
            get
            {
                return caretPositionInternal;
            }

            set
            {
                if (Input.compositionString.Length != 0)
                    return;

                caretPositionInternal = value;
                isStringPositionDirty = true;
            }
        }

        /// <summary>
        /// Get: Returns the variable position of selection
        /// Set: If Input.compositionString is 0 set the variable position
        /// </summary>
        public int selectionFocusPosition
        {
            get
            {
                return caretSelectPositionInternal;
            }
            set
            {
                if (Input.compositionString.Length != 0)
                    return;

                caretSelectPositionInternal = value;
                isStringPositionDirty = true;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public int stringPosition
        {
            get { return stringSelectPositionInternal; }
            set { selectionStringAnchorPosition = value; selectionStringFocusPosition = value; }
        }


        /// <summary>
        /// The fixed position of the selection in the raw string which may contains rich text.
        /// </summary>
        public int selectionStringAnchorPosition
        {
            get
            {
                return stringPositionInternal;
            }

            set
            {
                if (Input.compositionString.Length != 0)
                    return;

                stringPositionInternal = value;
                //isStringPositionDirty = true;
            }
        }


        /// <summary>
        /// The variable position of the selection in the raw string which may contains rich text.
        /// </summary>
        public int selectionStringFocusPosition
        {
            get
            {
                return stringSelectPositionInternal;
            }
            set
            {
                if (Input.compositionString.Length != 0)
                    return;

                stringSelectPositionInternal = value;
                //isStringPositionDirty = true;
            }
        }


#if UNITY_EDITOR
        // Remember: This is NOT related to text validation!
        // This is Unity's own OnValidate method which is invoked when changing values in the Inspector.
        protected override void OnValidate()
        {
            base.OnValidate();
            EnforceContentType();

            m_CharacterLimit = Math.Max(0, m_CharacterLimit);

            //This can be invoked before OnEnabled is called. So we shouldn't be accessing other objects, before OnEnable is called.
            if (!IsActive())
                return;

            SetTextComponentRichTextMode();

            UpdateLabel();
            if (m_AllowInput)
                SetCaretActive();
        }

#endif // if UNITY_EDITOR

        protected override void OnEnable()
        {
            //Debug.Log("*** OnEnable() *** - " + this.name);

            base.OnEnable();

            if (m_Text == null)
                m_Text = string.Empty;

            if (Application.isPlaying)
            {
                if (m_CachedInputRenderer == null && m_TextComponent != null)
                {
                    GameObject go = new GameObject(transform.name + " Input Caret", typeof(RectTransform));

                    // Add MaskableGraphic Component
                    TMP_SelectionCaret caret = go.AddComponent<TMP_SelectionCaret>();
                    caret.raycastTarget = false;
                    caret.color = Color.clear;

                    go.hideFlags = HideFlags.DontSave;
                    go.transform.SetParent(m_TextComponent.transform.parent);
                    go.transform.SetAsFirstSibling();
                    go.layer = gameObject.layer;

                    caretRectTrans = go.GetComponent<RectTransform>();
                    m_CachedInputRenderer = go.GetComponent<CanvasRenderer>();
                    m_CachedInputRenderer.SetMaterial(Graphic.defaultGraphicMaterial, Texture2D.whiteTexture);

                    // Needed as if any layout is present we want the caret to always be the same as the text area.
                    go.AddComponent<LayoutElement>().ignoreLayout = true;

                    AssignPositioningIfNeeded();
                }
            }

            // If we have a cached renderer then we had OnDisable called so just restore the material.
            if (m_CachedInputRenderer != null)
                m_CachedInputRenderer.SetMaterial(Graphic.defaultGraphicMaterial, Texture2D.whiteTexture);

            if (m_TextComponent != null)
            {
                m_TextComponent.RegisterDirtyVerticesCallback(MarkGeometryAsDirty);
                m_TextComponent.RegisterDirtyVerticesCallback(UpdateLabel);
                //m_TextComponent.ignoreRectMaskCulling = multiLine;

                m_DefaultTransformPosition = m_TextComponent.rectTransform.localPosition;

                // Cache reference to Vertical Scrollbar RectTransform and add listener.
                if (m_VerticalScrollbar != null)
                {
                    m_TextComponent.ignoreRectMaskCulling = true;
                    m_VerticalScrollbar.onValueChanged.AddListener(OnScrollbarValueChange);
                }

                UpdateLabel();
            }

            // Subscribe to event fired when text object has been regenerated.
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
        }

        protected override void OnDisable()
        {
            // the coroutine will be terminated, so this will ensure it restarts when we are next activated
            m_BlinkCoroutine = null;

            DeactivateInputField();
            if (m_TextComponent != null)
            {
                m_TextComponent.UnregisterDirtyVerticesCallback(MarkGeometryAsDirty);
                m_TextComponent.UnregisterDirtyVerticesCallback(UpdateLabel);

                if (m_VerticalScrollbar != null)
                    m_VerticalScrollbar.onValueChanged.RemoveListener(OnScrollbarValueChange);

            }
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            // Clear needs to be called otherwise sync never happens as the object is disabled.
            if (m_CachedInputRenderer != null)
                m_CachedInputRenderer.Clear();

            if (m_Mesh != null)
                DestroyImmediate(m_Mesh);
            m_Mesh = null;

            // Unsubscribe to event triggered when text object has been regenerated
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);

            base.OnDisable();
        }


        /// <summary>
        /// Method used to update the tracking of the caret position when the text object has been regenerated.
        /// </summary>
        /// <param name="obj"></param>
        private void ON_TEXT_CHANGED(UnityEngine.Object obj)
        {
            if (obj == m_TextComponent && Application.isPlaying)
            {
                caretPositionInternal = GetCaretPositionFromStringIndex(stringPositionInternal);
                caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);

                //Debug.Log("Updating Caret position - Caret Position: " + m_CaretPosition + "  Caret Select Position: " + m_CaretSelectPosition);
            }
        }


        IEnumerator CaretBlink()
        {
            // Always ensure caret is initially visible since it can otherwise be confusing for a moment.
            m_CaretVisible = true;
            yield return null;

            while (/*isFocused &&*/ m_CaretBlinkRate > 0)
            {
                // the blink rate is expressed as a frequency
                float blinkPeriod = 1f / m_CaretBlinkRate;

                // the caret should be ON if we are in the first half of the blink period
                bool blinkState = (Time.unscaledTime - m_BlinkStartTime) % blinkPeriod < blinkPeriod / 2;
                if (m_CaretVisible != blinkState)
                {
                    m_CaretVisible = blinkState;
                    if (!hasSelection)
                        MarkGeometryAsDirty();
                }

                // Then wait again.
                yield return null;
            }
            m_BlinkCoroutine = null;
        }

        void SetCaretVisible()
        {
            if (!m_AllowInput)
                return;

            m_CaretVisible = true;
            m_BlinkStartTime = Time.unscaledTime;
            SetCaretActive();
        }

        // SetCaretActive will not set the caret immediately visible - it will wait for the next time to blink.
        // However, it will handle things correctly if the blink speed changed from zero to non-zero or non-zero to zero.
        void SetCaretActive()
        {
            if (!m_AllowInput)
                return;

            if (m_CaretBlinkRate > 0.0f)
            {
                if (m_BlinkCoroutine == null)
                    m_BlinkCoroutine = StartCoroutine(CaretBlink());
            }
            else
            {
                m_CaretVisible = true;
            }
        }

        protected void OnFocus()
        {
            if (m_OnFocusSelectAll)
                SelectAll();
        }

        protected void SelectAll()
        {
            m_isSelectAll = true;
            stringPositionInternal = text.Length;
            stringSelectPositionInternal = 0;
        }

        /// <summary>
        /// Move to the end of the text.
        /// </summary>
        /// <param name="shift"></param>
        public void MoveTextEnd(bool shift)
        {
            if (m_isRichTextEditingAllowed)
            {
                int position = text.Length;

                if (shift)
                {
                    stringSelectPositionInternal = position;
                }
                else
                {
                    stringPositionInternal = position;
                    stringSelectPositionInternal = stringPositionInternal;
                }
            }
            else
            {
                int position = m_TextComponent.textInfo.characterCount - 1;

                if (shift)
                {
                    caretSelectPositionInternal = position;
                    stringSelectPositionInternal = GetStringIndexFromCaretPosition(position);
                }
                else
                {
                    caretPositionInternal = caretSelectPositionInternal = position;
                    stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(position);
                }
            }

            UpdateLabel();
        }

        /// <summary>
        /// Move to the start of the text.
        /// </summary>
        /// <param name="shift"></param>
        public void MoveTextStart(bool shift)
        {
            if (m_isRichTextEditingAllowed)
            {
                int position = 0;

                if (shift)
                {
                    stringSelectPositionInternal = position;
                }
                else
                {
                    stringPositionInternal = position;
                    stringSelectPositionInternal = stringPositionInternal;
                }
            }
            else
            {
                int position = 0;

                if (shift)
                {
                    caretSelectPositionInternal = position;
                    stringSelectPositionInternal = GetStringIndexFromCaretPosition(position);
                }
                else
                {
                    caretPositionInternal = caretSelectPositionInternal = position;
                    stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(position);
                }
            }

            UpdateLabel();
        }


        /// <summary>
        /// Move to the end of the current line of text.
        /// </summary>
        /// <param name="shift"></param>
        public void MoveToEndOfLine(bool shift, bool ctrl)
        {
            // Get the line the caret is currently located on.
            int currentLine = m_TextComponent.textInfo.characterInfo[caretPositionInternal].lineNumber;

            // Get the last character of the given line.
            int position = ctrl == true ? m_TextComponent.textInfo.characterCount - 1 : m_TextComponent.textInfo.lineInfo[currentLine].lastCharacterIndex;

            position = GetStringIndexFromCaretPosition(position);

            if (shift)
            {
                stringSelectPositionInternal = position;
            }
            else
            {
                stringPositionInternal = position;
                stringSelectPositionInternal = stringPositionInternal;
            }

            UpdateLabel();
        }

        /// <summary>
        /// Move to the start of the current line of text.
        /// </summary>
        /// <param name="shift"></param>
        public void MoveToStartOfLine(bool shift, bool ctrl)
        {
            // Get the line the caret is currently located on.
            int currentLine = m_TextComponent.textInfo.characterInfo[caretPositionInternal].lineNumber;

            // Get the last character of the given line.
            int position = ctrl == true ? 0 : m_TextComponent.textInfo.lineInfo[currentLine].firstCharacterIndex;

            position = GetStringIndexFromCaretPosition(position);

            if (shift)
            {
                stringSelectPositionInternal = position;
            }
            else
            {
                stringPositionInternal = position;
                stringSelectPositionInternal = stringPositionInternal;
            }

            UpdateLabel();
        }


        static string clipboard
        {
            get
            {
                return GUIUtility.systemCopyBuffer;
            }
            set
            {
                GUIUtility.systemCopyBuffer = value;
            }
        }

        private bool InPlaceEditing()
        {
            return !TouchScreenKeyboard.isSupported;
        }

        /// <summary>
        /// Update the text based on input.
        /// </summary>
        // TODO: Make LateUpdate a coroutine instead. Allows us to control the update to only be when the field is active.
        protected virtual void LateUpdate()
        {
            // Only activate if we are not already activated.
            if (m_ShouldActivateNextUpdate)
            {
                if (!isFocused)
                {
                    ActivateInputFieldInternal();
                    m_ShouldActivateNextUpdate = false;
                    return;
                }

                // Reset as we are already activated.
                m_ShouldActivateNextUpdate = false;
            }

            // Update Scrollbar if needed
            if (m_IsScrollbarUpdateRequired)
            {
                UpdateScrollbar();
                m_IsScrollbarUpdateRequired = false;
            }

            //if (!isFocused && !m_VerticalScrollbarEventHandler.isSelected)
            //{
            //    m_ForceDeactivation = true;
            //    DeactivateInputField();

            //    return;
            //}

            //if (!isFocused)
            //{
            //    GameObject currentSelection = EventSystem.current == null ? null : EventSystem.current.currentSelectedGameObject;

            //    if (currentSelection != null)
            //        Debug.Log("Current Selection is: " + EventSystem.current.currentSelectedGameObject);
            //    else
            //        Debug.Log("No GameObject is selected...");
            //}


            if (InPlaceEditing() || !isFocused)
                return;

            //Debug.Log(this + " has focus...");

            AssignPositioningIfNeeded();

            if (m_Keyboard == null || !m_Keyboard.active)
            {
                if (m_Keyboard != null)
                {
                    if (!m_ReadOnly)
                        text = m_Keyboard.text;

                    if (m_Keyboard.status == TouchScreenKeyboard.Status.Canceled)
                        m_WasCanceled = true;

                    if (m_Keyboard.status == TouchScreenKeyboard.Status.Done)
                        OnSubmit(null);
                }

                OnDeselect(null);
                return;
            }

            string val = m_Keyboard.text;

            if (m_Text != val)
            {
                if (m_ReadOnly)
                {
                    m_Keyboard.text = m_Text;
                }
                else
                {
                    m_Text = "";

                    for (int i = 0; i < val.Length; ++i)
                    {
                        char c = val[i];

                        if (c == '\r' || (int)c == 3)
                            c = '\n';

                        if (onValidateInput != null)
                            c = onValidateInput(m_Text, m_Text.Length, c);
                        else if (characterValidation != CharacterValidation.None)
                            c = Validate(m_Text, m_Text.Length, c);

                        if (lineType == LineType.MultiLineSubmit && c == '\n')
                        {
                            m_Keyboard.text = m_Text;

                            OnSubmit(null);
                            OnDeselect(null);
                            return;
                        }

                        if (c != 0)
                            m_Text += c;
                    }

                    if (characterLimit > 0 && m_Text.Length > characterLimit)
                        m_Text = m_Text.Substring(0, characterLimit);
                    stringPositionInternal = stringSelectPositionInternal = m_Text.Length;

                    // Set keyboard text before updating label, as we might have changed it with validation
                    // and update label will take the old value from keyboard if we don't change it here
                    if (m_Text != val)
                        m_Keyboard.text = m_Text;

                    SendOnValueChangedAndUpdateLabel();
                }
            }


            if (m_Keyboard.status == TouchScreenKeyboard.Status.Done)
            {
                if (m_Keyboard.status == TouchScreenKeyboard.Status.Canceled)
                    m_WasCanceled = true;

                OnDeselect(null);
            }
        }


        private bool MayDrag(PointerEventData eventData)
        {
            return IsActive() &&
                   IsInteractable() &&
                   eventData.button == PointerEventData.InputButton.Left &&
                   m_TextComponent != null &&
                   m_Keyboard == null;
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            m_UpdateDrag = true;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            CaretPosition insertionSide;
            int insertionIndex = TMP_TextUtilities.GetCursorIndexFromPosition(m_TextComponent, eventData.position, eventData.pressEventCamera, out insertionSide);

            if (insertionSide == CaretPosition.Left)
                stringSelectPositionInternal = GetStringIndexFromCaretPosition(insertionIndex);
            else if (insertionSide == CaretPosition.Right)
                stringSelectPositionInternal = GetStringIndexFromCaretPosition(insertionIndex) + 1;

            caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);

            MarkGeometryAsDirty();

            m_DragPositionOutOfBounds = !RectTransformUtility.RectangleContainsScreenPoint(textViewport, eventData.position, eventData.pressEventCamera);
            if (m_DragPositionOutOfBounds && m_DragCoroutine == null)
                m_DragCoroutine = StartCoroutine(MouseDragOutsideRect(eventData));

            eventData.Use();
        }

        IEnumerator MouseDragOutsideRect(PointerEventData eventData)
        {
            while (m_UpdateDrag && m_DragPositionOutOfBounds)
            {
                Vector2 localMousePos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(textViewport, eventData.position, eventData.pressEventCamera, out localMousePos);

                Rect rect = textViewport.rect;

                if (multiLine)
                {
                    if (localMousePos.y > rect.yMax)
                        MoveUp(true, true);
                    else if (localMousePos.y < rect.yMin)
                        MoveDown(true, true);
                }
                else
                {
                    if (localMousePos.x < rect.xMin)
                        MoveLeft(true, false);
                    else if (localMousePos.x > rect.xMax)
                        MoveRight(true, false);
                }

                UpdateLabel();

                float delay = multiLine ? kVScrollSpeed : kHScrollSpeed;
                yield return new WaitForSeconds(delay);
                //yield return new WaitForSecondsRealtime(delay); // Unity 5.4
            }
            m_DragCoroutine = null;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            m_UpdateDrag = false;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            EventSystem.current.SetSelectedGameObject(gameObject, eventData);

            bool hadFocusBefore = m_AllowInput;
            base.OnPointerDown(eventData);

            if (!InPlaceEditing())
            {
                if (m_Keyboard == null || !m_Keyboard.active)
                {
                    OnSelect(eventData);
                    return;
                }
            }

            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            // Check for Double Click
            bool isDoubleClick = false;
            float timeStamp = Time.unscaledTime;

            if (m_ClickStartTime + m_DoubleClickDelay > timeStamp)
                isDoubleClick = true;   

            m_ClickStartTime = timeStamp;

            // Only set caret position if we didn't just get focus now.
            // Otherwise it will overwrite the select all on focus.
            if (hadFocusBefore || !m_OnFocusSelectAll)
            {
                CaretPosition insertionSide;
                int insertionIndex = TMP_TextUtilities.GetCursorIndexFromPosition(m_TextComponent, eventData.position, eventData.pressEventCamera, out insertionSide);

                if (shift)
                {
                    if (insertionSide == CaretPosition.Left)
                        stringSelectPositionInternal = GetStringIndexFromCaretPosition(insertionIndex);
                    else if (insertionSide == CaretPosition.Right)
                        stringSelectPositionInternal = GetStringIndexFromCaretPosition(insertionIndex) + 1;
                }
                else
                {
                    if (insertionSide == CaretPosition.Left)
                        stringPositionInternal = stringSelectPositionInternal = GetStringIndexFromCaretPosition(insertionIndex);
                    else if (insertionSide == CaretPosition.Right)
                        stringPositionInternal = stringSelectPositionInternal = GetStringIndexFromCaretPosition(insertionIndex) + 1;
                }


                if (isDoubleClick)
                {
                    int wordIndex = TMP_TextUtilities.FindIntersectingWord(m_TextComponent, eventData.position, eventData.pressEventCamera);

                    if (wordIndex != -1)
                    {
                        // Select current word
                        caretPositionInternal = m_TextComponent.textInfo.wordInfo[wordIndex].firstCharacterIndex;
                        caretSelectPositionInternal = m_TextComponent.textInfo.wordInfo[wordIndex].lastCharacterIndex + 1;

                        stringPositionInternal = GetStringIndexFromCaretPosition(caretPositionInternal);
                        stringSelectPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
                    }
                    else
                    {
                        // Select current character
                        caretPositionInternal = GetCaretPositionFromStringIndex(stringPositionInternal);

                        stringSelectPositionInternal += 1;
                        caretSelectPositionInternal = caretPositionInternal + 1;
                        caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
                    }
                }
                else
                {
                    caretPositionInternal = caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringPositionInternal);
                }
            }

            UpdateLabel();
            eventData.Use();
        }

        protected enum EditState
        {
            Continue,
            Finish
        }

        protected EditState KeyPressed(Event evt)
        {
            var currentEventModifiers = evt.modifiers;
            RuntimePlatform rp = Application.platform;
            bool isMac = (rp == RuntimePlatform.OSXEditor || rp == RuntimePlatform.OSXPlayer);
            bool ctrl = isMac ? (currentEventModifiers & EventModifiers.Command) != 0 : (currentEventModifiers & EventModifiers.Control) != 0;
            bool shift = (currentEventModifiers & EventModifiers.Shift) != 0;
            bool alt = (currentEventModifiers & EventModifiers.Alt) != 0;
            bool ctrlOnly = ctrl && !alt && !shift;

            switch (evt.keyCode)
            {
                case KeyCode.Backspace:
                    {
                        Backspace();
                        return EditState.Continue;
                    }

                case KeyCode.Delete:
                    {
                        ForwardSpace();
                        return EditState.Continue;
                    }

                case KeyCode.Home:
                    {
                        MoveToStartOfLine(shift, ctrl);
                        return EditState.Continue;
                    }

                case KeyCode.End:
                    {
                        MoveToEndOfLine(shift, ctrl);
                        return EditState.Continue;
                    }

                // Select All
                case KeyCode.A:
                    {
                        if (ctrlOnly)
                        {
                            SelectAll();
                            return EditState.Continue;
                        }
                        break;
                    }

                // Copy
                case KeyCode.C:
                    {
                        if (ctrlOnly)
                        {
                            if (inputType != InputType.Password)
                                clipboard = GetSelectedString();
                            else
                                clipboard = "";
                            return EditState.Continue;
                        }
                        break;
                    }

                // Paste
                case KeyCode.V:
                    {
                        if (ctrlOnly)
                        {
                            Append(clipboard);
                            return EditState.Continue;
                        }
                        break;
                    }

                // Cut
                case KeyCode.X:
                    {
                        if (ctrlOnly)
                        {
                            if (inputType != InputType.Password)
                                clipboard = GetSelectedString();
                            else
                                clipboard = "";
                            Delete();
                            SendOnValueChangedAndUpdateLabel();
                            return EditState.Continue;
                        }
                        break;
                    }

                case KeyCode.LeftArrow:
                    {
                        MoveLeft(shift, ctrl);
                        return EditState.Continue;
                    }

                case KeyCode.RightArrow:
                    {
                        MoveRight(shift, ctrl);
                        return EditState.Continue;
                    }

                case KeyCode.UpArrow:
                    {
                        MoveUp(shift);
                        return EditState.Continue;
                    }

                case KeyCode.DownArrow:
                    {
                        MoveDown(shift);
                        return EditState.Continue;
                    }

                case KeyCode.PageUp:
                    {
                        MovePageUp(shift);
                        return EditState.Continue;
                    }

                case KeyCode.PageDown:
                    {
                        MovePageDown(shift);
                        return EditState.Continue;
                    }

                // Submit
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    {
                        if (lineType != LineType.MultiLineNewline)
                        {
                            return EditState.Finish;
                        }
                        break;
                    }

                case KeyCode.Escape:
                    {
                        m_WasCanceled = true;
                        return EditState.Finish;
                    }
            }

            char c = evt.character;

            // Don't allow return chars or tabulator key to be entered into single line fields.
            if (!multiLine && (c == '\t' || c == '\r' || c == 10))
                return EditState.Continue;

            // Convert carriage return and end-of-text characters to newline.
            if (c == '\r' || (int)c == 3)
                c = '\n';

            if (IsValidChar(c))
            {
                Append(c);
            }

            if (c == 0)
            {
                if (Input.compositionString.Length > 0)
                {
                    UpdateLabel();
                }
            }
            return EditState.Continue;
        }

        private bool IsValidChar(char c)
        {
            // Delete key on mac
            if ((int)c == 127)
                return false;
            // Accept newline and tab
            if (c == '\t' || c == '\n')
                return true;

            return m_TextComponent.font.HasCharacter(c, true);
        }

        /// <summary>
        /// Handle the specified event.
        /// </summary>
        private Event m_ProcessingEvent = new Event();

        public void ProcessEvent(Event e)
        {
            KeyPressed(e);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnUpdateSelected(BaseEventData eventData)
        {
            if (!isFocused)
                return;

            bool consumedEvent = false;
            while (Event.PopEvent(m_ProcessingEvent))
            {
                if (m_ProcessingEvent.rawType == EventType.KeyDown)
                {
                    consumedEvent = true;
                    var shouldContinue = KeyPressed(m_ProcessingEvent);
                    if (shouldContinue == EditState.Finish)
                    {
                        SendOnSubmit();
                        DeactivateInputField();
                        break;
                    }
                }

                switch (m_ProcessingEvent.type)
                {
                    case EventType.ValidateCommand:
                    case EventType.ExecuteCommand:
                        switch (m_ProcessingEvent.commandName)
                        {
                            case "SelectAll":
                                SelectAll();
                                consumedEvent = true;
                                break;
                        }
                        break;
                }
            }

            if (consumedEvent)
                UpdateLabel();

            eventData.Use();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnScroll(PointerEventData eventData)
        {
            if (m_TextComponent.preferredHeight < m_TextViewport.rect.height) return;

            float scrollDirection = -eventData.scrollDelta.y;

            m_ScrollPosition = m_ScrollPosition + (1f / m_TextComponent.textInfo.lineCount) * scrollDirection * m_ScrollSensitivity;

            m_ScrollPosition = Mathf.Clamp01(m_ScrollPosition);

            AdjustTextPositionRelativeToViewport(m_ScrollPosition);

            // Disable focus until user re-selected the input field.
            m_AllowInput = false;

            if (m_VerticalScrollbar)
            {
                m_IsUpdatingScrollbarValues = true;
                m_VerticalScrollbar.value = m_ScrollPosition;
                //m_VerticalScrollbar.numberOfSteps = (int)(m_TextComponent.textInfo.lineCount / scrollSensitivity);
            }

            //Debug.Log("Scroll Position:" + m_ScrollPosition);
        }


        private string GetSelectedString()
        {
            if (!hasSelection)
                return "";

            int startPos = stringPositionInternal;
            int endPos = stringSelectPositionInternal;

            // Ensure pos is always less then selPos to make the code simpler
            if (startPos > endPos)
            {
                int temp = startPos;
                startPos = endPos;
                endPos = temp;
            }

            //for (int i = m_CaretPosition; i < m_CaretSelectPosition; i++)
            //{
            //    Debug.Log("Character [" + m_TextComponent.textInfo.characterInfo[i].character + "] using Style [" + m_TextComponent.textInfo.characterInfo[i].style + "] has been selected.");
            //}


            return text.Substring(startPos, endPos - startPos);
        }

        private int FindtNextWordBegin()
        {
            if (stringSelectPositionInternal + 1 >= text.Length)
                return text.Length;

            int spaceLoc = text.IndexOfAny(kSeparators, stringSelectPositionInternal + 1);

            if (spaceLoc == -1)
                spaceLoc = text.Length;
            else
                spaceLoc++;

            return spaceLoc;
        }

        private void MoveRight(bool shift, bool ctrl)
        {
            if (hasSelection && !shift)
            {
                // By convention, if we have a selection and move right without holding shift,
                // we just place the cursor at the end.
                stringPositionInternal = stringSelectPositionInternal = Mathf.Max(stringPositionInternal, stringSelectPositionInternal);
                caretPositionInternal = caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);

                #if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
                #endif
                return;
            }

            int position;
            if (ctrl)
                position = FindtNextWordBegin();
            else
            {
                if (m_isRichTextEditingAllowed)
                    position = stringSelectPositionInternal + 1;
                else
                    position = GetStringIndexFromCaretPosition(caretSelectPositionInternal + 1);

            }

            if (shift)
            {
                stringSelectPositionInternal = position;
                caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
            }
            else
            {
                stringSelectPositionInternal = stringPositionInternal = position;
                caretSelectPositionInternal = caretPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
            }

            #if TMP_DEBUG_MODE
            Debug.Log("Caret Position: " + caretPositionInternal + "  Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + "  String Select Position: " + stringSelectPositionInternal);
            #endif
        }

        private int FindtPrevWordBegin()
        {
            if (stringSelectPositionInternal - 2 < 0)
                return 0;

            int spaceLoc = text.LastIndexOfAny(kSeparators, stringSelectPositionInternal - 2);

            if (spaceLoc == -1)
                spaceLoc = 0;
            else
                spaceLoc++;

            return spaceLoc;
        }

        private void MoveLeft(bool shift, bool ctrl)
        {
            if (hasSelection && !shift)
            {
                // By convention, if we have a selection and move left without holding shift,
                // we just place the cursor at the start.
                stringPositionInternal = stringSelectPositionInternal = Mathf.Min(stringPositionInternal, stringSelectPositionInternal);
                caretPositionInternal = caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);

                #if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
                #endif
                return;
            }

            int position;
            if (ctrl)
                position = FindtPrevWordBegin();
            else
            {
                if (m_isRichTextEditingAllowed)
                    position = stringSelectPositionInternal - 1;
                else
                    position = GetStringIndexFromCaretPosition(caretSelectPositionInternal - 1);
            }

            if (shift)
            {
                stringSelectPositionInternal = position;
                caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
            }
            else
            {

                stringSelectPositionInternal = stringPositionInternal = position;
                caretSelectPositionInternal = caretPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
            }

            #if TMP_DEBUG_MODE
            Debug.Log("Caret Position: " + caretPositionInternal + "  Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + "  String Select Position: " + stringSelectPositionInternal);
            #endif
        }


        private int LineUpCharacterPosition(int originalPos, bool goToFirstChar)
        {
            if (originalPos >= m_TextComponent.textInfo.characterCount)
                originalPos -= 1;

            TMP_CharacterInfo originChar = m_TextComponent.textInfo.characterInfo[originalPos];
            int originLine = originChar.lineNumber;

            // We are on the first line return first character
            if (originLine - 1 < 0)
                return goToFirstChar ? 0 : originalPos;

            int endCharIdx = m_TextComponent.textInfo.lineInfo[originLine].firstCharacterIndex - 1;

            int closest = -1;
            float distance = TMP_Math.FLOAT_MAX;
            float range = 0;

            for (int i = m_TextComponent.textInfo.lineInfo[originLine - 1].firstCharacterIndex; i < endCharIdx; ++i)
            {
                TMP_CharacterInfo currentChar = m_TextComponent.textInfo.characterInfo[i];

                float d = originChar.origin - currentChar.origin;
                float r = d / (currentChar.xAdvance - currentChar.origin);

                if (r >= 0 && r <= 1)
                {
                    if (r < 0.5f)
                        return i;
                    else
                        return i + 1;
                }

                d = Mathf.Abs(d);

                if (d < distance)
                {
                    closest = i;
                    distance = d;
                    range = r;
                }
            }

            if (closest == -1) return endCharIdx;

            //Debug.Log("Returning nearest character with Range = " + range);

            if (range < 0.5f)
                return closest;
            else
                return closest + 1;
        }


        private int LineDownCharacterPosition(int originalPos, bool goToLastChar)
        {
            if (originalPos >= m_TextComponent.textInfo.characterCount)
                return m_TextComponent.textInfo.characterCount - 1; // text.Length;

            TMP_CharacterInfo originChar = m_TextComponent.textInfo.characterInfo[originalPos];
            int originLine = originChar.lineNumber;

            //// We are on the last line return last character
            if (originLine + 1 >= m_TextComponent.textInfo.lineCount)
                return goToLastChar ? m_TextComponent.textInfo.characterCount - 1 : originalPos;

            // Need to determine end line for next line.
            int endCharIdx = m_TextComponent.textInfo.lineInfo[originLine + 1].lastCharacterIndex;

            int closest = -1;
            float distance = TMP_Math.FLOAT_MAX;
            float range = 0;

            for (int i = m_TextComponent.textInfo.lineInfo[originLine + 1].firstCharacterIndex; i < endCharIdx; ++i)
            {
                TMP_CharacterInfo currentChar = m_TextComponent.textInfo.characterInfo[i];

                float d = originChar.origin - currentChar.origin;
                float r = d / (currentChar.xAdvance - currentChar.origin);

                if (r >= 0 && r <= 1)
                {
                    if (r < 0.5f)
                        return i;
                    else
                        return i + 1;
                }

                d = Mathf.Abs(d);

                if (d < distance)
                {
                    closest = i;
                    distance = d;
                    range = r;
                }
            }

            if (closest == -1) return endCharIdx;

            //Debug.Log("Returning nearest character with Range = " + range);

            if (range < 0.5f)
                return closest;
            else
                return closest + 1;
        }


         private int PageUpCharacterPosition(int originalPos, bool goToFirstChar)
        {
            if (originalPos >= m_TextComponent.textInfo.characterCount)
                originalPos -= 1;

            TMP_CharacterInfo originChar = m_TextComponent.textInfo.characterInfo[originalPos];
            int originLine = originChar.lineNumber;

            // We are on the first line return first character
            if (originLine - 1 < 0)
                return goToFirstChar ? 0 : originalPos;

            float viewportHeight = m_TextViewport.rect.height;

            int newLine = originLine - 1;
            // Iterate through each subsequent line to find the first baseline that is not visible in the viewport.
            for (; newLine > 0; newLine--)
            {
                if (m_TextComponent.textInfo.lineInfo[newLine].baseline > m_TextComponent.textInfo.lineInfo[originLine].baseline + viewportHeight)
                    break;
            }

            int endCharIdx = m_TextComponent.textInfo.lineInfo[newLine].lastCharacterIndex;

            int closest = -1;
            float distance = TMP_Math.FLOAT_MAX;
            float range = 0;

            for (int i = m_TextComponent.textInfo.lineInfo[newLine].firstCharacterIndex; i < endCharIdx; ++i)
            {
                TMP_CharacterInfo currentChar = m_TextComponent.textInfo.characterInfo[i];

                float d = originChar.origin - currentChar.origin;
                float r = d / (currentChar.xAdvance - currentChar.origin);

                if (r >= 0 && r <= 1)
                {
                    if (r < 0.5f)
                        return i;
                    else
                        return i + 1;
                }

                d = Mathf.Abs(d);

                if (d < distance)
                {
                    closest = i;
                    distance = d;
                    range = r;
                }
            }

            if (closest == -1) return endCharIdx;

            //Debug.Log("Returning nearest character with Range = " + range);

            if (range < 0.5f)
                return closest;
            else
                return closest + 1;
        }


         private int PageDownCharacterPosition(int originalPos, bool goToLastChar)
        {
            if (originalPos >= m_TextComponent.textInfo.characterCount)
                return m_TextComponent.textInfo.characterCount - 1;

            TMP_CharacterInfo originChar = m_TextComponent.textInfo.characterInfo[originalPos];
            int originLine = originChar.lineNumber;

            // We are on the last line return last character
            if (originLine + 1 >= m_TextComponent.textInfo.lineCount)
                return goToLastChar ? m_TextComponent.textInfo.characterCount - 1 : originalPos;

            float viewportHeight = m_TextViewport.rect.height;

            int newLine = originLine + 1;
            // Iterate through each subsequent line to find the first baseline that is not visible in the viewport.
            for (; newLine < m_TextComponent.textInfo.lineCount - 1; newLine++)
            {
                if (m_TextComponent.textInfo.lineInfo[newLine].baseline < m_TextComponent.textInfo.lineInfo[originLine].baseline - viewportHeight)
                    break;
            }

            // Need to determine end line for next line.
            int endCharIdx = m_TextComponent.textInfo.lineInfo[newLine].lastCharacterIndex;

            int closest = -1;
            float distance = TMP_Math.FLOAT_MAX;
            float range = 0;

            for (int i = m_TextComponent.textInfo.lineInfo[newLine].firstCharacterIndex; i < endCharIdx; ++i)
            {
                TMP_CharacterInfo currentChar = m_TextComponent.textInfo.characterInfo[i];

                float d = originChar.origin - currentChar.origin;
                float r = d / (currentChar.xAdvance - currentChar.origin);

                if (r >= 0 && r <= 1)
                {
                    if (r < 0.5f)
                        return i;
                    else
                        return i + 1;
                }

                d = Mathf.Abs(d);

                if (d < distance)
                {
                    closest = i;
                    distance = d;
                    range = r;
                }
            }

            if (closest == -1) return endCharIdx;

            if (range < 0.5f)
                return closest;
            else
                return closest + 1;
        }


        private void MoveDown(bool shift)
        {
            MoveDown(shift, true);
        }


        private void MoveDown(bool shift, bool goToLastChar)
        {
            if (hasSelection && !shift)
            {
                // If we have a selection and press down without shift,
                // set caret to end of selection before we move it down.
                caretPositionInternal = caretSelectPositionInternal = Mathf.Max(caretPositionInternal, caretSelectPositionInternal);
            }

            int position = multiLine ? LineDownCharacterPosition(caretSelectPositionInternal, goToLastChar) : m_TextComponent.textInfo.characterCount - 1; // text.Length;

            if (shift)
            {
                caretSelectPositionInternal = position;
                stringSelectPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
            }
            else
            {
                caretSelectPositionInternal = caretPositionInternal = position;
                stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
            }

            #if TMP_DEBUG_MODE
            Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
            #endif
        }

        private void MoveUp(bool shift)
        {
            MoveUp(shift, true);
        }


        private void MoveUp(bool shift, bool goToFirstChar)
        {
            if (hasSelection && !shift)
            {
                // If we have a selection and press up without shift,
                // set caret position to start of selection before we move it up.
                caretPositionInternal = caretSelectPositionInternal = Mathf.Min(caretPositionInternal, caretSelectPositionInternal);
            }

            int position = multiLine ? LineUpCharacterPosition(caretSelectPositionInternal, goToFirstChar) : 0;

            if (shift)
            {
                caretSelectPositionInternal = position;
                stringSelectPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
            }
            else
            {
                caretSelectPositionInternal = caretPositionInternal = position;
                stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
            }

            #if TMP_DEBUG_MODE
            Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
            #endif
        }


        private void MovePageUp(bool shift)
        {
            MovePageUp(shift, true);
        }

        private void MovePageUp(bool shift, bool goToFirstChar)
        {
            if (hasSelection && !shift)
            {
                // If we have a selection and press up without shift,
                // set caret position to start of selection before we move it up.
                caretPositionInternal = caretSelectPositionInternal = Mathf.Min(caretPositionInternal, caretSelectPositionInternal);
            }

            int position = multiLine ? PageUpCharacterPosition(caretSelectPositionInternal, goToFirstChar) : 0;

            if (shift)
            {
                caretSelectPositionInternal = position;
                stringSelectPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
            }
            else
            {
                caretSelectPositionInternal = caretPositionInternal = position;
                stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
            }


            // Scroll to top of viewport
            //int currentLine = m_TextComponent.textInfo.characterInfo[position].lineNumber;
            //float lineAscender = m_TextComponent.textInfo.lineInfo[currentLine].ascender;

            // Adjust text area up or down if not in single line mode.
            if (m_LineType != LineType.SingleLine)
            {
                float offset = m_TextViewport.rect.height; // m_TextViewport.rect.yMax - (m_TextComponent.rectTransform.anchoredPosition.y + lineAscender);

                float topTextBounds = m_TextComponent.rectTransform.position.y + m_TextComponent.textBounds.max.y;
                float topViewportBounds = m_TextViewport.position.y + m_TextViewport.rect.yMax;

                offset = topViewportBounds > topTextBounds + offset ? offset : topViewportBounds - topTextBounds;

                m_TextComponent.rectTransform.anchoredPosition += new Vector2(0, offset);
                AssignPositioningIfNeeded();
                m_IsScrollbarUpdateRequired = true;
            }

            #if TMP_DEBUG_MODE
            Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
            #endif

        }


        private void MovePageDown(bool shift)
        {
            MovePageDown(shift, true);
        }

        private void MovePageDown(bool shift, bool goToLastChar)
        {
             if (hasSelection && !shift)
            {
                // If we have a selection and press down without shift,
                // set caret to end of selection before we move it down.
                caretPositionInternal = caretSelectPositionInternal = Mathf.Max(caretPositionInternal, caretSelectPositionInternal);
            }

            int position = multiLine ? PageDownCharacterPosition(caretSelectPositionInternal, goToLastChar) : m_TextComponent.textInfo.characterCount - 1;

            if (shift)
            {
                caretSelectPositionInternal = position;
                stringSelectPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
            }
            else
            {
                caretSelectPositionInternal = caretPositionInternal = position;
                stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
            }

            // Scroll to top of viewport
            //int currentLine = m_TextComponent.textInfo.characterInfo[position].lineNumber;
            //float lineAscender = m_TextComponent.textInfo.lineInfo[currentLine].ascender;

            // Adjust text area up or down if not in single line mode.
            if (m_LineType != LineType.SingleLine)
            {
                float offset = m_TextViewport.rect.height; // m_TextViewport.rect.yMax - (m_TextComponent.rectTransform.anchoredPosition.y + lineAscender);

                float bottomTextBounds = m_TextComponent.rectTransform.position.y + m_TextComponent.textBounds.min.y;
                float bottomViewportBounds = m_TextViewport.position.y + m_TextViewport.rect.yMin;

                offset = bottomViewportBounds > bottomTextBounds + offset ? offset : bottomViewportBounds - bottomTextBounds;

                m_TextComponent.rectTransform.anchoredPosition += new Vector2(0, offset);
                AssignPositioningIfNeeded();
                m_IsScrollbarUpdateRequired = true;
            }

            #if TMP_DEBUG_MODE
            Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
            #endif

        }

        private void Delete()
        {
            if (m_ReadOnly)
                return;

            if (stringPositionInternal == stringSelectPositionInternal)
                return;

            if (m_isRichTextEditingAllowed || m_isSelectAll)
            {
                // Handling of Delete when Rich Text is allowed.
                if (stringPositionInternal < stringSelectPositionInternal)
                {
                    m_Text = text.Substring(0, stringPositionInternal) + text.Substring(stringSelectPositionInternal, text.Length - stringSelectPositionInternal);
                    stringSelectPositionInternal = stringPositionInternal;
                }
                else
                {
                    m_Text = text.Substring(0, stringSelectPositionInternal) + text.Substring(stringPositionInternal, text.Length - stringPositionInternal);
                    stringPositionInternal = stringSelectPositionInternal;
                }

                m_isSelectAll = false;
            }
            else
            {
                stringPositionInternal = GetStringIndexFromCaretPosition(caretPositionInternal);
                stringSelectPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);

                // Handling of Delete when Rich Text is not allowed.
                if (caretPositionInternal < caretSelectPositionInternal)
                {
                    m_Text = text.Substring(0, stringPositionInternal) + text.Substring(stringSelectPositionInternal, text.Length - stringSelectPositionInternal);

                    stringSelectPositionInternal = stringPositionInternal;
                    caretSelectPositionInternal = caretPositionInternal;
                }
                else
                {
                    m_Text = text.Substring(0, stringSelectPositionInternal) + text.Substring(stringPositionInternal, text.Length - stringPositionInternal);
                    stringPositionInternal = stringSelectPositionInternal;

                    stringPositionInternal = stringSelectPositionInternal;
                    caretPositionInternal = caretSelectPositionInternal;
                }
            }

            #if TMP_DEBUG_MODE
            Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
            #endif
        }

        /// <summary>
        /// Handling of DEL key
        /// </summary>
        private void ForwardSpace()
        {
            if (m_ReadOnly)
                return;

            if (hasSelection)
            {
                Delete();
                SendOnValueChangedAndUpdateLabel();
            }
            else
            {
                if (m_isRichTextEditingAllowed)
                {
                    if (stringPositionInternal < text.Length)
                    { 
                        m_Text = text.Remove(stringPositionInternal, 1);

                        SendOnValueChangedAndUpdateLabel();
                    }
                }
                else
                {
                    if (caretPositionInternal < m_TextComponent.textInfo.characterCount - 1)
                    {
                        stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(caretPositionInternal);
                        m_Text = text.Remove(stringPositionInternal, 1);

                        SendOnValueChangedAndUpdateLabel();
                    }
                }
            }

            #if TMP_DEBUG_MODE
            Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
            #endif
        }

        /// <summary>
        /// Handling of Backspace key
        /// </summary>
        private void Backspace()
        {
            if (m_ReadOnly)
                return;

            if (hasSelection)
            {
                Delete();
                SendOnValueChangedAndUpdateLabel();
            }
            else
            {
                if (m_isRichTextEditingAllowed)
                {
                    if (stringPositionInternal > 0)
                    {
                        m_Text = text.Remove(stringPositionInternal - 1, 1);
                        stringSelectPositionInternal = stringPositionInternal = stringPositionInternal - 1;

                        m_isLastKeyBackspace = true;

                        SendOnValueChangedAndUpdateLabel();
                    }
                }
                else
                {
                    if (caretPositionInternal > 0)
                    {
                        m_Text = text.Remove(GetStringIndexFromCaretPosition(caretPositionInternal - 1), 1);
                        caretSelectPositionInternal = caretPositionInternal = caretPositionInternal - 1;
                        stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(caretPositionInternal);
                    }

                    m_isLastKeyBackspace = true;

                    SendOnValueChangedAndUpdateLabel();
                }

            }

            #if TMP_DEBUG_MODE
            Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
            #endif
        }


        /// <summary>
        /// Append the specified text to the end of the current.
        /// </summary>
        protected virtual void Append(string input)
        {
            if (m_ReadOnly)
                return;

            if (!InPlaceEditing())
                return;

            for (int i = 0, imax = input.Length; i < imax; ++i)
            {
                char c = input[i];

                if (c >= ' ' || c == '\t' || c == '\r' || c == 10 || c == '\n')
                {
                    Append(c);
                }
            }
        }

        protected virtual void Append(char input)
        {
            if (m_ReadOnly)
                return;

            if (!InPlaceEditing())
                return;

            // If we have an input validator, validate the input first
            if (onValidateInput != null)
                input = onValidateInput(text, stringPositionInternal, input);
            else if (characterValidation == CharacterValidation.CustomValidator)
            {
                input = Validate(text, stringPositionInternal, input);

                if (input == 0) return;

                SendOnValueChanged();
                UpdateLabel();

                return;
            }
            else if (characterValidation != CharacterValidation.None)
                input = Validate(text, stringPositionInternal, input);



            // If the input is invalid, skip it
            if (input == 0)
                return;

            // Append the character and update the label
            Insert(input);
        }


        // Insert the character and update the label.
        private void Insert(char c)
        {
            if (m_ReadOnly)
                return;

            string replaceString = c.ToString();
            Delete();

            // Can't go past the character limit
            if (characterLimit > 0 && text.Length >= characterLimit)
                return;

            m_Text = text.Insert(m_StringPosition, replaceString);
            stringSelectPositionInternal = stringPositionInternal += replaceString.Length;

            SendOnValueChanged();

            #if TMP_DEBUG_MODE
            Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
            #endif
        }

        private void SendOnValueChangedAndUpdateLabel()
        {
            SendOnValueChanged();
            UpdateLabel();
        }

        private void SendOnValueChanged()
        {
            if (onValueChanged != null)
                onValueChanged.Invoke(text);
        }

        /// <summary>
        /// Submit the input field's text.
        /// </summary>

        protected void SendOnEndEdit()
        {
            if (onEndEdit != null)
                onEndEdit.Invoke(m_Text);
        }

        protected void SendOnSubmit()
        {
            if (onSubmit != null)
                onSubmit.Invoke(m_Text);
        }

        protected void SendOnFocus()
        {
            if (onSelect != null)
                onSelect.Invoke(m_Text);
        }

        protected void SendOnFocusLost()
        {
            if (onDeselect != null)
                onDeselect.Invoke(m_Text);
        }

        protected void SendOnTextSelection()
        {
            m_isSelected = true;

            if (onTextSelection != null)
                onTextSelection.Invoke(m_Text, stringPositionInternal, stringSelectPositionInternal);
        }

        protected void SendOnEndTextSelection()
        {
            if (!m_isSelected) return;

            if (onEndTextSelection != null)
                onEndTextSelection.Invoke(m_Text, stringPositionInternal, stringSelectPositionInternal);

            m_isSelected = false;
        }


        /// <summary>
        /// Update the visual text Text.
        /// </summary>

        protected void UpdateLabel()
        {
            if (m_TextComponent != null && m_TextComponent.font != null)
            {
                // TextGenerator.Populate invokes a callback that's called for anything
                // that needs to be updated when the data for that font has changed.
                // This makes all Text components that use that font update their vertices.
                // In turn, this makes the InputField that's associated with that Text component
                // update its label by calling this UpdateLabel method.
                // This is a recursive call we want to prevent, since it makes the InputField
                // update based on font data that didn't yet finish executing, or alternatively
                // hang on infinite recursion, depending on whether the cached value is cached
                // before or after the calculation.
                //
                // This callback also occurs when assigning text to our Text component, i.e.,
                // m_TextComponent.text = processed;

                //m_PreventFontCallback = true;

                string fullText;
                if (Input.compositionString.Length > 0)
                    fullText = text.Substring(0, m_StringPosition) + Input.compositionString + text.Substring(m_StringPosition);
                else
                    fullText = text;

                string processed;
                if (inputType == InputType.Password)
                    processed = new string(asteriskChar, fullText.Length);
                else
                    processed = fullText;

                bool isEmpty = string.IsNullOrEmpty(fullText);

                if (m_Placeholder != null)
                    m_Placeholder.enabled = isEmpty; // && !isFocused;

                // If not currently editing the text, set the visible range to the whole text.
                // The UpdateLabel method will then truncate it to the part that fits inside the Text area.
                // We can't do this when text is being edited since it would discard the current scroll,
                // which is defined by means of the m_DrawStart and m_DrawEnd indices.

                if (!isEmpty)
                {
                //    // Determine what will actually fit into the given line
                //    Vector2 extents = m_TextComponent.rectTransform.rect.size;

                //    var settings = m_TextComponent.GetGenerationSettings(extents);
                //    settings.generateOutOfBounds = true;

                //    cachedInputTextGenerator.Populate(processed, settings);

                //    SetDrawRangeToContainCaretPosition(stringSelectPositionInternal - 1);

                //    processed = processed.Substring(m_DrawStart, Mathf.Min(m_DrawEnd, processed.Length) - m_DrawStart);

                    SetCaretVisible();
                }

                m_TextComponent.text = processed + "\u200B"; // Extra space is added for Caret tracking.
                MarkGeometryAsDirty();

                // Scrollbar should be updated.
                m_IsScrollbarUpdateRequired = true;

                //m_PreventFontCallback = false;
            }
        }

        //private bool IsSelectionVisible()
        //{
        //    if (m_DrawStart > stringPositionInternal || m_DrawStart > stringSelectPositionInternal)
        //        return false;

        //    if (m_DrawEnd < stringPositionInternal || m_DrawEnd < stringSelectPositionInternal)
        //        return false;

        //    return true;
        //}

        void UpdateScrollbar()
        {
            // Update Scrollbar
            if (m_VerticalScrollbar)
            {
                float size = m_TextViewport.rect.height / m_TextComponent.preferredHeight;

                m_IsUpdatingScrollbarValues = true;

                m_VerticalScrollbar.size = size;

                m_ScrollPosition = m_VerticalScrollbar.value = m_TextComponent.rectTransform.anchoredPosition.y / (m_TextComponent.preferredHeight - m_TextViewport.rect.height);

                //m_VerticalScrollbar.numberOfSteps = (int)(m_TextComponent.textInfo.lineCount / 0.25f); // Replace by scroll sensitivity.

                //Debug.Log("Updating Scrollbar... Value: " + m_VerticalScrollbar.value);
            }
        }


        /// <summary>
        /// Function to update the vertical position of the text container when OnValueChanged event is received from the Scrollbar.
        /// </summary>
        /// <param name="value"></param>
        void OnScrollbarValueChange(float value)
        {
            if (m_IsUpdatingScrollbarValues) { m_IsUpdatingScrollbarValues = false; return; }

            if (value < 0 || value > 1) return;

            AdjustTextPositionRelativeToViewport(value);

            m_ScrollPosition = value;

            //Debug.Log("Scrollbar value is: " + value + "  Transform POS: " + m_TextComponent.rectTransform.anchoredPosition);
        }

        /// <summary>
        /// Adjusts the relative position of the body of the text relative to the viewport.
        /// </summary>
        /// <param name="relativePosition"></param>
        void AdjustTextPositionRelativeToViewport (float relativePosition)
        {
            //Debug.Log("- Adjusting vertical text position to " + relativePosition);

            TMP_TextInfo textInfo = m_TextComponent.textInfo;

            // Check to make sure we have valid data and lines to query.
            if (textInfo == null || textInfo.lineInfo == null || textInfo.lineCount == 0 || textInfo.lineCount > textInfo.lineInfo.Length) return;

            //m_TextComponent.rectTransform.anchoredPosition = new Vector2(m_TextComponent.rectTransform.anchoredPosition.x, (textHeight - viewportHeight) * relativePosition);
            m_TextComponent.rectTransform.anchoredPosition = new Vector2(m_TextComponent.rectTransform.anchoredPosition.x, (m_TextComponent.preferredHeight - m_TextViewport.rect.height) * relativePosition);

            AssignPositioningIfNeeded();

            //Debug.Log("Text height: " + m_TextComponent.preferredHeight + "  Viewport height: " + m_TextViewport.rect.height + "  Adjusted RectTransform anchordedPosition:" + m_TextComponent.rectTransform.anchoredPosition + "  Text Bounds: " + m_TextComponent.bounds.ToString("f3"));
        }


        private int GetCaretPositionFromStringIndex(int stringIndex)
        {
            int count = m_TextComponent.textInfo.characterCount;

            for (int i = 0; i < count; i++)
            {
                if (m_TextComponent.textInfo.characterInfo[i].index >= stringIndex)
                    return i;
            }

            return count;
        }

        private int GetStringIndexFromCaretPosition(int caretPosition)
        {
            // Clamp values between 0 and character count.
            ClampCaretPos(ref caretPosition);

            return m_TextComponent.textInfo.characterInfo[caretPosition].index;
        }


        public void ForceLabelUpdate()
        {
            UpdateLabel();
        }

        private void MarkGeometryAsDirty()
        {
#if UNITY_EDITOR
    #if UNITY_2018_3_OR_NEWER
            if (!Application.isPlaying || UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this))
                return;
    #else
            if (!Application.isPlaying || UnityEditor.PrefabUtility.GetPrefabObject(gameObject) != null)
                return;
    #endif
#endif

            CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
        }

        public virtual void Rebuild(CanvasUpdate update)
        {
            switch (update)
            {
                case CanvasUpdate.LatePreRender:
                    UpdateGeometry();
                    break;
            }
        }

        public virtual void LayoutComplete()
        { }

        public virtual void GraphicUpdateComplete()
        { }

        private void UpdateGeometry()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            // No need to draw a cursor on mobile as its handled by the devices keyboard.
            if (!shouldHideMobileInput)
                return;

            //if (m_CachedInputRenderer == null && m_TextComponent != null)
            //{
            //    GameObject go = new GameObject(transform.name + " Input Caret");

            //    // Add MaskableGraphic Component
            //    go.AddComponent<TMP_SelectionCaret>();

            //    go.hideFlags = HideFlags.DontSave;
            //    go.transform.SetParent(m_TextComponent.transform.parent);
            //    go.transform.SetAsFirstSibling();
            //    go.layer = gameObject.layer;

            //    caretRectTrans = go.GetComponent<RectTransform>(); // go.AddComponent<RectTransform>();
            //    m_CachedInputRenderer = go.GetComponent<CanvasRenderer>(); // go.AddComponent<CanvasRenderer>();
            //    m_CachedInputRenderer.SetMaterial(Graphic.defaultGraphicMaterial, Texture2D.whiteTexture);

            //    // Needed as if any layout is present we want the caret to always be the same as the text area.
            //    go.AddComponent<LayoutElement>().ignoreLayout = true;

            //    AssignPositioningIfNeeded();
            //}

            if (m_CachedInputRenderer == null)
                return;

            OnFillVBO(mesh);

            m_CachedInputRenderer.SetMesh(mesh);
        }


        /// <summary>
        /// Method to keep the Caret RectTransform properties in sync with the text object's RectTransform
        /// </summary>
        private void AssignPositioningIfNeeded()
        {
            if (m_TextComponent != null && caretRectTrans != null &&
                (caretRectTrans.localPosition != m_TextComponent.rectTransform.localPosition ||
                 caretRectTrans.localRotation != m_TextComponent.rectTransform.localRotation ||
                 caretRectTrans.localScale != m_TextComponent.rectTransform.localScale ||
                 caretRectTrans.anchorMin != m_TextComponent.rectTransform.anchorMin ||
                 caretRectTrans.anchorMax != m_TextComponent.rectTransform.anchorMax ||
                 caretRectTrans.anchoredPosition != m_TextComponent.rectTransform.anchoredPosition ||
                 caretRectTrans.sizeDelta != m_TextComponent.rectTransform.sizeDelta ||
                 caretRectTrans.pivot != m_TextComponent.rectTransform.pivot))
            {
                caretRectTrans.localPosition = m_TextComponent.rectTransform.localPosition;
                caretRectTrans.localRotation = m_TextComponent.rectTransform.localRotation;
                caretRectTrans.localScale = m_TextComponent.rectTransform.localScale;
                caretRectTrans.anchorMin = m_TextComponent.rectTransform.anchorMin;
                caretRectTrans.anchorMax = m_TextComponent.rectTransform.anchorMax;
                caretRectTrans.anchoredPosition = m_TextComponent.rectTransform.anchoredPosition;
                caretRectTrans.sizeDelta = m_TextComponent.rectTransform.sizeDelta;
                caretRectTrans.pivot = m_TextComponent.rectTransform.pivot;

                // Get updated world corners of viewport.
                //m_TextViewport.GetLocalCorners(m_ViewportCorners);
            }
        }


        private void OnFillVBO(Mesh vbo)
        {
            using (var helper = new VertexHelper())
            {
                if (!isFocused && m_ResetOnDeActivation)
                {
                    helper.FillMesh(vbo);
                    return;
                }

                if (isStringPositionDirty)
                {
                    stringPositionInternal = GetStringIndexFromCaretPosition(m_CaretPosition);
                    stringSelectPositionInternal = GetStringIndexFromCaretPosition(m_CaretSelectPosition);
                    isStringPositionDirty = false;
                }

                if (!hasSelection)
                {
                    GenerateCaret(helper, Vector2.zero);
                    SendOnEndTextSelection();
                }
                else
                {
                    GenerateHightlight(helper, Vector2.zero);
                    SendOnTextSelection();
                }

                helper.FillMesh(vbo);
            }
        }


        private void GenerateCaret(VertexHelper vbo, Vector2 roundingOffset)
        {
            if (!m_CaretVisible)
                return;

            if (m_CursorVerts == null)
            {
                CreateCursorVerts();
            }

            float width = m_CaretWidth;

            // Optimize to only update the caret position when needed.
            //
            //

            int characterCount = m_TextComponent.textInfo.characterCount;
            Vector2 startPosition = Vector2.zero;
            float height = 0;
            TMP_CharacterInfo currentCharacter;

            // Get the position of the Caret based on position in the string.
            caretPositionInternal = GetCaretPositionFromStringIndex(stringPositionInternal);

            if (caretPositionInternal == 0)
            {
                currentCharacter = m_TextComponent.textInfo.characterInfo[0];
                startPosition = new Vector2(currentCharacter.origin, currentCharacter.descender);
                height = currentCharacter.ascender - currentCharacter.descender;
            }
            else if (caretPositionInternal < characterCount)
            {
                currentCharacter = m_TextComponent.textInfo.characterInfo[caretPositionInternal];
                startPosition = new Vector2(currentCharacter.origin, currentCharacter.descender);
                height = currentCharacter.ascender - currentCharacter.descender;
            }
            else
            {
                currentCharacter = m_TextComponent.textInfo.characterInfo[characterCount - 1];
                startPosition = new Vector2(currentCharacter.xAdvance, currentCharacter.descender);
                height = currentCharacter.ascender - currentCharacter.descender;
            }

            //Debug.Log("String Char [" + m_Text[m_StringPosition] + "] at Index:" + m_StringPosition + "  Caret Char [" + currentCharacter.character + "] at Index:" + caretPositionInternal);

            // Adjust the position of the RectTransform based on the caret position in the viewport (only if we have focus).
            if (isFocused && startPosition != m_LastPosition || m_forceRectTransformAdjustment)
                AdjustRectTransformRelativeToViewport(startPosition, height, currentCharacter.isVisible);

            m_LastPosition = startPosition;

            // Clamp Caret height
            float top = startPosition.y + height;
            float bottom = top - height; // Mathf.Min(height, m_TextComponent.rectTransform.rect.height);

            m_CursorVerts[0].position = new Vector3(startPosition.x, bottom, 0.0f);
            m_CursorVerts[1].position = new Vector3(startPosition.x, top, 0.0f);
            m_CursorVerts[2].position = new Vector3(startPosition.x + width, top, 0.0f);
            m_CursorVerts[3].position = new Vector3(startPosition.x + width, bottom, 0.0f);

            // Set Vertex Color for the caret color.
            m_CursorVerts[0].color = caretColor;
            m_CursorVerts[1].color = caretColor;
            m_CursorVerts[2].color = caretColor;
            m_CursorVerts[3].color = caretColor;

            vbo.AddUIVertexQuad(m_CursorVerts);

            int screenHeight = Screen.height;
            // Removed multiple display support until it supports none native resolutions(case 741751)
            //int displayIndex = m_TextComponent.canvas.targetDisplay;
            //if (Screen.fullScreen && displayIndex < Display.displays.Length)
            //    screenHeight = Display.displays[displayIndex].renderingHeight;

            startPosition.y = screenHeight - startPosition.y;
            Input.compositionCursorPos = startPosition;

            //Debug.Log("Text Position: " + m_TextComponent.rectTransform.position + "  Local Position: " + m_TextComponent.rectTransform.localPosition);
        }


        private void CreateCursorVerts()
        {
            m_CursorVerts = new UIVertex[4];

            for (int i = 0; i < m_CursorVerts.Length; i++)
            {
                m_CursorVerts[i] = UIVertex.simpleVert;
                m_CursorVerts[i].uv0 = Vector2.zero;
            }
        }


        private void GenerateHightlight(VertexHelper vbo, Vector2 roundingOffset)
        {
            TMP_TextInfo textInfo = m_TextComponent.textInfo;

            caretPositionInternal = m_CaretPosition = GetCaretPositionFromStringIndex(stringPositionInternal);
            caretSelectPositionInternal = m_CaretSelectPosition = GetCaretPositionFromStringIndex(stringSelectPositionInternal);

            //Debug.Log("StringPosition:" + caretPositionInternal + "  StringSelectPosition:" + caretSelectPositionInternal);

            // Adjust text RectTranform position to make sure it is visible in viewport.
            Vector2 caretPosition;
            float height = 0;
            if (caretSelectPositionInternal < textInfo.characterCount)
            {
                caretPosition = new Vector2(textInfo.characterInfo[caretSelectPositionInternal].origin, textInfo.characterInfo[caretSelectPositionInternal].descender);
                height = textInfo.characterInfo[caretSelectPositionInternal].ascender - textInfo.characterInfo[caretSelectPositionInternal].descender;
            }
            else
            {
                caretPosition = new Vector2(textInfo.characterInfo[caretSelectPositionInternal - 1].xAdvance, textInfo.characterInfo[caretSelectPositionInternal - 1].descender);
                height = textInfo.characterInfo[caretSelectPositionInternal - 1].ascender - textInfo.characterInfo[caretSelectPositionInternal - 1].descender;
            }

            // TODO: Don't adjust the position of the RectTransform if Reset On Deactivation is disabled
            // and we just selected the Input Field again.
            AdjustRectTransformRelativeToViewport(caretPosition, height, true);

            int startChar = Mathf.Max(0, caretPositionInternal);
            int endChar = Mathf.Max(0, caretSelectPositionInternal);

            // Ensure pos is always less then selPos to make the code simpler
            if (startChar > endChar)
            {
                int temp = startChar;
                startChar = endChar;
                endChar = temp;
            }

            endChar -= 1;

            //Debug.Log("Updating Highlight... Caret Position: " + startChar + " Caret Select POS: " + endChar);


            int currentLineIndex = textInfo.characterInfo[startChar].lineNumber;
            int nextLineStartIdx = textInfo.lineInfo[currentLineIndex].lastCharacterIndex;

            UIVertex vert = UIVertex.simpleVert;
            vert.uv0 = Vector2.zero;
            vert.color = selectionColor;

            int currentChar = startChar;
            while (currentChar <= endChar && currentChar < textInfo.characterCount)
            {
                if (currentChar == nextLineStartIdx || currentChar == endChar)
                {
                    TMP_CharacterInfo startCharInfo = textInfo.characterInfo[startChar];
                    TMP_CharacterInfo endCharInfo = textInfo.characterInfo[currentChar];

                    // Extra check to handle Carriage Return
                    if (currentChar > 0 && endCharInfo.character == 10 && textInfo.characterInfo[currentChar - 1].character == 13)
                        endCharInfo = textInfo.characterInfo[currentChar - 1];

                    Vector2 startPosition = new Vector2(startCharInfo.origin, textInfo.lineInfo[currentLineIndex].ascender);
                    Vector2 endPosition = new Vector2(endCharInfo.xAdvance, textInfo.lineInfo[currentLineIndex].descender);

                    var startIndex = vbo.currentVertCount;
                    vert.position = new Vector3(startPosition.x, endPosition.y, 0.0f);
                    vbo.AddVert(vert);

                    vert.position = new Vector3(endPosition.x, endPosition.y, 0.0f);
                    vbo.AddVert(vert);

                    vert.position = new Vector3(endPosition.x, startPosition.y, 0.0f);
                    vbo.AddVert(vert);

                    vert.position = new Vector3(startPosition.x, startPosition.y, 0.0f);
                    vbo.AddVert(vert);

                    vbo.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
                    vbo.AddTriangle(startIndex + 2, startIndex + 3, startIndex + 0);

                    startChar = currentChar + 1;
                    currentLineIndex++;

                    if (currentLineIndex < textInfo.lineCount)
                        nextLineStartIdx = textInfo.lineInfo[currentLineIndex].lastCharacterIndex;
                }
                currentChar++;
            }

            // Scrollbar should be updated.
            m_IsScrollbarUpdateRequired = true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="height"></param>
        /// <param name="isCharVisible"></param>
        private void AdjustRectTransformRelativeToViewport(Vector2 startPosition, float height, bool isCharVisible)
        {
            //Debug.Log("Adjusting transform position relative to viewport.");

            float viewportMin = m_TextViewport.rect.xMin;
            float viewportMax = m_TextViewport.rect.xMax;

            //Debug.Log("Viewport Rect: " + viewportMax + "  Start Position: " + startPosition);
            // Adjust the position of the RectTransform based on the caret position in the viewport.
            float rightOffset = viewportMax - (m_TextComponent.rectTransform.anchoredPosition.x + startPosition.x + m_TextComponent.margin.z + m_CaretWidth);
            if (rightOffset < 0f)
            {
                if (!multiLine || (multiLine && isCharVisible))
                {
                    //Debug.Log("Shifting text to the right by " + rightOffset.ToString("f3"));
                    m_TextComponent.rectTransform.anchoredPosition += new Vector2(rightOffset, 0);

                    AssignPositioningIfNeeded();
                }
            }

            float leftOffset = (m_TextComponent.rectTransform.anchoredPosition.x + startPosition.x - m_TextComponent.margin.x) - viewportMin;
            if (leftOffset < 0f)
            {
                //Debug.Log("Shifting text to the left by " + leftOffset.ToString("f3"));
                m_TextComponent.rectTransform.anchoredPosition += new Vector2(-leftOffset, 0);
                AssignPositioningIfNeeded();
            }


            // Adjust text area up or down if not in single line mode.
            if (m_LineType != LineType.SingleLine)
            {
                float topOffset = m_TextViewport.rect.yMax - (m_TextComponent.rectTransform.anchoredPosition.y + startPosition.y + height);
                if (topOffset < -0.0001f)
                {
                    m_TextComponent.rectTransform.anchoredPosition += new Vector2(0, topOffset);
                    AssignPositioningIfNeeded();
                    m_IsScrollbarUpdateRequired = true;
                }

                float bottomOffset = (m_TextComponent.rectTransform.anchoredPosition.y + startPosition.y) - m_TextViewport.rect.yMin;
                if (bottomOffset < 0f)
                {
                    m_TextComponent.rectTransform.anchoredPosition -= new Vector2(0, bottomOffset);
                    AssignPositioningIfNeeded();
                    m_IsScrollbarUpdateRequired = true;
                }
            }

            // Special handling of backspace
            if (m_isLastKeyBackspace)
            {
                float firstCharPosition = m_TextComponent.rectTransform.anchoredPosition.x + m_TextComponent.textInfo.characterInfo[0].origin - m_TextComponent.margin.x;
                float lastCharPosition = m_TextComponent.rectTransform.anchoredPosition.x + m_TextComponent.textInfo.characterInfo[m_TextComponent.textInfo.characterCount - 1].origin + m_TextComponent.margin.z;

                // Check if caret is at the left most position of the viewport
                if (m_TextComponent.rectTransform.anchoredPosition.x + startPosition.x <= viewportMin + 0.0001f)
                {
                    if (firstCharPosition < viewportMin)
                    {
                        float offset = Mathf.Min((viewportMax - viewportMin) / 2, viewportMin - firstCharPosition);
                        m_TextComponent.rectTransform.anchoredPosition += new Vector2(offset, 0);
                        AssignPositioningIfNeeded();
                    }
                }
                else if (lastCharPosition < viewportMax && firstCharPosition < viewportMin)
                {
                    float offset = Mathf.Min(viewportMax - lastCharPosition, viewportMin - firstCharPosition);

                    m_TextComponent.rectTransform.anchoredPosition += new Vector2(offset, 0);
                    AssignPositioningIfNeeded();
                }

                m_isLastKeyBackspace = false;
            }

            m_forceRectTransformAdjustment = false;
        }

        /// <summary>
        /// Validate the specified input.
        /// </summary>
        protected char Validate(string text, int pos, char ch)
        {
            // Validation is disabled
            if (characterValidation == CharacterValidation.None || !enabled)
                return ch;

            if (characterValidation == CharacterValidation.Integer || characterValidation == CharacterValidation.Decimal)
            {
                // Integer and decimal
                bool cursorBeforeDash = (pos == 0 && text.Length > 0 && text[0] == '-');
                bool selectionAtStart = stringPositionInternal == 0 || stringSelectPositionInternal == 0;
                if (!cursorBeforeDash)
                {
                    if (ch >= '0' && ch <= '9') return ch;
                    if (ch == '-' && (pos == 0 || selectionAtStart)) return ch;
                    if (ch == '.' && characterValidation == CharacterValidation.Decimal && !text.Contains(".")) return ch;
                }
            }
            else if (characterValidation == CharacterValidation.Digit)
            {
                if (ch >= '0' && ch <= '9') return ch;
            }
            else if (characterValidation == CharacterValidation.Alphanumeric)
            {
                // All alphanumeric characters
                if (ch >= 'A' && ch <= 'Z') return ch;
                if (ch >= 'a' && ch <= 'z') return ch;
                if (ch >= '0' && ch <= '9') return ch;
            }
            else if (characterValidation == CharacterValidation.Name)
            {
                char lastChar = (text.Length > 0) ? text[Mathf.Clamp(pos, 0, text.Length - 1)] : ' ';
                char nextChar = (text.Length > 0) ? text[Mathf.Clamp(pos + 1, 0, text.Length - 1)] : '\n';

                if (char.IsLetter(ch))
                {
                    // Space followed by a letter -- make sure it's capitalized
                    if (char.IsLower(ch) && lastChar == ' ')
                        return char.ToUpper(ch);

                    // Uppercase letters are only allowed after spaces (and apostrophes)
                    if (char.IsUpper(ch) && lastChar != ' ' && lastChar != '\'')
                        return char.ToLower(ch);

                    // If character was already in correct case, return it as-is.
                    // Also, letters that are neither upper nor lower case are always allowed.
                    return ch;
                }
                else if (ch == '\'')
                {
                    // Don't allow more than one apostrophe
                    if (lastChar != ' ' && lastChar != '\'' && nextChar != '\'' && !text.Contains("'"))
                        return ch;
                }
                else if (ch == ' ')
                {
                    // Don't allow more than one space in a row
                    if (lastChar != ' ' && lastChar != '\'' && nextChar != ' ' && nextChar != '\'')
                        return ch;
                }
            }
            else if (characterValidation == CharacterValidation.EmailAddress)
            {
                // From StackOverflow about allowed characters in email addresses:
                // Uppercase and lowercase English letters (a-z, A-Z)
                // Digits 0 to 9
                // Characters ! # $ % & ' * + - / = ? ^ _ ` { | } ~
                // Character . (dot, period, full stop) provided that it is not the first or last character,
                // and provided also that it does not appear two or more times consecutively.

                if (ch >= 'A' && ch <= 'Z') return ch;
                if (ch >= 'a' && ch <= 'z') return ch;
                if (ch >= '0' && ch <= '9') return ch;
                if (ch == '@' && text.IndexOf('@') == -1) return ch;
                if (kEmailSpecialCharacters.IndexOf(ch) != -1) return ch;
                if (ch == '.')
                {
                    char lastChar = (text.Length > 0) ? text[Mathf.Clamp(pos, 0, text.Length - 1)] : ' ';
                    char nextChar = (text.Length > 0) ? text[Mathf.Clamp(pos + 1, 0, text.Length - 1)] : '\n';
                    if (lastChar != '.' && nextChar != '.')
                        return ch;
                }
            }
            else if (characterValidation == CharacterValidation.Regex)
            {
                // Regex expression
                if (Regex.IsMatch(ch.ToString(), m_RegexValue))
                {
                    return ch;
                }
            }
            else if (characterValidation == CharacterValidation.CustomValidator)
            {
                if (m_InputValidator != null)
                {
                    char c = m_InputValidator.Validate(ref text, ref pos, ch);
                    m_Text = text;
                    stringSelectPositionInternal = stringPositionInternal = pos;
                    return c;
                }
            }
            return (char)0;
        }

        public void ActivateInputField()
        {
            if (m_TextComponent == null || m_TextComponent.font == null || !IsActive() || !IsInteractable())
                return;

            if (isFocused)
            {
                if (m_Keyboard != null && !m_Keyboard.active)
                {
                    m_Keyboard.active = true;
                    m_Keyboard.text = m_Text;
                }
            }

            m_ShouldActivateNextUpdate = true;
        }

        private void ActivateInputFieldInternal()
        {
            if (EventSystem.current == null)
                return;

            if (EventSystem.current.currentSelectedGameObject != gameObject)
                EventSystem.current.SetSelectedGameObject(gameObject);

            if (TouchScreenKeyboard.isSupported)
            {
                if (Input.touchSupported)
                {
                    TouchScreenKeyboard.hideInput = shouldHideMobileInput;
                }

                m_Keyboard = (inputType == InputType.Password) ?
                    TouchScreenKeyboard.Open(m_Text, keyboardType, false, multiLine, true) :
                    TouchScreenKeyboard.Open(m_Text, keyboardType, inputType == InputType.AutoCorrect, multiLine);

                // Mimics OnFocus but as mobile doesn't properly support select all
                // just set it to the end of the text (where it would move when typing starts)
                MoveTextEnd(false);
            }
            else
            {
                Input.imeCompositionMode = IMECompositionMode.On;
                OnFocus();
            }

            //m_StringPosition = m_StringSelectPosition = 0;
            //m_CaretPosition = m_CaretSelectPosition = 0;

            m_AllowInput = true;
            m_OriginalText = text;
            m_WasCanceled = false;
            SetCaretVisible();
            UpdateLabel();
        }

        public override void OnSelect(BaseEventData eventData)
        {
            //Debug.Log("OnSelect()");

            base.OnSelect(eventData);
            SendOnFocus();

            ActivateInputField();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            ActivateInputField();
        }

        public void OnControlClick()
        {
            //Debug.Log("Input Field control click...");
        }

        public void DeactivateInputField()
        {
            //Debug.Log("Deactivate Input Field...");

            // Not activated do nothing.
            if (!m_AllowInput)
                return;

            m_HasDoneFocusTransition = false;
            m_AllowInput = false;

            if (m_Placeholder != null)
                m_Placeholder.enabled = string.IsNullOrEmpty(m_Text);

            if (m_TextComponent != null && IsInteractable())
            {
                if (m_WasCanceled && m_RestoreOriginalTextOnEscape)
                    text = m_OriginalText;

                if (m_Keyboard != null)
                {
                    m_Keyboard.active = false;
                    m_Keyboard = null;
                }

                if (m_ResetOnDeActivation)
                {
                    m_StringPosition = m_StringSelectPosition = 0;
                    m_CaretPosition = m_CaretSelectPosition = 0;
                    m_TextComponent.rectTransform.localPosition = m_DefaultTransformPosition;

                    if (caretRectTrans != null)
                        caretRectTrans.localPosition = Vector3.zero;

                    //m_ForceDeactivation = false;
                }

                SendOnEndEdit();
                SendOnEndTextSelection();

                Input.imeCompositionMode = IMECompositionMode.Auto;
            }

            MarkGeometryAsDirty();

            // Scrollbar should be updated.
            m_IsScrollbarUpdateRequired = true;
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            //return;

            DeactivateInputField();

            base.OnDeselect(eventData);
            SendOnFocusLost();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            //Debug.Log("OnSubmit()");

            if (!IsActive() || !IsInteractable())
                return;

            if (!isFocused)
                m_ShouldActivateNextUpdate = true;

            SendOnSubmit();
        }

        //public virtual void OnLostFocus(BaseEventData eventData)
        //{
        //    if (!IsActive() || !IsInteractable())
        //        return;
        //}

        private void EnforceContentType()
        {
            switch (contentType)
            {
                case ContentType.Standard:
                    {
                        // Don't enforce line type for this content type.
                        m_InputType = InputType.Standard;
                        m_KeyboardType = TouchScreenKeyboardType.Default;
                        m_CharacterValidation = CharacterValidation.None;
                        return;
                    }
                case ContentType.Autocorrected:
                    {
                        // Don't enforce line type for this content type.
                        m_InputType = InputType.AutoCorrect;
                        m_KeyboardType = TouchScreenKeyboardType.Default;
                        m_CharacterValidation = CharacterValidation.None;
                        return;
                    }
                case ContentType.IntegerNumber:
                    {
                        m_LineType = LineType.SingleLine;
                        m_TextComponent.enableWordWrapping = false;
                        m_InputType = InputType.Standard;
                        m_KeyboardType = TouchScreenKeyboardType.NumberPad;
                        m_CharacterValidation = CharacterValidation.Integer;
                        return;
                    }
                case ContentType.DecimalNumber:
                    {
                        m_LineType = LineType.SingleLine;
                        m_TextComponent.enableWordWrapping = false;
                        m_InputType = InputType.Standard;
                        m_KeyboardType = TouchScreenKeyboardType.NumbersAndPunctuation;
                        m_CharacterValidation = CharacterValidation.Decimal;
                        return;
                    }
                case ContentType.Alphanumeric:
                    {
                        m_LineType = LineType.SingleLine;
                        m_TextComponent.enableWordWrapping = false;
                        m_InputType = InputType.Standard;
                        m_KeyboardType = TouchScreenKeyboardType.ASCIICapable;
                        m_CharacterValidation = CharacterValidation.Alphanumeric;
                        return;
                    }
                case ContentType.Name:
                    {
                        m_LineType = LineType.SingleLine;
                        m_TextComponent.enableWordWrapping = false;
                        m_InputType = InputType.Standard;
                        m_KeyboardType = TouchScreenKeyboardType.Default;
                        m_CharacterValidation = CharacterValidation.Name;
                        return;
                    }
                case ContentType.EmailAddress:
                    {
                        m_LineType = LineType.SingleLine;
                        m_TextComponent.enableWordWrapping = false;
                        m_InputType = InputType.Standard;
                        m_KeyboardType = TouchScreenKeyboardType.EmailAddress;
                        m_CharacterValidation = CharacterValidation.EmailAddress;
                        return;
                    }
                case ContentType.Password:
                    {
                        m_LineType = LineType.SingleLine;
                        m_TextComponent.enableWordWrapping = false;
                        m_InputType = InputType.Password;
                        m_KeyboardType = TouchScreenKeyboardType.Default;
                        m_CharacterValidation = CharacterValidation.None;
                        return;
                    }
                case ContentType.Pin:
                    {
                        m_LineType = LineType.SingleLine;
                        m_TextComponent.enableWordWrapping = false;
                        m_InputType = InputType.Password;
                        m_KeyboardType = TouchScreenKeyboardType.NumberPad;
                        m_CharacterValidation = CharacterValidation.Digit;
                        return;
                    }
                default:
                    {
                        // Includes Custom type. Nothing should be enforced.
                        return;
                    }
            }
        }


        void SetTextComponentWrapMode()
        {
            if (m_TextComponent == null)
                return;

            if (m_LineType == LineType.SingleLine)
                m_TextComponent.enableWordWrapping = false;
            else
                m_TextComponent.enableWordWrapping = true;
        }

        // Control Rich Text option on the text component.
        void SetTextComponentRichTextMode()
        {
            if (m_TextComponent == null)
                return;

            m_TextComponent.richText = m_RichText;
        }

        void SetToCustomIfContentTypeIsNot(params ContentType[] allowedContentTypes)
        {
            if (contentType == ContentType.Custom)
                return;

            for (int i = 0; i < allowedContentTypes.Length; i++)
                if (contentType == allowedContentTypes[i])
                    return;

            contentType = ContentType.Custom;
        }

        void SetToCustom()
        {
            if (contentType == ContentType.Custom)
                return;

            contentType = ContentType.Custom;
        }

        void SetToCustom(CharacterValidation characterValidation)
        {
            if (contentType == ContentType.Custom)
            {
                characterValidation = CharacterValidation.CustomValidator;
                return;
            }

            contentType = ContentType.Custom;
            characterValidation = CharacterValidation.CustomValidator;
        }


        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (m_HasDoneFocusTransition)
                state = SelectionState.Highlighted;
            else if (state == SelectionState.Pressed)
                m_HasDoneFocusTransition = true;

            base.DoStateTransition(state, instant);
        }

        /// <summary>
        /// Function to conveniently set the point size of both Placeholder and Input Field text object.
        /// </summary>
        /// <param name="pointSize"></param>
        public void SetGlobalPointSize(float pointSize)
        {
            TMP_Text placeholderTextComponent = m_Placeholder as TMP_Text;

            if (placeholderTextComponent != null) placeholderTextComponent.fontSize = pointSize;
            textComponent.fontSize = pointSize;
        }

        /// <summary>
        /// Function to conveniently set the Font Asset of both Placeholder and Input Field text object.
        /// </summary>
        /// <param name="fontAsset"></param>
        public void SetGlobalFontAsset(TMP_FontAsset fontAsset)
        {
            TMP_Text placeholderTextComponent = m_Placeholder as TMP_Text;

            if (placeholderTextComponent != null) placeholderTextComponent.font = fontAsset;
            textComponent.font = fontAsset;

        }

    }



    static class SetPropertyUtility
    {
        public static bool SetColor(ref Color currentValue, Color newValue)
        {
            if (currentValue.r == newValue.r && currentValue.g == newValue.g && currentValue.b == newValue.b && currentValue.a == newValue.a)
                return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetEquatableStruct<T>(ref T currentValue, T newValue) where T : IEquatable<T>
        {
            if (currentValue.Equals(newValue))
                return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
        {
            if (currentValue.Equals(newValue))
                return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return false;

            currentValue = newValue;
            return true;
        }
    }
}