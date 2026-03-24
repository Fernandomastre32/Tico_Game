using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 
using TMPro; 
using System.Collections; 
using System.Threading.Tasks;
using Supabase;
using Postgrest.Attributes;
using Postgrest.Models;

public class GameManager : MonoBehaviour
{
    [Header("Interfaz de Usuario")]
    public GameObject overlayInstrucciones; 
    public GameObject contenedorJuego; 
    public Image imagenObjetivoArriba; 
    
    [Header("Panel de Resultados")]
    public GameObject panelResultados; 
    public Text textoCorrectas; 
    public Text textoIncorrectas; 
    public Text textoTiempo; 

    [Header("Reacciones de Tico")]
    public Image imagenTico; 
    public TMP_Text textoDialogoTico; 
    public Sprite spriteNeutral; 
    public string fraseNeutral = "Puedes reventar las burbujas tocándolas"; 
    public float tiempoReaccion = 2.5f; 
    public Sprite[] spritesFelices; 
    public Sprite[] spritesAnimo; 
    public string[] frasesCorrectas = { "¡Muy bien!", "¡Ese es el color!", "¡Genial, sigue así!" };
    public string[] frasesAnimo = { "¡Casi!, busca bien", "¡Tú puedes, intenta con otro!", "¡Sigue buscando!" };

    [Header("Lógica del Nivel")]
    public Sprite[] secuenciaObjetivos; 
    [Tooltip("Activa esto para que el orden cambie en cada partida")]
    public bool patronAleatorio = true; // <-- NUEVO: Te da el control desde Unity
    private int indiceActual = 0; 
    private int conteoCorrectas = 0; 
    private int conteoIncorrectas = 0; 
    private float tiempoJugado = 0f; 
    private bool juegoActivo = false; 

    [Header("Métricas IA y Supabase")]
    private string supabaseUrl = "https://gflucxpldvijkagerlzb.supabase.co";
    private string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImdmbHVjeHBsZHZpamthZ2VybHpiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzM0NDk2OTQsImV4cCI6MjA4OTAyNTY5NH0.vYYELn2ofGJRHPsFE4ZmCsq9a6-DMVLNQ6vn7zMc4vo";
    private Supabase.Client _supabase;
    private float momentoAparicionObjetivo = 0f; 
    private float sumaTiempoReaccion = 0f; 
    private int toquesValidos = 0; 

    private Coroutine rutinaReaccion;

    async void Awake()
    {
        var options = new SupabaseOptions { AutoRefreshToken = true };
        _supabase = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);
        await _supabase.InitializeAsync();

