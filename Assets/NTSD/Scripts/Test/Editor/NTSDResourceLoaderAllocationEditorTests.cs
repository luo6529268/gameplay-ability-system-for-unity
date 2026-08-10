using System;
using NTSD.Load;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSDResourceLoaderAllocationEditorTests
    {
        [SetUp]
        public void SetUp()
        {
            Assert.That(NTSD_ResourceLoader.Instance.IsIdle(), Is.True);
        }

        [Test]
        public void ProcessFrame_WhenQueueIsIdle_AllocatesZeroBytes()
        {
            NTSD_ResourceLoader loader = NTSD_ResourceLoader.Instance;
            Assert.That(loader.IsIdle(), Is.True, "The focused allocation test requires an idle resource loader.");

            loader.ProcessFrame().GetAwaiter().GetResult();
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();

            for (int iteration = 0; iteration < 256; iteration++)
                loader.ProcessFrame().GetAwaiter().GetResult();

            long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocatedBytes, Is.Zero);
        }

        [Test]
        [Timeout(1000)]
        public void ProcessFrame_WhenOnlyTaskIsPaused_ReturnsAndPreservesTheTask()
        {
            NTSD_ResourceLoader loader = NTSD_ResourceLoader.Instance;
            var task = new NTSD_LoadTask
            {
                IsPaused = true,
            };

            loader.AddTask(task);
            try
            {
                loader.ProcessFrame().GetAwaiter().GetResult();
                Assert.That(loader.GetQueuedTaskCount(), Is.EqualTo(1));

                loader.ResumeTask(task);
                loader.ProcessFrame().GetAwaiter().GetResult();
                Assert.That(loader.IsIdle(), Is.True);
            }
            finally
            {
                if (!loader.IsIdle())
                {
                    loader.CancelTask(task);
                    loader.ProcessFrame().GetAwaiter().GetResult();
                }
            }
        }
    }
}
