using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creative : MonoBehaviour
{
    private static Creative instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public static Creative Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    public float cameraZoom = 10f;

}
