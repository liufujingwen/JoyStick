using UnityEngine;
using System;
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

    //拖拽开始
    public Action beginHandler { get; set; }
    //拖拽中
    public Action<JoyStickData> moveHandler { get; set; }
    //拖拽结束
    public Action endHandle { get; set; }

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
        joyStickEvent.pointerDownHandler = OnPointerDown;
        joyStickEvent.beginDragHandler = BeginDrag;
        joyStickEvent.dragHandler = OnDrag;
        joyStickEvent.pointerUpHandler = OnPointerUp;

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

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector3 touchPosition = m_UiCamera.WorldToScreenPoint(m_Joystick.transform.position);

        //是否在摇杆控制的最大范围
        m_InRect = RectTransformUtility.RectangleContainsScreenPoint(m_MoveRect, touchPosition, eventData.enterEventCamera);

        if (!m_InRect)
            return;

        Vector3 mousePosition;
        //是否在摇杆圆形底座范围
        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(m_BackGround, touchPosition, eventData.enterEventCamera, out mousePosition))
            return;

        mousePosition.z = m_BackGround.position.z;

        //修正x,y
        mousePosition.x = Mathf.Clamp(mousePosition.x, m_BackgroundRectCorners[0].x, m_BackgroundRectCorners[2].x);
        mousePosition.y = Mathf.Clamp(mousePosition.y, m_BackgroundRectCorners[3].y, m_BackgroundRectCorners[1].y);

        m_MouseOriginScreenPoint = RectTransformUtility.WorldToScreenPoint(m_UiCamera, mousePosition);
        m_BackGround.transform.position = mousePosition;

        if (m_Direction && m_Direction.gameObject.activeSelf)
            m_Direction.gameObject.SetActive(false);
    }

    public void BeginDrag(PointerEventData eventData)
    {
        if (!m_InRect)
            return;

        m_BeginDrag = true;
        beginHandler?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!m_BeginDrag)
            return;

        if (!m_InRect)
            return;

        if (m_Direction && m_Direction.gameObject.activeSelf != m_Dragging)
            m_Direction.gameObject.SetActive(m_Dragging);
        Vector3 mousePosition = new Vector3(eventData.position.x, eventData.position.y, 0);
        Vector3 direction = mousePosition - m_MouseOriginScreenPoint;
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

        if (m_Direction)
        {
            Vector3 eulerAngles = m_Direction.eulerAngles;
            eulerAngles.z = angle;
            m_Direction.eulerAngles = eulerAngles;
        }

        if (distance > m_JoystickMoveRadius)
            distance = m_JoystickMoveRadius;

        //移动摇杆
        if (m_Joystick != null)
        {
            Vector3 pos = normalizedDirection * distance;
            m_Joystick.transform.localPosition = pos;
        }

        if (m_ThreeD)
        {
            float y = normalizedDirection.y;
            normalizedDirection.y = normalizedDirection.z;
            normalizedDirection.z = y;
        }

        m_Data.Power = distance / m_JoystickMoveRadius; ;
        m_Data.Angle = angle < 0 ? 360 + angle : angle;
        m_Data.Direction = normalizedDirection;

        //派发事件
        moveHandler?.Invoke(m_Data);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Reset();
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        if (m_BeginDrag)
        {
            endHandle?.Invoke();
        }

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
