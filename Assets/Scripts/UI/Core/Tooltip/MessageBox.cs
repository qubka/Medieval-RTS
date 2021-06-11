using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour 
{
	[SerializeField] private TextMeshProUGUI mainText;
	[Space]
	[SerializeField] private GameObject actions;
	[SerializeField] private ConfirmationButton yes;
	[SerializeField] private ConfirmationButton no;
	[Space]
	[SerializeField] private GameObject close;
	
	public readonly UnityEvent onYes = new UnityEvent();
	public readonly UnityEvent onNo = new UnityEvent();
	public readonly UnityEvent onDestroy = new UnityEvent();
	
	private Animator animator;
	private Coroutine destroyed;
	
	private void Awake()
	{
		animator = GetComponent<Animator>();
		DisableYesAndNo();
	}

	public void DisableYesAndNo()
	{
		actions.SetActive(false);
	}
	
	public void DisableCloseBtn()
	{
		close.SetActive(false);
	}
	
	public void EnableYesAndNo(string yesLabel = "Yes", string noLabel = "No")
	{
		actions.SetActive(true);
		yes.text.text = yesLabel;
		no.text.text = noLabel;
	}
	
	public void SetMessage(string message)
	{
		mainText.text = message;
	}

	public void YesClicked()
	{
		onYes?.Invoke();
		Disappear();
	}
	
	public void NoClicked()
	{
		onNo?.Invoke();
		Disappear();
	}

	public void Disappear()
	{
		if (destroyed != null)
			return;
		
		yes.button.interactable = false;
		no.button.interactable = false;
		
		destroyed = StartCoroutine(Delete());
		animator.Play("MessageBoxDisappear");
	}

	private IEnumerator Delete()
	{
		yield return new WaitForSecondsRealtime(0.45f);
		onDestroy?.Invoke();
		DestroyImmediate(gameObject);
	}

	private void Update()
	{
		if (Input.GetKey(KeyCode.Escape)) {
			Disappear();
		}
	}
}