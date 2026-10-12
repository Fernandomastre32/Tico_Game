using Postgrest.Attributes;
using Postgrest.Models;

[Table("pacientes")]
public class Paciente : BaseModel
{
    // Regresamos el Id original a string para que no choque
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

    [Column("tutor_id")] 
    public int TutorId { get; set; } 
    
    [Column("estado_activo")]
    public bool EstadoActivo { get; set; }

    // ¡TU EXCELENTE IDEA! Agregamos el expediente (int8 en BD = long en C#)
    [Column("numero_expediente")]
    public long NumeroExpediente { get; set; }
}