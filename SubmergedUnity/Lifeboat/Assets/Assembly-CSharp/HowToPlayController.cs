using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HowToPlayController : MonoBehaviour
{
	public Transform DotParent;

	public SpriteRenderer leftButton;

	public SpriteRenderer rightButton;

	[Header("Console Controller Navigation")]
	public UiElement CloseButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	public ConditionalSceneController PCMove;

	public SceneController[] Scenes;

	public int SceneNum;

	public void Start()
	{
		this.Scenes[2] = this.PCMove;
		for (int i = 1; i < this.Scenes.Length; i++)
		{
			this.Scenes[i].gameObject.SetActive(false);
		}
		for (int j = 0; j < this.DotParent.childCount; j++)
		{
			this.DotParent.GetChild(j).localScale = Vector3.one;
		}
		this.ChangeScene(0);
		ControllerManager.Instance.NewScene(base.name, this.CloseButton, this.DefaultButtonSelected, this.ControllerSelectable, false);
	}

	public void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			this.Close();
		}
	}

	public void NextScene()
	{
		this.ChangeScene(1);
	}

	public void PreviousScene()
	{
		this.ChangeScene(-1);
	}

	public void Close()
	{
		SceneManager.LoadScene("MainMenu");
	}

	private void ChangeScene(int del)
	{
		this.Scenes[this.SceneNum].gameObject.SetActive(false);
		this.DotParent.GetChild(this.SceneNum).localScale = Vector3.one;
		this.SceneNum = Mathf.Clamp(this.SceneNum + del, 0, this.Scenes.Length - 1);
		this.Scenes[this.SceneNum].gameObject.SetActive(true);
		this.DotParent.GetChild(this.SceneNum).localScale = new Vector3(1.5f, 1.5f, 1.5f);
		this.leftButton.gameObject.SetActive(this.SceneNum > 0);
		this.rightButton.gameObject.SetActive(this.SceneNum < this.Scenes.Length - 1);
	}
}
