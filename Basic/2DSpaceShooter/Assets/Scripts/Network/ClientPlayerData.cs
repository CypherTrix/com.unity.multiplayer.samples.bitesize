using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct ClientPlayerData {
    public string PlayerName;
    public Color PlayerColor;


    public ClientPlayerData(string playerName, Color playerColor) {
        PlayerName = playerName;
        PlayerColor = playerColor;
    }
    public ClientPlayerData(string playerName = "") {
        PlayerName = RandomPlayerGenerator.GetRandomName();
        PlayerColor = RandomPlayerGenerator.GetRandomColor();
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
        return Random.ColorHSV(0f, 1f, 1f, 1f, 0.8f, 1f);
 //       return new Color(
 //    Random.Range(0f, 1f),
 //    Random.Range(0f, 1f),
 //    Random.Range(0f, 1f), 1
 //);
    }
}

