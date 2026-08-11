using System;

namespace SuperCore.RunTime;

/// <summary>
/// Defines the lifecycle controlled by the SuperCore startup runner.
/// </summary>
internal interface IModule
{
	void Create(Action completeInit);

	void Init();

	void Update(double deltaTime, double unscaledDeltaTime);

	void LateUpdate(double deltaTime, double unscaledDeltaTime);

	void Clear();

	void Destroy();
}
