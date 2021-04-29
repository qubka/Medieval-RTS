using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityJSON;

public class SaveLoadManager : SingletonObject<SaveLoadManager>
{
    public Game current;
    public static string Path => Application.persistentDataPath + "/savedGames.dat";

    /*public void New()
    {
        current = ScriptableObject.CreateInstance<Game>();
        current.Load();
        current.OnDeserializationSucceeded(null);
        //Debug.Log(current.ToJSONString());
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
    }*/
}