using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.LevelEditor;

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
            (100, "治疗卷轴 heal-scroll"),
            (101, "爆炸标签 ex-tag"),
            (120, "苦无 kunai"),
            (124, "重型武器9 weapon9"),
            (150, "轻型武器1 weapon1"),
            (151, "原木 log"),
        };

        private int _f1Index = 0;
        private readonly List<LF2Entity> _queryBuf = new List<LF2Entity>(32);

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null)
            {
                Debug.LogWarning("[WeaponSpawner] Keyboard.current is null");
                return;
            }

            if (kb.f1Key.wasPressedThisFrame)
                SpawnNextWeapon();

            if (kb[Key.F9].isPressed)
                DropWeaponFromSky();
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
                world.GetAllEntities(_queryBuf);
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
                    if (f.frameId > 0 && f.frameId < minFrame) minFrame = f.frameId;
                    if (groundFrame < 0 && f.frameId > 0 &&
                        (f.state == LF2States.WeaponOnGround || f.state == LF2States.HeavyWeaponOnGround))
                        groundFrame = f.frameId;
                }
            }
            if (groundFrame < 0) groundFrame = minFrame != int.MaxValue ? minFrame : 0;

            // 3. 计算 LF2 坐标（ppu=100，LF2 纵深 z 映射到 Unity Y 轴）
            Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            float lf2X = pos.x * 100f;
            float lf2Z = spawnPoint != null ? pos.y * 100f : spawnZ;

            // 4. 构造任务，入队（FlushTasks 由 SimulationTickDriver 自动调用）
            var task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = new ObjectPoint
            {
                oid    = oid,
                kind   = 0,
                action = groundFrame,
                x      = Mathf.RoundToInt(lf2X),
                y      = 666,
                dvx    = 0,
                dvy    = 0,
                facing = 0,
            };
            task.parent = null; task.team = 0;
            task.pos = new Vector3(lf2X, 0f, 0f);
            task.z = lf2Z; task.dir = "right"; task.dvz = 0f;

            LF2ObjectPointFactory.Instance?.EnqueueCreateObject(task);

            Debug.Log($"[WeaponSpawner] F1 → {wname} (oid={oid}, frame={groundFrame}, lf2X={lf2X:F0}, lf2Z={lf2Z:F0})");
        }

        // 反汇编 Game_FrameUpdate 0x004237B3：
        // dword_449020==1 时，对每个 type 100-199 的武器调用 sub_424630(weapon, 0, x, -500, z)。
        // x/z 在地图边界内随机（边距 30），y=-500（高空落下）。
        private void DropWeaponFromSky()
        {
            // 1. 随机选武器（oid 100-199，排除 122 除非随机通过，对齐反汇编）
            var (oid, _) = _f1Weapons[Random.Range(0, _f1Weapons.Length)];

            if (GameDataManager.Instance == null)
            {
                Debug.LogWarning("[WeaponSpawner] F8: GameDataManager.Instance is null");
                return;
            }
            if (GameDataManager.Instance.GetObjectById(oid) == null)
            {
                Debug.LogWarning($"[WeaponSpawner] F8: oid={oid} not found in GameDataManager");
                return;
            }

            // 2. 在可走区域随机采样落点，保留向内收缩边距
            var boundaryManager = BoundaryWallManager.Instance;
            if (boundaryManager == null || !boundaryManager.TryGetRandomWalkablePoint(out var walkablePoint, insetWorld: 0.9f))
            {
                Debug.LogWarning("[WeaponSpawner] F8: no walkable point found");
                return;
            }

            // 3. 随机位置，y=-500（高空，对齐 sub_424630 arg_C=-500）
            float lf2X = walkablePoint.x * 100f;
            float lf2Z = walkablePoint.y * 100f;
            const float lf2Y = -500f;

            // 4. 找飞行帧（state=1000/1002/2000，fallback 最小非零帧）
            // 注意：action=0 会被 InitializeFrame 替换为 999（音效帧），必须传有效帧号
            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(oid);
            int flyFrame = -1;
            int minFrame = int.MaxValue;
            if (charData?.frames != null)
            {
                foreach (var f in charData.frames)
                {
                    if (f == null) continue;
                    if (f.frameId > 0 && f.frameId < minFrame) minFrame = f.frameId;
                    if (flyFrame < 0 && f.frameId > 0 && (
                        f.state == LF2States.WeaponInSky ||
                        f.state == LF2States.WeaponThrowing ||
                        f.state == LF2States.HeavyWeaponInSky))
                        flyFrame = f.frameId;
                }
            }
            if (flyFrame < 0) flyFrame = minFrame != int.MaxValue ? minFrame : 0;

            // 5. 构造任务，入队
            var task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = new ObjectPoint
            {
                oid    = oid,
                kind   = 0,
                action = flyFrame,
                x      = Mathf.RoundToInt(lf2X),
                y      = Mathf.RoundToInt(lf2Y),
                dvx    = 0,
                dvy    = 0,
                facing = 0,
            };
            task.parent = null; task.team = 0;
            task.pos = new Vector3(lf2X, lf2Y, 0f);
            task.z = lf2Z; task.dir = "right"; task.dvz = 0f;
            task.frameDelay = 99;

            LF2ObjectPointFactory.Instance.EnqueueCreateObject(task);
        }
    }
}
