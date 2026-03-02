# NTSD 帧同步项目问题记录

> **重要**：每次解决问题后，Claude 必须记录到本文件
> 最后更新：2026-03-02（项目初始化）

---

## 📋 问题记录规则

### Claude 必须遵守的规则

**每次遇到问题并解决后，必须**：
1. ✅ 在"已解决问题"章节添加新记录
2. ✅ 包含：时间、现象、原因、解决方案、相关文件
3. ✅ 分配问题编号（#1, #2, #3...）
4. ✅ 更新 `PROGRESS.md` 中的任务备注，引用问题编号
5. ✅ 提醒用户推送到 Git

**记录格式**：
```markdown
### #N 问题标题
- **时间**：YYYY-MM-DD HH:MM
- **阶段**：Phase X
- **现象**：详细描述问题表现
- **原因**：根本原因分析
- **解决方案**：具体解决步骤
- **相关文件**：涉及的文件路径
- **操作人**：电脑 A / 电脑 B
- **参考文档**：相关文档章节
```

---

## ✅ 已解决问题

### 示例记录（供参考，实际使用时删除）

#### #0 示例：Photon Fusion 连接失败
- **时间**：2026-03-02 10:30
- **阶段**：Phase B1
- **现象**：运行 FusionConnectionTest，Console 显示 "❌ 连接失败：InvalidAuthentication"
- **原因**：App ID 从 Dashboard 复制时，末尾多了一个空格
- **解决方案**：
  1. 重新打开 Photon Dashboard
  2. 复制 App ID（确保无空格）
  3. 粘贴到 Unity → Fusion → Realtime Settings
  4. 重新运行测试，连接成功 ✅
- **相关文件**：`Assets/Photon/Fusion/Resources/PhotonAppSettings.asset`
- **操作人**：电脑 A
- **参考文档**：`NTSD_Photon_Fusion_Integration_Guide.md` 第 2 节

---

## ⏳ 待解决问题

### 示例记录（供参考，实际使用时删除）

#### #0 示例：输入延迟手感问题
- **时间**：2026-03-02 14:00
- **阶段**：Phase C1
- **现象**：InputDelayTicks = 6 时，按键到角色动作有明显延迟（约 200ms）
- **尝试方案**：
  - 改成 4：手感好，但网络不稳定时会卡顿
  - 改成 8：不卡顿，但手感太差
- **当前状态**：待测试不同网络环境，寻找最佳平衡点
- **操作人**：电脑 A

---

## 🔍 问题分类统计

| 类别 | 已解决 | 待解决 | 总计 |
|------|--------|--------|------|
| 网络连接 | 0 | 0 | 0 |
| 确定性问题 | 0 | 0 | 0 |
| 输入同步 | 0 | 0 | 0 |
| 性能问题 | 0 | 0 | 0 |
| 配置错误 | 0 | 0 | 0 |
| 其他 | 0 | 0 | 0 |
| **总计** | **0** | **0** | **0** |

---

## 📚 常见问题快速索引

### 网络连接类
- 无

### 确定性问题类
- 无

### 输入同步类
- 无

### 性能问题类
- 无

### 配置错误类
- 无

---

## 💡 经验总结

### 避坑指南（随着问题积累更新）

1. **Photon Fusion 相关**
   - 待补充

2. **确定性相关**
   - 待补充

3. **输入同步相关**
   - 待补充

4. **性能优化相关**
   - 待补充

---

## 🔔 使用示例

### 场景 1：Claude 帮用户解决了问题

**问题**：回放 hash 不一致

**Claude 的操作**：
1. 帮助用户排查，发现 `LF2Character.cs` Line 1520 还有一处 `UnityEngine.Random`
2. 修改为 `context.Rng.NextFloat()`
3. 验证通过
4. **立即更新本文件**：
   ```markdown
   ### #1 回放 hash 不一致
   - **时间**：2026-03-03 09:15
   - **阶段**：Phase A2
   - **现象**：10 次回放，有 3 次 hash 不一致
   - **原因**：LF2Character.cs Line 1520 还有一处 UnityEngine.Random 未替换
   - **解决方案**：
     1. 搜索整个项目：UnityEngine.Random（排除 Test 目录）
     2. 发现 Line 1520：`float chance = UnityEngine.Random.value;`
     3. 替换为：`float chance = Match.Context.Rng.NextFloat();`
     4. 重新回放 10 次，hash 完全一致 ✅
   - **相关文件**：`Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`
   - **操作人**：电脑 A
   - **参考文档**：`NTSD_Lockstep_Code_Templates.md` 常见问题 Q1
   ```
5. **更新 PROGRESS.md**：
   ```markdown
   - [x] Phase A2：录制回放测试（2026-03-03 完成）
     - 备注：遇到问题 #1（hash 不一致），已解决
   ```
6. **提醒用户**：
   ```
   ✅ 问题已解决并记录到 ISSUES.md (#1)
   
   请推送到 Git：
   git add Assets/NTSD/ISSUES.md Assets/NTSD/PROGRESS.md
   git commit -m "解决问题 #1：回放 hash 不一致"
   git push
   ```

---

### 场景 2：电脑 B 遇到类似问题

**用户对 Claude 说**：
```
我在 Phase A2 遇到回放 hash 不一致的问题，帮我排查
```

**Claude 的操作**：
1. 读取 `ISSUES.md`
2. 发现问题 #1 是同样的问题
3. 告诉用户：
   ```
   这个问题之前在电脑 A 遇到过（问题 #1）。
   
   根据记录，原因是 LF2Character.cs Line 1520 还有一处 UnityEngine.Random。
   
   解决方案：
   1. 打开 LF2Character.cs
   2. 跳转到 Line 1520
   3. 将 `UnityEngine.Random.value` 替换为 `Match.Context.Rng.NextFloat()`
   
   参考：ISSUES.md #1
   ```

**避免了重复排查！**

---

## 📌 注意事项

1. **问题编号连续递增**：#1, #2, #3...（不要跳号）
2. **时间格式统一**：YYYY-MM-DD HH:MM
3. **现象描述要详细**：包括错误信息、复现步骤
4. **原因分析要准确**：不要猜测，要有依据
5. **解决方案要具体**：步骤清晰，可复现
6. **相关文件要完整**：包含完整路径
7. **定期整理**：将常见问题提取到"经验总结"

---

## 🔄 版本历史

| 时间 | 操作 | 操作人 | 说明 |
|------|------|--------|------|
| 2026-03-02 | 创建文件 | 初始化 | 项目启动 |

---

**记住：问题记录是避免重复踩坑的关键！每次解决问题后必须记录。**
