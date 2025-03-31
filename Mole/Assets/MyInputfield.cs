using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyInputfield : MonoBehaviour
{
    [SerializeField] Image inputFeild;
    [SerializeField] Color initColor;
    private void OnEnable()
    {
        inputFeild.color = initColor;
    }
    public void Input()
    {
        inputFeild.color = Color.white;
    }
}
