# ModuleCfg 配置资源迁移方案

状态：已撤销  
撤销任务：`SCG-20260810-003`  
任务：`SCG-20260810-002`  
根任务：`SCG-20260806-001`  
直接依赖：`SCG-20260810-001`  
目标版本：Godot 4.7.1 .NET

## 一、目标

在 `Project/SuperCore/RunTime` 下新增 `Configuration` 目录，建立 Godot 可由 Inspector 编辑和序列化的 `ModuleCfg`、`ModuleInfo` 与 `Cfg_Module.tres`。

本阶段只建立 Module 配置数据模型、资源加载和严格校验，不迁移 `ResModule` 或其他具体 Module，也不创建 EditorPlugin。

## 二、序列化结论

Godot C# 的 `[Export]` 只支持 Variant 兼容类型。Unity 的普通 `[Serializable] struct ModuleInfo` 不能直接作为可编辑对象列表照搬。

目标模型：

```text
ModuleCfg : Resource
└─ Godot.Collections.Array<ModuleInfo>
   ├─ ModuleInfo : Resource
   └─ ModuleInfo : Resource
```

- `ModuleInfo` 使用 `[GlobalClass] public sealed partial class ModuleInfo : Resource`。
- `Priority`、`Enable`、`TypeName` 延续 Unity 封装方式，使用 `[Export] private m_...` 字段和只读 getter；Godot 官方允许导出任意访问级别的字段或属性。
- `ModuleCfg` 使用 `[GlobalClass] public sealed partial class ModuleCfg : Resource`。
- `ModuleInfos` 由 `[Export] private Godot.Collections.Array<ModuleInfo> m_ModuleInfos` 保存，并通过只读 getter 暴露；Inspector 中每项可创建独立的内嵌 `ModuleInfo` 子资源。
- `System.Type` 不是可导出类型，继续序列化完整类型名字符串；后续 Editor 工具再负责扫描和填写类型。
- 只序列化唯一的 `ModuleInfos`。启用项和优先级排序在运行时生成，不再保存第二份 `EnableModuleInfos`，避免双数据源失配。

## 三、目标文件

```text
Project/SuperCore/RunTime/Configuration/
├─ ModuleInfo.cs
├─ ModuleCfg.cs
└─ Cfg_Module.tres
```

类名使用 `ModuleCfg`，资源文件使用配置资源前缀 `Cfg_Module.tres`。

## 四、运行时行为

- `SuperCore` 从固定路径 `res://SuperCore/RunTime/Configuration/Cfg_Module.tres` 加载 `ModuleCfg`。
- `ModuleCfg` 过滤 `Enable == true` 的条目，按 `Priority`、再按 `TypeName` 确定稳定顺序。
- 所有条目拒绝 null、空类型名和首尾空白；启用条目继续拒绝找不到的类型、抽象类型、非 `IModule` 类型和重复类型。
- 当前没有真实 Module，因此 `Cfg_Module.tres` 初始数组为空；启动器明确报告“没有启用 Module”并停止初始化，不记录完整启动成功。
- “首个 Module 必须是 `ResModule`”在 `ResModule` 尚未存在时不伪造类型；该约束在 `ResModule` 迁移任务中加入，并填充首个真实配置项。

## 五、验证

- .NET 主工程零警告编译。
- Godot 4.7.1 能识别 `ModuleCfg` 和 `ModuleInfo` 自定义 Resource。
- `Cfg_Module.tres` 能保存和重新加载 `Godot.Collections.Array<ModuleInfo>`。
- 测试覆盖过滤、稳定排序、非法类型、重复类型和空配置错误。
- Headless 主场景能加载资源，并在当前空配置下给出预期错误而不是异常崩溃。

## 六、不做事项

- 不迁移任何具体 Module。
- 不创建假的 `ResModule` 或测试 Module 生产配置。
- 不序列化第二份启用列表。
- 不使用 `Dictionary`、JSON 或自定义 `_GetPropertyList()` 绕过 Resource 模型。
- 不创建 EditorPlugin、类型下拉框、自动扫描或排序写回工具。
- 不修改 Unity 参考工程。

## 七、风险

- `ModuleInfo` 是 Resource 引用语义，不是 Unity struct 的值语义；每个数组元素必须是独立子资源，不能让多个位置共享同一个实例。
- C# 自定义 Resource 必须先成功编译，才会出现在 Godot Inspector 的创建菜单中。
- 当前 `Cfg_Module.tres` 没有真实条目，只用于建立正式资源契约和验证空配置失败路径；生产成功路径仍依赖后续 `ResModule` 任务。
