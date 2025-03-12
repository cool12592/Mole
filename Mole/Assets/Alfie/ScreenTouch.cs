using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScreenTouch : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
{
    [SerializeField] JoyStickScript _joy  = null;
    bool _enableTouch = true;

    public void OnPointerDown(PointerEventData eventData)
    {
        if(_enableTouch == false)
            return;

        _joy.OnTouch(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(_enableTouch == false)
            return;
        _joy.EndTouch();            
    }

    public void StartNoTouchTime()
    {
        _enableTouch = false;
        _joy.EndTouch();
    }

    public void EndNoTouchTime()
    {
        _enableTouch = true;
    }
}
