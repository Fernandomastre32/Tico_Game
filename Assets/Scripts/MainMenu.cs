using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Pantallas del Menú")]
    public GameObject optionsMenu;
    public GameObject mainMenu;
    public GameObject login;

    private void Start()
    {
        // Se supone que debes iniciar en Login y los demás desactivados
        OpenLogin();
    }

    public void OpenOptionsMenu()
    {
        optionsMenu.SetActive(true);
        mainMenu.SetActive(false);
        login.SetActive(false);
    }

    public void CloseOptionsMenu()
    {
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void OpenLogin()
    {
        login.SetActive(true);
        mainMenu.SetActive(false);
        optionsMenu.SetActive(false);
    }

    // Esta función la pondrías en el botón de Iniciar Sesión de tu Login
    public void LoginAttempt()
    {
        // Aquí puedes poner la lógica de si los campos tienen información
        // Se desactiva login y se activa menu pero sigue desactivado options
        OpenMainMenu();
    }

    public void OpenMainMenu()
    {
        mainMenu.SetActive(true);
        optionsMenu.SetActive(false);
        login.SetActive(false);
    }

    public void QuitGame()
    {
        // En el editor de Unity no se cierra, pero esto te avisará que sí funciona
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    public void StartGame()
    {
        // Cambia de escena. Asegúrate que la escena se llame así en el Build Settings
        SceneManager.LoadScene("nivel1");
    }
}
