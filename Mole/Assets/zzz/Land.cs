using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Land : MonoBehaviour
{
    public MeshGenerator _owner;

    public void Init(MeshGenerator owner)
    {
        _owner = owner;
    }
}
