using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.TryGetComponent<playerScript>(out playerScript player))
        {
            player.meshGenerator.StartDrillMode();
            Destroy(gameObject);
        }
    }
}
