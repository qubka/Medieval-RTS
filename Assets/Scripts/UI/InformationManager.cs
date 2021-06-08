using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InformationManager : SingletonObject<InformationManager>
{
    [SerializeField] private Transform canvas;
    [SerializeField] private GameObject messageBox;
    
    private readonly Dictionary<string, MessageBox> messages = new Dictionary<string, MessageBox>();
    
    public void ShowInquiry(string description, UnityAction yes = null, UnityAction no = null)
    {
        if (messages.ContainsKey(description))
            return;

        var box = Instantiate(messageBox, canvas).GetComponent<MessageBox>();
        
        box.EnableYesAndNo();
        box.DisableCloseBtn();
        box.SetMessage(description);
        
        box.onYes.AddListener(yes ?? (() => box.Disappear()));
        box.onNo.AddListener(no ?? (() => box.Disappear()));
        box.onDestroy.AddListener( () => messages.Remove(description));
        
        messages.Add(description, box);
    }
}