using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; 
using System.Text.RegularExpressions;

public class MovimientoTicoCurvo : MonoBehaviour
{
    public RectTransform tico;
    public float velocidad = 400f;
    
    private bool moviendose = false;

    // Esta función la llama el botón (OnClick)
    public void SeleccionarNivel(GameObject contenedorCamino)
    {
        if (moviendose) return;

        // Buscamos los puntos (hijos del objeto que arrastraste al botón)
        List<RectTransform> puntos = new List<RectTransform>();
        foreach (RectTransform hijo in contenedorCamino.transform)
        {
            puntos.Add(hijo);
        }

        if (puntos.Count > 0)
        {
            StartCoroutine(RutinaSeguirCamino(puntos, contenedorCamino.name));
        }
        else
        {
            Debug.LogError("¡Error! El objeto " + contenedorCamino.name + " no tiene puntos hijos para el camino.");
        }
    }

    IEnumerator RutinaSeguirCamino(List<RectTransform> puntos, string nombreObjeto)
    {
        moviendose = true;

        foreach (RectTransform punto in puntos)
        {
            Vector3 posDestino = punto.position; 

            // Movimiento fluido punto a punto
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
        
        // Extraemos el número del nombre (ej: de "Nivel1" saca "1")
        string numeroNivel = Regex.Match(nombreObjeto, @"\d+").Value;
        
        CargarNivel(numeroNivel);
    }

    void CargarNivel(string numero)
    {
        string escenaFinal = "";

        // Lógica para coincidir con tus nombres de archivo exactos
        if (numero == "1") escenaFinal = "nivel1";
        else if (numero == "2") escenaFinal = "nivel2";
        else if (numero == "3") escenaFinal = "Nivel3";

        if (!string.IsNullOrEmpty(escenaFinal))
        {
            Debug.Log("Tico llegó. Cargando escena: " + escenaFinal);
            SceneManager.LoadScene(escenaFinal);
        }
        else
        {
            Debug.LogError("No se encontró una configuración para el nivel número: " + numero);
        }
    }

    public void RegresarAlMenu()
{
    SceneManager.LoadScene("Flujo_Menu");
}
}