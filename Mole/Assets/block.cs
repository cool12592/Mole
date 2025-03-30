using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class block : MonoBehaviour
{
    [SerializeField] SpriteShatter spShatter;
    [SerializeField] SpriteRenderer sr;
    bool b = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (b)
            return;
       // if (collision.gameObject.TryGetComponent<playerScript>(out playerScript player))
        {
        //    if (player.IsEnemy)
         //       return;

            spShatter.BlockInit(sr.sprite);
            gameObject.SetActive(false);
            b = true;
        }
    }
    [SerializeField] Color customColor;
    public void MyOnTriggerEnter(Collider other)
    {
        if (b)
        {
            return;
        }
        b = true;
        spShatter.BlockInit(sr.sprite);
        gameObject.SetActive(false);
    }
}
