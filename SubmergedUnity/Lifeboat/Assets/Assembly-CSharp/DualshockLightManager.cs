using System;
using System.Collections.Generic;
using Rewired;
using Rewired.ControllerExtensions;
using UnityEngine;

public class DualshockLightManager : DestroyableSingleton<DualshockLightManager>
{
	private const float lightIntensity = 0.5f;

	private Color baseColor = new Color(0f, 0.5f, 1f, 1f);

	private Color oldColor = Color.white;

	private List<DualshockLightManager.LightOverlayHandle> overlays = new List<DualshockLightManager.LightOverlayHandle>();

	private List<DualshockLightManager.LightOverlayFlash> lightOverlayFlashes = new List<DualshockLightManager.LightOverlayFlash>();

	public Color BaseColor
	{
		get
		{
			return this.baseColor;
		}
		set
		{
			this.baseColor = value;
		}
	}

	public DualshockLightManager.LightOverlayHandle AllocateLight()
	{
		DualshockLightManager.LightOverlayHandle lightOverlayHandle = new DualshockLightManager.LightOverlayHandle();
		this.overlays.Add(lightOverlayHandle);
		return lightOverlayHandle;
	}

	private float GetExternalBrightnessFromElectrical()
	{
		if (ShipStatus.Instance)
		{
			float num = (float)((SwitchSystem)ShipStatus.Instance.Systems[SystemTypes.Electrical]).Value / 255f;
			return Mathf.Lerp(0.05f, 1f, num);
		}
		return 1f;
	}

	private void Update()
	{
		Player player = ReInput.players.GetPlayer(0);
		if (player.controllers.joystickCount > 0)
		{
			IDualShock4Extension extension = player.controllers.Joysticks[0].GetExtension<IDualShock4Extension>();
			if (extension != null)
			{
				if (ShipStatus.Instance || LobbyBehaviour.Instance)
				{
					Color color = this.baseColor;
					color.a *= this.GetExternalBrightnessFromElectrical() * 0.5f;
					for (int i = this.lightOverlayFlashes.Count - 1; i >= 0; i--)
					{
						if (this.lightOverlayFlashes[i].Alive)
						{
							this.lightOverlayFlashes[i].Update(Time.deltaTime);
						}
						else
						{
							this.lightOverlayFlashes[i].Dispose();
							this.lightOverlayFlashes.RemoveAt(i);
						}
					}
					foreach (DualshockLightManager.LightOverlayHandle lightOverlayHandle in this.overlays)
					{
						Color color2 = lightOverlayHandle.color;
						color2.a = lightOverlayHandle.intensity;
						color = Color.Lerp(color, color2, lightOverlayHandle.color.a);
					}
					if (this.oldColor != color)
					{
						this.oldColor = color;
						if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
						{
							extension.SetLightColor(color);
							return;
						}
					}
				}
				else if (this.oldColor != Palette.Blue)
				{
					this.oldColor = Palette.Blue;
					if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
					{
						extension.SetLightColor(Palette.Blue);
					}
				}
			}
		}
	}

	public static void Flash(Color c, float intensity, AudioClip clip)
	{
		DualshockLightManager.LightOverlayFlash lightOverlayFlash = new DualshockLightManager.LightOverlayFlash();
		lightOverlayFlash.handle = DestroyableSingleton<DualshockLightManager>.Instance.AllocateLight();
		lightOverlayFlash.handle.color = c;
		lightOverlayFlash.handle.intensity = intensity;
		lightOverlayFlash.clip = clip;
		lightOverlayFlash.Init();
		DestroyableSingleton<DualshockLightManager>.Instance.lightOverlayFlashes.Add(lightOverlayFlash);
	}

	public class LightOverlayHandle
	{
		public Color color;

		public float intensity = 1f;

		public void Dispose()
		{
			DestroyableSingleton<DualshockLightManager>.Instance.overlays.Remove(this);
		}
	}

	public class LightOverlayFlash
	{
		public DualshockLightManager.LightOverlayHandle handle;

		public AudioClip clip;

		public float t;

		public float duration;

		private static float[] samples = new float[2];

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

		public void Dispose()
		{
			if (this.handle != null)
			{
				this.handle.Dispose();
				this.handle = null;
			}
		}

		public void Update(float deltaTime)
		{
			float num = 1f;
			if (this.clip != null)
			{
				float num2 = this.t / this.clip.length;
				if (num2 > 1f)
				{
					num = 0f;
					this.t = this.duration;
				}
				else
				{
					int num3 = (int)(Mathf.Clamp01(num2) * (float)(this.clip.samples - 1));
					if (this.clip.GetData(DualshockLightManager.LightOverlayFlash.samples, num3))
					{
						num *= Mathf.Abs(DualshockLightManager.LightOverlayFlash.samples[0]);
					}
				}
			}
			this.t += deltaTime;
			this.handle.color.a = Mathf.Clamp01(num * 10f);
		}
	}
}
