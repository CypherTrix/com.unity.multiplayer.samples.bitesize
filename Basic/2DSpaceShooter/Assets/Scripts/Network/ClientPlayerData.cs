using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct ClientPlayerData {
    //Data
    public string PlayerName;
    public Color PlayerColor;
    //Achivments
    public int ShootsFired;
    public int Kills;
    public int Deaths;
    public int PowerUpsCollected;
    public float PlayTime;
    [NonSerialized] public bool debug;

    public readonly float GetPlayerKD() {
        try {
            float ratio = (float)Kills / Deaths;
            ratio = (float)Math.Round(ratio, 2);
            if (float.IsNaN(ratio)) {
                return 0;
            } else {
            return ratio;
            }
        } catch (DivideByZeroException) {

            return Kills;
        }
    }

    public readonly string GetTimePlayed() {
        TimeSpan t = TimeSpan.FromSeconds(PlayTime);

        string answer = string.Format("{0:D2}h:{1:D2}m",
                        t.Hours,
                        t.Minutes);
        return answer;
    }

    public ClientPlayerData(string playerName, Color playerColor) {
        PlayerName = playerName;
        PlayerColor = playerColor;
        ShootsFired = 0;
        Kills = 0;
        Deaths = 0;
        PowerUpsCollected = 0;
        PlayTime = 0;
#if UNITY_EDITOR
        debug = true;
#else
        debug = false;
#endif
    }
}


public static class RandomPlayerGenerator {
    private static string randomNames = "2coolBillion;AloneMo;Arrowpl;BrightIdol;Cartdrum;Chicu;Edgynylo;Fiducan;Griffonsu;Grivant;Guantopoly;Hactslim;Lakery;MohawkWeb;MudTools;Nip;NotesTastic;Number1or;PersonalGlossy;Printeket;Rosesys;ShardWeb;SnoopGiggly;SpoiledAware;Stadiuma;Stargalls;Sysoften;Troikem;Twitiq;Vodse";


    public static string GetRandomName() {
        System.Random rand = new();
        List<string> Names = new();
        Names.AddRange(randomNames.Split(';').ToList());
        int toSkip = rand.Next(0, Names.Count());

        return Names.Skip(toSkip).Take(1).First();
    }

    public static Color GetRandomColor() {
        return UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.8f, 1f);
        //       return new Color(
        //    Random.Range(0f, 1f),
        //    Random.Range(0f, 1f),
        //    Random.Range(0f, 1f), 1
        //);
    }
}

