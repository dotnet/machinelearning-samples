using UnityEngine;
using System.Collections;


namespace TMPro
{
    // Class used to convert scenes and objects saved in version 0.1.44 to the new Text Container
    public static class TMP_Compatibility
    {
        public enum AnchorPositions { TopLeft, Top, TopRight, Left, Center, Right, BottomLeft, Bottom, BottomRight, BaseLine, None };  
        
        /// <summary>
        /// Function used to convert text alignment option enumeration format.
        /// </summary>
        /// <param name="oldValue"></param>
        /// <returns></returns>
        public static TextAlignmentOptions ConvertTextAlignmentEnumValues(TextAlignmentOptions oldValue)
        {
            switch ((int)oldValue)
            {
                case 0:
                    return TextAlignmentOptions.TopLeft;
                case 1:
                    return TextAlignmentOptions.Top;
                case 2:
                    return TextAlignmentOptions.TopRight;
                case 3:
                    return TextAlignmentOptions.TopJustified;
                case 4:
                    return TextAlignmentOptions.Left;
                case 5:
                    return TextAlignmentOptions.Center;
                case 6:
                    return TextAlignmentOptions.Right;
                case 7:
                    return TextAlignmentOptions.Justified;
                case 8:
                    return TextAlignmentOptions.BottomLeft;
                case 9:
                    return TextAlignmentOptions.Bottom;
                case 10:
                    return TextAlignmentOptions.BottomRight;
                case 11:
                    return TextAlignmentOptions.BottomJustified;
                case 12:
                    return TextAlignmentOptions.BaselineLeft;
                case 13:
                    return TextAlignmentOptions.Baseline;
                case 14:
                    return TextAlignmentOptions.BaselineRight;
                case 15:
                    return TextAlignmentOptions.BaselineJustified;
                case 16:
                    return TextAlignmentOptions.MidlineLeft;
                case 17:
                    return TextAlignmentOptions.Midline;
                case 18:
                    return TextAlignmentOptions.MidlineRight;
                case 19:
                    return TextAlignmentOptions.MidlineJustified;
                case 20:
                    return TextAlignmentOptions.CaplineLeft;
                case 21:
                    return TextAlignmentOptions.Capline;
                case 22:
                    return TextAlignmentOptions.CaplineRight;
                case 23:
                    return TextAlignmentOptions.CaplineJustified;
            }

            return TextAlignmentOptions.TopLeft;
        }
    }
}
