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
    [Header("Configuración de Supabase")]
    [SerializeField] private string supabaseUrl = "https://gflucxpldvijkagerlzb.supabase.co";
    [SerializeField] private string supabaseAnonKey = "TU_KEY_AQUI"; 

    [Header("UI Login")]
    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button loginNormalBtn;
    [SerializeField] private Button loginGoogleBtn;

    private Supabase.Client _supabase;

    void Awake() {
        InitSupabase();
    }

    private void InitSupabase() {
        var options = new SupabaseOptions { AutoRefreshToken = true };
        _supabase = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);
    }

    void Start() {
        if(loginNormalBtn != null) loginNormalBtn.onClick.AddListener(() => _ = LoginNormal());
        if(loginGoogleBtn != null) loginGoogleBtn.onClick.AddListener(LoginWithGoogle);
        
        // Manejo de Deep Link para Google
        Application.deepLinkActivated += OnDeepLinkActivated;
        if (!string.IsNullOrEmpty(Application.absoluteURL)) OnDeepLinkActivated(Application.absoluteURL);
    }

    public async Task LoginNormal() {
        try {
            var session = await _supabase.Auth.SignIn(emailField.text, passwordField.text);
            GuardarSesionYContinuar(session);
        } catch (Exception ex) { Debug.LogError("Error Login: " + ex.Message); }
    }

    public async void LoginWithGoogle() {
        var authState = await _supabase.Auth.SignIn(Supabase.Gotrue.Constants.Provider.Google, new SignInOptions {
            RedirectTo = "ticoapp://login-callback" 
        });
        Application.OpenURL(authState.Uri.ToString());
    }

    private async void OnDeepLinkActivated(string url) {
        try {
            var session = await _supabase.Auth.GetSessionFromUrl(new Uri(url));
            GuardarSesionYContinuar(session);
        } catch (Exception ex) { Debug.LogWarning("Sesión inválida: " + ex.Message); }
    }

    private void GuardarSesionYContinuar(Session session) {
        if (session != null && session.User != null) {
            // Guardamos el token para que el GameManager lo use en las métricas
            PlayerPrefs.SetString("TokenSesion", session.AccessToken);
            PlayerPrefs.SetString("UserEmail", session.User.Email);
            PlayerPrefs.Save();
            
            SceneManager.LoadScene("MainMenu"); // Vas al menú, no al juego directo
        }
    }
}