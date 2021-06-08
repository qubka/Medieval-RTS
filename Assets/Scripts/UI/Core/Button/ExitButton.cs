using UnityEngine;

public class ExitButton : MonoBehaviour
{
    [SerializeField] private string description;

    public void Pressed()
    {
        InformationManager.Instance.ShowInquiry(description, Application.Quit);
    }
}