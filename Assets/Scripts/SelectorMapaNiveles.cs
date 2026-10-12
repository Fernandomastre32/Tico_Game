using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectorMapaNiveles : MonoBehaviour
{
    public Button[] niveles;
    public TMP_Text[] estados;
    public RectTransform[] recorrido;
    public int[] paradas;
    public RectTransform tico;
    public RectTransform seleccion;
    public TMP_Text titulo, descripcion, progreso, textoJugar;
    public Button jugar, volver;
    public RectTransform areaSegura;
    public RectTransform contenido;
    private int elegido, puntoActual;
    private bool viajando;
    private Rect ultimaArea;
    private Vector2 ultimaResolucion;

    private void Start()
    {
        Time.timeScale = 1;
        for (int i = 0; i < niveles.Length; i++)
        {
            int indice = i;
            niveles[i].onClick.AddListener(() => Seleccionar(indice));
        }
        jugar.onClick.AddListener(Jugar);
        volver.onClick.AddListener(() => SceneManager.LoadScene("Flujo_Menu"));
        elegido = Mathf.Min(ProgresoNiveles.Completados, niveles.Length - 1);
        puntoActual = paradas[elegido];
        tico.anchoredPosition = recorrido[puntoActual].anchoredPosition + new Vector2(0, 65);
        Mostrar();
        AjustarPantalla();
        EventSystem.current?.SetSelectedGameObject(niveles[elegido].gameObject);
        if (AudioManager.instance != null)
            AudioManager.instance.CambiarMusica(AudioManager.instance.musicaMenu);
    }

    private void Update()
    {
        if (ultimaArea != Screen.safeArea || ultimaResolucion != new Vector2(Screen.width, Screen.height))
            AjustarPantalla();
    }

    private void AjustarPantalla()
    {
        ultimaArea = Screen.safeArea;
        ultimaResolucion = new Vector2(Screen.width, Screen.height);
        areaSegura.anchorMin = ultimaArea.position / ultimaResolucion;
        areaSegura.anchorMax = (ultimaArea.position + ultimaArea.size) / ultimaResolucion;
        Canvas.ForceUpdateCanvases();
        float escala = Mathf.Min(areaSegura.rect.width / 1600f, areaSegura.rect.height / 900f);
        contenido.localScale = Vector3.one * escala;
    }

    public void Seleccionar(int indice)
    {
        if (viajando || indice < 0 || indice >= niveles.Length) return;
        elegido = indice;
        Mostrar();
    }

    public void Mostrar()
    {
        int completados = ProgresoNiveles.Completados;
        for (int i = 0; i < niveles.Length; i++)
        {
            bool abierto = ProgresoNiveles.Desbloqueado(i + 1);
            niveles[i].image.color = i < completados ? new Color32(39, 113, 88, 255)
                : abierto ? new Color32(239, 169, 58, 255) : new Color32(92, 120, 119, 255);
            niveles[i].GetComponentInChildren<TMP_Text>().color = abierto && i >= completados
                ? new Color32(36, 77, 73, 255) : new Color32(255, 247, 227, 255);
            estados[i].text = string.IsNullOrEmpty(ProgresoNiveles.Escenas[i]) ? "PRÓXIMAMENTE"
                : i < completados ? "COMPLETADO" : abierto ? "DISPONIBLE" : "BLOQUEADO";
        }
        seleccion.anchoredPosition = niveles[elegido].GetComponent<RectTransform>().anchoredPosition;
        titulo.text = $"{elegido + 1:00}  /  {ProgresoNiveles.Nombres[elegido]}";
        bool existe = !string.IsNullOrEmpty(ProgresoNiveles.Escenas[elegido]);
        bool disponible = ProgresoNiveles.Desbloqueado(elegido + 1);
        descripcion.text = !existe ? "Una nueva aventura te espera aquí. Próximamente."
            : !disponible ? $"Completa {ProgresoNiveles.Nombres[elegido - 1]} para abrir este camino."
            : "Sigue el sendero con Tico. ¡Vamos a explorar!";
        progreso.text = $"{completados} / 5 fases completadas";
        jugar.interactable = disponible && existe && !viajando;
        textoJugar.text = !existe ? "Próximamente" : !disponible ? "Bloqueado" : "¡Vamos!";
    }

    public void Jugar()
    {
        if (viajando || !ProgresoNiveles.Desbloqueado(elegido + 1)) return;
        string escena = ProgresoNiveles.Escenas[elegido];
        if (string.IsNullOrEmpty(escena)) return;
        if (!Application.CanStreamedLevelBeLoaded(escena))
        {
            descripcion.text = "Este nivel no está disponible en esta versión.";
            return;
        }
        StartCoroutine(Viajar(escena));
    }

    private IEnumerator Viajar(string escena)
    {
        viajando = true;
        jugar.interactable = false;
        volver.interactable = false;
        foreach (Button boton in niveles) boton.interactable = false;
        int destino = paradas[elegido];
        while (puntoActual != destino)
        {
            puntoActual += destino > puntoActual ? 1 : -1;
            Vector2 punto = recorrido[puntoActual].anchoredPosition + new Vector2(0, 65);
            while (Vector2.Distance(tico.anchoredPosition, punto) > 0.1f)
            {
                tico.anchoredPosition = Vector2.MoveTowards(tico.anchoredPosition, punto, 650 * Time.unscaledDeltaTime);
                yield return null;
            }
        }
        SceneManager.LoadScene(escena);
    }
}
