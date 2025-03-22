using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureCargo : MonoBehaviour
{
    public RenderTexture targetTexture;
    public Material blitMaterial;

    private void Awake()
    {
        RenderTexture.active = targetTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = null;
    }
}
