using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerToBuyUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject TowerToBuyPanel;

    public void OnPointerEnter(PointerEventData eventData)
    {
        TowerToBuyPanel.SetActive(true);

    }
    public void OnPointerExit(PointerEventData eventData)
    {
        TowerToBuyPanel.SetActive(false);
    }
}
