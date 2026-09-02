using System;
using UnityEngine;

namespace NTSD.Animation.Rendering
{
    [Serializable]
    public struct BattleFootMarkerStyle
    {
        [SerializeField][Min(1f)] private float widthPixels;
        [SerializeField][Min(1f)] private float heightPixels;
        [SerializeField] private Vector2 offsetPixels;
        [SerializeField] private Color32 tint;

        public BattleFootMarkerStyle(
            float widthPixels,
            float heightPixels,
            Vector2 offsetPixels,
            Color32 tint)
        {
            this.widthPixels = widthPixels;
            this.heightPixels = heightPixels;
            this.offsetPixels = offsetPixels;
            this.tint = tint;
        }

        public float WidthPixels => widthPixels;
        public float HeightPixels => heightPixels;
        public Vector2 SizePixels => new Vector2(widthPixels, heightPixels);
        public Vector2 OffsetPixels => offsetPixels;
        public Color32 Tint => tint;

        public static BattleFootMarkerStyle Default => new BattleFootMarkerStyle(
            128f,
            48f,
            Vector2.zero,
            new Color32(255, 255, 255, 255));

        internal BattleFootMarkerStyle Normalized()
        {
            return new BattleFootMarkerStyle(
                Mathf.Max(1f, widthPixels),
                Mathf.Max(1f, heightPixels),
                offsetPixels,
                tint);
        }
    }
}
