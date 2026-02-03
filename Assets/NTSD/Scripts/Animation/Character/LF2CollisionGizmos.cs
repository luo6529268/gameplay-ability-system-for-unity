using MoreMountains.TopDownEngine;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// Debug only: draw FLF-style BDY/ITR volumes using Unity Gizmos.
    /// Kept out of LF2CharacterAnimator to avoid mixing runtime logic with editor visualization.
    /// </summary>
    public sealed class LF2CollisionGizmos : MonoBehaviour
    {
        [Header("Debug / Collision Volumes")]
        [SerializeField]
        private bool _drawBodyVolumes = false;

        [SerializeField]
        private bool _drawItrVolumes = false;

        //private LF2CharacterAnimator _animator;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            CacheComponents();
        }

        private void OnValidate()
        {
            CacheComponents();
        }

        private void CacheComponents()
        {
            //if (_animator == null) _animator = GetComponent<LF2CharacterAnimator>();
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private Transform GetGroundTransform()
        {
            if (transform.parent != null) return transform.parent;
            return transform;
        }

        private float GetCurrentSpriteWidthPx()
        {
            if (_spriteRenderer == null || _spriteRenderer.sprite == null) return 0f;
            return _spriteRenderer.sprite.textureRect.width;
        }

        private void OnDrawGizmosSelected()
        {
            //if (!_drawBodyVolumes && !_drawItrVolumes) return;
            //if (_animator == null || _animator.ps == null) return;
            //if (_animator.CurrentFrame == null) return;

            //float spriteWidthPx = GetCurrentSpriteWidthPx();
            //if (spriteWidthPx <= 0f) return;

            //float ppu = SimulationConstants.PIXELS_PER_UNIT;
            //float planeZ = GetGroundTransform().position.z;

            //if (_drawBodyVolumes)
            //{
            //    var bodyVolumes = _animator.ps.GetBodyVolumes(
            //        _animator.CurrentFrame.bodies,
            //        _animator.CurrentFrame.centerx,
            //        _animator.CurrentFrame.centery,
            //        spriteWidthPx
            //    );

            //    Gizmos.color = Color.yellow;
            //    foreach (var v in bodyVolumes)
            //    {
            //        float leftPx = v.x + v.vx;
            //        float topPx = v.y + v.vy;
            //        float wPx = v.w;
            //        float hPx = v.h;

            //        float centerX = (leftPx + wPx * 0.5f) / ppu;
            //        float centerY = (_animator.ps.z - (topPx + hPx * 0.5f)) / ppu;

            //        float sizeX = Mathf.Max(0.001f, wPx / ppu);
            //        float sizeY = Mathf.Max(0.001f, hPx / ppu);
            //        float sizeZ = Mathf.Max(0.001f, (v.zwidth * 2f) / ppu);

            //        Gizmos.DrawWireCube(new Vector3(centerX, centerY, planeZ), new Vector3(sizeX, sizeY, sizeZ));
            //    }
            //}

            //if (_drawItrVolumes)
            //{
            //    var itrVolumes = _animator.ps.GetItrVolumes(
            //        _animator.CurrentFrame.itrs,
            //        _animator.CurrentFrame.centerx,
            //        _animator.CurrentFrame.centery,
            //        spriteWidthPx,
            //        itrZWidthPx: 0f
            //    );

            //    Gizmos.color = Color.red;
            //    foreach (var v in itrVolumes)
            //    {
            //        float leftPx = v.x + v.vx;
            //        float topPx = v.y + v.vy;
            //        float wPx = v.w;
            //        float hPx = v.h;

            //        float centerX = (leftPx + wPx * 0.5f) / ppu;
            //        float centerY = (_animator.ps.z - (topPx + hPx * 0.5f)) / ppu;

            //        float sizeX = Mathf.Max(0.001f, wPx / ppu);
            //        float sizeY = Mathf.Max(0.001f, hPx / ppu);
            //        float sizeZ = Mathf.Max(0.001f, (v.zwidth * 2f) / ppu);

            //        Gizmos.DrawWireCube(new Vector3(centerX, centerY, planeZ), new Vector3(sizeX, sizeY, sizeZ));
            //    }
            //}
        }
    }
}

