using System;
using System.Globalization;
using System.Text;
using NTSD.App;
using NTSD.Simulation.Presentation;
using UnityEngine;
using UnityEngine.Rendering;

namespace NTSD.Animation.Rendering
{
    public enum BattleAtlasPolicyMode : byte
    {
        Auto = 0,
        TextureArray = 1,
        OrderedPages = 2,
    }

    public enum BattleDrawPolicyMode : byte
    {
        Auto = 0,
        OrderedChunks = 1,
        StrictOrderedDraw = 2,
    }

    public enum BattleRenderingPlatformCategory : byte
    {
        Other = 0,
        Desktop = 1,
        Mobile = 2,
    }

    public static class BattleRenderingPlatformPolicy
    {
        public const long MobileAtlasMemoryBudgetBytes = 256L * 1024L * 1024L;
        public const long DesktopAtlasMemoryBudgetBytes = 512L * 1024L * 1024L;

        public static BattleRenderingPlatformCategory ResolvePlatformCategory(
            bool isEditor,
            bool isStandalone,
            bool isAndroid,
            bool isIos)
        {
            if (isEditor)
                return BattleRenderingPlatformCategory.Desktop;
            if (isAndroid || isIos)
                return BattleRenderingPlatformCategory.Mobile;
            if (isStandalone)
                return BattleRenderingPlatformCategory.Desktop;
            return BattleRenderingPlatformCategory.Other;
        }

        public static long ResolveDefaultAtlasMemoryBudgetBytes(
            BattleRenderingPlatformCategory category)
        {
            return category == BattleRenderingPlatformCategory.Desktop
                ? DesktopAtlasMemoryBudgetBytes
                : MobileAtlasMemoryBudgetBytes;
        }

        internal static BattleRenderingPlatformCategory ResolveCurrentPlatformCategory()
        {
#if UNITY_EDITOR
            return ResolvePlatformCategory(true, false, false, false);
#elif UNITY_ANDROID
            return ResolvePlatformCategory(false, false, true, false);
#elif UNITY_IOS
            return ResolvePlatformCategory(false, false, false, true);
#elif UNITY_STANDALONE
            return ResolvePlatformCategory(false, true, false, false);
#else
            return ResolvePlatformCategory(false, false, false, false);
#endif
        }
    }

    public sealed class BattleRenderingDeviceCapabilities
    {
        public const long MobileAtlasMemoryBudgetBytes =
            BattleRenderingPlatformPolicy.MobileAtlasMemoryBudgetBytes;
        public const long DesktopAtlasMemoryBudgetBytes =
            BattleRenderingPlatformPolicy.DesktopAtlasMemoryBudgetBytes;

        public BattleRenderingDeviceCapabilities(
            string gpuName,
            string deviceName,
            string graphicsApiName,
            bool supports2DArrayTextures,
            int maxTextureSize,
            int maxTextureArraySlices,
            bool supportsRgba32Sampling,
            bool supportsCopyTexture,
            long atlasMemoryBudgetBytes)
        {
            GpuName = gpuName ?? string.Empty;
            DeviceName = deviceName ?? string.Empty;
            GraphicsApiName = graphicsApiName ?? string.Empty;
            Supports2DArrayTextures = supports2DArrayTextures;
            MaxTextureSize = maxTextureSize;
            MaxTextureArraySlices = maxTextureArraySlices;
            SupportsRgba32Sampling = supportsRgba32Sampling;
            SupportsCopyTexture = supportsCopyTexture;
            AtlasMemoryBudgetBytes = atlasMemoryBudgetBytes;
        }

        public string GpuName { get; }
        public string DeviceName { get; }
        public string GraphicsApiName { get; }
        public bool Supports2DArrayTextures { get; }
        public int MaxTextureSize { get; }
        public int MaxTextureArraySlices { get; }
        public bool SupportsRgba32Sampling { get; }
        public bool SupportsCopyTexture { get; }
        public long AtlasMemoryBudgetBytes { get; }

