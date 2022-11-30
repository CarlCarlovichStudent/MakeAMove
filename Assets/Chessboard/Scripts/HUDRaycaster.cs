using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HUDRaycaster : MonoBehaviour
{
    [SerializeField] private CardDeckHandler handler;
    
    private GraphicRaycaster raycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;

    private void Awake()
    {
        raycaster = GetComponent<GraphicRaycaster>();
        eventSystem = GetComponent<EventSystem>();
        pointerEventData = new PointerEventData(eventSystem);
    }

    private void Update()
    {
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();

        raycaster.Raycast(pointerEventData, results);

        bool noCardsFound = true;
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.TryGetComponent(out Card card))
            {
                noCardsFound = false;
                if (!card.startMovement)
                {
                    handler.HandleCardHover(card);
                    if (Input.GetMouseButtonDown(0))
                    {
                        handler.HandleCardSelect(card);
                    }
                }
            }
        }

        if (noCardsFound)
        {
            handler.HandleNoCardHover();
        }
    }
}