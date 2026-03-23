using UnityEngine;

public class MoverUIConTeclado : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 posicionOriginal;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null) posicionOriginal = rectTransform.anchoredPosition;
    }

    void Update()
    {
        // Si el teclado está visible en el celular
        if (TouchScreenKeyboard.visible)
        {
            float keyboardHeight = TouchScreenKeyboard.area.height;
            if (keyboardHeight > 0)
            {
                // Sube el panel un 40% de la altura del teclado
                rectTransform.anchoredPosition = new Vector2(posicionOriginal.x, posicionOriginal.y + (keyboardHeight * 0.4f));
            }
        }
        else
        {
            // Vuelve a su lugar cuando se cierra el teclado
            if (rectTransform != null) rectTransform.anchoredPosition = posicionOriginal;
        }
    }
}