# Godot 项目统一命名规范方案

状态：待确认  
根任务：`SCG-20260806-001`  
目标版本：Godot 4.7.1 .NET

## 一、目标

重写 `Project` 内自有目录、C# 源码、场景、资源、节点和项目标识符的命名规则，形成后续 SuperCore 迁移和游戏开发共同遵守的唯一规范。

总体风格：

- 自有目录、C# 脚本、类型和节点使用 `PascalCase`。
- 资源文件使用“类型前缀 + 下划线 + PascalCase 名称”：`<Type>_<Name>[_<Variant>][_<Index>]`。
- C# 成员继续采用 SuperCore Unity 当前规范，不改成 GDScript 风格。
- Godot 固定名称、工具生成文件和第三方内容保留其规定或上游名称。

`Tex_Bg` 不是传统意义的 `snake_case`，本规范将其称为“资源类型前缀格式”。用户示例中的 `Font_Defualt` 修正为 `Font_Default`。

## 二、适用范围

适用于：

- `Project` 内由本项目维护的目录和文件。
- SuperCore、GameLogic、Tests、Editor 工具和自有资源。
- 新增、迁移、重命名和生成的 C#、场景与资源。

不要求重命名：

- `Docs` 现有中文文档体系和任务文件。
- Godot、.NET、Git 固定文件和生成缓存。
- 第三方插件、SDK、库及其自带资源；它们放入明确的第三方边界并保留上游结构，避免升级和引用失效。

## 三、顶层目录

目标结构使用以下名字：

```text
Project/
├─ addons/
│  └─ SuperCore/
│     ├─ Runtime/
│     ├─ Editor/
│     ├─ Lib/
│     ├─ Assets/
│     └─ Tests/
├─ GameLogic/
├─ Assets/
├─ Tests/
└─ ThirdParty/
```

规则：

- 自有目录使用英文名词和 `PascalCase`，例如 `SuperCore`、`GameLogic`、`Assets`、`Runtime`、`Editor`、`Modules`、`Scenes`、`Textures`。
- 集合目录优先使用复数，例如 `Assets`、`Scenes`、`Textures`、`Fonts`、`Tables`；职责名和不可数名词按自然语义使用，例如 `Runtime`、`Editor`、`Audio`、`Localization`。
- 使用标准单词拼写：采用 `Runtime`，不采用 `RunTime`；采用 `Default`，不采用 `Defualt`。
- 目录名不使用空格、连字符、中文、纯数字或无意义缩写。
- `addons` 是 Godot EditorPlugin 的固定根目录，必须保持小写；`SuperCore` 作为插件子目录继续使用项目的 PascalCase 规范。
- 框架全部位于 `addons/SuperCore`，避免运行时与编辑器代码分散到两棵目录。
- 游戏业务代码放 `GameLogic`；项目资源放顶层 `Assets`；框架自带资源放 `addons/SuperCore/Assets`。
- `ThirdParty` 只作为自有第三方收口目录；必须位于 `addons` 的第三方 EditorPlugin 保留其插件要求和上游目录名。

## 四、C# 文件与类型

### 4.1 文件

- C# 文件使用 `PascalCase`，并与文件内主要类型大小写完全一致，例如 `SuperCore.cs`、`ModuleCfg.cs`、`UIModule.cs`。
- 每个继承 `GodotObject` 的类型独占一个同名文件；`Node`、`Resource`、`EditorPlugin` 和 `[GlobalClass]` 类型不得使用与类名不同的文件名。
- 接口文件保留 `I` 前缀，例如 `IModule.cs`。
- 泛型基类文件使用类型本名，例如 `Module.cs` 对应 `Module<T>`。
- partial 类型的文件以主体名开头并用点分职责，例如 `TableModule.Loader.cs`、`TableModule.Cache.cs`；没有明确收益时不拆分。
- 测试文件使用 `{Target}Tests.cs`，例如 `SuperCoreTests.cs`。
- 生成文件使用 `{Target}.Generated.cs`，不得手工修改；生成器模板必须输出同一命名规则。

### 4.2 命名空间和类型

- 命名空间使用 `PascalCase`，与职责路径保持一致，例如 `SuperCore.Runtime`、`SuperCore.Runtime.Modules`、`GameLogic.Battle`。
- class、struct、record、enum、delegate 使用 `PascalCase`。
- interface 使用 `I` + `PascalCase`。
- Attribute 类型以 `Attribute` 结尾；Exception 类型以 `Exception` 结尾。
- enum 使用单数名，成员使用 `PascalCase`。
- 泛型类型参数使用 `T` 或 `T` + 职责，例如 `TModule`、`TConfig`。
- 项目已有且公认的缩写保持大写，例如 `UI`、`GM`、`AOT`、`LZ4`，对应 `UIModule`、`UICfg`、`GMPage`。

### 4.3 C# 成员

