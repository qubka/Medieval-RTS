using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityJSON;

public class SaveLoadManager : SingletonObject<SaveLoadManager>
{
    public Game current;
    private static string Path => Application.persistentDataPath + "/savedGames.dat";
    
    private void Start()
    {
        current = gameObject.AddComponent<Game>();
    }
    
    public void New()
    {
        DestroyImmediate(current);
        current = gameObject.AddComponent<Game>();
    }
    
    public void Save() {
        var bf = new BinaryFormatter();
        var file = File.Create(Path);
        bf.Serialize(file, current.ToJSONString());
        file.Close();
        Debug.Log("Game data saved!");
    }
    
    public void Load() {
        if(File.Exists(Path)) {
            var bf = new BinaryFormatter();
            var file = File.Open(Path, FileMode.Open);
            DestroyImmediate(current);
            //JsonUtility.FromJsonOverwrite((string) bf.Deserialize(file), current);
            current = JSON.Deserialize<Game>((string) bf.Deserialize(file));
            file.Close();
        }else {
            Debug.Log("There is no save data!");
        }
        
        // Only specifying the sceneName or sceneBuildIndex will load the Scene with the Single mode
        //SceneManager.LoadScene("Campaign", LoadSceneMode.Single);
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