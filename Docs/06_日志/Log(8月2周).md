# 2026-08-11

## [SCG-20260811-002] 初始化 Git 仓库并上传 GitHub

```yaml
id: SCG-20260811-002
status: draft
operation: create
created_by: "weijuncheng"
executed_by: ["weijuncheng"]
root_task: null
proposal: null
areas: [repository, git, docs]
targets:
  files:
    - .git/config
    - Docs/01_项目总览/任务索引.md
    - Docs/05_任务/索引/2026-08.md
    - Docs/06_日志/Log(8月2周).md
  symbols: []
relation_status: independent
relations:
  continues: []
  depends_on: []
  supersedes: []
  reverts: []
  conflicts_with: []
started_at: 2026-08-11
completed_at:
```

目标：
- 在项目总根目录初始化 Git 仓库，将完整可追踪项目内容上传到用户指定的 GitHub 空仓库。

结果：
- 已初始化本地 `main` 分支并配置 SSH 远端 `git@github.com:tiaokandashan/SuperCore-Godot.git`；提交与推送待执行。

决策：
- 本任务只建立版本库与远端，不改变框架迁移内容，因此独立于 `SCG-20260806-001`，不设置 `root_task` 或迁移依赖。
- 沿用现有 Git 作者 `weijuncheng <@abc123456>`，不修改全局或仓库 Git 配置。

约束：
- 不执行 `git pull`、强制推送、历史重写或 Unity 参考工程写入；远端存在已有引用时停止。
- `Project/.gitignore` 必须排除 `.godot`、`bin` 和 `obj` 生成内容。

文档：
- 已登记总任务索引、月索引和本结构化日志。

后续：
- 待完成远端校验、暂存审查、初始提交和推送。

审查：
- 待完成。

验证：
- 待执行。

## [SCG-20260811-001] Module 初始化完成日志

```yaml
id: SCG-20260811-001
status: completed
operation: update
created_by: "weijuncheng"
executed_by: ["weijuncheng"]
root_task: SCG-20260806-001
proposal: null
areas: [framework, godot, tests, docs]
targets:
  files:
    - Project/SuperCore/RunTime/Module/Module.cs
    - Docs/01_项目总览/启动流程与Module生命周期.md
  symbols:
    - SuperCore.RunTime.Module<T>.CompleteInit
relation_status: linked
relations:
  continues: []
  depends_on: [SCG-20260810-003]
  supersedes: []
  reverts: []
  conflicts_with: []
started_at: 2026-08-11
completed_at: 2026-08-11
```

目标：
- 在每个 Module 成功调用 `CompleteInit()` 时输出包含当前 Module 类型名的初始化完成日志。

结果：
- `Module<T>.CompleteInit()` 在完成状态提交后输出当前泛型 Module 类型名，再同步推进下一个 Module。
- 当前 12 个启动骨架会各输出一条初始化完成日志，日志顺序与实际完成顺序一致。

决策：
- 日志位于完成状态提交后、推进下一 Module 的同步回调前，保证输出顺序与 Module 初始化完成顺序一致。
- 使用 `Console.WriteLine` 输出，日志格式为 `[Module->CompleteInit] <ModuleType> initialization completed.`；直接使用 `GD.Print` 会使 Godot 引擎外的独立 .NET 生命周期测试触发原生访问冲突。

约束：
- 不改变 Module 初始化状态、异常保护或链式调度语义，不修改 Unity 参考工程。

文档：
- 已更新当前生命周期文档，记录日志格式、触发位置、顺序语义和纯 C# 测试边界。
- 已同步月索引和本结构化日志；任务完成后已从总未完成任务索引移除。

后续：
- 无。

审查：
- 已确认日志仅在重复完成保护和回调有效性检查通过后输出，失败调用不会伪造完成日志。
- 已确认日志位于 `m_IsCompleteInit = true` 与 `callback.Invoke()` 之间，不会因同步链式初始化产生逆序输出。
- 已确认未改变 Module 状态、异常、调度和关闭语义，也未修改 Unity 参考工程。

