using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadManager
{
    private const string fileType = ".sav";
    private static readonly string saveDirectory = Path.Combine(Application.persistentDataPath, "Game Saves");
    
    private static GameFile file;

    public static void LoadGame(string fileName)
    {
        file = new GameFile(fileName);
        SaveList.Instance.LoadLevel();
    }
    
    public static void SaveGame(string fileName)
    {
        if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);

        var savedGame = new ProgressSave(Game.Instance);
        var bf = new BinaryFormatter();
        var stream = File.Create(GetPath(fileName));
        bf.Serialize(stream, JsonUtility.ToJson(savedGame));
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
        var savedGame = JsonUtility.FromJson<ProgressSave>((string) bf.Deserialize(stream));
        stream.Close();
        
        Debug.Log(fileName + " loaded!");
        
        return savedGame;
    }
    
    public static string GetPath(string fileName) => Path.Combine(saveDirectory, fileName + fileType);
    public static string[] GetFiles() => Directory.Exists(saveDirectory) ? Directory.GetFiles(saveDirectory, '*' + fileType) : new string[0];
    
}

[Serializable]
public class ProgressSave {
    
    public CameraSave camera;
    public TimeSave time;

    public List<FactionSave> factions = new List<FactionSave>();
    public List<CharacterSave> characters = new List<CharacterSave>();
    public List<PartySave> parties = new List<PartySave>();
    public List<SettlementSave> settlements = new List<SettlementSave>();
    public List<HouseSave> houses = new List<HouseSave>();

    public ProgressSave(Game game) 
    {
        //SAVE IN-GAME STUFF
		time = new TimeSave(game.timeController);
        camera = new CameraSave(game.cameraController);

        //SAVE OBJECTS FROM PARENT
        factions.Capacity = game.factions.Count;
        foreach (var faction in game.factions) {
            factions.Add(new FactionSave(faction));
        }
        
        characters.Capacity = game.characters.Count;
        foreach (var character in game.characters) {
            characters.Add(new CharacterSave(character));
        }
        
        parties.Capacity = game.parties.Count;
        foreach (var party in game.parties) {
            parties.Add(new PartySave(party));
        }
        
        settlements.Capacity = game.settlements.Count;
        foreach (var settlement in game.settlements) {
            settlements.Add(new SettlementSave(settlement));
        }
        
        houses.Capacity = game.houses.Count;
        foreach (var house in game.houses) {
            houses.Add(new HouseSave(house));
        }
    }
}