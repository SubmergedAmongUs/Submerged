/*
 * Copyright (c) 2016 Beebyte Limited. All rights reserved. 
 */
using System;

namespace Beebyte.Obfuscator
{
	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface|AttributeTargets.Struct|AttributeTargets.Method|AttributeTargets.Enum|AttributeTargets.Field|AttributeTargets.Property|AttributeTargets.Event|AttributeTargets.Delegate)]
	public class ReplaceLiteralsWithNameAttribute : System.Attribute
	{
		public ReplaceLiteralsWithNameAttribute()
		{
		}
	}
}
