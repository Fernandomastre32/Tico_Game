using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Supabase;

public class MenuPausaManager : MonoBehaviour
{
    [Header("UI Paneles")]
    public GameObject botonPausaUI;       // El botoncito de la esquina para abrir el menú
    public GameObject panelMenuOpciones;  // El panel oscuro con los botones

    [Header("Configuración Base de Datos")]
    public int tipoJuegoID = 1; // Cambia esto según el minijuego (1=Burbujas, 2=Laberinto, etc.)
    
    private string supabaseUrl = "https://gflucxpldvijkagerlzb.supabase.co";
    private string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImdmbHVjeHBsZHZpamthZ2VybHpiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzM0NDk2OTQsImV4cCI6MjA4OTAyNTY5NH0.vYYELn2ofGJRHPsFE4ZmCsq9a6-DMVLNQ6vn7zMc4vo";
    private Supabase.Client _supabase;

    private bool estaPausado = false;

    async void Awake()
    {
        // Asegurarnos de que el menú empiece apagado y el juego corriendo
        panelMenuOpciones.SetActive(false);
        botonPausaUI.SetActive(true);
        Time.timeScale = 1f; 

        // Conectar Supabase silenciosamente
        try {
            var options = new SupabaseOptions { AutoRefreshToken = true };
            _supabase = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);
            await _supabase.InitializeAsync();
        } catch (System.Exception ex) { Debug.LogWarning("Supabase Pausa: " + ex.Message); }
    }

    // --- MÉTODOS PARA LOS BOTONES ---

    public void AbrirPausa()
    {
        estaPausado = true;
        panelMenuOpciones.SetActive(true);
        botonPausaUI.SetActive(false);
        
        // ¡Magia! Congelamos el tiempo del juego y los cronómetros
        Time.timeScale = 0f; 
    }

    public void ReanudarJuego()
    {
        estaPausado = false;
        panelMenuOpciones.SetActive(false);
        botonPausaUI.SetActive(true);
        
        // Descongelamos el tiempo
        Time.timeScale = 1f; 
    }

    public void BotonSalirNivel()
    {
        // Opcional: Aquí podrías activar un sub-panel que pregunte "¿Estás seguro?"
        // Por ahora, asumo que sale directo.
        
        // El tiempo debe volver a la normalidad ANTES de cargar otra escena
        Time.timeScale = 1f; 
        
        // Enviamos el registro de abandono a Supabase
        RegistrarAbandono();

        // Cargamos el menú principal
        SceneManager.LoadScene("Flujo_Menu"); 
    }

    // --- LÓGICA DE BASE DE DATOS ---

    private void RegistrarAbandono()
    {
        // Leemos el ID del paciente como ya aprendimos (en formato string/UUID)
        string pId = PlayerPrefs.GetString("PacienteID", "");
        int cId = PlayerPrefs.GetInt("CitaID", 1);

        if (string.IsNullOrEmpty(pId)) return; // Si no hay usuario, no enviamos nada

        // Ejecutamos el envío sin detener el juego
        _ = EnviarAbandonoAsync(pId, cId);
    }

    private async Task EnviarAbandonoAsync(string pId, int cId)
    {
        if (_supabase == null) return;

        try {
            // Mandamos una métrica especial que indique que se abandonó
            var metricaAbandono = new MetricaIA {
                PacienteId = pId,
                CitaId = cId,
                TipoJuegoId = tipoJuegoID,
                Frustracion = 0, // 0 o null, porque no terminó de medirse
                TiempoReaccionMs = 0,
                // AQUÍ AGREGAS LA NUEVA COLUMNA A TU MODELO C#
                // EstadoPartida = "Abandonado" 
            };
            
            await _supabase.From<MetricaIA>().Insert(metricaAbandono);
            Debug.Log("Abandono registrado en BD.");
        } catch (System.Exception ex) {
            Debug.LogError("Error al registrar abandono: " + ex.Message);
        }
    }
}