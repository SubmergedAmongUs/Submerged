/*
 * Copyright (c) 2015,2016 Beebyte Limited. All rights reserved. 
 */
using System;

namespace Beebyte.Obfuscator
{
	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface|AttributeTargets.Struct|AttributeTargets.Method|AttributeTargets.Enum|AttributeTargets.Field|AttributeTargets.Property|AttributeTargets.Delegate)]
	public class RenameAttribute : System.Attribute
	{
		private readonly string target;
		
		private RenameAttribute()
		{
		}

		public RenameAttribute(string target)
		{
			this.target = target;
		}
		
		public string GetTarget()
		{
			return target;
		}
	}
}
