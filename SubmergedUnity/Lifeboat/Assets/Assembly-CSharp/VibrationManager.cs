using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class VibrationManager : DestroyableSingleton<VibrationManager>
{
	private List<VibrationManager.LocalVibration> currentLocalVibration = new List<VibrationManager.LocalVibration>();

	private List<VibrationManager.WorldVibration> currentWorldVibration = new List<VibrationManager.WorldVibration>();

	private Vector2 singleFrameVibration = Vector2.zero;

	private bool hasFrameVibration;

	private bool zeroNextFrame;

	public int numVibrationsActive;

	public Vector2 currentVibration;

	private Camera cam;

	private VibrationManager.WorldVibration tempSingleFrameWorldVibration = new VibrationManager.WorldVibration();

	private VibrationManager.WorldVibration tempAmbientSoundVibration = new VibrationManager.WorldVibration();

	private static float[] samples = new float[2];

	private void Start()
	{
		this.cam = Camera.main;
	}

	private void OnEnable()
	{
		SceneManager.activeSceneChanged += new UnityAction<Scene, Scene>(this.SceneManager_activeSceneChanged);
	}

	private void OnDisable()
	{
		SceneManager.activeSceneChanged -= new UnityAction<Scene, Scene>(this.SceneManager_activeSceneChanged);
	}

	private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
	{
		VibrationManager.ClearAllVibration();
	}

	public static void ClearAllVibration()
	{
		DestroyableSingleton<VibrationManager>.Instance.currentLocalVibration.Clear();
		DestroyableSingleton<VibrationManager>.Instance.currentWorldVibration.Clear();
		DestroyableSingleton<VibrationManager>.Instance.currentVibration = Vector2.zero;
	}

	private void Update()
	{
		if (!this.cam)
		{
			this.cam = Camera.main;
		}
		if (RaycastAmbientSoundPlayer.players.Count > 0)
		{
			Vector2 cameraPos = this.cam.transform.position;
			foreach (RaycastAmbientSoundPlayer raycastAmbientSoundPlayer in RaycastAmbientSoundPlayer.players)
			{
				if (raycastAmbientSoundPlayer.AmbientSound)
				{
					raycastAmbientSoundPlayer.t += Time.deltaTime;
					if (raycastAmbientSoundPlayer.t > raycastAmbientSoundPlayer.AmbientSound.length)
					{
						raycastAmbientSoundPlayer.t -= raycastAmbientSoundPlayer.AmbientSound.length;
					}
					if (raycastAmbientSoundPlayer.ambientVolume > 0.01f)
					{
						this.tempAmbientSoundVibration.clip = raycastAmbientSoundPlayer.AmbientSound;
						this.tempAmbientSoundVibration.duration = raycastAmbientSoundPlayer.AmbientSound.length;
						this.tempAmbientSoundVibration.intensity = raycastAmbientSoundPlayer.ambientVolume * 0.5f;
						this.tempAmbientSoundVibration.location = raycastAmbientSoundPlayer.transform.position;
						this.tempAmbientSoundVibration.radius = ((raycastAmbientSoundPlayer.AmbientMaxDist > 0f) ? raycastAmbientSoundPlayer.AmbientMaxDist : 10000f);
						this.tempAmbientSoundVibration.t = raycastAmbientSoundPlayer.t;
						this.singleFrameVibration += this.tempAmbientSoundVibration.UpdateIntensity(cameraPos, 0f);
						this.hasFrameVibration = true;
					}
				}
			}
		}
		if (this.currentLocalVibration.Count > 0 || this.currentWorldVibration.Count > 0 || this.hasFrameVibration)
		{
			Player player = ReInput.players.GetPlayer(0);
			Vector2 vector = this.singleFrameVibration;
			this.singleFrameVibration = Vector2.zero;
			for (int i = this.currentLocalVibration.Count - 1; i >= 0; i--)
			{
				if (this.currentLocalVibration[i].Alive)
				{
					vector += this.currentLocalVibration[i].UpdateIntensity(Time.deltaTime);
				}
				else
				{
					this.currentLocalVibration.RemoveAt(i);
				}
			}
			Vector2 cameraPos2 = this.cam.transform.position;
			for (int j = this.currentWorldVibration.Count - 1; j >= 0; j--)
			{
				if (this.currentWorldVibration[j].Alive)
				{
					vector += this.currentWorldVibration[j].UpdateIntensity(cameraPos2, Time.deltaTime);
				}
				else
				{
					this.currentWorldVibration.RemoveAt(j);
				}
			}
			this.currentVibration = vector;
			vector.x = Mathf.Clamp01(vector.x);
			vector.y = Mathf.Clamp01(vector.y);
			if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
			{
				player.SetVibration(0, vector.x);
				player.SetVibration(1, vector.y);
			}
			this.zeroNextFrame = true;
		}
		else if (this.zeroNextFrame)
		{
			Player player2 = ReInput.players.GetPlayer(0);
			player2.SetVibration(0, 0f);
			player2.SetVibration(1, 0f);
		}
		this.numVibrationsActive = this.currentLocalVibration.Count + this.currentWorldVibration.Count;
	}

	public static void CancelVibration(AudioClip clipToCancel)
	{
		for (int i = DestroyableSingleton<VibrationManager>.Instance.currentLocalVibration.Count - 1; i >= 0; i--)
		{
			if (DestroyableSingleton<VibrationManager>.Instance.currentLocalVibration[i].clip == clipToCancel)
			{
				DestroyableSingleton<VibrationManager>.Instance.currentLocalVibration.RemoveAt(i);
				return;
			}
		}
		for (int j = DestroyableSingleton<VibrationManager>.Instance.currentLocalVibration.Count - 1; j >= 0; j--)
		{
			if (DestroyableSingleton<VibrationManager>.Instance.currentWorldVibration[j].clip == clipToCancel)
			{
				DestroyableSingleton<VibrationManager>.Instance.currentWorldVibration.RemoveAt(j);
				return;
			}
		}
	}

	public static void Vibrate(float left, float right)
	{
		DestroyableSingleton<VibrationManager>.Instance.singleFrameVibration += new Vector2(left, right);
		DestroyableSingleton<VibrationManager>.Instance.hasFrameVibration = true;
	}

	public static void Vibrate(float left, float right, float duration, VibrationManager.VibrationFalloff falloffType = VibrationManager.VibrationFalloff.None, AudioClip sourceClip = null, bool loopClip = false)
	{
		VibrationManager.LocalVibration localVibration = new VibrationManager.LocalVibration();
		localVibration.intensity = new Vector2(left, right);
		localVibration.duration = duration;
		localVibration.falloff = falloffType;
		localVibration.t = 0f;
		localVibration.clip = sourceClip;
		localVibration.loopClip = loopClip;
		localVibration.Init();
		DestroyableSingleton<VibrationManager>.Instance.currentLocalVibration.Add(localVibration);
	}

	public static void Vibrate(float intensity, Vector2 worldPosition, float radius)
	{
		DestroyableSingleton<VibrationManager>.Instance.tempSingleFrameWorldVibration.intensity = intensity;
		DestroyableSingleton<VibrationManager>.Instance.tempSingleFrameWorldVibration.location = worldPosition;
		DestroyableSingleton<VibrationManager>.Instance.tempSingleFrameWorldVibration.radius = radius;
		Vector2 cameraPos = DestroyableSingleton<VibrationManager>.Instance.cam.transform.position;
		DestroyableSingleton<VibrationManager>.Instance.singleFrameVibration += DestroyableSingleton<VibrationManager>.Instance.tempSingleFrameWorldVibration.UpdateIntensity(cameraPos, 0f);
		DestroyableSingleton<VibrationManager>.Instance.hasFrameVibration = true;
	}

	public static void Vibrate(float intensity, Vector2 worldPosition, float radius, float duration, VibrationManager.VibrationFalloff falloffType = VibrationManager.VibrationFalloff.None, AudioClip sourceClip = null, bool loopClip = false)
	{
		VibrationManager.WorldVibration worldVibration = new VibrationManager.WorldVibration();
		worldVibration.intensity = intensity;
		worldVibration.location = worldPosition;
		worldVibration.duration = duration;
		worldVibration.falloff = falloffType;
		worldVibration.radius = radius;
		worldVibration.t = 0f;
		worldVibration.clip = sourceClip;
		worldVibration.loopClip = loopClip;
		worldVibration.Init();
		DestroyableSingleton<VibrationManager>.Instance.currentWorldVibration.Add(worldVibration);
	}

	public enum VibrationFalloff
	{
		None,
		Linear,
		InverseLinear
	}

	private class LocalVibration
	{
		public Vector2 intensity;

		public float t;

		public float duration;

		public VibrationManager.VibrationFalloff falloff;

		public AudioClip clip;

		public bool loopClip;

		public void Init()
		{
			if (this.clip != null)
			{
				this.duration = Mathf.Max(this.duration, this.clip.length);
			}
		}

		public bool Alive
		{
			get
			{
				return this.t <= this.duration;
			}
		}

		public Vector2 UpdateIntensity(float deltaTime)
		{
			float num = 1f;
			VibrationManager.VibrationFalloff vibrationFalloff = this.falloff;
			if (vibrationFalloff != VibrationManager.VibrationFalloff.Linear)
			{
				if (vibrationFalloff == VibrationManager.VibrationFalloff.InverseLinear)
				{
					num = Mathf.Clamp01(this.t / this.duration);
				}
			}
			else
			{
				float num2 = Mathf.Clamp01(this.t / this.duration);
				num = 1f - num2;
			}
			if (this.clip != null)
			{
				float num3 = this.t / this.clip.length;
				if (num3 > 1f)
				{
					num = 0f;
					this.t = this.duration;
				}
				else
				{
					int num4 = (int)(Mathf.Clamp01(num3) * (float)(this.clip.samples - 1));
					if (this.clip.GetData(VibrationManager.samples, num4))
					{
						num *= Mathf.Abs(VibrationManager.samples[0]);
					}
				}
			}
			this.t += deltaTime;
			if (this.t >= this.duration && this.loopClip)
			{
				this.t -= this.duration;
				if (this.t < 0f)
				{
					this.t = 0f;
				}
			}
			return this.intensity * num;
		}
	}

	private class WorldVibration
	{
		public float intensity;

		public Vector2 location;

		public float radius;

		public float t;

		public float duration;

		public VibrationManager.VibrationFalloff falloff;

		public AudioClip clip;

		public bool loopClip;

		public void Init()
		{
			if (this.clip != null)
			{
				this.duration = Mathf.Max(this.duration, this.clip.length);
			}
		}

		public bool Alive
		{
			get
			{
				return this.t <= this.duration;
			}
		}

		public Vector2 UpdateIntensity(Vector2 cameraPos, float deltaTime)
		{
			float num = 1f;
			VibrationManager.VibrationFalloff vibrationFalloff = this.falloff;
			if (vibrationFalloff != VibrationManager.VibrationFalloff.Linear)
			{
				if (vibrationFalloff == VibrationManager.VibrationFalloff.InverseLinear)
				{
					num = Mathf.Clamp01(this.t / this.duration);
				}
			}
			else
			{
				float num2 = Mathf.Clamp01(this.t / this.duration);
				num = 1f - num2;
			}
			Vector2 vector = this.location - cameraPos;
			float magnitude = vector.magnitude;
			vector /= magnitude;
			Vector2 vector2 = new Vector2(1f - Mathf.Clamp01(vector.x), 1f - Mathf.Clamp01(-vector.x));
			num *= 1f - Mathf.Clamp01(magnitude / this.radius);
			if (this.clip != null)
			{
				float num3 = this.t / this.clip.length;
				if (num3 > 1f)
				{
					num = 0f;
					this.t = this.duration;
				}
				else if (this.clip.channels == 2)
				{
					int num4 = (int)(Mathf.Clamp01(num3) * (float)(this.clip.samples / 2 - 1)) * 2;
					if (this.clip.GetData(VibrationManager.samples, num4))
					{
						vector2.x *= Mathf.Abs(VibrationManager.samples[0]);
						vector2.y *= Mathf.Abs(VibrationManager.samples[1]);
					}
				}
				else
				{
					int num5 = (int)(Mathf.Clamp01(num3) * (float)(this.clip.samples - 1));
					if (this.clip.GetData(VibrationManager.samples, num5))
					{
						num *= Mathf.Abs(VibrationManager.samples[0]);
					}
				}
			}
			this.t += deltaTime;
			if (this.t >= this.duration && this.loopClip)
			{
				this.t -= this.duration;
				if (this.t < 0f)
				{
					this.t = 0f;
				}
			}
			return this.intensity * num * vector2;
		}
	}
}
