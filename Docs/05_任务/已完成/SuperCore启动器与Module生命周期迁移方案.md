# SuperCore 启动场景与生命周期入口迁移方案

状态：已完成  
任务：`SCG-20260810-001`  
根任务：`SCG-20260806-001`  
目标版本：Godot 4.7.1 .NET

## 一、目标

以 Unity 当前 `GameMain.unity`、`SuperCore.cs`、`IModule.cs`、`Module.cs` 和 `ModuleCfg.cs` 为只读事实源，在 Godot 中建立启动场景、框架 Node Host 与最小 Module 生命周期内核。

框架根目录固定为 `Project/SuperCore`，直属目录为 `RunTime`、`Lib`、`Editor`；`RunTime` 按 Unity 一级结构建立 `Assembly`、`Common`、`Log`、`Module`、`Util`。`SuperCore` 只负责入口和 Godot 生命周期桥接，普通 C# `ModuleRunner` 负责顺序初始化、逐帧调度和逆序清理。

## 二、当前事实

- 实施前 Godot `Project` 只有工程基础文件和 `.godot` 缓存，没有 `Project/SuperCore`、C# 框架源码或主场景。
- Unity `SuperCore.Awake/Start` 负责唯一实例、固定配置校验、Module 创建和链式初始化；`Update` 对已完成初始化的 Module 先执行 `Update`，再同帧执行 `LateUpdate`；销毁时逆序执行 `Clear/Destroy`。
- `Module<T>.CompleteInit()` 是推进下一 Module 的唯一成功信号；重复完成属于框架错误，初始化失败不得伪造完成。
- 当前 Godot 没有任何真实 Module。直接创建生产 `ModuleCfg` 或注册 Autoload 会得到空配置或缺失类型，因此本阶段不能用占位 Module 冒充已迁移能力。
- 当前目录不是可用 Git 工作树，无法通过 Git 区分未提交差异；实施时仍逐文件读取并只修改本方案列出的目标。
- Godot 4.7.1 .NET 可执行文件位于 `D:/Godot/Godot_v4.7.1-stable_mono_win64`，可用于 Headless Runtime 验证。

## 三、Godot 映射

| Unity | Godot 目标 |
| --- | --- |
| `MonoBehaviour` 启动入口 | `Node` 派生的 `SuperCore` |
| `Awake/Start` | `_EnterTree/_Ready` |
| `DontDestroyOnLoad` | `SuperCore` 作为常驻总场景根节点，后续业务内容挂载在其下 |
| `Update + LateUpdate` | `_Process(double delta)` 内两趟顺序调度 |
| `Time.unscaledDeltaTime` | 使用单调时钟计算未缩放帧间隔 |
| `OnDestroy` | `_ExitTree` 逆序清理 |
| `Resources/ModuleCfg` | 本阶段不迁移；等 `ResModule` 任务确定正式 Godot Resource 格式 |
| `Activator.CreateInstance` | `ModuleRunner` 根据已校验的类型创建普通 C# `IModule` |

入口保持严格失败语义：配置缺失、空列表、类型缺失、抽象类型、非 `IModule`、重复类型或初始化协议错误都明确报错并停止，不自动补建、重排或继续后续 Module。

## 四、实施范围

### 4.1 运行时代码

- 创建 `Project/SuperCore/RunTime`、`Lib`、`Editor`，并在 `RunTime` 下建立 Unity 当前一级目录。
- 建立 Godot C# 项目文件，仅包含运行和验证所需的最小程序集设置。
- 新增 `Project/SuperCore/RunTime/Scn_GameMain.tscn`，以 `SuperCore` 为唯一根节点并设置为主场景。
- 新增 `Project/SuperCore/RunTime/SuperCore.cs`：
  - 严格唯一实例；重复节点停止自身处理并释放。
  - 桥接 `_EnterTree / _Ready / _Process / _ExitTree`。
  - 没有生产配置时明确提示未接通，不伪造完整启动成功。
  - 退出树时关闭调度器并清空静态状态。
