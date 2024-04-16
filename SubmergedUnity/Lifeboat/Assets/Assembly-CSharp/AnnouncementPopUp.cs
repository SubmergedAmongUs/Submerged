using System;
using System.Collections;
using System.Net;
using Hazel;
using Hazel.Udp;
using TMPro;
using UnityEngine;

public class AnnouncementPopUp : MonoBehaviour
{
	public const uint AnnouncementVersion = 2U;

	private UnityUdpClientConnection connection;

	private AnnouncementPopUp.AnnounceState AskedForUpdate;

	public TextMeshPro AnnounceText;

	private Announcement announcementUpdate;

	public GameObject ConnectIcon;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	private static bool IsSuccess(AnnouncementPopUp.AnnounceState state)
	{
		return state == AnnouncementPopUp.AnnounceState.Success || state == AnnouncementPopUp.AnnounceState.Cached;
	}

	private void OnEnable()
	{
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton);
	}

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public void EnableAnnouncement()
	{
		base.gameObject.SetActive(true);
	}

	public IEnumerator Init()
	{
		if (this.AskedForUpdate != AnnouncementPopUp.AnnounceState.Fetching)
		{
			yield break;
		}
		yield return DestroyableSingleton<ServerManager>.Instance.WaitForServers();
		Debug.Log("Requesting announcement from: " + DestroyableSingleton<ServerManager>.Instance.OnlineNetAddress);
		this.connection = new UnityUdpClientConnection(new IPEndPoint(IPAddress.Parse(DestroyableSingleton<ServerManager>.Instance.OnlineNetAddress), 22024), 0);
		this.connection.ResendTimeout = 1000;
		this.connection.ResendPingMultiplier = 1f;
		this.connection.DataReceived += this.Connection_DataReceived;
		this.connection.Disconnected += this.Connection_Disconnected;
		try
		{
			Announcement lastAnnouncement = SaveManager.LastAnnouncement;
			MessageWriter messageWriter = MessageWriter.Get(SendOption.None);
			messageWriter.WritePacked(2U);
			messageWriter.WritePacked(lastAnnouncement.Id);
			messageWriter.WritePacked(SaveManager.LastLanguage);
			this.connection.ConnectAsync(messageWriter.ToByteArray(true));
			messageWriter.Recycle();
		}
		catch
		{
			this.AskedForUpdate = AnnouncementPopUp.AnnounceState.Failed;
		}
		yield return this.Show();
		yield break;
	}

	private void Connection_Disconnected(object sender, DisconnectedEventArgs e)
	{
		Debug.Log("Announcement failed: " + e.Reason);
		this.AskedForUpdate = AnnouncementPopUp.AnnounceState.Failed;
		this.connection.Dispose();
		this.connection = null;
	}

	public void FixedUpdate()
	{
		UnityUdpClientConnection unityUdpClientConnection = this.connection;
		if (unityUdpClientConnection != null)
		{
			unityUdpClientConnection.FixedUpdate();
		}
	}

	private void Connection_DataReceived(DataReceivedEventArgs e)
	{
		MessageReader message = e.Message;
		try
		{
			while (message.Position < message.Length)
			{
				MessageReader messageReader = message.ReadMessage();
				switch (messageReader.Tag)
				{
				case 0:
					this.AskedForUpdate = AnnouncementPopUp.AnnounceState.Cached;
					break;
				case 1:
					this.announcementUpdate = default(Announcement);
					this.announcementUpdate.DateFetched = DateTime.UtcNow;
					this.announcementUpdate.Id = messageReader.ReadPackedUInt32();
					this.announcementUpdate.AnnounceText = messageReader.ReadString();
					this.AskedForUpdate = ((this.announcementUpdate.Id == 0U) ? AnnouncementPopUp.AnnounceState.Cached : AnnouncementPopUp.AnnounceState.Success);
					break;
				case 4:
					try
					{
						int num = (int)messageReader.ReadByte();
						for (int i = 0; i < num; i++)
						{
							ChatLanguageSet.Instance.Languages[messageReader.ReadString()] = messageReader.ReadUInt32();
						}
					}
					catch (Exception ex)
					{
						Debug.Log("Error while loading languages: " + ex.Message);
					}
					try
					{
						ChatLanguageSet.Instance.Save();
					}
					catch
					{
					}
					break;
				}
			}
		}
		finally
		{
			message.Recycle();
		}
		try
		{
			this.connection.Dispose();
			this.connection = null;
		}
		catch
		{
		}
	}

	public IEnumerator Show()
	{
		float timer = 0f;
		while (this.AskedForUpdate == AnnouncementPopUp.AnnounceState.Fetching && timer < 6f)
		{
			timer += Time.deltaTime;
			yield return null;
		}
		if (!AnnouncementPopUp.IsSuccess(this.AskedForUpdate))
		{
			Announcement lastAnnouncement = SaveManager.LastAnnouncement;
			if (lastAnnouncement.Id == 0U)
			{
				this.AnnounceText.text = "Couldn't get announcement.";
			}
			else
			{
				this.AnnounceText.text = "Couldn't get announcement. Last Known:\r\n" + lastAnnouncement.AnnounceText;
				this.AnnounceText.text = this.AnnounceText.text.Replace("[http", "<link=http");
				this.AnnounceText.text = this.AnnounceText.text.Replace("[]", "</link>");
				this.AnnounceText.text = this.AnnounceText.text.Replace("]", ">");
				base.GetComponentInChildren<OpenHyperlinks>().SetLinkColor();
			}
		}
		else if (this.announcementUpdate.Id != SaveManager.LastAnnouncement.Id)
		{
			if (this.AskedForUpdate != AnnouncementPopUp.AnnounceState.Cached)
			{
				base.gameObject.SetActive(true);
			}
			if (this.announcementUpdate.Id == 0U)
			{
				this.announcementUpdate = SaveManager.LastAnnouncement;
				this.announcementUpdate.DateFetched = DateTime.UtcNow;
			}
			SaveManager.LastAnnouncement = this.announcementUpdate;
			this.AnnounceText.text = this.announcementUpdate.AnnounceText;
			this.AnnounceText.text = this.AnnounceText.text.Replace("[http", "<link=http");
			this.AnnounceText.text = this.AnnounceText.text.Replace("[]", "</link>");
			this.AnnounceText.text = this.AnnounceText.text.Replace("]", ">");
			base.GetComponentInChildren<OpenHyperlinks>().SetLinkColor();
		}
		while (base.gameObject.activeSelf)
		{
			yield return null;
		}
		yield break;
	}

	public void Close()
	{
		base.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		if (this.connection != null)
		{
			this.connection.Dispose();
			this.connection = null;
		}
	}

	private enum AnnounceState
	{
		Fetching,
		Failed,
		Success,
		Cached
	}
}
