using System;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Animation
{
    // Alignment contract: NTSD-M12-D0-BASE-PATCH-001.
    public sealed class BattleSpriteReplacementFrame
    {
        internal const int MaximumFrames = 34;
        internal const int MaterialVariant = 1001;
        public Material CentralMaterial { get; }
        public Material LegacyMaterial { get; }
        public int Index { get; }
        internal Texture2D Texture { get; }
        internal Rect BaseRect { get; }

        internal readonly struct Patch
        {
            internal readonly Rect Source, Destination;
            internal Patch(Rect source, Rect destination) { Source = source; Destination = destination; }
        }

        private BattleSpriteReplacementFrame(Material central, Material legacy, int index, Texture2D texture, Rect baseRect)
        {
            CentralMaterial = central; LegacyMaterial = legacy; Index = index; Texture = texture; BaseRect = baseRect;
        }

        internal static BattleSpriteReplacementFrame[] Create(Texture2D texture, Rect baseRect,
            IReadOnlyList<Patch> patches, ISet<UnityEngine.Object> owned)
        {
            if (texture == null || patches == null || patches.Count == 0 || patches.Count > MaximumFrames || owned == null)
                throw new ArgumentException("Invalid replacement resource ownership or frame count.");
            Shader shader = Resources.Load<Shader>("BattleD0Replacement");
            if (shader == null || !shader.isSupported)
                throw new InvalidOperationException("D0 RGBA replacement shader is unavailable.");
            var sources = new Vector4[MaximumFrames];
            var destinations = new Vector4[MaximumFrames];
            for (int i = 0; i < patches.Count; i++)
            {
                Rect s = patches[i].Source, d = patches[i].Destination;
                sources[i] = new Vector4(s.x, s.y, s.width, s.height);
                destinations[i] = new Vector4(d.x, d.y, d.width, d.height);
            }
            Material NewMaterial(int frame)
            {
                var material = new Material(shader) { name = "D0Replacement_" + frame, hideFlags = HideFlags.HideAndDontSave };
                owned.Add(material);
                material.SetTexture("_MainTex", texture);
                material.SetVector("_CompositeBaseRect", new Vector4(baseRect.x, baseRect.y, baseRect.width, baseRect.height));
                material.SetVectorArray("_PatchSources", sources);
                material.SetVectorArray("_PatchDestinations", destinations);
                material.SetFloat("_CompositeFrameOverride", frame);
                return material;
            }
            Material central = NewMaterial(-1);
            var result = new BattleSpriteReplacementFrame[patches.Count];
            for (int i = 0; i < result.Length; i++)
                result[i] = new BattleSpriteReplacementFrame(central, NewMaterial(i), i, texture, baseRect);
            return result;
        }
    }
}