- 新增 `Project/SuperCore/RunTime/Module/IModule.cs` 与 `Module.cs`：保留创建、初始化、完成回调、双阶段更新、清理、销毁和 `Module<T>.Get()` 单例语义，时间参数使用 Godot 的 `double`。
- 新增 `Project/SuperCore/RunTime/Module/ModuleRunner.cs`：一次性创建全部 Module、按顺序链式初始化、只更新已完成 Module、同帧执行 Update/LateUpdate 并逆序关闭。
- 不新增生产 `ModuleCfg`；正式配置格式留给 `ResModule` 阶段。

### 4.2 验证资产

- 在 `Project/Tests/SuperCore` 新增独立控制台测试工程和专用测试 Module，不让测试源码进入生产程序集。
- 验证同步与延迟完成回调均严格串行推进；未完成 Module 不接收更新。
- 验证同帧 `Update` 全量完成后才进入 `LateUpdate`。
- 验证退出时逆序 `Clear/Destroy`，重复 `CompleteInit()` 直接暴露错误。
- 测试 Module 只存在于测试目录，不写入生产配置，也不作为已迁移 Module 登记。

### 4.3 文档与任务闭环

- 有效确认后创建迁移子任务，填写 `root_task: SCG-20260806-001`；本任务暂无已完成子任务前置依赖，`depends_on` 为空。
- 新增 Godot 当前有效的框架入口/生命周期文档，并只补充本阶段实际形成的编码与目录规则。
- 完成后更新任务索引、2026-08 月索引、当周结构化日志，并将本方案移入 `已完成`。

### 4.4 Editor 目录边界

- `Project/SuperCore/Editor` 本阶段只作为框架 Editor 源码边界。
- Godot 不会因为目录名为 `Editor` 就自动识别插件；是否建立 `res://addons` 入口由后续 Editor 迁移任务独立设计。
- 本阶段不创建 `addons`、`plugin.cfg` 或空 EditorPlugin。

## 五、不做事项

- 不修改 Unity 工程及其 AI 文档。
- 不恢复或参考已删除的旧 Godot 迁移实现与验证结论。
- 不迁移 `SuperCoreCfg` 的资源版本、`ResModule`、YooAsset、HotUpdate 或任何具体 Module。
- 不创建生产空 `ModuleCfg`、占位 Module、兼容层、Module Registry 或程序集全局扫描。
- 不在没有真实生产 Module 时注册 Autoload 或宣称框架已可完整初始化；本阶段只设置常驻根主场景。
- 不实现 SuperCoreKit、Inspector 配置面板、构建、导出或发布。
- 不在本阶段创建 `addons/super_core`、`plugin.cfg` 或空 EditorPlugin。

## 六、风险与控制

- **首阶段无真实 Module**：用独立测试工程证明生命周期，不让测试替身进入生产目录或配置；主场景明确显示未配置状态。
- **异步回调晚于退出**：Module 必须在 `Clear` 中解除自身异步来源；框架销毁后回调不推进已释放数组。
- **Godot 无原生 LateUpdate**：在同一次 `_Process` 内明确执行两趟循环，保留 Unity 当前同帧生产/消费语义。
- **静态单例残留**：正常退出统一复位；重复入口与重复 Module 实例直接报错，不吞掉框架错误。
- **Godot Headless 环境差异**：沙盒内引擎崩溃时必须在正常权限环境复测，并同时记录两次真实结果。

## 七、验证标准

1. `dotnet build` 无错误且不新增警告。
2. 使用 Godot 4.7.1 .NET 启动主场景的 Headless Runtime，进程退出码为 0。
3. 测试记录证明初始化顺序、已完成范围更新、双阶段帧顺序和逆序清理均符合 Unity 当前语义。
4. 启动配置错误会停止初始化链，没有成功回调、自动重排或兜底继续。
5. 生产目录不存在具体 Module 占位实现、空配置和 Autoload。
6. 当前文档、任务索引、方案状态和结构化日志与实际结果一致。
