//
// Color Helper
//
// Author     : Alex Tuduran
// Copyright  : OmniSAR Technologies
//

using UnityEngine;

namespace OmniSARTechnologies.Helper {
    static public class ColorHelper {
        static public Color HexStrToColor(string hexStrColor, Color fallbackColor = default(Color)) {
            if (hexStrColor.Length < 1) {
                return fallbackColor;
            }

            hexStrColor = hexStrColor.
                Replace("#", "").
                Replace("0x", "").
                Replace("$", "");

            if (hexStrColor.Length < 0) {
                return fallbackColor;
            }

            return new Color32(
                (hexStrColor.Length > 1) ? byte.Parse(hexStrColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) : (byte)0x00,
                (hexStrColor.Length > 3) ? byte.Parse(hexStrColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) : (byte)0x00,
                (hexStrColor.Length > 5) ? byte.Parse(hexStrColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) : (byte)0x00,
                (hexStrColor.Length > 7) ? byte.Parse(hexStrColor.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) : (byte)0xFF
            );
        }

        static public string ColorText(string text, Color color) {
            return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + text + "</color>";
        }
    }
}
