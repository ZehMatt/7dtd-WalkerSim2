using System;
using System.Collections.Generic;

namespace WalkerSim
{
    public static partial class Drawing
    {
        public struct Color : IEquatable<Color>
        {
            public Color(byte r, byte g, byte b)
            {
                R = r;
                G = g;
                B = b;
                A = 255;
            }
            public Color(byte r, byte g, byte b, byte a)
            {
                R = r;
                G = g;
                B = b;
                A = a;
            }

            public bool Equals(Color other)
            {
                return (R == other.R && G == other.G && B == other.B && A == other.A);
            }

            public override bool Equals(object obj) => Equals((Color)obj);

            public static bool operator ==(Color a, Color b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(Color a, Color b)
            {
                return !a.Equals(b);
            }

            public override int GetHashCode()
            {
                return A.GetHashCode() ^ B.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode();
            }

            public byte R;
            public byte G;
            public byte B;
            public byte A;

            public static Color Transparent = new Color(0, 0, 0, 0);
            public static Color Black = new Color(0, 0, 0, 255);
            public static Color White = new Color(255, 255, 255, 255);
            public static Color AliceBlue = new Color(240, 248, 255);
            public static Color AntiqueWhite = new Color(250, 235, 215);
            public static Color Aqua = new Color(0, 255, 255);
            public static Color Aquamarine = new Color(127, 255, 212);
            public static Color Azure = new Color(240, 255, 255);
            public static Color Beige = new Color(245, 245, 220);
            public static Color Bisque = new Color(255, 228, 196);
            public static Color BlanchedAlmond = new Color(255, 235, 205);
            public static Color Blue = new Color(0, 0, 255);
            public static Color BlueViolet = new Color(138, 43, 226);
            public static Color Brown = new Color(165, 42, 42);
            public static Color BurlyWood = new Color(222, 184, 135);
            public static Color CadetBlue = new Color(95, 158, 160);
            public static Color Chartreuse = new Color(127, 255, 0);
            public static Color Chocolate = new Color(210, 105, 30);
            public static Color Coral = new Color(255, 127, 80);
            public static Color CornflowerBlue = new Color(100, 149, 237);
            public static Color Cornsilk = new Color(255, 248, 220);
            public static Color Crimson = new Color(220, 20, 60);
            public static Color Cyan = new Color(0, 255, 255);
            public static Color DarkBlue = new Color(0, 0, 139);
            public static Color DarkCyan = new Color(0, 139, 139);
            public static Color DarkGoldenRod = new Color(184, 134, 11);
            public static Color DarkGray = new Color(169, 169, 169);
            public static Color DarkGreen = new Color(0, 100, 0);
            public static Color DarkKhaki = new Color(189, 183, 107);
            public static Color DarkMagenta = new Color(139, 0, 139);
            public static Color DarkOliveGreen = new Color(85, 107, 47);
            public static Color DarkOrange = new Color(255, 140, 0);
            public static Color DarkOrchid = new Color(153, 50, 204);
            public static Color DarkRed = new Color(139, 0, 0);
            public static Color DarkSalmon = new Color(233, 150, 122);
            public static Color DarkSeaGreen = new Color(143, 188, 143);
            public static Color DarkSlateBlue = new Color(72, 61, 139);
            public static Color DarkSlateGray = new Color(47, 79, 79);
            public static Color DarkTurquoise = new Color(0, 206, 209);
            public static Color DarkViolet = new Color(148, 0, 211);
            public static Color DeepPink = new Color(255, 20, 147);
            public static Color DeepSkyBlue = new Color(0, 191, 255);
            public static Color DimGray = new Color(105, 105, 105);
            public static Color DodgerBlue = new Color(30, 144, 255);
            public static Color FireBrick = new Color(178, 34, 34);
            public static Color FloralWhite = new Color(255, 250, 240);
            public static Color ForestGreen = new Color(34, 139, 34);
            public static Color Fuchsia = new Color(255, 0, 255);
            public static Color Gainsboro = new Color(220, 220, 220);
            public static Color GhostWhite = new Color(248, 248, 255);
            public static Color Gold = new Color(255, 215, 0);
            public static Color GoldenRod = new Color(218, 165, 32);
            public static Color Gray = new Color(128, 128, 128);
            public static Color Green = new Color(0, 128, 0);
            public static Color GreenYellow = new Color(173, 255, 47);
            public static Color HoneyDew = new Color(240, 255, 240);
            public static Color HotPink = new Color(255, 105, 180);
            public static Color IndianRed = new Color(205, 92, 92);
            public static Color Indigo = new Color(75, 0, 130);
            public static Color Ivory = new Color(255, 255, 240);
            public static Color Khaki = new Color(240, 230, 140);
            public static Color Lavender = new Color(230, 230, 250);
            public static Color LavenderBlush = new Color(255, 240, 245);
            public static Color LawnGreen = new Color(124, 252, 0);
            public static Color LemonChiffon = new Color(255, 250, 205);
            public static Color LightBlue = new Color(173, 216, 230);
            public static Color LightCoral = new Color(240, 128, 128);
            public static Color LightCyan = new Color(224, 255, 255);
            public static Color LightGoldenRodYellow = new Color(250, 250, 210);
            public static Color LightGray = new Color(211, 211, 211);
            public static Color LightGreen = new Color(144, 238, 144);
            public static Color LightPink = new Color(255, 182, 193);
            public static Color LightSalmon = new Color(255, 160, 122);
            public static Color LightSeaGreen = new Color(32, 178, 170);
            public static Color LightSkyBlue = new Color(135, 206, 250);
            public static Color LightSlateGray = new Color(119, 136, 153);
            public static Color LightSteelBlue = new Color(176, 196, 222);
            public static Color LightYellow = new Color(255, 255, 224);
            public static Color Lime = new Color(0, 255, 0);
            public static Color LimeGreen = new Color(50, 205, 50);
            public static Color Linen = new Color(250, 240, 230);
            public static Color Magenta = new Color(255, 0, 255);
            public static Color Maroon = new Color(128, 0, 0);
            public static Color MediumAquaMarine = new Color(102, 205, 170);
            public static Color MediumBlue = new Color(0, 0, 205);
            public static Color MediumOrchid = new Color(186, 85, 211);
            public static Color MediumPurple = new Color(147, 112, 219);
            public static Color MediumSeaGreen = new Color(60, 179, 113);
            public static Color MediumSlateBlue = new Color(123, 104, 238);
            public static Color MediumSpringGreen = new Color(0, 250, 154);
            public static Color MediumTurquoise = new Color(72, 209, 204);
            public static Color MediumVioletRed = new Color(199, 21, 133);
            public static Color MidnightBlue = new Color(25, 25, 112);
            public static Color MintCream = new Color(245, 255, 250);
            public static Color MistyRose = new Color(255, 228, 225);
            public static Color Moccasin = new Color(255, 228, 181);
            public static Color NavajoWhite = new Color(255, 222, 173);
            public static Color Navy = new Color(0, 0, 128);
            public static Color OldLace = new Color(253, 245, 230);
            public static Color Olive = new Color(128, 128, 0);
            public static Color OliveDrab = new Color(107, 142, 35);
            public static Color Orange = new Color(255, 165, 0);
            public static Color OrangeRed = new Color(255, 69, 0);
            public static Color Orchid = new Color(218, 112, 214);
            public static Color PaleGoldenRod = new Color(238, 232, 170);
            public static Color PaleGreen = new Color(152, 251, 152);
            public static Color PaleTurquoise = new Color(175, 238, 238);
            public static Color PaleVioletRed = new Color(219, 112, 147);
            public static Color PapayaWhip = new Color(255, 239, 213);
            public static Color PeachPuff = new Color(255, 218, 185);
            public static Color Peru = new Color(205, 133, 63);
            public static Color Pink = new Color(255, 192, 203);
            public static Color Plum = new Color(221, 160, 221);
            public static Color PowderBlue = new Color(176, 224, 230);
            public static Color Purple = new Color(128, 0, 128);
            public static Color Red = new Color(255, 0, 0);
            public static Color RosyBrown = new Color(188, 143, 143);
            public static Color RoyalBlue = new Color(65, 105, 225);
            public static Color SaddleBrown = new Color(139, 69, 19);
            public static Color Salmon = new Color(250, 128, 114);
            public static Color SandyBrown = new Color(244, 164, 96);
            public static Color SeaGreen = new Color(46, 139, 87);
            public static Color SeaShell = new Color(255, 245, 238);
            public static Color Sienna = new Color(160, 82, 45);
            public static Color Silver = new Color(192, 192, 192);
            public static Color SkyBlue = new Color(135, 206, 235);
            public static Color SlateBlue = new Color(106, 90, 205);
            public static Color SlateGray = new Color(112, 128, 144);
            public static Color Snow = new Color(255, 250, 250);
            public static Color SpringGreen = new Color(0, 255, 127);
            public static Color SteelBlue = new Color(70, 130, 180);
            public static Color Tan = new Color(210, 180, 140);
            public static Color Teal = new Color(0, 128, 128);
            public static Color Thistle = new Color(216, 191, 216);
            public static Color Tomato = new Color(255, 99, 71);
            public static Color Turquoise = new Color(64, 224, 208);
            public static Color Violet = new Color(238, 130, 238);
            public static Color Wheat = new Color(245, 222, 179);
            public static Color WhiteSmoke = new Color(245, 245, 245);
            public static Color Yellow = new Color(255, 255, 0);
            public static Color YellowGreen = new Color(154, 205, 50);

            private static readonly Dictionary<string, Color> HtmlColorNames =
                new Dictionary<string, Color>(System.StringComparer.OrdinalIgnoreCase)
            {
                { "AliceBlue", Color.AliceBlue },
                { "AntiqueWhite", Color.AntiqueWhite },
                { "Aqua", Color.Aqua },
                { "Aquamarine", Color.Aquamarine },
                { "Azure", Color.Azure },
                { "Beige", Color.Beige },
                { "Bisque", Color.Bisque },
                { "Black", Color.Black },
                { "BlanchedAlmond", Color.BlanchedAlmond },
                { "Blue", Color.Blue },
                { "BlueViolet", Color.BlueViolet },
                { "Brown", Color.Brown },
                { "BurlyWood", Color.BurlyWood },
                { "CadetBlue", Color.CadetBlue },
                { "Chartreuse", Color.Chartreuse },
                { "Chocolate", Color.Chocolate },
                { "Coral", Color.Coral },
                { "CornflowerBlue", Color.CornflowerBlue },
                { "Cornsilk", Color.Cornsilk },
                { "Crimson", Color.Crimson },
                { "Cyan", Color.Cyan },
                { "DarkBlue", Color.DarkBlue },
                { "DarkCyan", Color.DarkCyan },
                { "DarkGoldenRod", Color.DarkGoldenRod },
                { "DarkGray", Color.DarkGray },
                { "DarkGreen", Color.DarkGreen },
                { "DarkKhaki", Color.DarkKhaki },
                { "DarkMagenta", Color.DarkMagenta },
                { "DarkOliveGreen", Color.DarkOliveGreen },
                { "DarkOrange", Color.DarkOrange },
                { "DarkOrchid", Color.DarkOrchid },
                { "DarkRed", Color.DarkRed },
                { "DarkSalmon", Color.DarkSalmon },
                { "DarkSeaGreen", Color.DarkSeaGreen },
                { "DarkSlateBlue", Color.DarkSlateBlue },
                { "DarkSlateGray", Color.DarkSlateGray },
                { "DarkTurquoise", Color.DarkTurquoise },
                { "DarkViolet", Color.DarkViolet },
                { "DeepPink", Color.DeepPink },
                { "DeepSkyBlue", Color.DeepSkyBlue },
                { "DimGray", Color.DimGray },
                { "DodgerBlue", Color.DodgerBlue },
                { "FireBrick", Color.FireBrick },
                { "FloralWhite", Color.FloralWhite },
                { "ForestGreen", Color.ForestGreen },
                { "Fuchsia", Color.Fuchsia },
                { "Gainsboro", Color.Gainsboro },
                { "GhostWhite", Color.GhostWhite },
                { "Gold", Color.Gold },
                { "GoldenRod", Color.GoldenRod },
                { "Gray", Color.Gray },
                { "Green", Color.Green },
                { "GreenYellow", Color.GreenYellow },
                { "HoneyDew", Color.HoneyDew },
                { "HotPink", Color.HotPink },
                { "IndianRed", Color.IndianRed },
                { "Indigo", Color.Indigo },
                { "Ivory", Color.Ivory },
                { "Khaki", Color.Khaki },
                { "Lavender", Color.Lavender },
                { "LavenderBlush", Color.LavenderBlush },
                { "LawnGreen", Color.LawnGreen },
                { "LemonChiffon", Color.LemonChiffon },
                { "LightBlue", Color.LightBlue },
                { "LightCoral", Color.LightCoral },
                { "LightCyan", Color.LightCyan },
                { "LightGoldenRodYellow", Color.LightGoldenRodYellow },
                { "LightGray", Color.LightGray },
                { "LightGreen", Color.LightGreen },
                { "LightPink", Color.LightPink },
                { "LightSalmon", Color.LightSalmon },
                { "LightSeaGreen", Color.LightSeaGreen },
                { "LightSkyBlue", Color.LightSkyBlue },
                { "LightSlateGray", Color.LightSlateGray },
                { "LightSteelBlue", Color.LightSteelBlue },
                { "LightYellow", Color.LightYellow },
                { "Lime", Color.Lime },
                { "LimeGreen", Color.LimeGreen },
                { "Linen", Color.Linen },
                { "Magenta", Color.Magenta },
                { "Maroon", Color.Maroon },
                { "MediumAquaMarine", Color.MediumAquaMarine },
                { "MediumBlue", Color.MediumBlue },
                { "MediumOrchid", Color.MediumOrchid },
                { "MediumPurple", Color.MediumPurple },
                { "MediumSeaGreen", Color.MediumSeaGreen },
                { "MediumSlateBlue", Color.MediumSlateBlue },
                { "MediumSpringGreen", Color.MediumSpringGreen },
                { "MediumTurquoise", Color.MediumTurquoise },
                { "MediumVioletRed", Color.MediumVioletRed },
                { "MidnightBlue", Color.MidnightBlue },
                { "MintCream", Color.MintCream },
                { "MistyRose", Color.MistyRose },
                { "Moccasin", Color.Moccasin },
                { "NavajoWhite", Color.NavajoWhite },
                { "Navy", Color.Navy },
                { "OldLace", Color.OldLace },
                { "Olive", Color.Olive },
                { "OliveDrab", Color.OliveDrab },
                { "Orange", Color.Orange },
                { "OrangeRed", Color.OrangeRed },
                { "Orchid", Color.Orchid },
                { "PaleGoldenRod", Color.PaleGoldenRod },
                { "PaleGreen", Color.PaleGreen },
                { "PaleTurquoise", Color.PaleTurquoise },
                { "PaleVioletRed", Color.PaleVioletRed },
                { "PapayaWhip", Color.PapayaWhip },
                { "PeachPuff", Color.PeachPuff },
                { "Peru", Color.Peru },
                { "Pink", Color.Pink },
                { "Plum", Color.Plum },
                { "PowderBlue", Color.PowderBlue },
                { "Purple", Color.Purple },
                { "Red", Color.Red },
                { "RosyBrown", Color.RosyBrown },
                { "RoyalBlue", Color.RoyalBlue },
                { "SaddleBrown", Color.SaddleBrown },
                { "Salmon", Color.Salmon },
                { "SandyBrown", Color.SandyBrown },
                { "SeaGreen", Color.SeaGreen },
                { "SeaShell", Color.SeaShell },
                { "Sienna", Color.Sienna },
                { "Silver", Color.Silver },
                { "SkyBlue", Color.SkyBlue },
                { "SlateBlue", Color.SlateBlue },
                { "SlateGray", Color.SlateGray },
                { "Snow", Color.Snow },
                { "SpringGreen", Color.SpringGreen },
                { "SteelBlue", Color.SteelBlue },
                { "Tan", Color.Tan },
                { "Teal", Color.Teal },
                { "Thistle", Color.Thistle },
                { "Tomato", Color.Tomato },
                { "Turquoise", Color.Turquoise },
                { "Violet", Color.Violet },
                { "Wheat", Color.Wheat },
                { "White", Color.White },
                { "WhiteSmoke", Color.WhiteSmoke },
                { "Yellow", Color.Yellow },
                { "YellowGreen", Color.YellowGreen }
            };

            public static Color FromHtml(string value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new System.ArgumentException("HTML color string cannot be null or empty.");
                }

                // Check if its a named color.
                if (HtmlColorNames.TryGetValue(value, out var color))
                {
                    return color;
                }

                // Remove '#' if present
                string hex = value.StartsWith("#") ? value.Substring(1) : value;

                // Ensure the hex string is either 6 or 8 characters long
                if (hex.Length != 6 && hex.Length != 8)
                {
                    throw new System.ArgumentException("Invalid HTML color string. Expected format: #RRGGBB or #RRGGBBAA.");
                }

                try
                {
                    // Parse the hex string
                    var r = System.Convert.ToByte(hex.Substring(0, 2), 16);
                    var g = System.Convert.ToByte(hex.Substring(2, 2), 16);
                    var b = System.Convert.ToByte(hex.Substring(4, 2), 16);
                    var a = hex.Length == 8 ? System.Convert.ToByte(hex.Substring(6, 2), 16) : (byte)255;

                    return new Color(r, g, b, a);
                }
                catch (System.FormatException ex)
                {
                    throw new System.ArgumentException("Invalid HTML color string format.", ex);
                }
            }

            public static string ToHtml(Color color)
            {
                // Check if we can use a named color first.
                foreach (var namedKv in HtmlColorNames)
                {
                    if (namedKv.Value.Equals(color))
                    {
                        return namedKv.Key;
                    }
                }

                // Convert to hex and ensure two digits per component
                string hexR = color.R.ToString("X2");
                string hexG = color.G.ToString("X2");
                string hexB = color.B.ToString("X2");

                // Include alpha only if it's not 255
                if (color.A == 255)
                {
                    return $"#{hexR}{hexG}{hexB}";
                }

                string hexA = color.A.ToString("X2");
                return $"#{hexR}{hexG}{hexB}{hexA}";
            }

            public string ToHtml()
            {
                return ToHtml(this);
            }
        }
    }
}
