# Debug Session: history-save-test

Status: [OPEN]

## Hypotheses
1. 保存入口未被触发，或扫描完成路径未调用保存。
2. 保存目标路径与加载来源路径不一致，导致历史记录看似未保存。
3. 保存过程中发生异常或写入结果为空/文件长度异常。
4. 加载过程中发生异常，或读取数量与保存后数量不一致。

## Session
- sessionId: history-save-test
- runId: pre-fix
- Debug Server: http://127.0.0.1:7777/event

## Scope
仅添加调试日志插桩，不修改业务逻辑。
