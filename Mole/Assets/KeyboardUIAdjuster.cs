using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardUIAdjuster : MonoBehaviour
{
    public RectTransform uiRoot; // 전체 UI 패널 (Stretch 상태여도 OK)
    
    private Vector2 originalPos;
    private bool keyboardWasVisible = false;

    void Start()
    {
        if (uiRoot == null)
        {
            uiRoot = GetComponent<RectTransform>();
        }
        originalPos = uiRoot.anchoredPosition;
    }

    void Update()
    {
        if (TouchScreenKeyboard.visible)
        {
            float rawKeyboardHeight = TouchScreenKeyboard.area.height;

            // fallback: area가 0일 경우 대비
            if (rawKeyboardHeight <= 0f)
            {
                rawKeyboardHeight = Screen.height * 0.35f;
            }

            // height 비율 계산
            float canvasHeight = GetCanvasHeight();
            float keyboardHeightOnCanvas = rawKeyboardHeight / Screen.height * canvasHeight;

            uiRoot.anchoredPosition = originalPos + new Vector2(0, keyboardHeightOnCanvas);
            keyboardWasVisible = true;
        }
        else if (keyboardWasVisible)
        {
            uiRoot.anchoredPosition = originalPos;
            keyboardWasVisible = false;
        }
    }

    float GetCanvasHeight()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        
        if (scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            return scaler.referenceResolution.y;
        }
        else
        {
            return Screen.height;
        }
    }
}

