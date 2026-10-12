
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Collections;
using System.Threading.Tasks;
using Supabase;

public class GameManager_Nivel4 : MonoBehaviour
{
    [Header("Configuración del Nivel 4")]
    public int tipoJuegoID = 4;
    public float duracionNivelSegundos = 60f;

    [Header("Generador (Spawner)")]
    public GameObject spawner;

    [Header("Interfaz de Usuario")]
    public GameObject overlayInstrucciones;
    public GameObject contenedorJuego;
    public TMP_Text textoCronometroUI;

    [Header("Panel de Resultados")]
    public GameObject panelResultados;
    public Text textoAciertos;
    public Text textoOmisiones;
    public Text textoComisiones;

    [Header("Reacciones de Tico")]
    public Image imagenTico;
    public TMP_Text textoDialogoTico;
    public Sprite spriteNeutral;

    public string fraseNeutral = "Solo atrapa las manzanas";
    public float tiempoReaccion = 2.0f;

    public Sprite[] spritesFelices;
    public Sprite[] spritesAnimo;

    public string[] frasesCorrectas =
    {
        "¡Atrapada!",
        "¡Muy rápido!",
        "¡Bien!"
    };

    public string[] frasesAnimo =
    {
        "¡Ups, era una hoja!",
        "¡Espera a la manzana!"
    };

    // Variables internas
    [Header("Métricas del Juego")]
    [HideInInspector] public int aciertos = 0;
    [HideInInspector] public int omisiones = 0;
    [HideInInspector] public int comisiones = 0;
    [HideInInspector] public float sumaLatencia = 0f;

    private float tiempoRestante;
    private bool juegoActivo = false;
    private Coroutine rutinaReaccion;

    // Configuración Supabase
    private string supabaseUrl =
        "https://gflucxpldvijkagerlzb.supabase.co";

    // Reemplazar por la clave pública anon de Supabase
    private string supabaseAnonKey = "TU_SUPABASE_ANON_KEY";

    private Supabase.Client _supabase;

    // =====================================
    // INICIALIZACIÓN
    // =====================================

