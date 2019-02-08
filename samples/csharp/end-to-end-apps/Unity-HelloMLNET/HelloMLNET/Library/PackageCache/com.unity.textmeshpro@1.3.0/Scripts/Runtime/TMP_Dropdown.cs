using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI.CoroutineTween;

namespace TMPro
{
    [AddComponentMenu("UI/TMP Dropdown", 35)]
    [RequireComponent(typeof(RectTransform))]
    public class TMP_Dropdown : Selectable, IPointerClickHandler, ISubmitHandler, ICancelHandler
    {
        protected internal class DropdownItem : MonoBehaviour, IPointerEnterHandler, ICancelHandler
        {
            [SerializeField]
            private TMP_Text m_Text;
            [SerializeField]
            private Image m_Image;
            [SerializeField]
            private RectTransform m_RectTransform;
            [SerializeField]
            private Toggle m_Toggle;

            public TMP_Text text { get { return m_Text; } set { m_Text = value; } }
            public Image image { get { return m_Image; } set { m_Image = value; } }
            public RectTransform rectTransform { get { return m_RectTransform; } set { m_RectTransform = value; } }
            public Toggle toggle { get { return m_Toggle; } set { m_Toggle = value; } }

            public virtual void OnPointerEnter(PointerEventData eventData)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }

            public virtual void OnCancel(BaseEventData eventData)
            {
                TMP_Dropdown dropdown = GetComponentInParent<TMP_Dropdown>();
                if (dropdown)
                    dropdown.Hide();
            }
        }

        [Serializable]
        public class OptionData
        {
            [SerializeField]
            private string m_Text;
            [SerializeField]
            private Sprite m_Image;

            public string text { get { return m_Text; } set { m_Text = value; } }
            public Sprite image { get { return m_Image; } set { m_Image = value; } }

            public OptionData()
            {
            }

            public OptionData(string text)
            {
                this.text = text;
            }

            public OptionData(Sprite image)
            {
                this.image = image;
            }

            public OptionData(string text, Sprite image)
            {
                this.text = text;
                this.image = image;
            }
        }

        [Serializable]
        public class OptionDataList
        {
            [SerializeField]
            private List<OptionData> m_Options;
            public List<OptionData> options { get { return m_Options; } set { m_Options = value; } }


            public OptionDataList()
            {
                options = new List<OptionData>();
            }
        }

        [Serializable]
        public class DropdownEvent : UnityEvent<int> { }

        // Template used to create the dropdown.
        [SerializeField]
        private RectTransform m_Template;
        public RectTransform template { get { return m_Template; } set { m_Template = value; RefreshShownValue(); } }

        // Text to be used as a caption for the current value. It's not required, but it's kept here for convenience.
        [SerializeField]
        private TMP_Text m_CaptionText;
        public TMP_Text captionText { get { return m_CaptionText; } set { m_CaptionText = value; RefreshShownValue(); } }

        [SerializeField]
        private Image m_CaptionImage;
        public Image captionImage { get { return m_CaptionImage; } set { m_CaptionImage = value; RefreshShownValue(); } }

        [Space]

        [SerializeField]
        private TMP_Text m_ItemText;
        public TMP_Text itemText { get { return m_ItemText; } set { m_ItemText = value; RefreshShownValue(); } }

        [SerializeField]
        private Image m_ItemImage;
        public Image itemImage { get { return m_ItemImage; } set { m_ItemImage = value; RefreshShownValue(); } }

        [Space]

        [SerializeField]
        private int m_Value;

        [Space]

        // Items that will be visible when the dropdown is shown.
        // We box this into its own class so we can use a Property Drawer for it.
        [SerializeField]
        private OptionDataList m_Options = new OptionDataList();
        public List<OptionData> options
        {
            get { return m_Options.options; }
            set { m_Options.options = value; RefreshShownValue(); }
        }

        [Space]

        // Notification triggered when the dropdown changes.
        [SerializeField]
        private DropdownEvent m_OnValueChanged = new DropdownEvent();
        public DropdownEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

