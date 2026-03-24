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
        // Al entrar, siempre mostramos el menú principal y ocultamos opciones
        ShowMainMenu();

        // Opcional: Mostrar el nombre del usuario si lo guardamos en el login
        if (welcomeText != null && PlayerPrefs.HasKey("UserEmail"))
        {
            string email = PlayerPrefs.GetString("UserEmail");
            welcomeText.text = "¡Hola, " + email + "!";
        }
    }

    // --------------------------------------------------------
    // NAVEGACIÓN ENTRE PANELES
    // --------------------------------------------------------
    
  public void ShowMainMenu() 
    { 
        // Encendemos el menú principal y apagamos las opciones
        if(mainMenuPanel != null) mainMenuPanel.SetActive(true); 
        if(optionsMenuPanel != null) optionsMenuPanel.SetActive(false); 
    }

    public void ShowOptionsMenu() 
    { 
        // Encendemos las opciones y APAGAMOS el menú principal por completo
        if(optionsMenuPanel != null) optionsMenuPanel.SetActive(true); 
        if(mainMenuPanel != null) mainMenuPanel.SetActive(false); 
    }
    //-----------------------------------------------
    // ACCIONES DEL JUEGO
    // --------------------------------------------------------

    public void StartGame() 
    { 
        // Asegúrate de que "nivel1" esté en Build Settings
        SceneManager.LoadScene("nivel1"); 
    }

    public void Logout()
    {
        // Borramos los datos de sesión para que tenga que loguearse de nuevo
        PlayerPrefs.DeleteKey("TokenSesion");
        PlayerPrefs.DeleteKey("UserEmail");
        PlayerPrefs.Save();
        
        // Regresamos a la escena de Login (ajusta el nombre según tu escena)
        SceneManager.LoadScene("LoginScene"); 
    }

    public void QuitGame() 
    { 
        Debug.Log("Cerrando el juego Tico..."); 
        Application.Quit(); 
    }
}