    void Awake()
    {
        PlayerPrefs.SetInt("JuegoActualID", tipoJuegoID);
        PlayerPrefs.Save();

        tiempoRestante = duracionNivelSegundos;

        if (overlayInstrucciones != null)
            overlayInstrucciones.SetActive(true);

        if (panelResultados != null)
            panelResultados.SetActive(false);

        if (contenedorJuego != null)
            contenedorJuego.SetActive(false);

        // Desactivar generador al iniciar
        if (spawner != null)
            spawner.SetActive(false);

        _ = ConectarSupabase();
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

            Debug.Log("Supabase conectado en Nivel 4.");
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "Error conectando Supabase: " + ex.Message
            );
        }
    }

    void Start()
    {
        ActualizarCronometro();

        if (AudioManager.instance != null)
        {
            AudioManager.instance.CambiarMusica(
                AudioManager.instance.musicaNivel1
            );
        }

        VolverANeutral();
    }

    // =====================================
    // CRONÓMETRO
    // =====================================

    void Update()
    {
        if (!juegoActivo)
            return;

        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            ActualizarCronometro();
            TerminarJuego();
            return;
        }

        ActualizarCronometro();
    }

    private void ActualizarCronometro()
    {
        if (textoCronometroUI != null)
        {
            textoCronometroUI.text =
                Mathf.CeilToInt(tiempoRestante).ToString();
        }
    }

    // =====================================
    // INICIAR JUEGO
    // =====================================

    public void IniciarJuego()
    {
        if (juegoActivo)
            return;

        // Reiniciar métricas
        aciertos = 0;
        omisiones = 0;
        comisiones = 0;
        sumaLatencia = 0f;

        tiempoRestante = duracionNivelSegundos;

        if (overlayInstrucciones != null)
            overlayInstrucciones.SetActive(false);

        if (panelResultados != null)
            panelResultados.SetActive(false);

        if (contenedorJuego != null)
            contenedorJuego.SetActive(true);

        juegoActivo = true;

        // Activar generador de manzanas y hojas
        if (spawner != null)
            spawner.SetActive(true);

        ActualizarCronometro();
        VolverANeutral();

        Debug.Log("Nivel 4 iniciado.");
    }

    // =====================================
    // REGISTRO DE EVENTOS
    // =====================================

    public void RegistrarAcierto(float latenciaDelObjeto)
    {
        if (!juegoActivo)
            return;

        aciertos++;

        sumaLatencia += Mathf.Max(0f, latenciaDelObjeto);

        DispararReaccion(true);

        Debug.Log("Aciertos: " + aciertos);
    }

    public void RegistrarOmision()
    {
        if (!juegoActivo)
            return;

        omisiones++;

        Debug.Log("Omisiones: " + omisiones);
    }

    public void RegistrarComision()
    {
        if (!juegoActivo)
            return;

        comisiones++;

        DispararReaccion(false);

        Debug.Log("Comisiones: " + comisiones);
    }

    // =====================================
    // TERMINAR JUEGO
    // =====================================

    private void TerminarJuego()
    {
        if (!juegoActivo)
            return;

        juegoActivo = false;

        // Guardar progreso del nivel 4
        ProgresoNiveles.Completar(4);

        // Detener generador
        if (spawner != null)
            spawner.SetActive(false);

        // Ocultar contenedor del juego
        if (contenedorJuego != null)
            contenedorJuego.SetActive(false);

        // Detener reacciones pendientes
        if (rutinaReaccion != null)
        {
            StopCoroutine(rutinaReaccion);
            rutinaReaccion = null;
        }

        VolverANeutral();

        // Mostrar resultados
        if (textoAciertos != null)
            textoAciertos.text = aciertos.ToString();

        if (textoOmisiones != null)
            textoOmisiones.text = omisiones.ToString();

        if (textoComisiones != null)
            textoComisiones.text = comisiones.ToString();

        if (panelResultados != null)
            panelResultados.SetActive(true);

        // =====================================
        // CÁLCULO DE MÉTRICAS
        // =====================================

        int nivelFrustracion = Mathf.Clamp(
            1 + comisiones,
            1,
            10
        );

        int promedioReaccionMs = aciertos > 0
            ? Mathf.RoundToInt(
                (sumaLatencia / aciertos) * 1000f
            )
            : 0;

        string pId = PlayerPrefs.GetString("PacienteID", "");
        int cId = PlayerPrefs.GetInt("CitaID", 1);

        _ = EnviarMetricasSupabase(
            pId,
            cId,
            nivelFrustracion,
            promedioReaccionMs
        );

        Debug.Log("Nivel 4 completado.");
    }

    // =====================================
    // ENVIAR MÉTRICAS A SUPABASE
    // =====================================

    private async Task EnviarMetricasSupabase(
        string pId,
        int cId,
        int frustracion,
        int reaccionMs
    )
    {
        if (_supabase == null)
        {
            Debug.LogWarning(
                "Supabase no está disponible."
            );
            return;
        }

        if (string.IsNullOrEmpty(pId))
        {
            Debug.LogError(
                "No se encontró PacienteID."
            );
            return;
        }

        try
        {
            var metrica = new MetricaIA
            {
                PacienteId = pId,
                CitaId = cId,
                TipoJuegoId = tipoJuegoID,
                Frustracion = frustracion,
                TiempoReaccionMs = reaccionMs,
                LatenciaMs = 30,
                PresionToque = 1.0f,
                EstadoPartida = "completado"
            };

            await _supabase
                .From<MetricaIA>()
                .Insert(metrica);

            Debug.Log(
                "Métricas del Nivel 4 enviadas correctamente."
            );
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "Error enviando métricas Nivel 4: " +
                ex.Message
            );
        }
    }

    // =====================================
    // SISTEMA DE REACCIONES DE TICO
    // =====================================

    private void DispararReaccion(bool esCorrecto)
    {
        if (rutinaReaccion != null)
            StopCoroutine(rutinaReaccion);

        rutinaReaccion = StartCoroutine(
            RutinaReaccionTico(esCorrecto)
        );
    }

    private IEnumerator RutinaReaccionTico(bool esCorrecto)
    {
        if (esCorrecto)
        {
            if (imagenTico != null &&
                spritesFelices != null &&
                spritesFelices.Length > 0)
            {
                imagenTico.sprite = spritesFelices[
                    UnityEngine.Random.Range(
                        0,
                        spritesFelices.Length
                    )
                ];
            }

            if (textoDialogoTico != null &&
                frasesCorrectas != null &&
                frasesCorrectas.Length > 0)
            {
                textoDialogoTico.text = frasesCorrectas[
                    UnityEngine.Random.Range(
                        0,
                        frasesCorrectas.Length
                    )
                ];
            }
        }
        else
        {
            if (imagenTico != null &&
                spritesAnimo != null &&
                spritesAnimo.Length > 0)
            {
                imagenTico.sprite = spritesAnimo[
                    UnityEngine.Random.Range(
                        0,
                        spritesAnimo.Length
                    )
                ];
            }

            if (textoDialogoTico != null &&
                frasesAnimo != null &&
                frasesAnimo.Length > 0)
            {
                textoDialogoTico.text = frasesAnimo[
                    UnityEngine.Random.Range(
                        0,
                        frasesAnimo.Length
                    )
                ];
            }
        }

        yield return new WaitForSeconds(tiempoReaccion);

        VolverANeutral();

        rutinaReaccion = null;
    }

    private void VolverANeutral()
    {
        if (imagenTico != null)
            imagenTico.sprite = spriteNeutral;

        if (textoDialogoTico != null)
            textoDialogoTico.text = fraseNeutral;
    }

    // =====================================
    // NAVEGACIÓN
    // =====================================

    public void BotonMenu()
    {
        SceneManager.LoadScene("Flujo_Menu");
    }

    public void BotonSiguiente()
    {
        SceneManager.LoadScene("flujo_Niveles");
    }
}