        private GameObject m_Dropdown;
        private GameObject m_Blocker;
        private List<DropdownItem> m_Items = new List<DropdownItem>();
        private TweenRunner<FloatTween> m_AlphaTweenRunner;
        private bool validTemplate = false;

        private static OptionData s_NoOptionData = new OptionData();

        // Current value.
        public int value
        {
            get
            {
                return m_Value;
            }
            set
            {
                if (Application.isPlaying && (value == m_Value || options.Count == 0))
                    return;

                m_Value = Mathf.Clamp(value, 0, options.Count - 1);
                RefreshShownValue();

                // Notify all listeners
                m_OnValueChanged.Invoke(m_Value);
            }
        }

        public bool IsExpanded
        { get { return m_Dropdown != null; } }

        protected TMP_Dropdown()
        { }

        protected override void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            m_AlphaTweenRunner = new TweenRunner<FloatTween>();
            m_AlphaTweenRunner.Init(this);

            if (m_CaptionImage)
                m_CaptionImage.enabled = (m_CaptionImage.sprite != null);

            if (m_Template)
                m_Template.gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (!IsActive())
                return;

            RefreshShownValue();
        }
#endif

        public void RefreshShownValue()
        {
            OptionData data = s_NoOptionData;

            if (options.Count > 0)
                data = options[Mathf.Clamp(m_Value, 0, options.Count - 1)];

            if (m_CaptionText)
            {
                if (data != null && data.text != null)
                    m_CaptionText.text = data.text;
                else
                    m_CaptionText.text = "";
            }

            if (m_CaptionImage)
            {
                if (data != null)
                    m_CaptionImage.sprite = data.image;
                else
                    m_CaptionImage.sprite = null;
                m_CaptionImage.enabled = (m_CaptionImage.sprite != null);
            }
        }

        public void AddOptions(List<OptionData> options)
        {
            this.options.AddRange(options);
            RefreshShownValue();
        }

        public void AddOptions(List<string> options)
        {
            for (int i = 0; i < options.Count; i++)
                this.options.Add(new OptionData(options[i]));
            RefreshShownValue();
        }

        public void AddOptions(List<Sprite> options)
        {
            for (int i = 0; i < options.Count; i++)
                this.options.Add(new OptionData(options[i]));
            RefreshShownValue();
        }

        public void ClearOptions()
        {
            options.Clear();
            RefreshShownValue();
        }

        private void SetupTemplate()
        {
            validTemplate = false;

            if (!m_Template)
            {
                Debug.LogError("The dropdown template is not assigned. The template needs to be assigned and must have a child GameObject with a Toggle component serving as the item.", this);
                return;
            }

            GameObject templateGo = m_Template.gameObject;
            templateGo.SetActive(true);
            Toggle itemToggle = m_Template.GetComponentInChildren<Toggle>();

            validTemplate = true;
            if (!itemToggle || itemToggle.transform == template)
            {
                validTemplate = false;
                Debug.LogError("The dropdown template is not valid. The template must have a child GameObject with a Toggle component serving as the item.", template);
            }
            else if (!(itemToggle.transform.parent is RectTransform))
            {
                validTemplate = false;
                Debug.LogError("The dropdown template is not valid. The child GameObject with a Toggle component (the item) must have a RectTransform on its parent.", template);
            }
            else if (itemText != null && !itemText.transform.IsChildOf(itemToggle.transform))
            {
                validTemplate = false;
                Debug.LogError("The dropdown template is not valid. The Item Text must be on the item GameObject or children of it.", template);
            }
            else if (itemImage != null && !itemImage.transform.IsChildOf(itemToggle.transform))
            {
                validTemplate = false;
                Debug.LogError("The dropdown template is not valid. The Item Image must be on the item GameObject or children of it.", template);
            }

            if (!validTemplate)
            {
                templateGo.SetActive(false);
                return;
            }

            DropdownItem item = itemToggle.gameObject.AddComponent<DropdownItem>();
            item.text = m_ItemText;
            item.image = m_ItemImage;
            item.toggle = itemToggle;
            item.rectTransform = (RectTransform)itemToggle.transform;

            Canvas popupCanvas = GetOrAddComponent<Canvas>(templateGo);
            popupCanvas.overrideSorting = true;
            popupCanvas.sortingOrder = 30000;

            GetOrAddComponent<GraphicRaycaster>(templateGo);
            GetOrAddComponent<CanvasGroup>(templateGo);
            templateGo.SetActive(false);

            validTemplate = true;
        }

        private static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if (!comp)
                comp = go.AddComponent<T>();
            return comp;
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            Show();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            Show();
        }

        public virtual void OnCancel(BaseEventData eventData)
        {
            Hide();
        }

        // Show the dropdown.
        //
        // Plan for dropdown scrolling to ensure dropdown is contained within screen.
        //
        // We assume the Canvas is the screen that the dropdown must be kept inside.
        // This is always valid for screen space canvas modes.
        // For world space canvases we don't know how it's used, but it could be e.g. for an in-game monitor.
        // We consider it a fair constraint that the canvas must be big enough to contains dropdowns.
        public void Show()
        {
            if (!IsActive() || !IsInteractable() || m_Dropdown != null)
                return;

            if (!validTemplate)
            {
                SetupTemplate();
                if (!validTemplate)
                    return;
            }

            // Get root Canvas.
            var list = TMP_ListPool<Canvas>.Get();
            gameObject.GetComponentsInParent(false, list);
            if (list.Count == 0)
                return;
            Canvas rootCanvas = list[0];
            TMP_ListPool<Canvas>.Release(list);

            m_Template.gameObject.SetActive(true);

            // Instantiate the drop-down template
            m_Dropdown = CreateDropdownList(m_Template.gameObject);
            m_Dropdown.name = "Dropdown List";
            m_Dropdown.SetActive(true);

            // Make drop-down RectTransform have same values as original.
            RectTransform dropdownRectTransform = m_Dropdown.transform as RectTransform;
            dropdownRectTransform.SetParent(m_Template.transform.parent, false);

            // Instantiate the drop-down list items

            // Find the dropdown item and disable it.
            DropdownItem itemTemplate = m_Dropdown.GetComponentInChildren<DropdownItem>();

            GameObject content = itemTemplate.rectTransform.parent.gameObject;
            RectTransform contentRectTransform = content.transform as RectTransform;
            itemTemplate.rectTransform.gameObject.SetActive(true);

            // Get the rects of the dropdown and item
            Rect dropdownContentRect = contentRectTransform.rect;
            Rect itemTemplateRect = itemTemplate.rectTransform.rect;

            // Calculate the visual offset between the item's edges and the background's edges
            Vector2 offsetMin = itemTemplateRect.min - dropdownContentRect.min + (Vector2)itemTemplate.rectTransform.localPosition;
            Vector2 offsetMax = itemTemplateRect.max - dropdownContentRect.max + (Vector2)itemTemplate.rectTransform.localPosition;
            Vector2 itemSize = itemTemplateRect.size;

            m_Items.Clear();

            Toggle prev = null;
            for (int i = 0; i < options.Count; ++i)
            {
                OptionData data = options[i];
                DropdownItem item = AddItem(data, value == i, itemTemplate, m_Items);
                if (item == null)
                    continue;

                // Automatically set up a toggle state change listener
                item.toggle.isOn = value == i;
                item.toggle.onValueChanged.AddListener(x => OnSelectItem(item.toggle));

                // Select current option
                if (item.toggle.isOn)
                    item.toggle.Select();

                // Automatically set up explicit navigation
                if (prev != null)
                {
                    Navigation prevNav = prev.navigation;
                    Navigation toggleNav = item.toggle.navigation;
                    prevNav.mode = Navigation.Mode.Explicit;
                    toggleNav.mode = Navigation.Mode.Explicit;

                    prevNav.selectOnDown = item.toggle;
                    prevNav.selectOnRight = item.toggle;
                    toggleNav.selectOnLeft = prev;
                    toggleNav.selectOnUp = prev;

                    prev.navigation = prevNav;
                    item.toggle.navigation = toggleNav;
                }
                prev = item.toggle;
            }

            // Reposition all items now that all of them have been added
            Vector2 sizeDelta = contentRectTransform.sizeDelta;
            sizeDelta.y = itemSize.y * m_Items.Count + offsetMin.y - offsetMax.y;
            contentRectTransform.sizeDelta = sizeDelta;

            float extraSpace = dropdownRectTransform.rect.height - contentRectTransform.rect.height;
            if (extraSpace > 0)
                dropdownRectTransform.sizeDelta = new Vector2(dropdownRectTransform.sizeDelta.x, dropdownRectTransform.sizeDelta.y - extraSpace);

            // Invert anchoring and position if dropdown is partially or fully outside of canvas rect.
            // Typically this will have the effect of placing the dropdown above the button instead of below,
            // but it works as inversion regardless of initial setup.
            Vector3[] corners = new Vector3[4];
            dropdownRectTransform.GetWorldCorners(corners);

            RectTransform rootCanvasRectTransform = rootCanvas.transform as RectTransform;
            Rect rootCanvasRect = rootCanvasRectTransform.rect;
            for (int axis = 0; axis < 2; axis++)
            {
            bool outside = false;
            for (int i = 0; i < 4; i++)
            {
                Vector3 corner = rootCanvasRectTransform.InverseTransformPoint(corners[i]);
                    if (corner[axis] < rootCanvasRect.min[axis] || corner[axis] > rootCanvasRect.max[axis])
                {
                    outside = true;
                    break;
                }
            }
            if (outside)
                    RectTransformUtility.FlipLayoutOnAxis(dropdownRectTransform, axis, false, false);
            }

            for (int i = 0; i < m_Items.Count; i++)
            {
                RectTransform itemRect = m_Items[i].rectTransform;
                itemRect.anchorMin = new Vector2(itemRect.anchorMin.x, 0);
                itemRect.anchorMax = new Vector2(itemRect.anchorMax.x, 0);
                itemRect.anchoredPosition = new Vector2(itemRect.anchoredPosition.x, offsetMin.y + itemSize.y * (m_Items.Count - 1 - i) + itemSize.y * itemRect.pivot.y);
                itemRect.sizeDelta = new Vector2(itemRect.sizeDelta.x, itemSize.y);
            }

            // Fade in the popup
            AlphaFadeList(0.15f, 0f, 1f);

            // Make drop-down template and item template inactive
            m_Template.gameObject.SetActive(false);
            itemTemplate.gameObject.SetActive(false);

            m_Blocker = CreateBlocker(rootCanvas);
        }

        protected virtual GameObject CreateBlocker(Canvas rootCanvas)
        {
            // Create blocker GameObject.
            GameObject blocker = new GameObject("Blocker");

            // Setup blocker RectTransform to cover entire root canvas area.
            RectTransform blockerRect = blocker.AddComponent<RectTransform>();
            blockerRect.SetParent(rootCanvas.transform, false);
            blockerRect.anchorMin = Vector3.zero;
            blockerRect.anchorMax = Vector3.one;
            blockerRect.sizeDelta = Vector2.zero;

            // Make blocker be in separate canvas in same layer as dropdown and in layer just below it.
            Canvas blockerCanvas = blocker.AddComponent<Canvas>();
            blockerCanvas.overrideSorting = true;
            Canvas dropdownCanvas = m_Dropdown.GetComponent<Canvas>();
            blockerCanvas.sortingLayerID = dropdownCanvas.sortingLayerID;
            blockerCanvas.sortingOrder = dropdownCanvas.sortingOrder - 1;

            // Add raycaster since it's needed to block.
            blocker.AddComponent<GraphicRaycaster>();

            // Add image since it's needed to block, but make it clear.
            Image blockerImage = blocker.AddComponent<Image>();
            blockerImage.color = Color.clear;

            // Add button since it's needed to block, and to close the dropdown when blocking area is clicked.
            Button blockerButton = blocker.AddComponent<Button>();
            blockerButton.onClick.AddListener(Hide);

            return blocker;
        }

        protected virtual void DestroyBlocker(GameObject blocker)
        {
            Destroy(blocker);
        }

        protected virtual GameObject CreateDropdownList(GameObject template)
        {
            return (GameObject)Instantiate(template);
        }

        protected virtual void DestroyDropdownList(GameObject dropdownList)
        {
            Destroy(dropdownList);
        }

        protected virtual DropdownItem CreateItem(DropdownItem itemTemplate)
        {
            return (DropdownItem)Instantiate(itemTemplate);
        }

        protected virtual void DestroyItem(DropdownItem item)
        {
            // No action needed since destroying the dropdown list destroys all contained items as well.
        }

        // Add a new drop-down list item with the specified values.
        private DropdownItem AddItem(OptionData data, bool selected, DropdownItem itemTemplate, List<DropdownItem> items)
        {
            // Add a new item to the dropdown.
            DropdownItem item = CreateItem(itemTemplate);
            item.rectTransform.SetParent(itemTemplate.rectTransform.parent, false);

            item.gameObject.SetActive(true);
            item.gameObject.name = "Item " + items.Count + (data.text != null ? ": " + data.text : "");

            if (item.toggle != null)
            {
                item.toggle.isOn = false;
            }

            // Set the item's data
            if (item.text)
                item.text.text = data.text;
            if (item.image)
            {
                item.image.sprite = data.image;
                item.image.enabled = (item.image.sprite != null);
            }

            items.Add(item);
            return item;
        }

        private void AlphaFadeList(float duration, float alpha)
        {
            CanvasGroup group = m_Dropdown.GetComponent<CanvasGroup>();
            AlphaFadeList(duration, group.alpha, alpha);
        }

        private void AlphaFadeList(float duration, float start, float end)
        {
            if (end.Equals(start))
                return;

            FloatTween tween = new FloatTween { duration = duration, startValue = start, targetValue = end };
            tween.AddOnChangedCallback(SetAlpha);
            tween.ignoreTimeScale = true;
            m_AlphaTweenRunner.StartTween(tween);
        }

        private void SetAlpha(float alpha)
        {
            if (!m_Dropdown)
                return;
            CanvasGroup group = m_Dropdown.GetComponent<CanvasGroup>();
            group.alpha = alpha;
        }

        // Hide the dropdown.
        public void Hide()
        {
            if (m_Dropdown != null)
            {
                AlphaFadeList(0.15f, 0f);

                // User could have disabled the dropdown during the OnValueChanged call.
                if (IsActive())
                StartCoroutine(DelayedDestroyDropdownList(0.15f));
            }
            if (m_Blocker != null)
                DestroyBlocker(m_Blocker);
            m_Blocker = null;
            Select();
        }

        private IEnumerator DelayedDestroyDropdownList(float delay)
        {

                yield return new WaitForSecondsRealtime(delay);

            for (int i = 0; i < m_Items.Count; i++)
            {
                if (m_Items[i] != null)
                    DestroyItem(m_Items[i]);
                m_Items.Clear();
            }
            if (m_Dropdown != null)
                DestroyDropdownList(m_Dropdown);
            m_Dropdown = null;
        }

        // Change the value and hide the dropdown.
        private void OnSelectItem(Toggle toggle)
        {
            if (!toggle.isOn)
                toggle.isOn = true;

            int selectedIndex = -1;
            Transform tr = toggle.transform;
            Transform parent = tr.parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i) == tr)
                {
                    // Subtract one to account for template child.
                    selectedIndex = i - 1;
                    break;
                }
            }

            if (selectedIndex < 0)
                return;

            value = selectedIndex;
            Hide();
        }
    }
}