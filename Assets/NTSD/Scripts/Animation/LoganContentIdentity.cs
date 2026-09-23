using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NTSD.Simulation.Lockstep;

namespace NTSD.Animation
{
    /// <summary>Immutable raw input and decoder identity, computed before publication or simulation.</summary>
    public sealed class LoganContentIdentity
    {
        public const string CurrentDecodeContractTag = "NTSD28_LOGAN_DAT_SEMANTICS_V3";
        private const string BattleInputTag = "NTSD28_LOGAN_BATTLE_INPUTS_V1";
        private const string ModeBattleInputTag = "NTSD28_LOGAN_BATTLE_INPUTS_V2";
        private const string KindOnlyBattleInputTag = "NTSD28_LOGAN_BATTLE_INPUTS_V3_KIND_ONLY";
        private const string KindModeBattleInputTag = "NTSD28_LOGAN_BATTLE_INPUTS_V3";

        public string ObjectDefinitionFingerprint { get; }
        public string FusionInputFingerprint { get; }
        public string FusionSemanticFingerprint { get; }
        public string ModeInputFingerprint { get; }
        public string ModeSemanticFingerprint { get; }
        public string KindInputFingerprint { get; }
        public string KindSemanticFingerprint { get; }
        public string BattleInputContractTag { get; }

        public string RawDefinitionFingerprint { get; }
        public string DecodeContractTag { get; }
        public string SemanticFingerprint { get; }
        public ulong CatalogFingerprint { get; }

        private LoganContentIdentity(string rawFingerprint, string contractTag)
        {
            byte[] raw = DecodeFingerprint(rawFingerprint);
            if (string.IsNullOrEmpty(contractTag))
                throw new ArgumentException("A nonempty ASCII decoder contract is required.", nameof(contractTag));
            foreach (char character in contractTag)
                if (character == '\0' || character > 127)
                    throw new ArgumentException("Decoder contract must be ASCII without NUL.", nameof(contractTag));

            RawDefinitionFingerprint = Hex(raw);
            DecodeContractTag = contractTag;
            byte[] tag = Encoding.ASCII.GetBytes(contractTag);
            byte[] preimage = new byte[tag.Length + 1 + raw.Length];
            Buffer.BlockCopy(tag, 0, preimage, 0, tag.Length);
            Buffer.BlockCopy(raw, 0, preimage, tag.Length + 1, raw.Length);
            using (var sha = SHA256.Create())
            {
                byte[] digest = sha.ComputeHash(preimage);
                SemanticFingerprint = Hex(digest);
                CatalogFingerprint = ProjectCatalogFingerprint(digest);
            }
        }

        public static LoganContentIdentity FromDefinitionFingerprint(string rawFingerprint)
        {
            return new LoganContentIdentity(rawFingerprint, "NTSD28_LOGAN_DAT_SEMANTICS_V2");
        }

        private LoganContentIdentity(string composite, string objects, string fusionInput, string fusionSemantic,
            string modeInput = null, string modeSemantic = null,
            string kindInput = null, string kindSemantic = null)
            : this(composite, CurrentDecodeContractTag)
        {
            ObjectDefinitionFingerprint = objects;
            FusionInputFingerprint = fusionInput;
            FusionSemanticFingerprint = fusionSemantic;
            ModeInputFingerprint = modeInput;
            ModeSemanticFingerprint = modeSemantic;
            KindInputFingerprint = kindInput;
            KindSemanticFingerprint = kindSemantic;
            BattleInputContractTag = kindInput != null
                ? (modeInput == null ? KindOnlyBattleInputTag : KindModeBattleInputTag)
                : (modeInput == null ? BattleInputTag : ModeBattleInputTag);
        }

        public static LoganContentIdentity FromBattleComponents(string objects, string fusionInput, string fusionSemantic)
        {
            return FromBattleComponents(objects, fusionInput, fusionSemantic, null, null);
        }

        public static LoganContentIdentity FromBattleComponents(string objects, string fusionInput, string fusionSemantic,
            string modeInput, string modeSemantic)
        {
            return FromBattleComponents(objects, fusionInput, fusionSemantic,
                modeInput, modeSemantic, null, null);
        }

