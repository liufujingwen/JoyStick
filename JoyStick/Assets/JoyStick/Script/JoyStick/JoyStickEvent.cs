using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoyStickEvent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,IBeginDragHandler,IDragHandler
{
    public Action<PointerEventData> PointerDownHandler;
    public Action<PointerEventData> BeginDragHandler;
    public Action<PointerEventData> PointerUpHandler;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (null != PointerDownHandler) PointerDownHandler(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (null != BeginDragHandler) BeginDragHandler(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (null != PointerUpHandler) PointerUpHandler(eventData);
    }
}
