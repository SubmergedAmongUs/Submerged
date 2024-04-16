using System;

[AttributeUsage(AttributeTargets.Field)]
public class QuickChatMenuDisplayAttribute : Attribute
{
	public QuickChatMenuItem.QuickChatMenuItemType propertyMenuType;

	public QuickChatMenuDisplayAttribute(QuickChatMenuItem.QuickChatMenuItemType displayType)
	{
		this.propertyMenuType = displayType;
	}
}
