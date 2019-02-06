using UnityEngine;
using UnityEditor;
using System.Collections;


namespace TMPro.EditorUtilities
{

    [CustomPropertyDrawer(typeof(TMP_Sprite))]
    public class SpriteInfoDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //SerializedProperty prop_fileID = property.FindPropertyRelative("fileID");
            SerializedProperty prop_id = property.FindPropertyRelative("id");
            SerializedProperty prop_name = property.FindPropertyRelative("name");
            SerializedProperty prop_hashCode = property.FindPropertyRelative("hashCode");
            SerializedProperty prop_unicode = property.FindPropertyRelative("unicode");
            SerializedProperty prop_x = property.FindPropertyRelative("x");
            SerializedProperty prop_y = property.FindPropertyRelative("y");
            SerializedProperty prop_width = property.FindPropertyRelative("width");
            SerializedProperty prop_height = property.FindPropertyRelative("height");
            SerializedProperty prop_xOffset = property.FindPropertyRelative("xOffset");
            SerializedProperty prop_yOffset = property.FindPropertyRelative("yOffset");
            SerializedProperty prop_xAdvance = property.FindPropertyRelative("xAdvance");
            SerializedProperty prop_scale = property.FindPropertyRelative("scale");
            SerializedProperty prop_sprite = property.FindPropertyRelative("sprite");

            // Get a reference to the sprite texture
            Texture tex = (property.serializedObject.targetObject as TMP_SpriteAsset).spriteSheet;

            // Return if we don't have a texture assigned to the sprite asset.
            if (tex == null)
            {
                Debug.LogWarning("Please assign a valid Sprite Atlas texture to the [" + property.serializedObject.targetObject.name + "] Sprite Asset.", property.serializedObject.targetObject);
                return;
            }

            Vector2 spriteTexPosition = new Vector2(position.x, position.y);
            Vector2 spriteSize = new Vector2(65, 65);
            if (prop_width.floatValue >= prop_height.floatValue)
            {
                spriteSize.y = prop_height.floatValue * spriteSize.x / prop_width.floatValue;
                spriteTexPosition.y += (spriteSize.x - spriteSize.y) / 2;
            }
            else
            {
                spriteSize.x = prop_width.floatValue * spriteSize.y / prop_height.floatValue;
                spriteTexPosition.x += (spriteSize.y - spriteSize.x) / 2;
            }

            // Compute the normalized texture coordinates
            Rect texCoords = new Rect(prop_x.floatValue / tex.width, prop_y.floatValue / tex.height, prop_width.floatValue / tex.width, prop_height.floatValue / tex.height);
            GUI.DrawTextureWithTexCoords(new Rect(spriteTexPosition.x + 5, spriteTexPosition.y + 2.5f, spriteSize.x,  spriteSize.y), tex, texCoords, true);

            // We get Rect since a valid position may not be provided by the caller.
            Rect rect = new Rect(position.x, position.y, position.width, 49);
            rect.x += 70;

            bool isEnabled = GUI.enabled;
            GUI.enabled = true;
            EditorGUIUtility.labelWidth = 30f;
            EditorGUI.LabelField(new Rect(rect.x + 5f, rect.y, 65f, 18), new GUIContent("ID:"), new GUIContent(prop_id.intValue.ToString()));

            GUI.enabled = isEnabled;
            EditorGUI.BeginChangeCheck();
            EditorGUIUtility.labelWidth = 55f;
            GUI.SetNextControlName("Unicode Input");
            string unicode = EditorGUI.TextField(new Rect(rect.x + 75f, rect.y, 105, 18), "Unicode:", prop_unicode.intValue.ToString("X"));

            if (GUI.GetNameOfFocusedControl() == "Unicode Input")
            {
                //Filter out unwanted characters.
                char chr = Event.current.character;
                if ((chr < '0' || chr > '9') && (chr < 'a' || chr > 'f') && (chr < 'A' || chr > 'F'))
                {
                    Event.current.character = '\0';
                }

                if (EditorGUI.EndChangeCheck())
                {
                    prop_unicode.intValue = TMP_TextUtilities.StringToInt(unicode);

                    property.serializedObject.ApplyModifiedProperties();

                    TMP_SpriteAsset spriteAsset = property.serializedObject.targetObject as TMP_SpriteAsset;
                    spriteAsset.UpdateLookupTables();
                }
            }


            EditorGUIUtility.labelWidth = 45f;
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(new Rect(rect.x + 185f, rect.y, rect.width - 260, 18), prop_name, new GUIContent("Name: " + prop_name.stringValue));
            if (EditorGUI.EndChangeCheck())
            {
                Sprite sprite = prop_sprite.objectReferenceValue as Sprite;
                if (sprite != null)
                    sprite.name = prop_name.stringValue;

                // Recompute hashCode for new name
                prop_hashCode.intValue = TMP_TextUtilities.GetSimpleHashCode(prop_name.stringValue);
                // Check to make sure for duplicates
                property.serializedObject.ApplyModifiedProperties();
                // Dictionary needs to be updated since HashCode has changed.
                //TMP_StyleSheet.Instance.LoadStyleDictionary();
            }

            EditorGUIUtility.labelWidth = 30f;
            EditorGUIUtility.fieldWidth = 10f;

            //GUI.enabled = false;
            float width = (rect.width - 75f) / 4;
            EditorGUI.PropertyField(new Rect(rect.x + 5f + width * 0, rect.y + 22, width - 5f, 18), prop_x, new GUIContent("X:"));
            EditorGUI.PropertyField(new Rect(rect.x + 5f + width * 1, rect.y + 22, width - 5f, 18), prop_y, new GUIContent("Y:"));
            EditorGUI.PropertyField(new Rect(rect.x + 5f + width * 2, rect.y + 22, width - 5f, 18), prop_width, new GUIContent("W:"));
            EditorGUI.PropertyField(new Rect(rect.x + 5f + width * 3, rect.y + 22, width - 5f, 18), prop_height, new GUIContent("H:"));

            //GUI.enabled = true;

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(new Rect(rect.x + 5f + width * 0, rect.y + 44, width - 5f, 18), prop_xOffset, new GUIContent("OX:"));
            EditorGUI.PropertyField(new Rect(rect.x + 5f + width * 1, rect.y + 44, width - 5f, 18), prop_yOffset, new GUIContent("OY:"));
            EditorGUI.PropertyField(new Rect(rect.x + 5f + width * 2, rect.y + 44, width - 5f, 18), prop_xAdvance, new GUIContent("Adv."));
            EditorGUI.PropertyField(new Rect(rect.x + 5f + width * 3, rect.y + 44, width - 5f, 18), prop_scale, new GUIContent("SF."));
            if (EditorGUI.EndChangeCheck())
            {
                //Sprite sprite = prop_sprite.objectReferenceValue as Sprite;
                //sprite = Sprite.Create(sprite.texture, sprite.rect, new Vector2(0.1f, 0.8f));
                //prop_sprite.objectReferenceValue = sprite;
                //Debug.Log(sprite.bounds);
            }
        }


        //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        //{
        //    return 40f;
        //}


    }
}
