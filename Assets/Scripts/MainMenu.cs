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

    private string urlApiLogin = "http://localhost:3000/api/tutores/login-unity"; 

    // NUEVO: El candado para evitar que el jugador presione el botón múltiples veces
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
    // LÓGICA DE VALIDACIÓN INSTANTÁNEA
    // --------------------------------------------------------
    public void LoginAttempt()
    {
        // 0. Si el candado está activado, ignoramos cualquier clic extra
        if (intentandoLogin) return;

        string usuarioInput = inputUsuarioCorreo.text;
        string passwordInput = inputPassword.text;

        // 1. REVISIÓN DE CAMPOS VACÍOS
        if(string.IsNullOrEmpty(usuarioInput) || string.IsNullOrEmpty(passwordInput))
        {
            if(textoError != null) textoError.text = "Por favor, llena todos los campos.";
            return; 
        }

        // 2. REVISIÓN INTELIGENTE DE FORMATO
        if (usuarioInput.Contains("@"))
        {
            bool esCorreoValido = Regex.IsMatch(usuarioInput, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!esCorreoValido)
            {
                if(textoError != null) textoError.text = "Formato de correo inválido.";
                return; 
            }
        }
        else
        {
            if (usuarioInput.Length < 3)
            {
                if(textoError != null) textoError.text = "El nombre de usuario es muy corto.";
                return; 
            }
        }

        // 3. ENVIAR A LA BASE DE DATOS
        // Activamos el candado e informamos al usuario
        intentandoLogin = true;
        if(textoError != null) textoError.text = "Conectando...";
        
        StartCoroutine(EnviarLoginAPI(usuarioInput, passwordInput));
    }

    // Corrutina para la conexión
    private IEnumerator EnviarLoginAPI(string usuario, string password)
    {
        string jsonDatos = "{\"usuario\":\"" + usuario + "\",\"password\":\"" + password + "\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonDatos);

        // NUEVO: El bloque "using" asegura que Unity destruya la conexión web en cuanto termine.
        // Esto evita que la interfaz gráfica se congele esperando un cierre manual.
        using (UnityWebRequest request = new UnityWebRequest(urlApiLogin, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            // Desactivamos el candado sin importar si hubo error o éxito
            intentandoLogin = false;

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + request.error);
                if(textoError != null) textoError.text = "Usuario o contraseña incorrectos.";
            }else
            {
                Debug.Log("Login Exitoso: " + request.downloadHandler.text);
                if(textoError != null) textoError.text = "";
                
                // --- NUEVO: GUARDAR EL TOKEN EN PLAYERPREFS ---
                // 1. Traducimos el texto de la API a nuestro objeto de Unity
                RespuestaLogin datosLogin = JsonUtility.FromJson<RespuestaLogin>(request.downloadHandler.text);
                
                // 2. Guardamos el token en la memoria del juego (como si fuera una cookie)
                PlayerPrefs.SetString("TokenSesion", datosLogin.token);
                PlayerPrefs.Save(); // Forzamos el guardado inmediato
                
                Debug.Log("¡Gafete guardado en el bolsillo! Token: " + datosLogin.token);
                // ----------------------------------------------

                // Cambiamos de pantalla instantáneamente
                OpenMainMenu(); 
            }
        } // Aquí termina el "using" y se libera la memoria automáticamente.
    }

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