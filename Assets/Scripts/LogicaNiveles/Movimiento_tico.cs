using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Movimiento_tico : MonoBehaviour 
{
    [Header("Configuración de Movimiento")]
    public float velocidad = 5f;
    [Range(0f, 0.5f)]
    public float zonaMuerta = 0.15f; 

    [Header("Controles")]
    public Joystick joystick; 

    private Rigidbody2D rb;
    private Vector2 direccion;
    private Animator anim;
    private GameManagerLaberinto manager; // Referencia al manager para avisarle de los choques

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
        // Buscamos al manager en la escena automáticamente
        manager = Object.FindFirstObjectByType<GameManagerLaberinto>();
    }

    void Update()
    {
        if (joystick != null)
        {
            float inputH = joystick.Horizontal;
            float inputV = joystick.Vertical;
            Vector2 inputRaw = new Vector2(inputH, inputV);

            // Implementación de Zona Muerta
            if (inputRaw.magnitude < zonaMuerta)
            {
                direccion = Vector2.zero;
            }
            else
            {
                direccion = inputRaw; 
            }
            
            // Control de Animación
            bool moviendose = direccion.magnitude > 0.01f;
            if (anim != null)
            {
                anim.SetBool("estaCaminando", moviendose);
            }

            // Lógica de Volteo (Flip) - Mira hacia donde camina
            if (direccion.x < -0.1f) transform.localScale = new Vector3(-1, 1, 1);
            else if (direccion.x > 0.1f) transform.localScale = new Vector3(1, 1, 1);
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            if (direccion.magnitude > 0.01f)
            {
                rb.MovePosition(rb.position + direccion * velocidad * Time.fixedDeltaTime);
            }
            else
            {
                rb.linearVelocity = Vector2.zero; 
            }
        }
    }

    // --- DETECCIÓN DE CHOQUES ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Revisa que la pared tenga el Tag "Pared"
        if (collision.gameObject.CompareTag("Pared"))
        {
            if (manager != null)
            {
                manager.RegistrarGolpePared();
            }
        }
    }
}