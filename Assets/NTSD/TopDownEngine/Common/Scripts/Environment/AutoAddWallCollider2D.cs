using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAddWallCollider2D : TopDownMonoBehaviour
{
    private List<SpriteRenderer> spriteRendererList;

    protected virtual void Awake() 
    {
        spriteRendererList = new List<SpriteRenderer> ();
    }

    protected virtual void OnEnable()
    {
        this.transform.GetComponentsInChildren<SpriteRenderer> (spriteRendererList);

        for (int i = 0; i < spriteRendererList.Count; i++) 
        {
            SpriteRenderer spriteRenderer = spriteRendererList[i];
            spriteRenderer.gameObject.MMGetOrAddComponent<BoxCollider2D>();
        }
    }

}
