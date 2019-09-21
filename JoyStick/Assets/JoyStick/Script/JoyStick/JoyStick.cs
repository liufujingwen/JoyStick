using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Slider = UnityEngine.Experimental.UIElements.Slider;
using UnityEngine.EventSystems;

public class JoyStick : MonoBehaviour
{
    public Action<JoyStickData> MoveHandler;

    [Tooltip("摇杆触发区域")]
    [SerializeField]
    private RectTransform m_MoveRect;

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

    //data
    public JoyStickData data = new JoyStickData();

    private float m_JoystickMoveRadius;
    private Vector3 m_TouchOrigin;//按下原点（Input.mouseposition）
    private Rect BackroundMaxRect;//底座的显示范围

    private Camera m_UiCamera;
    private Vector3 m_JoyStickDefaultPosition;

    private bool m_Initalize = false;
    private bool m_InRect = false;
    private bool m_Dragging = false;

    private void Start()
    {
        JoyStickEvent joyStickEvent = m_MoveRect.GetComponent<JoyStickEvent>();
        if (joyStickEvent == null)
            joyStickEvent = m_MoveRect.gameObject.AddComponent<JoyStickEvent>();
        joyStickEvent.PointerDownHandler = OnPointerDown;
        joyStickEvent.PointerUpHandler = OnPointerUp;

        //计算底座显示的最大范围
        Vector2 joyStickBackgroundSize = new Vector2(m_BackGround.rect.width, m_BackGround.rect.height);
        Vector2 joyStickBackgroundHalfSize = joyStickBackgroundSize * 0.5f;
        float minX = m_MoveRect.position.x + joyStickBackgroundHalfSize.x;
        float minY = m_MoveRect.position.y + joyStickBackgroundHalfSize.y;
        float width = m_MoveRect.anchorMax.x - joyStickBackgroundSize.x;
        float height = m_MoveRect.anchorMax.y - joyStickBackgroundSize.y;
        BackroundMaxRect = new Rect(minX, minY, width, height);

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
        //拖动
        if (m_Dragging)
            OnDrag();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector3 touchPosition = Input.mousePosition;

        m_InRect = RectTransformUtility.RectangleContainsScreenPoint(m_MoveRect, touchPosition, m_UiCamera);

        if (!m_InRect)
            return;

        m_Dragging = true;

        Vector3 mouseWorlkPoint;
        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(m_BackGround, touchPosition, m_UiCamera, out mouseWorlkPoint))
            return;

        mouseWorlkPoint.z = m_BackGround.position.z;
        m_BackGround.transform.position = mouseWorlkPoint;
        m_TouchOrigin = touchPosition;

        if (m_Direction && m_Direction.gameObject.activeSelf)
            m_Direction.gameObject.SetActive(false);
    }

    public void OnDrag()
    {
        if (!m_Dragging)
            return;

        if (!m_InRect)
            return;

        Vector3 direction = Input.mousePosition - m_TouchOrigin;
        Vector3 normalizedDirection = direction.normalized;
        float distance = direction.magnitude;

        if (distance < 0.01f)
            return;

        m_Dragging = true;

        //求出弧度
        float radians = Mathf.Atan2(direction.y, direction.x);
        float angle = radians * Mathf.Rad2Deg;

        //移动摇杆
        if (m_Joystick != null)
        {
            if (distance > m_JoystickMoveRadius)
                distance = m_JoystickMoveRadius;

            Vector3 pos = normalizedDirection * distance;
            m_Joystick.transform.localPosition = pos;
        }

        if (m_Direction)
        {
            if (m_Direction && !m_Direction.gameObject.activeSelf)
                m_Direction.gameObject.SetActive(true);

            Vector3 eulerAngles = m_Direction.eulerAngles;
            eulerAngles.z = angle;
            m_Direction.eulerAngles = eulerAngles;
        }

        //派发事件
        if (MoveHandler != null)
        {
            data.Power = distance / m_JoystickMoveRadius;
            data.Radians = radians;
            data.Angle = angle < 0 ? 360 + angle : angle;
            data.Direction = normalizedDirection;
            MoveHandler(data);
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
        m_Dragging = false;

        m_BackGround.localPosition = m_JoyStickDefaultPosition;
        m_Joystick.transform.localPosition = Vector3.zero;
        if (m_Direction && m_Direction.gameObject.activeSelf)
            m_Direction.gameObject.SetActive(false);
    }
}
