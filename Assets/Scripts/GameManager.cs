using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Interfaz de Usuario")]
    public GameObject overlayInstrucciones;
    public GameObject contenedorJuego; 
    public Image imagenObjetivoArriba; 
    
    [Header("Lógica del Nivel")]
    public Sprite[] secuenciaObjetivos; 
    private int indiceActual = 0;

    void Start()
    {
        overlayInstrucciones.SetActive(true);
        if(contenedorJuego != null) 
        {
            contenedorJuego.SetActive(false); 
        }
    }

    public void IniciarJuego()
    {
        overlayInstrucciones.SetActive(false);
        if(contenedorJuego != null)
        {
            contenedorJuego.SetActive(true); 
        }
        ActualizarImagenObjetivo();
    }

    public void EvaluarBurbujaTocada(Sprite spriteBurbuja, GameObject burbujaObject)
    {
        if (indiceActual >= secuenciaObjetivos.Length) return; 

        // 1. Verificamos si tocó el color que pide la imagen de arriba
        if (spriteBurbuja == secuenciaObjetivos[indiceActual])
        {
            // Apagamos la burbuja que acaba de tocar
            burbujaObject.SetActive(false); 

            // 2. Revisamos si AÚN QUEDAN burbujas de este mismo color
            if (!QuedanBurbujasDeColor(spriteBurbuja))
            {
                // Si ya NO quedan, avanzamos al siguiente color en la lista
                indiceActual++;

                if (indiceActual < secuenciaObjetivos.Length)
                {
                    ActualizarImagenObjetivo();
                }
                else
                {
                    Debug.Log("¡Nivel Completado!");
                    // Aquí el nivel termina
                }
            }
        }
        else
        {
            Debug.Log("Color incorrecto, intenta de nuevo.");
        }
    }

    // Esta es la nueva función que busca en la pantalla si quedan globos del color actual
    private bool QuedanBurbujasDeColor(Sprite colorBuscado)
    {
        // Encuentra todos los scripts BotonBurbuja en la escena
        BotonBurbuja[] todasLasBurbujas = FindObjectsOfType<BotonBurbuja>();

        foreach (BotonBurbuja burbuja in todasLasBurbujas)
        {
            // Si encuentra un globo que sigue activo y tiene el mismo dibujo, avisa que sí quedan
            if (burbuja.gameObject.activeInHierarchy && burbuja.GetComponent<Image>().sprite == colorBuscado)
            {
                return true;
            }
        }
        // Si revisó todo y no encontró ninguno, avisa que ya no quedan
        return false;
    }

    private void ActualizarImagenObjetivo()
    {
        if(secuenciaObjetivos.Length > 0 && indiceActual < secuenciaObjetivos.Length)
        {
            imagenObjetivoArriba.sprite = secuenciaObjetivos[indiceActual];
        }
    }
}