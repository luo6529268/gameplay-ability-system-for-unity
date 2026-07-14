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
    }

    public enum InitialRuntimeIntPositionHoldMode
    {
        None = 0,
        UntilCurrentTickTu = 1,
        UntilNextTickPresentation = 2,
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

        public bool useDirectVelocity;
        public float directVx;
        public float directVy;
        public float directVz;
        public bool preserveActionZero;
        public bool skipPostInitZOffset;
        public bool useInitialRuntimeIntPosition;
        public int initialRuntimeX;
        public int initialRuntimeY;
        public int initialRuntimeZ;
        public InitialRuntimeIntPositionHoldMode initialRuntimeHoldMode;

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

        public bool IsFromPool { get; set; }
        public void Clear()
        {
            opoint = default; parent = null; team = 0;
            pos = Vector3.zero; z = 0f; dir = null; dvz = 0f;
            useDirectVelocity = false; directVx = 0f; directVy = 0f; directVz = 0f;
            preserveActionZero = false;
            skipPostInitZOffset = false;
            useInitialRuntimeIntPosition = false;
            initialRuntimeX = 0;
            initialRuntimeY = 0;
            initialRuntimeZ = 0;
            initialRuntimeHoldMode = InitialRuntimeIntPositionHoldMode.None;
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
