using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderDectector : MonoBehaviour
{
    [SerializeField] MeshGenerator _owner;
    private void OnTriggerEnter(Collider other)
    {
        _owner.OnTriggerEnter3D(other);
    }

    private void OnTriggerStay(Collider other)
    {
        _owner.OnTriggerStay3D(other);
    }

    private void OnTriggerExit(Collider other)
    {
        _owner.OnTriggerExit3D(other);
    }
}
