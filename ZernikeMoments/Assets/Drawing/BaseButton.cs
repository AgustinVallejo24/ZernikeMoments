using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BaseButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    //[SerializeField] protected SoundManager soundManager;
    public abstract void OnPointerDown(PointerEventData eventData);
    public abstract void OnPointerUp(PointerEventData eventData);
}
