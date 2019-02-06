using UnityEngine;
using UnityEngine.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace TMPro
{
    /// <summary>
    /// Class that contains the basic information about the font.
    /// </summary>
    [Serializable]
    public class FaceInfo
    {
        public string Name;
        public float PointSize;
        public float Scale;

        public int CharacterCount;

        public float LineHeight;
        public float Baseline;
        public float Ascender;
        public float CapHeight;
        public float Descender;
        public float CenterLine;

        public float SuperscriptOffset;
        public float SubscriptOffset;
        public float SubSize;

        public float Underline;
        public float UnderlineThickness;

        public float strikethrough;
        public float strikethroughThickness;

        public float TabWidth;

        public float Padding;
        public float AtlasWidth;
        public float AtlasHeight;
    }


    // Class which contains the Glyph Info / Character definition for each character contained in the font asset.
    [Serializable]
    public class TMP_Glyph : TMP_TextElement
    {
        /// <summary>
        /// Function to create a deep copy of a GlyphInfo.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TMP_Glyph Clone(TMP_Glyph source)
        {
            TMP_Glyph copy = new TMP_Glyph();

            copy.id = source.id;
            copy.x = source.x;
            copy.y = source.y;
            copy.width = source.width;
            copy.height = source.height;
            copy.xOffset = source.xOffset;
            copy.yOffset = source.yOffset;
            copy.xAdvance = source.xAdvance;
            copy.scale = source.scale;

            return copy;
        }
    }


    // Structure which holds the font creation settings
    [Serializable]
    public struct FontAssetCreationSettings
    {
        public string sourceFontFileName;
        public string sourceFontFileGUID;
        public int pointSizeSamplingMode;
        public int pointSize;
        public int padding;
        public int packingMode;
        public int atlasWidth;
        public int atlasHeight;
        public int characterSetSelectionMode;
        public string characterSequence;
        public string referencedFontAssetGUID;
        public string referencedTextAssetGUID;
        public int fontStyle;
        public float fontStyleModifier;
        public int renderMode;
        public bool includeFontFeatures;
    }


    public struct KerningPairKey
    {
        public uint ascii_Left;
        public uint ascii_Right;
        public uint key;

        public KerningPairKey(uint ascii_left, uint ascii_right)
        {
            ascii_Left = ascii_left;
            ascii_Right = ascii_right;
            key = (ascii_right << 16) + ascii_left;
        }
    }

    /// <summary>
    /// Positional adjustments of a glyph
    /// </summary>
    [Serializable]
    public struct GlyphValueRecord
    {
        public float xPlacement;
        public float yPlacement;
        public float xAdvance;
        public float yAdvance;

        public static GlyphValueRecord operator +(GlyphValueRecord a, GlyphValueRecord b)
        {
            GlyphValueRecord c;
            c.xPlacement = a.xPlacement + b.xPlacement;
            c.yPlacement = a.yPlacement + b.yPlacement;
            c.xAdvance = a.xAdvance + b.xAdvance;
            c.yAdvance = a.yAdvance + b.yAdvance;

            return c;
        }
    }


    [Serializable]
    public class KerningPair
    {
        /// <summary>
        /// The first glyph part of a kerning pair.
        /// </summary>
        public uint firstGlyph
    {
            get { return m_FirstGlyph; }
            set { m_FirstGlyph = value; }
        }
        [FormerlySerializedAs("AscII_Left")]
        [SerializeField]
        private uint m_FirstGlyph;

        /// <summary>
        /// The positional adjustment of the first glyph.
        /// </summary>
        public GlyphValueRecord firstGlyphAdjustments
        {
            get { return m_FirstGlyphAdjustments; }
        }
        [SerializeField]
        private GlyphValueRecord m_FirstGlyphAdjustments;

        /// <summary>
        /// The second glyph part of a kerning pair.
        /// </summary>
        public uint secondGlyph
        {
            get { return m_SecondGlyph; }
            set { m_SecondGlyph = value; }
        }
        [FormerlySerializedAs("AscII_Right")]
        [SerializeField]
        private uint m_SecondGlyph;

        /// <summary>
        /// The positional adjustment of the second glyph.
        /// </summary>
        public GlyphValueRecord secondGlyphAdjustments
        {
            get { return m_SecondGlyphAdjustments; }
    }
        [SerializeField]
        private GlyphValueRecord m_SecondGlyphAdjustments;

        [FormerlySerializedAs("XadvanceOffset")]
        public float xOffset;


        public KerningPair()
        {
            m_FirstGlyph = 0;
            m_FirstGlyphAdjustments = new GlyphValueRecord();

            m_SecondGlyph = 0;
            m_SecondGlyphAdjustments = new GlyphValueRecord();
        }

        public KerningPair(uint left, uint right, float offset)
    {
            firstGlyph = left;
            m_SecondGlyph = right;
            xOffset = offset;
        }

        public KerningPair(uint firstGlyph, GlyphValueRecord firstGlyphAdjustments, uint secondGlyph, GlyphValueRecord secondGlyphAdjustments)
        {
            m_FirstGlyph = firstGlyph;
            m_FirstGlyphAdjustments = firstGlyphAdjustments;
            m_SecondGlyph = secondGlyph;
            m_SecondGlyphAdjustments = secondGlyphAdjustments;
        }

        internal void ConvertLegacyKerningData()
        {
            m_FirstGlyphAdjustments.xAdvance = xOffset;
            //xOffset = 0;
        }

    }


    [Serializable]
    public class KerningTable
    {
        public List<KerningPair> kerningPairs;


        public KerningTable()
        {
            kerningPairs = new List<KerningPair>();
        }


        public void AddKerningPair()
        {
            if (kerningPairs.Count == 0)
            {
                kerningPairs.Add(new KerningPair(0, 0, 0));
            }
            else
            {
                uint left = kerningPairs.Last().firstGlyph;
                uint right = kerningPairs.Last().secondGlyph;
                float xoffset = kerningPairs.Last().xOffset;

                kerningPairs.Add(new KerningPair(left, right, xoffset));
            }
        }


        /// <summary>
        /// Add Kerning Pair
        /// </summary>
        /// <param name="first">First glyph</param>
        /// <param name="second">Second glyph</param>
        /// <param name="offset">xAdvance value</param>
        /// <returns></returns>
        public int AddKerningPair(uint first, uint second, float offset)
        {
            int index = kerningPairs.FindIndex(item => item.firstGlyph == first && item.secondGlyph == second);

            if (index == -1)
            {
                kerningPairs.Add(new KerningPair(first, second, offset));
                return 0;
            }

            // Return -1 if Kerning Pair already exists.
            return -1;
        }

        /// <summary>
        /// Add Glyph pair adjustment record
        /// </summary>
        /// <param name="firstGlyph">The first glyph</param>
        /// <param name="firstGlyphAdjustments">Adjustment record for the first glyph</param>
        /// <param name="secondGlyph">The second glyph</param>
        /// <param name="secondGlyphAdjustments">Adjustment record for the second glyph</param>
        /// <returns></returns>
        public int AddGlyphPairAdjustmentRecord(uint first, GlyphValueRecord firstAdjustments, uint second, GlyphValueRecord secondAdjustments)
        {
            int index = kerningPairs.FindIndex(item => item.firstGlyph == first && item.secondGlyph == second);

            if (index == -1)
            {
                kerningPairs.Add(new KerningPair(first, firstAdjustments, second, secondAdjustments));
                return 0;
            }

            // Return -1 if Kerning Pair already exists.
            return -1;
        }

        public void RemoveKerningPair(int left, int right)
        {
            int index = kerningPairs.FindIndex(item => item.firstGlyph == left && item.secondGlyph == right);

            if (index != -1)
                kerningPairs.RemoveAt(index);
        }


        public void RemoveKerningPair(int index)
        {
            kerningPairs.RemoveAt(index);
        }


        public void SortKerningPairs()
        {
            // Sort List of Kerning Info
            if (kerningPairs.Count > 0)
                kerningPairs = kerningPairs.OrderBy(s => s.firstGlyph).ThenBy(s => s.secondGlyph).ToList();
        }
    }


    public static class TMP_FontUtilities
    {
        private static List<int> k_searchedFontAssets;

        /// <summary>
        /// Search through the given font and its fallbacks for the specified character.
        /// </summary>
        /// <param name="font">The font asset to search for the given character.</param>
        /// <param name="character">The character to find.</param>
        /// <param name="glyph">out parameter containing the glyph for the specified character (if found).</param>
        /// <returns></returns>
        public static TMP_FontAsset SearchForGlyph(TMP_FontAsset font, int character, out TMP_Glyph glyph)
        {
            if (k_searchedFontAssets == null)
                k_searchedFontAssets = new List<int>();

            k_searchedFontAssets.Clear();

            return SearchForGlyphInternal(font, character, out glyph);
        }


        /// <summary>
        /// Search through the given list of fonts and their possible fallbacks for the specified character.
        /// </summary>
        /// <param name="fonts"></param>
        /// <param name="character"></param>
        /// <param name="glyph"></param>
        /// <returns></returns>
        public static TMP_FontAsset SearchForGlyph(List<TMP_FontAsset> fonts, int character, out TMP_Glyph glyph)
        {
            return SearchForGlyphInternal(fonts, character, out glyph);
        }


        private static TMP_FontAsset SearchForGlyphInternal (TMP_FontAsset font, int character, out TMP_Glyph glyph)
        {
            glyph = null;

            if (font == null) return null;

            if (font.characterDictionary.TryGetValue(character, out glyph))
            {
                return font;
            }
            else if (font.fallbackFontAssets != null && font.fallbackFontAssets.Count > 0)
            {
                for (int i = 0; i < font.fallbackFontAssets.Count && glyph == null; i++)
                {
                    TMP_FontAsset temp = font.fallbackFontAssets[i];
                    if (temp == null) continue;

                    int id = temp.GetInstanceID();

                    // Skip over the fallback font asset in the event it is null or if already searched.
                    if (k_searchedFontAssets.Contains(id)) continue;

                    // Add to list of font assets already searched.
                    k_searchedFontAssets.Add(id);

                    temp = SearchForGlyphInternal(temp, character, out glyph);

                    if (temp != null)
                        return temp;
                }
            }

            return null;
        }


        private static TMP_FontAsset SearchForGlyphInternal(List<TMP_FontAsset> fonts, int character, out TMP_Glyph glyph)
        {
            glyph = null;

            if (fonts != null && fonts.Count > 0)
            {
                for (int i = 0; i < fonts.Count; i++)
                {
                    TMP_FontAsset fontAsset = SearchForGlyphInternal(fonts[i], character, out glyph);

                    if (fontAsset != null)
                        return fontAsset;
                }
            }

            return null;
        }
    }



}