using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

namespace UnityEngine.SignInWithApple
{
	public class SignInWithApple : MonoBehaviour
	{
		private static SignInWithApple.Callback s_LoginCompletedCallback;

		private static SignInWithApple.Callback s_CredentialStateCallback;

		private static readonly Queue<Action> s_EventQueue = new Queue<Action>();

		[Header("Event fired when login is complete.")]
		public SignInWithAppleEvent onLogin;

		[Header("Event fired when the users credential state has been retrieved.")]
		public SignInWithAppleEvent onCredentialState;

		[Header("Event fired when there is an error.")]
		public SignInWithAppleEvent onError;

		[MonoPInvokeCallback(typeof(SignInWithApple.LoginCompleted))]
		private static void LoginCompletedCallback(int result, [MarshalAs(UnmanagedType.Struct)] UserInfo info)
		{
			SignInWithApple.CallbackArgs args = default(SignInWithApple.CallbackArgs);
			if (result != 0)
			{
				args.userInfo = new UserInfo
				{
					idToken = info.idToken,
					displayName = info.displayName,
					email = info.email,
					userId = info.userId,
					userDetectionStatus = info.userDetectionStatus
				};
			}
			else
			{
				args.error = info.error;
			}
			SignInWithApple.s_LoginCompletedCallback(args);
			SignInWithApple.s_LoginCompletedCallback = null;
		}

		[MonoPInvokeCallback(typeof(SignInWithApple.GetCredentialStateCompleted))]
		private static void GetCredentialStateCallback([MarshalAs(UnmanagedType.SysInt)] UserCredentialState state)
		{
			SignInWithApple.CallbackArgs args = new SignInWithApple.CallbackArgs
			{
				credentialState = state
			};
			SignInWithApple.s_CredentialStateCallback(args);
			SignInWithApple.s_CredentialStateCallback = null;
		}

		public void GetCredentialState(string userID)
		{
			this.GetCredentialState(userID, new SignInWithApple.Callback(this.TriggerCredentialStateEvent));
		}

		public void GetCredentialState(string userID, SignInWithApple.Callback callback)
		{
			if (userID == null)
			{
				throw new InvalidOperationException("Credential state fetch called without a user id.");
			}
			if (SignInWithApple.s_CredentialStateCallback != null)
			{
				throw new InvalidOperationException("Credential state fetch called while another request is in progress");
			}
			SignInWithApple.s_CredentialStateCallback = callback;
			this.GetCredentialStateInternal(userID);
		}

		private void GetCredentialStateInternal(string userID)
		{
		}

		public void Login()
		{
			this.Login(new SignInWithApple.Callback(this.TriggerOnLoginEvent));
		}

		public void Login(SignInWithApple.Callback callback)
		{
			if (SignInWithApple.s_LoginCompletedCallback != null)
			{
				throw new InvalidOperationException("Login called while another login is in progress");
			}
			SignInWithApple.s_LoginCompletedCallback = callback;
			this.LoginInternal();
		}

		private void LoginInternal()
		{
		}

		private void TriggerOnLoginEvent(SignInWithApple.CallbackArgs args)
		{
			if (args.error != null)
			{
				this.TriggerOnErrorEvent(args);
				return;
			}
			SignInWithApple.s_EventQueue.Enqueue(delegate
			{
				if (this.onLogin != null)
				{
					this.onLogin.Invoke(args);
				}
			});
		}

		private void TriggerCredentialStateEvent(SignInWithApple.CallbackArgs args)
		{
			if (args.error != null)
			{
				this.TriggerOnErrorEvent(args);
				return;
			}
			SignInWithApple.s_EventQueue.Enqueue(delegate
			{
				if (this.onCredentialState != null)
				{
					this.onCredentialState.Invoke(args);
				}
			});
		}

		private void TriggerOnErrorEvent(SignInWithApple.CallbackArgs args)
		{
			SignInWithApple.s_EventQueue.Enqueue(delegate
			{
				if (this.onError != null)
				{
					this.onError.Invoke(args);
				}
			});
		}

		public void Update()
		{
			while (SignInWithApple.s_EventQueue.Count > 0)
			{
				SignInWithApple.s_EventQueue.Dequeue()();
			}
		}

		public struct CallbackArgs
		{
			public UserCredentialState credentialState;

			public UserInfo userInfo;

			public string error;
		}

		public delegate void Callback(SignInWithApple.CallbackArgs args);

		private delegate void LoginCompleted(int result, UserInfo info);

		private delegate void GetCredentialStateCompleted(UserCredentialState state);
	}
}
