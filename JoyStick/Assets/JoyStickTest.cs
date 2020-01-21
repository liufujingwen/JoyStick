using UnityEngine;

public class JoyStickTest : MonoBehaviour
{
    public JoyStick joystick;
    public Transform moveTarget;
    public float moveSpeed = 5f;
    private JoyStickData m_JoyStickData;

    private Animator m_Animator;

    // Use this for initialization
    private void Start()
    {
        m_Animator = moveTarget.GetComponent<Animator>();
        joystick.beginHandler += OnBegin;
        joystick.moveHandler += OnJoyStickMove;
        joystick.endHandle += OnEnd;

        m_Animator.CrossFade("Idle", 0.1f);
    }

    private void OnBegin()
    {
        m_JoyStickData = null;
        m_Animator.CrossFade("WalkFront", 0.1f);

    }

    private void OnEnd()
    {
        m_JoyStickData = null;
        m_Animator.CrossFade("Idle", 0.1f);
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
        moveTarget.rotation = Quaternion.Lerp(moveTarget.transform.rotation, Quaternion.LookRotation(m_JoyStickData.Direction), Time.deltaTime * 10);
        //向前移动
        moveTarget.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }
}