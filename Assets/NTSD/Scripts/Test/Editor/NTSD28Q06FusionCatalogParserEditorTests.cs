#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NTSD.DatParser;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06FusionCatalogParserEditorTests
    {
        private static string Catalog(string fields) => "<fusion_begin>\nfusion: ignored\n" + fields + "\nfusion_end:\n<fusion_end>\n";

        [Test]
        public void FormalRecordsMatchActualSourceWitness()
        {
            const string path = "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime/decoded_dat/data/fusion.dat";
            var catalog = LoganFusionCatalogParser.ParseText(File.ReadAllText(path));
            Assert.That(catalog.SourceAvailable, Is.True);
            Assert.That(catalog.IsValid, Is.True);
            Assert.That(catalog.Records.Count, Is.EqualTo(2));
            var rows = File.ReadLines("artifacts/diagnostics/NTSD28-Q06-FUSION-CATALOG-TRANSACTION-WITNESS-001/source/first.jsonl")
                .Take(2).Select(JObject.Parse).ToArray();
            string[] names = { "id1", "id2", "id3", "hp", "mp", "respond", "decrease", "wait", "state", "action", "frame", "chp", "hitJa", "cover", "frontHurtAction", "backHurtAction" };
            for (int index = 0; index < 2; index++)
            {
                LoganFusionRecord record = catalog.Records[index];
                int[] values = { record.Id1, record.Id2, record.Id3, record.Hp, record.Mp, record.Respond, record.Decrease, record.Wait, record.State, record.Action, record.Frame, record.Chp, record.HitJa, record.Cover, record.FrontHurtAction, record.BackHurtAction };
                Assert.That(values, Is.EqualTo(names.Select(name => (int)rows[index]["catalogRecord"][name]).ToArray()));
                Assert.That(record.SourceLine, Is.GreaterThan(0));
            }
        }

        [TestCase("-2147483648", int.MinValue)]
        [TestCase("2147483647", int.MaxValue)]
        [TestCase("-0", 0)]
        [TestCase("0007", 7)]
        public void NativeStrictSignedIntegersAreAccepted(string text, int expected)
        {
            var catalog = LoganFusionCatalogParser.ParseText(Catalog("hp: " + text));
            Assert.That(catalog.IsValid, Is.True);
            Assert.That(catalog.Records[0].Hp, Is.EqualTo(expected));
        }

        [TestCase("+7")]
        [TestCase("1.0")]
        [TestCase("1x")]
        [TestCase("2147483648")]
        [TestCase("-2147483649")]
        [TestCase("")]
        [TestCase("1 mp: 2")]
        [TestCase("\u00a01")]
        public void InvalidNumericFieldIsNotSilentlyCoerced(string value)
        {
            var catalog = LoganFusionCatalogParser.ParseText(Catalog("hp: 9\nhp: " + value));
            Assert.That(catalog.IsValid, Is.False);
            Assert.That(catalog.Records.Count, Is.EqualTo(1));
            Assert.That(catalog.Records[0].Hp, Is.EqualTo(9));
            Assert.That(catalog.Diagnostics.Any(d => d.Severity == LoganFusionDiagnosticSeverity.Error && d.Line == 4), Is.True);
        }

        [Test]
        public void CommentsDuplicatesMissingFieldsAndRecordOrderMatchNativeGrammar()
        {
            var catalog = LoganFusionCatalogParser.ParseText("outside\n<fusion_begin>\nunknown: 3\n" +
                "fusion: not-a-number\nhp: 10 ; ignored\nhp: -7 # ignored\nunknown: 123\n" +
                "fusion_end:\nfusion: 0\nid1: 8\nfusion_end:\n<fusion_end>\n");
            Assert.That(catalog.IsValid, Is.True);
            Assert.That(catalog.Records.Count, Is.EqualTo(2));
            Assert.That(catalog.Records[0].Hp, Is.EqualTo(-7));
            Assert.That(catalog.Records[0].Id1, Is.Zero);
            Assert.That(catalog.Records[0].SourceLine, Is.EqualTo(4));
            Assert.That(catalog.Records[1].Id1, Is.EqualTo(8));
            Assert.That(catalog.Records[1].Hp, Is.Zero);
            Assert.That(catalog.Diagnostics.All(d => d.Severity == LoganFusionDiagnosticSeverity.Warning), Is.True);
            Assert.That(catalog.Diagnostics.Count, Is.EqualTo(3));
        }

        [TestCase(50, true)]
        [TestCase(51, false)]
        public void NativeRecordLimitRetainsFirstFifty(int count, bool valid)
        {
            string body = string.Concat(Enumerable.Range(0, count).Select(i => "fusion: 0\nid1: " + i + "\nfusion_end:\n"));
            var catalog = LoganFusionCatalogParser.ParseText("<fusion_begin>\n" + body + "<fusion_end>");
            Assert.That(catalog.IsValid, Is.EqualTo(valid));
            Assert.That(catalog.Records.Count, Is.EqualTo(50));
            Assert.That(catalog.Records.Select(r => r.Id1), Is.EqualTo(Enumerable.Range(0, 50)));
        }

        [TestCase("")]
        [TestCase("<fusion_end>")]
        [TestCase("<fusion_begin>\nfusion: 1\nhp: 3\n<fusion_end>")]
        [TestCase("<fusion_begin>\nfusion: 1\nfusion_end:")]
        [TestCase("<fusion_begin>\n<fusion_begin>\n<fusion_end>")]
        [TestCase("\ufeff<fusion_begin>\n<fusion_end>")]
        public void InvalidBoundariesAreRejected(string text)
        {
            var catalog = LoganFusionCatalogParser.ParseText(text);
            Assert.That(catalog.SourceAvailable, Is.True);
            Assert.That(catalog.IsValid, Is.False);
            Assert.That(catalog.Diagnostics.Any(d => d.Severity == LoganFusionDiagnosticSeverity.Error), Is.True);
        }

        [Test]
        public void EmptyOuterIsValidAndRepeatedStartDiscardsUnfinishedRecordWithError()
        {
            Assert.That(LoganFusionCatalogParser.ParseText("<fusion_begin>\n<fusion_end>").IsValid, Is.True);
            var catalog = LoganFusionCatalogParser.ParseText("<fusion_begin>\nfusion: 0\nhp: 7\nfusion: 1\nid1: 8\nfusion_end:\n<fusion_end>");
            Assert.That(catalog.IsValid, Is.False);
            Assert.That(catalog.Records.Count, Is.EqualTo(1));
            Assert.That(catalog.Records[0].Id1, Is.EqualTo(8));
            Assert.That(catalog.Records[0].Hp, Is.Zero);
            Assert.That(LoganFusionCatalogParser.ParseText(null).IsValid, Is.False);
        }

        [Test]
        public void PublishedRecordCollectionCannotBeMutated()
        {
            var catalog = LoganFusionCatalogParser.ParseText(Catalog("id1: 7"));
            var records = catalog.Records as IList<LoganFusionRecord>;
            Assert.That(records, Is.Not.Null);
            Assert.Throws<NotSupportedException>(() => records[0] = default);
            Assert.Throws<NotSupportedException>(() => records.Add(default));
            Assert.That(typeof(LoganFusionRecord).GetProperties().All(p => !p.CanWrite), Is.True);
        }
    }
}
#endif
