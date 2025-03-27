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
        
        OnMeshCollide?.Invoke(other.gameObject);
    }



    // public bool IsCompletelyCovered2D(Collider other)
    // {
    //     Vector3 size = boxCol.size;
    //     Vector3 center = boxCol.center;
    //     Transform t = boxCol.transform;

    //     // ������ 4�� ��� (z�� ����)
    //     Vector2[] corners = new Vector2[4];
    //     corners[0] = To2D(t.TransformPoint(center + new Vector3(-size.x, -size.y, 0) * 0.5f)); // ����
    //     corners[1] = To2D(t.TransformPoint(center + new Vector3(-size.x, size.y, 0) * 0.5f));  // �»�
    //     corners[2] = To2D(t.TransformPoint(center + new Vector3(size.x, size.y, 0) * 0.5f));   // ���
    //     corners[3] = To2D(t.TransformPoint(center + new Vector3(size.x, -size.y, 0) * 0.5f));  // ����

    //     foreach (Vector2 corner in corners)
    //     {
    //         // z���� 0���� �����ؼ� 2Dó�� OverlapPoint üũ
    //         Vector3 point3D = new Vector3(corner.x, corner.y, other.transform.position.z);
    //         if (!other.bounds.Contains(point3D))
    //         {
    //             return false; // �ϳ��� �� ��ġ�� false
    //         }
    //     }

    //     return true; // ���� ��ġ�� true
    // }

    // private Vector2 To2D(Vector3 pos)
    // {
    //     return new Vector2(pos.x, pos.y);
    // }
}
