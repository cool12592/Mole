using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDetector : MonoBehaviour
{
    public event Action<GameObject> OnMeshCollide;
    [SerializeField] BoxCollider boxCol;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("RenderTexture") && other.gameObject.layer != LayerMask.NameToLayer("FinishRenderTexture"))
            return;
        if(IsCompletelyCovered2D(other))
            OnMeshCollide?.Invoke(other.gameObject);
    }



    public bool IsCompletelyCovered2D(Collider other)
    {
        Vector3 size = boxCol.size;
        Vector3 center = boxCol.center;
        Transform t = boxCol.transform;

        // 꼭짓점 4개 계산 (z는 무시)
        Vector2[] corners = new Vector2[4];
        corners[0] = To2D(t.TransformPoint(center + new Vector3(-size.x, -size.y, 0) * 0.5f)); // 좌하
        corners[1] = To2D(t.TransformPoint(center + new Vector3(-size.x, size.y, 0) * 0.5f));  // 좌상
        corners[2] = To2D(t.TransformPoint(center + new Vector3(size.x, size.y, 0) * 0.5f));   // 우상
        corners[3] = To2D(t.TransformPoint(center + new Vector3(size.x, -size.y, 0) * 0.5f));  // 우하

        foreach (Vector2 corner in corners)
        {
            // z축은 0으로 고정해서 2D처럼 OverlapPoint 체크
            Vector3 point3D = new Vector3(corner.x, corner.y, other.transform.position.z);
            if (!other.bounds.Contains(point3D))
            {
                return false; // 하나라도 안 겹치면 false
            }
        }

        return true; // 전부 겹치면 true
    }

    private Vector2 To2D(Vector3 pos)
    {
        return new Vector2(pos.x, pos.y);
    }
}
