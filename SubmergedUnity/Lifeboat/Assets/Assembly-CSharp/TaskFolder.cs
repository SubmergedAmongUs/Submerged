using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TaskFolder : MonoBehaviour
{
	public string FolderName;

	public TextMeshPro Text;

	public TaskAdderGame Parent;

	public List<TaskFolder> SubFolders = new List<TaskFolder>();

	public List<PlayerTask> Children = new List<PlayerTask>();

	[HideInInspector]
	public PassiveButton Button;

	private void Awake()
	{
		this.Button = base.GetComponent<PassiveButton>();
	}

	public void Start()
	{
		this.Text.text = this.FolderName;
	}

	public void OnClick()
	{
		this.Parent.ShowFolder(this);
	}

	internal List<TaskFolder> OrderBy()
	{
		throw new NotImplementedException();
	}
}