        public BattleAtlasCapabilityPolicy ToAtlasCapabilityPolicy(string forcedOrderedPagesReason = null)
        {
            return new BattleAtlasCapabilityPolicy(
                Supports2DArrayTextures,
                MaxTextureSize,
                MaxTextureArraySlices,
                SupportsRgba32Sampling,
                SupportsCopyTexture,
                AtlasMemoryBudgetBytes,
                null,
                forcedOrderedPagesReason);
        }

        public static BattleRenderingDeviceCapabilities FromSystem()
        {
            BattleRenderingPlatformCategory category =
                BattleRenderingPlatformPolicy.ResolveCurrentPlatformCategory();
            return FromSystem(
                BattleRenderingPlatformPolicy.ResolveDefaultAtlasMemoryBudgetBytes(category));
        }

        public static BattleRenderingDeviceCapabilities FromSystem(long atlasMemoryBudgetBytes)
        {
            return new BattleRenderingDeviceCapabilities(
                SystemInfo.graphicsDeviceName,
                SystemInfo.deviceModel,
                SystemInfo.graphicsDeviceType.ToString(),
                SystemInfo.supports2DArrayTextures,
                SystemInfo.maxTextureSize,
                SystemInfo.maxTextureArraySlices,
                SystemInfo.SupportsTextureFormat(TextureFormat.RGBA32),
                SystemInfo.copyTextureSupport != CopyTextureSupport.None,
                atlasMemoryBudgetBytes);
        }
    }

    public sealed class BattleAtlasPolicyDecision
    {
        internal BattleAtlasPolicyDecision(
            BattleAtlasPolicyMode requestedMode,
            BattleAtlasPolicyMode effectiveMode,
            string fallbackOrRefusalReason,
            BattleAtlasCapabilityPolicy capabilityPolicy)
        {
            RequestedMode = requestedMode;
            EffectiveMode = effectiveMode;
            FallbackOrRefusalReason = fallbackOrRefusalReason ?? string.Empty;
            CapabilityPolicy = capabilityPolicy ?? throw new ArgumentNullException(nameof(capabilityPolicy));
        }

        public BattleAtlasPolicyMode RequestedMode { get; }
        public BattleAtlasPolicyMode EffectiveMode { get; }
        public string FallbackOrRefusalReason { get; }
        public BattleAtlasCapabilityPolicy CapabilityPolicy { get; }
    }

    public readonly struct BattleDrawPolicyDecision
    {
        public BattleDrawPolicyDecision(
            BattleDrawPolicyMode requestedMode,
            BattleCentralDrawMode effectiveMode,
            string fallbackOrRefusalReason)
        {
            RequestedMode = requestedMode;
            EffectiveMode = effectiveMode;
            FallbackOrRefusalReason = fallbackOrRefusalReason ?? string.Empty;
        }

        public BattleDrawPolicyMode RequestedMode { get; }
        public BattleCentralDrawMode EffectiveMode { get; }
        public string FallbackOrRefusalReason { get; }
    }

    public static class BattleRenderingPolicyResolver
    {
        public const string AtlasModeArgument = "-ntsdBattleAtlasMode";
        public const string DrawModeArgument = "-ntsdBattleDrawMode";

        public static bool TryParseAtlasMode(string value, out BattleAtlasPolicyMode mode)
        {
            if (string.Equals(value, nameof(BattleAtlasPolicyMode.Auto), StringComparison.OrdinalIgnoreCase))
            {
                mode = BattleAtlasPolicyMode.Auto;
                return true;
            }
            if (string.Equals(value, nameof(BattleAtlasPolicyMode.TextureArray), StringComparison.OrdinalIgnoreCase))
            {
                mode = BattleAtlasPolicyMode.TextureArray;
                return true;
            }
            if (string.Equals(value, nameof(BattleAtlasPolicyMode.OrderedPages), StringComparison.OrdinalIgnoreCase))
            {
                mode = BattleAtlasPolicyMode.OrderedPages;
                return true;
            }

            mode = BattleAtlasPolicyMode.Auto;
            return false;
        }

