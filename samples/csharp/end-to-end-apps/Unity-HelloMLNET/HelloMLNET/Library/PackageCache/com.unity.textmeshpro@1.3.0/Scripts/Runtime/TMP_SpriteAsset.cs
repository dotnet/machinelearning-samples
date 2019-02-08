using UnityEngine;
using System.Collections.Generic;


namespace TMPro
{

    public class TMP_SpriteAsset : TMP_Asset
    {
        internal Dictionary<int, int> m_UnicodeLookup;
        internal Dictionary<int, int> m_NameLookup;

        /// <summary>
        /// Static reference to the default font asset included with TextMesh Pro.
        /// </summary>
        public static TMP_SpriteAsset defaultSpriteAsset
        {
            get
            {
                if (m_defaultSpriteAsset == null)
                {
                    m_defaultSpriteAsset = Resources.Load<TMP_SpriteAsset>("Sprite Assets/Default Sprite Asset");
                }

                return m_defaultSpriteAsset;
            }
        }
        public static TMP_SpriteAsset m_defaultSpriteAsset;
        
        
        // The texture which contains the sprites.
        public Texture spriteSheet;

        // List which contains the SpriteInfo for the sprites contained in the sprite sheet.
        public List<TMP_Sprite> spriteInfoList;

        /// <summary>
        /// Dictionary used to lookup the index of a given sprite based on a Unicode value.
        /// </summary>
        //private Dictionary<int, int> m_SpriteUnicodeLookup;


        /// <summary>
        /// List which contains the Fallback font assets for this font.
        /// </summary>
        [SerializeField]
        public List<TMP_SpriteAsset> fallbackSpriteAssets;


        //private bool isEditingAsset;

        void OnEnable()
        {
            // Make sure we have a valid material.
            //if (this.material == null && !isEditingAsset)
            //   this.material = GetDefaultSpriteMaterial();
        }


#if UNITY_EDITOR
        /// <summary>
        /// 
        /// </summary>
        void OnValidate()
        {
            UpdateLookupTables();

            TMPro_EventManager.ON_SPRITE_ASSET_PROPERTY_CHANGED(true, this);
        }
#endif


        /// <summary>
        /// Create a material for the sprite asset.
        /// </summary>
        /// <returns></returns>
        Material GetDefaultSpriteMaterial()
        {
            //isEditingAsset = true;
            ShaderUtilities.GetShaderPropertyIDs();

            // Add a new material
            Shader shader = Shader.Find("TextMeshPro/Sprite");
            Material tempMaterial = new Material(shader);
            tempMaterial.SetTexture(ShaderUtilities.ID_MainTex, spriteSheet);
            tempMaterial.hideFlags = HideFlags.HideInHierarchy;

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.AddObjectToAsset(tempMaterial, this);
            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(this));
#endif
            //isEditingAsset = false;

            return tempMaterial;
        }


        /// <summary>
        /// Function to update the sprite name and unicode lookup tables.
        /// This function should be called when a sprite's name or unicode value changes or when a new sprite is added.
        /// </summary>
        public void UpdateLookupTables()
        {
            if (m_NameLookup == null) m_NameLookup = new Dictionary<int, int>();
            m_NameLookup.Clear();

            if (m_UnicodeLookup == null) m_UnicodeLookup = new Dictionary<int, int>();
            m_UnicodeLookup.Clear();

            for (int i = 0; i < spriteInfoList.Count; i++)
            {
                int nameHashCode = spriteInfoList[i].hashCode;

                if (m_NameLookup.ContainsKey(nameHashCode) == false)
                    m_NameLookup.Add(nameHashCode, i);

                int unicode = spriteInfoList[i].unicode;

                if (m_UnicodeLookup.ContainsKey(unicode) == false)
                    m_UnicodeLookup.Add(unicode, i);
            }
        }


        /// <summary>
        /// Function which returns the sprite index using the hashcode of the name
        /// </summary>
        /// <param name="hashCode"></param>
        /// <returns></returns>
        public int GetSpriteIndexFromHashcode(int hashCode)
        {
            if (m_NameLookup == null)
                UpdateLookupTables();

            int index = 0;
            if (m_NameLookup.TryGetValue(hashCode, out index))
                return index;

            return -1;
        }


        /// <summary>
        /// Returns the index of the sprite for the given unicode value.
        /// </summary>
        /// <param name="unicode"></param>
        /// <returns></returns>
        public int GetSpriteIndexFromUnicode (int unicode)
        {
            if (m_UnicodeLookup == null)
                UpdateLookupTables();

            int index = 0;
            if (m_UnicodeLookup.TryGetValue(unicode, out index))
                return index;

            return -1;
        }