- public、internal 字段使用 `camelCase`；能使用属性表达的公开状态优先使用属性。
- private、protected 字段使用 `m_` + `PascalCase`，例如 `m_Instance`、`m_ModuleCfg`。
- 属性、方法、事件使用 `PascalCase`；方法以动词开头。
- 参数和局部变量使用 `camelCase`。
- 常量和 static readonly 字段使用 `PascalCase`。
- bool 使用 `Is`、`Has`、`Can`、`Should` 等明确前缀。
- 异步方法以 `Async` 结尾；遵守 Try 模式的方法以 `Try` 开头并返回 bool。
- 事件处理方法以 `On` 开头；Godot C# Signal delegate 以 `EventHandler` 结尾。
- 缩写、数字和版本号不破坏单词边界，例如 `LoadLZ4Data`、`Version2`，不使用 `load_lz4_data`。

## 五、资源文件

### 5.1 通用格式

```text
<Type>_<Name>[_<Variant>][_<Index>].<extension>
```

示例：

- `Tex_Bg.png`
- `Tex_Bg_Dark.png`
- `Tex_CharacterIdle_01.png`
- `Txt_GameInfo.txt`
- `Font_Default.ttf`
- `Cfg_SuperCore.tres`
- `Scn_GameMain.tscn`

规则：

- `Type` 使用统一前缀；`Name` 和 `Variant` 使用 `PascalCase`。
- 一个主名称内部不加下划线，例如 `Tex_LoginBackground`，不写 `Tex_Login_Background`。
- Variant 表示确有语义的变体，例如 `Dark`、`Mobile`、`Disabled`；不使用 `New`、`Final`、`Copy`、`Temp`。
- 同类序号使用至少两位数字，从 `01` 开始，例如 `Sfx_Hit_01`。
- 扩展名保持工具标准的小写。
- 文件名只使用 ASCII 英文字母、数字和下划线，不使用空格、连字符、中文、括号或其他符号。
- 技术类型优先于模糊用途；能够判断类型时不使用 `Res_`、`File_`、`Asset_` 等泛化前缀。

### 5.2 前缀词典

| 类型 | 前缀 | 示例 |
| --- | --- | --- |
| Texture / Image | `Tex` | `Tex_Bg` |
| AtlasTexture / 图集 | `Atlas` | `Atlas_UICommon` |
| Font | `Font` | `Font_Default` |
| Theme | `Theme` | `Theme_Default` |
| StyleBox | `Style` | `Style_ButtonPrimary` |
| Material | `Mat` | `Mat_Character` |
| Shader | `Shader` | `Shader_Outline` |
| Mesh | `Mesh` | `Mesh_Ground` |
| Scene / PackedScene | `Scn` | `Scn_GameMain` |
| Animation | `Anim` | `Anim_PlayerIdle` |
| AnimationLibrary | `AnimLib` | `AnimLib_Player` |
| AudioStream（音乐） | `Bgm` | `Bgm_Main` |
| AudioStream（音效） | `Sfx` | `Sfx_ButtonClick` |
| AudioStream（语音） | `Voice` | `Voice_Guide_01` |
| VideoStream | `Video` | `Video_Opening` |
| Config Resource | `Cfg` | `Cfg_Module` |
| 通用数据 Resource | `Data` | `Data_ItemCatalog` |
| 配置表 | `Table` | `Table_Item` |
| 本地化资源 | `Loc` | `Loc_ZhCN` |
| 纯文本 | `Txt` | `Txt_GameInfo` |
| JSON | `Json` | `Json_ServerList` |
| XML | `Xml` | `Xml_Localization` |
| CSV | `Csv` | `Csv_Item` |
| Icon 专用纹理 | `Icon` | `Icon_Settings` |
| Cursor 专用纹理 | `Cursor` | `Cursor_Default` |

新增前缀前必须确认现有词典无法准确表达，并同步更新本规范；同一技术类型不得并存多个同义前缀。

## 六、场景与节点

- 场景文件使用 `Scn_<Name>.tscn`，例如 `Scn_GameMain.tscn`、`Scn_Login.tscn`。
- 场景根节点和场景主体名称使用同一 PascalCase 语义，例如文件 `Scn_Login.tscn` 的根节点为 `Login`。
- 节点名称使用 `PascalCase`，优先采用“语义 + 类型”，例如 `StartButton`、`TitleLabel`、`PlayerCamera`、`AudioPlayer`。
- 不使用自动默认名长期提交，例如 `Node2D2`、`Control3`、`Button4`。
- Scene Unique Name 仍使用 PascalCase，例如 `%StartButton`。
- 只有真实集合节点使用复数，例如 `Enemies`、`SpawnPoints`。

## 七、Godot 项目标识符

