using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Threading.Tasks;
using Supabase;
using System; 

public class GameManagerLaberinto : MonoBehaviour 
{
    [Header("Configuración del Tipo de Juego")]
    public int tipoJuegoID = 2; 

    [Header("Contenedores de Jerarquía")]
    public GameObject contenedorJuego; 
    public GameObject overlayInstrucciones; 
    public GameObject panelResultados; 
    public GameObject joystick; 

    [Header("Textos de Resultados")]
    public TMP_Text textoTiempo; 
    public TMP_Text textoGolpes; 

    [Header("Métricas Internas")]
    private int conteoGolpes = 0;
    private float tiempoJugado = 0f;
    private bool juegoActivo = false;

    [Header("Configuración Supabase")]
    private string supabaseUrl = "https://gflucxpldvijkagerlzb.supabase.co";
    private string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImdmbHVjeHBsZHZpamthZ2VybHpiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzM0NDk2OTQsImV4cCI6MjA4OTAyNTY5NH0.vYYELn2ofGJRHPsFE4ZmCsq9a6-DMVLNQ6vn7zMc4vo";
    private Supabase.Client _supabase;

    void Awake() 
    {
        PlayerPrefs.SetInt("JuegoActualID", tipoJuegoID); 
        PlayerPrefs.Save();
        overlayInstrucciones.SetActive(true);
        panelResultados.SetActive(false);
        
        if (joystick != null) joystick.SetActive(false);
        if (contenedorJuego != null) contenedorJuego.SetActive(false); 

        _ = ConectarSupabase(); 
    }

    void Start()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.CambiarMusica(AudioManager.instance.musicaNivel2);
        }
    }

    private async Task ConectarSupabase()
    {
        try {
            var options = new SupabaseOptions { AutoRefreshToken = true };
            _supabase = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);
            await _supabase.InitializeAsync();
            Debug.Log("Supabase conectado para enviar métricas.");
        } catch (Exception ex) { 
            Debug.LogWarning("Conexión silenciosa falló: " + ex.Message); 
        }
    }

    void Update()
    {
        if (juegoActivo) tiempoJugado += Time.deltaTime;
    }

    public void IniciarJuego() 
    {
        overlayInstrucciones.SetActive(false); 
        if (contenedorJuego != null) contenedorJuego.SetActive(true); 
        if (joystick != null) joystick.SetActive(true); 
        
        juegoActivo = true;
        tiempoJugado = 0f;
        conteoGolpes = 0;
    }

    public void RegistrarGolpePared() 
    {
        if (juegoActivo)
        {
            conteoGolpes++;
        }
    }

    public void TerminarJuego() 
    {
        if (!juegoActivo) return; 
        juegoActivo = false; 
        
        if (joystick != null) joystick.SetActive(false);
        if (contenedorJuego != null) contenedorJuego.SetActive(false); 
        
        int minutos = Mathf.FloorToInt(tiempoJugado / 60F);
        int segundos = Mathf.FloorToInt(tiempoJugado % 60F);
        
        if (textoGolpes != null) textoGolpes.text = conteoGolpes.ToString();
        if (textoTiempo != null) textoTiempo.text = string.Format("{0:00}:{1:00}", minutos, segundos);

        panelResultados.SetActive(true); 

        // AHORA LO LEEMOS COMO TEXTO (STRING) PARA QUE ENTIENDA EL UUID
     string pId = PlayerPrefs.GetString("PacienteID", "");
        int cId = PlayerPrefs.GetInt("CitaID", 1); 
        int nivelFrustracion = Mathf.Clamp(1 + (conteoGolpes / 2), 1, 10);
        
        _ = EnviarMetricasSupabase(pId, cId, nivelFrustracion, Mathf.RoundToInt(tiempoJugado * 1000));
    }

    // CAMBIAMOS EL PARÁMETRO pId a string
private async Task EnviarMetricasSupabase(string pId, int cId, int frustracion, int tiempoMs)    {
        if (_supabase == null) return;
        
        if (string.IsNullOrEmpty(pId)) 
        {
            Debug.LogError("No se pudo enviar la métrica: El UUID no está guardado en el dispositivo.");
            return;
        }

        try {
            var metrica = new MetricaIA {
                PacienteId = pId, // Ahora manda el texto UUID correctamente
                CitaId = cId,
                Frustracion = frustracion,
                LatenciaMs = 0, 
                PresionToque = 1.0f,
                TiempoReaccionMs = tiempoMs,
                TipoJuegoId = tipoJuegoID
            };
            await _supabase.From<MetricaIA>().Insert(metrica);
            Debug.Log("¡Métricas enviadas correctamente con el UUID!");
        } catch (Exception ex) {
            Debug.LogError("Error Supabase al enviar métricas: " + ex.Message);
        }
    }

    public void BotonMenu() { SceneManager.LoadScene("Flujo_Menu"); }
    public void BotonSiguiente() { SceneManager.LoadScene("flujo_Niveles"); }
}