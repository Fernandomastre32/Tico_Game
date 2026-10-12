using UnityEngine;
using UnityEngine.EventSystems; 
using TMPro; 

public class HighlightButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Color normalColour = Color.black;
    [SerializeField] private Color highlightColour = Color.black;

    private TextMeshProUGUI textoComponente;

    void Awake()
    {
        textoComponente = GetComponentInChildren<TextMeshProUGUI>();
        
        if (textoComponente != null)
        {
            textoComponente.color = normalColour;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (textoComponente != null)
        {
            textoComponente.color = highlightColour;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (textoComponente != null)
        {
            textoComponente.color = normalColour;
        }
    }
}