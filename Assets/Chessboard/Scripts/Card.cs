using UnityEngine;

public class Card : MonoBehaviour
{
    private CardBehavior behavior;
    private Canvas canvas;
    private int slot = -1;
    private int totalSlots;
    private float lerpTime;
    private Vector2Int cardOffset;
    private Vector2 targetPosition;
    private Vector2 previousPosition;
    private RectTransform rectTransform;

    public void SetStartValues(int slot, int totalSlots, Vector2Int offset)
    {
        this.slot = slot;
        this.totalSlots = totalSlots;
        cardOffset = offset;
        
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0);
        rectTransform.anchorMax = new Vector2(0.5f, 0);
        rectTransform.pivot = new Vector2(0.5f, 1);
        rectTransform.anchoredPosition = Vector2.zero;

        previousPosition = new Vector2((offset.x + rectTransform.rect.width) * (slot + 0.5f - totalSlots / 2f) * 0.8f, -10);
        targetPosition = new Vector2((offset.x + rectTransform.rect.width) * (slot + 0.5f - totalSlots / 2f), offset.y + rectTransform.rect.height);
        lerpTime = 0f;
    }

    public void Update()
    {
        if (slot < 0) return;
        if (lerpTime >= 1f) return;

        rectTransform.anchoredPosition = Vector2.Lerp(previousPosition, targetPosition, lerpTime);
        
        lerpTime += Time.deltaTime;
        if (lerpTime >= 1f)
        {
            rectTransform.anchoredPosition = targetPosition;
        }
    }
}