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
    public GameObject contenedorJuego; // Arrastra aquí "Ejercicio_Nivel"
    public GameObject overlayInstrucciones; // Arrastra aquí tu panel azul
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

 void Awake() // Quitamos el async de aquí
    {
        overlayInstrucciones.SetActive(true);
        panelResultados.SetActive(false);
        if (joystick != null) joystick.SetActive(false);
        if (contenedorJuego != null) contenedorJuego.SetActive(true);

        // Llamamos a la conexión sin esperar (Fire and forget)
        _ = ConectarSupabase(); 
    }

    private async Task ConectarSupabase()
    {
        try {
            var options = new SupabaseOptions { AutoRefreshToken = true };
            _supabase = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);
            await _supabase.InitializeAsync();
            Debug.Log("Supabase listo en segundo plano");
        } catch { /* Si falla, no detiene el juego */ }
    }

    // Se llama desde el botón "Entendido" de las instrucciones
   
    void Update()
    {
        if (juegoActivo) tiempoJugado += Time.deltaTime;
    }

    // Se llama desde el botón "Entendido" de las instrucciones
    public void IniciarJuego() 
    {
        overlayInstrucciones.SetActive(false); // Quitar letrero azul
        
        // Ya no es necesario prender el contenedorJuego porque ya estaba prendido
        
        if (joystick != null) joystick.SetActive(true); // Activar controles para poder jugar
        
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

        // Formatear tiempo para la UI
        int minutos = Mathf.FloorToInt(tiempoJugado / 60F);
        int segundos = Mathf.FloorToInt(tiempoJugado % 60F);
        
        if (textoGolpes != null) textoGolpes.text = conteoGolpes.ToString();
        if (textoTiempo != null) textoTiempo.text = string.Format("{0:00}:{1:00}", minutos, segundos);

        panelResultados.SetActive(true);

        // Cálculos para métricas
        int nivelFrustracion = Mathf.Clamp(1 + (conteoGolpes / 2), 1, 10);
        int pId = PlayerPrefs.GetInt("PacienteID", 1);
        int cId = PlayerPrefs.GetInt("CitaID", 1);

        // Enviar a Supabase
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
                TiempoReaccionMs = tiempoMs, // Usamos este campo para el tiempo total en laberinto
                TipoJuegoId = tipoJuegoID
            };
            await _supabase.From<MetricaIA>().Insert(metrica);
            Debug.Log("¡Métricas del Laberinto enviadas correctamente!");
        } catch (System.Exception ex) {
            Debug.LogError("Error al enviar a Supabase: " + ex.Message);
        }
    }

    public void BotonMenu() { SceneManager.LoadScene("Flujo_Menu"); }
    public void BotonSiguiente() { SceneManager.LoadScene("nivel3"); }
}