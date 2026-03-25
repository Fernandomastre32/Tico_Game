using UnityEngine;

public class MetaLaberinto : MonoBehaviour
{
    [Header("Conexión con el Juego")]
    public GameManagerLaberinto gameManager; // <-- Necesitamos decirle quién controla el juego

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.CompareTag("Player"))
        {
            Debug.Log("¡Tico llegó a la estrella! Nivel Completado.");
            
            // <-- Llamamos a la función que muestra el panel y apaga el joystick
            if (gameManager != null)
            {
                gameManager.TerminarJuego();
            }
            else
            {
                Debug.LogWarning("Falta asignar el GameManager en la estrella.");
            }
        }
    }
}