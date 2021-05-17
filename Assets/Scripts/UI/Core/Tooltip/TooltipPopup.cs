﻿using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipPopup : Tooltip
{
    [SerializeField] private TextMeshProUGUI textMesh;
    
    public void DisplayInfo(string caption, string description, TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft)
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
        textMesh.alignment = alignment;
        
        SetActive(true);
        ForceRebuildLayoutImmediate();
    }

    public void HideInfo()
    {
        SetActive(false);
    }
}