        /// <summary>
        /// Returns the index of the sprite for the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetSpriteIndexFromName (string name)
        {
            if (m_NameLookup == null)
                UpdateLookupTables();

            int hashCode = TMP_TextUtilities.GetSimpleHashCode(name);

            return GetSpriteIndexFromHashcode(hashCode);
        }


        /// <summary>
        /// Used to keep track of which Sprite Assets have been searched.
        /// </summary>
        private static List<int> k_searchedSpriteAssets;

        /// <summary>
        /// Search through the given sprite asset and its fallbacks for the specified sprite matching the given unicode character.
        /// </summary>
        /// <param name="spriteAsset">The font asset to search for the given character.</param>
        /// <param name="unicode">The character to find.</param>
        /// <param name="glyph">out parameter containing the glyph for the specified character (if found).</param>
        /// <returns></returns>
        public static TMP_SpriteAsset SearchForSpriteByUnicode(TMP_SpriteAsset spriteAsset, int unicode, bool includeFallbacks, out int spriteIndex)
        {
            // Check to make sure sprite asset is not null
            if (spriteAsset == null) { spriteIndex = -1; return null; }

            // Get sprite index for the given unicode
            spriteIndex = spriteAsset.GetSpriteIndexFromUnicode(unicode);
            if (spriteIndex != -1)
                return spriteAsset;

            // Initialize list to track instance of Sprite Assets that have already been searched.
            if (k_searchedSpriteAssets == null)
                k_searchedSpriteAssets = new List<int>();

            k_searchedSpriteAssets.Clear();

            // Get instance ID of sprite asset and add to list.
            int id = spriteAsset.GetInstanceID();
            k_searchedSpriteAssets.Add(id);

            // Search potential fallback sprite assets if includeFallbacks is true.
            if (includeFallbacks && spriteAsset.fallbackSpriteAssets != null && spriteAsset.fallbackSpriteAssets.Count > 0)
                return SearchForSpriteByUnicodeInternal(spriteAsset.fallbackSpriteAssets, unicode, includeFallbacks, out spriteIndex);

            // Search default sprite asset potentially assigned in the TMP Settings.
            if (includeFallbacks && TMP_Settings.defaultSpriteAsset != null)
                return SearchForSpriteByUnicodeInternal(TMP_Settings.defaultSpriteAsset, unicode, includeFallbacks, out spriteIndex);

            spriteIndex = -1;
            return null;
        }


        /// <summary>
        /// Search through the given list of sprite assets and fallbacks for a sprite whose unicode value matches the target unicode.
        /// </summary>
        /// <param name="spriteAssets"></param>
        /// <param name="unicode"></param>
        /// <param name="includeFallbacks"></param>
        /// <param name="spriteIndex"></param>
        /// <returns></returns>
        private static TMP_SpriteAsset SearchForSpriteByUnicodeInternal(List<TMP_SpriteAsset> spriteAssets, int unicode, bool includeFallbacks, out int spriteIndex)
        {
            for (int i = 0; i < spriteAssets.Count; i++)
            {
                TMP_SpriteAsset temp = spriteAssets[i];
                if (temp == null) continue;

                int id = temp.GetInstanceID();

                // Skip over the fallback sprite asset if it has already been searched.
                if (k_searchedSpriteAssets.Contains(id)) continue;

                // Add to list of font assets already searched.
                k_searchedSpriteAssets.Add(id);

                temp = SearchForSpriteByUnicodeInternal(temp, unicode, includeFallbacks, out spriteIndex);

                if (temp != null)
                    return temp;
            }

            spriteIndex = -1;
            return null;
        }


        /// <summary>
        /// Search the given sprite asset and fallbacks for a sprite whose unicode value matches the target unicode.
        /// </summary>
        /// <param name="spriteAsset"></param>
        /// <param name="unicode"></param>
        /// <param name="includeFallbacks"></param>
        /// <param name="spriteIndex"></param>
        /// <returns></returns>
        private static TMP_SpriteAsset SearchForSpriteByUnicodeInternal(TMP_SpriteAsset spriteAsset, int unicode, bool includeFallbacks, out int spriteIndex)
        {
            // Get sprite index for the given unicode
            spriteIndex = spriteAsset.GetSpriteIndexFromUnicode(unicode);
            if (spriteIndex != -1)
                return spriteAsset;

            if (includeFallbacks && spriteAsset.fallbackSpriteAssets != null && spriteAsset.fallbackSpriteAssets.Count > 0)
                return SearchForSpriteByUnicodeInternal(spriteAsset.fallbackSpriteAssets, unicode, includeFallbacks, out spriteIndex);

            spriteIndex = -1;
            return null;
        }


