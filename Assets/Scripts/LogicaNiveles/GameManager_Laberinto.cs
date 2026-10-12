
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Collections;
using System.Threading.Tasks;
using Supabase;

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
    private string supabaseUrl =
        "https://gflucxpldvijkagerlzb.supabase.co";

    // Coloca aquí tu clave pública anon de Supabase.
    private string supabaseAnonKey = "TU_SUPABASE_ANON_KEY";

    private Supabase.Client _supabase;

    void Awake()
    {
        PlayerPrefs.SetInt("JuegoActualID", tipoJuegoID);
        PlayerPrefs.Save();

        if (overlayInstrucciones != null)
            overlayInstrucciones.SetActive(true);

        if (panelResultados != null)
            panelResultados.SetActive(false);

        if (joystick != null)
            joystick.SetActive(false);

        if (contenedorJuego != null)
            contenedorJuego.SetActive(false);

        _ = ConectarSupabase();
    }

    void Start()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.CambiarMusica(
                AudioManager.instance.musicaNivel2
            );
        }
    }

    private async Task ConectarSupabase()
    {
        try
        {
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true
            };

            _supabase = new Supabase.Client(
                supabaseUrl,
                supabaseAnonKey,
                options
            );

            await _supabase.InitializeAsync();

            Debug.Log("Supabase conectado para enviar métricas.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning(
                "Error de conexión con Supabase: " + ex.Message
            );
        }
    }

    void Update()
    {
        if (juegoActivo)
        {
            tiempoJugado += Time.deltaTime;
        }
    }

    public void IniciarJuego()
    {
        if (overlayInstrucciones != null)
            overlayInstrucciones.SetActive(false);

        if (contenedorJuego != null)
            contenedorJuego.SetActive(true);

        if (joystick != null)
            joystick.SetActive(true);

        if (panelResultados != null)
            panelResultados.SetActive(false);

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
        if (!juegoActivo)
            return;

        juegoActivo = false;

        // Registrar el nivel 2 como completado
        ProgresoNiveles.Completar(2);

        if (joystick != null)
            joystick.SetActive(false);

        if (contenedorJuego != null)
            contenedorJuego.SetActive(false);

        int minutos = Mathf.FloorToInt(tiempoJugado / 60f);
        int segundos = Mathf.FloorToInt(tiempoJugado % 60f);

        if (textoGolpes != null)
        {
            textoGolpes.text = conteoGolpes.ToString();
        }

        if (textoTiempo != null)
        {
            textoTiempo.text = string.Format(
                "{0:00}:{1:00}",
                minutos,
                segundos
            );
        }

        if (panelResultados != null)
            panelResultados.SetActive(true);

        string pId = PlayerPrefs.GetString("PacienteID", "");
        int cId = PlayerPrefs.GetInt("CitaID", 1);

        int nivelFrustracion = Mathf.Clamp(
            1 + (conteoGolpes / 2),
            1,
            10
        );

        _ = EnviarMetricasSupabase(
            pId,
            cId,
            nivelFrustracion,
            Mathf.RoundToInt(tiempoJugado * 1000)
        );
    }

    private async Task EnviarMetricasSupabase(
        string pId,
        int cId,
        int frustracion,
        int tiempoMs
    )
    {
        if (_supabase == null)
        {
            Debug.LogWarning("Supabase no está conectado.");
            return;
        }

        if (string.IsNullOrEmpty(pId))
        {
            Debug.LogError(
                "No se pudo enviar la métrica: " +
                "El UUID del paciente no está guardado."
            );
            return;
        }

        try
        {
            var metrica = new MetricaIA
            {
                PacienteId = pId,
                CitaId = cId,
                Frustracion = frustracion,
                LatenciaMs = 0,
                PresionToque = 1.0f,
                TiempoReaccionMs = tiempoMs,
                TipoJuegoId = tipoJuegoID
            };

            await _supabase
                .From<MetricaIA>()
                .Insert(metrica);

            Debug.Log("Métricas enviadas correctamente.");
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "Error Supabase al enviar métricas: " +
                ex.Message
            );
        }
    }

    public void BotonMenu()
    {
        SceneManager.LoadScene("Flujo_Menu");
    }

    public void BotonSiguiente()
    {
        SceneManager.LoadScene("flujo_Niveles");
    }
}
