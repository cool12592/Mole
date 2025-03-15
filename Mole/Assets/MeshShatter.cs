using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshShatter : MonoBehaviour
{
    public int rows = 4; // 가로 조각 개수
    public int cols = 4; // 세로 조각 개수
    public float explosionForce = 5f; // 조각들이 튀는 힘
    public float spread = 1f; // 조각 퍼짐 정도
    public int circleSegments = 12; // 원형 조각의 정밀도 (삼각형 개수)

    private Mesh originalMesh;
    private Vector3[] originalVertices;
    private int[] originalTriangles;
    private Material mat;
    private GameObject fakeMesh;

    public void Init(Material mat_, GameObject fakeMesh_)
    {
        mat = mat_;
        fakeMesh = fakeMesh_;
        StartCoroutine(CoStart());
    }

    private IEnumerator CoStart()
    {
        yield return new WaitForSeconds(0.1f);
        Shatter();
    }

    public void Shatter()
    {
        // 1️⃣ 원본 Mesh 저장
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.mesh == null)
        {
            Debug.LogError("Mesh가 없습니다!");
            return;
        }

        originalMesh = meshFilter.mesh;
        originalVertices = originalMesh.vertices;
        originalTriangles = originalMesh.triangles;

        Bounds bounds = originalMesh.bounds;
        float pieceWidth = bounds.size.x / cols;
        float pieceHeight = bounds.size.y / rows;

        // 2️⃣ Mesh를 일정한 크기로 분할
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                CreateMeshPiece(bounds, x, y, pieceWidth, pieceHeight);
            }
        }

        // 3️⃣ 원본 Mesh 숨김
        fakeMesh.SetActive(false);
    }

    void CreateMeshPiece(Bounds bounds, int x, int y, float width, float height)
    {
        // 4️⃣ 조각을 위한 새로운 GameObject 생성
        GameObject piece = new GameObject("Piece_" + x + "_" + y);
        piece.transform.position = transform.position;

        MeshFilter mf = piece.AddComponent<MeshFilter>();
        MeshRenderer mr = piece.AddComponent<MeshRenderer>();
        mr.material = mat; // 기존 머티리얼 적용

        // 5️⃣ 원형 조각 Mesh 생성
        Mesh newMesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // 6️⃣ 원형 조각 중심 위치
        float centerX = bounds.min.x + (x + 0.5f) * width;
        float centerY = bounds.min.y + (y + 0.5f) * height;
        Vector3 center = new Vector3(centerX, centerY, 0);

        vertices.Add(center); // 중심점 추가
        uvs.Add(new Vector2(0.5f, 0.5f)); // 중심 UV

        for (int i = 0; i <= circleSegments; i++)
        {
            float angle = (i / (float)circleSegments) * Mathf.PI * 2f;

            // ✅ 랜덤한 변형 추가 (0.8 ~ 1.2 배율 조정)
            float randomFactor = Random.Range(0.8f, 1.2f);
            float vx = centerX + Mathf.Cos(angle) * (width / 2f) * randomFactor;
            float vy = centerY + Mathf.Sin(angle) * (height / 2f) * randomFactor;
            vertices.Add(new Vector3(vx, vy, 0));

            float uvX = 0.5f + Mathf.Cos(angle) * 0.5f * randomFactor;
            float uvY = 0.5f + Mathf.Sin(angle) * 0.5f * randomFactor;
            uvs.Add(new Vector2(uvX, uvY));

            if (i > 0)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }
        }

        newMesh.vertices = vertices.ToArray();
        newMesh.triangles = triangles.ToArray();
        newMesh.uv = uvs.ToArray();
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();

        mf.mesh = newMesh;

        // 9️⃣ Rigidbody2D 추가해서 물리 적용
        Rigidbody2D rb = piece.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        rb.AddForce(new Vector2(Random.Range(-spread, spread), Random.Range(0, spread)) * explosionForce, ForceMode2D.Impulse);

        //Destroy(piece, 2f); // 2초 후 자동 삭제
    }
}
