using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardAvoidance : MonoBehaviour
{
//    public RectTransform panelToMove; // UI �г�
//    public InputField inputField;     // �Է� �ʵ�

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
//            // Ű���尡 Ȱ��ȭ�Ǹ� ���� �̵�
//            if (TouchScreenKeyboard.visible)
//            {
//                panelToMove.localPosition = Vector3.Lerp(
//                    panelToMove.localPosition,
//                    originalPosition + new Vector3(0, 300, 0), // ���ϴ� ��ŭ �ø���
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
//        panelToMove.localPosition = originalPosition; // ���� ��ġ��
//    }
}
