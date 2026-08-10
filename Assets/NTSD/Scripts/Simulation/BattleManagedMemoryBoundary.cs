using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace NTSD.Simulation
{
    /// <summary>
    /// Owns the managed-memory boundary between loading/prewarm and formal battle ticks.
    /// Loading garbage is collected before the window opens. The formal battle window
    /// disables managed collection and separately records any remaining main-thread
    /// allocations so collection prevention cannot hide a growing managed heap.
    /// </summary>
    public sealed class BattleManagedMemoryBoundary
    {
        private bool battleWindowOpen;
        private int baselineGeneration0Collections;
        private int baselineGeneration1Collections;
        private int baselineGeneration2Collections;
        private int generation0Collections;
        private int generation1Collections;
        private int generation2Collections;
        private int firstCollectionTick;
        private GarbageCollector.Mode previousGcMode;
        private bool ownsGcModeOverride;
        private bool tickObservationOpen;
        private long tickAllocationBaseline;
        private long allocatedBytes;
        private int allocationViolationCount;
        private int firstAllocationTick;
        private bool driverUpdateObservationOpen;
        private long driverUpdateAllocationBaseline;
        private long tickAllocatedBytesWithinDriverUpdate;
        private long driverUpdateAllocatedBytes;
        private int driverUpdateAllocationViolationCount;
        private int firstDriverUpdateAllocationTick;
        private bool presentationObservationOpen;
        private long presentationAllocationBaseline;
        private long presentationAllocatedBytes;
        private int presentationAllocationViolationCount;
        private int firstPresentationAllocationTick;
        private bool playerLoopObservationOpen;
        private long playerLoopAllocationBaseline;
        private long playerLoopAllocatedBytes;
        private int playerLoopAllocationViolationCount;
        private int firstPlayerLoopAllocationTick;
        private readonly Func<long> allocationCounter;

        public BattleManagedMemoryBoundary()
        {
        }

        public BattleManagedMemoryBoundary(Func<long> allocationCounter)
        {
            this.allocationCounter = allocationCounter;
        }

        public bool BattleWindowOpen => battleWindowOpen;
        public bool HasCollectionViolation =>
            generation0Collections != 0 ||
            generation1Collections != 0 ||
            generation2Collections != 0;
        public int Generation0Collections => generation0Collections;
        public int Generation1Collections => generation1Collections;
        public int Generation2Collections => generation2Collections;
        public int FirstCollectionTick => firstCollectionTick;
        public bool ManagedCollectionControlSupported
        {
            get
            {
#if UNITY_EDITOR
                return false;
#else
                return true;
#endif
            }
        }
        public bool ManagedCollectionDisabled =>
            ManagedCollectionControlSupported &&
            battleWindowOpen &&
            GarbageCollector.GCMode == GarbageCollector.Mode.Disabled;

        public bool HasAllocationViolation => allocationViolationCount != 0;
        public long AllocatedBytes => allocatedBytes;
        public int AllocationViolationCount => allocationViolationCount;
        public int FirstAllocationTick => firstAllocationTick;
        public bool HasDriverUpdateAllocationViolation =>
            driverUpdateAllocationViolationCount != 0;
        public long DriverUpdateAllocatedBytes => driverUpdateAllocatedBytes;
        public int DriverUpdateAllocationViolationCount =>
            driverUpdateAllocationViolationCount;
        public int FirstDriverUpdateAllocationTick =>
            firstDriverUpdateAllocationTick;
        public bool HasPresentationAllocationViolation =>
            presentationAllocationViolationCount != 0;
        public long PresentationAllocatedBytes => presentationAllocatedBytes;
        public int PresentationAllocationViolationCount =>
            presentationAllocationViolationCount;
        public int FirstPresentationAllocationTick =>
            firstPresentationAllocationTick;
        public bool PlayerLoopEnvelopeHardGateSupported
        {
            get
            {
#if UNITY_EDITOR
                return false;
#else
                return true;
#endif
            }
        }
        public bool HasPlayerLoopAllocationViolation =>
            playerLoopAllocationViolationCount != 0;
        public long PlayerLoopAllocatedBytes => playerLoopAllocatedBytes;
        public int PlayerLoopAllocationViolationCount =>
            playerLoopAllocationViolationCount;
        public int FirstPlayerLoopAllocationTick =>
            firstPlayerLoopAllocationTick;

        /// <summary>
        /// Must be called after resources, pools, roster and presentation capacity are
        /// ready, and before the first formal battle tick is allowed to run.
        /// </summary>
        public void CompleteLoadingAndOpenBattleWindow()
        {
            if (battleWindowOpen)
                return;

            previousGcMode = GarbageCollector.GCMode;
            ownsGcModeOverride = false;

#if UNITY_EDITOR
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
#else
            ownsGcModeOverride = previousGcMode != GarbageCollector.Mode.Disabled;

            if (ownsGcModeOverride)
            {
                try
                {
                    if (GarbageCollector.GCMode != GarbageCollector.Mode.Enabled)
                        GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
                }
                catch
                {
                    GarbageCollector.GCMode = previousGcMode;
                    ownsGcModeOverride = false;
                    throw;
                }
            }
#endif

            baselineGeneration0Collections = GC.CollectionCount(0);
            baselineGeneration1Collections = GC.CollectionCount(1);
            baselineGeneration2Collections = GC.CollectionCount(2);
            generation0Collections = 0;
            generation1Collections = 0;
            generation2Collections = 0;
            firstCollectionTick = 0;
            tickObservationOpen = false;
            tickAllocationBaseline = 0;
            allocatedBytes = 0;
            allocationViolationCount = 0;
            firstAllocationTick = 0;
            driverUpdateObservationOpen = false;
            driverUpdateAllocationBaseline = 0;
            tickAllocatedBytesWithinDriverUpdate = 0;
            driverUpdateAllocatedBytes = 0;
            driverUpdateAllocationViolationCount = 0;
            firstDriverUpdateAllocationTick = 0;
            presentationObservationOpen = false;
            presentationAllocationBaseline = 0;
            presentationAllocatedBytes = 0;
            presentationAllocationViolationCount = 0;
            firstPresentationAllocationTick = 0;
            playerLoopObservationOpen = false;
            playerLoopAllocationBaseline = 0;
            playerLoopAllocatedBytes = 0;
            playerLoopAllocationViolationCount = 0;
            firstPlayerLoopAllocationTick = 0;
            battleWindowOpen = true;
        }

        public void BeginPlayerLoopFrame(int tickIndex)
        {
            if (!battleWindowOpen)
                return;

            if (playerLoopObservationOpen)
                ObserveAfterPlayerLoopFrame(tickIndex);

            playerLoopAllocationBaseline = ReadAllocatedBytes();
            playerLoopObservationOpen = true;
        }

        public void ObserveAfterPlayerLoopFrame(int tickIndex)
        {
            if (!battleWindowOpen || !playerLoopObservationOpen)
                return;

            long allocated = Math.Max(
                0L,
                ReadAllocatedBytes() - playerLoopAllocationBaseline);
            playerLoopObservationOpen = false;
            if (allocated > 0)
            {
                playerLoopAllocatedBytes += allocated;
                playerLoopAllocationViolationCount++;
                if (firstPlayerLoopAllocationTick == 0)
                    firstPlayerLoopAllocationTick = tickIndex;
            }

            ObserveCollections(tickIndex);
        }

        public void BeginDriverUpdate()
        {
            if (!battleWindowOpen || driverUpdateObservationOpen)
                return;

            driverUpdateAllocationBaseline = ReadAllocatedBytes();
            tickAllocatedBytesWithinDriverUpdate = 0;
            driverUpdateObservationOpen = true;
        }

        public void ObserveAfterDriverUpdate(int tickIndex)
        {
            if (!battleWindowOpen)
                return;

            if (driverUpdateObservationOpen)
            {
                long totalUpdateAllocatedBytes = Math.Max(
                    0L,
                    ReadAllocatedBytes() - driverUpdateAllocationBaseline);
                long nonTickAllocatedBytes = Math.Max(
                    0L,
                    totalUpdateAllocatedBytes - tickAllocatedBytesWithinDriverUpdate);
                driverUpdateObservationOpen = false;
                tickAllocatedBytesWithinDriverUpdate = 0;

                if (nonTickAllocatedBytes > 0)
                {
                    driverUpdateAllocatedBytes += nonTickAllocatedBytes;
                    driverUpdateAllocationViolationCount++;
                    if (firstDriverUpdateAllocationTick == 0)
                        firstDriverUpdateAllocationTick = tickIndex;
                }
            }

            ObserveCollections(tickIndex);
        }

        public void BeginPresentation()
        {
            if (!battleWindowOpen || presentationObservationOpen)
                return;

            presentationAllocationBaseline = ReadAllocatedBytes();
            presentationObservationOpen = true;
        }

        public void ObserveAfterPresentation(int tickIndex)
        {
            if (!battleWindowOpen)
                return;

            if (presentationObservationOpen)
            {
                long allocated = Math.Max(
                    0L,
                    ReadAllocatedBytes() - presentationAllocationBaseline);
                presentationObservationOpen = false;
                if (allocated > 0)
                {
                    presentationAllocatedBytes += allocated;
                    presentationAllocationViolationCount++;
                    if (firstPresentationAllocationTick == 0)
                        firstPresentationAllocationTick = tickIndex;
                }
            }

            ObserveCollections(tickIndex);
        }

        public void BeginTick()
        {
            if (!battleWindowOpen || tickObservationOpen)
                return;

            tickAllocationBaseline = ReadAllocatedBytes();
            tickObservationOpen = true;
        }

        public void ObserveAfterTick(int tickIndex)
        {
            if (!battleWindowOpen)
                return;

            if (tickObservationOpen)
            {
                long currentAllocatedBytes = ReadAllocatedBytes();
                long tickAllocatedBytes = Math.Max(
                    0L,
                    currentAllocatedBytes - tickAllocationBaseline);
                tickObservationOpen = false;

                if (tickAllocatedBytes > 0)
                {
                    allocatedBytes += tickAllocatedBytes;
                    if (driverUpdateObservationOpen)
                        tickAllocatedBytesWithinDriverUpdate += tickAllocatedBytes;
                    allocationViolationCount++;
                    if (firstAllocationTick == 0)
                        firstAllocationTick = tickIndex;
                }
            }

            ObserveCollections(tickIndex);
        }

        private void ObserveCollections(int tickIndex)
        {
            generation0Collections = Math.Max(
                0,
                GC.CollectionCount(0) - baselineGeneration0Collections);
            generation1Collections = Math.Max(
                0,
                GC.CollectionCount(1) - baselineGeneration1Collections);
            generation2Collections = Math.Max(
                0,
                GC.CollectionCount(2) - baselineGeneration2Collections);

            if (firstCollectionTick == 0 && HasCollectionViolation)
                firstCollectionTick = tickIndex;
        }

        public void CloseBattleWindow()
        {
            if (!battleWindowOpen)
                return;

            battleWindowOpen = false;
            tickObservationOpen = false;
            driverUpdateObservationOpen = false;
            presentationObservationOpen = false;
            playerLoopObservationOpen = false;

            if (ownsGcModeOverride)
            {
                GarbageCollector.GCMode = previousGcMode;
                ownsGcModeOverride = false;
            }

        }

        private long ReadAllocatedBytes()
        {
            return allocationCounter != null
                ? allocationCounter()
                : GC.GetAllocatedBytesForCurrentThread();
        }
    }

    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-32000)]
    internal sealed class BattleManagedMemoryFrameBeginProbe : MonoBehaviour
    {
        private SimulationTickDriver driver;
        private BattleManagedMemoryBoundary boundary;

        internal void Bind(
            SimulationTickDriver owner,
            BattleManagedMemoryBoundary memoryBoundary)
        {
            driver = owner;
            boundary = memoryBoundary;
        }

        private void Update()
        {
            boundary?.BeginPlayerLoopFrame(driver != null ? driver.CurrentTickIndex : 0);
        }
    }

    [DisallowMultipleComponent]
    internal sealed class BattleManagedMemoryFrameEndProbe : MonoBehaviour
    {
        private SimulationTickDriver driver;
        private BattleManagedMemoryBoundary boundary;

        internal void Bind(
            SimulationTickDriver owner,
            BattleManagedMemoryBoundary memoryBoundary)
        {
            driver = owner;
            boundary = memoryBoundary;
        }

        private void OnEnable()
        {
            RenderPipelineManager.endFrameRendering += OnEndFrameRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.endFrameRendering -= OnEndFrameRendering;
        }

        private void OnEndFrameRendering(
            ScriptableRenderContext context,
            Camera[] cameras)
        {
            boundary?.ObserveAfterPlayerLoopFrame(
                driver != null ? driver.CurrentTickIndex : 0);
        }
    }
}
