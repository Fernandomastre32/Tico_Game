using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    private AudioSource audioSource;

    [Header("Configuración de Pistas")]
    public AudioClip musicaMenu;
    public AudioClip musicaNivel1;
    public AudioClip musicaNivel2;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                Debug.LogError("¡Cuidado! El objeto AudioManager NO tiene el componente Audio Source.");
            }
            else
            {
                // Cargar el volumen de la música guardado (1f es el máximo por defecto)
                audioSource.volume = PlayerPrefs.GetFloat("VolumenMusica", 1f);
            }

            // Cargar el volumen general/efectos del juego
            AudioListener.volume = PlayerPrefs.GetFloat("VolumenJuego", 1f);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (musicaMenu != null)
        {
            CambiarMusica(musicaMenu);
        }
    }

    public void CambiarMusica(AudioClip nuevaPista)
    {
        if (audioSource == null) 
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) return; 
        }

        if (nuevaPista == null || audioSource.clip == nuevaPista) return;

        audioSource.Stop();
        audioSource.clip = nuevaPista;
        audioSource.Play();
    }

    // --- NUEVAS FUNCIONES PARA LOS SLIDERS ---

    public void SetVolumenMusica(float volumen)
    {
        if (audioSource != null) audioSource.volume = volumen;
        PlayerPrefs.SetFloat("VolumenMusica", volumen); // Guarda el valor
    }

    public void SetVolumenJuego(float volumen)
    {
        AudioListener.volume = volumen; // Controla todos los demás sonidos (efectos)
        PlayerPrefs.SetFloat("VolumenJuego", volumen); // Guarda el valor
    }
}