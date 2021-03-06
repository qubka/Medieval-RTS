using System.Text;
using TMPro;
using UnityEngine;

public class TooltipPopup : Tooltip
{
    [SerializeField] private TMP_Text textMesh;
    
    public void DisplayInfo(string caption, string description)
    {
        var builder = new StringBuilder();
        if (caption.Length > 0) {
            builder
                .Append("<size=20>")
                .Append(caption)
                .Append("</size>")
                .AppendLine();
        }
        builder.Append(description);
        
        textMesh.text = builder.ToString();

        SetActive(true);
        ForceRebuildLayoutImmediate();
    }

    public void HideInfo()
    {
        SetActive(false);
    }
}