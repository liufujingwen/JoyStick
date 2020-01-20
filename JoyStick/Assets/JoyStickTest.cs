using UnityEngine;

public class JoyStickTest : MonoBehaviour
{
    public JoyStick joystick;
    public Transform moveTarget;
    public float moveSpeed = 10;
    private JoyStickData m_JoyStickData;

    // Use this for initialization
    private void Start()
    {
        joystick.beginHandler += OnBegin;
        joystick.moveHandler += OnJoyStickMove;
        joystick.endHandle += OnEnd;
    }

    private void OnBegin()
    {
        m_JoyStickData = null;
    }

    private void OnEnd()
    {
        m_JoyStickData = null;
    }

    private void OnJoyStickMove(JoyStickData joyStickData)
    {
        m_JoyStickData = joyStickData;
    }

    private void Update()
    {
        if (m_JoyStickData == null)
            return;

        //控制转向
        moveTarget.rotation = Quaternion.Lerp(moveTarget.transform.rotation, Quaternion.LookRotation(m_JoyStickData.Direction), Time.deltaTime * 100);
        //向前移动
        moveTarget.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }
}