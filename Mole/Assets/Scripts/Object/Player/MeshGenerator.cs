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

    [SerializeField] SpriteRenderer _dust;
    [SerializeField] SpriteRenderer _dustRockSR;

    Vector3 lastExitTr;
    Vector3 lastEnterTr;
    Vector3 lastEnterDirection;

    HashSet<Collider2D> _myRoadSet = new HashSet<Collider2D>();
    HashSet<GameObject> _myMeshSet = new HashSet<GameObject>();

    HashSet<GameObject> _curInMyMeshSet = new HashSet<GameObject>();
    HashSet<GameObject> _curInOtherMeshSet = new HashSet<GameObject>();

    [SerializeField] GamePalette palette;  // 팔레트 오브젝트 (씬에 있어야 함)
    Color myColor;

    [SerializeField] Sprite pieceSprite;
    List<GameObject> removeReserveList = new List<GameObject>();
    [SerializeField] GameObject _dustParticle;

    [SerializeField] AudioSource _meshGenSound;
    [SerializeField] AudioSource _moveSound;

    public void AssignColor()
    {
        if (!PV.IsMine) return;

        // 현재 방의 속성 가져오기
        var roomProps = PhotonNetwork.CurrentRoom.CustomProperties;

        // 현재 플레이어의 ActorNumber 가져오기
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        int assignedColorIndex;

        // 🔴 이미 색상이 할당된 경우 기존 값 유지
        if (roomProps.ContainsKey($"Color_{actorNumber}"))
        {
            assignedColorIndex = (int)roomProps[$"Color_{actorNumber}"];
        }
        else
        {
            // 🔴 방의 다음 색상 인덱스 가져오기
            int nextColorIndex = 0;
            if (roomProps.ContainsKey("NextColorIndex"))
            {
                nextColorIndex = (int)roomProps["NextColorIndex"];
            }

            // 🔴 현재 유저에게 색상 할당
            assignedColorIndex = nextColorIndex;

            // 🔴 다음 색상 인덱스 업데이트 (최대값 넘으면 0으로 초기화)
            int newColorIndex = (nextColorIndex + 1) % palette.MaxColors;

            // 🔴 방 속성 업데이트 (다음 플레이어를 위한 값 저장)
            var newProps = new ExitGames.Client.Photon.Hashtable
            {
                { $"Color_{actorNumber}", assignedColorIndex }, // 현재 플레이어의 색상 저장
                { "NextColorIndex", newColorIndex } // 다음 할당을 위한 값 갱신
            };
                PhotonNetwork.CurrentRoom.SetCustomProperties(newProps);
            }

            // 🔴 모든 유저에게 색상 동기화
            PV.RPC("RPC_SyncColor", RpcTarget.AllBuffered, assignedColorIndex);
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

        WriteFirstMeshPoint(20, 2);

        yield return null;
        GenerateMeshObject();

    }

    void WriteFirstMeshPoint(int segmentCount, float radius)
    {
        posList.Clear();
        Vector3 center = transform.position;

        for (int i = 0; i < segmentCount; i++)
        {
            float angle = (i / (float)segmentCount) * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            posList.Add(center + new Vector3(x, y, 0f));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Road"))
            return;

        if(collision.TryGetComponent<Road>(out Road road))
        {
            if (road._myOwner == null)
                return;

            if (road._myOwner == this)
                return;

            if (PV == null)
                return;

            road._myOwner.gameObject.GetComponent<PlayerHealth>().Death(PV.Owner.NickName);
        }
    }

    public void OnTriggerEnter3D(Collider other)
    {

    }
    public void OnTriggerStay3D(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("RenderTexture") && other.gameObject.layer != LayerMask.NameToLayer("FinishRenderTexture"))
            return;


        if (_myMeshSet.Contains(other.gameObject))
        {
            _curInMyMeshSet.Add(other.gameObject);
        }
        else
        {
            _curInOtherMeshSet.Add(other.gameObject);
            return;
        }
    }

    public void OnTriggerExit3D(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("RenderTexture") && other.gameObject.layer != LayerMask.NameToLayer("FinishRenderTexture"))
            return;

        if (_myMeshSet.Contains(other.gameObject))
        {
            _curInMyMeshSet.Remove(other.gameObject);
            posList.Clear();
        }
        else
        {
            _curInOtherMeshSet.Remove(other.gameObject);
            return;
        }
    }

    private void LateUpdate()
    {
        if(_curInMyMeshSet.Count == 0)
        {
            SetInHouse(false);
            return;
        }

        float minZ = 1f;
        foreach (GameObject go in _curInMyMeshSet)
        {
            if (go.transform.position.z < minZ)
            {
                minZ = go.transform.position.z;
            }
        }

        foreach (GameObject go in _curInOtherMeshSet)
        {
            if(go == null)
            {
                removeReserveList.Add(go);
                continue;
            }
            if (go.transform.position.z < minZ)
            {
                SetInHouse(false);
                return;
            }
        }

        foreach(var go in removeReserveList)
        {
            _curInOtherMeshSet.Remove(go);
        }
        removeReserveList.Clear();



        SetInHouse(true);
    }

    void SetInHouse(bool res)
    {
        if (res == inHouse)
            return;

        inHouse = res;

        if (inHouse)
        {
           // GetComponent<SpriteRenderer>().color = Color.white;
            //if(1 < posList.Count)
            {
                lastEnterTr = transform.position;
                lastEnterDirection = transform.up;
                GenerateMeshObject();
            }
        }
        else
        {
          //  GetComponent<SpriteRenderer>().color = Color.red;

            lastExitTr = transform.position;
            posList.Clear();
        }
    }

    void DeactiveDust()
    {
        flipTimer = 0f;
        _dust.gameObject.SetActive(false);
    }

    void ActiveDust()
    {
        if (inHouse)
            return;

        dustTimer = 0f;
        _dust.gameObject.SetActive(true);

        flipTimer += Time.deltaTime;
        if (0.02f < flipTimer)
        {
            _dust.flipY = !_dust.flipY;
            _dustRockSR.flipX = !_dustRockSR.flipX;
            flipTimer = 0f;
        }
    }

    int count = 0;

    private void Update()
    {

        if (inHouse)
        {
            DeactiveDust();
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
            bool shatter = false;
            if (count % 5 == 0)
            {
                posList.Add(new Vector2(transform.position.x, transform.position.y));
            }
            if (count%5==0)
            {
                shatter = true;
            }

            if(count%7==0)
            {
                _moveSound.volume = 0.3f;
                _moveSound.Play();
            }
            //if (count%20==0)
            //{
            //    _moveSound.Play();
            //}

            dustTimer = 0f;
            CreateLoad(transform.position, shatter);
        }
    }

    void CreateLoad(Vector3 pos, bool shatter = false)
    {
        if (PV.IsMine == false)
            return;

        pos += transform.up * 0.5f;
        pos.z = GetSharedFloat();

        photonView.RPC("CreateLoad_RPC", RpcTarget.AllBuffered, pos.x, pos.y, pos.z,shatter);
    }

    [PunRPC]
    void CreateLoad_RPC(float x, float y, float z,bool shatter)
    {
        Vector3 pos = new Vector3(x, y, z);
        var road = Instantiate(_recordObj, pos, Quaternion.identity).GetComponent<Road>();
        road.GetComponent<SpriteRenderer>().color = myColor;
        _myRoadSet.Add(road.collider_);
        OnGenerateMesh += road.ChangeLayer;
        road._myMeshSet = _myMeshSet;
        road._myOwner = this;

        if (shatter)
            road.gameObject.AddComponent<SpriteShatter>().Init(pieceSprite,transform.up*0.5f);
    }

    void GenerateMeshObject()
    {
        if (PV.IsMine == false)
            return;

       // CreateLoad(transform.position);

        originLastIndex = posList.Count - 1;
        if (lastEnterTr != Vector3.zero && lastEnterTr != Vector3.zero)
            CastRaysAlongLine();

        float z = GetSharedFloat();

        if (posList.Count < 3)
            return;

        photonView.RPC("SyncPosListAndGenerateMesh_RPC", RpcTarget.AllBuffered, posList.ToArray(),z);
        photonView.RPC("ShatterMesh_RPC", RpcTarget.All);

    }

    [Header("hmm")]

    public Vector2 pointA;   // 시작 점
    public Vector2 pointB;   // 끝 점
    public float spacing = 0.1f;  // 점 간격
    //public float rayLength = 1000f;  // 레이 길이
    public LayerMask hitLayer;    // 충돌 레이어 설정

    void CastRaysAlongLine()
{
    pointA = lastExitTr;
    pointB = lastEnterTr;
    Vector2 direction = (pointB - pointA).normalized; // 선분 방향
    float length = Vector2.Distance(pointA, pointB);  // 총 길이

    float NoLenth = 0.1f;
    // 시작과 끝에서 2 유닛씩 제외
    float adjustedLength = length - NoLenth*2f; 
    if (adjustedLength <= 0) return; // 길이가 4 이하라면 레이캐스트 실행 안 함

    Vector2 newPointA = pointA + direction * NoLenth; 
    Vector2 newPointB = pointB - direction * NoLenth; 

    int numPoints = Mathf.FloorToInt(adjustedLength / spacing); // 새로 찍을 점 개수

    for (int i = 0; i <= numPoints; i++)
    {
        Vector2 point = newPointA + direction * (i * spacing); // 새로운 선분 위 점

        // 수직 방향 2개 (왼쪽, 오른쪽)
        Vector2 perpDirection1 = new Vector2(-direction.y, direction.x).normalized; // 시계 방향 90도 회전
        Vector2 perpDirection2 = new Vector2(direction.y, -direction.x).normalized; // 반시계 방향 90도 회전

        float dot = Vector2.Dot(lastEnterDirection, perpDirection1);
        Vector2 chosenDirection = dot >= 0 ? perpDirection1 : perpDirection2;

        // 첫 번째 수직 방향으로 레이 쏘기
        RaycastHit2D[] hits = Physics2D.RaycastAll(point, chosenDirection, length, hitLayer);
        foreach (var hit in hits)
        {
            if (_myRoadSet.Contains(hit.collider))
            {
                posList.Add(hit.collider.transform.position + new Vector3(chosenDirection.x, chosenDirection.y, 0f) * 1f);
                break;
            }
        }

#if UNITY_EDITOR            
        Debug.DrawRay(point, chosenDirection * length, Color.red, 6f);
#endif
    }
}


    void CreateMesh()
    {
        if (meshObj == null)
        {
            meshObj = new GameObject("GeneratedMesh");
            _myMeshSet.Add(meshObj);

            meshCollider = meshObj.AddComponent<MeshCollider>();
            meshFilter = meshObj.AddComponent<MeshFilter>();
            meshRenderer = meshObj.AddComponent<MeshRenderer>();
            meshRenderer.material = meshMaterial;
            meshRenderer.material.color = myColor;

        }
        else
        {
            meshObj.layer = LayerMask.NameToLayer("FinishRenderTexture");
            meshObj = Instantiate(meshObj);
            _myMeshSet.Add(meshObj);

            meshObj.name = "GeneratedMesh";
            meshCollider = meshObj.GetComponent<MeshCollider>();
            meshFilter = meshObj.GetComponent<MeshFilter>();
            meshRenderer = meshObj.GetComponent<MeshRenderer>();
            meshRenderer.material = meshMaterial;
            meshRenderer.material.color = myColor;

        }

        meshObj.AddComponent<AttackMesh>().Init(PV.Owner.NickName);
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

        Vector3 sumVec = Vector3.zero;
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        for (int i = 0; i < posList.Count; i++)
        {
            float x = posList[i].x;
            float y = posList[i].y;

            vertices[i] = new Vector3(x, y, 0);
            sumVec += new Vector3(x, y, 0);

            // 최소/최대 좌표 업데이트
            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
        }

        Vector3 centerPos = sumVec / posList.Count; // 중심 좌표
        centerPos.z = -10f;
        float width = maxX - minX; // AABB 가로 길이
        float height = maxY - minY; // AABB 세로 길이
        float boundingBoxArea = width * height; // 사각형 넓이

        _meshGenSound.Play();
        GetComponent<PlayerMovement>().ShakeCamera();
        var particle = Instantiate(_dustParticle,centerPos,Quaternion.identity);
        //var particleScale = boundingBoxArea * 0.1f;
       // particle.transform.localScale *=  particleScale;
       // particle.transform.GetChild(0).localScale *=  particleScale;

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

        posList.Clear();

        StartCoroutine(CoPostGenerateMesh());
    }

    [PunRPC]
    void ShatterMesh_RPC()
    {
        if(meshObj!=null)
            meshObj.AddComponent<MeshShatter>().Init(groundPieceMat, _fallingGround.gameObject);
    }

    bool first = true;

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

    public void OnALLDestroy()
    {
        foreach(var a in _myMeshSet)
        {
            if(a != null) 
                Destroy(a);
        }

        foreach (var b in _myRoadSet)
        {
            if(b != null) 
                Destroy(b.gameObject);
        }
    }
}
