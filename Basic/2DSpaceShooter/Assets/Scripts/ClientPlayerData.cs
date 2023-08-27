using System;
using UnityEngine;

[Serializable]
public struct ClientPlayerData {
    public string PlayerName { get; private set; }
    public Color PlayerDefaultColor { get; private set; }

    public ClientPlayerData(string playerName,Color playerDefaultColor) {
        PlayerName = playerName;
        PlayerDefaultColor = playerDefaultColor;
    }
}

