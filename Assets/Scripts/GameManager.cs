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
    [Header("Configuración del Tipo de Juego")]
    public int tipoJuegoID = 1; // 1 para Burbujas

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
    // AQUÍ ESTABA EL ERROR: Agregamos las frases que faltaban
    public string[] frasesCorrectas = { "¡Muy bien!", "¡Ese es el color!", "¡Genial!" };
    public string[] frasesAnimo = { "¡Casi!, intenta otra vez", "¡Tú puedes!", "¡Sigue buscando!" };

    [Header("Lógica del Nivel")]
    public Sprite[] secuenciaObjetivos; 
    public bool patronAleatorio = true;
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
        overlayInstrucciones.SetActive(true);
        panelResultados.SetActive(false); 
        if(contenedorJuego != null) contenedorJuego.SetActive(false); 

        try {
            var options = new SupabaseOptions { AutoRefreshToken = true };
            _supabase = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);
            await _supabase.InitializeAsync();
        } catch (System.Exception ex) { Debug.LogError("Error Supabase: " + ex.Message); }
    }

    void Update() { if (juegoActivo) tiempoJugado += Time.deltaTime; }

    public void IniciarJuego()
    {
        overlayInstrucciones.SetActive(false); 
        if(contenedorJuego != null) contenedorJuego.SetActive(true); 
        juegoActivo = true; 
        if (patronAleatorio && secuenciaObjetivos.Length > 0) MezclarSecuenciaObjetivos();
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
        if (!juegoActivo || indiceActual >= secuenciaObjetivos.Length) return; 

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

    private void TerminarJuego()
    {
        juegoActivo = false; 
        textoCorrectas.text = conteoCorrectas.ToString();
        textoIncorrectas.text = conteoIncorrectas.ToString();
        textoTiempo.text = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(tiempoJugado / 60F), Mathf.FloorToInt(tiempoJugado % 60F));
        
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
        } catch (System.Exception ex) { Debug.LogError("Error: " + ex.Message); }
    }

    // Métodos de ayuda
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
    private bool QuedanBurbujasDeColor(Sprite colorBuscado, GameObject burbujaIgnorada) { BotonBurbuja[] todasLasBurbujas = FindObjectsByType<BotonBurbuja>(FindObjectsSortMode.None); foreach (BotonBurbuja burbuja in todasLasBurbujas) { if (burbuja.gameObject.activeInHierarchy && burbuja.gameObject != burbujaIgnorada && burbuja.GetComponent<Image>().sprite == colorBuscado) return true; } return false; }
    private void ActualizarImagenObjetivo() { if(indiceActual < secuenciaObjetivos.Length) { imagenObjetivoArriba.sprite = secuenciaObjetivos[indiceActual]; momentoAparicionObjetivo = Time.time; } }
    public void BotonMenu() { SceneManager.LoadScene("Flujo_Menu"); }
    public void BotonSiguiente() { SceneManager.LoadScene("nivel2"); }
}