

using UnityEngine;

public static class PlayerColors {
    public enum PlayerColor : int {
        White = 0,
        Black = 1,
        Red = 2,
        Green = 3,
        Blue = 4,
        Yellow = 5,
        Gray = 6,
        Magenta = 7,
        Cyan = 8

    }
    public static Color ToColor(this string color) {
        return (Color)typeof(Color).GetProperty(color.ToLowerInvariant()).GetValue(null, null);
    }
}