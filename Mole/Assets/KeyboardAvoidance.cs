using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardAvoidance : MonoBehaviour
{
//    public RectTransform panelToMove; // UI 패널
//    public InputField inputField;     // 입력 필드

//    private Vector3 originalPosition;
//    private bool isKeyboardVisible = false;

//    void Start()
//    {
//        originalPosition = panelToMove.localPosition;
//       / //inputField.onSelect.AddListener(OnInputFieldSelected);
//       // inputField.onDeselect.AddListener(OnInputFieldDeselected);
//    }

//    void Update()
//    {
//#if UNITY_ANDROID || UNITY_IOS
//        if (isKeyboardVisible)
//        {
//            // 키보드가 활성화되면 위로 이동
//            if (TouchScreenKeyboard.visible)
//            {
//                panelToMove.localPosition = Vector3.Lerp(
//                    panelToMove.localPosition,
//                    originalPosition + new Vector3(0, 300, 0), // 원하는 만큼 올리기
//                    Time.deltaTime * 5
//                );
//            }
//        }
//#endif
//    }

//    void OnInputFieldSelected(string text)
//    {
//        isKeyboardVisible = true;
//    }

//    void OnInputFieldDeselected(string text)
//    {
//        isKeyboardVisible = false;
//        panelToMove.localPosition = originalPosition; // 원래 위치로
//    }
}
