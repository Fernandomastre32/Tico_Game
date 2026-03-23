using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 
using TMPro; 
using System.Collections; 
using UnityEngine.Networking; 
using System.Text; 

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
    private int indiceActual = 0; 
    private int conteoCorrectas = 0; 
    private int conteoIncorrectas = 0; 
    private float tiempoJugado = 0f; 
    private bool juegoActivo = false; 

    [Header("Métricas IA y API")]
    private string urlApiMetricas = "http://localhost:3000/api/metricas-ia";
    private float momentoAparicionObjetivo = 0f; 
    private float sumaTiempoReaccion = 0f; 
    private int toquesValidos = 0; 

    private Coroutine rutinaReaccion;

    void Start()
    {
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
        ActualizarImagenObjetivo(); 
        VolverANeutral();
    }

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

// MÉTRICAS
int nivelFrustracion = Mathf.Clamp(1 + conteoIncorrectas, 1, 10);
int promedioReaccionMs = toquesValidos > 0 ? Mathf.RoundToInt((sumaTiempoReaccion / toquesValidos) * 1000f) : 0;

        // Recuperamos los IDs guardados por el AuthManager
        int pId = PlayerPrefs.GetInt("PacienteID", 1);
        int cId = PlayerPrefs.GetInt("CitaID", 1);

StartCoroutine(EnviarMetricasAPI(pId, cId, nivelFrustracion, 30, 1.0f, promedioReaccionMs));
  }

private IEnumerator EnviarMetricasAPI(int pId, int cId, int frustracion, int latencia, float presion, int tiempoReaccion)
{
string jsonDatos = "{" +
"\"paciente_id\":" + pId + "," +
"\"cita_id\":" + cId + "," +
"\"frustracion\":" + frustracion + "," +
"\"latencia_ms\":" + latencia + "," +
"\"presion_toque\":" + presion.ToString("F2").Replace(",", ".") + "," +
"\"tiempo_reaccion_ms\":" + tiempoReaccion +
"}";

byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonDatos);
using (UnityWebRequest request = new UnityWebRequest(urlApiMetricas, "POST"))
{
request.uploadHandler = new UploadHandlerRaw(bodyRaw);
request.downloadHandler = new DownloadHandlerBuffer();
request.SetRequestHeader("Content-Type", "application/json");

// EL TOKEN VIENE DEL AUTHMANAGER
string token = PlayerPrefs.GetString("TokenSesion", ""); 
if (!string.IsNullOrEmpty(token)) request.SetRequestHeader("Authorization", "Bearer " + token);

yield return request.SendWebRequest();
if (request.result != UnityWebRequest.Result.Success) Debug.LogError("Error API: " + request.error);
else Debug.Log("Métricas guardadas!");
}
}

public void BotonMenu() { SceneManager.LoadScene("MainMenu"); }
public void BotonSiguiente() { SceneManager.LoadScene("nivel2"); }
}