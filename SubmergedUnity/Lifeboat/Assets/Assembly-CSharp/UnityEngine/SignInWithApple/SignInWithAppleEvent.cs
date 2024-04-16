using System;
using UnityEngine.Events;

namespace UnityEngine.SignInWithApple
{
	[Serializable]
	public class SignInWithAppleEvent : UnityEvent<SignInWithApple.CallbackArgs>
	{
	}
}
