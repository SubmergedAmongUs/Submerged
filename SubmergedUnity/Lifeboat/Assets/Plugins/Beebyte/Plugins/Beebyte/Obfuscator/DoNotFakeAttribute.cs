/*
 * Copyright (c) 2015,2016 Beebyte Limited. All rights reserved. 
 */
using System;

namespace Beebyte.Obfuscator
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
	public class DoNotFakeAttribute: System.Attribute
	{
		public DoNotFakeAttribute()
		{
		}
	}
}
