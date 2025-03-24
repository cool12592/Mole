using Lofelt.NiceVibrations;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
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

    float dustTimer = 0f;
    float flipTimer = 0f;

    Vector3 lastPos;

    Action OnGenerateMesh;
    [SerializeField] bool inHouse = true;

    [SerializeField] SpriteRenderer _dust;
    [SerializeField] SpriteRenderer _dustRockSR;

    Road lastExitRoad;
    Road lastEnterRoad;

    int checkNum = 0;

    int myKillCount;
    Text myKillText;


    public HashSet<Road> _myRoadSet = new HashSet<Road>();
    public HashSet<GameObject> _myMeshSet = new HashSet<GameObject>();

    HashSet<GameObject> _curInMyMeshSet = new HashSet<GameObject>();
    HashSet<GameObject> _curInOtherMeshSet = new HashSet<GameObject>();

    [SerializeField] GamePalette palette;  
    Color myColor;

    [SerializeField] Sprite pieceSprite;
    List<GameObject> removeReserveList = new List<GameObject>();
    [SerializeField] GameObject _dustParticle;

    [SerializeField] AudioSource _meshGenSound;
    [SerializeField] AudioSource _moveSound;

    public LayerMask targetLayer;    // 충돌 레이어 설정

    private static int RoadLayer;
    private static int FinishRoadLayer;

    playerScript player;

    public void SetMyColor(Color color)
    {
        myColor = color;
    }

    private void Awake()
    {
        player = GetComponent<playerScript>();
        RoadLayer = LayerMask.NameToLayer("Road");
        FinishRoadLayer = LayerMask.NameToLayer("FinishRoad");

        PV = GetComponent<PhotonView>();

        myKillText = GameObject.Find("Canvas").transform.Find("Rope").transform.Find("Kill").transform.Find("MyKillCount").GetComponent<Text>();

        ResetSharedFloat();

        GameManager.Instance.UserMeshMap[PV.Owner.NickName] = this;
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => myColor != default(Color));

        // Vector3[] positions =
        // {
        //     new Vector3(-0.5f, 0.5f, 0f), new Vector3(0f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0f),
        //     new Vector3(-0.5f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0.5f, 0f, 0f),
        //     new Vector3(-0.5f, -0.5f, 0f), new Vector3(0f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f)
        // };

        // foreach (Vector3 offset in positions)
        // {
        //     CreateLoad(transform.position + offset,true);
        // }

        WriteFirstMeshPoint(20, 1.2f);

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

            CreateLoad(center + new Vector3(x, y, 0f),true);

        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == FinishRoadLayer)
        {
            lastEnterRoad = other.GetComponent<Road>();
        }

        if (other.gameObject.layer != RoadLayer)
            return;

        if (other.TryGetComponent<Road>(out Road otherRoad))
        {
            if (PhotonNetwork.IsMasterClient == false)
                return;

            if (otherRoad._myOwner == null)
                return;

            if (otherRoad._myOwner == this)
                return;

            if (otherRoad._myOwner.PV == null || PV == null)
                return;

            var otherHealth = otherRoad._myOwner.gameObject.GetComponent<PlayerHealth>();
            if (otherHealth == null || otherHealth.PlayerActive == false)
                return;

            otherHealth.Death(player, PV.Owner.NickName);
        }
    }

    public void TakeAwayLand(string targetNick)
    {
        myKillText.text = ++myKillCount + " Kill";

        var target = GameManager.Instance.UserMeshMap[targetNick];

        foreach (var otherMeshObj in target._myMeshSet)
        {
            if (otherMeshObj == null)
                continue;

            otherMeshObj.GetComponent<MeshRenderer>().material.color = myColor;
            _myMeshSet.Add(otherMeshObj);
        }

        foreach (var otherRoad in target._myRoadSet)
        {
            if (otherRoad == null)
                continue;

            if (otherRoad._isFinishRoad == false)
            {
                posList.Add(otherRoad.transform.position);
            }

            _myRoadSet.Add(otherRoad);
            OnGenerateMesh += otherRoad.ChangeLayer;
            otherRoad._myMeshSet = _myMeshSet;
            otherRoad._myOwner = this;
            otherRoad._sr.color = myColor;
        }

        GenerateMeshObject(needBfs: false);
    }


    private void OnTriggerExit2D(Collider2D other) 
    {                        
        if (other.gameObject.layer == FinishRoadLayer)
        {
            if(2<posList.Count)
                return;
            lastExitRoad = other.GetComponent<Road>();

          //  lastExitRoad.transform.position = new Vector3(lastExitRoad.transform.position.x,lastExitRoad.transform.position.y,-999f);
       // lastExitRoad._sr.color = Color.red;
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
           // posList.Clear();
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
                //lastEnterRoad = transform.position;
                GenerateMeshObject();
            }
        }
        else
        {
          //  GetComponent<SpriteRenderer>().color = Color.red;
            
           // lastExitRoad = transform.position;
            posList.Clear();
          //  CreateLoad(transform.position,false);
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

            if(count%7==0 && PV.IsMine)
            {
                _moveSound.volume = 0.3f;
                _moveSound.Play();
            }
            //if (count%20==0)
            //{
            //    _moveSound.Play();
            //}

            dustTimer = 0f;

            bool isNeighCheck = false;
           // if(count%3 == 0)
                isNeighCheck = true;
            CreateLoadForward(transform.position,isNeighCheck, shatter);
        }
    }

    void CreateLoad(Vector3 pos,bool isNeighCheckRoad, bool shatter = false)
    {
        if (PV.IsMine == false)
            return;

        pos.z = GetSharedFloat();

        photonView.RPC("CreateLoad_RPC", RpcTarget.All, pos.x, pos.y, pos.z,isNeighCheckRoad,shatter);
    }

    void CreateLoadForward(Vector3 pos,bool isNeighCheckRoad ,bool shatter = false)
    {
        if (PV.IsMine == false)
            return;

        pos += transform.up * 0.7f;
        pos.z = GetSharedFloat();

        photonView.RPC("CreateLoad_RPC", RpcTarget.All, pos.x, pos.y, pos.z,isNeighCheckRoad, shatter);
    }

    [PunRPC]
    void CreateLoad_RPC(float x, float y, float z,bool isNeighCheckRoad,bool shatter)
    {
        Vector3 pos = new Vector3(x, y, z);
        var road = GlobalRoadPool.Instance.GetRoad(pos,Vector3.one *0.6f);
        
        road._sr.color = myColor;
        _myRoadSet.Add(road);
        OnGenerateMesh += road.ChangeLayer;
        road._myMeshSet = _myMeshSet;
        road._myOwner = this;

        if(isNeighCheckRoad)
        {
            road.IsNeighCheckRoad = true;
        }

        if (shatter)
        {
            road.GetComponent<SpriteShatter>().Init(pieceSprite, transform.up * 0.5f);
        }

    }


    [PunRPC]
    void FirstCreateLoad_RPC(float x, float y, float z, float radius)
    {
        Vector3 pos = new Vector3(x, y, z);
        var road = GlobalRoadPool.Instance.GetRoad(pos, Vector3.one * radius * 0.7f);
        road._sr.color = myColor;

        _myRoadSet.Add(road);
        OnGenerateMesh += road.ChangeLayer;
        road._myMeshSet = _myMeshSet;
        road._myOwner = this;
        road.IsNeighCheckRoad = true;
    }

    void SavePath(Dictionary<Road, Road> parentMap, Road target)
    {
        List<Road> roadsToDestroy = new List<Road>();

        float r = UnityEngine.Random.Range(0f, 1f);
        float g = UnityEngine.Random.Range(0f, 1f);
        float b = UnityEngine.Random.Range(0f, 1f);
        Color color =  new Color(r, g, b, 1f); // 알파값 1 (불투명)

        HashSet<Road> visitedNodes = new HashSet<Road>(); // 🔥 방문 체크용

        while (target != null)
        {
            if(visitedNodes.Contains(target)) 
                break;

            visitedNodes.Add(target);

            var pos = new Vector2(target.transform.position.x, target.transform.position.y);
           // target._sr.color = color;
           // target._sr.enabled = true;
            //target.transform.position = new Vector3(target.transform.position.x,target.transform.position.y,-900f);
            posList.Add(pos);
            //roadsToDestroy.Add(target); // 삭제할 리스트에 추가
            target = parentMap.ContainsKey(target) ? parentMap[target] : null;
        }

        // // 루프가 끝난 후 삭제
        // foreach (var road in roadsToDestroy)
        // {
        //     if(road!= null)
        //     //Destroy(road.gameObject);
        //     road.DeleteNeigh();
        // }
    }

    const int NodesPerFrame = 10000;
    IEnumerator CoBFSSearch()
    {
        if (lastEnterRoad == null || lastExitRoad == null)
        {
            yield break;
        }
        Queue<Road> queue = new Queue<Road>();
        HashSet<Road> visited = new HashSet<Road>();
        Dictionary<Road, Road> parentMap = new Dictionary<Road, Road>(); // 부모 저장용

        queue.Enqueue(lastExitRoad);
        visited.Add(lastExitRoad);
        parentMap[lastExitRoad] = null; // 시작점의 부모는 없음

        int nodeCount = 0;
        while (queue.Count > 0)
        {
            Road node = queue.Dequeue();

            foreach (Road neighbor in node.GetNeigh())
            {
                if (neighbor == null || neighbor._isFinishRoad == false)
                    continue;

                if (neighbor == lastEnterRoad) // 목적지 도달
                {

                    parentMap[neighbor] = node; // 부모 저장
                    SavePath(parentMap, lastEnterRoad);

                   // lastExitRoad.transform.position = new Vector3(lastExitRoad.transform.position.x, lastExitRoad.transform.position.y, -999f);
                   // lastEnterRoad.transform.position = new Vector3(lastEnterRoad.transform.position.x, lastEnterRoad.transform.position.y, -999f);
                    //lastExitRoad._sr.color = Color.red;
                   // lastEnterRoad._sr.color = Color.blue;

                   // lastEnterRoad.name += checkNum.ToString();
                   // lastExitRoad.name += checkNum.ToString();
                   // checkNum++;

                   // lastExitRoad._sr.enabled = true;
                   // lastEnterRoad._sr.enabled = true;

                    yield break;
                }

                if (!visited.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                    parentMap[neighbor] = node; // 부모 저장
                }

                nodeCount++;
                if (nodeCount % NodesPerFrame == 0)
                {
                    yield return null; // 10,000개 노드 탐색마다 한 프레임 쉬기
                }
            }
        }

        UnityEngine.Debug.LogError("BFS 탐색 실패!!!!");
    }

    void SphereCastDetectEnterRoad()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, 3f, targetLayer);

        if (hit.collider != null && hit.collider.gameObject.TryGetComponent<Road>(out Road road))
        {
            Debug.DrawLine(transform.position, Vector3.zero, Color.red, 3f);
            lastEnterRoad = road;
        }
    }

    void GenerateMeshObject(bool needBfs = true)
    {
         if (PV.IsMine == false)
            return;

        if (posList.Count < 3)
        {
            photonView.RPC("BabyLand", RpcTarget.All);
            return;
        }
        StartCoroutine(CoGenerateMeshObject(needBfs));
    }

    IEnumerator CoGenerateMeshObject(bool needBfs = true)
    {
        if (PV.IsMine == false)
            yield break;

        if (posList.Count < 3)
            yield break;

        // CreateLoad(transform.position);


        //if(lastEnterRoad == lastExitRoad || lastEnterRoad ==null)
        //{

        //}
        SphereCastDetectEnterRoad();

        originLastIndex = posList.Count - 1;
        
        if(needBfs)
            yield return StartCoroutine(CoBFSSearch());

        float z = GetSharedFloat();



        photonView.RPC("SyncPosListAndGenerateMesh_RPC", RpcTarget.All, posList.ToArray(),z);
    }

    [PunRPC]
    void BabyLand()
    {
        FinishLand();
    }

    void CreateMesh()
    {
        if (meshObj == null)
        {
            meshObj = new GameObject("GeneratedMesh");
            _myMeshSet.Add(meshObj);

            meshObj.AddComponent<MeshShatter>();
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

        meshObj.AddComponent<AttackMesh>().Init(player, PV.Owner.NickName);
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

        // 정점 리스트를 배열로 변환
        Vector3[] vertices = new Vector3[posList.Count];
        Vector2[] uvs = new Vector2[posList.Count]; // ✅ UV 배열 추가

        Vector3 sumVec = Vector3.zero;
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        float totalArea = 0f;
        for (int i = 0; i < posList.Count; i++)
        {
            float x = posList[i].x;
            float y = posList[i].y;

            vertices[i] = new Vector3(x, y, 0);
            sumVec += new Vector3(x, y, 0);

            if(0<i && i <= posList.Count-2)
            {
                Vector2 a = posList[0];
                Vector2 b = posList[i];
                Vector2 c = posList[i+1];

                float area = Mathf.Abs((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) * 0.5f;
                totalArea += area;
            }

            // 최소/최대 좌표 업데이트
            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
        }

        Vector3 centerPos = sumVec / posList.Count; // 중심 좌표
        centerPos.z = -10f;
        //float width = maxX - minX; // AABB 가로 길이
        //float height = maxY - minY; // AABB 세로 길이
        //float boundingBoxArea = width * height; // 사각형 넓이

        if (PV.IsMine)
        {
            _meshGenSound.Play();
            GetComponent<PlayerMovement>().ShakeCamera();

            GameManager.Instance.ReportTheMakeLand(PV.Owner.NickName, totalArea);
        }
        var particle = Instantiate(_dustParticle, centerPos, Quaternion.identity);

        //// ✅ UV 매핑 설정
        //for (int i = 0; i < posList.Count; i++)
        //{
        //    float u = (posList[i].x - minX) / width;   // X 정규화 (0~1)
        //    float v = (posList[i].y - minY) / height;  // Y 정규화 (0~1)
        //    uvs[i] = new Vector2(u, v);
        //}

        // 삼각형 인덱스 자동 생성
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



       // mesh.uv = uvs;  // ✅ UV 추가
        //mesh.RecalculateNormals();

        meshCollider.sharedMesh = mesh;
      //  UnityEngine.Debug.Log("Mesh 생성 완료! 정점 개수: " + vertices.Length);

        // 🔥 위치 및 레이어 동기화
        meshObj.transform.position = new Vector3(meshObj.transform.position.x, meshObj.transform.position.y, z);
        meshObj.layer = Mathf.RoundToInt(Mathf.Log(changeLayer.value, 2));


        Destroy(particle, 2f);

        FinishLand();

        if (meshObj != null)
            meshObj.GetComponent<MeshShatter>().Init(_groundPieces);

    }

    void FinishLand()
    {
        posList.Clear();
        lastExitRoad = null;
        lastEnterRoad = null;

        OnGenerateMesh?.Invoke();
        OnGenerateMesh = null;
    }

    [SerializeField] Sprite[] _groundPieces;

    static float sharedFloat = 0f; // 🔴 공유할 float 값 (초기값 100)

    public float GetSharedFloat()
    {
        PV.RPC("DecreaseSharedFloat_RPC", RpcTarget.All); 
        return sharedFloat; 
    }

    [PunRPC]
    void DecreaseSharedFloat_RPC()
    {
        sharedFloat -= 0.001f; 
    }

    public void ResetSharedFloat()
    {
        if (PhotonNetwork.IsMasterClient)
            PV.RPC("ResetSharedFloat_RPC", RpcTarget.All);
    }

    [PunRPC]
    void ResetSharedFloat_RPC()
    {
        sharedFloat = 0f; 
    }

    public void OnALLDestroy()
    {
        myKillText.text = "0 Kill";

        foreach (var mesh in _myMeshSet)
        {
            if (mesh != null)
                Destroy(mesh);
        }

        foreach (var road in _myRoadSet)
        {
            if (road != null)
                GlobalRoadPool.Instance.Release(road);
        }
    }
}
