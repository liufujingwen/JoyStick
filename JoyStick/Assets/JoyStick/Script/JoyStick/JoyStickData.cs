using UnityEngine;

/// <summary>
/// Joystick信息
/// </summary>
public class JoyStickData
{
    public float Radians;   //弧度
    public float Angle;     //0-360  0为右 90为上 180为做 270为下
    public float Power;     //0-1 拖拽的力度
    public Vector3 Direction;//方向
}
