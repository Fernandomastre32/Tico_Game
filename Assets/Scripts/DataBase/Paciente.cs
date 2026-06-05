using Postgrest.Attributes;
using Postgrest.Models;

[Table("pacientes")]
public class Paciente : BaseModel
{
    [PrimaryKey("id", false)] 
    public string Id { get; set; }

    [Column("nombre")]
    public string Nombre { get; set; }

    [Column("tutor_email")]
    public string TutorEmail { get; set; }
    
    [Column("peso_kg")]
    public float Peso { get; set; }
}