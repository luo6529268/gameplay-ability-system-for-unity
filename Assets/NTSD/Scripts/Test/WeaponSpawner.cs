using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;

namespace NTSD.Test
{
    /// <summary>
    /// 按 F1 循环刷新地面武器（测试用）
    /// 对齐 Python _spawn_ground_weapon：销毁所有空闲地面武器，生成新武器。
    /// </summary>
    public class WeaponSpawner : MonoBehaviour
    {
        [Tooltip("武器生成位置参考点：X=水平位置，Y=场景纵深（LF2 z 轴映射到 Unity Y 轴）")]
        public Transform spawnPoint;

        [Tooltip("spawnPoint 为空时使用的 LF2 纵深坐标（默认 260）")]
        public float spawnZ = 260f;

        private static readonly (int oid, string name)[] _f1Weapons =
        {
            (121, "手里剑 shuriken"),
            (100, "铁球 iron-ball"),
            (101, "爆炸标签 ex-tag"),
            (120, "苦无 kunai"),
            (124, "重型武器9 weapon9"),
            (150, "轻型武器1 weapon1"),
            (151, "原木 log"),
        };

        private int _f1Index = 0;
        private readonly List<LF2LivingObject> _queryBuf = new List<LF2LivingObject>(32);

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.f1Key.wasPressedThisFrame)
                SpawnNextWeapon();
        }

        private void SpawnNextWeapon()
        {
            var (oid, wname) = _f1Weapons[_f1Index];
            _f1Index = (_f1Index + 1) % _f1Weapons.Length;

            if (GameDataManager.Instance?.GetObjectById(oid) == null)
            {
                Debug.LogWarning($"[WeaponSpawner] oid={oid} not found in data.txt");
                return;
            }

            // 1. 对齐 Python _spawn_ground_weapon：
            //    清除所有空闲（未被持有）的地面武器，保留持有中的（HoldObj != null）
            var world = SimulationTickDriver.Instance?.World;
            if (world != null)
            {
                world.GetAllLivingObjects(_queryBuf);
                foreach (var obj in _queryBuf)
                {
                    if (obj is LF2WeaponBase w && w.HoldObj == null)
                        w.OnTransitDestroy();
                }
            }

            // 2. 查找落地帧（state=1004 或 2004），fallback 最小帧号
            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(oid);
            int groundFrame = -1;
            int minFrame = int.MaxValue;
            if (charData?.frames != null)
            {
                foreach (var f in charData.frames)
                {
                    if (f == null) continue;
                    if (f.frameId < minFrame) minFrame = f.frameId;
                    if (groundFrame < 0 &&
                        (f.state == LF2States.WeaponOnGround || f.state == LF2States.HeavyWeaponOnGround))
                        groundFrame = f.frameId;
                }
            }
            if (groundFrame < 0) groundFrame = minFrame >= 0 ? minFrame : 0;

            // 3. 计算 LF2 坐标（ppu=100，LF2 纵深 z 映射到 Unity Y 轴）
            Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            float lf2X = pos.x * 100f;
            float lf2Z = spawnPoint != null ? pos.y * 100f : spawnZ;

            // 4. 构造任务，入队（FlushTasks 由 SimulationTickDriver 自动调用）
            var task = new OPointCreateTask
            {
                opoint = new ObjectPoint
                {
                    oid    = oid,
                    kind   = 1,
                    action = groundFrame,
                    x      = Mathf.RoundToInt(lf2X),
                    y      = 666,
                    dvx    = 0,
                    dvy    = 0,
                    facing = 0,
                },
                parent = null,
                team   = 0,
                pos    = new Vector3(lf2X, 0f, 0f),
                z      = lf2Z,
                dir    = "right",
                dvz    = 0f,
            };

            LF2ObjectPointFactory.Instance?.EnqueueCreateObject(task);

            Debug.Log($"[WeaponSpawner] F1 → {wname} (oid={oid}, frame={groundFrame}, lf2X={lf2X:F0}, lf2Z={lf2Z:F0})");
        }
    }
}