验证：
- `dotnet build Project/SuperCore.csproj --no-restore --nologo`：通过，0 警告、0 错误。
- `dotnet run --project Project/Tests/SuperCore/SuperCore.Tests.csproj --no-restore`：通过，输出 `RepeatGuardModule` 探针日志、12 个骨架日志和 `SuperCore tests passed.`。
- Godot 4.7.1 Headless 主场景：正常桌面环境退出码 0，按 `ResModule` 至 `ProcedureModule` 顺序输出 12 条预期日志。
- 初版 `GD.Print` 在 Godot 引擎外的独立测试中触发原生访问冲突，已改为经两种运行环境验证的 `Console.WriteLine`；受限沙盒内 Godot 仍因无法写入 `user://logs` 触发既有 signal 11，正常环境复测未复现。

# 2026-08-10

## [SCG-20260810-003] 特性驱动 Module 发现与全启动骨架

```yaml
id: SCG-20260810-003
status: completed
operation: update
created_by: "weijuncheng"
executed_by: ["weijuncheng"]
root_task: SCG-20260806-001
proposal: "Docs/05_任务/已完成/特性驱动Module发现与全骨架方案.md"
areas: [framework, configuration, godot, migration, tests, docs]
targets:
  files:
    - Project/SuperCore/RunTime/SuperCore.cs
    - Project/SuperCore/RunTime/Configuration
    - Project/SuperCore/RunTime/Module
    - Project/Tests/SuperCore/Program.cs
    - Docs/00_AI快速入口.md
    - Docs/01_项目总览
    - AGENTS.md
  symbols:
    - SuperCore.RunTime.ModuleAttribute
    - SuperCore.RunTime.ModuleCollector
    - SuperCore.RunTime.ResModule
    - SuperCore.RunTime.DebugModule
    - SuperCore.RunTime.EventModule
    - SuperCore.RunTime.HotUpdateModule
    - SuperCore.RunTime.TimerModule
    - SuperCore.RunTime.UpdateModule
    - SuperCore.RunTime.TableModule
    - SuperCore.RunTime.LocalizationModule
    - SuperCore.RunTime.AudioModule
    - SuperCore.RunTime.UIModule
    - SuperCore.RunTime.EntityModule
    - SuperCore.RunTime.ProcedureModule
relation_status: linked
relations:
  continues: []
  depends_on: [SCG-20260810-001]
  supersedes: []
  reverts: [SCG-20260810-002]
  conflicts_with: []
started_at: 2026-08-10
completed_at: 2026-08-10
```

目标：
- 用 Module 类型特性替换 ModuleCfg 注册资源，并建立 Unity 当前启用的 12 个 Module 启动骨架和模块自有空配置。

结果：
- 已删除 `RunTime/Configuration` 下的 `ModuleCfg`、`ModuleInfo` 和 `Cfg_Module.tres`，生产代码不存在旧注册资源引用或兼容读取。
- 已新增公开 `ModuleAttribute` 和内部 `ModuleCollector`；启动时扫描已加载程序集，严格校验类型，并按 `Priority + Type.FullName` 稳定排序。
- 已建立 `ResModule` 至 `ProcedureModule` 共 12 个启动骨架；每个骨架仅在 `OnInit()` 调用 `CompleteInit()`。
- 已在 8 个所属模块目录建立 10 个空 Resource 类型和对应 10 个 `.tres`，配置未接入骨架，也不参与 Module 注册。
- `SuperCore._Ready()` 已直接收集并启动类型；`ModuleRunner` 继续负责创建、链式初始化、帧调度和逆序关闭。

决策：
- `[Module(priority)]` 的存在即代表参与启动，运行时扫描已加载程序集并稳定排序。
- 本任务明确撤销 `SCG-20260810-002` 的实现，不保留 `ModuleCfg` 兼容层。
- 收集结果不能为空，必须由 `ResModule` 排在首位；同优先级由类型完整名序号比较决定顺序。

