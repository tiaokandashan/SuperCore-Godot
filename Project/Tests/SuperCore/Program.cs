using System;
using System.Collections.Generic;
using SuperCore.RunTime;

namespace SuperCore.Tests;

internal static class Program
{
    private static int Main()
    {
        try
        {
            VerifySequentialInitializationAndFrameOrder();
            VerifyRepeatedCompleteInitIsRejected();
            VerifyModuleCollectionAndSkeletonLifecycle();
            VerifyModuleCollectorValidation();
            Console.WriteLine("SuperCore tests passed.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private static void VerifySequentialInitializationAndFrameOrder()
    {
        TestTrace.Reset();
        ModuleRunner runner = new();
        runner.Start(new[] { typeof(ImmediateProbeModule), typeof(DelayedProbeModule) });

        TestTrace.Assert("Immediate.Init", "Delayed.Init");

        runner.Update(0.1, 0.2);
        TestTrace.Assert(
            "Immediate.Init",
            "Delayed.Init",
            "Immediate.Update",
            "Immediate.LateUpdate");

        DelayedProbeModule.CompletePendingInit();
        runner.Update(0.3, 0.4);
        TestTrace.Assert(
            "Immediate.Init",
            "Delayed.Init",
            "Immediate.Update",
            "Immediate.LateUpdate",
            "Immediate.Update",
            "Delayed.Update",
            "Immediate.LateUpdate",
            "Delayed.LateUpdate");

        runner.Shutdown();
        TestTrace.Assert(
            "Immediate.Init",
            "Delayed.Init",
            "Immediate.Update",
            "Immediate.LateUpdate",
            "Immediate.Update",
            "Delayed.Update",
            "Immediate.LateUpdate",
            "Delayed.LateUpdate",
            "Delayed.Clear",
            "Delayed.Destroy",
            "Immediate.Clear",
            "Immediate.Destroy");
    }

    private static void VerifyRepeatedCompleteInitIsRejected()
    {
        ModuleRunner runner = new();
        runner.Start(new[] { typeof(RepeatGuardModule) });

        RepeatGuardModule module = RepeatGuardModule.Get();
        AssertThrows<InvalidOperationException>(module.CompleteAgain);

        runner.Shutdown();
        AssertThrows<InvalidOperationException>(() => RepeatGuardModule.Get());
    }

    private static void VerifyModuleCollectionAndSkeletonLifecycle()
    {
        if (!ModuleCollector.TryCollect(out Type[] moduleTypes, out string error))
            throw new InvalidOperationException($"Expected Module collection to succeed: {error}");

        AssertTypes(
            moduleTypes,
            typeof(ResModule),
            typeof(DebugModule),
            typeof(EventModule),
            typeof(HotUpdateModule),
            typeof(TimerModule),
            typeof(UpdateModule),
            typeof(TableModule),
            typeof(LocalizationModule),
            typeof(AudioModule),
            typeof(UIModule),
            typeof(EntityModule),
            typeof(ProcedureModule));

        ModuleRunner runner = new();
        runner.Start(moduleTypes);
        if (!runner.IsFullyInitialized)
            throw new InvalidOperationException("All Module skeletons should complete initialization synchronously.");
        runner.Shutdown();
    }

    private static void VerifyModuleCollectorValidation()
    {
        ModuleRegistration[] stableOrderRegistrations =
        {
            new(typeof(UpdateModule), -1000),
            new(typeof(ResModule), -20000),
            new(typeof(TimerModule), -1000),
        };
        if (!ModuleCollector.TryOrder(stableOrderRegistrations, out Type[] stableOrder, out string error))
            throw new InvalidOperationException($"Expected Module ordering to succeed: {error}");
        AssertTypes(stableOrder, typeof(ResModule), typeof(TimerModule), typeof(UpdateModule));

        AssertOrderFails(Array.Empty<ModuleRegistration>(), "No Module type");
        AssertOrderFails(new[] { new ModuleRegistration(typeof(DebugModule), 0) }, "first Module");
        AssertOrderFails(
            new[]
            {
                new ModuleRegistration(typeof(ResModule), -1),
                new ModuleRegistration(typeof(string), 0),
            },
            "Invalid Module type");
        AssertOrderFails(
            new[]
            {
                new ModuleRegistration(typeof(ResModule), -1),
                new ModuleRegistration(typeof(NoDefaultConstructorModule), 0),
            },
            "no parameterless constructor");
        AssertOrderFails(
            new[]
            {
                new ModuleRegistration(typeof(ResModule), -1),
                new ModuleRegistration(typeof(ResModule), 0),
            },
            "Duplicate Module type");
    }

    private static void AssertOrderFails(ModuleRegistration[] registrations, string expectedError)
    {
        if (ModuleCollector.TryOrder(registrations, out _, out string error))
            throw new InvalidOperationException("Expected Module ordering to fail.");
        if (!error.Contains(expectedError, StringComparison.Ordinal))
            throw new InvalidOperationException(
                $"Expected Module ordering error containing '{expectedError}', actual '{error}'.");
    }

    private static void AssertTypes(Type[] actual, params Type[] expected)
    {
        if (actual.Length != expected.Length)
            throw new InvalidOperationException(
                $"Type count mismatch. Expected {expected.Length}, actual {actual.Length}.");

        for (int i = 0; i < expected.Length; i++)
        {
            if (actual[i] != expected[i])
                throw new InvalidOperationException(
                    $"Type mismatch at {i}. Expected {expected[i].FullName}, actual {actual[i].FullName}.");
        }
    }

    private static void AssertThrows<TException>(Action action) where TException : Exception
    {
        try
        {
            action.Invoke();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
    }
}

internal static class TestTrace
{
    private static readonly List<string> s_Entries = new();

    internal static void Add(string entry)
    {
        s_Entries.Add(entry);
    }

    internal static void Reset()
    {
        s_Entries.Clear();
    }

    internal static void Assert(params string[] expected)
    {
        if (s_Entries.Count != expected.Length)
            throw new InvalidOperationException(
                $"Trace length mismatch. Expected {expected.Length}, actual {s_Entries.Count}: " +
                string.Join(", ", s_Entries));

        for (int i = 0; i < expected.Length; i++)
        {
            if (!string.Equals(s_Entries[i], expected[i], StringComparison.Ordinal))
                throw new InvalidOperationException(
                    $"Trace mismatch at {i}. Expected {expected[i]}, actual {s_Entries[i]}.");
        }
    }
}

internal sealed class ImmediateProbeModule : IModule
{
    private Action? m_CompleteInit;

    public void Create(Action completeInit)
    {
        m_CompleteInit = completeInit;
    }

    public void Init()
    {
        TestTrace.Add("Immediate.Init");
        m_CompleteInit!.Invoke();
    }

    public void Update(double deltaTime, double unscaledDeltaTime)
    {
        TestTrace.Add("Immediate.Update");
    }

    public void LateUpdate(double deltaTime, double unscaledDeltaTime)
    {
        TestTrace.Add("Immediate.LateUpdate");
    }

    public void Clear()
    {
        TestTrace.Add("Immediate.Clear");
    }

    public void Destroy()
    {
        TestTrace.Add("Immediate.Destroy");
        m_CompleteInit = null;
    }
}

internal sealed class DelayedProbeModule : IModule
{
    private static DelayedProbeModule? s_Instance;

    private Action? m_CompleteInit;

    public void Create(Action completeInit)
    {
        s_Instance = this;
        m_CompleteInit = completeInit;
    }

    public void Init()
    {
        TestTrace.Add("Delayed.Init");
    }

    public void Update(double deltaTime, double unscaledDeltaTime)
    {
        TestTrace.Add("Delayed.Update");
    }

    public void LateUpdate(double deltaTime, double unscaledDeltaTime)
    {
        TestTrace.Add("Delayed.LateUpdate");
    }

    public void Clear()
    {
        TestTrace.Add("Delayed.Clear");
    }

    public void Destroy()
    {
        TestTrace.Add("Delayed.Destroy");
        m_CompleteInit = null;
        s_Instance = null;
    }

    internal static void CompletePendingInit()
    {
        DelayedProbeModule module = s_Instance
                                    ?? throw new InvalidOperationException("DelayedProbeModule is unavailable.");
        module.m_CompleteInit!.Invoke();
    }
}

internal sealed class RepeatGuardModule : Module<RepeatGuardModule>
{
    protected override void OnInit()
    {
        CompleteInit();
    }

    internal void CompleteAgain()
    {
        CompleteInit();
    }
}

internal sealed class NoDefaultConstructorModule : IModule
{
    internal NoDefaultConstructorModule(int value)
    {
    }

    public void Create(Action completeInit)
    {
    }

    public void Init()
    {
    }

    public void Update(double deltaTime, double unscaledDeltaTime)
    {
    }

    public void LateUpdate(double deltaTime, double unscaledDeltaTime)
    {
    }

    public void Clear()
    {
    }

    public void Destroy()
    {
    }
}
