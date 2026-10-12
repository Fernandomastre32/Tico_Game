using UnityEngine;

public class MovimientoBurbuja : MonoBehaviour
{
    [Header("Ajustes de Movimiento")]
    public float velocidad = 2f; 
    public float amplitudX = 30f; 
    public float amplitudY = 50f; 
    
    private RectTransform rectTransform;
    private Vector2 posicionInicial;
    private float tiempoDesfase; 

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        posicionInicial = rectTransform.anchoredPosition;
        
        // El desfase hace que no todas las burbujas se muevan sincronizadas
        tiempoDesfase = Random.Range(0f, 10f); 
    }

    void Update()
    {
        // Movimiento cíclico usando Seno y Coseno
        float x = Mathf.Sin((Time.time + tiempoDesfase) * velocidad) * amplitudX;
        float y = Mathf.Cos((Time.time + tiempoDesfase) * velocidad) * amplitudY;

        rectTransform.anchoredPosition = posicionInicial + new Vector2(x, y);
    }
}