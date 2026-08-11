using System;

namespace SuperCore.RunTime;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ModuleAttribute : Attribute
{
	public ModuleAttribute(int priority)
	{
		Priority = priority;
	}

	public int Priority { get; }
}
