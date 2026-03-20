using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Supabase;
using System;
using System.Threading.Tasks;
using Supabase.Gotrue;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    [Header("Configuración de Supabase")]
    [SerializeField] private string supabaseUrl = "https://gflucxpldvijkagerlzb.supabase.co";
    [SerializeField] private string supabaseAnonKey = "sb_publishable_jE1MuciLMYV5ZQsBBJdLWQ_V891F9cC"; // Recuerda rotar tu key si fue pública

    [Header("Referencias de UI")]
    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button loginNormalBtn;
    [SerializeField] private Button loginGoogleBtn;

    private Supabase.Client _supabase;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitSupabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitSupabase()
    {
        var options = new SupabaseOptions
        {
            AutoConnectRealtime = true,
            AutoRefreshToken = true
        };
        _supabase = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);
    }

    void Start()
    {
        // IMPORTANTE: Escuchar cuando la app se abre desde un link (Deep Link)
        Application.deepLinkActivated += OnDeepLinkActivated;

        // Verificar si la app ya se abrió mediante un Deep Link (común en Android)
        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            OnDeepLinkActivated(Application.absoluteURL);
        }

        if(loginNormalBtn != null) 
            loginNormalBtn.onClick.AddListener(() => _ = LoginNormal());
        
        if(loginGoogleBtn != null) 
            loginGoogleBtn.onClick.AddListener(LoginWithGoogle);
    }

    // --- DETECTOR DE REGRESO ---
    private async void OnDeepLinkActivated(string url)
    {
        Debug.Log("Deep Link recibido: " + url);
        try
        {
            // Le pasamos la URL de regreso a Supabase para que extraiga la sesión
            var session = await _supabase.Auth.GetSessionFromUrl(new Uri(url));
            
            if (session != null && session.User != null)
            {
                Debug.Log("Sesión recuperada de Google: " + session.User.Email);
                EntrarAlJuego();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("El link no contenía una sesión válida o expiró: " + ex.Message);
        }
    }

    public async Task LoginNormal()
    {
        try
        {
            var session = await _supabase.Auth.SignIn(emailField.text, passwordField.text);
            if (session != null && session.User != null)
            {
                Debug.Log("Login exitoso: " + session.User.Email);
                EntrarAlJuego();
            }
        }
        catch (Exception ex) 
        { 
            Debug.LogError("Error en Login Normal: " + ex.Message); 
        }
    }

    public async void LoginWithGoogle()
    {
        try
        {
            var authState = await _supabase.Auth.SignIn(Supabase.Gotrue.Constants.Provider.Google, new SignInOptions {
                RedirectTo = "ticoapp://login-callback" 
            });
            
            Application.OpenURL(authState.Uri.ToString());
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al abrir Google: " + ex.Message);
        }
    }

    private void EntrarAlJuego()
    {
        Debug.Log("¡Entrando al juego!");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainGameScene");
    }
}