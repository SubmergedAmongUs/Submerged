using System;
using UnityEngine;

public abstract class OptionBehaviour : MonoBehaviour
{
	public StringNames Title;

	public Action<OptionBehaviour> OnValueChanged;

	public virtual float GetFloat()
	{
		throw new NotImplementedException();
	}

	public virtual int GetInt()
	{
		throw new NotImplementedException();
	}

	public virtual bool GetBool()
	{
		throw new NotImplementedException();
	}

	public void SetAsPlayer()
	{
		PassiveButton[] componentsInChildren = base.GetComponentsInChildren<PassiveButton>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.SetActive(false);
		}
	}
}
