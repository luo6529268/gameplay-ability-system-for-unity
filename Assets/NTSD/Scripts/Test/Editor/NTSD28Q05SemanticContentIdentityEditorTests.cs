#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using NTSD.Animation;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q05SemanticContentIdentityEditorTests
    {
        private const string Raw = "000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F";
        private const string Tag = "NTSD28_LOGAN_DAT_SEMANTICS_V2";
        private static Type IdentityType
        {
            get
            {
                Type type = typeof(LoganObjectCatalog).Assembly.GetType("NTSD.Animation.LoganContentIdentity");
                Assert.That(type, Is.Not.Null, "Semantic content identity is missing.");
                return type;
            }
        }

        private static object Call(string method, params object[] args)
        {
            MethodInfo info = IdentityType.GetMethod(method, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(info, Is.Not.Null, method);
            try { return info.Invoke(null, args); }
            catch (TargetInvocationException error) when (error.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(error.InnerException).Throw();
                throw;
            }
        }

        private static object Property(object value, string name)
        {
            PropertyInfo property = value.GetType().GetProperty(name);
            Assert.That(property, Is.Not.Null, name);
            return property.GetValue(value);
        }

        [TestCase("NTSD28_LOGAN_DAT_SEMANTICS_V1", Raw, "1C7BBF55B73EE0EDE9A99989181D877D7E4A1B4A62EB49E46865B3846D7DB525", 17140769138910657308UL)]
        [TestCase(Tag, Raw, "77F512D2E948F9B6DE003292E27D2DFFD9811850DA3B91EBDF0B1A4F2C60E6F8", 13184649553192875383UL)]
        [TestCase(Tag, "000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1EFF", "D9F7C7CC1337E994779667BDB7C76B302B43625C67EA8906AAB5BB3406547513", 10730168145366480857UL)]
        public void FrozenQ03VectorsMatchExactBytes(string tag, string raw, string digest, ulong projection)
        {
            object identity = Call("ForDecodeContract", raw, tag);
            Assert.That(Property(identity, "RawDefinitionFingerprint"), Is.EqualTo(raw));
            Assert.That(Property(identity, "DecodeContractTag"), Is.EqualTo(tag));
            Assert.That(Property(identity, "SemanticFingerprint"), Is.EqualTo(digest));
            Assert.That(Property(identity, "CatalogFingerprint"), Is.EqualTo(projection));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("xyz")]
        [TestCase("000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1Z")]
        public void InvalidRawIdentityIsRejected(string raw)
        {
            Assert.Throws<ArgumentException>(() => Call("FromDefinitionFingerprint", raw));
        }

        [TestCase("")]
        [TestCase("bad\0tag")]
        [TestCase("非ASCII")]
        public void InvalidDecodeContractIsRejected(string tag)
        {
            Assert.Throws<ArgumentException>(() => Call("ForDecodeContract", Raw, tag));
        }

        [Test]
        public void ProjectionUsesLittleEndianAndProtectsZero()
        {
            Assert.That(Call("ProjectCatalogFingerprint", new byte[32]), Is.EqualTo(1UL));
            var digest = new byte[32];
            digest[0] = 1;
            digest[7] = 2;
            Assert.That(Call("ProjectCatalogFingerprint", digest), Is.EqualTo(0x0200000000000001UL));
        }

        [Test]
        public void CurrentIdentityNormalizesHexAndSeparatesDecoderCacheKeys()
        {
            object current = Call("FromBattleComponents", Raw.ToLowerInvariant(), Raw, Raw);
            object older = Call("ForDecodeContract", Raw, "NTSD28_LOGAN_DAT_SEMANTICS_V1");
            Assert.That(Property(current, "DecodeContractTag"), Is.EqualTo(LoganContentIdentity.CurrentDecodeContractTag));
            Assert.That(Property(current, "ObjectDefinitionFingerprint"), Is.EqualTo(Raw));
            MethodInfo key = IdentityType.GetMethod("CreateSourceCacheKey");
            Assert.That(key, Is.Not.Null);
            Assert.That(key.Invoke(current, new object[] { "root" }), Is.Not.EqualTo(key.Invoke(older, new object[] { "root" })));
        }

        private static string CreateRuntime(string name)
        {
            string root = Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).FullName,
                "Temp/Q05SemanticIdentity", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "decoded_dat"));
            Directory.CreateDirectory(Path.Combine(root, "vfs"));
            File.WriteAllText(Path.Combine(root, "catalog.csv"),
                "registry_section,registry_index,id,type,source_path,published_folder\nobject,0,56,0,a.dat,missing\n", new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(root, "decoded_dat/a.dat"),
                "<bmp_begin>\nname: " + name + "\n<bmp_end>\n<frame> 0 standing\nstate: 0 wait: 1 next: 0\n<frame_end>\n", new UTF8Encoding(false));
            return root;
        }

        [Test]
        public void CatalogAndCandidateCarryRawAndSemanticIdentityThroughCacheKey()
        {
            string root = CreateRuntime("first");
            var candidate = LoganVisualContentCandidate.Capture(BattleContentSource.ForLoganRuntime(root));
            object identity = Property(candidate, "ContentIdentity");
            Assert.That(Property(candidate.Catalog, "ContentIdentity"), Is.SameAs(identity));
            Assert.That(Property(identity, "ObjectDefinitionFingerprint"), Is.EqualTo(candidate.Catalog.DefinitionFingerprint));
            Assert.That(candidate.SourceCacheKey, Does.Contain((string)Property(identity, "SemanticFingerprint")));
            Assert.That(candidate.SourceCacheKey, Does.Contain(candidate.VisualFingerprint));
            Assert.That(candidate.Catalog.SourceCacheKey, Does.Contain((string)Property(identity, "SemanticFingerprint")));
            var relocated = LoganVisualContentCandidate.Capture(BattleContentSource.ForLoganRuntime(CreateRuntime("first")));
            Assert.That(relocated.VisualFingerprint, Is.EqualTo(candidate.VisualFingerprint));
            Assert.That(Property(Property(relocated, "ContentIdentity"), "SemanticFingerprint"), Is.EqualTo(Property(identity, "SemanticFingerprint")));
            Assert.That(relocated.SourceCacheKey, Is.Not.EqualTo(candidate.SourceCacheKey));
            File.AppendAllText(Path.Combine(root, "decoded_dat/a.dat"), "\n# different raw input\n");
            var changed = LoganVisualContentCandidate.Capture(BattleContentSource.ForLoganRuntime(root));
            Assert.That(Property(Property(changed, "ContentIdentity"), "SemanticFingerprint"), Is.Not.EqualTo(Property(identity, "SemanticFingerprint")));
            Assert.Throws<InvalidDataException>(() => candidate.AssertInputsCurrent());
        }

        [Test]
        public void LocalSessionUsesSemanticProjectionAndRejectsOtherContent()
        {
            object current = Call("FromBattleComponents", Raw, Raw, Raw);
            object changed = Call("FromBattleComponents", new string('F', 64), Raw, Raw);
            MethodInfo create = IdentityType.GetMethod("CreateLocalValidationSessionIdentity");
            Assert.That(create, Is.Not.Null);
            object[] args = { 100UL, 424242U, 200UL, new[] { 0 } };
            var session = (LockstepSessionIdentity)create.Invoke(current, args);
            var other = (LockstepSessionIdentity)create.Invoke(changed, args);
            Assert.That(session.CatalogFingerprint, Is.EqualTo(Property(current, "CatalogFingerprint")));
            var input = new StrictDelayedInputBuffer(session, 4);
            Assert.That(input.TrySubmit(new LockstepFramePacket(other, 1, 0, SimulationInputButtons.None)), Is.EqualTo(LockstepProtocolReason.CatalogFingerprintMismatch));
            Assert.That(input.TrySubmit(new LockstepFramePacket(session, 1, 0, SimulationInputButtons.Attack)), Is.EqualTo(LockstepProtocolReason.None));
            Assert.That(input.IsFrameReady(1), Is.True);
        }

        [Test]
        public void CachedCandidateWithEarlierDecodeContractIsRejected()
        {
            var candidate = LoganVisualContentCandidate.Capture(BattleContentSource.ForLoganRuntime(CreateRuntime("cached")));
            object older = Call("ForDecodeContract", candidate.Catalog.DefinitionFingerprint, "NTSD28_LOGAN_DAT_SEMANTICS_V1");
            FieldInfo field = typeof(LoganObjectCatalog).GetField("<ContentIdentity>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(candidate.Catalog, older);
            Assert.Throws<InvalidDataException>(() => candidate.AssertInputsCurrent());
        }

        [Test]
        public void Formal330CandidateRetainsRawIdentityAndPublishesSemanticReceipt()
        {
            var source = BattleContentSource.ForLoganRuntime(@"J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\resources\runtime");
            var candidate = LoganVisualContentCandidate.Capture(source);
            object identity = Property(candidate, "ContentIdentity");
            Assert.That(candidate.Catalog.Entries.Count, Is.EqualTo(330));
            Assert.That(Property(identity, "ObjectDefinitionFingerprint"), Is.EqualTo(candidate.Catalog.DefinitionFingerprint));
            Assert.That(Property(identity, "DecodeContractTag"), Is.EqualTo(LoganContentIdentity.CurrentDecodeContractTag));
            Assert.That(candidate.Catalog.DefinitionFingerprint, Is.EqualTo(LoganObjectCatalog.Read(source).DefinitionFingerprint));
            candidate.AssertInputsCurrent();
            string report = UnityEngine.JsonUtility.ToJson(new IdentityReceipt
            {
                rawDefinition = candidate.Catalog.DefinitionFingerprint,
                rawVisual = candidate.VisualFingerprint,
                decodeContract = (string)Property(identity, "DecodeContractTag"),
                semantic = (string)Property(identity, "SemanticFingerprint"),
                catalogProjection = ((ulong)Property(identity, "CatalogFingerprint")).ToString("X16"),
                cacheKey = candidate.SourceCacheKey,
                catalogEntries = candidate.Catalog.Entries.Count,
                imageInputs = candidate.Images.Count,
            }, true);
            File.WriteAllText(Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).FullName,
                "Temp/Q05SemanticIdentity-formal.json"), report);
        }

        [Serializable]
        private sealed class IdentityReceipt
        {
            public string rawDefinition, rawVisual, decodeContract, semantic, catalogProjection, cacheKey;
            public int catalogEntries, imageInputs;
        }
    }
}
#endif
