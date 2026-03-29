using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Threading.Tasks;
using Supabase;

public class GameManagerLaberinto : MonoBehaviour 
{
    [Header("Configuración del Tipo de Juego")]
    public int tipoJuegoID = 2; // ID 2 para Laberinto en Supabase

    [Header("Contenedores de Jerarquía")]
    public GameObject contenedorJuego; // El objeto "Ejercicio_Nivel"
    public GameObject overlayInstrucciones; // El panel azul inicial
    public GameObject panelResultados; // El panel de victoria
    public GameObject joystick; // El joystick de la UI

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
        // 1. ESTADO INICIAL: Todo apagado excepto las instrucciones
        overlayInstrucciones.SetActive(true);
        panelResultados.SetActive(false);
        
        if (joystick != null) joystick.SetActive(false);
        
        // BUG FIX: Apagamos el contenedor al inicio para que no interfiera con el Canvas
        if (contenedorJuego != null) contenedorJuego.SetActive(false); 

        _ = ConectarSupabase(); 
    }
    void Start()
    {
        // Llamamos al AudioManager que viene desde el Login
        if (AudioManager.instance != null)
        {
            // Cambiamos a la pista del nivel (Nivel 1 o la que asignaras para burbujas)
            // Esto hará que la música suene mientras están las instrucciones puestas.
            AudioManager.instance.CambiarMusica(AudioManager.instance.musicaNivel2);
        }
    }
    private async Task ConectarSupabase()
    {
        try {
            var options = new SupabaseOptions { AutoRefreshToken = true };
            _supabase = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);
            await _supabase.InitializeAsync();
            Debug.Log("Supabase listo en segundo plano");
        } catch { /* Conexión silenciosa */ }
    }


    void Update()
    {
        if (juegoActivo) tiempoJugado += Time.deltaTime;
    }

    // Se llama desde el botón "Entendido" del panel azul
    public void IniciarJuego() 
    {
        overlayInstrucciones.SetActive(false); // Quitamos letrero azul
        
        // BUG FIX: Encendemos el mundo del juego ahora que el usuario dio click
        if (contenedorJuego != null) contenedorJuego.SetActive(true); 
        
        if (joystick != null) joystick.SetActive(true); // Aparece el control
        
        juegoActivo = true;
        tiempoJugado = 0f;
        conteoGolpes = 0;
    }

    public void RegistrarGolpePared() 
    {
        if (juegoActivo)
        {
            conteoGolpes++;
            Debug.Log("Golpe registrado: " + conteoGolpes);
        }
    }

    public void TerminarJuego() 
    {
        juegoActivo = false; 
        
        if (joystick != null) joystick.SetActive(false);
        if (contenedorJuego != null) contenedorJuego.SetActive(false); // Apagamos el mundo al ganar

        // Formatear tiempo
        int minutos = Mathf.FloorToInt(tiempoJugado / 60F);
        int segundos = Mathf.FloorToInt(tiempoJugado % 60F);
        
        if (textoGolpes != null) textoGolpes.text = conteoGolpes.ToString();
        if (textoTiempo != null) textoTiempo.text = string.Format("{0:00}:{1:00}", minutos, segundos);

        panelResultados.SetActive(true);

        int nivelFrustracion = Mathf.Clamp(1 + (conteoGolpes / 2), 1, 10);
        int pId = PlayerPrefs.GetInt("PacienteID", 1);
        int cId = PlayerPrefs.GetInt("CitaID", 1);

        _ = EnviarMetricasSupabase(pId, cId, nivelFrustracion, Mathf.RoundToInt(tiempoJugado * 1000));
    }

    private async Task EnviarMetricasSupabase(int pId, int cId, int frustracion, int tiempoMs) 
    {
        if (_supabase == null) return;
        try {
            var metrica = new MetricaIA {
                PacienteId = pId,
                CitaId = cId,
                Frustracion = frustracion,
                LatenciaMs = 0, 
                PresionToque = 1.0f,
                TiempoReaccionMs = tiempoMs,
                TipoJuegoId = tipoJuegoID
            };
            await _supabase.From<MetricaIA>().Insert(metrica);
            Debug.Log("Métricas enviadas correctamente");
        } catch (System.Exception ex) {
            Debug.LogError("Error Supabase: " + ex.Message);
        }
    }

    public void BotonMenu() { SceneManager.LoadScene("Flujo_Menu"); }
    public void BotonSiguiente() { SceneManager.LoadScene("nivel3"); }
}