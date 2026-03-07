// Estas líneas le dicen a Unity qué herramientas extra vamos a usar en este archivo.
using UnityEngine;
using UnityEngine.UI;               // Necesario para los textos e imágenes clásicas de la interfaz.
using UnityEngine.SceneManagement;  // Necesario para poder cambiar de nivel al terminar.
using TMPro;                        // Necesario para los textos modernos (TextMeshPro).
using System.Collections;           // NUEVO: Necesario para usar las "Corrutinas" (los temporizadores).

public class GameManager : MonoBehaviour
{
    // [Header(...)] crea títulos en el Inspector de Unity para mantener tus casillas ordenadas.

    [Header("Interfaz de Usuario")]
    // Referencias a las partes principales de tu pantalla.
    public GameObject overlayInstrucciones; // La pantalla inicial con fondo de pizarrón.
    public GameObject contenedorJuego;      // La carpeta que agrupa los globos y al robot.
    public Image imagenObjetivoArriba;      // El cuadrito de arriba que te dice qué color reventar.
    
    [Header("Panel de Resultados")]
    // Referencias a los textos donde mostraremos los puntos al final.
    public GameObject panelResultados;      // La pantalla final de puntaje.
    public Text textoCorrectas;             // Texto para los aciertos.
    public Text textoIncorrectas;           // Texto para los errores.
    public Text textoTiempo;                // Texto para el reloj final.

    [Header("Reacciones de Tico")]
    // Referencias físicas del robot en la pantalla.
    public Image imagenTico;                // El componente Image del robot.
    public TMP_Text textoDialogoTico;       // El globo de texto de Tico.

    // Variables para el estado "Neutral" (cuando Tico está esperando o dando instrucciones).
    public Sprite spriteNeutral;            // El dibujo de Tico sentadito o esperando.
    public string fraseNeutral = "Puedes reventar las burbujas tocándolas"; // Su frase base.
    
    // float es un número con decimales. Aquí definimos cuántos segundos dura su reacción.
    public float tiempoReaccion = 2.5f; 

    // Los corchetes [] indican que esto es una "Lista" (Arreglo). Aquí pondremos varios dibujos y frases.
    public Sprite[] spritesFelices; // Lista de dibujos celebrando (tico_good, tico_title).
    public Sprite[] spritesAnimo;   // Lista de dibujos pensando o dando ánimos (tico_thinking, tico_calling).
    
    public string[] frasesCorrectas = { "¡Muy bien!", "¡Ese es el color!", "¡Genial, sigue así!" };
    public string[] frasesAnimo = { "¡Casi!, busca bien", "¡Tú puedes, intenta con otro!", "¡Sigue buscando!" };

    // Una "Coroutine" es como un proceso que corre en el fondo. La guardamos en esta variable para poder detenerla si es necesario.
    private Coroutine rutinaReaccion;

    [Header("Lógica del Nivel")]
    // Variables para controlar el avance del jugador.
    public Sprite[] secuenciaObjetivos;     // La lista de colores que debe seguir para ganar.
    private int indiceActual = 0;           // Un número que nos dice en qué color de la lista vamos (empieza en 0).

    // Variables internas para las matemáticas del nivel (no se ven en el Inspector).
    private int conteoCorrectas = 0;        
    private int conteoIncorrectas = 0;      
    private float tiempoJugado = 0f;        // Lleva la cuenta de los segundos transcurridos.
    private bool juegoActivo = false;       // Un "interruptor" verdadero/falso para saber si el reloj debe avanzar.

    // Start se ejecuta una sola vez cuando el nivel arranca.
    void Start()
    {
        // Encendemos las instrucciones y apagamos todo lo demás.
        overlayInstrucciones.SetActive(true);
        panelResultados.SetActive(false); 
        if(contenedorJuego != null) contenedorJuego.SetActive(false); 
    }

