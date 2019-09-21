using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoyStickTest : MonoBehaviour
{
    public JoyStick joystick;
    public Transform moveTarget;
    public float moveSpeed = 10;

    // Use this for initialization
    void Start()
    {
        joystick.MoveHandler += OnJoyStickMove;
    }

    private void OnJoyStickMove(JoyStickData joyStickData)
    {
        Vector3 direction = joyStickData.Direction * moveSpeed * Time.deltaTime * joyStickData.Power;
        moveTarget.Translate(new Vector3(direction.x, 0, direction.y));
    }
}
