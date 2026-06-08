using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Threading.Tasks;
using Supabase;
// using Postgrest.Attributes; // Activa esto si tu MetricaIA lo necesita en este script
// using Postgrest.Models;

public class GameManager_Nivel4 : MonoBehaviour
{
    [Header("Configuración del Nivel 4")]
    public int tipoJuegoID = 4;
    public float duracionNivelSegundos = 60f; // Cuánto dura la prueba CPT

    [Header("Generador (Spawner)")]
    public SpawnerBosque spawner; // Referencia al script que escupe los objetos

    [Header("Interfaz de Usuario")]
    public GameObject overlayInstrucciones;
    public GameObject contenedorJuego;
    public TMP_Text textoCronometroUI; // Opcional, para mostrar el tiempo bajando

    [Header("Panel de Resultados")]
    public GameObject panelResultados;
    public Text textoAciertos;     // Manzanas tocadas
    public Text textoOmisiones;    // Manzanas que se cayeron sin tocar
    public Text textoComisiones;   // Hojas tocadas por error

    [Header("Reacciones de Tico")]
    public Image imagenTico;
    public TMP_Text textoDialogoTico;
    public Sprite spriteNeutral;
    public string fraseNeutral = "Solo atrapa las manzanas";
    public float tiempoReaccion = 2.0f;
    public Sprite[] spritesFelices;
    public Sprite[] spritesAnimo;
    public string[] frasesCorrectas = { "¡Atrapada!", "¡Muy rápido!", "¡Bien!" };
    public string[] frasesAnimo = { "¡Ups, era una hoja!", "¡Espera a la manzana!" };

    // --- Variables Internas ---
    [HideInInspector] public int aciertos = 0;
    [HideInInspector] public int omisiones = 0;
    [HideInInspector] public int comisiones = 0;
    private float tiempoRestante;
    private bool juegoActivo = false;

    // --- Base de Datos ---
    private string supabaseUrl = "https://gflucxpldvijkagerlzb.supabase.co";
    private string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImdmbHVjeHBsZHZpamthZ2VybHpiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzM0NDk2OTQsImV4cCI6MjA4OTAyNTY5NH0.vYYELn2ofGJRHPsFE4ZmCsq9a6-DMVLNQ6vn7zMc4vo";
    private Supabase.Client _supabase;
    
    // Variables para promediar la latencia (Tiempo de Reacción)
    [HideInInspector] public float sumaLatencia = 0f;
    private Coroutine rutinaReaccion;

