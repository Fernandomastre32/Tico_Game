using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 
using TMPro; 
using System.Collections; 
using System.Threading.Tasks;
using Supabase;
using Postgrest.Attributes;
using Postgrest.Models;

public class GameManagerNivel3 : MonoBehaviour
{
    [Header("Configuración del Tipo de Juego")]
    public int tipoJuegoID = 3; // ID 3 para el modo Contrareloj

    [Header("Interfaz de Usuario")]
    public GameObject overlayInstrucciones; 
    public GameObject contenedorJuego; 
    public Image imagenObjetivoArriba; 
    public TMP_Text textoCronometro; // Texto para mostrar 00:30...

    [Header("Panel de Resultados")]
    public GameObject panelResultados; 
    public Text textoCorrectas; 
    public Text textoIncorrectas; 
    public Text textoPuntajeFinal; // "Lograste reventar X burbujas"

    [Header("Configuración de Tiempo")]
    public float tiempoLimite = 30f; // 30 segundos de juego
    private float tiempoRestante;

    [Header("Reacciones de Tico")]
    public Image imagenTico; 
    public TMP_Text textoDialogoTico; 
    public Sprite spriteNeutral; 
    public string fraseNeutral = "¡Rápido!, revienta las que puedas"; 
    public float tiempoReaccion = 2.0f; 
    public Sprite[] spritesFelices; 
    public Sprite[] spritesAnimo; 
    public string[] frasesCorrectas = { "¡Eso!", "¡Vas muy rápido!", "¡Sigue así!" };
    public string[] frasesAnimo = { "¡Casi!, otra vez", "¡No te detengas!", "¡Sigue buscando!" };

    [Header("Lógica del Nivel")]
    public Sprite[] secuenciaObjetivos; 
    private int indiceActual = 0; 
    private int conteoCorrectas = 0; 
    private int conteoIncorrectas = 0; 
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
        overlayInstrucciones.SetActive(true);
        panelResultados.SetActive(false); 
        if(contenedorJuego != null) contenedorJuego.SetActive(false); 
        tiempoRestante = tiempoLimite;

