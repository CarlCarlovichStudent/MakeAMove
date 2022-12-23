using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public enum textCollection
{
    Tutorial = 0,
    GamePlay = 1,
    Rematch = 2
}

public class TextManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] GraphicTextInTutorial;
    [SerializeField] private TextMeshProUGUI[] GraphicTextInGamePlay;
    [SerializeField] private TextMeshProUGUI[] GraphicTextInRematch;

    public TextMeshProUGUI GetText(string textName, textCollection collection)
    {
        var text = from X in GetTextCollection(collection) where X.name.Equals(textName) select X;
        try
        {
            return text.Single();
        }
        catch
        {
            Debug.Log(textName);
        }

        throw new Exception();
    }

    public void SetText(string newText, string textName, textCollection collection)
    {
        GetText(textName, collection).text = newText;
    }
    
    public void EnableText(string textName, textCollection collection, bool state)
    {
        GetText(textName, collection).enabled = state;
    }

    public void ResetTexts(textCollection collection)
    {
        foreach (TextMeshProUGUI c in GetTextCollection(collection))
        {
            EnableText(c.name,collection,false);
        }
    }

    public TextMeshProUGUI[] GetTextCollection(textCollection collection)
    {
        List<TextMeshProUGUI> collectionSelection = new List<TextMeshProUGUI>();
        
        switch (collection)
        {
            case textCollection.Tutorial:
                collectionSelection.InsertRange(0,GraphicTextInTutorial);
                break;
            case textCollection.GamePlay:
                collectionSelection.InsertRange(0,GraphicTextInGamePlay);
                break;
            case textCollection.Rematch:
                collectionSelection.InsertRange(0,GraphicTextInRematch);
                break;
        }

        return collectionSelection.ToArray();
    }
}