约束：
- 12 个 Module 只调用 `CompleteInit()`；不迁移真实 API、算法、Host 或配置字段。

文档：
- 已更新根入口、快速入口、框架总览、生命周期、模块索引、编码、目录和工作流程规则。
- 方案已转入 `Docs/05_任务/已完成`；被替代的旧待确认方案已归档。

后续：
- 按独立任务逐个迁移 12 个 Module 的真实能力，并在对应任务中重新审计配置字段和加载方式。
- 正式裁剪或 AOT 导出时验证反射发现和类型保留，必要时另行设计保留规则或生成注册表。

审查：
- 已确认生产目录恰有 12 个带特性的 Module，且所有骨架只有同步完成初始化逻辑。
- 已确认 `Configuration` 目录已删除，生产源码和 `project.godot` 中旧配置引用为 0。
- 已确认 10 个配置类和 10 个配置资源均位于所属模块目录；Event、Timer、Update、Entity 未凭空新增配置。
- 已确认文档把“启动骨架”与“真实功能迁移”严格区分，未宣称具体 Module 能力可用。
- 已清理独立测试工程的 `bin`、`obj` 生成目录。

验证：
- `dotnet build Project/SuperCore.csproj --no-restore --nologo`：通过，0 警告、0 错误。
- `dotnet run --project Project/Tests/SuperCore/SuperCore.Tests.csproj --no-restore`：通过，输出 `SuperCore tests passed.`。
- 独立测试验证精确收集 12 个骨架、同优先级稳定排序、空收集、非法类型、缺少无参构造、重复类型、首项非 `ResModule` 失败，以及全部骨架同步初始化和关闭。
- Godot 4.7.1 Headless Editor：退出码 0，10 个模块自有 Resource 全部进入全局脚本类缓存，旧 `ModuleCfg / ModuleInfo` 不存在。
- Godot 4.7.1 Headless 主场景：正常桌面环境退出码 0，最新日志无错误、无旧配置加载；受限运行环境首次返回访问冲突码 `-1073741819`，正常环境复测未复现，判定为运行环境差异。

## [SCG-20260810-002] ModuleCfg 配置资源迁移

```yaml
id: SCG-20260810-002
status: reverted
operation: create
created_by: "weijuncheng"
executed_by: ["weijuncheng"]
root_task: SCG-20260806-001
proposal: "Docs/05_任务/已完成/ModuleCfg配置资源迁移方案.md"
areas: [framework, configuration, godot, migration, tests, docs]
targets:
  files:
    - Project/SuperCore/RunTime/Configuration/ModuleInfo.cs
    - Project/SuperCore/RunTime/Configuration/ModuleInfo.cs.uid
    - Project/SuperCore/RunTime/Configuration/ModuleCfg.cs
    - Project/SuperCore/RunTime/Configuration/ModuleCfg.cs.uid
    - Project/SuperCore/RunTime/Configuration/Cfg_Module.tres
    - Project/SuperCore/RunTime/SuperCore.cs
    - Project/Tests/SuperCore/Program.cs
    - Docs/00_AI快速入口.md
    - Docs/01_项目总览/SuperCore框架总览.md
    - Docs/01_项目总览/启动流程与Module生命周期.md
    - Docs/01_项目总览/编码规范.md
    - Docs/01_项目总览/目录职责与修改边界.md
  symbols:
    - SuperCore.RunTime.ModuleInfo
    - SuperCore.RunTime.ModuleCfg
relation_status: linked
relations:
  continues: []
  depends_on: [SCG-20260810-001]
  supersedes: []
  reverts: []
  conflicts_with: []
started_at: 2026-08-10
completed_at: 2026-08-10
```

目标：
- 建立 Godot 可编辑和序列化的 Module 配置 Resource，并接入 SuperCore 固定配置路径。

