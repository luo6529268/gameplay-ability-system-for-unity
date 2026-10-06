#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NTSD.App;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28OriginalCommonAudioEditorTests
    {
        [TestCase(@"data\053.wav")]
        [TestCase(@"c\nar\w\a7.wav")]
        [TestCase(@"c\saku\w\tra.wav")]
        public void SharedBattleCueRetriggersOneVoiceAcrossRoles(string cue)
        {
            using var fixture = new Fixture();
            fixture.Prepare(cue);
            fixture.Player.PresentSound(new PendingSoundEvent(cue, 500, 1));
            AudioSource first = fixture.Assigned().Single();
            first.timeSamples = 100;
            fixture.Player.PresentSound(new PendingSoundEvent(cue, 700, 2));
            Assert.That(fixture.Assigned(), Has.Length.EqualTo(1));
            Assert.That(fixture.Assigned().Single(), Is.SameAs(first));
            Assert.That(first.timeSamples, Is.EqualTo(0));
            Assert.That(first.loop, Is.False);
            Assert.That(fixture.Player.OneShotVoiceLimitDropCountForDiagnostics, Is.Zero);
        }

        [Test]
        public void SameTickSharedCueAggregatesOnceWithoutMergingNextTick()
        {
            using var fixture = new Fixture();
            const string cue = @"data\053.wav";
            fixture.Prepare(cue);
            fixture.Player.PresentSounds(new[]
            {
                new PendingSoundEvent(cue, 500, 1),
                new PendingSoundEvent(cue, 700, 1)
            });
            Assert.That(fixture.Player.PooledOneShotPlayCountForDiagnostics, Is.EqualTo(1));
            Assert.That(fixture.Assigned(), Has.Length.EqualTo(1));
            Assert.That(fixture.Assigned()[0].panStereo, Is.LessThan(0));
            fixture.Player.PresentSounds(new[]
            {
                new PendingSoundEvent(cue, 500, 2),
                new PendingSoundEvent(cue, 700, 3)
            });
            Assert.That(fixture.Player.PooledOneShotPlayCountForDiagnostics, Is.EqualTo(3));
            Assert.That(fixture.Assigned(), Has.Length.EqualTo(1));
        }

        [Test]
        public void DifferentDynamicCuesAndBuiltinIdentityRemainIndependent()
        {
            using var fixture = new Fixture();
            foreach (string cue in new[] { @"data\053.wav", @"data\012.wav", "SFX_001", @"data\001.wav" })
            {
                fixture.Prepare(cue);
                fixture.Player.PresentSound(new PendingSoundEvent(cue, 500, 1));
            }
            Assert.That(fixture.Assigned(), Has.Length.EqualTo(4));
            fixture.Player.PresentSound(new PendingSoundEvent(@"data\053.wav", 700, 2));
            Assert.That(fixture.Assigned(), Has.Length.EqualTo(4));
            Assert.That(fixture.Player.OneShotVoiceLimitDropCountForDiagnostics, Is.Zero);
        }

        [Test]
        public void VoiceReplacementDoesNotRetainStaleBattleIdentity()
        {
            using var fixture = new Fixture();
            const string firstCue = @"data\053.wav";
            fixture.Prepare(firstCue);
            fixture.Player.PresentSound(new PendingSoundEvent(firstCue, 500, 1));
            for (int index = 0; index < 64; index++)
            {
                string cue = "diagnostic-" + index + ".wav";
                fixture.Prepare(cue);
                fixture.Player.PresentSound(new PendingSoundEvent(cue, 500, 2));
            }
            Assert.That(fixture.Assigned(), Has.Length.EqualTo(64));
            fixture.Player.PresentSound(new PendingSoundEvent(firstCue, 700, 3));
            Assert.That(fixture.Assigned(), Has.Length.EqualTo(64));
            Assert.That(fixture.Assigned().Count(value => value.clip.name == firstCue), Is.EqualTo(1));
        }

        [Test]
        public void UnityVoiceCapsMonoGainAboveOne()
        {
            using var fixture = new Fixture();
            fixture.Prepare(@"data\053.wav");
            fixture.Player.PresentSound(new PendingSoundEvent(@"data\053.wav", 666, 1));
            AudioSource voice = fixture.Player.GetComponentsInChildren<AudioSource>()[0];
            voice.volume = Mathf.Sqrt(2f);
            Assert.That(voice.volume, Is.EqualTo(1f));
        }

        [Test]
        public void MonoPlaybackAcceptsDirectSoundCenterGain()
        {
            using var fixture = new Fixture();
            const string cue = @"data\053.wav";
            fixture.Prepare(cue);
            fixture.Player.PresentSound(new PendingSoundEvent(cue, 666, 1));
            AudioSource voice = fixture.Assigned().Single();
            Assert.That(voice.clip.channels, Is.EqualTo(2));
            Assert.That(voice.volume, Is.EqualTo(1f).Within(1e-5));
            Assert.That(voice.panStereo, Is.EqualTo(0).Within(1e-5));
        }

        [Test]
        public void DestroyedOwnerReleasesCopiesAndCannotRecreateThem()
        {
            using var fixture = new Fixture();
            const string cue = @"data\053.wav";
            object prepared = fixture.Prepare(cue);
            fixture.Player.PresentSound(new PendingSoundEvent(cue, 666, 1));
            AudioClip playback = fixture.Assigned().Single().clip;
            fixture.DestroyHost();
            Assert.That(playback == null, Is.True);
            prepared.GetType().GetField("BattlePlaybackClips").SetValue(prepared, null);
            typeof(NTSDSoundPlayer).GetMethod("PrepareBattlePlaybackClips",
                BindingFlags.Instance | BindingFlags.NonPublic).Invoke(fixture.Player, new[] { prepared });
            Assert.That(prepared.GetType().GetField("BattlePlaybackClips").GetValue(prepared), Is.Null);
        }

        [Test]
        public void NonBattlePlaybackKeepsIndependentOneShots()
        {
            using var fixture = new Fixture();
            const string cue = "diagnostic-ui.wav";
            fixture.Prepare(cue, false);
            fixture.Player.PlaySfx(cue);
            fixture.Player.PlaySfx(cue);
            Assert.That(fixture.Assigned(), Has.Length.EqualTo(2));
        }

        private sealed class Fixture : IDisposable
        {
            private readonly GameObject host;
            private readonly List<AudioClip> ownedClips = new List<AudioClip>();
            private readonly FieldInfo instance;
            private readonly GameConfig previous;
            private readonly GameConfig config;
            internal NTSDSoundPlayer Player { get; }

            internal Fixture()
            {
                instance = typeof(GameConfig).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
                previous = (GameConfig)instance.GetValue(null);
                config = ScriptableObject.CreateInstance<GameConfig>();
                config.hideFlags = HideFlags.HideAndDontSave;
                config.BattleContentRuntimeRoot = "Assets/NTSD/Content/LoganRuntime";
                instance.SetValue(null, config);
                host = new GameObject("OriginalCommonAudioTests") { hideFlags = HideFlags.HideAndDontSave };
                Player = host.AddComponent<NTSDSoundPlayer>();
            }

            internal object Prepare(string cue, bool battle = true)
            {
                object prepared = typeof(NTSDSoundPlayer).GetMethod("GetOrPrepareCue",
                    BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Player, new object[] { cue, battle });
                Type type = prepared.GetType();
                if ((bool)type.GetField("IsLoaded").GetValue(prepared))
                    return prepared;
                AudioClip clip = AudioClip.Create(cue, 12000, 1, 12000, false);
                ownedClips.Add(clip);
                type.GetField("Clips").SetValue(prepared, new[] { clip });
                type.GetField("IsLoaded").SetValue(prepared, true);
                return prepared;
            }

            internal void DestroyHost()
            {
                if (host == null)
                    return;
                // EditMode fixtures do not receive the Play-mode Mono lifecycle.
                typeof(NTSDSoundPlayer).GetMethod("OnDestroy", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(Player, null);
                UnityEngine.Object.DestroyImmediate(host);
            }

            internal AudioSource[] Assigned()
            {
                return host.GetComponentsInChildren<AudioSource>()
                    .Where(value => value.clip != null).ToArray();
            }

            public void Dispose()
            {
                DestroyHost();
                foreach (AudioClip clip in ownedClips)
                    UnityEngine.Object.DestroyImmediate(clip);
                instance.SetValue(null, previous);
                UnityEngine.Object.DestroyImmediate(config);
            }
        }
    }
}
#endif
