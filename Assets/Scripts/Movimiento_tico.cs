using UnityEngine;

public class Movimiento_tico : MonoBehaviour 
{
    [Header("Configuración")]
    public float velocidad = 5f;

    [Header("Controles")]
    // ¡Aquí estaba el error! Cambiamos VariableJoystick por Joystick
    public Joystick joystick; 

    private Rigidbody2D rb;
    private Vector2 direccion;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (joystick != null)
        {
            direccion.x = joystick.Horizontal;
            direccion.y = joystick.Vertical;
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