        public static LoganContentIdentity FromBattleComponents(string objects, string fusionInput, string fusionSemantic,
            string modeInput, string modeSemantic, string kindInput, string kindSemantic)
        {
            if ((modeInput == null) != (modeSemantic == null))
                throw new ArgumentException("Mode input and semantic fingerprints must be supplied together.");
            if ((kindInput == null) != (kindSemantic == null))
                throw new ArgumentException("Kind input and semantic fingerprints must be supplied together.");
            byte[] objectBytes = DecodeFingerprint(objects);
            byte[] inputBytes = DecodeFingerprint(fusionInput);
            byte[] semanticBytes = DecodeFingerprint(fusionSemantic);
            byte[] modeInputBytes = modeInput == null ? null : DecodeFingerprint(modeInput);
            byte[] modeSemanticBytes = modeSemantic == null ? null : DecodeFingerprint(modeSemantic);
            byte[] kindInputBytes = kindInput == null ? null : DecodeFingerprint(kindInput);
            byte[] kindSemanticBytes = kindSemantic == null ? null : DecodeFingerprint(kindSemantic);
            string inputTag = kindInputBytes != null
                ? (modeInputBytes == null ? KindOnlyBattleInputTag : KindModeBattleInputTag)
                : (modeInputBytes == null ? BattleInputTag : ModeBattleInputTag);
            byte[] tag = Encoding.ASCII.GetBytes(inputTag);
            byte[] preimage = new byte[tag.Length + 1 + 96 +
                (modeInputBytes == null ? 0 : 64) + (kindInputBytes == null ? 0 : 64)];
            Buffer.BlockCopy(tag, 0, preimage, 0, tag.Length);
            Buffer.BlockCopy(objectBytes, 0, preimage, tag.Length + 1, 32);
            Buffer.BlockCopy(inputBytes, 0, preimage, tag.Length + 33, 32);
            Buffer.BlockCopy(semanticBytes, 0, preimage, tag.Length + 65, 32);
            if (modeInputBytes != null)
            {
                Buffer.BlockCopy(modeInputBytes, 0, preimage, tag.Length + 97, 32);
                Buffer.BlockCopy(modeSemanticBytes, 0, preimage, tag.Length + 129, 32);
            }
            if (kindInputBytes != null)
            {
                int kindOffset = tag.Length + 97 + (modeInputBytes == null ? 0 : 64);
                Buffer.BlockCopy(kindInputBytes, 0, preimage, kindOffset, 32);
                Buffer.BlockCopy(kindSemanticBytes, 0, preimage, kindOffset + 32, 32);
            }
            using (var sha = SHA256.Create())
                return new LoganContentIdentity(Hex(sha.ComputeHash(preimage)),
                    Hex(objectBytes), Hex(inputBytes), Hex(semanticBytes),
                    modeInputBytes == null ? null : Hex(modeInputBytes),
                    modeSemanticBytes == null ? null : Hex(modeSemanticBytes),
                    kindInputBytes == null ? null : Hex(kindInputBytes),
                    kindSemanticBytes == null ? null : Hex(kindSemanticBytes));
        }

        private static byte[] DecodeFingerprint(string value)
        {
            if (value == null || value.Length != 64)
                throw new ArgumentException("A 32-byte hexadecimal component fingerprint is required.", nameof(value));
            var bytes = new byte[32];
            for (int i = 0; i < bytes.Length; i++)
            {
                int high = HexDigit(value[i * 2]);
                int low = HexDigit(value[i * 2 + 1]);
                if (high < 0 || low < 0)
                    throw new ArgumentException("Component fingerprint contains a non-hexadecimal character.", nameof(value));
                bytes[i] = (byte)((high << 4) | low);
            }
            return bytes;
        }

        internal static LoganContentIdentity ForDecodeContract(string rawFingerprint, string contractTag)
        {
            return new LoganContentIdentity(rawFingerprint, contractTag);
        }

        public string CreateSourceCacheKey(string runtimeRoot)
        {
            if (string.IsNullOrEmpty(runtimeRoot))
                throw new ArgumentException("A content root is required.", nameof(runtimeRoot));
            return runtimeRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + "|" + SemanticFingerprint;
        }

        public LockstepSessionIdentity CreateLocalValidationSessionIdentity(
            ulong sessionId, uint seed, ulong stageFingerprint, IReadOnlyList<int> playerSlots)
        {
            if (DecodeContractTag != CurrentDecodeContractTag || ObjectDefinitionFingerprint == null ||
                FusionInputFingerprint == null || FusionSemanticFingerprint == null)
                throw new InvalidOperationException("Local validation requires the current decoder contract and all battle content components.");
            return new LockstepSessionIdentity(LockstepSessionIdentity.CurrentSchemaVersion,
                sessionId, seed, CatalogFingerprint, stageFingerprint, playerSlots);
        }

        internal static ulong ProjectCatalogFingerprint(byte[] digest)
        {
            if (digest == null || digest.Length != 32)
                throw new ArgumentException("A SHA-256 digest is required.", nameof(digest));
            ulong result = 0;
            for (int i = 0; i < 8; i++) result |= (ulong)digest[i] << (i * 8);
            return result == 0 ? 1UL : result;
        }

        private static int HexDigit(char value)
        {
            if (value >= '0' && value <= '9') return value - '0';
            if (value >= 'A' && value <= 'F') return value - 'A' + 10;
            if (value >= 'a' && value <= 'f') return value - 'a' + 10;
            return -1;
        }

        private static string Hex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }
    }
}
