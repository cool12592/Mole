using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour
{

    public Collider2D collider_;
    static int staticNumber = 0;
    public int myNumber = 0;
    public Road nextRoad;

    static Road staticRoad = null;
    [SerializeField] MeshDetector meshDetector;

    public HashSet<GameObject> _myMeshSet = new HashSet<GameObject>();

    bool _isFinishRoad = false;

    public MeshGenerator _myOwner;

    private void Awake()
    {
        if(staticRoad != null) 
            staticRoad.nextRoad = this;
        staticRoad = this;
        staticNumber++;
        myNumber = staticNumber;

        meshDetector.OnMeshCollide += CollideMesh;
    }

    public void ChangeLayer()
    {
        if (this == null) return;

        gameObject.layer = LayerMask.NameToLayer("FinishRoad");
        _isFinishRoad = true;
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
