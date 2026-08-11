# SuperCore 框架总览

状态：当前有效  
最后更新：2026-08-10

## 当前实现

- Godot 项目入口是 `Project/SuperCore/RunTime/Scn_GameMain.tscn`。
- 场景唯一根节点为 `SuperCore`，脚本类型是 `SuperCore.RunTime.SuperCore`。
- `SuperCore` 是 Godot `Node` Host，负责把 SceneTree 生命周期桥接到普通 C# Module 调度器。
- `IModule`、`Module<T>` 和 `ModuleRunner` 都不是 Node；Module 不进入场景树。
- Module 类型通过 `[Module(priority)]` 声明参与启动；`ModuleCollector` 在启动时扫描当前已加载程序集、验证类型并稳定排序。
- 当前存在 12 个生产启动骨架，从 `ResModule` 到 `ProcedureModule` 依序同步完成初始化；这些类型的真实功能均未迁移。
- 已删除 `ModuleCfg`、`ModuleInfo` 和 `Cfg_Module.tres`，不保留资源注册兼容路径。
- Res、Debug、HotUpdate、Table、Localization、Audio、UI 和 Procedure 目录已有模块自有空配置 Resource，但尚未接入骨架。
- `SuperCoreCfg` 尚未迁移。

## 当前边界

- 已实现唯一入口、Module 顺序创建与链式初始化、已完成范围更新、同帧 Update/LateUpdate 两阶段调度和逆序清理。
- `RunTime/Assembly`、`Common`、`Log`、`Module`、`Util` 已建立；Module 自有配置跟随所属模块目录。
- `Lib` 和 `Editor` 目前只是目录边界，没有库、EditorPlugin 或 `addons` 内容。
- 后续按独立任务逐个迁移真实 Module 能力；启动骨架和空配置不能作为功能可用的证据。
- 反射发现依赖相关程序集在启动前已加载；裁剪或 AOT 正式导出必须另行验证类型保留。

启动和生命周期细节见 [启动流程与 Module 生命周期](启动流程与Module生命周期.md)。
