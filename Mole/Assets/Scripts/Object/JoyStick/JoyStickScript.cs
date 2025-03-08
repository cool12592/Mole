using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class JoyStickScript : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
{
    [SerializeField]
    private RectTransform lever;
    private RectTransform rectTransform;

    [SerializeField]
    private float multiplier = 2.7f;

    [SerializeField,Range(10,150)]
    private float leverRange;

    private Vector2 inputDirection;
    private bool isInput = false;

    public GameObject MyPlayer;

    public enum JoystickType { Move,Aim};
    public JoystickType joystickType;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        controlJoyStickLever(eventData);
        isInput = true;
    }

    //드래그해서 마우스 멈추고있는동안은 이벤트안됨 그래서 isInput써서 update 에다 해야됨
    public void OnDrag(PointerEventData eventData)
    {
        controlJoyStickLever(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        lever.anchoredPosition = Vector2.zero;
        isInput = false;

        if (MyPlayer)
        {
            if(joystickType==JoystickType.Move)
               MyPlayer.GetComponent<PlayerMovement>().Move(Vector2.zero);
        }

    }
    [SerializeField] Canvas canvas;
    private void controlJoyStickLever(PointerEventData eventData)
    {


        Vector2 mousePosition = eventData.position; // 마우스 좌표
        Vector2 pos; // 변환된 canvas내 좌표

        // 마우스 좌표를 canvas내에서의 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePosition, canvas.worldCamera, out pos);

        var inputVector = pos.magnitude < leverRange ? pos : pos.normalized * leverRange;
        lever.anchoredPosition = inputVector;
        inputDirection = inputVector / leverRange;

        //Debug.Log(Camera.main.ScreenToViewportPoint(eventData.position - (rectTransform.anchoredPosition * canvas.scaleFactor)));
        //var inputPos = Camera.main.ScreenToWorldPoint(eventData.position - (rectTransform.anchoredPosition * canvas.scaleFactor));
        //// eventData.position - (rectTransform.anchoredPosition * canvas.scaleFactor);// new Vector2(Screen.width/rectTransform.anchoredPosition.x , Screen.height/rectTransform.anchoredPosition.y );// - new Vector2(Screen.width, Screen.height);//rectTransform.anchoredPosition; 

        //// if (joystickType == JoystickType.Aim)
        ////    inputPos = eventData.position - new Vector2(Screen.width, 0f) - rectTransform.anchoredPosition;

        ////var inputVector = inputPos.magnitude < leverRange ? inputPos : inputPos.normalized * leverRange;
        //Vector3 vec = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        //var v = Camera.main.ScreenToWorldPoint(vec);
        //v.z = 0;
        //lever.position =v;//.position;// * (multiplier/ canvas.scaleFactor);
        //inputDirection = inputPos / leverRange;


        //// 현재 위치를 저장하고
        //var currentPos = eventData.position - rectTransform.anchoredPosition;

        //// 기준점으로부터의 거리를 80으로 제한한다
        //Vector3 v = currentPos;// Vector3.ClampMagnitude(currentPos, leverRange);

        //// 스틱의 위치를 기준점에서 v를 더한 위치로 지정한다
        //lever.anchoredPosition =  v;

        //// 뱡항을 갱신한다
        //inputDirection = v.normalized;




        // if (leverRange<lever.anchoredPosition.magnitude)
        //    lever.anchoredPosition -= eventData.delta / canvas.scaleFactor;
        //  lever.anchoredPosition = lever.anchoredPosition.magnitude < leverRange ? lever.anchoredPosition : lever.anchoredPosition.normalized * leverRange;

        //  lever.anchoredPosition = lever.anchoredPosition / multiplier;
        //inputDirection = lever.anchoredPosition / leverRange;



        //var value = (eventData.delta / canvas.scaleFactor) * multiplier;
        //lever.anchoredPosition += value;
        //if(leverRange < lever.anchoredPosition.magnitude)
        //    lever.anchoredPosition -= value;
    }

    // Update is called once per frame
    void Update()
    {
        if (isInput)
            inputControlVector();

    }

    private void inputControlVector()
    {
        if (MyPlayer)
        {
            if (joystickType == JoystickType.Move)
            {
                MyPlayer.GetComponent<PlayerMovement>().Move(inputDirection);
            }
            else if (joystickType == JoystickType.Aim)
            {
                MyPlayer.GetComponent<PlayerAttack>().Attack();

                if (inputDirection != null)
                MyPlayer.GetComponent<PlayerPistol>().AimMove(inputDirection);
            }
        }
    }
}
