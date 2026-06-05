using Postgrest.Attributes;
using Postgrest.Models;

[Table("tutores")]
public class Tutor : BaseModel
{
    [Column("email")] 
    public string Email { get; set; }

    [Column("nombre")]
    public string Nombre { get; set; }
}