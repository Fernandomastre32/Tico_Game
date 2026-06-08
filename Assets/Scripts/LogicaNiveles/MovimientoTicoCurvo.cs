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
    
    [Header("Animación")]
    public Animator animatorTico; // Arrastra el Animator de Tico aquí

    private bool moviendose = false;

    private void Start()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.CambiarMusica(AudioManager.instance.musicaMenu);
        }
    }

    public void SeleccionarNivel(GameObject contenedorCamino)
    {
        if (moviendose) return;

        List<RectTransform> puntos = new List<RectTransform>();
        foreach (RectTransform hijo in contenedorCamino.transform)
        {
            puntos.Add(hijo);
        }

        if (puntos.Count > 0)
        {
            StartCoroutine(RutinaSeguirCamino(puntos, contenedorCamino.name));
        }
    }

    IEnumerator RutinaSeguirCamino(List<RectTransform> puntos, string nombreObjeto)
    {
        moviendose = true;

        // 1. ACTIVAR ANIMACIÓN DE CAMINAR
        if (animatorTico != null) 
        {
            animatorTico.SetBool("estaCaminando", true);
        }

        foreach (RectTransform punto in puntos)
        {
            Vector3 posDestino = punto.position; 

            // 2. VOLTEAR A TICO HACIA EL DESTINO
            ActualizarOrientacion(posDestino);

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

        // 3. DESACTIVAR ANIMACIÓN (Tico se detiene al llegar)
        if (animatorTico != null) 
        {
            animatorTico.SetBool("estaCaminando", false);
        }

        moviendose = false;
        string numeroNivel = Regex.Match(nombreObjeto, @"\d+").Value;
        CargarNivel(numeroNivel);
    }

    // Función extra para que Tico siempre mire a donde va
    void ActualizarOrientacion(Vector3 destino)
    {
        if (destino.x < tico.position.x)
        {
            tico.localScale = new Vector3(-1, 1, 1); // Mirar izquierda
        }
        else if (destino.x > tico.position.x)
        {
            tico.localScale = new Vector3(1, 1, 1);  // Mirar derecha
        }
    }

    void CargarNivel(string numero)
    {
        string escenaFinal = "";
        if (numero == "1") escenaFinal = "nivel1";
        else if (numero == "2") escenaFinal = "nivel2";
        else if (numero == "3") escenaFinal = "Nivel3";

        if (!string.IsNullOrEmpty(escenaFinal))
        {
            SceneManager.LoadScene(escenaFinal);
        }
    }

    public void RegresarAlMenu()
    {
        SceneManager.LoadScene("Flujo_Menu");
    }
}