    async void Awake()
    {
        PlayerPrefs.SetInt("JuegoActualID", tipoJuegoID);
        PlayerPrefs.Save();
        
        overlayInstrucciones.SetActive(true);
        panelResultados.SetActive(false);
        if (contenedorJuego != null) contenedorJuego.SetActive(false);
        if (spawner != null) spawner.gameObject.SetActive(false); // Apagamos el spawner al inicio

        try {
            var options = new SupabaseOptions { AutoRefreshToken = true };
            _supabase = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);
            await _supabase.InitializeAsync();
        } catch (System.Exception ex) { Debug.LogError("Error Supabase: " + ex.Message); }
    }

    void Start()
    {
        tiempoRestante = duracionNivelSegundos;
        if (AudioManager.instance != null) AudioManager.instance.CambiarMusica(AudioManager.instance.musicaNivel1);
    }

    void Update()
    {
        if (juegoActivo)
        {
            tiempoRestante -= Time.deltaTime;
            
            if (textoCronometroUI != null)
                textoCronometroUI.text = Mathf.CeilToInt(tiempoRestante).ToString();

            if (tiempoRestante <= 0)
            {
                TerminarJuego();
            }
        }
    }

    public void IniciarJuego()
    {
        overlayInstrucciones.SetActive(false);
        if (contenedorJuego != null) contenedorJuego.SetActive(true);
        if (spawner != null) spawner.gameObject.SetActive(true); // Encendemos la caída de hojas
        
        juegoActivo = true;
        VolverANeutral();
    }

    // --- REGISTRO DE EVENTOS (Llamados desde los objetos que caen) ---

    public void RegistrarAcierto(float latenciaDelObjeto)
    {
        if (!juegoActivo) return;
        aciertos++;
        sumaLatencia += latenciaDelObjeto;
        DispararReaccion(true);
    }

    public void RegistrarOmision()
    {
        if (!juegoActivo) return;
        omisiones++;
        // Las omisiones suelen ser silenciosas para no frustrar, pero puedes cambiarlo
    }

    public void RegistrarComision() // Tocó un distractor
    {
        if (!juegoActivo) return;
        comisiones++;
        DispararReaccion(false);
    }

    // --- FIN DEL JUEGO ---

    private void TerminarJuego()
    {
        juegoActivo = false;
        if (spawner != null) spawner.gameObject.SetActive(false);
        if (contenedorJuego != null) contenedorJuego.SetActive(false);
        
        textoAciertos.text = aciertos.ToString();
        textoOmisiones.text = omisiones.ToString();
        textoComisiones.text = comisiones.ToString();
        
        panelResultados.SetActive(true);

        // Cálculos clínicos
        int nivelFrustracion = Mathf.Clamp(1 + comisiones, 1, 10);
        int promedioReaccionMs = aciertos > 0 ? Mathf.RoundToInt((sumaLatencia / aciertos) * 1000f) : 0;

        string pId = PlayerPrefs.GetString("PacienteID", "");
        int cId = PlayerPrefs.GetInt("CitaID", 1);

        _ = EnviarMetricasSupabase(pId, cId, nivelFrustracion, promedioReaccionMs);
    }

    private async Task EnviarMetricasSupabase(string pId, int cId, int frustracion, int reaccionMs)
    {
        if (_supabase == null || string.IsNullOrEmpty(pId)) return;

        try {
            var metrica = new MetricaIA {
                PacienteId = pId,
                CitaId = cId,
                TipoJuegoId = tipoJuegoID,
                Frustracion = frustracion,
                TiempoReaccionMs = reaccionMs,
                LatenciaMs = 30,
                PresionToque = 1.0f,
                EstadoPartida = "completado"
            };
            await _supabase.From<MetricaIA>().Insert(metrica);
            Debug.Log("Métricas Nivel 4 (Vigía) enviadas.");
        } catch (System.Exception ex) { Debug.LogError("Error: " + ex.Message); }
    }

    // --- SISTEMA DE TICO ---
    private void DispararReaccion(bool esCorrecto) { if (rutinaReaccion != null) StopCoroutine(rutinaReaccion); rutinaReaccion = StartCoroutine(RutinaReaccionTico(esCorrecto)); }
    private IEnumerator RutinaReaccionTico(bool esCorrecto) {
        if (esCorrecto) {
            if (spritesFelices.Length > 0) imagenTico.sprite = spritesFelices[Random.Range(0, spritesFelices.Length)];
            textoDialogoTico.text = frasesCorrectas[Random.Range(0, frasesCorrectas.Length)];
        } else {
            if (spritesAnimo.Length > 0) imagenTico.sprite = spritesAnimo[Random.Range(0, spritesAnimo.Length)];
            textoDialogoTico.text = frasesAnimo[Random.Range(0, frasesAnimo.Length)];
        }
        yield return new WaitForSeconds(tiempoReaccion);
        VolverANeutral();
    }
    private void VolverANeutral() { imagenTico.sprite = spriteNeutral; textoDialogoTico.text = fraseNeutral; }
    
    // --- NAVEGACIÓN ---
    public void BotonMenu() { SceneManager.LoadScene("Flujo_Menu"); }
    public void BotonSiguiente() { SceneManager.LoadScene("flujo_Niveles"); }
}