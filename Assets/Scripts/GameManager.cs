using UnityEngine;
using UnityEngine.UI;               
using UnityEngine.SceneManagement;  
using TMPro;                        
using System.Collections;           
using UnityEngine.Networking;       // NUEVO: Necesario para mandar las métricas a la API.
using System.Text;                  // NUEVO: Para codificar el JSON.

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

    private Coroutine rutinaReaccion;

    [Header("Lógica del Nivel")]
    public Sprite[] secuenciaObjetivos;     
    private int indiceActual = 0;           

    private int conteoCorrectas = 0;        
    private int conteoIncorrectas = 0;      
    private float tiempoJugado = 0f;        
    private bool juegoActivo = false;       

    [Header("Métricas IA y Conexión")]
    // Aquí pondremos la dirección de tu API para las métricas
    private string urlApiMetricas = "http://localhost:3000/api/metricas-ia";
    
    // Variables para calcular las matemáticas del paciente
    private float momentoAparicionObjetivo = 0f; // Guarda el momento en el que se pidió un color
    private float sumaTiempoReaccion = 0f;       // Acumula los tiempos de reacción
    private int toquesValidos = 0;               // Para sacar el promedio al final

    void Start()
    {
        overlayInstrucciones.SetActive(true);
        panelResultados.SetActive(false); 
        if(contenedorJuego != null) contenedorJuego.SetActive(false); 
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
        overlayInstrucciones.SetActive(false); 
        if(contenedorJuego != null) contenedorJuego.SetActive(true); 
        
        juegoActivo = true; 
        ActualizarImagenObjetivo(); 
        VolverANeutral();
    }

    public void EvaluarBurbujaTocada(Sprite spriteBurbuja, GameObject burbujaObject)
    {
        if (!juegoActivo || indiceActual >= secuenciaObjetivos.Length) return; 

        // 1. SI ACERTÓ EL COLOR:
        if (spriteBurbuja == secuenciaObjetivos[indiceActual])
        {
            // MÉTRICA: Calculamos cuánto tardó desde que se le pidió el color hasta que lo tocó
            float tiempoReaccionToque = Time.time - momentoAparicionObjetivo;
            sumaTiempoReaccion += tiempoReaccionToque;
            toquesValidos++;

            burbujaObject.SetActive(false); 
            conteoCorrectas++;              
            DispararReaccion(true); 

            if (!QuedanBurbujasDeColor(spriteBurbuja, burbujaObject))
            {
                indiceActual++; 
                
                if (indiceActual < secuenciaObjetivos.Length)
                {
                    ActualizarImagenObjetivo(); 
                }
                else
                {
                    TerminarJuego(); 
                }
            }
        }
        // 2. SI SE EQUIVOCÓ DE COLOR:
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
        BotonBurbuja[] todasLasBurbujas = FindObjectsOfType<BotonBurbuja>();
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
            
            // MÉTRICA: Registramos en qué segundo exacto apareció este nuevo objetivo
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

        // --- CÁLCULO DE MÉTRICAS IA ---
        
        // 1. Nivel de frustración (Inicia en 1. Suma 1 por cada error. Topado a 10).
        int nivelFrustracion = Mathf.Clamp(1 + conteoIncorrectas, 1, 10);
        
        // 2. Promedio de Tiempo de Reacción (en milisegundos)
        int promedioReaccionMs = 0;
        if (toquesValidos > 0) 
        {
            promedioReaccionMs = Mathf.RoundToInt((sumaTiempoReaccion / toquesValidos) * 1000f);
        }

        // 3. Variables de entorno / Hardware
        int latenciaSimuladaMs = Random.Range(15, 45); // Milisegundos de retraso simulado del dispositivo
        float presionPantalla = 1.0f; // En PC es 1.0. (En móvil sería: Input.GetTouch(0).pressure)

        // IMPORTANTE: Debes obtener estos IDs de donde los hayas guardado al hacer Login
        int pacienteIdTemporal = 1; 
        int citaIdTemporal = 1;

        // Disparamos la corrutina para enviar los datos a PostgreSQL
        StartCoroutine(EnviarMetricasAPI(pacienteIdTemporal, citaIdTemporal, nivelFrustracion, latenciaSimuladaMs, presionPantalla, promedioReaccionMs));
    }

    // --------------------------------------------------------
    // CONEXIÓN CON LA API DE NODE.JS
    // --------------------------------------------------------
    private IEnumerator EnviarMetricasAPI(int pId, int cId, int frustracion, int latencia, float presion, int tiempoReaccion)
    {
        // 1. Armamos el JSON respetando los nombres de tu tabla en la BD
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

            // --- SEGURIDAD JWT ---
            // Como tu ruta está protegida por verifyToken, necesitamos enviarle el gafete de acceso.
            // Aquí extraemos el token que (idealmente) guardaste en PlayerPrefs durante el Login.
            string tokenGuardado = PlayerPrefs.GetString("TokenSesion", ""); 
            if (!string.IsNullOrEmpty(tokenGuardado))
            {
                request.SetRequestHeader("Authorization", "Bearer " + tokenGuardado);
            }

            // Enviamos el paquete
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error al guardar métricas: " + request.error);
                Debug.LogError("Respuesta del servidor: " + request.downloadHandler.text);
            }
            else
            {
                Debug.Log("¡Métricas IA guardadas con éxito en PostgreSQL! " + request.downloadHandler.text);
            }
        }
    }

    // --- BOTONES FINALES ---
    public void BotonMenu() { SceneManager.LoadScene("SampleScene"); }
    public void BotonSiguiente() { SceneManager.LoadScene("nivel2"); }
}