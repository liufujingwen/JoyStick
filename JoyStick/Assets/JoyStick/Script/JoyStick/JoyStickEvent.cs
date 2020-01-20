using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoyStickEvent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,IBeginDragHandler,IDragHandler
{
    public Action<PointerEventData> pointerDownHandler;
    public Action<PointerEventData> beginDragHandler;
    public Action<PointerEventData> dragHandler;
    public Action<PointerEventData> pointerUpHandler;

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownHandler?.Invoke(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        beginDragHandler?.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragHandler?.Invoke(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerUpHandler?.Invoke(eventData);
    }
}
