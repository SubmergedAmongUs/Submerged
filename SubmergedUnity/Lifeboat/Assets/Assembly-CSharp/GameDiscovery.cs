using System;
using System.Collections.Generic;
using System.Linq;
using Hazel.Udp;
using InnerNet;
using UnityEngine;

public class GameDiscovery : MonoBehaviour
{
	public JoinGameButton ButtonPrefab;

	public Transform ItemLocation;

	public float YStart = 0.56f;

	public float YOffset = -0.75f;

	public Scroller TargetArea;

	private Dictionary<string, JoinGameButton> received = new Dictionary<string, JoinGameButton>();

	public void Start()
	{
		InnerDiscover component = base.GetComponent<InnerDiscover>();
		component.OnPacketGet += this.Receive;
		component.StartAsClient();
	}

	public void Update()
	{
		float time = Time.time;
		string[] array = this.received.Keys.ToArray<string>();
		Vector3 vector = new Vector3(0f, this.YStart, -1f);
		foreach (string key in array)
		{
			JoinGameButton joinGameButton = this.received[key];
			if (time - joinGameButton.timeRecieved > 3f)
			{
				this.received.Remove(key);
				 UnityEngine.Object.Destroy(joinGameButton.gameObject);
			}
			else
			{
				joinGameButton.transform.localPosition = vector;
				vector.y += this.YOffset;
			}
		}
		this.TargetArea.YBounds.max = Mathf.Max(0f, -vector.y - 2f * this.YStart);
	}

	private void Receive(BroadcastPacket packet)
	{
		string[] array = packet.Data.Split(new char[]
		{
			'~'
		});
		string address = packet.GetAddress();
		JoinGameButton joinGameButton;
		if (this.received.TryGetValue(address, out joinGameButton))
		{
			joinGameButton.timeRecieved = Time.time;
			joinGameButton.SetGameName(array);
			return;
		}
		if (array[1].Equals("Open"))
		{
			this.CreateButtonForAddess(address, array);
		}
	}

	private void CreateButtonForAddess(string fromAddress, string[] gameNameParts)
	{
		JoinGameButton joinGameButton = null;
		bool flag = false;
		if (!this.received.TryGetValue(fromAddress, out joinGameButton))
		{
			joinGameButton = UnityEngine.Object.Instantiate<JoinGameButton>(this.ButtonPrefab, this.ItemLocation);
			flag = true;
			Debug.Log("GameDiscovery.CreateButtonForAddess: Instantiate(" + fromAddress + ")");
		}
		if (flag)
		{
			joinGameButton.transform.localPosition = new Vector3(0f, this.YStart + (float)(this.ItemLocation.childCount - 1) * this.YOffset, -1f);
			joinGameButton.netAddress = fromAddress;
			joinGameButton.GetComponentInChildren<MeshRenderer>().material.SetInt("_Mask", 4);
		}
		joinGameButton.timeRecieved = Time.time;
		joinGameButton.SetGameName(gameNameParts);
		if (flag)
		{
			ControllerManager.Instance.AddSelectableUiElement(joinGameButton.GetComponent<PassiveButton>(), false);
		}
		this.received[fromAddress] = joinGameButton;
	}
}
