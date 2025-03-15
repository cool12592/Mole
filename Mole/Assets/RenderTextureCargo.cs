using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureCargo : MonoBehaviour
{
    public RenderTexture sourceTexture;
    public RenderTexture targetTexture;
    public RenderTexture tempTexture;
    public Material blitMaterial;

    private void Awake()
    {
        RenderTexture.active = targetTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = null;

        RenderTexture.active = tempTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = null;
    }
}
