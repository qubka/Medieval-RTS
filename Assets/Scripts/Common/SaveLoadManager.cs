using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadManager
{
    private const string fileType = ".dat";

    public static GameFile file;

    public static void SaveGame(string dir, string filename)
    {
        var datapath = Application.persistentDataPath + "/";

        if (!Directory.Exists(datapath + dir)) Directory.CreateDirectory(datapath + dir);

        var savedGame = new ProgressSave(Game.Instance);
        var bf = new BinaryFormatter();
        var stream = File.Create(Path.Combine(datapath + dir, filename + fileType));
        bf.Serialize(stream, JsonUtility.ToJson(savedGame));
        stream.Close();
        Debug.Log(filename + " saved!");
    }

    public static AsyncOperation LoadLevel()
    {
        return SceneManager.LoadSceneAsync("Campaign");
    }

    public static bool FileExistsInFiles(string d, string f)
    {
        var datapath = Application.persistentDataPath + '/';
        return File.Exists(Path.Combine(datapath + d, f + fileType));
    }

    public static ProgressSave GetGameData()
    {
        if (file == null) return null;

        //keep current selected world
        var fileLoc = file.FileLocation;
        var fileName = file.FileName;

        //reset current selected world
        file = null;

        return GetSavedFromFiles(Application.persistentDataPath + '/' + fileLoc, fileName);
    }

    public static ProgressSave GetSavedFromFiles(string dir, string name)
    {
        var bf = new BinaryFormatter();
        var stream = File.Open(Path.Combine(dir, name + fileType), FileMode.Open);
        var savedGame = JsonUtility.FromJson<ProgressSave>((string) bf.Deserialize(stream));
        stream.Close();
        return savedGame;
    }

    public static void DeleteFile(string dir, string name)
    {
        File.Delete(Application.persistentDataPath + '/' + dir + '/' + name + fileType);
    }
}

[Serializable]
public class ProgressSave {
    
    public CameraSave camera;
    public TimeSave time;
    public DateTime saveDate;

	public List<FactionSave> factions = new List<FactionSave>();
    public List<CharacterSave> characters = new List<CharacterSave>();
    public List<PartySave> parties = new List<PartySave>();
    public List<SettlementSave> settlements = new List<SettlementSave>();
    public List<HouseSave> houses = new List<HouseSave>();

    public ProgressSave(Game game) 
    {
        saveDate = DateTime.Now;
		Debug.Log(saveDate.ToString("MM/dd/yyyy"));

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