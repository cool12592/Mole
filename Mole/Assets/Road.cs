using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour
{

    [SerializeField] Collider2D collider_;
    static int staticNumber = 0;
    public int myNumber = 0;
    public Road nextRoad;

    static Road staticRoad = null;


    private void Awake()
    {
        if(staticRoad != null) 
            staticRoad.nextRoad = this;
        staticRoad = this;
        staticNumber++;
        myNumber = staticNumber;
    }

    public void ChangeLayer()
    {
        gameObject.layer = LayerMask.NameToLayer("FinishRoad");

        // collider_.enabled = false;
    }

    public void ChangeColor()
    {
        //GetComponent<SpriteRenderer>().color = Color.black;
    }
}
