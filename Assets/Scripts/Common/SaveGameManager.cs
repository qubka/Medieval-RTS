using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveGameManager : SingletonObject<SaveGameManager>
{
    //public static List<Game> savedGames = new List<Game>();
    public List<SavableObject> savableObjects = new List<SavableObject>(1000);

    /*public List<SavableObject> GetSavableObjects()
    {
        
    }*/
    
    public void Save() {
        var bf = new BinaryFormatter(); 
        var file = File.Create(Application.persistentDataPath + "/MySaveData.dat");
        foreach (var o in savableObjects) {
            bf.Serialize(file, o.Save());
        }
        file.Close();
        Debug.Log("Game data saved!");

        
        
    }
    
    public void Load() {
        /*if (File.Exists(Application.persistentDataPath + "/MySaveData.dat")) {
            var bf = new BinaryFormatter();
            var file = File.Open(Application.persistentDataPath + "/MySaveData.dat", FileMode.Open);
            var data = (SaveData) bf.Deserialize(file);
            file.Close();
            //intToSave = data.savedInt;
            //floatToSave = data.savedFloat;
            //boolToSave = data.savedBool;
            Debug.Log("Game data loaded!");
        } else {
            Debug.LogError("There is no save data!");
        }*/
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 125, 50), "Save Your Game"))
            Save();
        if (GUI.Button(new Rect(0, 100, 125, 50), "Load Your Game"))
            Load();
    }
    
    
}