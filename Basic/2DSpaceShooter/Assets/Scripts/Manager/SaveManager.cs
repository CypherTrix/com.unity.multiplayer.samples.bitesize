using System;
using System.IO;
using UnityEngine;
public static class SaveManager {
    public static readonly string ROOT_PATH = Application.persistentDataPath;
    public static readonly string FILE_PATH = "Saves";
    public static readonly string PLAYER_SAVE_FILE_NAME = "player";
    public static readonly string FILE_EXTENSION = ".save";

    public static void SaveData<T>(string fileName, T dataToSave) {
        string savesFolderPath = Path.Combine(ROOT_PATH, FILE_PATH);
        if (!Directory.Exists(savesFolderPath))
            Directory.CreateDirectory(savesFolderPath);

        string filePath = Path.Combine(savesFolderPath, fileName + FILE_EXTENSION);
        using StreamWriter streamWriter = new(filePath);
        string json = JsonUtility.ToJson(dataToSave, true);
        if (!string.IsNullOrEmpty(json)) {
            streamWriter.WriteLine(json);
        } else {
            Debug.LogError($"SaveManager : Error creating JSON for {fileName}{FILE_EXTENSION} json is null or empty");
        }
    }

    public static bool LoadData<T>(string fileName, out T dataToLoad) {
        dataToLoad = default;
        string savesFolderPath = Path.Combine(ROOT_PATH, FILE_PATH);
        if (!Directory.Exists(savesFolderPath))
            return false;

        string filePath = Path.Combine(savesFolderPath, fileName + FILE_EXTENSION);
        if (!File.Exists(filePath))
            return false;

        using StreamReader streamReader = new(filePath);
        string json = streamReader.ReadToEnd();

        if (!string.IsNullOrEmpty(json)) {

            dataToLoad = JsonUtility.FromJson<T>(json);
            return dataToLoad != null;
        } else {
            Debug.LogError($"SaveManager : Error reading JSON for {fileName}{FILE_EXTENSION} json is null or empty");
            return false;
        }
    }

    public static void DeleteData(string fileName) {
        try {
            File.Delete(Path.Combine(ROOT_PATH, FILE_PATH, fileName + FILE_EXTENSION));
        } catch (FileNotFoundException ex) {
            Debug.LogError($"SaveManager : Error Delete File {fileName}{FILE_EXTENSION} {Environment.NewLine} {ex.Message}");
        }

    }

    public static bool SaveFileExsis(string fileName) {
        return File.Exists(Path.Combine(ROOT_PATH, FILE_PATH, fileName + FILE_EXTENSION));
    }
}


