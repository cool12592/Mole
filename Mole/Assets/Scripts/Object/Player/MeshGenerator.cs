using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Image;

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

    float timer = 0f;
    Vector3 lastPos;

    Action OnGenerateMesh;
    [SerializeField] FallingGround _fallingGround;
    [SerializeField] TextureAdd _textureADD;
    [SerializeField] bool inHouse = true;
    [SerializeField] int _curPointCount = 0;

    Road lastExitRoad;
    Road lastEnterRoad;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        PV.ObservedComponents.Add(this);
        PV.Synchronization = ViewSynchronization.UnreliableOnChange; // 변경 시만 동기화

        dashBtn = GameObject.Find("Canvas").transform.Find("DashButton").gameObject.GetComponent<Button>();
        dashBtn.onClick.AddListener(GenerateMeshObject);

        _fallingGround = GameObject.Find("FallingGround").GetComponent<FallingGround>();
        _fallingGround.gameObject.SetActive(false);

        Instantiate(_recordObj, transform.position + new Vector3(-1f,1f,0f), Quaternion.identity).GetComponent<Road>().ChangeLayer();
        Instantiate(_recordObj, transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponent<Road>().ChangeLayer();
        Instantiate(_recordObj, transform.position + new Vector3(1f, 1f, 0f), Quaternion.identity).GetComponent<Road>().ChangeLayer();
        Instantiate(_recordObj, transform.position + new Vector3(-1f, 0f, 0f), Quaternion.identity).GetComponent<Road>().ChangeLayer();
        Instantiate(_recordObj, transform.position + new Vector3(1f, 0f, 0f), Quaternion.identity).GetComponent<Road>().ChangeLayer();
        Instantiate(_recordObj, transform.position + new Vector3(-1f, -1f, 0f), Quaternion.identity).GetComponent<Road>().ChangeLayer();
        Instantiate(_recordObj, transform.position + new Vector3(0f, -1f, 0f), Quaternion.identity).GetComponent<Road>().ChangeLayer();
        Instantiate(_recordObj, transform.position + new Vector3(1f, -1f, 0f), Quaternion.identity).GetComponent<Road>().ChangeLayer();

        Instantiate(_recordObj, transform.position + new Vector3(0f, 0f, 0f), Quaternion.identity).GetComponent<Road>().ChangeLayer();


        _curPointCount = 100;
        posList.Add(transform.position + new Vector3(-1f, -1f, 0f));
        posList.Add(transform.position + new Vector3(-1f, 1f, 0f));
        posList.Add(transform.position + new Vector3(1f, 1f, 0f));
        posList.Add(transform.position + new Vector3(1f, -1f, 0f));
    }

    private IEnumerator Start()
    {
        yield return null;
        GenerateMeshObject();

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (inHouse)
            return;

        if (collision.gameObject.layer != LayerMask.NameToLayer("FinishRoad"))
            return;

        lastEnterRoad = collision.GetComponent<Road>();

        GenerateMeshObject();


    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("FinishRoad"))
            return;

        lastExitRoad = collision.GetComponent<Road>();

        _curPointCount = 0;
        posList.Clear();
    }

    public void OnTriggerEnter3D(Collider other)
    {
       // if (other.gameObject.layer != LayerMask.NameToLayer("RenderTexture") && other.gameObject.layer != LayerMask.NameToLayer("FinishRenderTexture"))
       //     return;

       // inHouse = true;


    }

    public void OnTriggerStay3D(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("RenderTexture") && other.gameObject.layer != LayerMask.NameToLayer("FinishRenderTexture"))
            return;

        inHouse = true;
    }

    public void OnTriggerExit3D(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("RenderTexture") && other.gameObject.layer != LayerMask.NameToLayer("FinishRenderTexture"))
            return;
   
        inHouse = false;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (inHouse)
        {
            GetComponent<SpriteRenderer>().color = Color.white;
            return;
        }

        GetComponent<SpriteRenderer>().color = Color.red;

        if (Vector3.SqrMagnitude(transform.position-lastPos)<0.01f)
            return;

        lastPos = transform.position;
       // if(0.1f<timer)
        {
            _curPointCount++;
            timer = 0f;
            posList.Add(new Vector2(transform.position.x, transform.position.y));


            OnGenerateMesh += Instantiate(_recordObj,transform.position, Quaternion.identity).GetComponent<Road>().ChangeLayer;
        }
    }

    void GenerateMeshObject()
    {
        if (_curPointCount < 10)
            return;


        if (lastExitRoad != null && lastEnterRoad != null)
        {
            if (lastExitRoad.myNumber > lastEnterRoad.myNumber)
            {
                var temp = lastExitRoad;
                lastExitRoad = lastEnterRoad;
                lastEnterRoad = temp;
            }

            Road nextRoad = lastExitRoad;
            while (nextRoad != null && nextRoad != lastEnterRoad)
            {
                posList.Add(nextRoad.transform.position);
                nextRoad.ChangeColor();
                nextRoad = nextRoad.nextRoad;
            }
        }

        photonView.RPC("SyncPosListAndGenerateMesh_RPC", RpcTarget.All, posList.ToArray());
    }

    void CreateMesh()
    {
        if (meshObj == null)
        {
            meshObj = new GameObject("GeneratedMesh");
            meshCollider = meshObj.AddComponent<MeshCollider>();
            meshFilter = meshObj.AddComponent<MeshFilter>();
            meshRenderer = meshObj.AddComponent<MeshRenderer>();
            meshRenderer.material = meshMaterial;
        }
        else
        {
            meshObj.layer = LayerMask.NameToLayer("FinishRenderTexture");
            meshObj = Instantiate(meshObj);
            meshObj.name = "GeneratedMesh";
            meshCollider = meshObj.GetComponent<MeshCollider>();
            meshFilter = meshObj.GetComponent<MeshFilter>();
            meshRenderer = meshObj.GetComponent<MeshRenderer>();
            meshRenderer.material = meshMaterial;
        }

        meshObj.transform.position = Vector3.zero;

    }

    /// <summary>
    /// 정점 리스트를 받아서 이동 경로 Mesh를 생성
    /// </summary>
    /// <param name="verticesList">Mesh를 구성할 정점 리스트</param>
    [PunRPC]
    private void SyncPosListAndGenerateMesh_RPC(Vector2[] receivedPosList)
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

        // ✅ Mesh 데이터 적용
        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        meshCollider.sharedMesh = mesh;
        Debug.Log("Mesh 생성 완료! 정점 개수: " + vertices.Length);

        // 🔥 위치 및 레이어 동기화
        meshObj.transform.position = new Vector3(meshObj.transform.position.x, meshObj.transform.position.y, 0f);
        meshObj.layer = Mathf.RoundToInt(Mathf.Log(changeLayer.value, 2));

        _curPointCount = 0;
        posList.Clear();
        StartCoroutine(CoPostGenerateMesh());
    }

    bool first = true;
    IEnumerator CoPostGenerateMesh()
    {
        yield return null;
        _fallingGround.BlendRenderTextures();
        _textureADD.BlendRenderTextures();
        if(first)
        {
            first = false;
            yield break;
        }
        _fallingGround.StartFalling();

       // yield return null;

        OnGenerateMesh?.Invoke();
        OnGenerateMesh = null;
    }
}
