using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour
{

    [SerializeField] Collider2D collider_;
    public bool isNoUseRoad = true; 
    public void DisableCollider()
    {
        isNoUseRoad = false;
       // collider_.enabled = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;

        gameObject.layer = LayerMask.NameToLayer("Road");

    }
}