        public static bool TryParseDrawMode(string value, out BattleDrawPolicyMode mode)
        {
            if (string.Equals(value, nameof(BattleDrawPolicyMode.Auto), StringComparison.OrdinalIgnoreCase))
            {
                mode = BattleDrawPolicyMode.Auto;
                return true;
            }
            if (string.Equals(value, nameof(BattleDrawPolicyMode.OrderedChunks), StringComparison.OrdinalIgnoreCase))
            {
                mode = BattleDrawPolicyMode.OrderedChunks;
                return true;
            }
            if (string.Equals(value, nameof(BattleDrawPolicyMode.StrictOrderedDraw), StringComparison.OrdinalIgnoreCase))
            {
                mode = BattleDrawPolicyMode.StrictOrderedDraw;
                return true;
            }

            mode = BattleDrawPolicyMode.Auto;
            return false;
        }

        public static BattleAtlasPolicyDecision ResolveAtlas(
            BattleRenderingDeviceCapabilities capabilities,
            int plannedPageCount,
            string[] commandLineArguments,
            string configuredMode)
        {
            if (capabilities == null)
                throw new ArgumentNullException(nameof(capabilities));
            if (plannedPageCount < 0)
                throw new ArgumentOutOfRangeException(nameof(plannedPageCount));

            string explicitMode = FindArgumentValue(commandLineArguments, AtlasModeArgument);
            BattleAtlasPolicyMode requestedMode = ResolveRequestedAtlasMode(explicitMode, configuredMode);
            BattleAtlasCapabilityPolicy capabilityPolicy = capabilities.ToAtlasCapabilityPolicy();
            BattleAtlasArrayDecision arrayDecision = capabilityPolicy.EvaluateArray(plannedPageCount);

            if (requestedMode == BattleAtlasPolicyMode.OrderedPages)
            {
                const string forcedPagesReason = "OrderedPages was explicitly requested; Texture2DArray allocation is disabled.";
                return new BattleAtlasPolicyDecision(
                    requestedMode,
                    BattleAtlasPolicyMode.OrderedPages,
                    forcedPagesReason,
                    capabilities.ToAtlasCapabilityPolicy(forcedPagesReason));
            }

            if (arrayDecision.UseTextureArray)
            {
                return new BattleAtlasPolicyDecision(
                    requestedMode,
                    BattleAtlasPolicyMode.TextureArray,
                    string.Empty,
                    capabilityPolicy);
            }

            string reason = requestedMode == BattleAtlasPolicyMode.TextureArray
                ? "Forced TextureArray was refused by the capability gate: " + arrayDecision.Reason
                : "Auto selected OrderedPages because the capability gate rejected TextureArray: " +
                  arrayDecision.Reason;
            return new BattleAtlasPolicyDecision(
                requestedMode,
                BattleAtlasPolicyMode.OrderedPages,
                reason,
                capabilities.ToAtlasCapabilityPolicy(reason));
        }

        public static BattleAtlasPolicyDecision ResolveAtlas(
            BattleRenderingDeviceCapabilities capabilities,
            int plannedPageCount,
            GameConfig config,
            string[] commandLineArguments = null)
        {
            return ResolveAtlas(
                capabilities,
                plannedPageCount,
                commandLineArguments ?? Environment.GetCommandLineArgs(),
                config?.BattleAtlasModeName);
        }

        public static BattleDrawPolicyDecision ResolveDraw(
            string[] commandLineArguments,
            string configuredMode,
            BattleCentralDrawMode serializedFallback)
        {
            string explicitMode = FindArgumentValue(commandLineArguments, DrawModeArgument);
            BattleDrawPolicyMode requestedMode = ResolveRequestedDrawMode(explicitMode, configuredMode);
            if (requestedMode == BattleDrawPolicyMode.StrictOrderedDraw)
            {
                return new BattleDrawPolicyDecision(
                    requestedMode,
                    BattleCentralDrawMode.StrictOrderedDraw,
                    string.Empty);
            }
            if (requestedMode == BattleDrawPolicyMode.OrderedChunks)
            {
                return new BattleDrawPolicyDecision(
                    requestedMode,
                    BattleCentralDrawMode.OrderedChunks,
                    string.Empty);
            }

            if (serializedFallback == BattleCentralDrawMode.StrictOrderedDraw)
            {
                return new BattleDrawPolicyDecision(
                    BattleDrawPolicyMode.Auto,
                    BattleCentralDrawMode.StrictOrderedDraw,
                    string.Empty);
            }

            string reason = serializedFallback == BattleCentralDrawMode.SingleMeshDiagnosticOnly
                ? "SingleMeshDiagnosticOnly is diagnostic-only and was replaced by OrderedChunks for production."
                : string.Empty;
            return new BattleDrawPolicyDecision(
                BattleDrawPolicyMode.Auto,
                BattleCentralDrawMode.OrderedChunks,
                reason);
        }

