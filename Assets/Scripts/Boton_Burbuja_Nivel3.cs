using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BotonBurbujaNivel3 : MonoBehaviour
{
    private GameManagerNivel3 gameManager;
    private Button boton;
    private Image imagen;
    private RectTransform rectTransform;

    [Header("Configuración de Reaparición")]
    public float tiempoEspera = 5f;
    public float minX = -400f, maxX = 400f;
    public float minY = -200f, maxY = 200f;

    void Start()
    {
        gameManager = Object.FindFirstObjectByType<GameManagerNivel3>();
        boton = GetComponent<Button>();
        imagen = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        boton.onClick.AddListener(AlSerPresionado);
    }

    void AlSerPresionado()
    {
        if (gameManager != null)
        {
            // 1. Preguntamos al manager si somos el color correcto
            bool esCorrecto = gameManager.EvaluarBurbujaTocada(imagen.sprite);
            
            // 2. Solo si es correcto, iniciamos la rutina de desaparecer y reaparecer
            if (esCorrecto)
            {
                StartCoroutine(RutinaRespawn());
            }
            // Si es incorrecto, no pasa nada (la burbuja se queda ahí)
        }
    }

    private IEnumerator RutinaRespawn()
    {
        // Desaparecer
        boton.interactable = false;
        imagen.enabled = false;

        yield return new WaitForSeconds(tiempoEspera);

        // Mover a nueva posición
        float nuevaX = Random.Range(minX, maxX);
        float nuevaY = Random.Range(minY, maxY);
        rectTransform.anchoredPosition = new Vector2(nuevaX, nuevaY);

        // Reaparecer
        imagen.enabled = true;
        boton.interactable = true;
    }
}