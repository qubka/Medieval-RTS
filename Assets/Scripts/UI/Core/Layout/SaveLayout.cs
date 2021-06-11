using TMPro;
using UnityEngine;

public class SaveLayout : MonoBehaviour
{
    public TMP_Text label;
    public TextMeshProUGUI date;

    public void Load()
    {
        if (!SaveLoadManager.FileExistsInFiles(label.text)) {
            DestroyImmediate(gameObject);
            return;
        }

        SaveLoadManager.LoadGame(label.text);
    }
    
    public void Delete()
    {
        if (!SaveLoadManager.FileExistsInFiles(label.text)) {
            DestroyImmediate(gameObject);
            return;
        }
        
        SaveLoadManager.DeleteFile(label.text);
    }
}