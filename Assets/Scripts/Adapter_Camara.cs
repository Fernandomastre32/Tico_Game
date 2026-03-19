using UnityEngine;

public class CamaraAdaptable : MonoBehaviour
{
    [Header("Medidas de tu Laberinto")]
    [Tooltip("El ancho total que quieres que siempre se vea")]
    public float anchoNivel = 16f; 
    [Tooltip("El alto total que quieres que siempre se vea")]
    public float altoNivel = 10f;  

    void Start()
    {
        AjustarCamara();
    }

    // También lo ponemos en Update por si cambias el tamaño de tu simulador 
    // en Unity mientras haces pruebas. En el juego final, con Start es suficiente.
    void Update()
    {
        AjustarCamara();
    }

    void AjustarCamara()
    {
        // Calculamos la proporción de la pantalla del celular actual
        float proporcionPantalla = (float)Screen.width / (float)Screen.height;
        float proporcionNivel = anchoNivel / altoNivel;

        if (proporcionPantalla >= proporcionNivel)
        {
            // Si la pantalla es ancha (tipo Tablet), ajustamos por la altura
            Camera.main.orthographicSize = altoNivel / 2f;
        }
        else
        {
            // Si la pantalla es alta/estrecha (tipo Celular horizontal), ajustamos por la anchura
            Camera.main.orthographicSize = (anchoNivel / proporcionPantalla) / 2f;
        }
    }
}