using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
 
 
[RequireComponent(typeof(Selectable))]
    public class buttonSelectionFix : MonoBehaviour, IPointerEnterHandler, IDeselectHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!EventSystem.current.alreadySelecting)
                EventSystem.current.SetSelectedGameObject(this.gameObject);
        }
 
        public void OnPointerExit(PointerEventData eventData)
        {
            /*
            if (EventSystem.current.currentSelectedGameObject == this.gameObject)
                if (!EventSystem.current.alreadySelecting)
                    EventSystem.current.SetSelectedGameObject(this.gameObject);
            */
        }
 
        public void OnDeselect(BaseEventData eventData)
        {
            this.GetComponent<Selectable>().OnPointerExit(null);
        }
    }
