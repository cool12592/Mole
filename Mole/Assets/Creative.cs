using Cinemachine;
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

    public float zoomInSize = 3f;
    public float zoomDuration = 0.5f;
    public float StartcameraZoom = 10f;

    public bool isFixedPositionMode = false;

    public bool isNoKillText = false;

    public void StartIntroZoom(CinemachineVirtualCamera cm)
    {
        StartCoroutine(ZoomTo(cm,zoomInSize, zoomDuration));
    }

    IEnumerator ZoomTo(CinemachineVirtualCamera cm, float targetZoom, float duration)
    {
        float startZoom = cm.m_Lens.OrthographicSize;
        float time = 0f;

        while (time < duration)
        {
            cm.m_Lens.OrthographicSize = Mathf.Lerp(startZoom, targetZoom, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        cm.m_Lens.OrthographicSize = targetZoom;
    }

}
