using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoyStickEvent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Action<PointerEventData> PointerDownHandler;
    public Action<PointerEventData> PointerUpHandler;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (null != PointerDownHandler) PointerDownHandler(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (null != PointerUpHandler) PointerUpHandler(eventData);
    }
}
