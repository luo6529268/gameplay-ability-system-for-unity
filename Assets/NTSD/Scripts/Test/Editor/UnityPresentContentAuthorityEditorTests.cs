#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NTSD.Animation;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace NTSD.Test
{
    [Category("UnityPresentContentAuthority")]
    public sealed class UnityPresentContentAuthorityEditorTests
    {
        private const string DatPassword =
            "odBearBecauseHeIsVeryGoodSiuHungIsAGo";
        private const int ExpectedDatCount = 138;
        private const string ArtifactRelativeDirectory =
            "artifacts/diagnostics/CAP-S0-1-unity-present-content-authority";

        [Test]
        [Explicit("Captures the Direction-B Unity-present content authority artifacts.")]
        public void CaptureUnityPresentContentAuthorityArtifacts()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            Assert.That(projectRoot, Is.Not.Null.And.Not.Empty);

            string configRoot = Path.Combine(projectRoot, "Assets", "NTSD", "Config");
            string[] files = Directory.GetFiles(
                    configRoot,
                    "*.dat",
                    SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            Assert.That(files.Length, Is.EqualTo(ExpectedDatCount));

            string rawManifest = BuildRawManifest(configRoot, files);
            string projection = BuildNormalizedProjection(configRoot, files);
            string rawManifestSha = GetSha256Hex(
                new UTF8Encoding(false).GetBytes(rawManifest));
            string projectionSha = GetSha256Hex(
                new UTF8Encoding(false).GetBytes(projection));

            string artifactDirectory = Path.Combine(
                projectRoot,
                ArtifactRelativeDirectory.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(artifactDirectory);
            File.WriteAllText(
                Path.Combine(artifactDirectory, "raw-manifest.tsv"),
                rawManifest,
                new UTF8Encoding(false));
            File.WriteAllText(
                Path.Combine(artifactDirectory, "normalized-projection.tsv"),
                projection,
                new UTF8Encoding(false));

            string summary =
                "authority=UNITY_PRESENT_DAT\n" +
                "schema=ntsd-unity-present-content-v1\n" +
                $"datCount={files.Length}\n" +
                $"rawManifestBytes={new UTF8Encoding(false).GetByteCount(rawManifest)}\n" +
                $"rawManifestSha256={rawManifestSha}\n" +
                $"projectionRows={CountLines(projection)}\n" +
                $"projectionBytes={new UTF8Encoding(false).GetByteCount(projection)}\n" +
                $"projectionSha256={projectionSha}\n";
            File.WriteAllText(
                Path.Combine(artifactDirectory, "authority-summary.txt"),
                summary,
                new UTF8Encoding(false));

            TestContext.Out.WriteLine(
                $"UNITY_PRESENT_CONTENT_AUTHORITY|files={files.Length}|" +
                $"rawManifestSha256={rawManifestSha}|" +
                $"projectionRows={CountLines(projection)}|" +
                $"projectionSha256={projectionSha}|" +
                $"path={ArtifactRelativeDirectory}");
        }

        private static string BuildRawManifest(string configRoot, string[] files)
        {
            var output = new StringBuilder(files.Length * 96);
            foreach (string absolutePath in files)
            {
                output.Append(GetRelativePath(configRoot, absolutePath)).Append('\t')
                    .Append(GetSha256Hex(File.ReadAllBytes(absolutePath))).Append('\n');
            }

            return output.ToString();
        }

        private static string BuildNormalizedProjection(
            string configRoot,
            string[] files)
        {
            var output = new StringBuilder(8 * 1024 * 1024);
            var parser = new Lf2DatParserV2();
            foreach (string absolutePath in files)
            {
                string relativePath = GetRelativePath(configRoot, absolutePath);
                string text = Lf2DatDecryptor.DecryptFile(absolutePath, DatPassword);
                Lf2DatFile dat = parser.Parse(text, absolutePath);
                AppendProjectionLine(output, relativePath, -1, -1,
                    "FILE", -1, "frameCount", dat.Frames.Count.ToString());

                for (int occurrence = 0; occurrence < dat.Frames.Count; occurrence++)
                {
                    Lf2DatFrameBlockProjection(
                        output,
                        relativePath,
                        occurrence,
                        dat.Frames[occurrence]);
                }
            }

            return output.ToString();
        }

        private static void Lf2DatFrameBlockProjection(
            StringBuilder output,
            string relativePath,
            int occurrence,
            Lf2FrameBlock frameBlock)
        {
            LF2FrameData frame = Lf2DatConverter.ConvertToFrameData(frameBlock);
            int frameId = frameBlock.FrameIndex;
            AppendProjectionLine(output, relativePath, occurrence, frameId,
                "FRAME", -1, "scalars", FormatFrameScalars(frame));

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
                $"unityExtra.dvz={itr.dvz};unityExtra.vaction={itr.vaction};" +
                $"unityExtra.kill={itr.kill}";
        }

        private static string FormatPair(int[] values)
        {
            return values == null ? "null" : string.Join(",", values);
        }

        private static string FormatObjectPoint(BattleObjectPointValue value)
        {
            return
                $"kind={value.Kind};x={value.X};y={value.Y};action={value.Action};" +
                $"dvx={value.Dvx};dvy={value.Dvy};oid={value.Oid};facing={value.Facing}";
        }

        private static string FormatWeaponPoint(BattleWeaponPointValue value)
        {
            return
                $"kind={value.Kind};x={value.X};y={value.Y};attacking={value.Attacking};" +
                $"cover={value.Cover};weaponact={value.WeaponAct};dvx={value.Dvx};" +
                $"dvy={value.Dvy};dvz={value.Dvz}";
        }

        private static string FormatBloodPoint(BattleBloodPointValue value)
        {
            return $"x={value.X};y={value.Y}";
        }

        private static string FormatCatchPoint(BattleCatchPointValue value)
        {
            return
                $"kind={value.Kind};x={value.X};y={value.Y};injury={value.Injury};" +
                $"cover={value.Cover};vaction={value.Vaction};aaction={value.Aaction};" +
                $"jaction={value.Jaction};daction={value.Daction};throwvx={value.ThrowVx};" +
                $"throwvy={value.ThrowVy};hurtable={value.Hurtable};decrease={value.Decrease};" +
                $"dircontrol={value.DirControl};taction={value.Taction};" +
                $"throwinjury={value.ThrowInjury};throwvz={value.ThrowVz};" +
                $"fronthurtact={value.FrontHurtAct};backhurtact={value.BackHurtAct}";
        }

        private static string GetRelativePath(string root, string absolutePath)
        {
            return absolutePath.Substring(root.Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Replace(Path.DirectorySeparatorChar, '/');
        }

        private static int CountLines(string text)
        {
            return text.Count(character => character == '\n');
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
    }

    [InitializeOnLoad]
    internal sealed class DirectionBValidationRequestRunner : ICallbacks
    {
        private const string RequestRelativePath =
            "Temp/NTSD_DirectionBFocused.request";
        private const string ResultRelativePath =
            "Temp/NTSD_DirectionBFocused.result";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static DirectionBValidationRequestRunner activeCallbacks;
        private static TestRunnerApi activeApi;

        static DirectionBValidationRequestRunner()
        {
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (activeCallbacks != null ||
                EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode ||
                !File.Exists(RequestPath))
            {
                return;
            }

            if (File.Exists(ResultPath))
                File.Delete(ResultPath);
            File.Delete(RequestPath);

            activeCallbacks = new DirectionBValidationRequestRunner();
            activeApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            activeApi.RegisterCallbacks(activeCallbacks);
            var settings = new ExecutionSettings(
                new Filter
                {
                    testMode = TestMode.EditMode,
                    testNames = new[]
                    {
                        "NTSD.Test.UnityPresentContentAuthorityEditorTests." +
                        "CaptureUnityPresentContentAuthorityArtifacts",
                    },
                },
                new Filter
                {
                    testMode = TestMode.EditMode,
                    groupNames = new[]
                    {
                        "^NTSD\\.Test\\.(FrameMultivalueParserAlignmentEditorTests|" +
                        "ItrParserDefaultsAlignmentEditorTests|" +
                        "FormalKernelBodyBoxValueSeamEditorTests|" +
                        "FormalKernelObjectPointValueSeamEditorTests|" +
                        "FormalKernelWeaponPointValueSeamEditorTests|" +
                        "WPointDefaultAlignmentEditorTests)$",
                    },
                })
            {
                runSynchronously = false,
            };
            activeApi.Execute(settings);
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            string text =
                $"state={result.ResultState}\n" +
                $"passed={result.PassCount}\n" +
                $"failed={result.FailCount}\n" +
                $"skipped={result.SkipCount}\n" +
                $"inconclusive={result.InconclusiveCount}\n" +
                $"message={result.Message}\n";
            File.WriteAllText(ResultPath, text, new UTF8Encoding(false));

            activeApi.UnregisterCallbacks(this);
            UnityEngine.Object.DestroyImmediate(activeApi);
            activeApi = null;
            activeCallbacks = null;
        }

        public void TestStarted(ITestAdaptor test)
        {
        }

        public void TestFinished(ITestResultAdaptor result)
        {
        }

        private static string ProjectPath(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.GetFullPath(Path.Combine(root ?? string.Empty, relativePath));
        }
    }
}
#endif
