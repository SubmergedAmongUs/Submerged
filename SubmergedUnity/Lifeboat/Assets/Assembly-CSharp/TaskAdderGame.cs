using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class TaskAdderGame : Minigame
{
	public TextMeshPro PathText;

	public TaskFolder RootFolderPrefab;

	public TaskAddButton TaskPrefab;

	public Transform TaskParent;

	public List<TaskFolder> Heirarchy = new List<TaskFolder>();

	public List<Transform> ActiveItems = new List<Transform>();

	public TaskAddButton InfectedButton;

	public float folderWidth;

	public float fileWidth;

	public float lineWidth;

	private TaskFolder Root;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement FolderBackButton;

	public List<UiElement> ControllerSelectable;

	private string restorePreviousSelectionByFolderName = string.Empty;

	public UiElement restorePreviousSelectionFound;

	private void OnEnable()
	{
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, null, this.ControllerSelectable, false);
	}

	private void OnDisable()
	{
		ControllerManager.Instance.ResetAll();
	}

	public override void Begin(PlayerTask t)
	{
		base.Begin(t);
		this.Root = UnityEngine.Object.Instantiate<TaskFolder>(this.RootFolderPrefab, base.transform);
		this.Root.gameObject.SetActive(false);
		Dictionary<SystemTypes, TaskFolder> folders = new Dictionary<SystemTypes, TaskFolder>();
		this.PopulateRoot(this.Root, folders, ShipStatus.Instance.CommonTasks);
		this.PopulateRoot(this.Root, folders, ShipStatus.Instance.LongTasks);
		this.PopulateRoot(this.Root, folders, ShipStatus.Instance.NormalTasks);
		this.Root.SubFolders = (from f in this.Root.SubFolders
		orderby f.FolderName
		select f).ToList<TaskFolder>();
		this.ShowFolder(this.Root);
	}

	private void PopulateRoot(TaskFolder rootFolder, Dictionary<SystemTypes, TaskFolder> folders, NormalPlayerTask[] taskList)
	{
		foreach (NormalPlayerTask normalPlayerTask in taskList)
		{
			SystemTypes systemTypes = normalPlayerTask.StartAt;
			if (normalPlayerTask is DivertPowerTask)
			{
				systemTypes = ((DivertPowerTask)normalPlayerTask).TargetSystem;
			}
			if (systemTypes == SystemTypes.LowerEngine)
			{
				systemTypes = SystemTypes.UpperEngine;
			}
			TaskFolder taskFolder;
			if (!folders.TryGetValue(systemTypes, out taskFolder))
			{
				taskFolder = (folders[systemTypes] = UnityEngine.Object.Instantiate<TaskFolder>(this.RootFolderPrefab, base.transform));
				taskFolder.gameObject.SetActive(false);
				if (systemTypes == SystemTypes.UpperEngine)
				{
					taskFolder.FolderName = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Engines, Array.Empty<object>());
				}
				else
				{
					taskFolder.FolderName = DestroyableSingleton<TranslationController>.Instance.GetString(systemTypes);
				}
				rootFolder.SubFolders.Add(taskFolder);
			}
			taskFolder.Children.Add(normalPlayerTask);
		}
	}

	public void GoToRoot()
	{
		this.Heirarchy.Clear();
		this.ShowFolder(this.Root);
	}

	public void GoUpOne()
	{
		if (this.Heirarchy.Count > 1)
		{
			TaskFolder taskFolder = this.Heirarchy[this.Heirarchy.Count - 2];
			this.Heirarchy.RemoveAt(this.Heirarchy.Count - 1);
			this.Heirarchy.RemoveAt(this.Heirarchy.Count - 1);
			this.ShowFolder(taskFolder);
		}
	}

	public void ShowFolder(TaskFolder taskFolder)
	{
		StringBuilder stringBuilder = new StringBuilder(64);
		this.Heirarchy.Add(taskFolder);
		for (int i = 0; i < this.Heirarchy.Count; i++)
		{
			stringBuilder.Append(this.Heirarchy[i].FolderName);
			stringBuilder.Append("\\");
		}
		this.PathText.text = stringBuilder.ToString();
		for (int j = 0; j < this.ActiveItems.Count; j++)
		{
			 UnityEngine.Object.Destroy(this.ActiveItems[j].gameObject);
		}
		this.ActiveItems.Clear();
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		for (int k = 0; k < taskFolder.SubFolders.Count; k++)
		{
			TaskFolder taskFolder2 = UnityEngine.Object.Instantiate<TaskFolder>(taskFolder.SubFolders[k], this.TaskParent);
			taskFolder2.gameObject.SetActive(true);
			taskFolder2.Parent = this;
			taskFolder2.transform.localPosition = new Vector3(num, num2, 0f);
			taskFolder2.transform.localScale = Vector3.one;
			num3 = Mathf.Max(num3, taskFolder2.Text.bounds.size.y + 1.1f);
			num += this.folderWidth;
			if (num > this.lineWidth)
			{
				num = 0f;
				num2 -= num3;
				num3 = 0f;
			}
			this.ActiveItems.Add(taskFolder2.transform);
			if (taskFolder2 != null && taskFolder2.Button != null)
			{
				ControllerManager.Instance.AddSelectableUiElement(taskFolder2.Button, false);
				if (!string.IsNullOrEmpty(this.restorePreviousSelectionByFolderName) && taskFolder2.FolderName.Equals(this.restorePreviousSelectionByFolderName))
				{
					this.restorePreviousSelectionFound = taskFolder2.Button;
				}
			}
		}
		bool flag = false;
		List<PlayerTask> list = (from t in taskFolder.Children
		orderby t.TaskType
		select t).ToList<PlayerTask>();
		for (int l = 0; l < list.Count; l++)
		{
			TaskAddButton taskAddButton = UnityEngine.Object.Instantiate<TaskAddButton>(this.TaskPrefab);
			taskAddButton.MyTask = list[l];
			if (taskAddButton.MyTask.TaskType == TaskTypes.DivertPower)
			{
				SystemTypes targetSystem = ((DivertPowerTask)taskAddButton.MyTask).TargetSystem;
				taskAddButton.Text.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DivertPowerTo, new object[]
				{
					DestroyableSingleton<TranslationController>.Instance.GetString(targetSystem)
				});
			}
			else if (taskAddButton.MyTask.TaskType == TaskTypes.ActivateWeatherNodes)
			{
				int nodeId = ((WeatherNodeTask)taskAddButton.MyTask).NodeId;
				taskAddButton.Text.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FixWeatherNode, Array.Empty<object>()) + " " + DestroyableSingleton<TranslationController>.Instance.GetString(WeatherSwitchGame.ControlNames[nodeId], Array.Empty<object>());
			}
			else
			{
				taskAddButton.Text.text = DestroyableSingleton<TranslationController>.Instance.GetString(taskAddButton.MyTask.TaskType);
			}
			this.AddFileAsChild(taskFolder, taskAddButton, ref num, ref num2, ref num3);
			if (taskAddButton != null && taskAddButton.Button != null)
			{
				ControllerManager.Instance.AddSelectableUiElement(taskAddButton.Button, false);
				if (this.Heirarchy.Count != 1 && !flag)
				{
					TaskFolder component = ControllerManager.Instance.CurrentUiState.CurrentSelection.GetComponent<TaskFolder>();
					if (component != null)
					{
						this.restorePreviousSelectionByFolderName = component.FolderName;
					}
					ControllerManager.Instance.SetDefaultSelection(taskAddButton.Button, null);
					flag = true;
				}
			}
		}
		if (this.Heirarchy.Count == 1)
		{
			TaskAddButton taskAddButton2 = UnityEngine.Object.Instantiate<TaskAddButton>(this.InfectedButton);
			taskAddButton2.Text.text = "Be_Impostor.exe";
			this.AddFileAsChild(this.Root, taskAddButton2, ref num, ref num2, ref num3);
			if (taskAddButton2 != null && taskAddButton2.Button != null)
			{
				ControllerManager.Instance.AddSelectableUiElement(taskAddButton2.Button, false);
				if (this.restorePreviousSelectionFound != null)
				{
					ControllerManager.Instance.SetDefaultSelection(this.restorePreviousSelectionFound, null);
					this.restorePreviousSelectionByFolderName = string.Empty;
					this.restorePreviousSelectionFound = null;
				}
				else
				{
					ControllerManager.Instance.SetDefaultSelection(taskAddButton2.Button, null);
				}
			}
		}
		if (this.Heirarchy.Count == 1)
		{
			ControllerManager.Instance.SetBackButton(this.BackButton);
			return;
		}
		ControllerManager.Instance.SetBackButton(this.FolderBackButton);
	}

	private void AddFileAsChild(TaskFolder taskFolder, TaskAddButton item, ref float xCursor, ref float yCursor, ref float maxHeight)
	{
		item.transform.SetParent(this.TaskParent);
		item.transform.localPosition = new Vector3(xCursor, yCursor, 0f);
		item.transform.localScale = Vector3.one;
		maxHeight = Mathf.Max(maxHeight, item.Text.bounds.size.y + 1.1f);
		xCursor += this.fileWidth;
		if (xCursor > this.lineWidth)
		{
			xCursor = 0f;
			yCursor -= maxHeight;
			maxHeight = 0f;
		}
		this.ActiveItems.Add(item.transform);
	}
}
