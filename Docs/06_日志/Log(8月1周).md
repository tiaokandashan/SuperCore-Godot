# 2026-08-06

## [SCG-20260806-001] SuperCore 框架完整迁移

```yaml
id: SCG-20260806-001
status: draft
operation: create
created_by: "weijuncheng"
executed_by: ["weijuncheng"]
root_task: null
proposal: "Docs/05_任务/进行中/SuperCore框架完整迁移方案.md"
sources:
  framework: "E:/UnityProject/SuperCore"
  ai_docs: "E:/UnityProject/SuperCore/ProjectDocs"
areas: [framework, godot, migration, docs]
targets:
  files:
    - AGENTS.md
    - Docs
    - Project
  symbols: []
relation_status: independent
relations:
  continues: []
  depends_on: []
  supersedes: []
  reverts: []
  conflicts_with: []
started_at: 2026-08-06
completed_at:
```

目标：
- 以整个 `E:/UnityProject/SuperCore` 为只读框架参考源，以其 `ProjectDocs` 和根 `AGENTS.md` 为 AI 文档参考，在 Godot 4.7.1 .NET 中分阶段完成整个 SuperCore 框架迁移。

结果：
- 已按当前 Unity AI 文档体系重建大写 `Docs` 分层目录，并建立根迁移任务、进行中方案、总索引、月索引和结构化日志。
- 已将根任务来源修正为整个 Unity 工程；`Assets/SuperCore` 是主框架入口，相关脚本、资源包、工具、配置、资源和调用点由子任务按依赖审计。
- 框架、Module、Editor 工具和业务当前文档只保留待补充文件，没有复制 Unity 内容冒充 Godot 实现。
- Godot 框架代码尚未开始，根任务继续保持 `draft`。

决策：
- `SCG-20260806-001` 是全部迁移任务的共同 `root_task`；直接执行依赖继续使用 `relations.depends_on`。
- Unity 历史任务、日志、方案和归档不复制到新项目，只迁移当前有效 AI 文档体系。
- Unity 当前代码、配置和资源优先于 AI 文档；`ProjectDocs` 只用于借鉴定位、职责、历史取舍和验证经验。

约束：
- Unity 整体工程及其 AI 文档只读；根任务不自动授权任何子任务的代码、构建、导出或发布范围。
- 旧失败 Godot 迁移完全失效，不恢复、不兼容。

文档：
- 已建立 `Docs/00_AI快速入口.md`、`01_项目总览`、领域手册占位、任务体系和日志。

后续：
- 创建首个子任务，审计 Unity 整体工程中与 SuperCore 有关的框架源码、公共 API、调用点、配置、资源、工具和 AI 文档。

审查：
- 已确认新文档只描述当前项目事实和迁移规则，没有把尚未实现的 Godot 能力标记为完成。
- 已确认旧 Unity 历史任务与业务日志未进入新项目。

验证：
- 文档部署后检查目录层级、相对链接、UTF-8 无 BOM 和 `Project` 零改动；Godot 构建不属于本次文档任务。
