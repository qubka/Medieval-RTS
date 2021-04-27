using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadManager : SingletonObject<SaveLoadManager>
{
    public Game current;
    public string session = "new_game";
    public static string Path => Application.persistentDataPath + "/savedGames.dat";

    public void New()
    {
        current = ScriptableObject.CreateInstance<Game>();
        current.Load();
        Debug.Log(current.Save());
        //current.name = session + "_" + DateTime.Now.ToString("MM_dd_yyyy_h_mm_tt");
    }
    
    public void Save() {
        var bf = new BinaryFormatter();
        var file = File.Create(Path);
        bf.Serialize(file, JsonUtility.ToJson(current));
        file.Close();
        Debug.Log("Game data saved!");
    }
    
    public void Load() {
        if(File.Exists(Path)) {
            var bf = new BinaryFormatter();
            var file = File.Open(Path, FileMode.Open);
            //JsonUtility.FromJsonOverwrite((string) bf.Deserialize(file), current);
            file.Close();
        }else {
            Debug.Log("There is no save data!");
        }
        
        // Only specifying the sceneName or sceneBuildIndex will load the Scene with the Single mode
        SceneManager.LoadScene("Campaign", LoadSceneMode.Single);
    }
    
    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 125, 50), "Save Your Game"))
            Save();
        if (GUI.Button(new Rect(0, 100, 125, 50), "Load Your Game"))
            Load();
        if (GUI.Button(new Rect(0, 200, 125, 50), "New Game"))
            New();
    }
}