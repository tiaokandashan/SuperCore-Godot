using System;
using Godot;

namespace SuperCore.RunTime;

/// <summary>
/// Godot lifecycle host for the SuperCore framework.
/// </summary>
public sealed partial class SuperCore : Node
{
	private static SuperCore? m_Instance;

	private ModuleRunner? m_ModuleRunner;
	private ulong m_LastUnscaledTickUsec;
	private bool m_IsPrimaryInstance;

	public override void _EnterTree()
	{
		if (m_Instance is not null)
		{
			GD.PushError("[SuperCore->_EnterTree] A SuperCore instance already exists.");
			SetProcess(false);
			QueueFree();
			return;
		}

		m_Instance = this;
		m_IsPrimaryInstance = true;
		m_ModuleRunner = new ModuleRunner();
		m_LastUnscaledTickUsec = Time.GetTicksUsec();
		SetProcess(false);
	}

	public override void _Ready()
	{
		if (!m_IsPrimaryInstance)
			return;

		if (!ModuleCollector.TryCollect(out Type[] moduleTypes, out string error))
		{
			GD.PushError($"[SuperCore->_Ready] {error}");
			return;
		}

		StartModules(moduleTypes);
	}

	public override void _Process(double delta)
	{
		if (!m_IsPrimaryInstance || m_ModuleRunner is not { IsRunning: true })
			return;

		ulong currentTickUsec = Time.GetTicksUsec();
		double unscaledDelta = (currentTickUsec - m_LastUnscaledTickUsec) / 1_000_000.0;
		m_LastUnscaledTickUsec = currentTickUsec;
		m_ModuleRunner.Update(delta, unscaledDelta);
	}

	public override void _ExitTree()
	{
		if (!m_IsPrimaryInstance)
			return;

		SetProcess(false);
		try
		{
			m_ModuleRunner?.Shutdown();
		}
		catch (Exception exception)
		{
			GD.PushError($"[SuperCore->_ExitTree] {exception}");
		}
		finally
		{
			m_ModuleRunner = null;
			m_LastUnscaledTickUsec = 0;
			m_IsPrimaryInstance = false;
			m_Instance = null;
		}
	}

	internal void StartModules(Type[] moduleTypes)
	{
		if (!m_IsPrimaryInstance || m_ModuleRunner is null)
			throw new InvalidOperationException("SuperCore is not active in the SceneTree.");

		m_ModuleRunner.Start(moduleTypes);
		m_LastUnscaledTickUsec = Time.GetTicksUsec();
		SetProcess(true);
	}
}
