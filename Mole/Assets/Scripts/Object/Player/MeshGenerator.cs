using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MeshGenerator : MonoBehaviourPunCallbacks
{
    [SerializeField] private Material meshMaterial; //  에디터에서 머터리얼 설정
    private GameObject meshObj;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    [SerializeField] LayerMask changeLayer;

    List<Vector2> posList = new List<Vector2>();
    [SerializeField] GameObject _recordObj;
    public PhotonView PV;
    private Button dashBtn;

    float dustTimer = 0f;
    float flipTimer = 0f;

    Vector3 lastPos;

    Action OnGenerateMesh;
    [SerializeField] FallingGround _fallingGround;
    [SerializeField] TextureAdd _textureADD;
    [SerializeField] bool inHouse = true;
    [SerializeField] int _curPointCount = 0;

    [SerializeField] GameObject _dust;
    [SerializeField] SpriteRenderer _dustRockSR;

    Road lastExitRoad;
    Road lastEnterRoad;

    HashSet<Collider2D> _myRoadSet = new HashSet<Collider2D>();
    HashSet<GameObject> _myMeshSet = new HashSet<GameObject>();

    [SerializeField] GamePalette palette;  // 팔레트 오브젝트 (씬에 있어야 함)
    Color myColor;

    void AssignColor()
    {
        if (PV.IsMine == false) 
            return;

        // 🔴 현재 방의 색상 인덱스를 가져옴
        var roomProps = PhotonNetwork.CurrentRoom.CustomProperties;
        int nextColorIndex = 0;

        if (roomProps.ContainsKey("NextColorIndex"))
        {
            nextColorIndex = (int)roomProps["NextColorIndex"];
        }
        
        // 🔴 본인의 색상을 모든 유저에게 전파
        PV.RPC("RPC_SyncColor", RpcTarget.AllBuffered, nextColorIndex);

        // 🔴 색상 인덱스를 증가 (최대값 넘으면 0으로 초기화)
        int newColorIndex = (nextColorIndex + 1) % palette.MaxColors;

        // 🔴 방 속성 업데이트 (다음 입장 플레이어를 위한 색상 인덱스 변경)
        var newProps = new ExitGames.Client.Photon.Hashtable { { "NextColorIndex", newColorIndex } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(newProps);
    }

    [PunRPC]
    void RPC_SyncColor(int colorIndex)
    {
        var newColor = palette.GetColor(colorIndex);
        myColor = newColor;
        GetComponent<playerScript>().NickNameText.color = myColor;
       
    }


    private void Awake()
    {
        PV = GetComponent<PhotonView>();

        AssignColor();

        dashBtn = GameObject.Find("Canvas").transform.Find("DashButton").gameObject.GetComponent<Button>();
        dashBtn.onClick.AddListener(GenerateMeshObject);

        _fallingGround = GameObject.Find("FallingGround").GetComponent<FallingGround>();
       
        _fallingGround = Instantiate(_fallingGround);
        _fallingGround.GetComponent<SpriteRenderer>().enabled = true;
        _fallingGround.gameObject.SetActive(false);
        _fallingGround.GetComponent<SpriteRenderer>().sortingOrder = 0;

        
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => myColor != default(Color));

        Vector3[] positions =
        {
            new Vector3(-1f, 1f, 0f), new Vector3(0f, 1f, 0f), new Vector3(1f, 1f, 0f),
            new Vector3(-1f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(1f, 0f, 0f),
            new Vector3(-1f, -1f, 0f), new Vector3(0f, -1f, 0f), new Vector3(1f, -1f, 0f)
        };

        foreach (Vector3 offset in positions)
        {
            CreateLoad(transform.position + offset);
        }

        _curPointCount = 100;
        posList.Add(transform.position + new Vector3(-1f, -1f, 0f));
        posList.Add(transform.position + new Vector3(-1f, 1f, 0f));
        posList.Add(transform.position + new Vector3(1f, 1f, 0f));
        posList.Add(transform.position + new Vector3(1f, -1f, 0f));

        yield return null;
        GenerateMeshObject();

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_myRoadSet.Contains(collision) == false)
            return;

        if (inHouse)
            return;

        if (collision.gameObject.layer != LayerMask.NameToLayer("FinishRoad"))
            return;

        lastEnterRoad = collision.GetComponent<Road>();

        GenerateMeshObject();


    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_myRoadSet.Contains(collision) == false)
            return;

        if (collision.gameObject.layer != LayerMask.NameToLayer("FinishRoad"))
            return;

        lastExitRoad = collision.GetComponent<Road>();

        _curPointCount = 0;
    }

    public void OnTriggerEnter3D(Collider other)
    {
       // if (_myMeshSet.Contains(other))
         //   return;

        // if (other.gameObject.layer != LayerMask.NameToLayer("RenderTexture") && other.gameObject.layer != LayerMask.NameToLayer("FinishRenderTexture"))
        //     return;

        // inHouse = true;


    }

    public void OnTriggerStay3D(Collider other)
    {
        if (_myMeshSet.Contains(other.gameObject)==false)
            return;

        if (other.gameObject.layer != LayerMask.NameToLayer("RenderTexture") && other.gameObject.layer != LayerMask.NameToLayer("FinishRenderTexture"))
            return;

        inHouse = true;
        _dust.SetActive(false);

    }

    public void OnTriggerExit3D(Collider other)
    {
        if (_myMeshSet.Contains(other.gameObject) == false)
            return;

        if (other.gameObject.layer != LayerMask.NameToLayer("RenderTexture") && other.gameObject.layer != LayerMask.NameToLayer("FinishRenderTexture"))
            return;
        inHouse = false;
        posList.Clear();

        DeactiveDust();
    }

    void DeactiveDust()
    {
        flipTimer = 0f;
        _dust.SetActive(false);
    }

    void ActiveDust()
    {
        dustTimer = 0f;
        _dust.SetActive(true);

        flipTimer += Time.deltaTime;
        if (0.02f < flipTimer)
        {
            _dustRockSR.flipX = !_dustRockSR.flipX;
            flipTimer = 0f;
        }
    }

    double count = 0;

    private void Update()
    {

        if (inHouse)
        {
            GetComponent<SpriteRenderer>().color = Color.white;
            return;
        }

       // GetComponent<SpriteRenderer>().color = Color.red;

        if (transform.position == lastPos)
        {
            dustTimer += Time.deltaTime;
            if (0.1f < dustTimer)
            {
                DeactiveDust();
            }
            return;
        }

        ActiveDust();
        
        lastPos = transform.position;

        count++;
       // if(0.1f<timer)
        {
            if (count % 5 == 0)
            {
                _curPointCount++;
                posList.Add(new Vector2(transform.position.x, transform.position.y));
            }

            dustTimer = 0f;
            CreateLoad(transform.position);
        }
    }

    void CreateLoad(Vector3 pos)
    {
        if (PV.IsMine == false)
            return;
        pos.z = GetSharedFloat();

        photonView.RPC("CreateLoad_RPC", RpcTarget.AllBuffered, pos.x, pos.y, pos.z);
    }

    [PunRPC]
    void CreateLoad_RPC(float x, float y, float z)
    {
        Vector3 pos = new Vector3(x, y, z);
        var road = Instantiate(_recordObj, pos, Quaternion.identity).GetComponent<Road>();
        road.GetComponent<SpriteRenderer>().color = myColor;
        _myRoadSet.Add(road.collider_);
        OnGenerateMesh += road.ChangeLayer;
    }

    void GenerateMeshObject()
    {
        if (PV.IsMine == false)
            return;

        if (_curPointCount < 2)
            return;

        CreateLoad(transform.position);

        originLastIndex = posList.Count - 1;
        if (lastExitRoad != null && lastEnterRoad != null)
            CastRaysAlongLine();

        float z = GetSharedFloat();


        photonView.RPC("SyncPosListAndGenerateMesh_RPC", RpcTarget.AllBuffered, posList.ToArray(),z);
        photonView.RPC("PostGenerateMesh_RPC", RpcTarget.All);

    }

    [Header("hmm")]

    public Vector2 pointA;   // 시작 점
    public Vector2 pointB;   // 끝 점
    public float spacing = 0.2f;  // 점 간격
    public float rayLength = 1000f;  // 레이 길이
    public LayerMask hitLayer;    // 충돌 레이어 설정

    void CastRaysAlongLine()
    {
        pointA = lastExitRoad.gameObject.transform.position;
        pointB = lastEnterRoad.gameObject.transform.position;
        Vector2 direction = (pointB - pointA).normalized; // 선분 방향
        float length = Vector2.Distance(pointA, pointB);  // 총 길이
        int numPoints = Mathf.FloorToInt(length / spacing); // 찍을 점 개수

        for (int i = 0; i <= numPoints; i++)
        {
            Vector2 point = pointA + direction * (i * spacing); // 선분 위 점

            // 수직 방향 2개 (왼쪽, 오른쪽)
            Vector2 perpDirection1 = new Vector2(-direction.y, direction.x); // 시계 방향 90도 회전
            Vector2 perpDirection2 = new Vector2(direction.y, -direction.x); // 반시계 방향 90도 회전

            // 첫 번째 수직 방향으로 레이 쏘기
            RaycastHit2D hit1 = Physics2D.Raycast(point, perpDirection1, rayLength, hitLayer);
            Debug.DrawRay(point, perpDirection1 * rayLength, Color.red, 1f);

            if (hit1.collider != null)
            {
                posList.Add(hit1.collider.transform.position);

               // GameObject go = new GameObject("test1");
               // go.transform.position = hit1.collider.transform.position;
            }

            // 두 번째 수직 방향으로 레이 쏘기
            RaycastHit2D hit2 = Physics2D.Raycast(point, perpDirection2, rayLength, hitLayer);
            Debug.DrawRay(point, perpDirection2 * rayLength, Color.blue, 1f);

            if (hit2.collider != null)
            {
                posList.Add(hit2.collider.transform.position);
              //  GameObject go = new GameObject("test2");
               // go.transform.position = hit1.collider.transform.position;

            }
        }
    }

    void CreateMesh()
    {
        if (meshObj == null)
        {
            meshObj = new GameObject("GeneratedMesh");
            meshCollider = meshObj.AddComponent<MeshCollider>();
            _myMeshSet.Add(meshObj);
            meshFilter = meshObj.AddComponent<MeshFilter>();
            meshRenderer = meshObj.AddComponent<MeshRenderer>();
            meshRenderer.material = meshMaterial;
            meshRenderer.material.color = myColor;

        }
        else
        {
            meshObj.layer = LayerMask.NameToLayer("FinishRenderTexture");
            meshObj = Instantiate(meshObj);
            meshObj.name = "GeneratedMesh";
            meshCollider = meshObj.GetComponent<MeshCollider>();
            _myMeshSet.Add(meshObj);
            meshFilter = meshObj.GetComponent<MeshFilter>();
            meshRenderer = meshObj.GetComponent<MeshRenderer>();
            meshRenderer.material = meshMaterial;
            meshRenderer.material.color = myColor;

            meshObj.AddComponent<MeshShatter>().Init(groundPieceMat, _fallingGround.gameObject);
        }

        meshObj.transform.position = Vector3.zero;

    }

    [SerializeField] Material groundPieceMat;

    int originLastIndex = 0;
    /// <summary>
    /// 정점 리스트를 받아서 이동 경로 Mesh를 생성
    /// </summary>
    /// <param name="verticesList">Mesh를 구성할 정점 리스트</param>
    [PunRPC]
    private void SyncPosListAndGenerateMesh_RPC(Vector2[] receivedPosList, float z)
    {
       

        // 🔥 받은 posList로 동기화
        posList = new List<Vector2>(receivedPosList);

        CreateMesh();

        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        //  정점 리스트를 배열로 변환
        Vector3[] vertices = new Vector3[posList.Count];
        for (int i = 0; i < posList.Count; i++)
        {
            vertices[i] = new Vector3(posList[i].x, posList[i].y, 0);
        }

        //  삼각형 인덱스 자동 생성
        List<int> triangles = new List<int>();
        for (int i = 1; i < vertices.Length - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }

        for (int i = 1; i < vertices.Length - 1; i++)
        {
            triangles.Add(originLastIndex);
            triangles.Add(i);
            triangles.Add(i + 1);
        }

        // ✅ Mesh 데이터 적용
        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        meshCollider.sharedMesh = mesh;
        Debug.Log("Mesh 생성 완료! 정점 개수: " + vertices.Length);

        // 🔥 위치 및 레이어 동기화


        meshObj.transform.position = new Vector3(meshObj.transform.position.x, meshObj.transform.position.y, z);
        meshObj.layer = Mathf.RoundToInt(Mathf.Log(changeLayer.value, 2));

        _curPointCount = 0;
        posList.Clear();
    }

    bool first = true;

    [PunRPC]
    void PostGenerateMesh_RPC()
    {
        StartCoroutine(CoPostGenerateMesh());
    }

    IEnumerator CoPostGenerateMesh()
    {
        yield return null;
        _fallingGround.BlendRenderTextures();
        yield return null;

        _textureADD.BlendRenderTextures();

        OnGenerateMesh?.Invoke();
        OnGenerateMesh = null;

        if (first)
        {
            first = false;
            yield break;
        }
        _fallingGround.StartFalling();

       // yield return null;
    }


    static float sharedFloat = 0f; // 🔴 공유할 float 값 (초기값 100)

    public float GetSharedFloat()
    {
        PV.RPC("RPC_DecreaseSharedFloat", RpcTarget.AllBuffered); // 🔴 값 감소 요청
        return sharedFloat; // 🔴 로컬 값 반환 (즉시 반영)
    }

    [PunRPC]
    void RPC_DecreaseSharedFloat()
    {
        sharedFloat -= 0.001f; // 🔴 모든 클라이언트에서 sharedFloat 값을 감소
    }
}
