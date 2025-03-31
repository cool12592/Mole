using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyInputfield : MonoBehaviour
{
    [SerializeField] Image inputFeild;

    public void Input()
    {
        inputFeild.color = Color.white;
    }
}
