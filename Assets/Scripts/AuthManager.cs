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

    [Header("UI Login (Solo Tutores)")]
    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button loginNormalBtn;
    [SerializeField] private Button loginGoogleBtn;
    [SerializeField] private TextMeshProUGUI errorText;

    private Supabase.Client _supabase;
    private Session _currentSession; 

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
            var session = await _supabase.Auth.SignIn(emailField.text, passwordField.text);
            if (session != null) EvaluarFlujoTutor(session);
        } 
        catch (Exception ex) 
        { 
            Debug.Log("Usuario no existe o error de red, intentando registro...");
            try {
                var session = await _supabase.Auth.SignUp(emailField.text, passwordField.text);
                if (session != null) EvaluarFlujoTutor(session);
            } catch (Exception e) {
                UpdateError("Error: " + e.Message);
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
            if (session != null) EvaluarFlujoTutor(session);
        } catch (Exception ex) { Debug.LogWarning(ex.Message); }
    }

    private async void EvaluarFlujoTutor(Session session)
    {
        _currentSession = session;

        PlayerPrefs.SetString("TokenSesion", session.AccessToken);
        PlayerPrefs.SetString("UserEmail", session.User.Email);
        PlayerPrefs.Save();

        UpdateError("Sincronizando perfil del tutor...");

        try
        {
            var queryTutor = await _supabase.From<Tutor>().Where(x => x.Email == session.User.Email).Get();
            Tutor tutorActual;

            if (queryTutor.Models.Count == 0)
            {
                // Extraemos nombre y apellidos de Google si existen
                string nombreCompletoExtraido = session.User.Email.Split('@')[0]; // Por defecto
                
                if (_currentSession.User.UserMetadata != null)
                {
                    if (_currentSession.User.UserMetadata.ContainsKey("full_name"))
                    {
                        nombreCompletoExtraido = _currentSession.User.UserMetadata["full_name"].ToString();
                    }
                    else if (_currentSession.User.UserMetadata.ContainsKey("name"))
                    {
                        nombreCompletoExtraido = _currentSession.User.UserMetadata["name"].ToString();
                    }
                }

                var nuevoTutor = new Tutor { Email = session.User.Email, Nombre = nombreCompletoExtraido };
                
                // Insertamos y capturamos el ID numérico generado
                var respuestaInsert = await _supabase.From<Tutor>().Insert(nuevoTutor);
                tutorActual = respuestaInsert.Models[0]; 
                Debug.Log("Nuevo tutor registrado en BD con ID: " + tutorActual.Id);
            }
            else
            {
                tutorActual = queryTutor.Models[0];
            }

            // Guardamos el ID numérico del tutor para usarlo en la siguiente pantalla
            PlayerPrefs.SetInt("TutorId", tutorActual.Id);
            PlayerPrefs.Save();

            // Verificamos si este tutor ya tiene un paciente (niño) asignado
            UpdateError("Verificando datos del niño...");
            var queryNino = await _supabase.From<Paciente>().Where(x => x.TutorId == tutorActual.Id).Get();

            if (queryNino.Models.Count == 0)
            {
                Debug.Log("No hay niños registrados. Yendo a escena de registro.");
                StartCoroutine(CargarEscenaSegura("Registro_nino")); 
            }
            else
            {
                Debug.Log("Niño detectado. Entrando directo al juego.");
                StartCoroutine(CargarEscenaSegura("Flujo_Menu"));
            }
        }
        catch (Exception ex)
        {
            UpdateError("Error al sincronizar datos.");
            Debug.LogError("Error en el flujo del tutor: " + ex.Message);
        }
    }

    private System.Collections.IEnumerator CargarEscenaSegura(string nombreEscena)
    {
        yield return null; 
        SceneManager.LoadScene(nombreEscena); 
    }

    private void UpdateError(string msg) { if (errorText != null) errorText.text = msg; }
}