结果：
- 本任务实现已由 `SCG-20260810-003` 撤销；以下内容仅保留为当时结果记录，不再描述当前实现。
- 已新增 `RunTime/Configuration`，包含 `ModuleInfo.cs`、`ModuleCfg.cs` 和 `Cfg_Module.tres`。
- `ModuleInfo` 与 `ModuleCfg` 已注册为 Godot `[GlobalClass] Resource`；Inspector 可创建并编辑内嵌 `ModuleInfo` 子资源。
- `ModuleCfg` 只序列化一份 `Godot.Collections.Array<ModuleInfo>`，运行时过滤启用项并按 `Priority + TypeName` 稳定排序。
- `SuperCore._Ready()` 已从固定路径加载配置，当前空数组明确报告没有启用 Module 并停止初始化。
- 未迁移任何具体 Module，也没有创建假 `ResModule`、EditorPlugin 或类型下拉工具。

决策：
- `ModuleInfo` 使用自定义 Resource；只序列化一份 `ModuleInfos`，启用过滤和排序在运行时完成。
- 自定义 Resource 类型使用 Godot 注册所需的 `public partial`，配置字段继续采用 `[Export] private m_...` 与只读 getter。
- `System.Type` 不序列化；资源保存完整类型名字符串，运行时先直接解析，再遍历已加载程序集。
- `ResModule` 尚不存在，本任务不伪造首项约束；后续迁移时加入并填充第一个真实条目。

约束：
- 不迁移具体 Module、EditorPlugin 或类型扫描编辑器工具。

文档：
- 已更新快速入口、框架总览、启动生命周期、编码规范和目录职责。
- 方案已转入 `Docs/05_任务/已完成`。

后续：
- 迁移 `ResModule`，将其写入首个启用 `ModuleInfo`，再加入首项强约束并验收生产启动成功路径。

审查：
- 已确认没有序列化第二份启用列表，运行时派生数据不会写回资源。
- 已确认所有配置条目校验基础字符串，只有启用条目参与类型解析、重复检查和启动排序，符合 Unity 当前启用列表语义。
- 已确认 `ModuleInfo` 是 Resource 引用语义，文档明确要求每个数组位置使用独立子资源。
- 已确认测试构建产物已清理，`Project/Tests` 仍由 `.gdignore` 隔离。

验证：
- `dotnet build Project/SuperCore.csproj --no-restore --nologo`：通过，0 警告、0 错误。
- `dotnet run --project Project/Tests/SuperCore/SuperCore.Tests.csproj --no-restore`：通过，输出 `SuperCore tests passed.`。
- 测试覆盖启用过滤、优先级排序、空配置、空类型名、缺失类型、非 Module 类型和重复类型。
- Godot 4.7.1 Headless Editor：退出码 0，`ModuleCfg` 与 `ModuleInfo` 均进入全局 Resource 注册缓存。
- Godot 4.7.1 Headless 主场景：退出码 0，实际加载 `Cfg_Module.tres`、`ModuleCfg.cs` 和 `ModuleInfo.cs`，并输出预期错误 `ModuleCfg has no enabled Module.`。

## [SCG-20260810-001] SuperCore 启动场景与生命周期入口迁移

```yaml
id: SCG-20260810-001
status: completed
operation: create
created_by: "weijuncheng"
executed_by: ["weijuncheng"]
root_task: SCG-20260806-001
proposal: "Docs/05_任务/已完成/SuperCore启动器与Module生命周期迁移方案.md"
areas: [framework, godot, migration, tests, docs]
targets:
  files:
    - Project/project.godot
    - Project/.gitignore
    - Project/SuperCore.csproj
    - Project/SuperCore/RunTime/Assembly/AssemblyInfo.cs
    - Project/SuperCore/RunTime/Module/IModule.cs
    - Project/SuperCore/RunTime/Module/Module.cs
    - Project/SuperCore/RunTime/Module/ModuleRunner.cs
    - Project/SuperCore/RunTime/SuperCore.cs
    - Project/SuperCore/RunTime/Scn_GameMain.tscn
    - Project/Tests/SuperCore/SuperCore.Tests.csproj
    - Project/Tests/SuperCore/Program.cs
    - Project/Tests/.gdignore
    - Docs/00_AI快速入口.md
    - Docs/01_项目总览/SuperCore框架总览.md
    - Docs/01_项目总览/启动流程与Module生命周期.md
    - Docs/01_项目总览/模块索引.md
    - Docs/01_项目总览/编码规范.md
    - Docs/01_项目总览/目录职责与修改边界.md
  symbols:
    - SuperCore.RunTime.SuperCore
    - SuperCore.RunTime.IModule
    - SuperCore.RunTime.Module<T>
    - SuperCore.RunTime.ModuleRunner
relation_status: linked
relations:
  continues: []
  depends_on: []
  supersedes: []
  reverts: []
  conflicts_with: []
started_at: 2026-08-10
completed_at: 2026-08-10
```

