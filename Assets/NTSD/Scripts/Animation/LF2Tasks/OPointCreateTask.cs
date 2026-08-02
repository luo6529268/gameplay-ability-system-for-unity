using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Animation.LF2Tasks
{
    public enum ReleaseSpawnSemantic
    {
        None = 0,
        LateOpoint = 1,
        ImmediateEffect = 2,
        TransitionEffect = 3,
        BrokenFragment = 4,
        StageSpawnAt = 5,
    }

    public class OPointCreateTask : LF2TaskBase, ILF2Recyclable
    {
        public override LF2TaskType TaskType => LF2TaskType.CreateObject;

        public ObjectPoint opoint;
        public LF2Entity parent;
        public int team;
        public Vector3 pos;
        public float z;
        public string dir;
        public float dvz;
        public int requiredRuntimeSlot = -1;

        public bool useDirectRuntimePosition;
        public double directX;
        public double directY;
        public double directZ;
        public bool useDirectVelocity;
        public double directVx;
        public double directVy;
        public double directVz;
        public bool preserveActionZero;
        public bool skipPostInitZOffset;
        public bool useInitialRuntimeIntPosition;
        public int initialRuntimeX;
        public int initialRuntimeY;
        public int initialRuntimeZ;
        public int ownerEntityIndex = -1;
        public int spawnerEntityIndex = -1;
        public int trackedTargetSlot = -1;
        public bool useExplicitRelationIdentity;
        public int relationTeam = 0;
        public int holderCopySlot = -1;
        public int frameDelay = 0;
        public int attackExempt = 0;
        public ReleaseSpawnSemantic releaseSpawnSemantic;
        public bool releaseOpointSpawn;
        public bool inheritParentRelation;
        public bool deferPresentationToNextTick;
        public bool suppressLateFrameTickThisTick;
        public bool deferFrameTickToNextTick;

        public bool IsLateOpointSpawn =>
            releaseSpawnSemantic == ReleaseSpawnSemantic.LateOpoint;

        public bool IsImmediateReleaseEffectSpawn =>
            releaseSpawnSemantic == ReleaseSpawnSemantic.ImmediateEffect;

        public bool IsTransitionEffectSpawn =>
            releaseSpawnSemantic == ReleaseSpawnSemantic.TransitionEffect;

        public bool IsBrokenFragmentSpawn =>
            releaseSpawnSemantic == ReleaseSpawnSemantic.BrokenFragment;

        public bool IsStageSpawnAt =>
            releaseSpawnSemantic == ReleaseSpawnSemantic.StageSpawnAt;

        public bool IsFromPool { get; set; }
        public void Clear()
        {
            opoint = default; parent = null; team = 0;
            pos = Vector3.zero; z = 0f; dir = null; dvz = 0f;
            requiredRuntimeSlot = -1;
            useDirectRuntimePosition = false; directX = 0.0; directY = 0.0; directZ = 0.0;
            useDirectVelocity = false; directVx = 0.0; directVy = 0.0; directVz = 0.0;
            preserveActionZero = false;
            skipPostInitZOffset = false;
            useInitialRuntimeIntPosition = false;
            initialRuntimeX = 0;
            initialRuntimeY = 0;
            initialRuntimeZ = 0;
            ownerEntityIndex = -1;
            spawnerEntityIndex = -1;
            trackedTargetSlot = -1;
            useExplicitRelationIdentity = false;
            relationTeam = 0;
            holderCopySlot = -1;
            frameDelay = 0;
            attackExempt = 0;
            releaseSpawnSemantic = ReleaseSpawnSemantic.None;
            releaseOpointSpawn = false;
            inheritParentRelation = false;
            deferPresentationToNextTick = false;
            suppressLateFrameTickThisTick = false;
            deferFrameTickToNextTick = false;
        }
    }
}
