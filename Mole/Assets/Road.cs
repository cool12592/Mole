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

    [SerializeField] List<Road> neighRoadList = new List<Road>();
    public SpriteRenderer _sr;
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(IsNeighCheckRoad)
        {
            if(other.TryGetComponent<Road>(out Road road))
            {
                if(road.IsNeighCheckRoad == false)// || road._isFinishRoad == false)
                    return;
                
                neighRoadList.Add(road);
            }
        }    
    }

    public List<Road> GetNeigh()
    {
        return neighRoadList;
    }

    public void DeleteNeigh()
    {
        foreach(Road road in neighRoadList)
        {
            if(road != null)
            {
                road.gameObject.SetActive(false);
            }
        }
    }

    private void Awake()
    {
        meshDetector.OnMeshCollide += CollideMesh;
        _sr = GetComponent<SpriteRenderer>();
    }

    public void ChangeLayer()
    {
        if (this == null) return;

        gameObject.layer = LayerMask.NameToLayer("FinishRoad");
        _isFinishRoad = true;

        transform.localScale *= 1.5f;
        // collider_.enabled = false;
    }

    public void ChangeColor()
    {
        //GetComponent<SpriteRenderer>().color = Color.black;
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
        Destroy(gameObject);
    }



}
