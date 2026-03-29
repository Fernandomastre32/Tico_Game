using UnityEngine;
using UnityEngine.SceneManagement; 
using TMPro; 

public class MainMenu : MonoBehaviour
{
    [Header("Paneles de Navegación")]
    public GameObject mainMenuPanel;    
    public GameObject optionsMenuPanel; 

    [Header("Información del Usuario")]
    public TextMeshProUGUI welcomeText;

    private void Start()
    {
        // 1. Al entrar, siempre mostramos el menú principal y ocultamos opciones
        ShowMainMenu();

        // 2. Mostrar el nombre del usuario si lo guardamos en el login
        if (welcomeText != null && PlayerPrefs.HasKey("UserEmail"))
        {
            string email = PlayerPrefs.GetString("UserEmail");
            welcomeText.text = "¡Hola, " + email + "!";
        } // <--- AQUÍ FALTABA ESTA LLAVE PARA CERRAR EL IF

        // 3. Control de Música: Lo ponemos FUERA del if para que siempre suene
        if (AudioManager.instance != null)
        {
            // Usamos musicaMenu porque estamos en el menú principal
            AudioManager.instance.CambiarMusica(AudioManager.instance.musicaMenu);
        }
    }

    // --------------------------------------------------------
    // NAVEGACIÓN ENTRE PANELES
    // --------------------------------------------------------
    
    public void ShowMainMenu() 
    { 
        if(mainMenuPanel != null) mainMenuPanel.SetActive(true); 
        if(optionsMenuPanel != null) optionsMenuPanel.SetActive(false); 
    }

    public void ShowOptionsMenu() 
    { 
        if(optionsMenuPanel != null) optionsMenuPanel.SetActive(true); 
        if(mainMenuPanel != null) mainMenuPanel.SetActive(false); 
    }

    // --------------------------------------------------------
    // ACCIONES DEL JUEGO
    // --------------------------------------------------------

    public void StartGame() 
    { 
        // Carga la escena donde el niño elige el nivel
        SceneManager.LoadScene("flujo_Niveles"); 
    }

    public void Logout()
    {
        // Borramos los datos de sesión
        PlayerPrefs.DeleteKey("TokenSesion");
        PlayerPrefs.DeleteKey("UserEmail");
        PlayerPrefs.Save();
        
        // Regresamos a la escena de Login
        SceneManager.LoadScene("LoginScene"); 
    }

    public void QuitGame() 
    { 
        Debug.Log("Cerrando el juego Tico..."); 
        Application.Quit(); 
    }
}