using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDetector : MonoBehaviour
{
    public event Action<GameObject> OnMeshCollide;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("RenderTexture") && other.gameObject.layer != LayerMask.NameToLayer("FinishRenderTexture"))
            return;

        OnMeshCollide?.Invoke(other.gameObject);
    }
}
