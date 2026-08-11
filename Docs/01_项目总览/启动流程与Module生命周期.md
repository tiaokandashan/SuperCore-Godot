# 启动流程与 Module 生命周期

状态：当前有效  
最后更新：2026-08-11

## 一、启动入口

`Project/project.godot` 将 `res://SuperCore/RunTime/Scn_GameMain.tscn` 注册为主场景。该场景只有一个名为 `SuperCore` 的 Node 根节点，并挂载 `SuperCore.cs`。

`SuperCore._EnterTree()` 建立严格唯一实例和 `ModuleRunner`。重复入口报告错误、停止处理并释放重复节点。`_Ready()` 调用 `ModuleCollector` 扫描当前已加载程序集，校验并排序所有带 `[Module(priority)]` 的类型，再把类型数组交给 `ModuleRunner`。

当前收集结果是 12 个启动骨架，首项为 `ResModule`，末项为 `ProcedureModule`；骨架会完成整条初始化链，但不代表具体模块能力可用。

## 二、职责边界

- `SuperCore`：唯一 Godot Node Host，桥接 `_EnterTree`、`_Ready`、`_Process` 和 `_ExitTree`。
- `ModuleRunner`：普通 C# 调度器，负责 Module 类型校验、创建、串行初始化、帧调度和逆序关闭。
- `IModule`：框架内部生命周期契约。
- `Module<T>`：普通 C# Module 基类，提供每种 Module 的唯一实例和 `CompleteInit()` 协议保护。
- `ModuleAttribute`：声明 Module 的启动优先级；特性存在即参与收集。
- `ModuleCollector`：运行时发现、类型校验、稳定排序和首项约束。

Module 不继承 `GodotObject` 或 `Node`，不使用 Godot Signal 推进初始化。

## 三、发现规则

- `[Module(priority)]` 只保存优先级，不保存启用开关或类型名。
- 只收集具体、非抽象、实现 `IModule`、无开放泛型参数且存在无参构造函数的类型。
- 收集结果按 `Priority` 升序，再按类型完整名使用序号比较，保证同优先级顺序稳定。
- 结果不能为空、类型不能重复，且排序后首项必须是 `ResModule`；违反约束时启动停止并报告错误。
- 扫描发生在 `SuperCore._Ready()`，不依赖 Godot Signal 或 Resource 注册表。
- 反射依赖程序集已经加载；裁剪或 AOT 导出可能移除仅由反射引用的类型，正式导出阶段必须验证并按需加入保留策略。

## 四、生命周期

```text
Create(all)
  -> Init(first)
  -> CompleteInit
  -> Init(next)
  -> ...
  -> Update(all initialized)
  -> LateUpdate(all initialized)
  -> Clear(reverse all created)
  -> Destroy(reverse all created)
```

- 全部 Module 先创建，再只初始化第一个。
- 当前 Module 调用 `CompleteInit()` 后才开始下一个 Module。
- `CompleteInit()` 在推进下一个 Module 前向标准输出写入当前类型的初始化完成日志，格式为 `[Module->CompleteInit] <ModuleType> initialization completed.`，因此日志顺序与初始化完成顺序一致；纯 C# 生命周期测试不依赖 Godot 原生运行环境。
- 未完成初始化的 Module 不接收 Update 或 LateUpdate。
- 每帧先完成全部已初始化 Module 的 Update，再执行全部 LateUpdate，保持同帧生产/消费语义。
- `_Process` 的 `delta` 作为缩放时间；`Time.GetTicksUsec()` 的单调时钟差作为未缩放时间。
- 关闭时按创建顺序的反序执行 Clear 和 Destroy；单个清理异常不会阻止剩余 Module 释放，最终聚合报告。
- 重复 Module 类型、抽象或非法类型、重复启动和重复 `CompleteInit()` 都直接失败。

## 五、当前骨架边界

- 12 个具体 Module 类型只在 `OnInit()` 中调用 `CompleteInit()`，没有业务 API、数据、算法、Host、事件订阅或更新逻辑。
- 模块自有空配置 Resource 尚未被任何骨架加载；字段和加载方式留给对应真实模块迁移任务。
- `SuperCoreCfg` 的平台和资源版本逻辑尚未迁移。
- Update、Event、Timer、UI 等具体 Module 功能均未迁移。

## 六、验证

独立测试工程位于 `Project/Tests/SuperCore`，主工程排除其 C# 编译项，`Project/Tests/.gdignore` 同时阻止 Godot 资源导入。当前覆盖生命周期调度、12 类型精确收集、优先级和类型名排序、首项约束、非法类型、重复类型，以及全部骨架同步完成初始化和关闭。Godot Headless Editor 另行验证模块自有 Resource 注册，Headless 主场景验证无 `Cfg_Module.tres` 的反射启动路径。
