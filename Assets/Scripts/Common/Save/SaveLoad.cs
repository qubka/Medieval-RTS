using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveLoad
{
    /*public static List<Game> savedGames = new List<Game>();
    
    public static void Save() {
        savedGames.Add(Game.current);
        var bf = new BinaryFormatter();
        var file = File.Create(Application.persistentDataPath + "/savedGames.gd");
        bf.Serialize(file, savedGames);
        file.Close();
    }
    
    public static void Load() {
        if (File.Exists(Application.persistentDataPath + "/savedGames.gd")) {
            var bf = new BinaryFormatter();
            var file = File.Open(Application.persistentDataPath + "/savedGames.gd", FileMode.Open);
            savedGames = (List<Game>) bf.Deserialize(file);
            file.Close();
        }
    }*/
}