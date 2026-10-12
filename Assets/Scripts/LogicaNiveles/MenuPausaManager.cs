using UnityEngine;
using UnityEngine.UI; // <--- OBLIGATORIO para usar Sliders
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System; // <--- Agregado para usar DateTime
using Supabase;

public class MenuPausaManager : MonoBehaviour
{
    [Header("UI Paneles")]
    public GameObject botonPausaUI;       
    public GameObject panelMenuOpciones;  

    [Header("Sliders de Audio")]
    public Slider sliderMusica; // Arrastra tu slider de música aquí
    public Slider sliderJuego;  // Arrastra tu slider de juego aquí

    [Header("Configuración Base de Datos")]
    
    private string supabaseUrl = "https://gflucxpldvijkagerlzb.supabase.co";
    private string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImdmbHVjeHBsZHZpamthZ2VybHpiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzM0NDk2OTQsImV4cCI6MjA4OTAyNTY5NH0.vYYELn2ofGJRHPsFE4ZmCsq9a6-DMVLNQ6vn7zMc4vo";
    private Supabase.Client _supabase;

    private bool estaPausado = false;

    async void Awake()
    {
        panelMenuOpciones.SetActive(false);
        botonPausaUI.SetActive(true);
        Time.timeScale = 1f; 

        try {
            var options = new SupabaseOptions { AutoRefreshToken = true };
            _supabase = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);
            await _supabase.InitializeAsync();
        } catch (Exception ex) { Debug.LogWarning("Supabase Pausa: " + ex.Message); }
    }

    void Start()
    {
        // 1. Acomodamos los sliders donde se quedaron la última vez
        if (sliderMusica != null)
        {
            sliderMusica.value = PlayerPrefs.GetFloat("VolumenMusica", 1f);
            // 2. Le decimos al slider qué función ejecutar cuando lo muevan
            sliderMusica.onValueChanged.AddListener(CambiarVolumenMusica);
        }

        if (sliderJuego != null)
        {
            sliderJuego.value = PlayerPrefs.GetFloat("VolumenJuego", 1f);
            sliderJuego.onValueChanged.AddListener(CambiarVolumenJuego);
        }
    }

    // --- MÉTODOS PARA LOS SLIDERS ---
    
    public void CambiarVolumenMusica(float valor)
    {
        if (AudioManager.instance != null) AudioManager.instance.SetVolumenMusica(valor);
    }

    public void CambiarVolumenJuego(float valor)
    {
        if (AudioManager.instance != null) AudioManager.instance.SetVolumenJuego(valor);
    }

    // --- MÉTODOS PARA LOS BOTONES ---

    public void AbrirPausa()
    {
        if (estaPausado) return; // Si ya está pausado, no hace nada repetido

        estaPausado = true;
        panelMenuOpciones.SetActive(true);
        botonPausaUI.SetActive(false);
        Time.timeScale = 0f; 
    }

    public void ReanudarJuego()
    {
        if (!estaPausado) return; // Si no estaba pausado, no hace nada

        estaPausado = false;
        panelMenuOpciones.SetActive(false);
        botonPausaUI.SetActive(true);
        Time.timeScale = 1f; 
    }

    // --- BOTÓN SALIR ---
    public async void BotonSalirNivel()
    {
        Time.timeScale = 1f; // Descongelamos el juego para que todo fluya

        // Esperamos a que la BD nos confirme que guardó
        await RegistrarAbandono(); 

        // Una vez confirmado, ahora sí cambiamos de escena
        SceneManager.LoadScene("Flujo_Menu"); 
    }

    // --- LÓGICA DE BASE DE DATOS ---

    private async Task RegistrarAbandono()
    {
        string pId = PlayerPrefs.GetString("PacienteID", "");
        int cId = PlayerPrefs.GetInt("CitaID", 1);
        
        if (string.IsNullOrEmpty(pId))
        {
            Debug.LogWarning("No hay PacienteID guardado. Simulando abandono sin mandar a la BD.");
            return;
        } 
        
        await EnviarAbandonoAsync(pId, cId);
    }

    private async Task EnviarAbandonoAsync(string pId, int cId)
    {
        if (_supabase == null) return;

        // Leemos el ID exacto que el GameManager actual acaba de guardar
        int idAutomatico = PlayerPrefs.GetInt("JuegoActualID", 1); 

        try {
            // ¡EL PAQUETE COMPLETO!
            var metricaAbandono = new MetricaIA {
                PacienteId = pId, 
                CitaId = cId, 
                TipoJuegoId = idAutomatico, 
                Frustracion = 0, 
                TiempoReaccionMs = 0,
                
                // Campos adicionales requeridos por tu BD
                LatenciaMs = 0,
                PresionToque = 1.0f,
                EstadoPartida = "Abandonado",
                FechaRegistro = DateTime.Now // Guarda la hora exacta del abandono
            };

            await _supabase.From<MetricaIA>().Insert(metricaAbandono);
            Debug.Log("Abandono registrado exitosamente en BD para el Juego ID: " + idAutomatico);
            
        } catch (Exception ex) {
            Debug.LogError("Error al registrar abandono: " + ex.Message);
        }
    }
}