- Autoload 名称使用 `PascalCase`，例如 `SuperCore`。
- 自定义 Input Action 使用 `PascalCase`，并按域表达，例如 `PlayerMove`、`PlayerAttack`、`UIConfirm`；Godot 内置 `ui_accept` 等固定 action 不重命名。
- 自定义 Group 使用 `PascalCase`；需要域时使用点分层，例如 `Gameplay.Player`、`Gameplay.Enemy`。
- C# Signal 使用 PascalCase 语义，例如 `StateChanged`；引擎内置信号和方法的 `StringName` 使用 Godot 提供的生成常量，不手写改名。
- ProjectSettings 的引擎内置 key 保持 Godot 原名；自有配置 key 若必须使用字符串路径，采用 `SuperCore/Category/Name` 的 PascalCase 分段。
- 资源 Address、存档 key、网络协议字段和配置表列名必须由各系统单独确定稳定契约；不能仅因文件重命名自动改变持久化或协议标识符。

## 八、测试、编辑器和生成内容

- 测试目录与命名使用 `Tests`、`{Target}Tests.cs`、`Scn_{Target}Test.tscn`。
- EditorPlugin 入口位于 `Project/addons/<PluginName>`；自有插件子目录使用 PascalCase，例如 `addons/SuperCore`。
- EditorPlugin C# 类型使用 `{Name}Plugin.cs`，例如 `SuperCorePlugin.cs`，并使用 `[Tool]` 与 `#if TOOLS`。
- 自有构建器、收集器和生成器分别以 `Builder`、`Collector`、`Generator` 结尾。
- 自动生成产物不得人工重命名；需要变更名称时修改生成器、模板和全部消费者后重新生成。

## 九、固定名称和例外

以下内容不套用 PascalCase 或资源前缀规则：

- Godot：`addons`、`.godot`、`project.godot`、`plugin.cfg`、`.godot/` 生成内容、`.import`、`.uid`。
- .NET / Git / EditorConfig：`.csproj` 与 `.sln` 的扩展名、`.editorconfig`、`.gitignore`、`.gitattributes`、`global.json`、`Directory.Build.props`、`packages.lock.json`。
- 第三方和工具生成内容：保留上游名称；不为满足本规范破坏升级路径、序列化引用、许可证或构建脚本。
- Godot 内置 Input Action、信号、方法、ProjectSettings key 和引擎资源路径保持官方名称。

重命名自有 Godot 资源时必须通过 Godot Editor 或经过 UID/引用验证的工具完成，不手工修改 `.godot`、`.import` 或 `.uid` 来伪造迁移。

## 十、实施范围

有效确认后：

1. 创建命名规范子任务，填写 `root_task: SCG-20260806-001`，当前无直接完成任务依赖。
2. 将 `Docs/01_项目总览/编码规范.md` 从待补充重写为当前有效，并纳入本方案全部规则。
3. 更新 `Docs/01_项目总览/目录职责与修改边界.md`，登记 `addons/SuperCore`、`GameLogic`、`Assets`、`Tests` 和 `ThirdParty` 的目标职责及固定名称例外。
4. 更新待确认的 SuperCore 启动器迁移方案，使目标路径采用 `Project/addons/SuperCore/Runtime`，并记录它依赖本命名规范任务完成。
5. 更新任务索引、月份索引和当周结构化日志，将本方案移入 `已完成`。

## 十一、不做事项

- 本任务不创建、移动、重命名或删除 `Project` 中的实际目录、脚本、场景和资源。
- 不清理 `.godot` 缓存，不修改 Unity 只读参考源。
- 不迁移 SuperCore 代码，不创建空 EditorPlugin、Module、配置或资源。
- 不批量重命名第三方、生成文件或现有 AI 文档。
- 具体资产迁移时再按本规范执行真实重命名和引用验证。

## 十二、风险与控制

- **与 Godot 固定名称冲突**：通过第九节白名单保留 `addons`、`plugin.cfg` 等强制名称。
- **C# 文件与类型不一致**：所有可挂载或注册的 Godot C# 类型强制文件名与类型名大小写一致。
- **资源前缀膨胀**：维护唯一前缀词典，禁止同义前缀和临时缩写。
- **重命名破坏引用**：本任务只定规范；后续真实重命名必须逐项验证 UID、场景、Resource、代码路径和配置引用。
- **第三方升级困难**：第三方内容保留上游布局，不把自有规范强加给外部包。
- **既有启动方案冲突**：本任务完成时同步修订待确认方案，启动器实施必须等待命名规范任务完成。

## 十三、验证标准

1. 规范覆盖目录、C#、资源、场景、节点、Godot 标识符、测试、Editor 和生成文件。
2. 示例没有拼写错误，`Runtime`、`Default` 等标准词统一。
3. `addons`、C# 文件同名约束和引擎生成文件例外准确。
4. 资源前缀不存在同义冲突，格式和变体规则可以机械检查。
5. `编码规范.md` 与 `目录职责与修改边界.md` 职责不重复冲突。
6. SuperCore 启动器待确认方案的路径和依赖与新规范一致。
7. 本任务没有修改任何 `Project` 实际内容或生成缓存。
