using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // NUEVO: Necesario para que funcionen los botones de nivel

public class GameManager : MonoBehaviour
{
    [Header("Interfaz de Usuario")]
    public GameObject overlayInstrucciones;
    public GameObject contenedorJuego; 
    public Image imagenObjetivoArriba; 
    
    [Header("Panel de Resultados")]
    public GameObject panelResultados; // Arrastra aquí tu nuevo panel
    public Text textoCorrectas;        // El texto donde irá el número de correctas
    public Text textoIncorrectas;      // El texto donde irá el número de errores
    public Text textoTiempo;           // El texto donde irá el reloj

    [Header("Lógica del Nivel")]
    public Sprite[] secuenciaObjetivos; 
    private int indiceActual = 0;

    // Variables de evaluación
    private int conteoCorrectas = 0;
    private int conteoIncorrectas = 0;
    private float tiempoJugado = 0f;
    private bool juegoActivo = false;

    void Start()
    {
        overlayInstrucciones.SetActive(true);
        panelResultados.SetActive(false); // Aseguramos que el score inicie oculto

        if(contenedorJuego != null) contenedorJuego.SetActive(false); 
    }

    void Update()
    {
        // El cronómetro solo avanza mientras el jugador está en el nivel
        if (juegoActivo)
        {
            tiempoJugado += Time.deltaTime;
        }
    }

    public void IniciarJuego()
    {
        overlayInstrucciones.SetActive(false);
        if(contenedorJuego != null) contenedorJuego.SetActive(true); 
        
        juegoActivo = true; // Iniciamos el reloj y el juego
        ActualizarImagenObjetivo();
    }

    public void EvaluarBurbujaTocada(Sprite spriteBurbuja, GameObject burbujaObject)
    {
        if (!juegoActivo || indiceActual >= secuenciaObjetivos.Length) return; 

        // 1. Verificamos si tocó el color correcto
        if (spriteBurbuja == secuenciaObjetivos[indiceActual])
        {
            burbujaObject.SetActive(false); 
            conteoCorrectas++;

            // 2. Revisamos si quedan burbujas (¡Le pasamos la burbuja actual para que la ignore!)
            if (!QuedanBurbujasDeColor(spriteBurbuja, burbujaObject))
            {
                indiceActual++;

                if (indiceActual < secuenciaObjetivos.Length)
                {
                    ActualizarImagenObjetivo();
                }
                else
                {
                    TerminarJuego(); // Ya no hay más colores en la secuencia
                }
            }
        }
        else
        {
            // Tocó una burbuja equivocada
            conteoIncorrectas++;
            Debug.Log("Error registrado.");
            // Opcional: Podrías reproducir un sonido de error aquí.
            // No apagamos la burbuja incorrecta para que pueda usarla cuando sea su turno real.
        }
    }

    // ARREGLO DEL BUG: Ahora recibe 'burbujaIgnorada' para no contar la que acabamos de reventar
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
        }
    }

    private void TerminarJuego()
    {
        juegoActivo = false;
        
        // Convertimos el tiempo flotante a minutos y segundos reales
        int minutos = Mathf.FloorToInt(tiempoJugado / 60F);
        int segundos = Mathf.FloorToInt(tiempoJugado % 60F);
        
        // Asignamos los números a la UI
        textoCorrectas.text = conteoCorrectas.ToString();
        textoIncorrectas.text = conteoIncorrectas.ToString();
        textoTiempo.text = string.Format("{0:00}:{1:00}", minutos, segundos);

        // Apagamos los globos que hayan sobrado y mostramos los resultados
        if(contenedorJuego != null) contenedorJuego.SetActive(false);
        panelResultados.SetActive(true);
    }

    // --- MÉTODOS PARA LOS BOTONES DEL PANEL DE RESULTADOS ---
    public void BotonMenu()
    {
        // Cambia "MenuPrincipal" por el nombre exacto de tu escena de menú
        SceneManager.LoadScene("MenuPrincipal"); 
    }

    public void BotonSiguiente()
    {
        // Cambia "Nivel2" por el nombre exacto de tu siguiente nivel
        SceneManager.LoadScene("Nivel2"); 
    }
}