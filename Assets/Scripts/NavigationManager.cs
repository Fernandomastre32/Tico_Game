using UnityEngine;
using UnityEngine.SceneManagement;

public class NavigationManager : MonoBehaviour
{
    public void IrAJugar(string nombreNivel) {
        SceneManager.LoadScene(nombreNivel);
    }

    public void IrAlMenuPrincipal() {
        SceneManager.LoadScene("MainMenu");
    }

    public void SalirDelJuego() {
        Application.Quit();
        Debug.Log("Saliendo...");
    }

    // Para los paneles dentro de la misma escena
    public void AlternarPanel(GameObject panel) {
        panel.SetActive(!panel.activeSelf);
    }
}