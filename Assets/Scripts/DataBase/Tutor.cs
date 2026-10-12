using Postgrest.Attributes;
using Postgrest.Models;

[Table("tutores")]
public class Tutor : BaseModel
{
    // Le decimos que es el ID y que NO lo generamos en Unity (false) porque Postgres lo hace automático
    [PrimaryKey("id", false)] 
    public int Id { get; set; }

    [Column("email")] 
    public string Email { get; set; }

    [Column("nombre")]
    public string Nombre { get; set; }
}