# SuperCore 框架完整迁移方案

状态：进行中  
任务编号：`SCG-20260806-001`  
创建日期：2026-08-06  
创建人：weijuncheng  
目标版本：Godot 4.7.1 .NET

## 一、任务定位

本任务是 `E:\UnityProject\SuperCoreGodot` 的根迁移任务，也是全部后续框架迁移任务的共同根依赖。它在完整框架最终验收成功前始终保持未完成状态，不因某个阶段或 Module 完成而提前结束。

## 二、迁移源与目标

### 2.1 当前框架源

- 整体框架参考源：`E:\UnityProject\SuperCore`
- 主框架代码入口：`E:\UnityProject\SuperCore\Assets\SuperCore`
- AI 文档参考源：`E:\UnityProject\SuperCore\ProjectDocs`
- 根 AI 入口参考：`E:\UnityProject\SuperCore\AGENTS.md`
- 关联实现范围：`Assets/@Scripts`、`Assets/@ResourcePackage`、`Tools`、配置、资源和其他调用点，由后续子任务按实际依赖只读检查。
- Unity 当前代码、配置、资源和调用关系是迁移语义的最高参考；`ProjectDocs` 用于借鉴定位、职责、历史取舍和验证经验，不能覆盖实际实现。
- 旧方案和已删除的旧 Godot 迁移不能替代对当前 Unity 工程的重新读取。

### 2.2 Godot 目标

- 项目总根：`E:\UnityProject\SuperCoreGodot`
- Godot 项目根：`E:\UnityProject\SuperCoreGodot\Project`
- AI 文档根：`E:\UnityProject\SuperCoreGodot\Docs`
- 引擎版本：Godot 4.7.1 .NET

## 三、迁移目标

- 完成整个当前 Unity 工程中与 SuperCore 相关的源码、配置、资源、工具、调用点和 AI 文档清单审计。
- 保留框架职责、生命周期、错误语义和使用思想，使用 Godot 原生能力重写底层。
- 分阶段迁移 SuperCore、Module、Procedure、GameSystem、资源能力和 SuperCoreKit。
- 为每个阶段建立独立任务、方案、代码、示例、验证和当前文档。
- 最终形成可编译、可运行、可在实际 Godot 编辑器中使用且文档闭合的完整框架。

## 四、根任务依赖规则

根任务自身：

```yaml
id: SCG-20260806-001
root_task: null
```

全部迁移子任务必须填写：

```yaml
root_task: SCG-20260806-001
```

`root_task` 只表示归属，不要求根任务先完成。子任务真实执行前置条件写入：

```yaml
relations:
  depends_on: [SCG-...]
```

后续任务如果没有填写根任务，不得计入本次完整迁移，也不能用于关闭本任务。

## 五、阶段拆分原则

具体阶段和类型清单必须先审计 Unity 当前源码再确定，不从失败迁移中复制。至少需要覆盖以下工作域：

1. Unity 当前工程中与 SuperCore 相关的框架目录、公共 API、配置、资源、Editor、工具、调用点和 AI 文档清单。
2. Godot 项目源码目录、程序集与测试边界。
3. SuperCore 最小入口与生命周期。
4. Module 基础、顺序、初始化、清理和获取规则。
5. 各通用 Module 独立迁移与验收。
6. Procedure、GameSystem、作用域和依赖调度。
7. 资源加载、Package 思想与 Godot 资源包映射。
8. SuperCoreKit 配置体系和 Godot 原生 Editor 工具。
9. 示例、自动检查、构建回归和实际编辑器验收。

每个工作域可以继续拆成多个子任务；不能用一个大任务一次性实现全部能力。

## 六、实施边界

- 不修改 Unity 框架参考源及其 AI 文档。
- 不恢复或兼容已经删除的旧 Godot 迁移。
- 不在源审计前预设所有 Unity 专属能力的保留或删除结论。
- 不为尚未迁移的能力创建空 Module、空配置、空编辑器页面或长期兼容层；`SCG-20260810-003` 是用户明确授权的启动骨架例外，骨架不代表功能迁移完成。
- 代码、资源生成、正式构建、导出和发布仍由各子任务单独确认；本根任务不自动授权。
- 当前任务创建阶段只建立 AI 文档与任务体系，不创建 Godot 框架代码。

## 七、子任务完成标准

每个迁移子任务至少满足：

- 实际读取并记录 Unity 当前源和调用点。
- Godot 职责、API、生命周期和错误边界明确。
- 只实现当前任务范围，没有混入后续阶段。
- C# 编译无新增警告和错误。
- 按风险完成 Headless Runtime、Headless Editor、实际编辑器或资源构建验证。
- 更新对应当前文档、任务索引和结构化日志。
- 未验证内容明确记录，不写成已通过。

## 八、根任务最终完成条件

只有同时满足以下条件，`SCG-20260806-001` 才能从 `draft` 改为 `completed`：

1. Unity 整体参考工程中与 SuperCore 有关的代码、配置、资源、工具、调用点和 AI 文档迁移清单已闭合，每项能力都有迁移、替代、删除或不适用结论。
2. 所有属于本根任务的子任务均已完成，不存在 `draft` 或 `paused` 子任务。
3. SuperCore、全部确认迁移的 Module、Procedure、GameSystem、资源能力和 SuperCoreKit 已形成 Godot 当前实现。
4. 完整项目构建、Headless Runtime、Headless Editor、核心示例和实际编辑器交互验收通过。
5. 目标项目没有旧迁移兼容层、失效配置、重复入口或无消费者占位能力。
6. `Docs/01-04` 当前文档、任务索引、方案和日志与实际实现一致。
7. 未完成事项和明确不迁移项都有可追溯结论。
8. 用户明确确认整体框架迁移成功。

## 九、当前进度

- AI 文档分层体系：已建立。
- 根迁移任务、方案、月索引和结构化日志：已建立。
- Godot 框架源码：启动场景、Module 生命周期、特性发现和 12 个启动骨架已建立。
- 已完成迁移子任务：`SCG-20260810-001`；`SCG-20260810-002` 的实现由 `SCG-20260810-003` 撤销。
- 具体 Module 功能、Procedure、GameSystem、资源能力和 Editor 工具尚未迁移。

下一步按独立任务审计并迁移具体 Module 能力；当前骨架和空配置不得作为能力已完成的依据。
