using UnityEngine;

public class Card : MonoBehaviour
{
    public CardBehavior behavior;
    public bool startMovement;

    private const float StartTime = 1f;
    private const float HoverTime = 0.2f;

    private bool selected;
    private float lerpTime;
    private Vector2 targetPosition;
    private Vector2 previousPosition;
    private RectTransform rectTransform;

    public void SetStartValues(float slotOffset, Vector2Int cardOffset, CardBehavior behavior)
    {
        this.behavior = behavior;
        
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0);
        rectTransform.anchorMax = new Vector2(0.5f, 0);
        rectTransform.pivot = new Vector2(0.5f, 1);
        rectTransform.anchoredPosition = Vector2.zero;

        Rect rect = rectTransform.rect;
        previousPosition = new Vector2((cardOffset.x + rect.width) * slotOffset * 0.8f, -10);
        targetPosition = new Vector2((cardOffset.x + rect.width) * slotOffset, cardOffset.y + rect.height);
        
        lerpTime = 0f;
        startMovement = true;
        selected = false;
    }

    // Hover
    public void Hover()
    {
        if (startMovement) return;
        
        previousPosition = targetPosition;
        targetPosition *= new Vector2(1, 1.1f);
        lerpTime = 1f - lerpTime;
    }

    public void Select()
    {
        if (startMovement) return;
        
        selected = true;
    }

    public void Unhover()
    {
        if (startMovement) return;
        if (selected) return;
        
        previousPosition = targetPosition;
        targetPosition /= new Vector2(1, 1.1f);
        lerpTime = 1f - lerpTime;
    }

    public void Deselect()
    {
        if (startMovement) return;
        
        selected = false;
        Unhover();
    }

    private void Update()
    {
        if (behavior == null) return;
        if (lerpTime >= 1f) return;

        rectTransform.anchoredPosition = Vector2.Lerp(previousPosition, targetPosition, lerpTime);

        lerpTime += Time.deltaTime / (startMovement ? StartTime : HoverTime);
        
        if (lerpTime >= 1f)
        {
            rectTransform.anchoredPosition = targetPosition;
            lerpTime = 1f;
            startMovement = false;
        }
    }
}