#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using NTSD.Animation;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    [Category("FormalContentClosure")]
    public sealed class FormalContentClosureEditorTests
    {
        private const string DatPassword =
            "odBearBecauseHeIsVeryGoodSiuHungIsAGo";
        private const string BdyAuthorityRelativePath =
            "Assets/NTSD/Scripts/Test/Fixtures/" +
            "S0FormalContentClosureBdyAppendixA.tsv";
        private const string BdyAuthorityParentSha256 =
            "614822964144cf3e6a153f7bb9f85932c5e563d701c7cc2e14a0fb913bdacc43";
        private const string BdyAuthorityPayloadSha256 =
            "da9dc3d8279165f7681f261c181b4a842ef2e300c031836985b2ac30d85a1478";
        private const string BdyAuthorityFileSha256 =
            "7a076f65d657c80736258d0c2ee349327f792ebfcbf80e0916cc73d600135bc4";
        private const string BdyCatalogBaselineRelativePath =
            "Temp/CAP-S0-1/bdy-full-catalog-baseline.tsv";
        private const string BdyCatalogCandidateRelativePath =
            "Temp/CAP-S0-1/bdy-full-catalog-candidate.tsv";
        private const string BdyCatalogBaselineSha256 =
            "03eb072da4647577a0891bc7071d95e4ebf9ca48484b494e309a894fc3ef9d5b";
        private const string OPointAuthorityRelativePath =
            "Assets/NTSD/Scripts/Test/Fixtures/" +
            "S0FormalContentClosureOPointAppendixA.tsv";
        private const string OPointAuthorityParentSha256 =
            "614822964144cf3e6a153f7bb9f85932c5e563d701c7cc2e14a0fb913bdacc43";
        private const string OPointAuthorityPayloadSha256 =
            "8fc2c05670a497ab0f1a5a3c645c763b15ab168c620a67a3de780b7b65398567";
        private const string OPointAuthorityFileSha256 =
            "2bf64561a27dcf94fc435002fdf702ab1b5c3675896ca69d36993c4973bda1f7";
        private const string OPointCatalogBaselineRelativePath =
            "Temp/CAP-S0-1/opoint-full-catalog-baseline.tsv";
        private const string OPointCatalogCandidateRelativePath =
            "Temp/CAP-S0-1/opoint-full-catalog-candidate.tsv";
        private const string OPointCatalogBaselineSha256 =
            "60e7951ecf2a7f0c9d11a6d80370ecdf16418845b36ee53faad85857dae34afc";
        private const string OPointResourceManifestSha256 =
            "20797a085278ebcd20dd5033220983b2b920f246277ef0d6305e02f4761f2ad0";
        private const string WPointAuthorityRelativePath =
            "Assets/NTSD/Scripts/Test/Fixtures/" +
            "S0FormalContentClosureWPointAppendixA.tsv";
        private const string WPointAuthorityParentSha256 =
            "614822964144cf3e6a153f7bb9f85932c5e563d701c7cc2e14a0fb913bdacc43";
        private const string WPointAuthorityPayloadSha256 =
            "8490a58c187c81996b183e55a4f7681f32fa1a604c842d188715975432422268";
        private const string WPointAuthorityFileSha256 =
            "ff2af7da98f2681d6f8f53aff5aa671a35c6361a5e701023973b713a01cfd796";
        private const string WPointCatalogBaselineRelativePath =
            "Temp/CAP-S0-1/wpoint-full-catalog-baseline.tsv";
        private const string WPointCatalogCandidateRelativePath =
            "Temp/CAP-S0-1/wpoint-full-catalog-candidate.tsv";
        private const string WPointCatalogBaselineSha256 =
            "PENDING_FRESH_CAPTURE";
        private const string WPointResourceManifestSha256 =
            "afe65d17a8d48d48953b54c89aa695019f52c41ca46e4b9ba10c59420cf89c26";
        private const int DatHeaderLength = 123;

        private static readonly UTF8Encoding StrictUtf8 =
            new UTF8Encoding(false, true);

        private static readonly ResourceIdentity[] BdyInlineResourceIdentities =
        {
            new ResourceIdentity(
                "Assets/NTSD/Config/chars/weapon4.dat",
                "dddf52039f4e4393dc09e17dfd620a24dc8fe1b70f119916baa3cb44a0f3d73a"),
            new ResourceIdentity(
                "Assets/NTSD/Config/effect/heart.dat",
                "6ea50830e0d8f637894cd833612a46a4ce59e54db197aea53dbcbb3d2ba48895"),
            new ResourceIdentity(
                "Assets/NTSD/Config/effect/heart2.dat",
                "e4a6fd8edc57155b4e009876d36426343bdcc0b071054dfd7e995403e56db20c"),
            new ResourceIdentity(
                "Assets/NTSD/Config/effect/heart3.dat",
                "5022085f3ab24a19a545a75631af3bbd0904e2e23d1fb2a7dbdacf1b0908c68d"),
            new ResourceIdentity(
                "Assets/NTSD/Config/effect/heart0.dat",
                "739321c3efcbd7542f8aa04ded23a72df3e917f8268a7b34a942e78cb6fce2d5"),
        };

        private static readonly ResourceIdentity SasoriBdyResourceIdentity =
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/sasori.dat",
                "4fbfd5338ba46d4261dd96cbce888317e75ec34b10c45e8d56bb1450e95e5f28");

        private static readonly ResourceIdentity[] OPointA33ResourceIdentities =
        {
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/deidara.dat",
                "e554a3b0ee8f3dbf10d74b8cee6898a74573ac8466c603edb51bd5936e842ea3"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/hidan.dat",
                "8b5f99aaddf06ce38ca6034ada29a7ba0559518e213383645c0494011946e837"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/itachi.dat",
                "6ad5a8e48190aac4645038c9a8d0936df9ea0ee1087744fe510c3a30ba940535"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/kakuzu.dat",
                "078a08b5e7654d7935c18f20d226c60176f449cd231a09a34aea47befde22982"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/naruto.dat",
                "098d08becf52fd49d7b56d1e9ec4fbb17f86de7389fc19000158ff975bb8b802"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/nckakuzu.dat",
                "3f35758ec2fac38377d3ca7120db110100722904329f112b941c09ade3345336"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/sasori.dat",
                "45a74fb460c006c6b72fbed3ae8d7fd393cd4f16daa061748fa6fb29e3425f29"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/shikamaru.dat",
                "f9c2467b07aad84d12eba41b7c2f58f01642a9556cc9722242f6e19ea8533bb4"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/temari.dat",
                "7c9428bc151845aaf11baf723cbf04d3ec8e75cf3f9e2330c3e18a99db1cccfa"),
            new ResourceIdentity(
                "Assets/NTSD/Config/effect/gate.dat",
                "d61c1a7a1e9c291e708d2f5c5f03902384341e08f5599275081a3e699e2bf64d"),
            new ResourceIdentity(
                "Assets/NTSD/Config/specialattack/earth_creature.dat",
                "35ae8f8ce36ba34b630d9ceebe9eae34a8cd1de133b3b156b40fda48c187a6e2"),
        };

        private static readonly ResourceIdentity[] WPointA37ResourceIdentities =
        {
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/deidara.dat",
                "573b38fc66ab12d5bbb1cba5feec2b830f85ec398bc7ee7305276672219e8fd0"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/hidan.dat",
                "a7434bc2bb0a59e14523a3a759ce21780ac86d8575478437eee42f16f29d51c2"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/jiroubou.dat",
                "b3bad5141bf3f48fb581a7a7c6a8e87d20afa39c2b212b2822960f64420df2c4"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/kabuto.dat",
                "e792e40c9c910606bc38c25337090fee2ae7d3c8f718e9a4333abc54d839ecb8"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/kakuzu.dat",
                "5b3cb86508ef5b2b0ba971fd63f63055a5630e811c094cfaca2e4918cd44d013"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/kankuro.dat",
                "dca3baa219fdc494b0a14a84d61efe3f618864bb4e7e446e6fa071caa8300ed9"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/kidomaru.dat",
                "17a5ee9f046bdaa7d4e730e3b999f9dd9ebeeb72d223ce87ac1a56f5dfd71afa"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/kyubi.dat",
                "0e982ac0096340411bdc7c7eabd1c659aab192e03731c5d1764c0a63e28bf4f7"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/naruto.dat",
                "358a35aff567b4b1e3796d57a3883268d3d913964b86185f81cc79f8ff42f1d1"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/nckakuzu.dat",
                "363ec0cd635e41da007c5df066544ba67b47d1ba072d4fdfd2596a55ed73b86e"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/pein.dat",
                "1730bbec7cecc42b2170cd512938449b69f9eb916d3f0825185250cd9ac2ea9e"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/puppet_sasori.dat",
                "7c3107a0cd6c36a63a76a99f5a8925bcca4b0d4b5e5ca9a767f5f60f85f7a5ba"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/reaper.dat",
                "a19e84f1c4731a1a58e20dfeecffa94cc38f0e34ded09eb48ac2c23617a7dd7e"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/rock_lee.dat",
                "2a1ff5b9e59694c7a472675d29744d9510db40167f4515eb5dc6688523619412"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/sakon.dat",
                "986c27df3e84a37ed74c08640a60eff0480106c4bcde54ee68f7f26bb4770ad8"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/sasori.dat",
                "2ee4b08f69aaae38a2e67cbabb5476afdb83fd70bd579ed4f4a8385f6f7ae638"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/sasuke_cs2.dat",
                "06f9d7594a6c07e4ddb2434ce19fe628f747ecf0c8424089ee9e1556b4521bd8"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/sasuke.dat",
                "5181a1dd25fecb093f89962324a47551401c5f30b736492d765a7a265dd55d0f"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/shikamaru.dat",
                "ae75cc2a469a4cb7195adb38f65dbd06e63dd524fbc8f32c2eb59d8b21faec94"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/shino.dat",
                "76258679656106d0d8ecc0cb10719d4b46c335f9fd6e08a342f663984da8221e"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/tayuya.dat",
                "819d2ca1c3c644523051a279c7fb21bb053ae19a558286c0afff4e66e9beedd9"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/temari.dat",
                "2203ee40a6361acd5ba74550d518294400891c96981e01c11971217f97940f80"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/tenten.dat",
                "98d6f98f3ec7277fb503c61d7d73729fd3b5a8ab5f8c21b383ea58e920f6a969"),
            new ResourceIdentity(
                "Assets/NTSD/Config/Character/yamato.dat",
                "fb851859737fea927d3ccd7c4ca1be66cbc745b310a66037e118236ae425cd8d"),
        };

        [Test]
        [Category("BdyStructure")]
        [Explicit("Captures the authorized pre-change 138-DAT production projection baseline.")]
        public void BdyFullCatalogProjectionBaselineDiagnostic()
        {
            BdyExpectation[] authority = LoadBdyExpectations();
            Assert.That(authority.Length, Is.EqualTo(55));

            string projection = BuildFullCatalogProjection();
            string absolutePath = ResolveProjectPath(BdyCatalogBaselineRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
            File.WriteAllText(absolutePath, projection, new UTF8Encoding(false));

            Debug.Log(
                $"BDY_CATALOG_BASELINE|files=138|rows={CountLines(projection)}|" +
                $"sha256={GetSha256Hex(Encoding.UTF8.GetBytes(projection))}|" +
                $"path={BdyCatalogBaselineRelativePath}");
        }

        [Test]
        [Category("BdyStructure")]
        [Explicit("Runs only after the B59 parser seam and five inline restorations, before sasori A4+C1.")]
        public void BdyFullCatalogCandidateDiffDiagnostic()
        {
            BdyExpectation[] authority = LoadBdyExpectations();
            string baselinePath = ResolveProjectPath(BdyCatalogBaselineRelativePath);
            Assert.That(File.Exists(baselinePath), Is.True,
                "Pre-change Bdy catalog baseline is missing.");

            string baseline = File.ReadAllText(baselinePath, Encoding.UTF8);
            string candidate = BuildFullCatalogProjection();
            AssertExactB59CatalogDiff(baseline, candidate, authority);

            string candidatePath = ResolveProjectPath(BdyCatalogCandidateRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(candidatePath));
            File.WriteAllText(candidatePath, candidate, new UTF8Encoding(false));
            Debug.Log(
                $"BDY_CATALOG_CANDIDATE_PASS|files=138|rows={CountLines(candidate)}|" +
                $"sha256={GetSha256Hex(Encoding.UTF8.GetBytes(candidate))}|" +
                "diff=H49/X5/Y5/other0");
        }

        [Test]
        [Category("BdyStructure")]
        [Explicit("Applies only the five authorized Frame48 inline-token restorations.")]
        public void BdyApplyFiveInlineRestorationsDiagnostic()
        {
            AssertFrozenBdyCatalogBaseline();
            var plans = new List<ResourceWritePlan>();
            foreach (ResourceIdentity identity in BdyInlineResourceIdentities)
            {
                string absolutePath = ResolveProjectPath(identity.RelativePath);
                byte[] originalBytes = File.ReadAllBytes(absolutePath);
                Assert.That(GetSha256Hex(originalBytes), Is.EqualTo(identity.BeforeSha256),
                    $"Unexpected pre-apply hash for {identity.RelativePath}.");

                DecodedResource decoded = DecodeResource(originalBytes, identity.RelativePath);
                MatchCollection frames = Regex.Matches(
                    decoded.Text,
                    @"(?ms)<frame>\s+48\b.*?<frame_end>");
                Assert.That(frames.Count, Is.EqualTo(1),
                    $"Frame48 occurrence drifted in {identity.RelativePath}.");

                Match frame = frames[0];
                const string inlineTokens =
                    "itr: dvx: 8 fall: 70 bdefend: 16 injury: 55 effect: 1";
                Assert.That(frame.Value.Contains(inlineTokens), Is.False,
                    $"Inline tokens already exist in {identity.RelativePath}.");

                MatchCollection insertionSites = Regex.Matches(
                    frame.Value,
                    @"(?m)^(?<line>[ \t]*kind:[ \t]+0[ \t]+x:[ \t]+6[ \t]+" +
                    @"y:[ \t]+16[ \t]+w:[ \t]+39[ \t]+h:[ \t]+19)" +
                    @"(?<newline>\r?\n)");
                Assert.That(insertionSites.Count, Is.EqualTo(1),
                    $"Frame48 Bdy insertion site drifted in {identity.RelativePath}.");

                Match insertion = insertionSites[0];
                string patchedFrame = frame.Value.Remove(insertion.Index, insertion.Length)
                    .Insert(
                        insertion.Index,
                        insertion.Groups["line"].Value + "  " + inlineTokens +
                        insertion.Groups["newline"].Value);
                string patchedText = decoded.Text.Remove(frame.Index, frame.Length)
                    .Insert(frame.Index, patchedFrame);
                Assert.That(
                    Regex.Matches(patchedFrame, Regex.Escape(inlineTokens)).Count,
                    Is.EqualTo(1));

                byte[] patchedBytes = EncodeResource(originalBytes, decoded, patchedText);
                DecodedResource roundTrip = DecodeResource(
                    patchedBytes,
                    identity.RelativePath);
                Assert.That(roundTrip.Text, Is.EqualTo(patchedText));
                plans.Add(new ResourceWritePlan(identity, absolutePath, patchedBytes));
            }

            Assert.That(plans.Count, Is.EqualTo(5));
            foreach (ResourceWritePlan plan in plans)
            {
                Assert.That(
                    GetSha256Hex(File.ReadAllBytes(plan.AbsolutePath)),
                    Is.EqualTo(plan.Identity.BeforeSha256));
            }

            foreach (ResourceWritePlan plan in plans)
            {
                File.WriteAllBytes(plan.AbsolutePath, plan.PatchedBytes);
                string afterSha = GetSha256Hex(File.ReadAllBytes(plan.AbsolutePath));
                Assert.That(afterSha, Is.Not.EqualTo(plan.Identity.BeforeSha256));
                Debug.Log(
                    $"BDY_INLINE_APPLY|{plan.Identity.RelativePath}|" +
                    $"before={plan.Identity.BeforeSha256}|after={afterSha}");
            }
        }

        [Test]
        [Category("BdyStructure")]
        [Explicit("Applies only sasori Frame45 Appendix C.4 A4+C1 after exact-B59 passes.")]
        public void BdyApplySasoriA4C1Diagnostic()
        {
            AssertFrozenBdyCatalogBaseline();
            string absolutePath = ResolveProjectPath(SasoriBdyResourceIdentity.RelativePath);
            byte[] originalBytes = File.ReadAllBytes(absolutePath);
            Assert.That(
                GetSha256Hex(originalBytes),
                Is.EqualTo(SasoriBdyResourceIdentity.BeforeSha256));

            DecodedResource decoded = DecodeResource(
                originalBytes,
                SasoriBdyResourceIdentity.RelativePath);
            MatchCollection frames = Regex.Matches(
                decoded.Text,
                @"(?ms)<frame>\s+45\b.*?<frame_end>");
            Assert.That(frames.Count, Is.EqualTo(2));
            Match frame = frames[frames.Count - 1];
            MatchCollection bodies = Regex.Matches(
                frame.Value,
                @"(?ms)^[ \t]*bdy:\r?\n[ \t]*kind:[ \t]+0[ \t]+" +
                @"x:[ \t]+-?\d+[ \t]+y:[ \t]+-?\d+[ \t]+" +
                @"w:[ \t]+-?\d+[ \t]+h:[ \t]+-?\d+\r?\n" +
                @"[ \t]*bdy_end:");
            Assert.That(bodies.Count, Is.EqualTo(2));
            AssertBodySignature(bodies[0].Value, 21, 80000, 43, 62);
            AssertBodySignature(bodies[1].Value, 51, 34, 26, 16);

            string correctedFirst = ReplaceBodyValue(bodies[1].Value, "x", 51, 11);
            correctedFirst = ReplaceBodyValue(correctedFirst, "y", 34, 32);
            correctedFirst = ReplaceBodyValue(correctedFirst, "w", 26, 33);
            correctedFirst = ReplaceBodyValue(correctedFirst, "h", 16, 43);
            string between = frame.Value.Substring(
                bodies[0].Index + bodies[0].Length,
                bodies[1].Index - bodies[0].Index - bodies[0].Length);
            string replacement = correctedFirst + between + bodies[0].Value;
            string patchedFrame = frame.Value.Remove(
                    bodies[0].Index,
                    bodies[1].Index + bodies[1].Length - bodies[0].Index)
                .Insert(bodies[0].Index, replacement);

            MatchCollection patchedBodies = Regex.Matches(
                patchedFrame,
                @"(?ms)^[ \t]*bdy:\r?\n[ \t]*kind:[ \t]+0[ \t]+" +
                @"x:[ \t]+-?\d+[ \t]+y:[ \t]+-?\d+[ \t]+" +
                @"w:[ \t]+-?\d+[ \t]+h:[ \t]+-?\d+\r?\n" +
                @"[ \t]*bdy_end:");
            Assert.That(patchedBodies.Count, Is.EqualTo(2));
            AssertBodySignature(patchedBodies[0].Value, 11, 32, 33, 43);
            Assert.That(patchedBodies[1].Value, Is.EqualTo(bodies[0].Value),
                "The original 21/80000/43/62 block must move byte-for-byte.");

            string patchedText = decoded.Text.Remove(frame.Index, frame.Length)
                .Insert(frame.Index, patchedFrame);
            byte[] patchedBytes = EncodeResource(originalBytes, decoded, patchedText);
            Assert.That(
                DecodeResource(patchedBytes, SasoriBdyResourceIdentity.RelativePath).Text,
                Is.EqualTo(patchedText));

            Assert.That(
                GetSha256Hex(File.ReadAllBytes(absolutePath)),
                Is.EqualTo(SasoriBdyResourceIdentity.BeforeSha256));
            File.WriteAllBytes(absolutePath, patchedBytes);
            string afterSha = GetSha256Hex(File.ReadAllBytes(absolutePath));
            Assert.That(afterSha, Is.Not.EqualTo(SasoriBdyResourceIdentity.BeforeSha256));
            Debug.Log(
                $"BDY_SASORI_APPLY|{SasoriBdyResourceIdentity.RelativePath}|" +
                $"before={SasoriBdyResourceIdentity.BeforeSha256}|after={afterSha}|" +
                "A4=4|C1=1|bdyCount=2");
        }

        [Test]
        [Category("OPointStructure")]
        [Explicit("Captures the authorized pre-change 138-DAT OPoint projection baseline.")]
        public void OPointFullCatalogProjectionBaselineDiagnostic()
        {
            OPointExpectation[] authority = LoadOPointExpectations();
            Assert.That(authority.Length, Is.EqualTo(36));
            AssertOPointResourceManifest();

            string projection = BuildFullCatalogProjection();
            string absolutePath = ResolveProjectPath(OPointCatalogBaselineRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
            File.WriteAllText(absolutePath, projection, new UTF8Encoding(false));

            Debug.Log(
                $"OPOINT_CATALOG_BASELINE|files=138|rows={CountLines(projection)}|" +
                $"sha256={GetSha256Hex(Encoding.UTF8.GetBytes(projection))}|" +
                $"manifest={OPointResourceManifestSha256}|" +
                $"path={OPointCatalogBaselineRelativePath}");
        }

        [Test]
        [Category("OPointStructure")]
        [Explicit("Validates the exact authorized A33 plus Frog B1 catalog delta.")]
        public void OPointFullCatalogCandidateDiffDiagnostic()
        {
            OPointExpectation[] authority = LoadOPointExpectations();
            string baselinePath = ResolveProjectPath(OPointCatalogBaselineRelativePath);
            Assert.That(File.Exists(baselinePath), Is.True,
                "Pre-change OPoint catalog baseline is missing.");

            string baseline = File.ReadAllText(baselinePath, Encoding.UTF8);
            Assert.That(
                GetSha256Hex(Encoding.UTF8.GetBytes(baseline)),
                Is.EqualTo(OPointCatalogBaselineSha256),
                "Pre-change OPoint catalog baseline changed.");
            Assert.That(CountLines(baseline), Is.EqualTo(180190));
            string candidate = BuildFullCatalogProjection();
            AssertExactOPointCatalogDiff(baseline, candidate, authority);
            AssertKisame292WPointUnchanged(baseline, candidate);

            string candidatePath = ResolveProjectPath(OPointCatalogCandidateRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(candidatePath));
            File.WriteAllText(candidatePath, candidate, new UTF8Encoding(false));
            Debug.Log(
                $"OPOINT_CATALOG_CANDIDATE_PASS|files=138|rows={CountLines(candidate)}|" +
                $"sha256={GetSha256Hex(Encoding.UTF8.GetBytes(candidate))}|" +
                "diff=A31-tuples/Frog-count-and-tuple/other0|kisame292-wpoint-unchanged");
        }

        [Test]
        [Category("OPointStructure")]
        [Explicit("Applies only Appendix-E A33 tokens after every preflight gate passes.")]
        public void OPointApplyA33Diagnostic()
        {
            OPointExpectation[] authority = LoadOPointExpectations();
            AssertOPointResourceManifest();
            Dictionary<string, ResourceIdentity> identities =
                OPointA33ResourceIdentities.ToDictionary(
                    value => value.RelativePath,
                    StringComparer.Ordinal);
            var plans = new List<ResourceWritePlan>();
            int changedRows = 0;
            int changedTokens = 0;

            foreach (IGrouping<string, OPointExpectation> group in authority
                         .GroupBy(value => value.Row.RelativePath, StringComparer.Ordinal)
                         .OrderBy(value => value.Key, StringComparer.Ordinal))
            {
                if (!identities.TryGetValue(group.Key, out ResourceIdentity identity))
                    continue;

                string absolutePath = ResolveProjectPath(identity.RelativePath);
                byte[] originalBytes = File.ReadAllBytes(absolutePath);
                Assert.That(GetSha256Hex(originalBytes), Is.EqualTo(identity.BeforeSha256),
                    $"Unexpected pre-apply hash for {identity.RelativePath}.");
                DecodedResource decoded = DecodeResource(originalBytes, identity.RelativePath);
                string patchedText = decoded.Text;

                foreach (OPointExpectation expectation in group
                             .OrderBy(value => value.Row.FrameId))
                {
                    int rowChanges = 0;
                    patchedText = PatchOPointExpectation(
                        patchedText,
                        expectation,
                        ref rowChanges);
                    if (rowChanges > 0)
                    {
                        changedRows++;
                        changedTokens += rowChanges;
                    }
                }

                byte[] patchedBytes = EncodeResource(originalBytes, decoded, patchedText);
                Assert.That(patchedBytes.SequenceEqual(originalBytes), Is.False,
                    $"Expected A33 changes in {identity.RelativePath}.");
                plans.Add(new ResourceWritePlan(identity, absolutePath, patchedBytes));
            }

            Assert.That(changedRows, Is.EqualTo(31));
            Assert.That(changedTokens, Is.EqualTo(33));
            Assert.That(plans.Count, Is.EqualTo(11));
            Assert.That(
                plans.Select(value => value.Identity.RelativePath).OrderBy(value => value),
                Is.EqualTo(identities.Keys.OrderBy(value => value)));

            foreach (ResourceWritePlan plan in plans)
            {
                Assert.That(
                    GetSha256Hex(File.ReadAllBytes(plan.AbsolutePath)),
                    Is.EqualTo(plan.Identity.BeforeSha256));
            }

            foreach (ResourceWritePlan plan in plans)
            {
                File.WriteAllBytes(plan.AbsolutePath, plan.PatchedBytes);
                Debug.Log(
                    $"OPOINT_A33_APPLY|{plan.Identity.RelativePath}|" +
                    $"before={plan.Identity.BeforeSha256}|" +
                    $"after={GetSha256Hex(plan.PatchedBytes)}");
            }
        }

        [TestCaseSource(nameof(OPointCases))]
        [Category("OPointStructure")]
        public void OPointProductionProjectionMatchesFrozenAuthority(
            OPointExpectation expectation)
        {
            LF2FrameData frame = LoadFrame(expectation.Row);
            Assert.That(frame.opoints.Count, Is.EqualTo(1), expectation.ToString());
            Assert.That(
                FormatObjectPoint(frame.opoints[0]),
                Is.EqualTo(expectation.ReleaseExpected),
                expectation.ToString());
        }

        [Test]
        [Category("WPointStructure")]
        [Explicit("Captures the authorized pre-change 138-DAT WPoint projection baseline.")]
        public void WPointFullCatalogProjectionBaselineDiagnostic()
        {
            WPointExpectation[] authority = LoadWPointExpectations();
            Assert.That(authority.Length, Is.EqualTo(36));
            AssertWPointResourceManifest();

            string projection = BuildFullCatalogProjection();
            string absolutePath = ResolveProjectPath(WPointCatalogBaselineRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
            File.WriteAllText(absolutePath, projection, new UTF8Encoding(false));

            Debug.Log(
                $"WPOINT_CATALOG_BASELINE|files=138|rows={CountLines(projection)}|" +
                $"sha256={GetSha256Hex(Encoding.UTF8.GetBytes(projection))}|" +
                $"manifest={WPointResourceManifestSha256}|" +
                $"path={WPointCatalogBaselineRelativePath}");
        }

        [Test]
        [Category("WPointStructure")]
        [Explicit("Validates the exact authorized A37 plus kisame B1 catalog delta.")]
        public void WPointFullCatalogCandidateDiffDiagnostic()
        {
            WPointExpectation[] authority = LoadWPointExpectations();
            string baselinePath = ResolveProjectPath(WPointCatalogBaselineRelativePath);
            Assert.That(File.Exists(baselinePath), Is.True,
                "Pre-change WPoint catalog baseline is missing.");

            string baseline = File.ReadAllText(baselinePath, Encoding.UTF8);
            Assert.That(
                GetSha256Hex(Encoding.UTF8.GetBytes(baseline)),
                Is.EqualTo(WPointCatalogBaselineSha256),
                "Pre-change WPoint catalog baseline changed.");
            string candidate = BuildFullCatalogProjection();
            AssertExactWPointCatalogDiff(baseline, candidate, authority);

            string candidatePath = ResolveProjectPath(WPointCatalogCandidateRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(candidatePath));
            File.WriteAllText(candidatePath, candidate, new UTF8Encoding(false));
            Debug.Log(
                $"WPOINT_CATALOG_CANDIDATE_PASS|files=138|rows={CountLines(candidate)}|" +
                $"sha256={GetSha256Hex(Encoding.UTF8.GetBytes(candidate))}|" +
                "diff=A35-tuples/kisame-count-and-tuple/other0");
        }

        [Test]
        [Category("WPointStructure")]
        [Explicit("Applies only Appendix-F A37 tokens after every preflight gate passes.")]
        public void WPointApplyA37Diagnostic()
        {
            WPointExpectation[] authority = LoadWPointExpectations();
            AssertWPointResourceManifest();
            Dictionary<string, ResourceIdentity> identities =
                WPointA37ResourceIdentities.ToDictionary(
                    value => value.RelativePath,
                    StringComparer.Ordinal);
            var plans = new List<ResourceWritePlan>();
            int changedRows = 0;
            int changedTokens = 0;

            foreach (IGrouping<string, WPointExpectation> group in authority
                         .Where(value => value.Row.ObjectId != 17)
                         .GroupBy(value => value.Row.RelativePath, StringComparer.Ordinal)
                         .OrderBy(value => value.Key, StringComparer.Ordinal))
            {
                Assert.That(identities.TryGetValue(
                    group.Key,
                    out ResourceIdentity identity), Is.True, group.Key);
                string absolutePath = ResolveProjectPath(identity.RelativePath);
                byte[] originalBytes = File.ReadAllBytes(absolutePath);
                Assert.That(GetSha256Hex(originalBytes), Is.EqualTo(identity.BeforeSha256),
                    $"Unexpected pre-apply hash for {identity.RelativePath}.");
                DecodedResource decoded = DecodeResource(originalBytes, identity.RelativePath);
                string patchedText = decoded.Text;

                foreach (WPointExpectation expectation in group
                             .OrderBy(value => value.Row.FrameId))
                {
                    int rowChanges = 0;
                    patchedText = PatchWPointExpectation(
                        patchedText,
                        expectation,
                        ref rowChanges);
                    Assert.That(rowChanges, Is.GreaterThan(0), expectation.ToString());
                    changedRows++;
                    changedTokens += rowChanges;
                }

                byte[] patchedBytes = EncodeResource(originalBytes, decoded, patchedText);
                Assert.That(patchedBytes.SequenceEqual(originalBytes), Is.False,
                    $"Expected A37 changes in {identity.RelativePath}.");
                plans.Add(new ResourceWritePlan(identity, absolutePath, patchedBytes));
            }

            Assert.That(changedRows, Is.EqualTo(35));
            Assert.That(changedTokens, Is.EqualTo(37));
            Assert.That(plans.Count, Is.EqualTo(24));
            Assert.That(
                plans.Select(value => value.Identity.RelativePath).OrderBy(value => value),
                Is.EqualTo(identities.Keys.OrderBy(value => value)));

            foreach (ResourceWritePlan plan in plans)
            {
                Assert.That(
                    GetSha256Hex(File.ReadAllBytes(plan.AbsolutePath)),
                    Is.EqualTo(plan.Identity.BeforeSha256));
            }

            foreach (ResourceWritePlan plan in plans)
            {
                File.WriteAllBytes(plan.AbsolutePath, plan.PatchedBytes);
                Debug.Log(
                    $"WPOINT_A37_APPLY|{plan.Identity.RelativePath}|" +
                    $"before={plan.Identity.BeforeSha256}|" +
                    $"after={GetSha256Hex(plan.PatchedBytes)}");
            }
        }

        [TestCaseSource(nameof(WPointCases))]
        [Category("WPointStructure")]
        public void WPointProductionProjectionMatchesFrozenAuthority(
            WPointExpectation expectation)
        {
            LF2FrameData frame = LoadFrame(expectation.Row);
            Assert.That(
                frame.FormalWeaponPoints.Count,
                Is.EqualTo(1),
                expectation.ToString());
            Assert.That(
                FormatWeaponPoint(frame.FormalWeaponPoints[0]),
                Is.EqualTo(expectation.ReleaseExpected),
                expectation.ToString());
        }

        [TestCaseSource(nameof(BdyCases))]
        [Category("BdyStructure")]
        public void BdyProductionProjectionMatchesFrozenAppendixA(
            BdyExpectation expectation)
        {
            LF2FrameData frame = LoadFrame(expectation.Row);
            string[] actual = frame.bodies
                .Select(FormatBdy)
                .ToArray();
            Assert.That(actual, Is.EqualTo(expectation.ReleaseExpected),
                $"OID{expectation.Row.ObjectId} Frame{expectation.Row.FrameId} " +
                $"from {expectation.Row.RelativePath}");
        }

        [Test]
        [Category("BdyStructure")]
        [Explicit("Read-only current-worktree measurement for checkpoint-3 Bdy classification.")]
        public void BdyCheckpointCurrentBeforeDiagnostic()
        {
            int rowCount = 0;
            foreach (ScalarExpectation row in BdyRows())
            {
                Lf2FrameBlock frameBlock = LoadFrameBlock(row);
                string[] raw = frameBlock.SubBlocks
                    .Where(block => string.Equals(
                        block.Name,
                        "bdy",
                        StringComparison.OrdinalIgnoreCase))
                    .Select((block, ordinal) =>
                        $"{ordinal}:" + string.Join(
                            ";",
                            block.Properties.Select(
                                property => $"{property.Key}={property.Value}")))
                    .ToArray();
                LF2FrameData frame = Lf2DatConverter.ConvertToFrameData(frameBlock);
                string[] typed = frame.bodies
                    .Select((body, ordinal) =>
                        $"{ordinal}:x={body.X};y={body.Y};w={body.W};h={body.H}")
                    .ToArray();

                Debug.Log(
                    $"BDY_BEFORE|{row.ObjectId}|{row.RelativePath}|" +
                    $"{row.FrameId}|raw={raw.Length}|{string.Join("||", raw)}|" +
                    $"typed={typed.Length}|{string.Join("||", typed)}");
                rowCount++;
            }

            Assert.That(rowCount, Is.EqualTo(55));
        }

        [Test]
        [Category("ItrStructure")]
        [Explicit("One-time read-only current-worktree measurement before checkpoint-2 expected-mismatch assertions are frozen.")]
        public void ItrCheckpointCurrentBeforeDiagnostic()
        {
            int rowCount = 0;
            foreach (ScalarExpectation row in ItrRows())
            {
                LF2FrameData frame = LoadFrame(row);
                if (frame.itrs.Count == 0)
                {
                    Debug.Log(
                        $"ITR_BEFORE|{row.ObjectId}|{row.RelativePath}|" +
                        $"{row.FrameId}|0|-1|");
                    rowCount++;
                    continue;
                }

                for (int ordinal = 0; ordinal < frame.itrs.Count; ordinal++)
                {
                    InteractionArea itr = frame.itrs[ordinal];
                    Debug.Log(
                        $"ITR_BEFORE|{row.ObjectId}|{row.RelativePath}|" +
                        $"{row.FrameId}|{frame.itrs.Count}|{ordinal}|" +
                        FormatItr(itr));
                }

                rowCount++;
            }

            Assert.That(rowCount, Is.EqualTo(10));
        }

        [Test]
        [Category("ItrStructure")]
        public void A_ItrProductionProjectionMatchesFrozenReleaseSequences()
        {
            var differences = new List<string>();
            foreach (ItrExpectation expectation in ItrExpectations())
            {
                LF2FrameData frame = LoadFrame(expectation.Row);
                string[] actual = frame.itrs
                    .Select(FormatItrEvaluation)
                    .ToArray();

                Assert.That(
                    expectation.ClientBefore,
                    Is.Not.EqualTo(expectation.ReleaseExpected),
                    $"Frozen test-first red baseline must remain distinct for " +
                    $"OID{expectation.Row.ObjectId} Frame{expectation.Row.FrameId}");

                if (!actual.SequenceEqual(expectation.ReleaseExpected))
                {
                    differences.Add(
                        $"OID{expectation.Row.ObjectId} Frame{expectation.Row.FrameId}: " +
                        $"expected count {expectation.ReleaseExpected.Length} " +
                        $"[{string.Join(" || ", expectation.ReleaseExpected)}], " +
                        $"actual count {actual.Length} [{string.Join(" || ", actual)}], " +
                        $"path {expectation.Row.RelativePath}");
                }
            }

            Assert.That(
                differences,
                Is.Empty,
                "Itr production-parser differences:\n" +
                string.Join("\n", differences));
        }

        [Test]
        [Category("FrameScalar")]
        public void A_FrameScalarAggregateWitnessMatchesFrozenReleaseTuples()
        {
            var differences = new List<string>();
            foreach (TestCaseData testCase in FrameScalarCases())
            {
                var expected = (ScalarExpectation)testCase.Arguments[0];
                LF2FrameData actual = LoadFrame(expected);
                AppendDifference(differences, expected.Pic, actual.pic, expected, "pic");
                AppendDifference(differences, expected.State, actual.state, expected, "state");
                AppendDifference(differences, expected.Wait, actual.wait, expected, "wait");
                AppendDifference(differences, expected.Next, actual.next, expected, "next");
                AppendDifference(differences, expected.Dvx, actual.dvx, expected, "dvx");
                AppendDifference(differences, expected.Dvy, actual.dvy, expected, "dvy");
                AppendDifference(differences, expected.CenterX, actual.centerx, expected, "centerx");
                AppendDifference(differences, expected.CenterY, actual.centery, expected, "centery");
                AppendDifference(differences, expected.HitD, actual.hit_d, expected, "hit_d");
                AppendDifference(differences, expected.HitDj, actual.hit_Dj, expected, "hit_Dj");
            }

            Assert.That(differences, Is.Empty,
                "Frame scalar production-parser differences:\n" +
                string.Join("\n", differences));
        }

        [TestCaseSource(nameof(FrameScalarCases))]
        [Category("FrameScalar")]
        public void FrameScalarProductionProjectionMatchesFrozenReleaseTuple(
            ScalarExpectation expected)
        {
            LF2FrameData actual = LoadFrame(expected);
            AssertExpected(expected.Pic, actual.pic, expected, "pic");
            AssertExpected(expected.State, actual.state, expected, "state");
            AssertExpected(expected.Wait, actual.wait, expected, "wait");
            AssertExpected(expected.Next, actual.next, expected, "next");
            AssertExpected(expected.Dvx, actual.dvx, expected, "dvx");
            AssertExpected(expected.Dvy, actual.dvy, expected, "dvy");
            AssertExpected(expected.CenterX, actual.centerx, expected, "centerx");
            AssertExpected(expected.CenterY, actual.centery, expected, "centery");
            AssertExpected(expected.HitD, actual.hit_d, expected, "hit_d");
            AssertExpected(expected.HitDj, actual.hit_Dj, expected, "hit_Dj");
        }

        private static LF2FrameData LoadFrame(ScalarExpectation expected)
        {
            return Lf2DatConverter.ConvertToFrameData(LoadFrameBlock(expected));
        }

        private static Lf2FrameBlock LoadFrameBlock(ScalarExpectation expected)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            Assert.That(projectRoot, Is.Not.Null.And.Not.Empty);

            string absolutePath = Path.Combine(
                projectRoot,
                expected.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.That(File.Exists(absolutePath), Is.True,
                $"OID{expected.ObjectId} DAT missing: {expected.RelativePath}");

            string text = Lf2DatDecryptor.DecryptFile(absolutePath, DatPassword);
            Lf2DatFile dat = new Lf2DatParserV2().Parse(text, absolutePath);
            Lf2FrameBlock frameBlock = dat.Frames.LastOrDefault(
                frame => frame.FrameIndex == expected.FrameId);
            Assert.That(frameBlock, Is.Not.Null,
                $"OID{expected.ObjectId} Frame{expected.FrameId} missing");
            return frameBlock;
        }

        private static string BuildFullCatalogProjection()
        {
            string configRoot = ResolveProjectPath("Assets/NTSD/Config");
            string[] files = Directory.GetFiles(
                    configRoot,
                    "*.dat",
                    SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            Assert.That(files.Length, Is.EqualTo(138),
                "The frozen full-catalog projection requires exactly 138 Client DAT files.");

            var output = new StringBuilder(8 * 1024 * 1024);
            var parser = new Lf2DatParserV2();
            foreach (string absolutePath in files)
            {
                string relativePath = absolutePath
                    .Substring(configRoot.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Replace(Path.DirectorySeparatorChar, '/');
                string text = Lf2DatDecryptor.DecryptFile(absolutePath, DatPassword);
                Lf2DatFile dat = parser.Parse(text, absolutePath);
                AppendProjectionLine(
                    output,
                    relativePath,
                    -1,
                    -1,
                    "FILE",
                    -1,
                    "frameCount",
                    dat.Frames.Count.ToString());

                for (int occurrence = 0; occurrence < dat.Frames.Count; occurrence++)
                {
                    Lf2FrameBlock frameBlock = null;
                    // Kept as an explicit assignment below so a future model rename
                    // cannot silently change source-occurrence ownership.
                    frameBlock = dat.Frames[occurrence];
                    LF2FrameData frame = Lf2DatConverter.ConvertToFrameData(frameBlock);
                    int frameId = frameBlock.FrameIndex;

                    AppendProjectionLine(
                        output,
                        relativePath,
                        occurrence,
                        frameId,
                        "FRAME",
                        -1,
                        "scalars",
                        FormatFrameScalars(frame));

                    AppendProjectionLine(output, relativePath, occurrence, frameId,
                        "ITR", -1, "count", frame.itrs.Count.ToString());
                    for (int index = 0; index < frame.itrs.Count; index++)
                    {
                        AppendProjectionLine(output, relativePath, occurrence, frameId,
                            "ITR", index, "tuple", FormatItr(frame.itrs[index]));
                    }

                    AppendProjectionLine(output, relativePath, occurrence, frameId,
                        "BDY", -1, "count", frame.bodies.Count.ToString());
                    for (int index = 0; index < frame.bodies.Count; index++)
                    {
                        BattleBodyBoxValue body = frame.bodies[index];
                        AppendProjectionLine(output, relativePath, occurrence, frameId,
                            "BDY", index, "x", body.X.ToString());
                        AppendProjectionLine(output, relativePath, occurrence, frameId,
                            "BDY", index, "y", body.Y.ToString());
                        AppendProjectionLine(output, relativePath, occurrence, frameId,
                            "BDY", index, "w", body.W.ToString());
                        AppendProjectionLine(output, relativePath, occurrence, frameId,
                            "BDY", index, "h", body.H.ToString());
                    }

                    AppendProjectionLine(output, relativePath, occurrence, frameId,
                        "OPOINT", -1, "count", frame.opoints.Count.ToString());
                    for (int index = 0; index < frame.opoints.Count; index++)
                    {
                        AppendProjectionLine(output, relativePath, occurrence, frameId,
                            "OPOINT", index, "tuple", FormatObjectPoint(frame.opoints[index]));
                    }

                    AppendProjectionLine(output, relativePath, occurrence, frameId,
                        "WPOINT", -1, "count", frame.FormalWeaponPoints.Count.ToString());
                    for (int index = 0; index < frame.FormalWeaponPoints.Count; index++)
                    {
                        AppendProjectionLine(output, relativePath, occurrence, frameId,
                            "WPOINT", index, "tuple",
                            FormatWeaponPoint(frame.FormalWeaponPoints[index]));
                    }

                    AppendProjectionLine(output, relativePath, occurrence, frameId,
                        "BPOINT", -1, "count", frame.BloodPoints.Count.ToString());
                    for (int index = 0; index < frame.BloodPoints.Count; index++)
                    {
                        AppendProjectionLine(output, relativePath, occurrence, frameId,
                            "BPOINT", index, "tuple", FormatBloodPoint(frame.BloodPoints[index]));
                    }

                    AppendProjectionLine(output, relativePath, occurrence, frameId,
                        "CPOINT", -1, "count", frame.CatchPoints.Count.ToString());
                    for (int index = 0; index < frame.CatchPoints.Count; index++)
                    {
                        AppendProjectionLine(output, relativePath, occurrence, frameId,
                            "CPOINT", index, "tuple", FormatCatchPoint(frame.CatchPoints[index]));
                    }
                }
            }

            return output.ToString();
        }

        private static void AppendProjectionLine(
            StringBuilder output,
            string relativePath,
            int occurrence,
            int frameId,
            string domain,
            int childOrdinal,
            string property,
            string value)
        {
            output.Append(relativePath).Append('\t')
                .Append(occurrence).Append('\t')
                .Append(frameId).Append('\t')
                .Append(domain).Append('\t')
                .Append(childOrdinal).Append('\t')
                .Append(property).Append('\t')
                .Append(value).Append('\n');
        }

        private static string FormatFrameScalars(LF2FrameData frame)
        {
            string encodedName = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(frame.frameName ?? string.Empty));
            string encodedSound = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(frame.sound ?? string.Empty));
            return
                $"name64={encodedName};pic={frame.pic};state={frame.state};" +
                $"wait={frame.wait};next={frame.next};dvx={frame.dvx};" +
                $"dvy={frame.dvy};dvz={frame.dvz};centerx={frame.centerx};" +
                $"centery={frame.centery};mp={frame.mp};hit_a={frame.hit_a};" +
                $"hit_d={frame.hit_d};hit_j={frame.hit_j};hit_Fj={frame.hit_Fj};" +
                $"hit_Fa={frame.hit_Fa};hit_Da={frame.hit_Da};hit_Ua={frame.hit_Ua};" +
                $"hit_ja={frame.hit_ja};hit_Dj={frame.hit_Dj};hit_Uj={frame.hit_Uj};" +
                $"sound64={encodedSound}";
        }

        private static string FormatBdy(BattleBodyBoxValue value) =>
            $"x={value.X};y={value.Y};w={value.W};h={value.H}";

        private static string FormatObjectPoint(BattleObjectPointValue value) =>
            $"kind={value.Kind};x={value.X};y={value.Y};action={value.Action};" +
            $"dvx={value.Dvx};dvy={value.Dvy};oid={value.Oid};facing={value.Facing}";

        private static string FormatWeaponPoint(BattleWeaponPointValue value) =>
            $"kind={value.Kind};x={value.X};y={value.Y};attacking={value.Attacking};" +
            $"cover={value.Cover};weaponact={value.WeaponAct};dvx={value.Dvx};" +
            $"dvy={value.Dvy};dvz={value.Dvz}";

        private static string FormatBloodPoint(BattleBloodPointValue value) =>
            $"x={value.X};y={value.Y}";

        private static string FormatCatchPoint(BattleCatchPointValue value) =>
            $"kind={value.Kind};x={value.X};y={value.Y};injury={value.Injury};" +
            $"cover={value.Cover};vaction={value.Vaction};aaction={value.Aaction};" +
            $"jaction={value.Jaction};daction={value.Daction};throwvx={value.ThrowVx};" +
            $"throwvy={value.ThrowVy};hurtable={value.Hurtable};decrease={value.Decrease};" +
            $"dircontrol={value.DirControl};taction={value.Taction};" +
            $"throwinjury={value.ThrowInjury};throwvz={value.ThrowVz};" +
            $"fronthurtact={value.FrontHurtAct};backhurtact={value.BackHurtAct}";

        private static string ResolveProjectPath(string relativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            Assert.That(projectRoot, Is.Not.Null.And.Not.Empty);
            return Path.GetFullPath(Path.Combine(
                projectRoot,
                relativePath.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static void AssertFrozenBdyCatalogBaseline()
        {
            string baselinePath = ResolveProjectPath(BdyCatalogBaselineRelativePath);
            Assert.That(File.Exists(baselinePath), Is.True);
            Assert.That(
                GetSha256Hex(File.ReadAllBytes(baselinePath)),
                Is.EqualTo(BdyCatalogBaselineSha256),
                "The authorized pre-change 138-DAT baseline changed.");
            Assert.That(CountLines(File.ReadAllText(baselinePath, Encoding.UTF8)),
                Is.EqualTo(180190));
        }

        private static DecodedResource DecodeResource(
            byte[] bytes,
            string relativePath)
        {
            int plainStart = bytes.Length >= 3 &&
                bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf
                    ? 3
                    : 0;
            string head = Encoding.ASCII.GetString(
                bytes,
                plainStart,
                Math.Min(64, bytes.Length - plainStart));
            string trimmedHead = head.TrimStart();
            bool isPlaintext =
                trimmedHead.StartsWith("<bmp_begin>", StringComparison.Ordinal) ||
                trimmedHead.StartsWith("<frame>", StringComparison.Ordinal) ||
                trimmedHead.StartsWith("<stage>", StringComparison.Ordinal);
            if (isPlaintext)
            {
                string text = StrictUtf8.GetString(
                    bytes,
                    plainStart,
                    bytes.Length - plainStart);
                byte[] roundTrip = StrictUtf8.GetBytes(text);
                Assert.That(
                    bytes.Skip(plainStart).SequenceEqual(roundTrip),
                    Is.True,
                    $"Plaintext DAT is not strict UTF-8 round-trippable: {relativePath}");
                return new DecodedResource(text, true, plainStart);
            }

            Assert.That(bytes.Length, Is.GreaterThan(DatHeaderLength),
                $"Encrypted DAT header is incomplete: {relativePath}");
            int payloadLength = bytes.Length - DatHeaderLength;
            var payload = new byte[payloadLength];
            for (int index = 0; index < payloadLength; index++)
            {
                unchecked
                {
                    payload[index] = (byte)(bytes[index + DatHeaderLength] -
                        (byte)DatPassword[index % DatPassword.Length]);
                }
            }

            return new DecodedResource(
                Encoding.ASCII.GetString(payload),
                false,
                DatHeaderLength);
        }

        private static byte[] EncodeResource(
            byte[] originalBytes,
            DecodedResource decoded,
            string text)
        {
            if (decoded.IsPlaintext)
            {
                byte[] payload = StrictUtf8.GetBytes(text);
                var result = new byte[decoded.PrefixLength + payload.Length];
                Buffer.BlockCopy(
                    originalBytes,
                    0,
                    result,
                    0,
                    decoded.PrefixLength);
                Buffer.BlockCopy(
                    payload,
                    0,
                    result,
                    decoded.PrefixLength,
                    payload.Length);
                return result;
            }

            Assert.That(text.All(value => value <= 0x7f), Is.True,
                "Encrypted DAT patch introduced a non-ASCII character.");
            byte[] plainBytes = Encoding.ASCII.GetBytes(text);
            var encrypted = new byte[DatHeaderLength + plainBytes.Length];
            Buffer.BlockCopy(originalBytes, 0, encrypted, 0, DatHeaderLength);
            for (int index = 0; index < plainBytes.Length; index++)
            {
                unchecked
                {
                    encrypted[index + DatHeaderLength] = (byte)(plainBytes[index] +
                        (byte)DatPassword[index % DatPassword.Length]);
                }
            }

            return encrypted;
        }

        private static void AssertBodySignature(
            string body,
            int x,
            int y,
            int w,
            int h)
        {
            Assert.That(ReadBodyValue(body, "x"), Is.EqualTo(x));
            Assert.That(ReadBodyValue(body, "y"), Is.EqualTo(y));
            Assert.That(ReadBodyValue(body, "w"), Is.EqualTo(w));
            Assert.That(ReadBodyValue(body, "h"), Is.EqualTo(h));
        }

        private static int ReadBodyValue(string body, string propertyName)
        {
            MatchCollection matches = Regex.Matches(
                body,
                @"(?<![A-Za-z0-9_])" + Regex.Escape(propertyName) +
                @":[ \t]+(?<value>-?\d+)(?![A-Za-z0-9_])");
            Assert.That(matches.Count, Is.EqualTo(1));
            return int.Parse(matches[0].Groups["value"].Value);
        }

        private static string ReplaceBodyValue(
            string body,
            string propertyName,
            int before,
            int after)
        {
            string pattern =
                @"(?<![A-Za-z0-9_])(?<name>" + Regex.Escape(propertyName) +
                @"):(?<space>[ \t]+)" + before + @"(?![A-Za-z0-9_])";
            MatchCollection matches = Regex.Matches(body, pattern);
            Assert.That(matches.Count, Is.EqualTo(1));
            Match match = matches[0];
            return body.Remove(match.Index, match.Length)
                .Insert(
                    match.Index,
                    match.Groups["name"].Value + ":" +
                    match.Groups["space"].Value + after);
        }

        private static int CountLines(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;
            int count = 0;
            for (int index = 0; index < text.Length; index++)
            {
                if (text[index] == '\n')
                    count++;
            }
            return count;
        }

        private static IEnumerable<TestCaseData> BdyCases()
        {
            foreach (BdyExpectation expectation in LoadBdyExpectations())
            {
                yield return new TestCaseData(expectation).SetName(
                    $"Bdy_OID{expectation.Row.ObjectId}_Frame{expectation.Row.FrameId}");
            }
        }

        private static IEnumerable<TestCaseData> OPointCases()
        {
            foreach (OPointExpectation expectation in LoadOPointExpectations())
            {
                yield return new TestCaseData(expectation).SetName(
                    $"OPoint_OID{expectation.Row.ObjectId}_Frame{expectation.Row.FrameId}");
            }
        }

        private static IEnumerable<TestCaseData> WPointCases()
        {
            foreach (WPointExpectation expectation in LoadWPointExpectations())
            {
                yield return new TestCaseData(expectation).SetName(
                    $"WPoint_OID{expectation.Row.ObjectId}_Frame{expectation.Row.FrameId}");
            }
        }

        private static OPointExpectation[] LoadOPointExpectations()
        {
            string absolutePath = ResolveProjectPath(OPointAuthorityRelativePath);
            byte[] bytes = File.ReadAllBytes(absolutePath);
            Assert.That(GetSha256Hex(bytes), Is.EqualTo(OPointAuthorityFileSha256),
                "Appendix-A OPoint fixture file hash changed.");

            string text = StrictUtf8.GetString(bytes);
            int firstLineEnd = text.IndexOf('\n');
            int secondLineEnd = text.IndexOf('\n', firstLineEnd + 1);
            Assert.That(firstLineEnd, Is.GreaterThanOrEqualTo(0));
            Assert.That(secondLineEnd, Is.GreaterThan(firstLineEnd));
            Assert.That(
                text.Substring(0, firstLineEnd).TrimEnd('\r'),
                Is.EqualTo($"# parent_projection_sha256={OPointAuthorityParentSha256}"));
            Assert.That(
                text.Substring(firstLineEnd + 1, secondLineEnd - firstLineEnd - 1)
                    .TrimEnd('\r'),
                Is.EqualTo(
                    $"# opoint_subset_payload_sha256={OPointAuthorityPayloadSha256}"));

            string payload = text.Substring(secondLineEnd + 1);
            Assert.That(
                GetSha256Hex(Encoding.UTF8.GetBytes(payload)),
                Is.EqualTo(OPointAuthorityPayloadSha256),
                "Appendix-A OPoint fixture payload hash changed.");

            var result = new List<OPointExpectation>(36);
            string[] lines = payload.Split('\n');
            for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex].TrimEnd('\r');
                if (line.Length == 0)
                    continue;

                string[] fields = line.Split('\t');
                Assert.That(fields.Length, Is.EqualTo(13),
                    $"Malformed OPoint authority row {lineIndex + 3}.");
                Assert.That(fields[0], Is.EqualTo("OPOINT"));
                Assert.That(fields[4], Is.EqualTo("1"));
                Assert.That(fields[5], Is.EqualTo("1"));
                Assert.That(fields[7], Is.EqualTo("1"));
                Assert.That(fields[8], Is.EqualTo("0"));
                Assert.That(
                    fields[9],
                    Is.EqualTo("kind,x,y,action,dvx,dvy,oid,facing"));

                string clientPath = FindUniqueClientDatPath(
                    Path.GetFileName(fields[2]));
                var row = E(
                    int.Parse(fields[1]),
                    clientPath,
                    int.Parse(fields[3]));
                result.Add(new OPointExpectation(
                    row,
                    int.Parse(fields[6]),
                    fields[11]));
            }

            Assert.That(result.Count, Is.EqualTo(36));
            return result.ToArray();
        }

        private static WPointExpectation[] LoadWPointExpectations()
        {
            string absolutePath = ResolveProjectPath(WPointAuthorityRelativePath);
            byte[] bytes = File.ReadAllBytes(absolutePath);
            Assert.That(GetSha256Hex(bytes), Is.EqualTo(WPointAuthorityFileSha256),
                "Appendix-A WPoint fixture file hash changed.");

            string text = StrictUtf8.GetString(bytes);
            int firstLineEnd = text.IndexOf('\n');
            int secondLineEnd = text.IndexOf('\n', firstLineEnd + 1);
            Assert.That(firstLineEnd, Is.GreaterThanOrEqualTo(0));
            Assert.That(secondLineEnd, Is.GreaterThan(firstLineEnd));
            Assert.That(
                text.Substring(0, firstLineEnd).TrimEnd('\r'),
                Is.EqualTo($"# parent_projection_sha256={WPointAuthorityParentSha256}"));
            Assert.That(
                text.Substring(firstLineEnd + 1, secondLineEnd - firstLineEnd - 1)
                    .TrimEnd('\r'),
                Is.EqualTo(
                    $"# wpoint_subset_payload_sha256={WPointAuthorityPayloadSha256}"));

            string payload = text.Substring(secondLineEnd + 1);
            Assert.That(
                GetSha256Hex(Encoding.UTF8.GetBytes(payload)),
                Is.EqualTo(WPointAuthorityPayloadSha256),
                "Appendix-A WPoint fixture payload hash changed.");

            var result = new List<WPointExpectation>(36);
            string[] lines = payload.Split('\n');
            for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex].TrimEnd('\r');
                if (line.Length == 0)
                    continue;

                string[] fields = line.Split('\t');
                Assert.That(fields.Length, Is.EqualTo(13),
                    $"Malformed WPoint authority row {lineIndex + 3}.");
                Assert.That(fields[0], Is.EqualTo("WPOINT"));
                Assert.That(fields[4], Is.EqualTo("1"));
                Assert.That(fields[5], Is.EqualTo("1"));
                Assert.That(fields[7], Is.EqualTo("1"));
                Assert.That(fields[8], Is.EqualTo("0"));
                Assert.That(
                    fields[9],
                    Is.EqualTo("kind,x,y,attacking,cover,weaponact,dvx,dvy,dvz"));

                string clientPath = FindUniqueClientDatPath(
                    Path.GetFileName(fields[2]));
                var row = E(
                    int.Parse(fields[1]),
                    clientPath,
                    int.Parse(fields[3]));
                result.Add(new WPointExpectation(
                    row,
                    int.Parse(fields[6]),
                    fields[11]));
            }

            Assert.That(result.Count, Is.EqualTo(36));
            return result.ToArray();
        }

        private static string FindUniqueClientDatPath(string fileName)
        {
            string configRoot = ResolveProjectPath("Assets/NTSD/Config");
            string[] matches = Directory.GetFiles(
                    configRoot,
                    fileName,
                    SearchOption.AllDirectories)
                .Where(path => string.Equals(
                    Path.GetFileName(path),
                    fileName,
                    StringComparison.OrdinalIgnoreCase))
                .ToArray();
            Assert.That(matches.Length, Is.EqualTo(1),
                $"Expected one Client DAT named {fileName}.");
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            Assert.That(projectRoot, Is.Not.Null.And.Not.Empty);
            return matches[0]
                .Substring(projectRoot.Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Replace(Path.DirectorySeparatorChar, '/');
        }

        private static void AssertOPointResourceManifest()
        {
            string[] rows = OPointA33ResourceIdentities
                .OrderBy(value => value.RelativePath, StringComparer.Ordinal)
                .Select(value =>
                {
                    string actual = GetSha256Hex(
                        File.ReadAllBytes(ResolveProjectPath(value.RelativePath)));
                    Assert.That(actual, Is.EqualTo(value.BeforeSha256),
                        $"OPoint A33 baseline drifted: {value.RelativePath}");
                    return $"{value.RelativePath}|{actual}";
                })
                .ToArray();
            string manifest = string.Join("\n", rows) + "\n";
            Assert.That(
                GetSha256Hex(Encoding.UTF8.GetBytes(manifest)),
                Is.EqualTo(OPointResourceManifestSha256));
        }

        private static void AssertWPointResourceManifest()
        {
            string[] rows = WPointA37ResourceIdentities
                .Select(value =>
                {
                    string actual = GetSha256Hex(
                        File.ReadAllBytes(ResolveProjectPath(value.RelativePath)));
                    Assert.That(actual, Is.EqualTo(value.BeforeSha256),
                        $"WPoint A37 baseline drifted: {value.RelativePath}");
                    return $"{value.RelativePath}|{actual}";
                })
                .ToArray();
            string manifest = string.Join("\n", rows) + "\n";
            Assert.That(
                GetSha256Hex(Encoding.UTF8.GetBytes(manifest)),
                Is.EqualTo(WPointResourceManifestSha256));
        }

        private static string PatchOPointExpectation(
            string text,
            OPointExpectation expectation,
            ref int changedTokens)
        {
            MatchCollection frames = Regex.Matches(
                text,
                @"(?ms)<frame>\s+" + expectation.Row.FrameId + @"\b.*?<frame_end>");
            Assert.That(frames.Count, Is.GreaterThanOrEqualTo(1), expectation.ToString());
            Match frame = frames[frames.Count - 1];
            MatchCollection points = Regex.Matches(
                frame.Value,
                @"(?ms)(?<![A-Za-z0-9_])opoint\s*:\s*.*?" +
                @"(?<![A-Za-z0-9_])opoint_end\s*:");
            Assert.That(points.Count, Is.EqualTo(1), expectation.ToString());

            string patchedPoint = points[0].Value;
            Dictionary<string, string> expected =
                ParseEvaluationFields(expectation.ReleaseExpected);
            foreach (KeyValuePair<string, string> field in expected)
            {
                string pattern =
                    @"(?<![A-Za-z0-9_])" + Regex.Escape(field.Key) +
                    @":[ \t]+(?<value>-?\d+)(?![A-Za-z0-9_])";
                MatchCollection values = Regex.Matches(patchedPoint, pattern);
                if (values.Count == 0)
                {
                    Assert.That(field.Value, Is.EqualTo("0"),
                        $"Missing non-default OPoint token {field.Key}: {expectation}");
                    continue;
                }

                Assert.That(values.Count, Is.EqualTo(1),
                    $"Ambiguous OPoint token {field.Key}: {expectation}");
                int before = int.Parse(values[0].Groups["value"].Value);
                int after = int.Parse(field.Value);
                if (before == after)
                    continue;

                patchedPoint = ReplaceBodyValue(
                    patchedPoint,
                    field.Key,
                    before,
                    after);
                changedTokens++;
            }

            string patchedFrame = frame.Value.Remove(points[0].Index, points[0].Length)
                .Insert(points[0].Index, patchedPoint);
            return text.Remove(frame.Index, frame.Length)
                .Insert(frame.Index, patchedFrame);
        }

        private static string PatchWPointExpectation(
            string text,
            WPointExpectation expectation,
            ref int changedTokens)
        {
            MatchCollection frames = Regex.Matches(
                text,
                @"(?ms)<frame>\s+" + expectation.Row.FrameId + @"\b.*?<frame_end>");
            Assert.That(frames.Count, Is.GreaterThanOrEqualTo(1), expectation.ToString());
            Match frame = frames[frames.Count - 1];
            MatchCollection points = Regex.Matches(
                frame.Value,
                @"(?ms)(?<![A-Za-z0-9_])wpoint\s*:\s*.*?" +
                @"(?<![A-Za-z0-9_])wpoint_end\s*:");
            Assert.That(points.Count, Is.EqualTo(1), expectation.ToString());

            string patchedPoint = points[0].Value;
            Dictionary<string, string> expected =
                ParseEvaluationFields(expectation.ReleaseExpected);
            foreach (KeyValuePair<string, string> field in expected)
            {
                string pattern =
                    @"(?<![A-Za-z0-9_])" + Regex.Escape(field.Key) +
                    @":[ \t]+(?<value>-?\d+)(?![A-Za-z0-9_])";
                MatchCollection values = Regex.Matches(patchedPoint, pattern);
                if (values.Count == 0)
                {
                    Assert.That(field.Value, Is.EqualTo("0"),
                        $"Missing non-default WPoint token {field.Key}: {expectation}");
                    continue;
                }

                Assert.That(values.Count, Is.EqualTo(1),
                    $"Ambiguous WPoint token {field.Key}: {expectation}");
                int before = int.Parse(values[0].Groups["value"].Value);
                int after = int.Parse(field.Value);
                if (before == after)
                    continue;

                patchedPoint = ReplaceBodyValue(
                    patchedPoint,
                    field.Key,
                    before,
                    after);
                changedTokens++;
            }

            string patchedFrame = frame.Value.Remove(points[0].Index, points[0].Length)
                .Insert(points[0].Index, patchedPoint);
            return text.Remove(frame.Index, frame.Length)
                .Insert(frame.Index, patchedFrame);
        }

        private static void AssertExactOPointCatalogDiff(
            string baselineText,
            string candidateText,
            IReadOnlyList<OPointExpectation> authority)
        {
            Dictionary<string, string> baseline = ParseProjection(baselineText);
            Dictionary<string, string> candidate = ParseProjection(candidateText);
            var expectedChanges = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (OPointExpectation expectation in authority)
            {
                string relativePath = ToConfigRelativePath(expectation.Row.RelativePath);
                string countKey = FindLastProjectionKey(
                    baseline.Keys,
                    relativePath,
                    expectation.Row.FrameId,
                    "OPOINT",
                    -1,
                    "count");
                if (!string.Equals(baseline[countKey], "1", StringComparison.Ordinal))
                    expectedChanges.Add(countKey, "1");

                string[] countParts = countKey.Split('\t');
                string tupleKey = string.Join("\t", new[]
                {
                    countParts[0],
                    countParts[1],
                    countParts[2],
                    "OPOINT",
                    "0",
                    "tuple",
                });
                if (!baseline.TryGetValue(tupleKey, out string beforeTuple) ||
                    !string.Equals(
                        beforeTuple,
                        expectation.ReleaseExpected,
                        StringComparison.Ordinal))
                {
                    expectedChanges.Add(tupleKey, expectation.ReleaseExpected);
                }
            }

            Assert.That(expectedChanges.Count, Is.EqualTo(33));
            Assert.That(
                expectedChanges.Keys.Count(value =>
                    value.EndsWith("\tcount", StringComparison.Ordinal)),
                Is.EqualTo(1));
            Assert.That(
                expectedChanges.Keys.Count(value =>
                    value.EndsWith("\ttuple", StringComparison.Ordinal)),
                Is.EqualTo(32));

            string[] changedKeys = baseline.Keys
                .Union(candidate.Keys, StringComparer.Ordinal)
                .Where(key =>
                    !baseline.TryGetValue(key, out string before) ||
                    !candidate.TryGetValue(key, out string after) ||
                    !string.Equals(before, after, StringComparison.Ordinal))
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();
            string[] expectedKeys = expectedChanges.Keys
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();
            Assert.That(
                changedKeys,
                Is.EqualTo(expectedKeys),
                "OPoint candidate contains a non-A33/B1 projection difference. " +
                $"Unexpected=[{string.Join(" | ", changedKeys.Except(expectedKeys))}]; " +
                $"Missing=[{string.Join(" | ", expectedKeys.Except(changedKeys))}].");

            foreach (KeyValuePair<string, string> expected in expectedChanges)
            {
                Assert.That(candidate.ContainsKey(expected.Key), Is.True, expected.Key);
                Assert.That(candidate[expected.Key], Is.EqualTo(expected.Value), expected.Key);
            }
        }

        private static void AssertExactWPointCatalogDiff(
            string baselineText,
            string candidateText,
            IReadOnlyList<WPointExpectation> authority)
        {
            Dictionary<string, string> baseline = ParseProjection(baselineText);
            Dictionary<string, string> candidate = ParseProjection(candidateText);
            var expectedChanges = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (WPointExpectation expectation in authority)
            {
                string relativePath = ToConfigRelativePath(expectation.Row.RelativePath);
                string countKey = FindLastProjectionKey(
                    baseline.Keys,
                    relativePath,
                    expectation.Row.FrameId,
                    "WPOINT",
                    -1,
                    "count");
                if (!string.Equals(baseline[countKey], "1", StringComparison.Ordinal))
                    expectedChanges.Add(countKey, "1");

                string[] countParts = countKey.Split('\t');
                string tupleKey = string.Join("\t", new[]
                {
                    countParts[0],
                    countParts[1],
                    countParts[2],
                    "WPOINT",
                    "0",
                    "tuple",
                });
                if (!baseline.TryGetValue(tupleKey, out string beforeTuple) ||
                    !string.Equals(
                        beforeTuple,
                        expectation.ReleaseExpected,
                        StringComparison.Ordinal))
                {
                    expectedChanges.Add(tupleKey, expectation.ReleaseExpected);
                }
            }

            Assert.That(expectedChanges.Count, Is.EqualTo(37));
            Assert.That(
                expectedChanges.Keys.Count(value =>
                    value.EndsWith("\tcount", StringComparison.Ordinal)),
                Is.EqualTo(1));
            Assert.That(
                expectedChanges.Keys.Count(value =>
                    value.EndsWith("\ttuple", StringComparison.Ordinal)),
                Is.EqualTo(36));

            string[] changedKeys = baseline.Keys
                .Union(candidate.Keys, StringComparer.Ordinal)
                .Where(key =>
                    !baseline.TryGetValue(key, out string before) ||
                    !candidate.TryGetValue(key, out string after) ||
                    !string.Equals(before, after, StringComparison.Ordinal))
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();
            string[] expectedKeys = expectedChanges.Keys
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();
            Assert.That(
                changedKeys,
                Is.EqualTo(expectedKeys),
                "WPoint candidate contains a non-A37/B1 projection difference. " +
                $"Unexpected=[{string.Join(" | ", changedKeys.Except(expectedKeys))}]; " +
                $"Missing=[{string.Join(" | ", expectedKeys.Except(changedKeys))}].");

            foreach (KeyValuePair<string, string> expected in expectedChanges)
            {
                Assert.That(candidate.ContainsKey(expected.Key), Is.True, expected.Key);
                Assert.That(candidate[expected.Key], Is.EqualTo(expected.Value), expected.Key);
            }
        }

        private static void AssertKisame292WPointUnchanged(
            string baselineText,
            string candidateText)
        {
            Dictionary<string, string> baseline = ParseProjection(baselineText);
            Dictionary<string, string> candidate = ParseProjection(candidateText);
            string countKey = FindLastProjectionKey(
                baseline.Keys,
                "Character/kisame.dat",
                292,
                "WPOINT",
                -1,
                "count");
            string[] parts = countKey.Split('\t');
            string prefix = string.Join("\t", new[]
            {
                parts[0], parts[1], parts[2], "WPOINT",
            }) + "\t";
            string[] keys = baseline.Keys
                .Where(value => value.StartsWith(prefix, StringComparison.Ordinal))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Assert.That(keys.Length, Is.GreaterThanOrEqualTo(1));
            foreach (string key in keys)
            {
                Assert.That(candidate.ContainsKey(key), Is.True, key);
                Assert.That(candidate[key], Is.EqualTo(baseline[key]), key);
            }
        }

        private static string FindLastProjectionKey(
            IEnumerable<string> keys,
            string relativePath,
            int frameId,
            string domain,
            int child,
            string property)
        {
            string[] matches = keys.Where(key =>
            {
                string[] parts = key.Split('\t');
                return parts.Length == 6 &&
                       string.Equals(parts[0], relativePath, StringComparison.Ordinal) &&
                       string.Equals(parts[2], frameId.ToString(), StringComparison.Ordinal) &&
                       string.Equals(parts[3], domain, StringComparison.Ordinal) &&
                       string.Equals(parts[4], child.ToString(), StringComparison.Ordinal) &&
                       string.Equals(parts[5], property, StringComparison.Ordinal);
            }).OrderBy(key => int.Parse(key.Split('\t')[1])).ToArray();
            Assert.That(matches.Length, Is.GreaterThanOrEqualTo(1),
                $"Projection key missing for {relativePath} Frame{frameId} " +
                $"{domain}[{child}].{property}");
            return matches[matches.Length - 1];
        }

        private static BdyExpectation[] LoadBdyExpectations()
        {
            string absolutePath = ResolveProjectPath(BdyAuthorityRelativePath);
            byte[] bytes = File.ReadAllBytes(absolutePath);
            Assert.That(GetSha256Hex(bytes), Is.EqualTo(BdyAuthorityFileSha256),
                "Appendix-A Bdy fixture file hash changed.");

            string text = new UTF8Encoding(false, true).GetString(bytes);
            int firstLineEnd = text.IndexOf('\n');
            int secondLineEnd = text.IndexOf('\n', firstLineEnd + 1);
            Assert.That(firstLineEnd, Is.GreaterThanOrEqualTo(0));
            Assert.That(secondLineEnd, Is.GreaterThan(firstLineEnd));
            Assert.That(
                text.Substring(0, firstLineEnd).TrimEnd('\r'),
                Is.EqualTo($"# parent_projection_sha256={BdyAuthorityParentSha256}"));
            Assert.That(
                text.Substring(firstLineEnd + 1, secondLineEnd - firstLineEnd - 1)
                    .TrimEnd('\r'),
                Is.EqualTo($"# bdy_subset_payload_sha256={BdyAuthorityPayloadSha256}"));

            string payload = text.Substring(secondLineEnd + 1);
            Assert.That(
                GetSha256Hex(Encoding.UTF8.GetBytes(payload)),
                Is.EqualTo(BdyAuthorityPayloadSha256),
                "Appendix-A Bdy fixture payload hash changed.");

            Dictionary<string, ScalarExpectation> rowByKey = BdyRows()
                .ToDictionary(
                    row => $"{row.ObjectId}:{row.FrameId}",
                    StringComparer.Ordinal);
            var builders = new Dictionary<string, BdyExpectationBuilder>(
                StringComparer.Ordinal);
            int dataRowCount = 0;
            string[] lines = payload.Split('\n');
            for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex].TrimEnd('\r');
                if (line.Length == 0)
                    continue;

                string[] fields = line.Split('\t');
                Assert.That(fields.Length, Is.EqualTo(13),
                    $"Malformed Bdy authority row {lineIndex + 3}.");
                Assert.That(fields[0], Is.EqualTo("BDY"));
                string key = $"{fields[1]}:{fields[3]}";
                Assert.That(rowByKey.ContainsKey(key), Is.True,
                    $"Unexpected Bdy authority key {key}.");

                if (!builders.TryGetValue(key, out BdyExpectationBuilder builder))
                {
                    builder = new BdyExpectationBuilder(
                        rowByKey[key],
                        int.Parse(fields[7]));
                    builders.Add(key, builder);
                }

                Assert.That(int.Parse(fields[7]), Is.EqualTo(builder.ExpectedCount));
                builder.Add(int.Parse(fields[8]), fields[11]);
                dataRowCount++;
            }

            Assert.That(dataRowCount, Is.EqualTo(82));
            Assert.That(builders.Count, Is.EqualTo(55));
            return BdyRows()
                .Select(row => builders[$"{row.ObjectId}:{row.FrameId}"].Build())
                .ToArray();
        }

        private static void AssertExactB59CatalogDiff(
            string baselineText,
            string candidateText,
            IReadOnlyList<BdyExpectation> authority)
        {
            Dictionary<string, string> baseline = ParseProjection(baselineText);
            Dictionary<string, string> candidate = ParseProjection(candidateText);
            var expectedChanges = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (BdyExpectation expectation in authority)
            {
                if (expectation.Row.ObjectId == 51)
                    continue;

                string configRelativePath = ToConfigRelativePath(
                    expectation.Row.RelativePath);
                for (int child = 0; child < expectation.ReleaseExpected.Length; child++)
                {
                    Dictionary<string, string> expectedFields =
                        ParseEvaluationFields(expectation.ReleaseExpected[child]);
                    foreach (string field in new[] { "x", "y", "w", "h" })
                    {
                        string projectionKey = FindProjectionKey(
                            baseline.Keys,
                            configRelativePath,
                            expectation.Row.FrameId,
                            "BDY",
                            child,
                            field);
                        string expectedValue = expectedFields[field];
                        if (!string.Equals(
                                baseline[projectionKey],
                                expectedValue,
                                StringComparison.Ordinal))
                        {
                            expectedChanges.Add(projectionKey, expectedValue);
                        }
                    }
                }
            }

            Assert.That(expectedChanges.Count, Is.EqualTo(59));
            Assert.That(
                expectedChanges.Keys.Count(key => key.EndsWith("\th", StringComparison.Ordinal)),
                Is.EqualTo(49));
            Assert.That(
                expectedChanges.Keys.Count(key => key.EndsWith("\tx", StringComparison.Ordinal)),
                Is.EqualTo(5));
            Assert.That(
                expectedChanges.Keys.Count(key => key.EndsWith("\ty", StringComparison.Ordinal)),
                Is.EqualTo(5));

            string[] changedKeys = baseline.Keys
                .Union(candidate.Keys, StringComparer.Ordinal)
                .Where(key =>
                    !baseline.TryGetValue(key, out string before) ||
                    !candidate.TryGetValue(key, out string after) ||
                    !string.Equals(before, after, StringComparison.Ordinal))
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();
            string[] expectedKeys = expectedChanges.Keys
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();
            string[] unexpectedKeys = changedKeys
                .Except(expectedKeys, StringComparer.Ordinal)
                .ToArray();
            string[] missingKeys = expectedKeys
                .Except(changedKeys, StringComparer.Ordinal)
                .ToArray();
            Assert.That(changedKeys, Is.EqualTo(expectedKeys),
                "Full-catalog candidate contains a non-B59 projection difference. " +
                $"Unexpected=[{string.Join(" | ", unexpectedKeys)}]; " +
                $"Missing=[{string.Join(" | ", missingKeys)}].");

            foreach (KeyValuePair<string, string> expected in expectedChanges)
            {
                Assert.That(candidate.ContainsKey(expected.Key), Is.True);
                Assert.That(candidate[expected.Key], Is.EqualTo(expected.Value),
                    expected.Key);
            }
        }

        private static Dictionary<string, string> ParseProjection(string text)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string rawLine in text.Split('\n'))
            {
                string line = rawLine.TrimEnd('\r');
                if (line.Length == 0)
                    continue;
                int separator = line.LastIndexOf('\t');
                Assert.That(separator, Is.GreaterThan(0));
                string key = line.Substring(0, separator);
                string value = line.Substring(separator + 1);
                Assert.That(result.ContainsKey(key), Is.False,
                    $"Duplicate projection key {key}.");
                result.Add(key, value);
            }
            return result;
        }

        private static string FindProjectionKey(
            IEnumerable<string> keys,
            string relativePath,
            int frameId,
            string domain,
            int child,
            string property)
        {
            string[] matches = keys.Where(key =>
            {
                string[] parts = key.Split('\t');
                return parts.Length == 6 &&
                       string.Equals(parts[0], relativePath, StringComparison.Ordinal) &&
                       string.Equals(parts[2], frameId.ToString(), StringComparison.Ordinal) &&
                       string.Equals(parts[3], domain, StringComparison.Ordinal) &&
                       string.Equals(parts[4], child.ToString(), StringComparison.Ordinal) &&
                       string.Equals(parts[5], property, StringComparison.Ordinal);
            }).ToArray();
            Assert.That(matches.Length, Is.EqualTo(1),
                $"Projection key must be unique for {relativePath} Frame{frameId} " +
                $"{domain}[{child}].{property}");
            return matches[0];
        }

        private static Dictionary<string, string> ParseEvaluationFields(string text)
        {
            return text.Split(';')
                .Select(part => part.Split(new[] { '=' }, 2))
                .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.Ordinal);
        }

        private static string ToConfigRelativePath(string projectRelativePath)
        {
            const string prefix = "Assets/NTSD/Config/";
            Assert.That(projectRelativePath.StartsWith(prefix, StringComparison.Ordinal), Is.True);
            return projectRelativePath.Substring(prefix.Length);
        }

        private static string GetSha256Hex(byte[] bytes)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(bytes))
                    .Replace("-", string.Empty)
                    .ToLowerInvariant();
            }
        }

        private static IEnumerable<TestCaseData> FrameScalarCases()
        {
            yield return Case(E(506, Effect("heart0.dat"), 399,
                state: 3005, next: 1000));
            yield return Case(E(204, Special("wind.dat"), 207, next: 208));
            yield return Case(E(204, Special("wind.dat"), 211, next: 312));
            yield return Case(E(206, Special("clay_bird2.dat"), 134, dvx: 15));
            yield return Case(E(416, Special("water.dat"), 45, centerX: 60));
            yield return Case(E(416, Special("water.dat"), 46, centerX: 60));
            yield return Case(E(446, Special("4TK_ball.dat"), 211, next: 213));
            yield return Case(E(446, Special("4TK_ball.dat"), 213, pic: 67));
            yield return Case(E(446, Special("4TK_ball.dat"), 214,
                pic: 68, next: 219));
            yield return Case(E(453, Special("earth_creature.dat"), 68,
                pic: 3, next: 69));
            yield return Case(E(462, Special("shadow.dat"), 211, next: 213));
            yield return Case(E(462, Special("shadow.dat"), 213,
                pic: 32, wait: 1, next: 214));
            yield return Case(E(514, Special("katon_kakuzu.dat"), 13, pic: 26));
            yield return Case(E(51, Character("sasori.dat"), 45,
                pic: 29, wait: 15, next: 999, centerX: 19, centerY: 91));
            yield return Case(E(51, Character("sasori.dat"), 390, next: 3910));
            yield return Case(E(33, Character("naruto_clone.dat"), 243,
                pic: 37, next: 235, dvx: 0, dvy: 0));
            yield return Case(E(2, Character("naruto.dat"), 123,
                dvy: 550, hitD: 370));
            yield return Case(E(13, Character("yamato.dat"), 102, centerY: 790));
            yield return Case(E(7, Character("rock_lee.dat"), 299, next: 294));
            yield return Case(E(7, Character("rock_lee.dat"), 332, dvy: 550));
            yield return Case(E(7, Character("rock_lee.dat"), 334, dvy: 550));
            yield return Case(E(7, Character("rock_lee.dat"), 336, dvy: 550));
            yield return Case(E(7, Character("rock_lee.dat"), 338, dvy: 550));
            yield return Case(E(7, Character("rock_lee.dat"), 340, dvy: 550));
            yield return Case(E(7, Character("rock_lee.dat"), 342, dvy: 550));
            yield return Case(E(7, Character("rock_lee.dat"), 344, dvy: 550));
            yield return Case(E(18, Character("neji.dat"), 102, centerY: 790));
            yield return Case(E(22, Character("shikamaru.dat"), 102, centerY: 790));
            yield return Case(E(14, Character("kankuro.dat"), 102, centerY: 790));
            yield return Case(E(15, Character("temari.dat"), 102, centerY: 790));
            yield return Case(E(15, Character("temari.dat"), 318,
                pic: 122, next: 349));
            yield return Case(E(10, Character("deidara.dat"), 7, hitDj: 0));
            yield return Case(E(10, Character("deidara.dat"), 8, hitDj: 0));
            yield return Case(E(122, "Assets/NTSD/Config/chars/weapon3.dat", 399,
                state: 3005, next: 1000, dvx: 550, dvy: 550));
            yield return Case(E(123, "Assets/NTSD/Config/FrameConfig/weapon8.dat", 399,
                next: 1000, dvx: 550, dvy: 550));
            yield return Case(E(500, Effect("heart.dat"), 399,
                state: 3005, next: 1000));
            yield return Case(E(501, Effect("heart2.dat"), 399,
                state: 3005, next: 1000));
            yield return Case(E(502, Effect("heart3.dat"), 399,
                state: 3005, next: 1000));
        }

        private static IEnumerable<ScalarExpectation> ItrRows()
        {
            yield return E(33, Character("naruto_clone.dat"), 243);
            yield return E(51, Character("sasori.dat"), 45);
            yield return E(57, Character("nckakuzu.dat"), 180);
            yield return E(57, Character("nckakuzu.dat"), 186);
            yield return E(120, "Assets/NTSD/Config/chars/weapon4.dat", 48);
            yield return E(453, Special("earth_creature.dat"), 68);
            yield return E(500, Effect("heart.dat"), 48);
            yield return E(501, Effect("heart2.dat"), 48);
            yield return E(502, Effect("heart3.dat"), 48);
            yield return E(506, Effect("heart0.dat"), 48);
        }

        private static IEnumerable<ScalarExpectation> BdyRows()
        {
            yield return E(120, "Assets/NTSD/Config/chars/weapon4.dat", 48);
            yield return E(15, Character("temari.dat"), 220);
            yield return E(15, Character("temari.dat"), 221);
            yield return E(15, Character("temari.dat"), 222);
            yield return E(15, Character("temari.dat"), 223);
            yield return E(15, Character("temari.dat"), 224);
            yield return E(15, Character("temari.dat"), 225);
            yield return E(15, Character("temari.dat"), 226);
            yield return E(15, Character("temari.dat"), 227);
            yield return E(15, Character("temari.dat"), 228);
            yield return E(15, Character("temari.dat"), 229);
            yield return E(15, Character("temari.dat"), 230);
            yield return E(15, Character("temari.dat"), 231);
            yield return E(15, Character("temari.dat"), 331);
            yield return E(15, Character("temari.dat"), 332);
            yield return E(15, Character("temari.dat"), 333);
            yield return E(17, Character("kisame.dat"), 340);
            yield return E(205, Special("poison.dat"), 10);
            yield return E(205, Special("poison.dat"), 391);
            yield return E(205, Special("poison.dat"), 392);
            yield return E(205, Special("poison.dat"), 393);
            yield return E(205, Special("poison.dat"), 394);
            yield return E(205, Special("poison.dat"), 68);
            yield return E(205, Special("poison.dat"), 69);
            yield return E(205, Special("poison.dat"), 70);
            yield return E(205, Special("poison.dat"), 71);
            yield return E(205, Special("poison.dat"), 72);
            yield return E(205, Special("poison.dat"), 73);
            yield return E(209, Special("area.dat"), 100);
            yield return E(213, Special("puppet.dat"), 28);
            yield return E(220, Special("crow_2.dat"), 215);
            yield return E(300, "Assets/NTSD/Config/chars/criminal.dat", 111);
            yield return E(417, Special("tree.dat"), 10);
            yield return E(417, Special("tree.dat"), 20);
            yield return E(417, Special("tree.dat"), 30);
            yield return E(417, Special("tree.dat"), 68);
            yield return E(417, Special("tree.dat"), 69);
            yield return E(417, Special("tree.dat"), 70);
            yield return E(417, Special("tree.dat"), 71);
            yield return E(441, Special("wood.dat"), 10);
            yield return E(441, Special("wood.dat"), 111);
            yield return E(441, Special("wood.dat"), 112);
            yield return E(441, Special("wood.dat"), 113);
            yield return E(441, Special("wood.dat"), 114);
            yield return E(441, Special("wood.dat"), 20);
            yield return E(441, Special("wood.dat"), 30);
            yield return E(441, Special("wood.dat"), 31);
            yield return E(446, Special("4TK_ball.dat"), 217);
            yield return E(500, Effect("heart.dat"), 48);
            yield return E(501, Effect("heart2.dat"), 48);
            yield return E(502, Effect("heart3.dat"), 48);
            yield return E(506, Effect("heart0.dat"), 48);
            yield return E(51, Character("sasori.dat"), 45);
            yield return E(56, Character("reaper.dat"), 389);
            yield return E(9, Character("itachi.dat"), 301);
        }

        private static IEnumerable<ItrExpectation> ItrExpectations()
        {
            const string weaponBefore =
                "kind=0;x=6;y=16;w=39;h=19;dvx=8;dvy=0;fall=70;" +
                "bdefend=16;injury=55;arest=0;vrest=0;effect=1;attacking=0;" +
                "respond=0;pickingact=0;pickedact=0;throwvx=0;throwvy=0;" +
                "zwidth=15;throwvz=0;throwinjury=0";
            const string nckBefore =
                "kind=4;x=22;y=13;w=35;h=29;dvx=2;dvy=0;fall=70;" +
                "bdefend=10;injury=30;arest=0;vrest=20;effect=0;attacking=0;" +
                "respond=0;pickingact=0;pickedact=0;throwvx=0;throwvy=0;" +
                "zwidth=15;throwvz=0;throwinjury=0";
            const string nckExpected =
                "kind=4;x=22;y=13;w=35;h=74;dvx=2;dvy=0;fall=70;" +
                "bdefend=10;injury=30;arest=0;vrest=20;effect=0;attacking=0;" +
                "respond=0;pickingact=0;pickedact=0;throwvx=0;throwvy=0;" +
                "zwidth=15;throwvz=0;throwinjury=0";
            const string earthBarrier =
                "kind=0;x=0;y=79900;w=100;h=800;dvx=0;dvy=0;fall=-1;" +
                "bdefend=0;injury=0;arest=100;vrest=0;effect=5;attacking=0;" +
                "respond=0;pickingact=0;pickedact=0;throwvx=0;throwvy=0;" +
                "zwidth=15;throwvz=0;throwinjury=0";
            const string earthKind8Before =
                "kind=8;x=0;y=-8000000;w=100;h=79;dvx=85;dvy=0;fall=0;" +
                "bdefend=0;injury=0;arest=0;vrest=0;effect=0;attacking=0;" +
                "respond=0;pickingact=0;pickedact=0;throwvx=0;throwvy=0;" +
                "zwidth=50;throwvz=0;throwinjury=0";
            const string earthKind8Expected =
                "kind=8;x=0;y=-10000000;w=100;h=79;dvx=85;dvy=0;fall=0;" +
                "bdefend=0;injury=0;arest=0;vrest=0;effect=0;attacking=0;" +
                "respond=0;pickingact=0;pickedact=0;throwvx=0;throwvy=0;" +
                "zwidth=50;throwvz=0;throwinjury=0";

            yield return I(
                E(33, Character("naruto_clone.dat"), 243),
                new[] {
                    "kind=0;x=20;y=33;w=50;h=20;dvx=10;dvy=-5;fall=70;" +
                    "bdefend=16;injury=35;arest=0;vrest=10;effect=0;attacking=0;" +
                    "respond=0;pickingact=0;pickedact=0;throwvx=0;throwvy=0;" +
                    "zwidth=15;throwvz=0;throwinjury=0"
                },
                Array.Empty<string>());
            yield return I(
                E(51, Character("sasori.dat"), 45),
                new[] {
                    "kind=0;x=35;y=-999;w=500;h=14;dvx=2;dvy=0;fall=0;" +
                    "bdefend=16;injury=1;arest=0;vrest=7;effect=5;attacking=0;" +
                    "respond=0;pickingact=0;pickedact=0;throwvx=0;throwvy=0;" +
                    "zwidth=15;throwvz=0;throwinjury=0"
                },
                Array.Empty<string>());
            yield return I(E(57, Character("nckakuzu.dat"), 180),
                new[] { nckBefore }, new[] { nckExpected });
            yield return I(E(57, Character("nckakuzu.dat"), 186),
                new[] { nckBefore }, new[] { nckExpected });
            yield return I(E(120, "Assets/NTSD/Config/chars/weapon4.dat", 48),
                new[] { weaponBefore }, Array.Empty<string>());
            yield return I(E(453, Special("earth_creature.dat"), 68),
                new[] { earthKind8Before, earthBarrier },
                new[] { earthBarrier, earthKind8Expected });
            yield return I(E(500, Effect("heart.dat"), 48),
                new[] { weaponBefore }, Array.Empty<string>());
            yield return I(E(501, Effect("heart2.dat"), 48),
                new[] { weaponBefore }, Array.Empty<string>());
            yield return I(E(502, Effect("heart3.dat"), 48),
                new[] { weaponBefore }, Array.Empty<string>());
            yield return I(E(506, Effect("heart0.dat"), 48),
                new[] { weaponBefore }, Array.Empty<string>());
        }

        private static ItrExpectation I(
            ScalarExpectation row,
            string[] clientBefore,
            string[] releaseExpected)
        {
            return new ItrExpectation(row, clientBefore, releaseExpected);
        }

        private static string FormatItrEvaluation(InteractionArea itr)
        {
            return
                $"kind={itr.kind};x={itr.x};y={itr.y};w={itr.w};h={itr.h};" +
                $"dvx={itr.dvx};dvy={itr.dvy};fall={itr.fall};" +
                $"bdefend={itr.bdefend};injury={itr.injury};" +
                $"arest={itr.arest};vrest={itr.vrest};effect={itr.effect};" +
                $"attacking={itr.attacking};respond={itr.respond};" +
                $"pickingact={itr.pickingact};pickedact={itr.pickedact};" +
                $"throwvx={itr.throwvx};throwvy={itr.throwvy};" +
                $"zwidth={itr.zwidth};throwvz={itr.throwvz};" +
                $"throwinjury={itr.throwinjury}";
        }

        private static string FormatItr(InteractionArea itr)
        {
            return
                $"kind={itr.kind};x={itr.x};y={itr.y};w={itr.w};h={itr.h};" +
                $"dvx={itr.dvx};dvy={itr.dvy};fall={itr.fall};" +
                $"bdefend={itr.bdefend};injury={itr.injury};" +
                $"arest={itr.arest};vrest={itr.vrest};effect={itr.effect};" +
                $"attacking={itr.attacking};" +
                $"catchingact={FormatPair(itr.catchingact)};" +
                $"catchingact2={FormatPair(itr.catchingact2)};" +
                $"caughtact={FormatPair(itr.caughtact)};" +
                $"caughtact2={FormatPair(itr.caughtact2)};" +
                $"respond={itr.respond};pickingact={itr.pickingact};" +
                $"pickedact={itr.pickedact};throwvx={itr.throwvx};" +
                $"throwvy={itr.throwvy};zwidth={itr.zwidth};" +
                $"throwvz={itr.throwvz};throwinjury={itr.throwinjury};" +
                $"unityExtra.dvz={itr.dvz};" +
                $"unityExtra.vaction={itr.vaction};unityExtra.kill={itr.kill}";
        }

        private static string FormatPair(int[] values)
        {
            return values == null ? "null" : string.Join(",", values);
        }

        private static void AssertExpected(
            int? expectedValue,
            int actualValue,
            ScalarExpectation expectation,
            string field)
        {
            if (!expectedValue.HasValue)
            {
                return;
            }

            Assert.That(actualValue, Is.EqualTo(expectedValue.Value),
                $"OID{expectation.ObjectId} Frame{expectation.FrameId} {field} " +
                $"from {expectation.RelativePath}");
        }

        private static void AppendDifference(
            List<string> differences,
            int? expectedValue,
            int actualValue,
            ScalarExpectation expectation,
            string field)
        {
            if (expectedValue.HasValue && actualValue != expectedValue.Value)
            {
                differences.Add(
                    $"OID{expectation.ObjectId} Frame{expectation.FrameId} {field}: " +
                    $"expected {expectedValue.Value}, actual {actualValue}, " +
                    $"path {expectation.RelativePath}");
            }
        }

        private static TestCaseData Case(ScalarExpectation expectation)
        {
            return new TestCaseData(expectation)
                .SetName($"FrameScalar_OID{expectation.ObjectId}_Frame{expectation.FrameId}");
        }

        private static ScalarExpectation E(
            int objectId,
            string relativePath,
            int frameId,
            int? pic = null,
            int? state = null,
            int? wait = null,
            int? next = null,
            int? dvx = null,
            int? dvy = null,
            int? centerX = null,
            int? centerY = null,
            int? hitD = null,
            int? hitDj = null)
        {
            return new ScalarExpectation(
                objectId,
                relativePath,
                frameId,
                pic,
                state,
                wait,
                next,
                dvx,
                dvy,
                centerX,
                centerY,
                hitD,
                hitDj);
        }

        private static string Character(string file) =>
            $"Assets/NTSD/Config/Character/{file}";

        private static string Effect(string file) =>
            $"Assets/NTSD/Config/effect/{file}";

        private static string Special(string file) =>
            $"Assets/NTSD/Config/specialattack/{file}";

        private sealed class ResourceIdentity
        {
            internal ResourceIdentity(string relativePath, string beforeSha256)
            {
                RelativePath = relativePath;
                BeforeSha256 = beforeSha256;
            }

            internal string RelativePath { get; }
            internal string BeforeSha256 { get; }
        }

        private sealed class ResourceWritePlan
        {
            internal ResourceWritePlan(
                ResourceIdentity identity,
                string absolutePath,
                byte[] patchedBytes)
            {
                Identity = identity;
                AbsolutePath = absolutePath;
                PatchedBytes = patchedBytes;
            }

            internal ResourceIdentity Identity { get; }
            internal string AbsolutePath { get; }
            internal byte[] PatchedBytes { get; }
        }

        private sealed class DecodedResource
        {
            internal DecodedResource(string text, bool isPlaintext, int prefixLength)
            {
                Text = text;
                IsPlaintext = isPlaintext;
                PrefixLength = prefixLength;
            }

            internal string Text { get; }
            internal bool IsPlaintext { get; }
            internal int PrefixLength { get; }
        }

        public sealed class ScalarExpectation
        {
            public ScalarExpectation(
                int objectId,
                string relativePath,
                int frameId,
                int? pic,
                int? state,
                int? wait,
                int? next,
                int? dvx,
                int? dvy,
                int? centerX,
                int? centerY,
                int? hitD,
                int? hitDj)
            {
                ObjectId = objectId;
                RelativePath = relativePath;
                FrameId = frameId;
                Pic = pic;
                State = state;
                Wait = wait;
                Next = next;
                Dvx = dvx;
                Dvy = dvy;
                CenterX = centerX;
                CenterY = centerY;
                HitD = hitD;
                HitDj = hitDj;
            }

            public int ObjectId { get; }
            public string RelativePath { get; }
            public int FrameId { get; }
            public int? Pic { get; }
            public int? State { get; }
            public int? Wait { get; }
            public int? Next { get; }
            public int? Dvx { get; }
            public int? Dvy { get; }
            public int? CenterX { get; }
            public int? CenterY { get; }
            public int? HitD { get; }
            public int? HitDj { get; }

            public override string ToString() =>
                $"OID{ObjectId}:Frame{FrameId}";
        }

        public sealed class BdyExpectation
        {
            internal BdyExpectation(
                ScalarExpectation row,
                string[] releaseExpected)
            {
                Row = row;
                ReleaseExpected = releaseExpected;
            }

            public ScalarExpectation Row { get; }
            public string[] ReleaseExpected { get; }

            public override string ToString() => Row.ToString();
        }

        public sealed class OPointExpectation
        {
            internal OPointExpectation(
                ScalarExpectation row,
                int selectedSourceIndex,
                string releaseExpected)
            {
                Row = row;
                SelectedSourceIndex = selectedSourceIndex;
                ReleaseExpected = releaseExpected;
            }

            public ScalarExpectation Row { get; }
            public int SelectedSourceIndex { get; }
            public string ReleaseExpected { get; }

            public override string ToString() => Row.ToString();
        }

        public sealed class WPointExpectation
        {
            internal WPointExpectation(
                ScalarExpectation row,
                int selectedSourceIndex,
                string releaseExpected)
            {
                Row = row;
                SelectedSourceIndex = selectedSourceIndex;
                ReleaseExpected = releaseExpected;
            }

            public ScalarExpectation Row { get; }
            public int SelectedSourceIndex { get; }
            public string ReleaseExpected { get; }

            public override string ToString() => Row.ToString();
        }

        private sealed class BdyExpectationBuilder
        {
            private readonly ScalarExpectation row;
            private readonly SortedDictionary<int, string> expected =
                new SortedDictionary<int, string>();

            internal BdyExpectationBuilder(
                ScalarExpectation row,
                int expectedCount)
            {
                this.row = row;
                ExpectedCount = expectedCount;
            }

            internal int ExpectedCount { get; }

            internal void Add(int ordinal, string value)
            {
                Assert.That(expected.ContainsKey(ordinal), Is.False,
                    $"Duplicate Bdy ordinal {ordinal} for {row}.");
                expected.Add(ordinal, value);
            }

            internal BdyExpectation Build()
            {
                Assert.That(expected.Count, Is.EqualTo(ExpectedCount), row.ToString());
                Assert.That(
                    expected.Keys,
                    Is.EqualTo(Enumerable.Range(0, ExpectedCount).ToArray()),
                    row.ToString());
                return new BdyExpectation(row, expected.Values.ToArray());
            }
        }

        private sealed class ItrExpectation
        {
            internal ItrExpectation(
                ScalarExpectation row,
                string[] clientBefore,
                string[] releaseExpected)
            {
                Row = row;
                ClientBefore = clientBefore;
                ReleaseExpected = releaseExpected;
            }

            internal ScalarExpectation Row { get; }
            internal string[] ClientBefore { get; }
            internal string[] ReleaseExpected { get; }
        }
    }
}
#endif
