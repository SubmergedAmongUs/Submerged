/*
 * Copyright (c) 2015,2016 Beebyte Limited. All rights reserved. 
 */
using System;

namespace Beebyte.Obfuscator
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Event | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Delegate)]
	public class SkipRenameAttribute : System.Attribute
	{
		public SkipRenameAttribute()
		{
		}
	}
}
