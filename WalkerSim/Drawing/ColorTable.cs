namespace WalkerSim
{
    public static partial class Drawing
    {
        public static class ColorTable
        {
            static readonly string[] ColorValues = new string[] {
                "FF0000", "00FF00", "0000FF", "FFFF00", "FF00FF", "00FFFF", "000000",
                "800000", "008000", "000080", "808000", "800080", "008080", "808080",
                "C00000", "00C000", "0000C0", "C0C000", "C000C0", "00C0C0", "C0C0C0",
                "400000", "004000", "000040", "404000", "400040", "004040", "404040",
                "200000", "002000", "000020", "202000", "200020", "002020", "202020",
                "600000", "006000", "000060", "606000", "600060", "006060", "606060",
                "A00000", "00A000", "0000A0", "A0A000", "A000A0", "00A0A0", "A0A0A0",
                "E00000", "00E000", "0000E0", "E0E000", "E000E0", "00E0E0", "E0E0E0",
            };

            public static Drawing.Color[] Colors;

            static ColorTable()
            {
                Colors = new Drawing.Color[ColorValues.Length];

                for (int i = 0; i < Colors.Length; i++)
                {
                    Colors[i] = Drawing.Color.FromHtml(ColorValues[i]);
                }
            }

            public static Drawing.Color GetColorForIndex(int index)
            {
                return Colors[index % Colors.Length];
            }
        }
    }

}
