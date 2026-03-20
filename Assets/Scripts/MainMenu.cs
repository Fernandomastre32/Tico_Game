// LIBRERÍAS NECESARIAS
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.Networking;       
using TMPro;                        
using System.Collections;           
using System.Text;                 
using System.Text.RegularExpressions; 

public class MainMenu : MonoBehaviour
{
    [Header("Pantallas del Menú")]
    public GameObject optionsMenu; 
    public GameObject mainMenu;    
    public GameObject login;       

    [Header("Campos de Inicio de Sesión")]
    public TMP_InputField inputUsuarioCorreo; 
    public TMP_InputField inputPassword;      
    public TextMeshProUGUI textoError;        

    // La URL de localhost ha sido eliminada para evitar conflictos con el proyecto Tico

    private bool intentandoLogin = false;

    private void Start()
    {
        OpenLogin();
        if(textoError != null) textoError.text = ""; 
    }

    // --------------------------------------------------------
    // MÉTODOS DE NAVEGACIÓN
    // --------------------------------------------------------
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

    public void OpenMainMenu() 
    { 
        mainMenu.SetActive(true); 
        optionsMenu.SetActive(false); 
        login.SetActive(false); 
    }

    // --------------------------------------------------------
    // LÓGICA DE VALIDACIÓN (DESACTIVADA PARA LOCALHOST)
    // --------------------------------------------------------
    public void LoginAttempt()
    {
        // Este método ahora solo muestra un mensaje o puede ser usado para validaciones locales
        if (intentandoLogin) return;

        string usuarioInput = inputUsuarioCorreo.text;
        string passwordInput = inputPassword.text;

        if(string.IsNullOrEmpty(usuarioInput) || string.IsNullOrEmpty(passwordInput))
        {
            if(textoError != null) textoError.text = "Por favor, llena todos los campos.";
            return; 
        }

        // Si usas Supabase, llama a AuthManager.Instance.LoginNormal() aquí en lugar de la API vieja
        Debug.Log("Intento de login local bloqueado. Usa el botón de Google.");
    }

    /* // COMENTADO PARA EVITAR ERROR CS0103 Y CONFLICTOS DE RED
    private IEnumerator EnviarLoginAPI(string usuario, string password)
    {
        string urlApiLogin = "http://localhost:3000/api/tutores/login-unity";
        string jsonDatos = "{\"usuario\":\"" + usuario + "\",\"password\":\"" + password + "\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonDatos);

        using (UnityWebRequest request = new UnityWebRequest(urlApiLogin, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            intentandoLogin = false;

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + request.error);
                if(textoError != null) textoError.text = "Error de conexión.";
            }
            else
            {
                RespuestaLogin datosLogin = JsonUtility.FromJson<RespuestaLogin>(request.downloadHandler.text);
                PlayerPrefs.SetString("TokenSesion", datosLogin.token);
                PlayerPrefs.Save();
                OpenMainMenu(); 
            }
        }
    }
    */

    // --------------------------------------------------------
    // MÉTODOS DEL JUEGO
    // --------------------------------------------------------
    public void QuitGame() 
    { 
        Debug.Log("Saliendo..."); 
        Application.Quit(); 
    }
    
    public void StartGame() 
    { 
        SceneManager.LoadScene("nivel1"); 
    }

    [System.Serializable]
    public class RespuestaLogin
    {
        public string mensaje;
        public string token;
    }
}