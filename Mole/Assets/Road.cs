using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Road : MonoBehaviour
{

    public GameObject CuteMesh;
    public Collider2D collider_;

    [SerializeField] MeshDetector meshDetector;

    public HashSet<GameObject> _myMeshSet = new HashSet<GameObject>();

    public bool _isFinishRoad = false;

    public MeshGenerator _myOwner;

    public bool IsNeighCheckRoad = false;

    [SerializeField] HashSet<Road> neighRoadSet = new HashSet<Road>();
    public SpriteRenderer _sr;


    private static int RoadLayer;
    private static int FinishRoadLayer;

    public bool IsInPool = false;


    public HashSet<Road> GetNeigh()
    {
        return neighRoadSet;
    }

    private void Awake()
    {
        RoadLayer = LayerMask.NameToLayer("Road");
        FinishRoadLayer = LayerMask.NameToLayer("FinishRoad");
        meshDetector.OnMeshCollide += CollideMesh;
    }

    public void Init()
    {
        CuteMesh.SetActive(false);
        CuteMesh.transform.localScale = Vector3.one;
        _myOwner = null;
        _myMeshSet = null;

        neighRoadSet.Clear();
        gameObject.layer = RoadLayer;
        _isFinishRoad = false;
    }

    public void ChangeLayer(float sizeUp)
    {
        if (this == null) return;
        if (_isFinishRoad) return;

        gameObject.layer = FinishRoadLayer;
        _isFinishRoad = true;

        transform.localScale *= sizeUp;
        CuteMesh.transform.localScale *= 1/sizeUp;
    }

    void CollideMesh(GameObject go)
    {
        if (_isFinishRoad == false)
            return;
        if (_myMeshSet == null)
            return;
        if(_myMeshSet.Contains(go))
        {
            return;
        }
        GlobalRoadPool.Instance.Release(this);
    }


    public void Release()
    {
        foreach(var road in neighRoadSet)
        {
            if(road!= null)
                road.neighRoadSet.Remove(this);
        }

        neighRoadSet.Clear();
    }



    float scanRadius = 1f;
    [SerializeField] LayerMask targetLayer;
    [SerializeField] Collider2D[] results = new Collider2D[20]; // 최대 10개까지 감지

    void Start()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            scanRadius,
            results,
            targetLayer
        );

        for (int i = 0; i < hitCount && i< results.Length; i++)
        {
            Collider2D col = results[i];
            if(col.gameObject.TryGetComponent<Road>(out Road road))
            {
                neighRoadSet.Add(road);
                road.neighRoadSet.Add(this);
            }
        }
    }

}
