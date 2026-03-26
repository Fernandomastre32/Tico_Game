using UnityEngine;

public class AjustePantallaPro : MonoBehaviour
{
    public SpriteRenderer fondoLaberinto; // Arrastra aquí el fondo verde de tu nivel

    void Start()
    {
        if (fondoLaberinto == null) return;

        // Calculamos cuánto mide el fondo en unidades de Unity
        float anchoFondo = fondoLaberinto.bounds.size.x;
        float altoFondo = fondoLaberinto.bounds.size.y;

        // Calculamos la relación de aspecto del celular actual
        float aspectoPantalla = (float)Screen.width / (float)Screen.height;
        float aspectoFondo = anchoFondo / altoFondo;

        if (aspectoPantalla >= aspectoFondo) {
            Camera.main.orthographicSize = altoFondo / 2f;
        } else {
            float diferencia = aspectoFondo / aspectoPantalla;
            Camera.main.orthographicSize = (altoFondo / 2f) * diferencia;
        }
    }
}