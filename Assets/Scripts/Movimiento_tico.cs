using UnityEngine;

public class Movimiento_tico : MonoBehaviour 
{
    [Header("Configuración")]
    public float velocidad = 5f;

    [Header("Controles")]
    public Joystick joystick; 

    private Rigidbody2D rb;
    private Vector2 direccion;
    
    // --- NUEVAS VARIABLES PARA ANIMACIÓN ---
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Buscamos los componentes automáticamente al iniciar
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (joystick != null)
        {
            direccion.x = joystick.Horizontal;
            direccion.y = joystick.Vertical;

            // 1. Lógica de Animación: 
            // Si la magnitud del movimiento es mayor a 0.1, está caminando
            bool moviendose = direccion.magnitude > 0.1f;
            
            if (anim != null)
            {
                anim.SetBool("estaCaminando", moviendose);
            }

            // 2. Lógica de Volteo (Flip):
            // Si va a la izquierda, lo volteamos. Si va a la derecha, normal.
            if (direccion.x < -0.1f)
            {
                transform.localScale = new Vector3(-1, 1, 1); // Mirar izquierda
            }
            else if (direccion.x > 0.1f)
            {
                transform.localScale = new Vector3(1, 1, 1);  // Mirar derecha
            }
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.MovePosition(rb.position + direccion * velocidad * Time.fixedDeltaTime);
        }
    }
}