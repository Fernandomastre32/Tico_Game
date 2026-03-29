using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    // 1. Asegúrate de que esta variable sea privada y NO la arrastres en el inspector
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
            
            // 2. ¡CRUCIAL! Aquí es donde el script "encuentra" al reproductor
            audioSource = GetComponent<AudioSource>();

            // Verificación de seguridad
            if (audioSource == null)
            {
                Debug.LogError("¡Cuidado! El objeto AudioManager NO tiene el componente Audio Source.");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Línea 34: Ahora llamamos a la música de inicio con seguridad
        if (musicaMenu != null)
        {
            CambiarMusica(musicaMenu);
        }
    }

    public void CambiarMusica(AudioClip nuevaPista)
    {
        // 3. Verificación de seguridad para evitar el NullReference en la línea 45
        if (audioSource == null) 
        {
            // Intentamos buscarlo una vez más por si acaso
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) return; 
        }

        if (nuevaPista == null || audioSource.clip == nuevaPista) return;

        // Línea 45: Ahora audioSource ya no será null
        audioSource.Stop();
        audioSource.clip = nuevaPista;
        audioSource.Play();
    }
}