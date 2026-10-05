namespace NTSD.Simulation
{
    /// <summary>
    /// Converts source-rule battle distances to the project's fixed full-view space.
    /// A position conversion requires one anchor shared by every entity in the battle.
    /// </summary>
    public readonly struct BattleSpatialProjection
    {
        public const int FormalViewWidthPx = 1333;
        public const int FormalViewHeightPx = 730;

        public static BattleSpatialProjection Identity =>
            new BattleSpatialProjection(1.0, 1.0, 0.0, 0.0);

        public double HorizontalScale { get; }
        public double DepthScale { get; }
        public double VerticalScale => DepthScale;
        public double SharedAnchorX { get; }
        public double SharedAnchorZ { get; }

        private BattleSpatialProjection(
            double horizontalScale,
            double depthScale,
            double sharedAnchorX,
            double sharedAnchorZ)
        {
            HorizontalScale = horizontalScale;
            DepthScale = depthScale;
            SharedAnchorX = sharedAnchorX;
            SharedAnchorZ = sharedAnchorZ;
        }

        public static BattleSpatialProjection FromReferenceViewport(
            int referenceWidthPx,
            int referenceHeightPx) =>
            FromReferenceViewport(referenceWidthPx, referenceHeightPx, 0.0, 0.0);

        public static BattleSpatialProjection FromReferenceViewport(
            int referenceWidthPx,
            int referenceHeightPx,
            double sharedAnchorX,
            double sharedAnchorZ)
        {
            return new BattleSpatialProjection(
                referenceWidthPx > FormalViewWidthPx
                    ? referenceWidthPx / (double)FormalViewWidthPx
                    : 1.0,
                referenceHeightPx > FormalViewHeightPx
                    ? referenceHeightPx / (double)FormalViewHeightPx
                    : 1.0,
                sharedAnchorX,
                sharedAnchorZ);
        }

        public double SourceDeltaToViewX(double deltaX) =>
            deltaX * HorizontalScale;

        public double SourceDeltaToViewZ(double deltaZ) =>
            deltaZ * DepthScale;

        public double SourceDeltaToViewY(double deltaY) =>
            deltaY * VerticalScale;

        public double SourceToViewX(double sourceX) =>
            SourceToViewX(sourceX, SharedAnchorX);

        public double SourceToViewZ(double sourceZ) =>
            SourceToViewZ(sourceZ, SharedAnchorZ);

        public double ViewToSourceX(double viewX) =>
            ViewToSourceX(viewX, SharedAnchorX);

        public double ViewToSourceZ(double viewZ) =>
            ViewToSourceZ(viewZ, SharedAnchorZ);

        public double SourceToViewX(double sourceX, double sharedAnchorX) =>
            sharedAnchorX + SourceDeltaToViewX(sourceX - sharedAnchorX);

        public double SourceToViewZ(double sourceZ, double sharedAnchorZ) =>
            sharedAnchorZ + SourceDeltaToViewZ(sourceZ - sharedAnchorZ);

        public double ViewToSourceX(double viewX, double sharedAnchorX) =>
            sharedAnchorX + (viewX - sharedAnchorX) / HorizontalScale;

        public double ViewToSourceZ(double viewZ, double sharedAnchorZ) =>
            sharedAnchorZ + (viewZ - sharedAnchorZ) / DepthScale;
    }
}
