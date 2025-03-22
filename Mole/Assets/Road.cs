using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Road : MonoBehaviour
{

    public Collider2D collider_;

    [SerializeField] MeshDetector meshDetector;

    public HashSet<GameObject> _myMeshSet = new HashSet<GameObject>();

    public bool _isFinishRoad = false;

    public MeshGenerator _myOwner;

    public bool IsNeighCheckRoad = false;

    [SerializeField] HashSet<Road> neighRoadSet = new HashSet<Road>();
    public SpriteRenderer _sr;

    List<Road> RemoveList= new List<Road>();

    Vector3 originScale = Vector3.one;
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(IsNeighCheckRoad)
        {
            if(other.TryGetComponent<Road>(out Road road))
            {
                if(road.IsNeighCheckRoad == false)
                    return;
                
                neighRoadSet.Add(road);
            }
        }    
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsNeighCheckRoad)
        {
            if (other.TryGetComponent<Road>(out Road road))
            {
                neighRoadSet.Remove(road);
            }
        }
    }

    public HashSet<Road> GetNeigh()
    {
        return neighRoadSet;
    }

    private void Awake()
    {
        meshDetector.OnMeshCollide += CollideMesh;
    }

    public void Release()
    {
        gameObject.layer = LayerMask.NameToLayer("Road");
        _isFinishRoad = false;
        transform.localScale = originScale;
        _myMeshSet.Clear();
    }

    public void ChangeLayer()
    {
        if (this == null) return;

        gameObject.layer = LayerMask.NameToLayer("FinishRoad");
        _isFinishRoad = true;

        originScale = transform.localScale;
        transform.localScale *= 2f;
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

}