        public static BattleDrawPolicyDecision ResolveDraw(
            GameConfig config,
            BattleCentralDrawMode serializedFallback,
            string[] commandLineArguments = null)
        {
            return ResolveDraw(
                commandLineArguments ?? Environment.GetCommandLineArgs(),
                config?.BattleDrawModeName,
                serializedFallback);
        }

        private static BattleAtlasPolicyMode ResolveRequestedAtlasMode(
            string explicitMode,
            string configuredMode)
        {
            if (TryParseAtlasMode(explicitMode, out BattleAtlasPolicyMode mode))
                return mode;
            if (TryParseAtlasMode(configuredMode, out mode))
                return mode;
            return BattleAtlasPolicyMode.Auto;
        }

        private static BattleDrawPolicyMode ResolveRequestedDrawMode(
            string explicitMode,
            string configuredMode)
        {
            if (TryParseDrawMode(explicitMode, out BattleDrawPolicyMode mode))
                return mode;
            if (TryParseDrawMode(configuredMode, out mode))
                return mode;
            return BattleDrawPolicyMode.Auto;
        }

        internal static string FindArgumentValue(string[] arguments, string argumentName)
        {
            if (arguments == null || string.IsNullOrEmpty(argumentName))
                return null;

            string assignmentPrefix = argumentName + "=";
            for (int index = 0; index < arguments.Length; index++)
            {
                string argument = arguments[index];
                if (argument != null &&
                    argument.StartsWith(assignmentPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    return argument.Substring(assignmentPrefix.Length);
                }
                if (string.Equals(argument, argumentName, StringComparison.OrdinalIgnoreCase) &&
                    index + 1 < arguments.Length)
                {
                    return arguments[index + 1];
                }
            }
            return null;
        }
    }

    public sealed class BattleAtlasDiagnosticInputs
    {
        public BattleAtlasDiagnosticInputs(
            BattleRenderingDeviceCapabilities capabilities,
            BattleAtlasPolicyDecision decision,
            int plannedPageCount,
            long estimatedAtlasBytes,
            BattleSpriteCentralBindingMode catalogResourceMode,
            string catalogDiagnostic)
        {
            Capabilities = capabilities ?? throw new ArgumentNullException(nameof(capabilities));
            Decision = decision ?? throw new ArgumentNullException(nameof(decision));
            PlannedPageCount = plannedPageCount;
            EstimatedAtlasBytes = estimatedAtlasBytes;
            CatalogResourceMode = catalogResourceMode;
            CatalogDiagnostic = catalogDiagnostic ?? string.Empty;
        }

        public BattleRenderingDeviceCapabilities Capabilities { get; }
        public BattleAtlasPolicyDecision Decision { get; }
        public int PlannedPageCount { get; }
        public long EstimatedAtlasBytes { get; }
        public BattleSpriteCentralBindingMode CatalogResourceMode { get; }
        public string CatalogDiagnostic { get; }

        public static long EstimateAtlasBytes(int pageCount)
        {
            return checked((long)pageCount * BattleAtlasLayoutPlanner.PageSize *
                           BattleAtlasLayoutPlanner.PageSize * 4L);
        }
    }

