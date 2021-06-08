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
	[SerializeField] private GameObject actionButtons;
	[SerializeField] private TextMeshProUGUI yesText;
	[SerializeField] private Button yesButton;
	[SerializeField] private TextMeshProUGUI noText;
	[SerializeField] private Button noButton;
	[Space]
	[SerializeField] private GameObject closeButton;
	
	public readonly UnityEvent onYes = new UnityEvent();
	public readonly UnityEvent onNo = new UnityEvent();
	public readonly UnityEvent onDestroy = new UnityEvent();
	
	private Animator animator;
	private Coroutine destroyed;
	
	private void Awake()
	{
		animator = GetComponent<Animator>();
		//DisableYesAndNo();
	}

	private void DisableYesAndNo()
	{
		actionButtons.SetActive(false);
	}
	
	public void DisableCloseBtn()
	{
		closeButton.SetActive(false);
	}
	
	public void EnableYesAndNo(string yes = "Yes", string no = "No")
	{
		actionButtons.SetActive(true);
		yesText.text = yes;
		noText.text = no;
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
		
		yesButton.interactable = false;
		noButton.interactable = false;
		
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