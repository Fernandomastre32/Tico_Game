using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // Para cargar niveles

public class MovimientoTicoCurvo : MonoBehaviour
{
    public RectTransform tico;
    public float velocidad = 400f;
    
    [Header("Configuración de Escenas")]
    public string nombreBaseEscena = "nivel1"; // Tus escenas deben llamarse Nivel_1, Nivel_2, etc.

    private bool moviendose = false;

    // Esta función la llamará el botón
    public void SeleccionarNivel(GameObject contenedorCamino)
    {
        if (moviendose) return;

        // Obtenemos todos los hijos (puntos) del camino
        List<RectTransform> puntos = new List<RectTransform>();
        foreach (RectTransform hijo in contenedorCamino.transform)
        {
            puntos.Add(hijo);
        }

        StartCoroutine(RutinaSeguirCamino(puntos, contenedorCamino.name));
    }

  IEnumerator RutinaSeguirCamino(List<RectTransform> puntos, string nombreObjeto)
{
    moviendose = true;

    foreach (RectTransform punto in puntos)
    {
        // Usamos .position (Mundial) en lugar de .anchoredPosition
        Vector3 posDestino = punto.position; 

        while (Vector3.Distance(tico.position, posDestino) > 0.1f)
        {
            tico.position = Vector3.MoveTowards(
                tico.position, 
                posDestino, 
                velocidad * Time.deltaTime 
            );
            yield return null;
        }
    }

        moviendose = false;
        
        // Extraer el número del nombre del objeto para cargar la escena
        // Si el objeto se llama "Nivel1", cargará la escena "Nivel_1"
        string numeroNivel = System.Text.RegularExpressions.Regex.Match(nombreObjeto, @"\d+").Value;
        CargarNivel(numeroNivel);
    }

    void CargarNivel(string numero)
    {
        Debug.Log("Cargando Nivel " + numero);
        // SceneManager.LoadScene(nombreBaseEscena + numero); 
        // Descomenta la línea de arriba cuando tengas las escenas creadas
    }
}