    public sealed class BattleRenderingDiagnosticReport
    {
        public BattleRenderingDiagnosticReport(
            BattleAtlasDiagnosticInputs atlas,
            BattleDrawPolicyDecision draw,
            int sourceCommandCount,
            int resolvedCommandCount,
            int unresolvedCommandCount,
            int unsupportedCategoryCount,
            int activeChunkCount,
            int segmentCount,
            int submissionDrawCount,
            BattlePresentationBackendMode requestedPixelMode,
            BattlePresentationBackendMode effectivePixelMode,
            int snapshotEntityCount = 0,
            int generation = 0,
            int buildTick = -1,
            int simulationTick = -1,
            int displayTick = -1,
            bool isStale = false,
            string refusalReason = "",
            bool submissionBuildCurrent = false,
            int unsupportedRenderStateCount = 0,
            int firstUnresolvedCommandIndex = -1,
            BattleRenderCommandType firstUnresolvedCommandType = default,
            BattleCentralResourceStatus firstUnresolvedStatus = BattleCentralResourceStatus.Resolved)
        {
            Atlas = atlas ?? throw new ArgumentNullException(nameof(atlas));
            Draw = draw;
            SourceCommandCount = sourceCommandCount;
            ResolvedCommandCount = resolvedCommandCount;
            UnresolvedCommandCount = unresolvedCommandCount;
            UnsupportedCategoryCount = unsupportedCategoryCount;
            ActiveChunkCount = activeChunkCount;
            SegmentCount = segmentCount;
            SubmissionDrawCount = submissionDrawCount;
            RequestedPixelMode = requestedPixelMode;
            EffectivePixelMode = effectivePixelMode;
            SnapshotEntityCount = snapshotEntityCount;
            Generation = generation;
            BuildTick = buildTick;
            SimulationTick = simulationTick;
            DisplayTick = displayTick;
            IsStale = isStale;
            RefusalReason = refusalReason ?? string.Empty;
            SubmissionBuildCurrent = submissionBuildCurrent;
            UnsupportedRenderStateCount = unsupportedRenderStateCount;
            FirstUnresolvedCommandIndex = firstUnresolvedCommandIndex;
            FirstUnresolvedCommandType = firstUnresolvedCommandType;
            FirstUnresolvedStatus = firstUnresolvedStatus;
        }

        public BattleAtlasDiagnosticInputs Atlas { get; }
        public BattleDrawPolicyDecision Draw { get; }
        public int SourceCommandCount { get; }
        public int ResolvedCommandCount { get; }
        public int UnresolvedCommandCount { get; }
        public int UnsupportedCategoryCount { get; }
        public int ActiveChunkCount { get; }
        public int SegmentCount { get; }
        public int SubmissionDrawCount { get; }
        public BattlePresentationBackendMode RequestedPixelMode { get; }
        public BattlePresentationBackendMode EffectivePixelMode { get; }
        public int SnapshotEntityCount { get; }
        public int Generation { get; }
        public int BuildTick { get; }
        public int SimulationTick { get; }
        public int DisplayTick { get; }
        public bool IsStale { get; }
        public string RefusalReason { get; }
        public bool SubmissionBuildCurrent { get; }
        public int UnsupportedRenderStateCount { get; }
        public int FirstUnresolvedCommandIndex { get; }
        public BattleRenderCommandType FirstUnresolvedCommandType { get; }
        public BattleCentralResourceStatus FirstUnresolvedStatus { get; }

