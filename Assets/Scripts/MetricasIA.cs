using Postgrest.Attributes;
using Postgrest.Models;
using System;

[Serializable]
[Table("metricas_ia")] // El nombre exacto de tu tabla en Supabase
public class MetricaIA : BaseModel
{
    // Usamos PrimaryKey para decirle a Supabase que este es el ID único.
    // El 'false' significa que la base de datos lo genera automáticamente (1, 2, 3...)
    [PrimaryKey("id", false)] 
    public int Id { get; set; }

    [Column("paciente_id")]
    public string PacienteId { get; set; } // string para aceptar el UUID

    [Column("cita_id")] 
    public int CitaId { get; set; }
    
    [Column("tipo_juego_id")] 
    public int TipoJuegoId { get; set; }

    [Column("frustracion")] 
    public int Frustracion { get; set; }

    [Column("latencia_ms")] 
    public int LatenciaMs { get; set; }

    [Column("presion_toque")] 
    public float PresionToque { get; set; }

    [Column("tiempo_reaccion_ms")] 
    public int TiempoReaccionMs { get; set; }

    // ¡La pieza que faltaba para registrar si es 'completado' o 'Abandonado'!
    [Column("estado_partida")] 
    public string EstadoPartida { get; set; }

    // Opcional pero recomendado: para poder leer cuándo se jugó
    [Column("fecha_registro")] 
    public DateTime FechaRegistro { get; set; }
}