        try {
            var options = new SupabaseOptions { AutoRefreshToken = true };
            _supabase = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);
            await _supabase.InitializeAsync();
        } catch (System.Exception ex) { Debug.LogError("Error Supabase: " + ex.Message); }
    }

    void Update() 
    { 
        if (juegoActivo) 
        {
            tiempoRestante -= Time.deltaTime;
            
            // Actualizar texto del cronómetro
            if (textoCronometro != null)
                textoCronometro.text = Mathf.Max(0, tiempoRestante).ToString("F1") + "s";

            if (tiempoRestante <= 0)
            {
                TerminarJuego();
            }
        } 
    }

    public void IniciarJuego()
    {
        overlayInstrucciones.SetActive(false); 
        if(contenedorJuego != null) contenedorJuego.SetActive(true); 
        juegoActivo = true; 
        MezclarSecuenciaObjetivos();
        ActualizarImagenObjetivo(); 
        VolverANeutral();
    }

    private void MezclarSecuenciaObjetivos()
    {
        for (int i = 0; i < secuenciaObjetivos.Length; i++)
        {
            Sprite temp = secuenciaObjetivos[i];
            int indiceRandom = Random.Range(i, secuenciaObjetivos.Length);
            secuenciaObjetivos[i] = secuenciaObjetivos[indiceRandom];
            secuenciaObjetivos[indiceRandom] = temp;
        }
    }

    public void EvaluarBurbujaTocada(Sprite spriteBurbuja, GameObject burbujaObject)
    {
        if (!juegoActivo) return; 

        if (spriteBurbuja == secuenciaObjetivos[indiceActual])
        {
            sumaTiempoReaccion += (Time.time - momentoAparicionObjetivo);
            toquesValidos++;
            burbujaObject.SetActive(false); 
            conteoCorrectas++; 
            DispararReaccion(true); 
            
            if (!QuedanBurbujasDeColor(spriteBurbuja, burbujaObject))
            {
                indiceActual++; 
                // Si terminamos la lista de colores, reiniciamos para que el niño siga jugando
                if (indiceActual >= secuenciaObjetivos.Length) 
                {
                    indiceActual = 0;
                    MezclarSecuenciaObjetivos();
                    ReactivarTodasLasBurbujas(); // Método nuevo para no quedarse sin burbujas
                }
                ActualizarImagenObjetivo(); 
            }
        }
        else
        {
            conteoIncorrectas++; 
            DispararReaccion(false);        
        }
    }

    // Para que el juego sea "infinito" hasta que acabe el tiempo
    private void ReactivarTodasLasBurbujas()
    {
        BotonBurbuja[] todas = FindObjectsByType<BotonBurbuja>(FindObjectsSortMode.None);
        foreach (BotonBurbuja b in todas) b.gameObject.SetActive(true);
    }

    private void TerminarJuego()
    {
        if (!juegoActivo) return; // Evitar llamadas dobles
        juegoActivo = false; 

        textoCorrectas.text = conteoCorrectas.ToString();
        textoIncorrectas.text = conteoIncorrectas.ToString();
        if (textoPuntajeFinal != null) textoPuntajeFinal.text = "¡Lograste reventar " + conteoCorrectas + " burbujas!";
        
        if(contenedorJuego != null) contenedorJuego.SetActive(false);
        panelResultados.SetActive(true);

        int nivelFrustracion = Mathf.Clamp(1 + conteoIncorrectas, 1, 10);
        int promedioReaccionMs = toquesValidos > 0 ? Mathf.RoundToInt((sumaTiempoReaccion / toquesValidos) * 1000f) : 0;

        int pId = PlayerPrefs.GetInt("PacienteID", 1);
        int cId = PlayerPrefs.GetInt("CitaID", 1);

        _ = EnviarMetricasSupabase(pId, cId, nivelFrustracion, promedioReaccionMs);
    }

    private async Task EnviarMetricasSupabase(int pId, int cId, int frustracion, int reaccionMs)
    {
        if (_supabase == null) return;
        try {
            var metrica = new MetricaIA {
                PacienteId = pId,
                CitaId = cId,
                Frustracion = frustracion,
                LatenciaMs = 30,
                PresionToque = 1.0f,
                TiempoReaccionMs = reaccionMs,
                TipoJuegoId = tipoJuegoID
            };
            await _supabase.From<MetricaIA>().Insert(metrica);
            Debug.Log("Métricas Nivel 3 (Contrareloj) enviadas.");
        } catch (System.Exception ex) { Debug.LogError("Error: " + ex.Message); }
    }

    // Métodos de ayuda idénticos
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
    private void VolverANeutral() { if(imagenTico != null) imagenTico.sprite = spriteNeutral; if(textoDialogoTico != null) textoDialogoTico.text = fraseNeutral; }
    private bool QuedanBurbujasDeColor(Sprite colorBuscado, GameObject burbujaIgnorada) { BotonBurbuja[] todasLasBurbujas = FindObjectsByType<BotonBurbuja>(FindObjectsSortMode.None); foreach (BotonBurbuja burbuja in todasLasBurbujas) { if (burbuja.gameObject.activeInHierarchy && burbuja.gameObject != burbujaIgnorada && burbuja.GetComponent<Image>().sprite == colorBuscado) return true; } return false; }
    private void ActualizarImagenObjetivo() { if(indiceActual < secuenciaObjetivos.Length) { imagenObjetivoArriba.sprite = secuenciaObjetivos[indiceActual]; momentoAparicionObjetivo = Time.time; } }
    
    public void BotonMenu() { SceneManager.LoadScene("Flujo_Menu"); }
    public void BotonReiniciar() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
}