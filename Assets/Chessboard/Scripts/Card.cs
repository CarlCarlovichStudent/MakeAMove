using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Card : MonoBehaviour
{
    public CardBehavior behavior;
    public bool forcedMovement; // Should be property

    private const float HoverTime = 0.3f;
    private const float StartTime = 1f;

    public int ManaCost { get; set; }
    
    private bool selected;
    private bool wasSelected;
    private float lerpTime;
    private float forcedTime;
    private int hoverOffset;
    private Vector2 targetPosition;
    private Vector2 previousPosition;
    private RectTransform rectTransform;
    private AudioPlay audioHoverCard;
    private AudioPlay audioSelectCard;

    public void SetStartValues(float slotOffset, Vector3Int cardOffset, CardBehavior behavior,AudioPlay audioHoverCard,AudioPlay audioSelectCard, bool start = false) // TODO: start yay or nay?
    {
        this.behavior = behavior;
        ManaCost = behavior.ManaCost;
        hoverOffset = cardOffset.z;
        this.audioHoverCard = audioHoverCard;
        this.audioSelectCard = audioSelectCard;
        
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0);
        rectTransform.anchorMax = new Vector2(0.5f, 0);
        rectTransform.pivot = new Vector2(0.5f, 1);
        rectTransform.anchoredPosition = Vector2.zero;

        Rect rect = rectTransform.rect;
        previousPosition = new Vector2((cardOffset.x + rect.width) * slotOffset * (start ? 0.75f : 1), -rect.height);
        targetPosition = new Vector2((cardOffset.x + rect.width) * slotOffset, cardOffset.y + rect.height);
        
        lerpTime = 0f;
        forcedMovement = true;
        forcedTime = StartTime;
        selected = false;
        wasSelected = false;
    }

    // Use card
    public void Use(float exitTime)
    {
        if (this != null)
        {
            Destroy(gameObject, exitTime);

            previousPosition = targetPosition;
            targetPosition += new Vector2(0, hoverOffset);
            
            lerpTime = 0f;
            forcedMovement = true;
            forcedTime = exitTime;
            
            targetPosition -= new Vector2(0, rectTransform.rect.height + rectTransform.position.y);
        }
    }

    // Hover
    public void Hover()
    {
        if (forcedMovement) return;
        
        audioHoverCard.PlayAudio();
        
        previousPosition = targetPosition;
        targetPosition += new Vector2(0, hoverOffset);
        lerpTime = 1f - lerpTime;
    }

    public void Select()
    {
        if (forcedMovement) return;

        selected = true;

        audioSelectCard.PlayAudio();
    }

    public void Unhover()
    {
        if (forcedMovement) return;
        if (selected) return;

        previousPosition = targetPosition;
        targetPosition -= new Vector2(0, hoverOffset);
        lerpTime = 1f - lerpTime;
    }

    public void Deselect()
    {
        if (forcedMovement) return;
        
        selected = false;
        Unhover();
    }

    private void Update()
    {
        if (behavior == null) return;
        if (lerpTime >= 1f) return;

        rectTransform.anchoredPosition = Vector2.Lerp(previousPosition, targetPosition, lerpTime);

        lerpTime += Time.deltaTime / (forcedMovement ? forcedTime : HoverTime);
        
        if (lerpTime >= 1f)
        {
            rectTransform.anchoredPosition = targetPosition;
            lerpTime = 1f;
            forcedMovement = false;
        }
    }
}