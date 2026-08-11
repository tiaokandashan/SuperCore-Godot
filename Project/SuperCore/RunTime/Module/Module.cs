using System;
using Godot;
using System.Collections.Generic;

namespace SuperCore.RunTime;

/// <summary>
/// Base class for a plain C# SuperCore module.
/// </summary>
/// <typeparam name="T">The concrete module type.</typeparam>
public abstract class Module<T> : IModule where T : Module<T>
{
	private static T? m_Instance;

	private Action? m_CompleteInitCallback;
	private bool m_IsCompleteInit;

	static Module()
	{
		ModuleStaticReset.Register(ResetStatic);
	}

	/// <summary>
	/// Gets the active module instance.
	/// </summary>
	public static T Get()
	{
		return m_Instance
			   ?? throw new InvalidOperationException($"{typeof(T).Name} is unavailable.");
	}

	internal bool IsInitComplete => m_IsCompleteInit;

	/// <summary>
	/// Completes this module's initialization and starts the next module.
	/// </summary>
	protected void CompleteInit()
	{
		if (m_IsCompleteInit)
			throw new InvalidOperationException($"{typeof(T).Name} repeated CompleteInit.");

		Action callback = m_CompleteInitCallback
						  ?? throw new InvalidOperationException(
							  $"{typeof(T).Name} cannot complete initialization after destruction.");

		m_IsCompleteInit = true;
		GD.PushError($"[Module->CompleteInit] {typeof(T).Name} initialization completed.");
		callback.Invoke();
	}

	void IModule.Create(Action completeInit)
	{
		ArgumentNullException.ThrowIfNull(completeInit);
		if (m_Instance is not null)
			throw new InvalidOperationException($"{typeof(T).Name} already exists.");
		if (this is not T instance)
			throw new InvalidOperationException(
				$"{GetType().Name} does not match its module generic type {typeof(T).Name}.");

		m_Instance = instance;
		m_CompleteInitCallback = completeInit;
		m_IsCompleteInit = false;
	}

	void IModule.Init()
	{
		OnInit();
	}

	void IModule.Update(double deltaTime, double unscaledDeltaTime)
	{
		OnUpdate(deltaTime, unscaledDeltaTime);
	}

	void IModule.LateUpdate(double deltaTime, double unscaledDeltaTime)
	{
		OnLateUpdate(deltaTime, unscaledDeltaTime);
	}

	void IModule.Clear()
	{
		OnClear();
	}

	void IModule.Destroy()
	{
		if (m_Instance is null)
			throw new InvalidOperationException($"{typeof(T).Name} does not exist.");

		m_CompleteInitCallback = null;
		m_IsCompleteInit = false;
		m_Instance = null;
	}

	protected virtual void OnInit()
	{
	}

	protected virtual void OnUpdate(double deltaTime, double unscaledDeltaTime)
	{
	}

	protected virtual void OnLateUpdate(double deltaTime, double unscaledDeltaTime)
	{
	}

	protected virtual void OnClear()
	{
	}

	private static void ResetStatic()
	{
		m_Instance = null;
	}
}

internal static class ModuleStaticReset
{
	private static readonly List<Action> s_ResetActions = new();

	internal static void Register(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);
		if (!s_ResetActions.Contains(action))
			s_ResetActions.Add(action);
	}

	internal static void ResetAll()
	{
		for (int i = 0; i < s_ResetActions.Count; i++)
			s_ResetActions[i].Invoke();
	}
}
