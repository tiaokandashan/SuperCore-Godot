# 特性驱动 Module 发现与全骨架方案

状态：已完成  
任务编号：`SCG-20260810-003`  
根任务：`SCG-20260806-001`  
直接依赖：`SCG-20260810-001`  
撤销任务：`SCG-20260810-002`  
目标版本：Godot 4.7.1 .NET

## 一、目标

删除 `ModuleCfg / ModuleInfo / Cfg_Module.tres` 资源注册体系，改成 Module 类型使用特性声明优先级，`SuperCore` 启动时扫描已加载程序集并自动收集。

同时建立 Unity 当前启用的 12 个 Module 启动骨架。每个骨架当前只在 `OnInit()` 中调用 `CompleteInit()`，真实能力仍按后续独立任务逐个迁移。

## 二、删除范围

删除：

```text
Project/SuperCore/RunTime/Configuration/
├─ ModuleCfg.cs
├─ ModuleCfg.cs.uid
├─ ModuleInfo.cs
├─ ModuleInfo.cs.uid
└─ Cfg_Module.tres
```

- `SuperCore` 不再加载 Resource，不保留配置兼容分支或旧路径回退。
- 删除独立测试中针对 `ModuleInfoData / ModuleTypeResolver` 的用例。
- `SCG-20260810-002` 在新任务完成后标记为 `reverted`，保留原方案和日志用于追溯。

## 三、特性与运行时收集

新增：

```text
RunTime/Module/
├─ ModuleAttribute.cs
└─ ModuleCollector.cs
```

Module 声明形式：

```csharp
[Module(-20000)]
public sealed partial class ResModule : Module<ResModule>
{
}
```

规则：

- `ModuleAttribute` 只保存 `Priority`，不再保存 `Enable` 或类型名。
- 类型上存在 `[Module(priority)]` 就参与启动；移除特性即不注册。
- `ModuleCollector` 只在框架启动时扫描一次当前已加载程序集。
- 扫描结果必须是具体、非抽象、实现 `IModule` 且可创建的类型；特性误用直接失败。
- 按 `Priority` 升序，再按类型完整名称序号比较，得到稳定顺序。
- 结果不能为空，首个类型必须是 `ResModule`。
- 收集结束后仍由现有 `ModuleRunner` 负责创建、链式初始化、更新和逆序关闭。

## 四、Module 骨架

| Priority | Module | 目录 |
| ---: | --- | --- |
| -20000 | `ResModule` | `Module/Res` |
| -10000 | `DebugModule` | `Module/Debug` |
| -9000 | `EventModule` | `Module/Event` |
| -8000 | `HotUpdateModule` | `Module/HotUpdate` |
| -1000 | `TimerModule` | `Module/Timer` |
| -1000 | `UpdateModule` | `Module/Update` |
| 500 | `TableModule` | `Module/Table` |
| 800 | `LocalizationModule` | `Module/Localization` |
| 1000 | `AudioModule` | `Module/Audio` |
| 1500 | `UIModule` | `Module/UI` |
| 2000 | `EntityModule` | `Module/Entity` |
| 10000 | `ProcedureModule` | `Module/Procedure` |

每个骨架只实现：

```csharp
protected override void OnInit()
{
    CompleteInit();
}
```

不创建 Node、不加载配置、不订阅事件、不实现 Update、Clear 或业务 API。它们只能登记为“启动骨架存在，功能未迁移”。

## 五、模块自有配置

此前关于模块自有配置归位的决定保留，但它们与 Module 注册完全无关：

| Module 目录 | 空 Resource 骨架 | 空资源文件 |
| --- | --- | --- |
| `Res` | `ResCfg`、`GlobalCfg` | `Cfg_Res.tres`、`Cfg_Global.tres` |
| `Debug` | `DebugCfg`、`GMCfg` | `Cfg_Debug.tres`、`Cfg_GM.tres` |
| `HotUpdate` | `HotUpdateCfg` | `Cfg_HotUpdate.tres` |
| `Table` | `TableCfg` | `Cfg_Table.tres` |
| `Localization` | `LocalizationCfg` | `Cfg_Localization.tres` |
| `Audio` | `AudioCfg` | `Cfg_Audio.tres` |
| `UI` | `UICfg` | `Cfg_UI.tres` |
| `Procedure` | `ProcedureCfg` | `Cfg_Procedure.tres` |

Event、Timer、Update、Entity 在 Unity 当前没有独立主配置，不创建配置。配置 Resource 本阶段不接入空 Module；对应真实模块迁移时重新审计字段和加载逻辑。

## 六、验证

- .NET 主工程零警告编译。
- 独立测试验证收集到且只收集 12 个骨架，顺序与表格一致。
- 测试验证同优先级使用类型全名稳定排序、特性误用失败、首项不是 ResModule 失败。
- 测试启动收集结果，确认 12 个骨架依次 `CompleteInit()` 并可逆序关闭。
- Godot Headless 主场景不再加载 `Cfg_Module.tres`，能够直接收集并启动全部骨架。
- Godot Headless Editor 能注册模块自有的空配置 Resource。

## 七、规则和文档

- 当前任务是用户明确授权的唯一批量空 Module/空配置骨架阶段；后续仍禁止无任务依据的占位实现。
- 模块索引把 12 个类型标记为“骨架”，不标记真实功能完成。
- 当前文档删除 ModuleCfg Resource 作为当前实现的描述；历史任务 `SCG-20260810-002` 保留但标记已撤销。

## 八、不做事项

- 不迁移任何 Module 的真实 API、算法、数据、Host 或配置字段。
- 不保留 `ModuleCfg` 兼容层、资源路径或迁移读取逻辑。
- 不创建 EditorPlugin、Module 开关页面或类型扫描编辑器工具。
- 不修改 Unity 参考工程。

## 九、风险

- 反射扫描依赖程序集已经加载；后续拆分独立程序集时必须保证其在 SuperCore 收集前加载。
- 仅通过反射引用的 Module 在裁剪或 AOT 导出中可能被移除，正式导出任务必须验证并按需要加入保留规则或生成注册表。
- 主场景会表现为全部骨架初始化完成，但这不代表任何具体 Module 功能可用。
