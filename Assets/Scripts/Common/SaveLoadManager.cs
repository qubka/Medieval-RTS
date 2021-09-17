using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadManager
{
    private const string fileType = ".sav";
    private static readonly string saveDirectory = Path.Combine(Application.persistentDataPath, "GameSaves");
    
    private static GameFile file;

    public static void LoadGame(string fileName)
    {
        file = new GameFile(fileName);
        ChangeSceneAsync.Instance.ChangeScene("Campaign");
    }
    
    public static void SaveGame(string fileName)
    {
        if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);

        var savedGame = new ProgressSave(Game.Instance);
        var bf = new BinaryFormatter();
        var stream = File.Create(GetPath(fileName));
        bf.Serialize(stream, Zip(JsonUtility.ToJson(savedGame)));
        stream.Close();
        
        Debug.Log(fileName + " saved!");
    }

    public static bool FileExistsInFiles(string fileName)
    {
        return File.Exists(GetPath(fileName));
    }
    
    public static void DeleteFile(string fileName)
    {
        File.Delete(GetPath(fileName));
    }

    public static ProgressSave GetGameData()
    {
        if (file == null) return null;

        //keep current selected world
        var fileName = file.FileName;
        var filePath = GetPath(fileName);
        
        //reset current selected world
        file = null;

        if (!File.Exists(filePath)) return null;
        
        var bf = new BinaryFormatter();
        var stream = File.Open(filePath, FileMode.Open);
        var savedGame = JsonUtility.FromJson<ProgressSave>(UnZip((byte[]) bf.Deserialize(stream)));
        stream.Close();
        
        Debug.Log(fileName + " loaded!");
        
        return savedGame;
    }
    
    public static string GetPath(string fileName) => Path.Combine(saveDirectory, fileName + fileType);
    public static string[] GetFiles() => Directory.Exists(saveDirectory) ? Directory.GetFiles(saveDirectory, '*' + fileType) : new string[0];
    private static byte[] Zip(string str)
    {
        using var output = new MemoryStream();
        using (var gzip = new DeflateStream(output, CompressionMode.Compress)) {
            using var writer = new StreamWriter(gzip, System.Text.Encoding.UTF8);
            writer.Write(str);
        }
        return output.ToArray();
    }
    private static string UnZip(byte[] input)
    {
        using var inputStream = new MemoryStream(input);
        using var gzip = new DeflateStream(inputStream, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, System.Text.Encoding.UTF8);
        return reader.ReadToEnd();
    }
}

[Serializable]
public class ProgressSave {
    
    public CameraSave camera;
    public TimeSave time;

    public FactionSave[] factions;
    public CharacterSave[] characters;
    public PartySave[] parties;
    public SettlementSave[] settlements;
    public HouseSave[] houses;
    public BattleSave[] battles;

    public ProgressSave(Game game) 
    {
		time = new TimeSave(game.timeController);
        camera = new CameraSave(game.cameraController);
        
        factions = game.factions.Select(f => new FactionSave(f)).ToArray();
        characters = game.characters.Select(c => new CharacterSave(c)).ToArray();
        parties = game.parties.Select(p => new PartySave(p)).ToArray();
        settlements = game.settlements.Select(s => new SettlementSave(s)).ToArray();
        houses = game.houses.Select(h => new HouseSave(h)).ToArray();
        battles = game.battles.Select(b => new BattleSave(b)).ToArray();
    }
}