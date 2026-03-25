using UnityEngine;

public class Detector_ChoquesTico : MonoBehaviour {
    public GameManagerLaberinto manager; // <-- Asegúrate que NO tenga "_"

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Pared")) {
            manager.RegistrarGolpePared();
        }
    }
}