        overlayInstrucciones.SetActive(true);
        panelResultados.SetActive(false); 
        if(contenedorJuego != null) contenedorJuego.SetActive(false); 
    }

    void Update()
    {
        if (juegoActivo) tiempoJugado += Time.deltaTime; 
    }

    public void IniciarJuego()
    {
        overlayInstrucciones.SetActive(false); 
        if(contenedorJuego != null) contenedorJuego.SetActive(true); 
        juegoActivo = true; 

        // <-- NUEVO: Mezclamos los objetivos antes de pedir el primero
        if (patronAleatorio && secuenciaObjetivos.Length > 0)
        {
            MezclarSecuenciaObjetivos();
        }

        ActualizarImagenObjetivo(); 
        VolverANeutral();
    }

    // <-- NUEVO MÉTODO: Este es el motor que barajea los colores
    private void MezclarSecuenciaObjetivos()
    {
        for (int i = 0; i < secuenciaObjetivos.Length; i++)
        {
            // Guardamos el color actual
            Sprite temp = secuenciaObjetivos[i];
            
            // Elegimos una posición al azar
            int indiceRandom = Random.Range(i, secuenciaObjetivos.Length);
            
            // Intercambiamos los colores
            secuenciaObjetivos[i] = secuenciaObjetivos[indiceRandom];
            secuenciaObjetivos[indiceRandom] = temp;
        }
    }

    // ESTA ES LA FUNCIÓN QUE BUSCA Boton_burbuja.cs
    public void EvaluarBurbujaTocada(Sprite spriteBurbuja, GameObject burbujaObject)
    {
        if (!juegoActivo || indiceActual >= secuenciaObjetivos.Length) return; 

        if (spriteBurbuja == secuenciaObjetivos[indiceActual])
        {
            float tiempoReaccionToque = Time.time - momentoAparicionObjetivo;
            sumaTiempoReaccion += tiempoReaccionToque;
            toquesValidos++;

            burbujaObject.SetActive(false); 
            conteoCorrectas++; 
            DispararReaccion(true); 

            if (!QuedanBurbujasDeColor(spriteBurbuja, burbujaObject))
            {
                indiceActual++; 
                if (indiceActual < secuenciaObjetivos.Length) ActualizarImagenObjetivo(); 
                else TerminarJuego(); 
            }
        }
        else
        {
            conteoIncorrectas++; 
            DispararReaccion(false);        
        }
    }

    private void DispararReaccion(bool esCorrecto)
    {
        if (rutinaReaccion != null) StopCoroutine(rutinaReaccion);
        rutinaReaccion = StartCoroutine(RutinaReaccionTico(esCorrecto));
    }

    private IEnumerator RutinaReaccionTico(bool esCorrecto)
    {
        if (esCorrecto)
        {
            if (spritesFelices.Length > 0 && imagenTico != null)
                imagenTico.sprite = spritesFelices[Random.Range(0, spritesFelices.Length)];
            if (textoDialogoTico != null && frasesCorrectas.Length > 0)
                textoDialogoTico.text = frasesCorrectas[Random.Range(0, frasesCorrectas.Length)];
        }
        else
        {
            if (spritesAnimo.Length > 0 && imagenTico != null)
                imagenTico.sprite = spritesAnimo[Random.Range(0, spritesAnimo.Length)];
            if (textoDialogoTico != null && frasesAnimo.Length > 0)
                textoDialogoTico.text = frasesAnimo[Random.Range(0, frasesAnimo.Length)];
        }

        yield return new WaitForSeconds(tiempoReaccion);
        VolverANeutral();
    }

    private void VolverANeutral()
    {
        if (imagenTico != null && spriteNeutral != null) imagenTico.sprite = spriteNeutral;
        if (textoDialogoTico != null) textoDialogoTico.text = fraseNeutral;
    }

    private bool QuedanBurbujasDeColor(Sprite colorBuscado, GameObject burbujaIgnorada)
    {
       // Nota: FindObjectsByType es para versiones nuevas de Unity.
       BotonBurbuja[] todasLasBurbujas = FindObjectsByType<BotonBurbuja>(FindObjectsSortMode.None);
       foreach (BotonBurbuja burbuja in todasLasBurbujas)
       {
           if (burbuja.gameObject.activeInHierarchy && 
               burbuja.gameObject != burbujaIgnorada && 
               burbuja.GetComponent<Image>().sprite == colorBuscado)
           {
               return true; 
           }
       }
       return false; 
    }

    private void ActualizarImagenObjetivo()
    {
        if(secuenciaObjetivos.Length > 0 && indiceActual < secuenciaObjetivos.Length)
        {
            imagenObjetivoArriba.sprite = secuenciaObjetivos[indiceActual];
            momentoAparicionObjetivo = Time.time;
        }
    }

    private void TerminarJuego()
    {
        juegoActivo = false; 
        if (rutinaReaccion != null) StopCoroutine(rutinaReaccion);

        int minutos = Mathf.FloorToInt(tiempoJugado / 60F);
        int segundos = Mathf.FloorToInt(tiempoJugado % 60F);

        textoCorrectas.text = conteoCorrectas.ToString();
        textoIncorrectas.text = conteoIncorrectas.ToString();
        textoTiempo.text = string.Format("{0:00}:{1:00}", minutos, segundos);

        if(contenedorJuego != null) contenedorJuego.SetActive(false);
        panelResultados.SetActive(true);

        int nivelFrustracion = Mathf.Clamp(1 + conteoIncorrectas, 1, 10);
        int promedioReaccionMs = toquesValidos > 0 ? Mathf.RoundToInt((sumaTiempoReaccion / toquesValidos) * 1000f) : 0;

        int pId = PlayerPrefs.GetInt("PacienteID", 1);
        int cId = PlayerPrefs.GetInt("CitaID", 1);

        _ = EnviarMetricasSupabase(pId, cId, nivelFrustracion, 30, 1.0f, promedioReaccionMs);
    }

    private async Task EnviarMetricasSupabase(int pId, int cId, int frustracion, int latencia, float presion, int tiempoReaccion)
    {
        try 
        {
            var nuevaMetrica = new MetricaIA
            {
                PacienteId = pId,
                CitaId = cId,
                Frustracion = frustracion,
                LatenciaMs = latencia,
                PresionToque = presion,
                TiempoReaccionMs = tiempoReaccion
            };

            await _supabase.From<MetricaIA>().Insert(nuevaMetrica);
            Debug.Log("¡Métricas enviadas con éxito a Supabase!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error al enviar a Supabase: " + ex.Message);
        }
    }

    public void BotonMenu() { SceneManager.LoadScene("Flujo_Menu"); }
    public void BotonSiguiente() { SceneManager.LoadScene("nivel2"); }
}

// CLASE DE MODELO (Afuera)
[Table("metricas_ia")]
public class MetricaIA : BaseModel
{
    [Column("paciente_id")]
    public int PacienteId { get; set; }

    [Column("cita_id")]
    public int CitaId { get; set; }

    [Column("frustracion")]
    public int Frustracion { get; set; }

    [Column("latencia_ms")]
    public int LatenciaMs { get; set; }

    [Column("presion_toque")]
    public float PresionToque { get; set; }

    [Column("tiempo_reaccion_ms")]
    public int TiempoReaccionMs { get; set; }
}