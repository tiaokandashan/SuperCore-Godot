# 全 Module 启动骨架与配置归位方案

状态：已过期  
根任务：`SCG-20260806-001`  
直接依赖：`SCG-20260810-002`  
目标版本：Godot 4.7.1 .NET

> 已被 `特性驱动Module发现与全骨架方案.md` 替代，不再等待确认，也不得实施。

## 一、目标与策略变更

一次建立 Unity 当前正式启用的 12 个 Module 启动骨架，使 `Cfg_Module.tres` 可以按原优先级完成整条初始化链；每个骨架当前只在 `OnInit()` 中调用 `CompleteInit()`。

这是用户明确要求的启动骨架阶段，覆盖此前“禁止空 Module”的默认边界，但不改变后续“每个真实能力逐个迁移和验收”的原则。所有骨架必须标记为未实现，不能在模块索引中登记为功能迁移完成。

## 二、Module 清单与顺序

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

每个脚本只包含：

```csharp
protected override void OnInit()
{
    CompleteInit();
}
```

不创建 Node、不订阅事件、不加载资源、不实现 Update、Clear 或业务逻辑。

## 三、配置归位

模块自己的配置类型和 `.tres` 资源放在对应 Module 文件夹，不集中放到根 `Configuration`：

| Module 目录 | 配置类型 | 资源 |
| --- | --- | --- |
| `Res` | `ResCfg`、`GlobalCfg` | `Cfg_Res.tres`、`Cfg_Global.tres` |
| `Debug` | `DebugCfg`、`GMCfg` | `Cfg_Debug.tres`、`Cfg_GM.tres` |
| `HotUpdate` | `HotUpdateCfg` | `Cfg_HotUpdate.tres` |
| `Table` | `TableCfg` | `Cfg_Table.tres` |
| `Localization` | `LocalizationCfg` | `Cfg_Localization.tres` |
| `Audio` | `AudioCfg` | `Cfg_Audio.tres` |
| `UI` | `UICfg` | `Cfg_UI.tres` |
| `Procedure` | `ProcedureCfg` | `Cfg_Procedure.tres` |

这些配置本阶段只建立空 `Resource` 骨架，不提前复制尚未审计的 Unity 字段。Event、Timer、Update、Entity 在 Unity 当前没有独立主配置，因此不凭空创建。

根 `RunTime/Configuration` 只保留框架级 `ModuleCfg`、`ModuleInfo` 和 `Cfg_Module.tres`；后续 `SuperCoreCfg` 也属于框架级配置。

## 四、ModuleCfg

- 将 12 个 `ModuleInfo` 作为独立内嵌子资源写入 `Cfg_Module.tres`，优先级、启用状态和类型名与 Unity 当前生产配置一致。
- 恢复“第一个启用 Module 必须是 `ResModule`”的严格校验。
- 相同优先级继续使用 `TypeName` 作为稳定次序，因此 Timer 在 Update 前。
- 主场景启动后 12 个骨架依次完成初始化，`ModuleRunner` 进入运行状态。

## 五、验证

- .NET 主工程零警告编译。
- 独立测试验证 12 个类型全部可解析、顺序准确、首项不是 ResModule 时失败。
- Godot Headless Editor 注册全部 Module 配置 Resource。
- Godot Headless 主场景加载 `Cfg_Module.tres`，12 个 Module 完成初始化且不再报告空配置。
- 退出时 12 个骨架均能逆序 Clear/Destroy，不残留 `Module<T>` 静态实例。

## 六、文档和状态

- 更新项目规则：本任务是唯一明确授权的批量空骨架阶段；后续仍禁止无任务依据的占位实现。
- 模块索引将 12 个类型标记为“启动骨架已建立，功能未迁移”，不能标记为已实现。
- 每个后续 Module 任务只填充一个真实能力及其配置字段，并把对应手册从待补充更新为当前有效。

## 七、不做事项

- 不迁移任何 Module 的真实功能、API、数据结构、Host、事件、计时、资源加载或业务逻辑。
- 不复制尚未审计的 Unity 配置字段。
- 不创建 EditorPlugin、配置 Inspector、类型扫描或自动生成工具。
- 不修改 Unity 参考工程。

## 八、风险

- 主场景将表现为“所有 Module 初始化完成”，但这只代表启动骨架贯通，不代表模块能力可用；文档和测试必须明确区分。
- 空配置 Resource 只是路径和类型骨架，不能被消费者当作真实默认值；对应 Module 功能迁移时必须重新审计并补齐校验。
- 批量骨架让类型名成为后续稳定契约，真实迁移不得随意改名或新增兼容别名。
