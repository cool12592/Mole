using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour
{

    [SerializeField] Collider2D collider_;
    

    public void DisableCollider()
    {
        gameObject.layer = LayerMask.NameToLayer("FinishRoad");

        // collider_.enabled = false;
    }
}
