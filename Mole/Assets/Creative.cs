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

    public bool isNoJoyPad = false;


    public float zoomInSize = 3f;
    public float zoomDuration = 0.5f;
    public float StartcameraZoom = 10f;

    public bool isFixedPositionMode = false;

    public bool isNoKillText = false;
    public bool isNoSountEffect = false;
    public bool isNoBGM = false;
    public bool isNoRockSound = false;
    public bool isNoCameraShake = false;

    public float playerSpped = 0f;

    public float DrillZoomOut = 2f;
    public int drillSpawnCount = 2;

    public Color meshShatterColor = Color.white;

    public void StartIntroZoom(CinemachineVirtualCamera cm)
    {
        StartCoroutine(ZoomTo(cm,zoomInSize, zoomDuration));
    }

    public void ChangeZoom(CinemachineVirtualCamera cm, float zoomSize, float zoomDuration)
    {
        StartCoroutine(ZoomTo(cm, zoomSize, zoomDuration));
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

    [SerializeField] UnityEngine.UI.Image _image1, _image2;
    private void Start()
    {
        if (Creative.Instance.isNoJoyPad)
        {
            _image1.color = new Color(_image1.color.r, _image1.color.g, _image1.color.b, 0f);
            _image2.color = new Color(_image2.color.r, _image2.color.g, _image2.color.b, 0f);

        }
    }

}
