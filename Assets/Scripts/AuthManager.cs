using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Supabase;
using System;
using System.Threading.Tasks;
using Supabase.Gotrue;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    // Datos de tu proyecto Supabase
    private string supabaseUrl = "https://gflucxpldvijkagerlzb.supabase.co";
    private string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImdmbHVjeHBsZHZpamthZ2VybHpiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzM0NDk2OTQsImV4cCI6MjA4OTAyNTY5NH0.vYYELn2ofGJRHPsFE4ZmCsq9a6-DMVLNQ6vn7zMc4vo"; 

    [Header("UI Login")]
    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button loginNormalBtn;
    [SerializeField] private Button loginGoogleBtn;

    private Supabase.Client _supabase;

    async void Awake() 
    {
        await InitSupabase();
    }

    private async Task InitSupabase() 
    {
        var options = new SupabaseOptions { AutoRefreshToken = true };
        _supabase = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);
        
        // Inicialización necesaria para el SDK de Unity
        await _supabase.InitializeAsync(); 
    }

    void Start() 
    {
        // Listeners de botones
        if(loginNormalBtn != null) loginNormalBtn.onClick.AddListener(() => _ = LoginNormal());
        if(loginGoogleBtn != null) loginGoogleBtn.onClick.AddListener(LoginWithGoogle);
        
        // Configuración de Deep Link
        Application.deepLinkActivated += OnDeepLinkActivated;
        
        // Verificar si la app se abrió mediante un Deep Link (p. ej. después del login de Google)
        if (!string.IsNullOrEmpty(Application.absoluteURL)) 
        {
            OnDeepLinkActivated(Application.absoluteURL);
        }
    }

    public async Task LoginNormal() 
    {
        try 
        {
            var session = await _supabase.Auth.SignIn(emailField.text, passwordField.text);
            GuardarSesionYContinuar(session);
        } 
        catch (Exception ex) 
        { 
            Debug.LogError("Error Login Normal: " + ex.Message); 
        }
    }

    public async void LoginWithGoogle() 
    {
        try 
        {
            // IMPORTANTE: El RedirectTo debe coincidir con tu Panel de Supabase y tu ID de Google
            var options = new SignInOptions {
                RedirectTo = "com.fer.ticoproject://login-callback" 
            };

            var authState = await _supabase.Auth.SignIn(Supabase.Gotrue.Constants.Provider.Google, options);
            
            // Abre el navegador del dispositivo para el login de Google
            if (authState.Uri != null)
            {
                Application.OpenURL(authState.Uri.ToString());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error iniciando Google Login: " + ex.Message);
        }
    }

    private async void OnDeepLinkActivated(string url) 
    {
        try 
        {
            Debug.Log("Deep Link recibido: " + url);
            
            // Extrae la sesión de la URL que devuelve Supabase
            var session = await _supabase.Auth.GetSessionFromUrl(new Uri(url));
            
            if (session != null)
            {
                GuardarSesionYContinuar(session);
            }
        } 
        catch (Exception ex) 
        { 
            Debug.LogWarning("Error procesando sesión desde URL: " + ex.Message); 
        }
    }

    private void GuardarSesionYContinuar(Session session) 
    {
        if (session != null && session.User != null) 
        {
            // Guardar datos básicos localmente
            PlayerPrefs.SetString("TokenSesion", session.AccessToken);
            PlayerPrefs.SetString("UserEmail", session.User.Email);
            PlayerPrefs.Save();
            
            Debug.Log("Login exitoso para: " + session.User.Email);
            
            // Cambiar a la escena del menú principal
            SceneManager.LoadScene("Flujo_Menu"); 
        }
    }
}