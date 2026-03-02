using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class BotonBurbuja : MonoBehaviour
{
    private GameManager gameManager;
    private Button boton;
    private Image imagenBurbuja;

    void Start()
    {
        // Busca automáticamente al GameManager en la escena para no tener que arrastrarlo a cada globo
        gameManager = FindObjectOfType<GameManager>();
        
        boton = GetComponent<Button>();
        imagenBurbuja = GetComponent<Image>();

        // Le asignamos la función al botón por código
        boton.onClick.AddListener(AlSerPresionado);
    }

    void AlSerPresionado()
    {
        if (gameManager != null)
        {
            // Le enviamos al GameManager nuestra propia imagen y nuestro GameObject
            gameManager.EvaluarBurbujaTocada(imagenBurbuja.sprite, this.gameObject);
        }
    }
}