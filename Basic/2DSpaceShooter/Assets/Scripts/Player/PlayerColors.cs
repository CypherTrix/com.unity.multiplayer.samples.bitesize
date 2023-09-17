using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public static class PlayerColors {

    public static Color StringToColor(this string colorStr) {
        try {
            return (Color)typeof(Color).GetProperty(colorStr.ToLowerInvariant()).GetValue(null, null);
        } catch (System.NullReferenceException) {
            Debug.LogError($"Parse Error Color : {colorStr}");
            return Color.white;
        }
    }

    public enum PlayerColorNames : int {
        White,
        Black,
        Lime,
        Green,
        Aqua,
        Blue,
        Navy,
        Purple,
        Red,
        Orange,
        Yellow
    }

    private static readonly Dictionary<PlayerColorNames, Color> playercolorcollection = new() {
        { PlayerColorNames.White,  Color.white },
        { PlayerColorNames.Black,  Color.black },
        { PlayerColorNames.Lime,   new Color( 0.043f, 0.851f, 0.396f,1) },
        { PlayerColorNames.Green,  Color.green },
        { PlayerColorNames.Aqua,   Color.cyan },
        { PlayerColorNames.Blue,   Color.blue },
        { PlayerColorNames.Navy,   new Color(0.059f,0.435f,0.839f,1f) },
        { PlayerColorNames.Purple, Color.magenta },
        { PlayerColorNames.Red,    Color.red },
        { PlayerColorNames.Orange, new Color( 0.98f, 0.655f, 0f,1f) },
        { PlayerColorNames.Yellow, Color.yellow },
    };

    public static Color GetPlayerColor(PlayerColorNames colorName) {
        return playercolorcollection[colorName];
    }
    public static PlayerColorNames GetPlayerColorNameByColor(Color color) {
        return  playercolorcollection.FirstOrDefault(x => x.Value.ToString() == color.ToString()).Key;
    }
}