    // Update se ejecuta constantemente, en cada fotograma del juego.
    void Update()
    {
        // Si el interruptor del juego está encendido, sumamos el tiempo que ha pasado al reloj.
        if (juegoActivo) 
        {
            tiempoJugado += Time.deltaTime; 
        }
    }

    // Esta función se llama al presionar la "X" en las instrucciones iniciales.
    public void IniciarJuego()
    {
        overlayInstrucciones.SetActive(false); // Apagamos el pizarrón.
        if(contenedorJuego != null) contenedorJuego.SetActive(true); // Encendemos los globos.
        
        juegoActivo = true; // Arrancamos el cronómetro interno.
        ActualizarImagenObjetivo(); // Ponemos el primer color a buscar.
        
        // Ponemos a Tico en su estado original usando esta función que creamos abajo.
        VolverANeutral();
    }

    // Esta función la llaman las burbujas cuando el jugador las toca.
    public void EvaluarBurbujaTocada(Sprite spriteBurbuja, GameObject burbujaObject)
    {
        // Si el juego ya terminó, no hacemos nada aunque toquen una burbuja.
        if (!juegoActivo || indiceActual >= secuenciaObjetivos.Length) return; 

        // 1. SI ACERTÓ EL COLOR:
        if (spriteBurbuja == secuenciaObjetivos[indiceActual])
        {
            burbujaObject.SetActive(false); // Escondemos esa burbuja.
            conteoCorrectas++;              // Le sumamos un punto bueno.
            
            DispararReaccion(true); // Le decimos a la función de Tico que ponga cara feliz (true).

            // Revisamos si quedan más burbujas de ese color (ignorando la que acabamos de reventar).
            if (!QuedanBurbujasDeColor(spriteBurbuja, burbujaObject))
            {
                indiceActual++; // Como ya no hay de ese color, avanzamos al siguiente de la lista.
                
                // Si aún nos quedan colores en la lista...
                if (indiceActual < secuenciaObjetivos.Length)
                {
                    ActualizarImagenObjetivo(); // Actualizamos el cuadrito de arriba.
                }
                else
                {
                    TerminarJuego(); // Si ya no hay colores, pasamos a la pantalla de resultados.
                }
            }
        }
        // 2. SI SE EQUIVOCÓ DE COLOR:
        else
        {
            conteoIncorrectas++;    // Le sumamos un error.
            DispararReaccion(false); // Le decimos a Tico que dé ánimos (false).
        }
    }

    // FUNCIÓN INTERMEDIARIA PARA LA REACCIÓN:
    // Esta función prepara el temporizador antes de iniciarlo.
    private void DispararReaccion(bool esCorrecto)
    {
        // Si Tico ya estaba haciendo una reacción anterior, detenemos ese temporizador viejo para que no se encimen.
        if (rutinaReaccion != null)
        {
            StopCoroutine(rutinaReaccion);
        }
        // Iniciamos el temporizador nuevo y lo guardamos en nuestra variable.
        rutinaReaccion = StartCoroutine(RutinaReaccionTico(esCorrecto));
    }

    // EL TEMPORIZADOR DE TICO (Corrutina):
    // IEnumerator permite que la función se pause a la mitad sin congelar todo el juego.
    private IEnumerator RutinaReaccionTico(bool esCorrecto)
    {
        // Si acertó (esCorrecto == true)
        if (esCorrecto)
        {
            // Elegimos un dibujo al azar de la lista "spritesFelices"
            if (spritesFelices.Length > 0 && imagenTico != null)
            {
                // Random.Range elige un número entre 0 y el total de dibujos en la lista.
                imagenTico.sprite = spritesFelices[Random.Range(0, spritesFelices.Length)];
            }
            // Elegimos una frase al azar de "frasesCorrectas"
            if (textoDialogoTico != null && frasesCorrectas.Length > 0)
            {
                textoDialogoTico.text = frasesCorrectas[Random.Range(0, frasesCorrectas.Length)];
            }
        }
        // Si se equivocó (esCorrecto == false)
        else
        {
            // Elegimos un dibujo al azar de "spritesAnimo"
            if (spritesAnimo.Length > 0 && imagenTico != null)
            {
                imagenTico.sprite = spritesAnimo[Random.Range(0, spritesAnimo.Length)];
            }
            // Elegimos una frase al azar de "frasesAnimo"
            if (textoDialogoTico != null && frasesAnimo.Length > 0)
            {
                textoDialogoTico.text = frasesAnimo[Random.Range(0, frasesAnimo.Length)];
            }
        }

        // AQUÍ ESTÁ LA MAGIA: Le decimos a Unity "Pausa esta función aquí mismo durante 2.5 segundos".
        // El resto del juego sigue funcionando normal, pero esta función espera.
        yield return new WaitForSeconds(tiempoReaccion);

        // Una vez que pasaron los 2.5 segundos, ejecutamos la función para que Tico vuelva a su pose normal.
        VolverANeutral();
    }

