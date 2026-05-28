using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteShadowCaster : MonoBehaviour
{
    void Start()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.shadowCastingMode = ShadowCastingMode.TwoSided;
        spriteRenderer.receiveShadows = true;
    }
}
