using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGenerate : MonoBehaviour
{
    [SerializeField] GameObject block;
    [SerializeField] int row, col;
    [SerializeField] float space;
    private void Start()
    {
        GameManager.Instance.isBlockMode = true;
        for(int i=0;i<row;i++)
        {
            for(int j=0; j<col;j++)
            {
                Instantiate(block, transform.position+ new Vector3(i * space, -j * space, 0f), Quaternion.identity);
            }
        }
    }


}