    // FUNCIÓN PARA REINICIAR A TICO:
    private void VolverANeutral()
    {
        // Le regresamos su dibujo base y su texto base.
        if (imagenTico != null && spriteNeutral != null)
        {
            imagenTico.sprite = spriteNeutral;
        }
        if (textoDialogoTico != null)
        {
            textoDialogoTico.text = fraseNeutral;
        }
    }

    // FUNCIÓN BUSCADORA DE BURBUJAS:
    private bool QuedanBurbujasDeColor(Sprite colorBuscado, GameObject burbujaIgnorada)
    {
        // Encuentra todos los botones de burbuja en la escena.
        BotonBurbuja[] todasLasBurbujas = FindObjectsOfType<BotonBurbuja>();
        
        foreach (BotonBurbuja burbuja in todasLasBurbujas)
        {
            // Si encuentra una activa, que no sea la que acabamos de tocar, y que coincida en color...
            if (burbuja.gameObject.activeInHierarchy && 
                burbuja.gameObject != burbujaIgnorada && 
                burbuja.GetComponent<Image>().sprite == colorBuscado)
            {
                return true; // ...avisa que SÍ encontró una.
            }
        }
        return false; // Si revisó todas y no encontró ninguna válida, avisa que NO quedan.
    }

    // FUNCIÓN PARA ACTUALIZAR EL CUADRITO SUPERIOR:
    private void ActualizarImagenObjetivo()
    {
        if(secuenciaObjetivos.Length > 0 && indiceActual < secuenciaObjetivos.Length)
        {
            imagenObjetivoArriba.sprite = secuenciaObjetivos[indiceActual];
        }
    }

    // FUNCIÓN PARA FINALIZAR EL NIVEL:
    private void TerminarJuego()
    {
        juegoActivo = false; // Apagamos el cronómetro.
        
        // Si Tico estaba en medio de una reacción, la detenemos de golpe.
        if (rutinaReaccion != null) StopCoroutine(rutinaReaccion);
        
        // Matemáticas para convertir los segundos puros en minutos y segundos (ej. 65 seg -> 01:05)
        int minutos = Mathf.FloorToInt(tiempoJugado / 60F);
        int segundos = Mathf.FloorToInt(tiempoJugado % 60F);
        
        // Pasamos los números a las cajas de texto de la pantalla final.
        textoCorrectas.text = conteoCorrectas.ToString();
        textoIncorrectas.text = conteoIncorrectas.ToString();
        textoTiempo.text = string.Format("{0:00}:{1:00}", minutos, segundos);

        // Escondemos el juego y mostramos los resultados.
        if(contenedorJuego != null) contenedorJuego.SetActive(false);
        panelResultados.SetActive(true);
    }

    // --- BOTONES FINALES ---
    // (Recuerda que estas escenas deben estar en File > Build Settings para funcionar).
    public void BotonMenu() { SceneManager.LoadScene("SampleScene"); }
    public void BotonSiguiente() { SceneManager.LoadScene("nivel2"); }
}