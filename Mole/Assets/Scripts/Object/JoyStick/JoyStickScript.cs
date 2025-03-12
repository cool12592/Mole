using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class JoyStickScript : MonoBehaviour
{
    [SerializeField] RectTransform _lever = null;
    [SerializeField] RectTransform _rectTransform = null;

    [SerializeField, Range(10, 150)]
    private float _leverRange = 0f;

    public bool IsInput { get; private set; }

    [SerializeField] Canvas _canvas = null;
    public static Vector2 InputAxis { private set; get; } = Vector2.zero;

    private Vector2 GetJoystickDir(Vector2 mousPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, mousPos, null, out Vector2 pos);

        pos = pos.magnitude < _leverRange ? pos : pos.normalized * _leverRange;
        _lever.anchoredPosition = pos;

        var inputDir = pos / _leverRange;
        return inputDir;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsInput)
        {
            InputAxis = GetJoystickDir(Input.mousePosition);
        }
    }

    public void OnTouch(Vector3 pos)
    {
        if (IsInput == true)
            return;
        transform.position = pos;
        gameObject.SetActive(true);
        IsInput = true;
    }

    public void EndTouch()
    {
        // if(_isInput == false)
        //   return;
        IsInput = false;
        _lever.anchoredPosition = Vector2.zero;
        InputAxis = Vector2.zero;
        gameObject.SetActive(false);
    }
}