        /// <summary>
        /// Search the given sprite asset and fallbacks for a sprite whose hash code value of its name matches the target hash code.
        /// </summary>
        /// <param name="spriteAsset">The Sprite Asset to search for the given sprite whose name matches the hashcode value</param>
        /// <param name="hashCode">The hash code value matching the name of the sprite</param>
        /// <param name="includeFallbacks">Include fallback sprite assets in the search</param>
        /// <param name="spriteIndex">The index of the sprite matching the provided hash code</param>
        /// <returns>The Sprite Asset that contains the sprite</returns>
        public static TMP_SpriteAsset SearchForSpriteByHashCode(TMP_SpriteAsset spriteAsset, int hashCode, bool includeFallbacks, out int spriteIndex)
        {
            // Make sure sprite asset is not null
            if (spriteAsset == null) { spriteIndex = -1; return null; }

            spriteIndex = spriteAsset.GetSpriteIndexFromHashcode(hashCode);
            if (spriteIndex != -1)
                return spriteAsset;

            // Initialize list to track instance of Sprite Assets that have already been searched.
            if (k_searchedSpriteAssets == null)
                k_searchedSpriteAssets = new List<int>();

            k_searchedSpriteAssets.Clear();

            int id = spriteAsset.GetInstanceID();
            // Add to list of font assets already searched.
            k_searchedSpriteAssets.Add(id);

            if (includeFallbacks && spriteAsset.fallbackSpriteAssets != null && spriteAsset.fallbackSpriteAssets.Count > 0)
                return SearchForSpriteByHashCodeInternal(spriteAsset.fallbackSpriteAssets, hashCode, includeFallbacks, out spriteIndex);

            // Search default sprite asset potentially assigned in the TMP Settings.
            if (includeFallbacks && TMP_Settings.defaultSpriteAsset != null)
                return SearchForSpriteByHashCodeInternal(TMP_Settings.defaultSpriteAsset, hashCode, includeFallbacks, out spriteIndex);

            spriteIndex = -1;
            return null;
        }


        /// <summary>
        ///  Search through the given list of sprite assets and fallbacks for a sprite whose hash code value of its name matches the target hash code.
        /// </summary>
        /// <param name="spriteAssets"></param>
        /// <param name="hashCode"></param>
        /// <param name="searchFallbacks"></param>
        /// <param name="spriteIndex"></param>
        /// <returns></returns>
        private static TMP_SpriteAsset SearchForSpriteByHashCodeInternal(List<TMP_SpriteAsset> spriteAssets, int hashCode, bool searchFallbacks, out int spriteIndex)
        {
            // Search through the list of sprite assets
            for (int i = 0; i < spriteAssets.Count; i++)
            {
                TMP_SpriteAsset temp = spriteAssets[i];
                if (temp == null) continue;

                int id = temp.GetInstanceID();

                // Skip over the fallback sprite asset if it has already been searched.
                if (k_searchedSpriteAssets.Contains(id)) continue;

                // Add to list of font assets already searched.
                k_searchedSpriteAssets.Add(id);

                temp = SearchForSpriteByHashCodeInternal(temp, hashCode, searchFallbacks, out spriteIndex);

                if (temp != null)
                    return temp;
            }

            spriteIndex = -1;
            return null;
        }


        /// <summary>
        /// Search through the given sprite asset and fallbacks for a sprite whose hash code value of its name matches the target hash code.
        /// </summary>
        /// <param name="spriteAsset"></param>
        /// <param name="hashCode"></param>
        /// <param name="searchFallbacks"></param>
        /// <param name="spriteIndex"></param>
        /// <returns></returns>
        private static TMP_SpriteAsset SearchForSpriteByHashCodeInternal(TMP_SpriteAsset spriteAsset, int hashCode, bool searchFallbacks, out int spriteIndex)
        {
            // Get the sprite for the given hash code.
            spriteIndex = spriteAsset.GetSpriteIndexFromHashcode(hashCode);
            if (spriteIndex != -1)
                return spriteAsset;

            if (searchFallbacks && spriteAsset.fallbackSpriteAssets != null && spriteAsset.fallbackSpriteAssets.Count > 0)
                return SearchForSpriteByHashCodeInternal(spriteAsset.fallbackSpriteAssets, hashCode, searchFallbacks, out spriteIndex);

            spriteIndex = -1;
            return null;
        }

    }
}
