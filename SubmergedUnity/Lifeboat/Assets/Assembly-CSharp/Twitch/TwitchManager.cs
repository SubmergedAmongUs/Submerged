using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ImaginationOverflow.UniversalDeepLinking;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Twitch
{
	public class TwitchManager : DestroyableSingleton<TwitchManager>
	{
		private const string RedirectUri = "AmongUs://callback";

		private const string ClientId = "yioca4gf70qx0v05qodt6tnwlkerr3";

		private static readonly string[] Scopes = new string[0];

		private string verify;

		public GenericPopup TwitchPopup;

		public bool running;

		public string Token { get; set; }

		private void Start()
		{
			LinkProviderFactory.DeferredExePath = Path.Combine(Application.dataPath.Replace('/', '\\'), "Resources", "AmongUsHelper.exe");
			DeepLinkManager.Instance.LinkActivated += new LinkActivationHandler(this.Instance_LinkActivated);
		}

		private void Instance_LinkActivated(LinkActivation s)
		{
			Debug.Log("Twitch link activated " + s.Uri);
			if (!s.Uri.StartsWith("amongus://callback", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			Debug.Log("Twitch link is correct format");
			string text = s.RawQueryString;
			int num = text.IndexOf('#');
			if (num != -1)
			{
				text = text.Substring(num + 1);
			}
			string path = Path.Combine(PlatformPaths.persistentDataPath, "twitch_verify");
			if (!FileIO.Exists(path))
			{
				return;
			}
			this.verify = FileIO.ReadAllText(path);
			string[] source = text.Split(new char[]
			{
				'&'
			});
			string text2 = source.First((string a) => a.StartsWith("state"));
			string b = text2.Substring(text2.LastIndexOf('=') + 1);
			if (this.verify != b)
			{
				return;
			}
			string text3 = source.First((string a) => a.StartsWith("access_token"));
			string text4 = text3.Substring(text3.LastIndexOf('=') + 1);
			FileIO.WriteAllText(Path.Combine(PlatformPaths.persistentDataPath, "twitch"), text4);
			this.Token = text4;
			this.LaunchImplicitAuthAsync();
		}

		private new void OnDestroy()
		{
			DeepLinkManager.Instance.LinkActivated -= new LinkActivationHandler(this.Instance_LinkActivated);
		}

		public void LaunchImplicitAuth(Transform target)
		{
			if (this.running)
			{
				return;
			}
			this.running = true;
			base.StartCoroutine(this.ShakeGlitch(target));
			this.LaunchImplicitAuthAsync();
		}

		private IEnumerator ShakeGlitch(Transform target)
		{
			while (DestroyableSingleton<TwitchManager>.Instance.running)
			{
				yield return Effects.Bounce(base.transform, 1f, 0.2f);
				yield return Effects.Wait(0.3f);
			}
			yield break;
		}

		private async void LaunchImplicitAuthAsync()
		{
			
		}

		private Task FetchNewToken()
		{
			return default;
		}

		public async Task<HttpStatusCode> FetchEntitlements(string token)
		{
			using (HttpClient http = new HttpClient())
			{
				using (HttpRequestMessage msg = new HttpRequestMessage())
				{
					msg.Method = HttpMethod.Get;
					msg.RequestUri = new Uri("https://api.twitch.tv/helix/entitlements/drops");
					msg.Headers.TryAddWithoutValidation("Authorization", "Bearer " + token);
					msg.Headers.TryAddWithoutValidation("Client-Id", "yioca4gf70qx0v05qodt6tnwlkerr3");
					HttpResponseMessage httpResponseMessage = await http.SendAsync(msg);
					using (HttpResponseMessage res = httpResponseMessage)
					{
						try
						{
							if (res.Content == null)
							{
								Debug.Log("Server returned no data: " + res.StatusCode.ToString());
								return HttpStatusCode.ExpectationFailed;
							}
							if (res.StatusCode != HttpStatusCode.OK)
							{
								return res.StatusCode;
							}
							string text = await res.Content.ReadAsStringAsync();
							JToken jtoken = JObject.Parse(text)["data"];
							if (!jtoken.HasValues)
							{
								Debug.Log("Server returned no drops: " + text);
								return HttpStatusCode.ExpectationFailed;
							}
							foreach (JToken jtoken2 in jtoken)
							{
								string text2 = jtoken2.Value<string>("benefit_id");
								Debug.Log("Drop unlocked: " + text2);
								SaveManager.SetPurchased(text2);
							}
						}
						catch (Exception ex)
						{
							Debug.LogException(ex);
						}
					}
				}
			}
			return HttpStatusCode.OK;
		}
	}
}
