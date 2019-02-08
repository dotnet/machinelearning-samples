using UnityEngine;
using System;
using System.Collections.Generic;


namespace TMPro
{
    /// <summary>
    /// Flags to control what vertex data is pushed to the mesh and renderer.
    /// </summary>
    public enum TMP_VertexDataUpdateFlags
    {
        None = 0x0,
        Vertices = 0x1,
        Uv0 = 0x2,
        Uv2 = 0x4,
        Uv4 = 0x8,
        Colors32 = 0x10,
        All = 0xFF
    };


    /// <summary>
    /// TMP custom data type to represent 32 bit characters.
    /// </summary>
    //public struct TMP_Char
    //{
    //    private int m_value;

    //    private TMP_Char(int value)
    //    {
    //        this.m_value = value;
    //    }

    //    private TMP_Char(TMP_Char value)
    //    {
    //        this.m_value = (int)value;
    //    }

    //    public static implicit operator TMP_Char(int value)
    //    {
    //        return new TMP_Char(value);
    //    }

    //    public static implicit operator TMP_Char(char c)
    //    {
    //        return new TMP_Char(c);
    //    }

    //    public static explicit operator int(TMP_Char value)
    //    {
    //        return value.m_value;
    //    }

    //    public override string ToString()
    //    {
    //        return m_value.ToString();
    //    }
    //}


    /// <summary>
    /// Structure containing information about individual text elements (character or sprites).
    /// </summary>
    //[Serializable]
    public struct TMP_CharacterInfo
    {
        public char character; // Should be changed to an int to handle UTF 32
        /// <summary>
        /// Index of the character in the raw string.
        /// </summary>
        public int index; // Index of the character in the input string.
        public TMP_TextElementType elementType;

        public TMP_TextElement textElement;
        public TMP_FontAsset fontAsset;
        public TMP_SpriteAsset spriteAsset;
        public int spriteIndex;
        public Material material;
        public int materialReferenceIndex;
        public bool isUsingAlternateTypeface;

        public float pointSize;
        
        //public short wordNumber;
        public int lineNumber;
        //public short charNumber;
        public int pageNumber;


        public int vertexIndex;
        public TMP_Vertex vertex_TL;
        public TMP_Vertex vertex_BL;
        public TMP_Vertex vertex_TR;
        public TMP_Vertex vertex_BR;
        
        public Vector3 topLeft;
        public Vector3 bottomLeft;
        public Vector3 topRight;
        public Vector3 bottomRight;
        public float origin;
        public float ascender;
        public float baseLine;
        public float descender;
        
        public float xAdvance;
        public float aspectRatio;
        public float scale;
        public Color32 color;
        public Color32 underlineColor;
        public Color32 strikethroughColor;
        public Color32 highlightColor;
        public FontStyles style;
        public bool isVisible;
        //public bool isIgnoringAlignment;
    }


    public struct TMP_Vertex
    {      
        public Vector3 position;
        public Vector2 uv;
        public Vector2 uv2;
        public Vector2 uv4;
        public Color32 color;

        //public Vector3 normal;
        //public Vector4 tangent;
    }


    //public struct TMP_VertexInfo
    //{      
    //    public TMP_Vertex topLeft;
    //    public TMP_Vertex bottomLeft;
    //    public TMP_Vertex topRight;
    //    public TMP_Vertex bottomRight;
    //}


    [Serializable]
    public struct VertexGradient
    {
        public Color topLeft;
        public Color topRight;
        public Color bottomLeft;
        public Color bottomRight;

        public VertexGradient (Color color)
        {
            this.topLeft = color;
            this.topRight = color;
            this.bottomLeft = color;
            this.bottomRight = color;
        }

        /// <summary>
        /// The vertex colors at the corners of the characters.
        /// </summary>
        /// <param name="color0">Top left color.</param>
        /// <param name="color1">Top right color.</param>
        /// <param name="color2">Bottom left color.</param>
        /// <param name="color3">Bottom right color.</param>
        public VertexGradient(Color color0, Color color1, Color color2, Color color3)
        {
            this.topLeft = color0;
            this.topRight = color1;
            this.bottomLeft = color2;
            this.bottomRight = color3;
        }
    }


    public struct TMP_PageInfo
    {
        public int firstCharacterIndex;
        public int lastCharacterIndex;
        public float ascender;
        public float baseLine;
        public float descender;
        // public float extents;
    }


    /// <summary>
    /// Structure containing information about individual links contained in the text object.
    /// </summary>
    public struct TMP_LinkInfo
    {
        public TMP_Text textComponent;

        public int hashCode;

        public int linkIdFirstCharacterIndex;
        public int linkIdLength;
        public int linkTextfirstCharacterIndex;
        public int linkTextLength;

        internal char[] linkID;


        internal void SetLinkID(char[] text, int startIndex, int length)
        {
            if (linkID == null || linkID.Length < length) linkID = new char[length];

            for (int i = 0; i < length; i++)
                linkID[i] = text[startIndex + i];
        }

        /// <summary>
        /// Function which returns the text contained in a link.
        /// </summary>
        /// <param name="textInfo"></param>
        /// <returns></returns>
        public string GetLinkText()
        {
            string text = string.Empty;
            TMP_TextInfo textInfo = textComponent.textInfo;

            for (int i = linkTextfirstCharacterIndex; i < linkTextfirstCharacterIndex + linkTextLength; i++)
                text += textInfo.characterInfo[i].character;

            return text;
        }