        public string ToJson()
        {
            var builder = new StringBuilder(1024);
            BattleRenderingDeviceCapabilities capabilities = Atlas.Capabilities;
            builder.Append('{');
            AppendProperty(builder, "requestedAtlasMode", Atlas.Decision.RequestedMode.ToString(), true);
            AppendProperty(builder, "effectiveAtlasMode", Atlas.Decision.EffectiveMode.ToString(), false);
            AppendProperty(builder, "atlasFallbackOrRefusalReason", Atlas.Decision.FallbackOrRefusalReason, false);
            AppendProperty(builder, "requestedDrawMode", Draw.RequestedMode.ToString(), false);
            AppendProperty(builder, "effectiveDrawMode", Draw.EffectiveMode.ToString(), false);
            AppendProperty(builder, "drawFallbackOrRefusalReason", Draw.FallbackOrRefusalReason, false);
            AppendProperty(builder, "gpuName", capabilities.GpuName, false);
            AppendProperty(builder, "deviceName", capabilities.DeviceName, false);
            AppendProperty(builder, "graphicsApiName", capabilities.GraphicsApiName, false);
            AppendProperty(builder, "supports2DArrayTextures", capabilities.Supports2DArrayTextures, false);
            AppendProperty(builder, "maxTextureSize", capabilities.MaxTextureSize, false);
            AppendProperty(builder, "maxTextureArraySlices", capabilities.MaxTextureArraySlices, false);
            AppendProperty(builder, "supportsRgba32Sampling", capabilities.SupportsRgba32Sampling, false);
            AppendProperty(builder, "supportsCopyTexture", capabilities.SupportsCopyTexture, false);
            AppendProperty(builder, "atlasMemoryBudgetBytes", capabilities.AtlasMemoryBudgetBytes, false);
            AppendProperty(builder, "plannedPageCount", Atlas.PlannedPageCount, false);
            AppendProperty(builder, "estimatedAtlasBytes", Atlas.EstimatedAtlasBytes, false);
            AppendProperty(builder, "catalogResourceMode", Atlas.CatalogResourceMode.ToString(), false);
            AppendProperty(builder, "catalogDiagnostic", Atlas.CatalogDiagnostic, false);
            AppendProperty(builder, "sourceCommandCount", SourceCommandCount, false);
            AppendProperty(builder, "resolvedCommandCount", ResolvedCommandCount, false);
            AppendProperty(builder, "unresolvedCommandCount", UnresolvedCommandCount, false);
            AppendProperty(builder, "unsupportedCategoryCount", UnsupportedCategoryCount, false);
            AppendProperty(builder, "unsupportedRenderStateCount", UnsupportedRenderStateCount, false);
            AppendProperty(builder, "firstUnresolvedCommandIndex", FirstUnresolvedCommandIndex, false);
            AppendProperty(
                builder,
                "firstUnresolvedCommandType",
                FirstUnresolvedCommandIndex >= 0 ? FirstUnresolvedCommandType.ToString() : string.Empty,
                false);
            AppendProperty(
                builder,
                "firstUnresolvedStatus",
                FirstUnresolvedCommandIndex >= 0 ? FirstUnresolvedStatus.ToString() : string.Empty,
                false);
            AppendProperty(builder, "activeChunkCount", ActiveChunkCount, false);
            AppendProperty(builder, "segmentCount", SegmentCount, false);
            AppendProperty(builder, "submissionDrawCount", SubmissionDrawCount, false);
            AppendProperty(builder, "snapshotEntityCount", SnapshotEntityCount, false);
            AppendProperty(builder, "requestedPixelMode", RequestedPixelMode.ToString(), false);
            AppendProperty(builder, "effectivePixelMode", EffectivePixelMode.ToString(), false);
            AppendProperty(builder, "generation", Generation, false);
            AppendProperty(builder, "buildTick", BuildTick, false);
            AppendProperty(builder, "simulationTick", SimulationTick, false);
            AppendProperty(builder, "displayTick", DisplayTick, false);
            AppendProperty(builder, "isStale", IsStale, false);
            AppendProperty(builder, "refusalReason", RefusalReason, false);
            AppendProperty(builder, "submissionBuildCurrent", SubmissionBuildCurrent, false);
            builder.Append('}');
            return builder.ToString();
        }

        public override string ToString()
        {
            return ToJson();
        }

        private static void AppendProperty(StringBuilder builder, string name, string value, bool first)
        {
            if (!first)
                builder.Append(',');
            AppendJsonString(builder, name);
            builder.Append(':');
            AppendJsonString(builder, value ?? string.Empty);
        }

        private static void AppendProperty(StringBuilder builder, string name, bool value, bool first)
        {
            if (!first)
                builder.Append(',');
            AppendJsonString(builder, name);
            builder.Append(value ? ":true" : ":false");
        }

        private static void AppendProperty(StringBuilder builder, string name, int value, bool first)
        {
            AppendProperty(builder, name, (long)value, first);
        }

        private static void AppendProperty(StringBuilder builder, string name, long value, bool first)
        {
            if (!first)
                builder.Append(',');
            AppendJsonString(builder, name);
            builder.Append(':');
            builder.Append(value.ToString(CultureInfo.InvariantCulture));
        }

        private static void AppendJsonString(StringBuilder builder, string value)
        {
            builder.Append('"');
            for (int index = 0; index < value.Length; index++)
            {
                char character = value[index];
                switch (character)
                {
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        if (character < 0x20)
                        {
                            builder.Append("\\u");
                            builder.Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            builder.Append(character);
                        }
                        break;
                }
            }
            builder.Append('"');
        }
    }
}
