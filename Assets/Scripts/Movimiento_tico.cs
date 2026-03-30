using UnityEngine;

// Aseguramos que el objeto tenga Rigidbody2D y Animator
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Movimiento_tico : MonoBehaviour 
{
    [Header("Configuración de Movimiento")]
    public float velocidad = 5f;

    // AÑADIDO: Zona muerta ajustable desde el inspector. 
    // Un valor entre 0.1 y 0.2 suele funcionar bien.
    [Range(0f, 0.5f)]
    public float zonaMuerta = 0.15f; 

    [Header("Controles (Arrastra aquí el Joystick)")]
    public Joystick joystick; 

    private Rigidbody2D rb;
    private Vector2 direccion;
    private Animator anim; // Referencia al Animator

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); // Buscamos el componente Animator

        // --- IMPORTANTE PARA EL LABERINTO ---
        // Asegúrate de que en el Inspector, en el Rigidbody2D de Tico, 
        // la opción "Gravity Scale" esté en 0.
    }

    void Update()
    {
        if (joystick != null)
        {
            // Obtenemos los valores 'crudos' del joystick
            float inputH = joystick.Horizontal;
            float inputV = joystick.Vertical;

            // Creamos un vector temporal para medir la 'fuerza' del toque
            Vector2 inputRaw = new Vector2(inputH, inputV);

            // --- CORRECCIÓN: Implementación de la Zona Muerta ---
            // Medimos la 'magnitud' (fuerza) del toque. Si es menor que la zona muerta,
            // forzamos la dirección a CERO absoluto.
            if (inputRaw.magnitude < zonaMuerta)
            {
                direccion = Vector2.zero; // Tico se queda quieto
            }
            else
            {
                // Si el toque es firme (mayor que la zona muerta), asignamos la dirección.
                // Opcional: Usar inputRaw.normalized para que corra igual en diagonal.
                direccion = inputRaw; 
            }
            
            // --- CONTROL DE ANIMACIÓN ---
            // Si direccion NO es cero, Tico debe caminar.
            // Usamos un pequeño umbral de seguridad (0.01f)
            bool moviendose = direccion.magnitude > 0.01f;

            if (anim != null)
            {
                // Actualizamos el parámetro del Animator.
                // IMPORTANTE: Asegúrate de crear un parámetro Bool llamado "estaCaminando" 
                // en tu Animator Controller y configurar las transiciones.
                anim.SetBool("estaCaminando", moviendose);
            }
        }
    }

    void FixedUpdate()
    {
        // Solo intentamos mover si el Rigidbody existe
        if (rb != null)
        {
            // CORRECCIÓN: Solo aplicamos movimiento si la dirección NO es cero.
            // Esto asegura paradas en seco.
            if (direccion.magnitude > 0.01f)
            {
                // Movemos a Tico usando su Rigidbody (física)
                rb.MovePosition(rb.position + direccion * velocidad * Time.fixedDeltaTime);
            }
            else
            {
                // Si no hay input, forzamos la velocidad física a cero.
                // Esto evita que siga deslizando por "inercia" si el Rigidbody tiene fricción.
                rb.linearVelocity = Vector2.zero; 
            }
        }
    }
}