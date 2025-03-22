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
    const int maxNum = 9999999;
    Sprite[] spriteFragments; // Inspector에서 원하는 스프라이트 조각 배열 등록

    void CreateMeshPiece(Bounds bounds, int x, int y, float width, float height)
    {
        // 6️⃣ 원형 조각 중심 위치 (정확한 중간 위치 보정)
        float centerX;
        float centerY;

        if (x >= maxNum)
        {
            centerX = bounds.min.x + ((x - maxNum) + 1f) * width;
            centerY = bounds.min.y + y * height;
        }
        else if (y >= maxNum)
        {
            centerX = bounds.min.x + x * width;
            centerY = bounds.min.y + ((y - maxNum) + 1f) * height;
        }
        else
        {
            centerX = bounds.min.x + (x + 0.5f) * width;
            centerY = bounds.min.y + (y + 0.5f) * height;
        }

        Vector3 center = new Vector3(centerX, centerY, 0);

        SpritePiece piece = GlobalSpritePool.Instance.GetPiece(center);
        piece.spriteRenderer.color = Color.white;

        // ✅ 원하는 스프라이트 랜덤하게 할당 (또는 순서대로)
        if (spriteFragments != null && spriteFragments.Length > 0)
        {
            int idx = Random.Range(0, spriteFragments.Length);
            piece.spriteRenderer.sprite = spriteFragments[idx];
        }

        // 💡 스케일 조정 (필요 시)

        float pieceScale = Mathf.Min(width, height);
        pieceScale = Mathf.Min(pieceScale, maxPieceScale);

        // ✅ 랜덤 오프셋 적용 (예: ±15%)
        float scaleOffset = Random.Range(0.5f, 1.2f);
        piece.transform.localScale = Vector3.one * pieceScale * scaleOffset;


        // 💨 물리 적용
        piece.rigid.gravityScale = 1f;
        piece.rigid.AddForce(new Vector2(Random.Range(-spread, spread), Random.Range(0, spread)) * explosionForce, ForceMode2D.Impulse);

        GlobalSpritePool.Instance.Release(piece, 2f); // 2초 후 자동 삭제z
    }


    public void Init(Sprite[] spriteFragments_)
    {
        spriteFragments = spriteFragments_;
        StartCoroutine(CoStart());
    }

    private IEnumerator CoStart()
    {
        yield return new WaitForSeconds(0.1f);
        Shatter();
    }

    public float maxPieceScale = 4f; // ✨ 조각 최대 스케일 설정


    public void Shatter()
    {
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

        // ✅ 조각 수 자동 조절
        float targetWidth = bounds.size.x / cols;
        float targetHeight = bounds.size.y / rows;

        if (targetWidth > maxPieceScale || targetHeight > maxPieceScale)
        {
            cols = Mathf.CeilToInt(bounds.size.x / maxPieceScale);
            rows = Mathf.CeilToInt(bounds.size.y / maxPieceScale);
        }

        float pieceWidth = bounds.size.x / cols;
        float pieceHeight = bounds.size.y / rows;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                CreateMeshPiece(bounds, x, y, pieceWidth, pieceHeight);
            }
        }
    }

    
    //    void CreateMeshPiece22(Bounds bounds, int x, int y, float width, float height)
    //{
    //    // 4️⃣ 조각을 위한 새로운 GameObject 생성
    //    GameObject piece = new GameObject("Piece_" + x + "_" + y);
    //    piece.transform.position = transform.position;

    //    MeshFilter mf = piece.AddComponent<MeshFilter>();
    //    MeshRenderer mr = piece.AddComponent<MeshRenderer>();
    //    mr.material = mat; // 기존 머티리얼 적용

    //    // 5️⃣ 원형 조각 Mesh 생성
    //    Mesh newMesh = new Mesh();
    //    List<Vector3> vertices = new List<Vector3>();
    //    List<int> triangles = new List<int>();
    //    List<Vector2> uvs = new List<Vector2>();

    //    // 6️⃣ 원형 조각 중심 위치 (정확한 중간 위치 보정)
    //    float centerX;
    //    float centerY;

    //    if (x >= maxNum)
    //    {
    //        centerX = bounds.min.x + ((x - maxNum) + 1f) * width;
    //        centerY = bounds.min.y + y * height;
    //    }
    //    else if (y >= maxNum)
    //    {
    //        centerX = bounds.min.x + x * width;
    //        centerY = bounds.min.y + ((y - maxNum) + 1f) * height;
    //    }
    //    else
    //    {
    //        centerX = bounds.min.x + (x + 0.5f) * width;
    //        centerY = bounds.min.y + (y + 0.5f) * height;
    //    }

    //    Vector3 center = new Vector3(centerX, centerY, 0);

    //    vertices.Add(center); // 중심점 추가
    //    uvs.Add(new Vector2(0.5f, 0.5f)); // 중심 UV

    //    for (int i = 0; i <= circleSegments; i++)
    //    {
    //        float angle = (i / (float)circleSegments) * Mathf.PI * 2f;

    //        // ✅ 랜덤한 변형 추가 (0.8 ~ 1.3 배율 조정)
    //        float randomFactor = Random.Range(0.8f, 1.3f);
    //        float vx = centerX + Mathf.Cos(angle) * (width / 2f) * randomFactor;
    //        float vy = centerY + Mathf.Sin(angle) * (height / 2f) * randomFactor;
    //        vertices.Add(new Vector3(vx, vy, 0));

    //        float uvX = 0.5f + Mathf.Cos(angle) * 0.5f * randomFactor;
    //        float uvY = 0.5f + Mathf.Sin(angle) * 0.5f * randomFactor;
    //        uvs.Add(new Vector2(uvX, uvY));

    //        if (i > 0)
    //        {
    //            triangles.Add(0);
    //            triangles.Add(i);
    //            triangles.Add(i + 1);
    //        }
    //    }

    //    newMesh.vertices = vertices.ToArray();
    //    newMesh.triangles = triangles.ToArray();
    //    newMesh.uv = uvs.ToArray();
    //    newMesh.RecalculateNormals();
    //    newMesh.RecalculateBounds();

    //    mf.mesh = newMesh;

    //    // 9️⃣ Rigidbody2D 추가해서 물리 적용
    //    Rigidbody2D rb = piece.AddComponent<Rigidbody2D>();
    //    rb.gravityScale = 1f;
    //    rb.AddForce(new Vector2(Random.Range(-spread, spread), Random.Range(0, spread)) * explosionForce, ForceMode2D.Impulse);

    //    Destroy(piece, 2f);


    //}

}
