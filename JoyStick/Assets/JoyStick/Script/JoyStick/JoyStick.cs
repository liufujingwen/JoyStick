using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Slider = UnityEngine.Experimental.UIElements.Slider;
using UnityEngine.EventSystems;

public class JoyStick : MonoBehaviour
{
    [Tooltip("是否控制3D Model")]
    [SerializeField]
    private bool m_ThreeD = true;

    [Tooltip("摇杆触发区域")]
    [SerializeField]
    private RectTransform m_MoveRect;

    [Tooltip("摇杆底座显示区域")]
    [SerializeField]
    private RectTransform m_BackgroundRect;

    [Tooltip("摇杆底座")]
    [SerializeField]
    private RectTransform m_BackGround;

    [Tooltip("摇杆按钮")]
    [SerializeField]
    private RectTransform m_Joystick;

    [Tooltip("摇杆方向")]
    [SerializeField]
    private RectTransform m_Direction;

    [Tooltip("摇杆半径缩放系数")]
    [SerializeField]
    private float m_JoystickRadiusFactor = 1f;

    //拖拽回调
    public Action<JoyStickData> MoveHandler;

    //摇杆数据
    private JoyStickData m_Data = new JoyStickData();

    private float m_JoystickMoveRadius;
    private Vector3 m_MouseOriginScreenPoint;//记录按下一瞬间的屏幕坐标

    private Camera m_UiCamera;
    private Vector3 m_JoyStickDefaultPosition;

    private bool m_Initalize = false;
    private bool m_InRect = false;
    private bool m_BeginDrag;//是否开始拖拽了
    private bool m_Dragging = false;//正在拖拽

    //背景底移动范围的对应四个角的世界坐标 0:左下角 1:左上角 2:右上角 3:右下角
    private Vector3[] m_BackgroundRectCorners = new Vector3[4];


    private void Start()
    {
        JoyStickEvent joyStickEvent = m_MoveRect.GetComponent<JoyStickEvent>();
        if (joyStickEvent == null)
            joyStickEvent = m_MoveRect.gameObject.AddComponent<JoyStickEvent>();
        joyStickEvent.PointerDownHandler = OnPointerDown;
        joyStickEvent.BeginDragHandler = BeginDrag;
        joyStickEvent.PointerUpHandler = OnPointerUp;

        m_BackgroundRect.GetWorldCorners(m_BackgroundRectCorners);

        Canvas canvas = m_Joystick.GetComponentInParent<Canvas>();
        m_UiCamera = canvas.worldCamera;

        //半径 = (摇杆底部宽度的一半  -  摇杆宽度的一半) * 缩放系数
        m_JoystickMoveRadius = (m_BackGround.rect.width * 0.5f - m_Joystick.rect.width * 0.5f) * m_JoystickRadiusFactor;
        m_JoyStickDefaultPosition = m_BackGround.transform.localPosition;

        if (m_Direction && m_Direction.gameObject.activeSelf)
            m_Direction.gameObject.SetActive(false);

        m_Initalize = true;
    }

    public void OnDisable()
    {
        if (m_Initalize)
            Reset();
    }

    public void Update()
    {
        if (!m_InRect)
            return;

        if (!m_BeginDrag)
            return;

        if (m_BeginDrag)
            OnDrag();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector3 touchPosition = Input.mousePosition;

        m_InRect = RectTransformUtility.RectangleContainsScreenPoint(m_MoveRect, touchPosition, m_UiCamera);

        if (!m_InRect)
            return;

        Vector3 mouseWorlkPoint;
        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(m_BackGround, touchPosition, m_UiCamera, out mouseWorlkPoint))
            return;

        mouseWorlkPoint.z = m_BackGround.position.z;

        //修正x
        if (mouseWorlkPoint.x < m_BackgroundRectCorners[0].x)
            mouseWorlkPoint.x = m_BackgroundRectCorners[0].x;
        else if (mouseWorlkPoint.x > m_BackgroundRectCorners[2].x)
            mouseWorlkPoint.x = m_BackgroundRectCorners[2].x;

        //修正y
        if (mouseWorlkPoint.y < m_BackgroundRectCorners[3].y)
            mouseWorlkPoint.y = m_BackgroundRectCorners[3].y;
        else if (mouseWorlkPoint.y > m_BackgroundRectCorners[1].y)
            mouseWorlkPoint.y = m_BackgroundRectCorners[1].y;

        m_MouseOriginScreenPoint = RectTransformUtility.WorldToScreenPoint(m_UiCamera, mouseWorlkPoint);
        m_BackGround.transform.position = mouseWorlkPoint;

        if (m_Direction && m_Direction.gameObject.activeSelf)
            m_Direction.gameObject.SetActive(false);
    }

    public void BeginDrag(PointerEventData eventData)
    {
        if (!m_InRect)
            return;

        m_BeginDrag = true;
    }

    public void OnDrag()
    {
        if (!m_BeginDrag)
            return;

        if (!m_InRect)
            return;

        if (m_Direction && m_Direction.gameObject.activeSelf != m_Dragging)
            m_Direction.gameObject.SetActive(m_Dragging);

        Vector3 direction = Input.mousePosition - m_MouseOriginScreenPoint;
        Vector3 normalizedDirection = direction.normalized;
        float distance = direction.magnitude;

        if (distance < 3f)
        {
            m_Dragging = false;
            return;
        }

        m_Dragging = true;

        //求出弧度
        float radians = Mathf.Atan2(direction.y, direction.x);
        float angle = radians * Mathf.Rad2Deg;

        if (distance > m_JoystickMoveRadius)
            distance = m_JoystickMoveRadius;

        //移动摇杆
        if (m_Joystick != null)
        {
            Vector3 pos = normalizedDirection * distance;
            m_Joystick.transform.localPosition = pos;
        }

        if (m_Direction)
        {
            Vector3 eulerAngles = m_Direction.eulerAngles;
            eulerAngles.z = angle;
            m_Direction.eulerAngles = eulerAngles;
        }

        m_Data.Power = distance / m_JoystickMoveRadius; ;
        m_Data.Angle = angle < 0 ? 360 + angle : angle;
        m_Data.Direction = normalizedDirection;

        //派发事件
        if (MoveHandler != null)
        {
            MoveHandler(m_Data);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        Reset();
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        m_InRect = false;
        m_BeginDrag = false;
        m_Dragging = false;

        m_Data.Angle = 0;
        m_Data.Power = 0;

        m_BackGround.localPosition = m_JoyStickDefaultPosition;
        m_Joystick.transform.localPosition = Vector3.zero;
        if (m_Direction && m_Direction.gameObject.activeSelf)
            m_Direction.gameObject.SetActive(false);
    }
}
