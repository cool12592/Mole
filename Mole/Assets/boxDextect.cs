using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boxDextect : MonoBehaviour
{
    [SerializeField] block block;
    private void OnTriggerEnter(Collider other)
    {
        block.MyOnTriggerEnter(other);
    }
}
