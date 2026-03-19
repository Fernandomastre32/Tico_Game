using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Text; // <- ¡ESTA ES LA QUE RESUELVE TU ERROR!
using TMPro;

public class GameManagerLaberinto : MonoBehaviour
{
    [Header("Interfaz de Usuario")]
public GameObject overlayInstrucciones;
public GameObject panelResultados;
public TMP_Text textoTiempo; // ¡Cambio aquí!
public TMP_Text textoGolpes; // ¡Cambio aquí!
    [Header("Métricas del Laberinto")]
    private int conteoGolpes = 0;
    private float tiempoJugado = 0f;
    private bool juegoActivo = false;

    [Header("Métricas IA y Conexión")]
    private string urlApiMetricas = "http://localhost:3000/api/metricas-ia";

    void Start()
    {
        // Al empezar, mostramos instrucciones y pausamos el tiempo
        overlayInstrucciones.SetActive(true);
        panelResultados.SetActive(false);
        juegoActivo = false;
    }

    void Update()
    {
        // Solo contamos el tiempo si el jugador ya cerró las instrucciones
        if (juegoActivo)
        {
            tiempoJugado += Time.deltaTime;
        }
    }

    // Pon este método en el botón de la "X" del panel de instrucciones
    public void IniciarJuego()
    {
        overlayInstrucciones.SetActive(false);
        juegoActivo = true;
    }

    // Este método lo llamará Tico cada vez que se pegue con un bambú
    public void RegistrarGolpePared()
    {
        if (juegoActivo)
        {
            conteoGolpes++;
            Debug.Log("¡Ouch! Tico chocó. Golpes totales: " + conteoGolpes);
        }
    }

    // Este método lo llamará la estrella cuando Tico la pise
    public void TerminarJuego()
    {
        juegoActivo = false; // Detenemos el cronómetro

        // Convertimos el tiempo a minutos y segundos para la pantalla
        int minutos = Mathf.FloorToInt(tiempoJugado / 60F);
        int segundos = Mathf.FloorToInt(tiempoJugado % 60F);

        if (textoGolpes != null) textoGolpes.text = conteoGolpes.ToString();
        if (textoTiempo != null) textoTiempo.text = string.Format("{0:00}:{1:00}", minutos, segundos);

        panelResultados.SetActive(true);

        // --- CÁLCULO DE MÉTRICAS IA ---
        // La frustración sube si choca mucho (ejemplo topado a 10)
        int nivelFrustracion = Mathf.Clamp(1 + conteoGolpes, 1, 10);
        int tiempoTotalSegundos = Mathf.RoundToInt(tiempoJugado);

        int pacienteIdTemporal = 1; 
        int citaIdTemporal = 1;

        StartCoroutine(EnviarMetricasAPI(pacienteIdTemporal, citaIdTemporal, nivelFrustracion, tiempoTotalSegundos, conteoGolpes));
    }

    private IEnumerator EnviarMetricasAPI(int pId, int cId, int frustracion, int tiempoSegundos, int golpes)
    {
        // Adapté el JSON para que mande los golpes en lugar de "presión táctil"
        string jsonDatos = "{" +
            "\"paciente_id\":" + pId + "," +
            "\"cita_id\":" + cId + "," +
            "\"frustracion\":" + frustracion + "," +
            "\"tiempo_jugado_s\":" + tiempoSegundos + "," +
            "\"golpes_pared\":" + golpes +
        "}";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonDatos);

        using (UnityWebRequest request = new UnityWebRequest(urlApiMetricas, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            string tokenGuardado = PlayerPrefs.GetString("TokenSesion", "");

            if (!string.IsNullOrEmpty(tokenGuardado))
            {
                request.SetRequestHeader("Authorization", "Bearer " + tokenGuardado);
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error al guardar métricas del laberinto: " + request.error);
            }
            else
            {
                Debug.Log("¡Métricas de Laberinto guardadas con éxito! " + request.downloadHandler.text);
            }
        }
    }

    // --- BOTONES FINALES ---
    public void BotonMenu() { SceneManager.LoadScene("MenuPrincipal"); } // Cambia el nombre si es distinto
    public void BotonSiguiente() { SceneManager.LoadScene("nivel3"); }   // Cambia al nivel que siga
}