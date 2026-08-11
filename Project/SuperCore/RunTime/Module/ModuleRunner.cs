using System;
using System.Collections.Generic;

namespace SuperCore.RunTime;

/// <summary>
/// Owns creation, sequential initialization, frame dispatch, and shutdown of plain C# modules.
/// </summary>
internal sealed class ModuleRunner
{
	private IModule?[]? m_Modules;
	private int m_InitIndex;

	internal bool IsRunning => m_Modules is { Length: > 0 };

	internal bool IsFullyInitialized => m_Modules is { Length: > 0 } && m_InitIndex == m_Modules.Length;

	internal void Start(IReadOnlyList<Type> moduleTypes)
	{
		ArgumentNullException.ThrowIfNull(moduleTypes);
		if (m_Modules is not null)
			throw new InvalidOperationException("ModuleRunner has already started.");
		if (moduleTypes.Count == 0)
			throw new InvalidOperationException("At least one Module is required.");

		ValidateModuleTypes(moduleTypes);

		m_Modules = new IModule?[moduleTypes.Count];
		m_InitIndex = 0;
		try
		{
			for (int i = 0; i < moduleTypes.Count; i++)
			{
				Type moduleType = moduleTypes[i];
				IModule module = Activator.CreateInstance(moduleType, nonPublic: true) as IModule
								 ?? throw new InvalidOperationException(
									 $"Create Module failed: {moduleType.FullName}.");

				m_Modules[i] = module;
				module.Create(CompleteInitCallback);
			}

			m_Modules[0]!.Init();
		}
		catch
		{
			TryShutdownAfterFailedStart();
			throw;
		}
	}

	internal void Update(double deltaTime, double unscaledDeltaTime)
	{
		if (m_Modules is null)
			return;

		int initializedCount = m_InitIndex;
		for (int i = 0; i < initializedCount; i++)
			m_Modules[i]!.Update(deltaTime, unscaledDeltaTime);

		for (int i = 0; i < initializedCount; i++)
			m_Modules[i]!.LateUpdate(deltaTime, unscaledDeltaTime);
	}

	internal void Shutdown()
	{
		if (m_Modules is null)
		{
			m_InitIndex = 0;
			return;
		}

		List<Exception>? exceptions = null;
		for (int i = m_Modules.Length - 1; i >= 0; i--)
		{
			IModule? module = m_Modules[i];
			if (module is null)
				continue;

			try
			{
				module.Clear();
			}
			catch (Exception exception)
			{
				(exceptions ??= new List<Exception>()).Add(exception);
			}

			try
			{
				module.Destroy();
			}
			catch (Exception exception)
			{
				(exceptions ??= new List<Exception>()).Add(exception);
			}
		}

		m_Modules = null;
		m_InitIndex = 0;
		ModuleStaticReset.ResetAll();

		if (exceptions is not null)
			throw new AggregateException("One or more Modules failed to shut down.", exceptions);
	}

	private static void ValidateModuleTypes(IReadOnlyList<Type> moduleTypes)
	{
		HashSet<Type> uniqueTypes = new();
		for (int i = 0; i < moduleTypes.Count; i++)
		{
			Type moduleType = moduleTypes[i]
							  ?? throw new InvalidOperationException($"Module type at index {i} is null.");
			if (moduleType.IsAbstract || !typeof(IModule).IsAssignableFrom(moduleType))
				throw new InvalidOperationException($"Invalid Module type: {moduleType.FullName}.");
			if (!uniqueTypes.Add(moduleType))
				throw new InvalidOperationException($"Duplicate Module type: {moduleType.FullName}.");
		}
	}

	private void CompleteInitCallback()
	{
		if (m_Modules is null)
			throw new InvalidOperationException("Module initialization completed after runner shutdown.");

		m_InitIndex++;
		if (m_InitIndex < m_Modules.Length)
			m_Modules[m_InitIndex]!.Init();
	}

	private void TryShutdownAfterFailedStart()
	{
		try
		{
			Shutdown();
		}
		catch
		{
			// Preserve the startup exception. Shutdown failures are reported by normal shutdown tests.
		}
	}
}
