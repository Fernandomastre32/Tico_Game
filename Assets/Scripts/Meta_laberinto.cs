using UnityEngine;

public class MetaLaberinto : MonoBehaviour
{
    // Esta función se activa AUTOMÁTICAMENTE cuando ALGUIEN entra en la zona de la estrella (Trigger)
    private void OnTriggerEnter2D(Collider2D otro)
    {
        // Revisamos si el que entró tiene el gafete (Tag) de "Player" (Tico)
        if (otro.CompareTag("Player"))
        {
            // ¡Magia de ganar!
            Debug.Log("¡Tico llegó a la estrella! Nivel Completado.");
            
            // Aquí puedes agregar lo que hacías con los globos, 
            // como mostrar un panel de victoria o llamar a tu GameManager.
        }
    }
}