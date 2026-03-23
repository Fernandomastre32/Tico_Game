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
    private string supabaseUrl = "https://gflucxpldvijkagerlzb.supabase.co";
    private string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImdmbHVjeHBsZHZpamthZ2VybHpiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzM0NDk2OTQsImV4cCI6MjA4OTAyNTY5NH0.vYYELn2ofGJRHPsFE4ZmCsq9a6-DMVLNQ6vn7zMc4vo"; 

    [Header("UI Login")]
    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button loginNormalBtn;
    [SerializeField] private Button loginGoogleBtn;
    [SerializeField] private TextMeshProUGUI errorText;

    private Supabase.Client _supabase;

    async void Awake() 
    {
        var options = new SupabaseOptions { AutoRefreshToken = true };
        _supabase = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);
        await _supabase.InitializeAsync(); 
    }

    void Start() 
    {
        if(loginNormalBtn != null) loginNormalBtn.onClick.AddListener(() => _ = LoginNormal());
        if(loginGoogleBtn != null) loginGoogleBtn.onClick.AddListener(LoginWithGoogle);
        
        Application.deepLinkActivated += OnDeepLinkActivated;
        if (!string.IsNullOrEmpty(Application.absoluteURL)) OnDeepLinkActivated(Application.absoluteURL);
    }

    public async Task LoginNormal() 
    {
        try 
        {
            UpdateError("Conectando...");
            // Intentamos entrar directamente
            var session = await _supabase.Auth.SignIn(emailField.text, passwordField.text);
            
            if (session != null) {
                GuardarSesionYContinuar(session);
            }
        } 
        catch (Exception ex) 
        { 
            Debug.Log("Usuario no existe o error de red, intentando registro...");
            try {
                // Registro automático para usuarios nuevos/viejos
                var session = await _supabase.Auth.SignUp(emailField.text, passwordField.text);
                if (session != null) GuardarSesionYContinuar(session);
            } catch (Exception e) {
                UpdateError("Error: " + e.Message);
                Debug.LogError("Error Total: " + e.Message);
            }
        }
    }

    public async void LoginWithGoogle() 
    {
        UpdateError("Abriendo Google...");
        var options = new SignInOptions { RedirectTo = "com.fer.ticoproject://login-callback" };
        var authState = await _supabase.Auth.SignIn(Supabase.Gotrue.Constants.Provider.Google, options);
        Application.OpenURL(authState.Uri.ToString());
    }

    private async void OnDeepLinkActivated(string url) 
    {
        try {
            var session = await _supabase.Auth.GetSessionFromUrl(new Uri(url));
            if (session != null) GuardarSesionYContinuar(session);
        } catch (Exception ex) { Debug.LogWarning(ex.Message); }
    }

    private void GuardarSesionYContinuar(Session session) 
{
    // Guardamos los datos localmente
    PlayerPrefs.SetString("TokenSesion", session.AccessToken);
    PlayerPrefs.SetString("UserEmail", session.User.Email);
    PlayerPrefs.Save();
    
    Debug.Log("¡Sesión capturada! Cambiando de escena de forma segura...");
    
    // Usar una corrutina es MUCHO más seguro para cambiar de escena tras un await
    StartCoroutine(CargarEscenaMenuSeguro());
}

private System.Collections.IEnumerator CargarEscenaMenuSeguro()
{
    // Esperamos un frame para asegurar que las tareas asíncronas terminen
    yield return null; 
    
    // Cargamos la escena
    SceneManager.LoadScene("Flujo_Menu"); 
}

    private void UpdateError(string msg) { if (errorText != null) errorText.text = msg; }
}