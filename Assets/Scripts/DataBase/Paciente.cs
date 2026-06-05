using Postgrest.Attributes;
using Postgrest.Models;

[Table("pacientes")]
public class Paciente : BaseModel
{
    [PrimaryKey("id", false)] 
    public string Id { get; set; }

    [Column("nombre")]
    public string Nombre { get; set; }

    [Column("apellido_paterno")]
    public string ApellidoPaterno { get; set; }

    [Column("apellido_materno")]
    public string ApellidoMaterno { get; set; }

    [Column("fecha_nacimiento")]
    public string FechaNacimiento { get; set; }

    // ¡Cambio clave! Ahora es un entero para que coincida con el int4 de tu imagen
    [Column("tutor_id")] 
    public int TutorId { get; set; } 
    
    [Column("estado_activo")]
    public bool EstadoActivo { get; set; }
}