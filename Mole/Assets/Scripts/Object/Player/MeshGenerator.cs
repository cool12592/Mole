using Photon.Pun;
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
    private Button dashBtn;

    float timer = 0f;
    Vector3 lastPos;

    Action OnGenerateMesh;
    [SerializeField] FallingGround _fallingGround;
    [SerializeField] TextureAdd _textureADD;
    [SerializeField] bool inHouse = false;
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        PV.ObservedComponents.Add(this);
        PV.Synchronization = ViewSynchronization.UnreliableOnChange; // 변경 시만 동기화

        // ✅ Mesh 오브젝트 초기화
        meshObj = new GameObject("GeneratedMesh");
        meshCollider = meshObj.AddComponent<MeshCollider>();
        meshCollider.isTrigger = true;
        meshFilter = meshObj.AddComponent<MeshFilter>();
        meshRenderer = meshObj.AddComponent<MeshRenderer>();
        // ✅ 머터리얼 설정
        if (meshMaterial != null)
        {
            meshRenderer.material = meshMaterial;
        }
        else
        {
            Debug.LogWarning("Mesh Material이 설정되지 않았습니다!");
        }

        // ✅ 위치 조정
        meshObj.transform.position = Vector3.zero;


        dashBtn = GameObject.Find("Canvas").transform.Find("DashButton").gameObject.GetComponent<Button>();
        dashBtn.onClick.AddListener(GenerateMeshObject);

        _fallingGround = GameObject.Find("FallingGround").GetComponent<FallingGround>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (inHouse)
            return;
        if (collision.gameObject.layer != LayerMask.NameToLayer("Road"))
            return;

        GenerateMeshObject();
    }

    public void OnTriggerEnter3D(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("RenderTexture"))
            return;
        inHouse = true;
    }

    public void OnTriggerExit3D(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("RenderTexture"))
            return;
        inHouse = false;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (inHouse)
            return;
        if (Vector3.SqrMagnitude(transform.position-lastPos)<0.01f)
            return;

        lastPos = transform.position;
       // if(0.1f<timer)
        {
            timer = 0f;
            posList.Add(new Vector2(transform.position.x, transform.position.y));
            OnGenerateMesh += Instantiate(_recordObj,transform.position, Quaternion.identity).GetComponent<Road>().DisableCollider;
        }
    }

    void GenerateMeshObject()
    {
        photonView.RPC("SyncPosListAndGenerateMesh_RPC", RpcTarget.All, posList.ToArray());
    }

    /// <summary>
    /// 정점 리스트를 받아서 이동 경로 Mesh를 생성
    /// </summary>
    /// <param name="verticesList">Mesh를 구성할 정점 리스트</param>
    [PunRPC]
    private void SyncPosListAndGenerateMesh_RPC(Vector2[] receivedPosList)
    {
        if (receivedPosList.Length < 3)
        {
            Debug.LogError("정점이 3개 이상 필요합니다!");
            return;
        }

        OnGenerateMesh?.Invoke();


        // 🔥 받은 posList로 동기화
        posList = new List<Vector2>(receivedPosList);

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

        StartCoroutine(CoPostGenerateMesh());
    }

    IEnumerator CoPostGenerateMesh()
    {
        yield return null;

        yield return null;
        _textureADD.BlendRenderTextures();
        yield return null;

        yield return null;
        _fallingGround.StartFalling();
    }
}
