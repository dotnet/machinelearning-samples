using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace TMPro.EditorUtilities
{
    public static class TMPro_CreateObjectMenu
    {

        /// <summary>
        /// Create a TextMeshPro object that works with the Mesh Renderer
        /// </summary>
        /// <param name="command"></param>
        [MenuItem("GameObject/3D Object/TextMeshPro - Text", false, 30)]
        static void CreateTextMeshProObjectPerform(MenuCommand command)
        {
            GameObject go = new GameObject("TextMeshPro");

            TextMeshPro textMeshPro = go.AddComponent<TextMeshPro>();
            textMeshPro.text = "Sample text";
            textMeshPro.alignment = TextAlignmentOptions.TopLeft;

            Undo.RegisterCreatedObjectUndo((Object)go, "Create " + go.name);

            GameObject contextObject = command.context as GameObject;
            if (contextObject != null)
            {
                GameObjectUtility.SetParentAndAlign(go, contextObject);
                Undo.SetTransformParent(go.transform, contextObject.transform, "Parent " + go.name);
            }

            Selection.activeGameObject = go;
        }


        /// <summary>
        /// Create a TextMeshPro object that works with the CanvasRenderer
        /// </summary>
        /// <param name="command"></param>
        [MenuItem("GameObject/UI/TextMeshPro - Text", false, 2001)]
        static void CreateTextMeshProGuiObjectPerform(MenuCommand command)
        {

            // Check if there is a Canvas in the scene
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                // Create new Canvas since none exists in the scene.
                GameObject canvasObject = new GameObject("Canvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                // Add a Graphic Raycaster Component as well
                canvas.gameObject.AddComponent<GraphicRaycaster>();

                Undo.RegisterCreatedObjectUndo(canvasObject, "Create " + canvasObject.name);
            }


            // Create the TextMeshProUGUI Object
            GameObject go = new GameObject("TextMeshPro Text");
            RectTransform goRectTransform = go.AddComponent<RectTransform>();

            Undo.RegisterCreatedObjectUndo((Object)go, "Create " + go.name);

            // Check if object is being create with left or right click
            GameObject contextObject = command.context as GameObject;
            if (contextObject == null)
            {
                //goRectTransform.sizeDelta = new Vector2(200f, 50f);
                GameObjectUtility.SetParentAndAlign(go, canvas.gameObject);

                TextMeshProUGUI textMeshPro = go.AddComponent<TextMeshProUGUI>();
                textMeshPro.text = "New Text";
                textMeshPro.alignment = TextAlignmentOptions.TopLeft;
            }
            else
            {
                if (contextObject.GetComponent<Button>() != null)
                {
                    goRectTransform.sizeDelta = Vector2.zero;
                    goRectTransform.anchorMin = Vector2.zero;
                    goRectTransform.anchorMax = Vector2.one;

                    GameObjectUtility.SetParentAndAlign(go, contextObject);

                    TextMeshProUGUI textMeshPro = go.AddComponent<TextMeshProUGUI>();
                    textMeshPro.text = "Button";
                    textMeshPro.fontSize = 24;
                    textMeshPro.alignment = TextAlignmentOptions.Center;
                }
                else
                {
                    //goRectTransform.sizeDelta = new Vector2(200f, 50f);

                    GameObjectUtility.SetParentAndAlign(go, contextObject);

                    TextMeshProUGUI textMeshPro = go.AddComponent<TextMeshProUGUI>();
                    textMeshPro.text = "New Text";
                    textMeshPro.alignment = TextAlignmentOptions.TopLeft;
                }
            }

         
            // Check if an event system already exists in the scene
            if (!Object.FindObjectOfType<EventSystem>())
            {
                GameObject eventObject = new GameObject("EventSystem", typeof(EventSystem));
                eventObject.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(eventObject, "Create " + eventObject.name);
            }

            Selection.activeGameObject = go;
        }



        [MenuItem("GameObject/UI/TextMeshPro - Input Field", false, 2037)]
        static void AddTextMeshProInputField(MenuCommand menuCommand)
        {
            GameObject go = TMP_DefaultControls.CreateInputField(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }


        [MenuItem("GameObject/UI/TextMeshPro - Dropdown", false, 2036)]
        static public void AddDropdown(MenuCommand menuCommand)
        {
            GameObject go = TMP_DefaultControls.CreateDropdown(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }


        private const string kUILayerName = "UI";

        private const string kStandardSpritePath = "UI/Skin/UISprite.psd";
        private const string kBackgroundSpritePath = "UI/Skin/Background.psd";
        private const string kInputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";
        private const string kKnobPath = "UI/Skin/Knob.psd";
        private const string kCheckmarkPath = "UI/Skin/Checkmark.psd";
        private const string kDropdownArrowPath = "UI/Skin/DropdownArrow.psd";
        private const string kMaskPath = "UI/Skin/UIMask.psd";

        static private TMP_DefaultControls.Resources s_StandardResources;


        static private TMP_DefaultControls.Resources GetStandardResources()
        {
            if (s_StandardResources.standard == null)
            {
                s_StandardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>(kStandardSpritePath);
                s_StandardResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpritePath);
                s_StandardResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>(kInputFieldBackgroundPath);
                s_StandardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>(kKnobPath);
                s_StandardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>(kCheckmarkPath);
                s_StandardResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>(kDropdownArrowPath);
                s_StandardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>(kMaskPath);
            }
            return s_StandardResources;
        }


        private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
        {
            // Find the best scene view
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null && SceneView.sceneViews.Count > 0)
                sceneView = SceneView.sceneViews[0] as SceneView;

            // Couldn't find a SceneView. Don't set position.
            if (sceneView == null || sceneView.camera == null)
                return;

            // Create world space Plane from canvas position.
            Vector2 localPlanePosition;
            Camera camera = sceneView.camera;
            Vector3 position = Vector3.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition))
            {
                // Adjust for canvas pivot
                localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
                localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

                localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
                localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);

                // Adjust for anchoring
                position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
                position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;

                Vector3 minLocalPosition;
                minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
                minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;

                Vector3 maxLocalPosition;
                maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
                maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;

                position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
                position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
            }

            itemTransform.anchoredPosition = position;
            itemTransform.localRotation = Quaternion.identity;
            itemTransform.localScale = Vector3.one;
        }


        private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                parent = GetOrCreateCanvasGameObject();
            }

            string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parent.transform, element.name);
            element.name = uniqueName;
            Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);
            Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
            GameObjectUtility.SetParentAndAlign(element, parent);
            if (parent != menuCommand.context) // not a context click, so center in sceneview
                SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());

            Selection.activeGameObject = element;
        }


        static public GameObject CreateNewUI()
        {
            // Root for the UI
            var root = new GameObject("Canvas");
            root.layer = LayerMask.NameToLayer(kUILayerName);
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

            // if there is no event system add one...
            CreateEventSystem(false);
            return root;
        }


        private static void CreateEventSystem(bool select)
        {
            CreateEventSystem(select, null);
        }


        private static void CreateEventSystem(bool select, GameObject parent)
        {
            var esys = Object.FindObjectOfType<EventSystem>();
            if (esys == null)
            {
                var eventSystem = new GameObject("EventSystem");
                GameObjectUtility.SetParentAndAlign(eventSystem, parent);
                esys = eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();

                Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
            }

            if (select && esys != null)
            {
                Selection.activeGameObject = esys.gameObject;
            }
        }


        static public GameObject GetOrCreateCanvasGameObject()
        {
            GameObject selectedGo = Selection.activeGameObject;

            // Try to find a gameobject that is the selected GO or one if its parents.
            Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;

            // No canvas in selection or its parents? Then use just any canvas..
            canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;

            // No canvas in the scene at all? Then create a new one.
            return CreateNewUI();
        }
    }
}
