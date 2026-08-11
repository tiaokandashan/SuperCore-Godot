using System;
using System.Collections.Generic;
using System.Reflection;

namespace SuperCore.RunTime;

internal readonly struct ModuleRegistration
{
	internal ModuleRegistration(Type moduleType, int priority)
	{
		ModuleType = moduleType;
		Priority = priority;
	}

	internal Type ModuleType { get; }

	internal int Priority { get; }
}

internal static class ModuleCollector
{
	internal static bool TryCollect(out Type[] moduleTypes, out string error)
	{
		List<ModuleRegistration> registrations = new();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			Assembly assembly = assemblies[i];
			Type?[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException exception)
			{
				types = exception.Types;
			}
			catch (Exception exception)
			{
				moduleTypes = Array.Empty<Type>();
				error = $"Scan assembly failed: {assembly.FullName}. {exception.Message}";
				return false;
			}

			for (int j = 0; j < types.Length; j++)
			{
				Type? type = types[j];
				if (type is null)
					continue;

				ModuleAttribute? attribute = type.GetCustomAttribute<ModuleAttribute>(inherit: false);
				if (attribute is not null)
					registrations.Add(new ModuleRegistration(type, attribute.Priority));
			}
		}

		return TryOrder(registrations, out moduleTypes, out error);
	}

	internal static bool TryOrder(
		IReadOnlyList<ModuleRegistration> registrations,
		out Type[] moduleTypes,
		out string error)
	{
		ArgumentNullException.ThrowIfNull(registrations);
		if (registrations.Count == 0)
		{
			moduleTypes = Array.Empty<Type>();
			error = "No Module type has ModuleAttribute.";
			return false;
		}

		List<ModuleRegistration> orderedRegistrations = new(registrations.Count);
		HashSet<Type> uniqueTypes = new();
		for (int i = 0; i < registrations.Count; i++)
		{
			ModuleRegistration registration = registrations[i];
			Type? moduleType = registration.ModuleType;
			if (moduleType is null || moduleType.IsAbstract || moduleType.IsInterface ||
				moduleType.ContainsGenericParameters || !typeof(IModule).IsAssignableFrom(moduleType))
			{
				moduleTypes = Array.Empty<Type>();
				error = $"Invalid Module type: {moduleType?.FullName ?? "<null>"}.";
				return false;
			}

			if (string.IsNullOrEmpty(moduleType.FullName))
			{
				moduleTypes = Array.Empty<Type>();
				error = $"Module type has no full name: {moduleType}.";
				return false;
			}

			ConstructorInfo? constructor = moduleType.GetConstructor(
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
				binder: null,
				Type.EmptyTypes,
				modifiers: null);
			if (constructor is null)
			{
				moduleTypes = Array.Empty<Type>();
				error = $"Module type has no parameterless constructor: {moduleType.FullName}.";
				return false;
			}

			if (!uniqueTypes.Add(moduleType))
			{
				moduleTypes = Array.Empty<Type>();
				error = $"Duplicate Module type: {moduleType.FullName}.";
				return false;
			}

			orderedRegistrations.Add(registration);
		}

		orderedRegistrations.Sort(CompareRegistration);
		if (orderedRegistrations[0].ModuleType != typeof(ResModule))
		{
			moduleTypes = Array.Empty<Type>();
			error = $"The first Module must be {typeof(ResModule).FullName}.";
			return false;
		}

		moduleTypes = new Type[orderedRegistrations.Count];
		for (int i = 0; i < orderedRegistrations.Count; i++)
			moduleTypes[i] = orderedRegistrations[i].ModuleType;

		error = string.Empty;
		return true;
	}

	private static int CompareRegistration(ModuleRegistration left, ModuleRegistration right)
	{
		int priorityComparison = left.Priority.CompareTo(right.Priority);
		if (priorityComparison != 0)
			return priorityComparison;

		return string.Compare(left.ModuleType.FullName, right.ModuleType.FullName, StringComparison.Ordinal);
	}
}
