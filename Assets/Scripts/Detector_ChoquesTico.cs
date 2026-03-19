using UnityEngine;

public class DetectorChoquesTico : MonoBehaviour
{
    private GameManagerLaberinto gameManager;

    void Start()
    {
        // Busca al cerebro en la pantalla
        gameManager = FindObjectOfType<GameManagerLaberinto>();
    }

    // Esta función se activa SOLA cada vez que el cuerpo físico de Tico choca contra una pared
    private void OnCollisionEnter2D(Collision2D choque)
    {
        if (gameManager != null)
        {
            // Le avisa al cerebro que sume un golpe
            gameManager.RegistrarGolpePared();
        }
    }
}