目标：
- 建立 Godot SuperCore 启动场景、Node Host 和普通 C# Module 生命周期基础，并用隔离测试验证调度语义。

结果：
- 已创建 `Project/SuperCore/RunTime`、`Lib`、`Editor`，并按 Unity 当前一级结构建立 `Assembly`、`Common`、`Log`、`Module`、`Util`。
- 已建立 Godot 4.7.1 .NET 工程、`Scn_GameMain.tscn` 主场景和唯一 `SuperCore` Node Host。
- 已迁移普通 C# `IModule`、`Module<T>` 和 `ModuleRunner`，实现链式初始化、已完成范围更新、同帧两阶段调度、重复完成保护和逆序关闭。
- 已建立独立控制台测试工程；测试替身不进入生产框架目录或生产程序集。
- 未创建具体 Module、生产空配置、Autoload、EditorPlugin 或 `addons`。

决策：
- 保留 `RunTime / Lib / Editor` 目录命名；具体 Module 和 Editor 工具不进入本任务。
- `SuperCore` 只负责 Godot 生命周期桥接，Module 调度算法放在纯 C# `ModuleRunner` 中。
- `Scn_GameMain.tscn` 作为常驻总场景根入口；后续业务内容不得通过整棵主场景替换销毁框架根节点。
- Unity `ModuleCfg` 强制依赖首个 `ResModule`，所以正式 `SuperCoreCfg / ModuleCfg` 留到 `ResModule` 迁移任务确定。

约束：
- 不创建生产空配置、占位 Module 或未审计的具体 Module。

文档：
- 已更新快速入口、框架总览、启动生命周期、模块索引、编码规范和目录职责。
- 方案已转入 `Docs/05_任务/已完成`。

后续：
- 单独审计并迁移 `ResModule`，随后接通 Godot 版 `SuperCoreCfg / ModuleCfg` 的生产初始化成功路径。
- Update、Event、Timer、UI 和 Editor 体系继续按独立任务迁移。

审查：
- 已确认生产目录没有测试 Module、具体 Module、空配置、EditorPlugin 或 `addons`。
- 已确认 Module 生命周期类型不继承 Node 或 GodotObject；只有 `SuperCore` 是 Godot Host。
- 已确认测试工程通过主工程的 `Compile Remove` 排除，并通过 `Project/Tests/.gdignore` 阻止 Godot 导入，不进入生产程序集或资源导入范围。
- 测试生成的 `Project/Tests/SuperCore/bin` 与 `obj` 已删除，并加入 `.gitignore`。

验证：
- `dotnet build Project/SuperCore.csproj --no-restore --nologo`：通过，0 警告、0 错误。
- `dotnet run --project Project/Tests/SuperCore/SuperCore.Tests.csproj`：通过，输出 `SuperCore lifecycle tests passed.`。
- Godot 4.7.1 .NET Headless 主场景验证：正常环境三次退出码均为 0；加入 `.gdignore` 后测试源码 UID 未重新生成。
- 同一 Godot 命令首次在受限沙盒内触发引擎 signal 11；在获准的正常环境复测后未复现，记录为运行环境差异，不作为项目代码通过依据。
