using System;
using NTSD.Animation.LF2Tasks;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    internal sealed class LF2CharacterLateRuntimeModule
    {
        private readonly LF2Character owner;

        internal LF2CharacterLateRuntimeModule(LF2Character owner)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        internal void RunLateCharacterCleanup()
        {
            RunLateN30InputTrigger();
        }

        private void RunLateN30InputTrigger()
        {
            int slotIndex = owner.Runtime?.SlotIndex ?? -1;
            if (slotIndex < 0 || slotIndex >= 10)
                return;
            if (owner.Health == null || owner.Health.HP <= 0)
                return;

            int[] history = owner.Runtime?.InputHistory;
            if (history == null || history.Length < 4)
                return;

            int frameVal = 0;
            int a = history[0];
            int b = history[1];
            int c = history[2];
            int d = history[3];
            if (a == 9 && b == 0 && c == 9 && d == 0)
                frameVal = 100;
            else if (a == 9 && b == 9 && c == 9 && d == 9)
                frameVal = 102;
            else if (a == 9 && b == 5 && c == 9 && d == 5)
                frameVal = 104;
            if (frameVal == 0)
                return;

            Array.Clear(history, 0, history.Length);

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null || owner.PS == null)
                return;

            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            if (task == null)
                return;

            task.opoint = new ObjectPoint
            {
                oid = 998,
                kind = 0,
                action = frameVal,
                facing = 0,
            };
            task.parent = null;
            task.team = 0;
            task.useExplicitRelationIdentity = true;
            task.relationTeam = owner.RelationTeam;
            task.holderCopySlot = -1;
            task.pos = new Vector3(
                owner.Runtime?.XInt ?? Mathf.RoundToInt((float)owner.PS.x),
                0f,
                owner.Runtime?.ZInt ?? Mathf.RoundToInt((float)owner.PS.z));
            task.z = owner.Runtime?.ZInt ?? Mathf.RoundToInt((float)owner.PS.z);
            task.dir = "right";
            task.useDirectVelocity = true;
            task.directVx = 0f;
            task.directVy = 0f;
            task.directVz = 0f;
            task.spawnerEntityIndex = slotIndex;
            task.releaseSpawnSemantic = ReleaseSpawnSemantic.ImmediateEffect;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = owner.Runtime?.XInt ?? Mathf.RoundToInt((float)owner.PS.x);
            task.initialRuntimeY = 0;
            task.initialRuntimeZ = owner.Runtime?.ZInt ?? Mathf.RoundToInt((float)owner.PS.z);
            task.deferPresentationToNextTick = false;
            task.suppressLateFrameTickThisTick = false;
            task.deferFrameTickToNextTick = false;
            try
            {
                factory.CreateObjectImmediate(task);
            }
            finally
            {
                LF2ReferencePool.Instance.Recycle(task);
            }
        }
    }
}