        /// <summary>
        /// Function which returns the link ID as a string.
        /// </summary>
        /// <param name="text">The source input text.</param>
        /// <returns></returns>
        public string GetLinkID()
        {
            if (textComponent == null)
                return string.Empty;

            return new string(linkID, 0, linkIdLength);
            //return textComponent.text.Substring(linkIdFirstCharacterIndex, linkIdLength);

        }
    }


    /// <summary>
    /// Structure containing information about the individual words contained in the text object.
    /// </summary>
    public struct TMP_WordInfo
    {
        // NOTE: Structure could be simplified by only including the firstCharacterIndex and length.

        public TMP_Text textComponent;

        public int firstCharacterIndex;
        public int lastCharacterIndex;
        public int characterCount;
        //public float length;

        /// <summary>
        /// Returns the word as a string.
        /// </summary>
        /// <returns></returns>
        public string GetWord()
        {
            string word = string.Empty;
            TMP_CharacterInfo[] charInfo = textComponent.textInfo.characterInfo;
            
            for (int i = firstCharacterIndex; i < lastCharacterIndex + 1; i++)
            {
                word += charInfo[i].character;
            }

            return word;
        }
    }


    public struct TMP_SpriteInfo
    {
        public int spriteIndex; // Index of the sprite in the sprite atlas.
        public int characterIndex; // The characterInfo index which holds the key information about this sprite.
        public int vertexIndex;
    }


    //public struct SpriteInfo
    //{
    //    
    //}


    public struct Extents
    {
        public Vector2 min;
        public Vector2 max;

        public Extents(Vector2 min, Vector2 max)
        {
            this.min = min;
            this.max = max;
        }

        public override string ToString()
        {
            string s = "Min (" + min.x.ToString("f2") + ", " + min.y.ToString("f2") + ")   Max (" + max.x.ToString("f2") + ", " + max.y.ToString("f2") + ")";           
            return s;
        }
    }


    [Serializable]
    public struct Mesh_Extents
    {
        public Vector2 min;
        public Vector2 max;
      
     
        public Mesh_Extents(Vector2 min, Vector2 max)
        {
            this.min = min;
            this.max = max;
        }

        public override string ToString()
        {
            string s = "Min (" + min.x.ToString("f2") + ", " + min.y.ToString("f2") + ")   Max (" + max.x.ToString("f2") + ", " + max.y.ToString("f2") + ")";
            //string s = "Center: (" + ")" + "  Extents: (" + ((max.x - min.x) / 2).ToString("f2") + "," + ((max.y - min.y) / 2).ToString("f2") + ").";
            return s;
        }
    }


    // Structure used for Word Wrapping which tracks the state of execution when the last space or carriage return character was encountered. 
    public struct WordWrapState
    {
        public int previous_WordBreak;
        public int total_CharacterCount;
        public int visible_CharacterCount;
        public int visible_SpriteCount;
        public int visible_LinkCount;
        public int firstCharacterIndex;
        public int firstVisibleCharacterIndex;
        public int lastCharacterIndex;
        public int lastVisibleCharIndex;
        public int lineNumber;

        public float maxCapHeight;
        public float maxAscender;
        public float maxDescender;
        public float maxLineAscender;
        public float maxLineDescender;
        public float previousLineAscender;

        public float xAdvance;
        public float preferredWidth;
        public float preferredHeight;
        //public float maxFontScale;
        public float previousLineScale;
      
        public int wordCount;
        public FontStyles fontStyle;
        public float fontScale;
        public float fontScaleMultiplier;
      
        public float currentFontSize;
        public float baselineOffset;
        public float lineOffset;

        public TMP_TextInfo textInfo;
        //public TMPro_CharacterInfo[] characterInfo;
        public TMP_LineInfo lineInfo;
        
        public Color32 vertexColor;
        public Color32 underlineColor;
        public Color32 strikethroughColor;
        public Color32 highlightColor;
        public TMP_BasicXmlTagStack basicStyleStack;
        public TMP_XmlTagStack<Color32> colorStack;
        public TMP_XmlTagStack<Color32> underlineColorStack;
        public TMP_XmlTagStack<Color32> strikethroughColorStack;
        public TMP_XmlTagStack<Color32> highlightColorStack;
        public TMP_XmlTagStack<TMP_ColorGradient> colorGradientStack;
        public TMP_XmlTagStack<float> sizeStack;
        public TMP_XmlTagStack<float> indentStack;
        public TMP_XmlTagStack<int> fontWeightStack;
        public TMP_XmlTagStack<int> styleStack;
        public TMP_XmlTagStack<float> baselineStack;
        public TMP_XmlTagStack<int> actionStack;
        public TMP_XmlTagStack<MaterialReference> materialReferenceStack;
        public TMP_XmlTagStack<TextAlignmentOptions> lineJustificationStack;
        //public TMP_XmlTagStack<int> spriteAnimationStack;
        public int spriteAnimationID;

        public TMP_FontAsset currentFontAsset;
        public TMP_SpriteAsset currentSpriteAsset;
        public Material currentMaterial;
        public int currentMaterialIndex;

        public Extents meshExtents;

        public bool tagNoParsing;
        public bool isNonBreakingSpace;
        //public Mesh_Extents lineExtents;
    }


    /// <summary>
    /// Structure used to store retrieve the name and hashcode of the font and material
    /// </summary>
    public struct TagAttribute
    {
        public int startIndex;
        public int length;
        public int hashCode;
    }


    public struct XML_TagAttribute
    {
        public int nameHashCode;
        public TagType valueType;
        public int valueStartIndex;
        public int valueLength;
        public int valueHashCode;
    }

}
