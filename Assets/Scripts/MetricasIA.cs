using Postgrest.Attributes;
using Postgrest.Models;
using System;

[Serializable]
[Table("metricas_ia")] // El nombre de tu tabla en Supabase
public class MetricaIA : BaseModel
{[Column("paciente_id")]
public string PacienteId { get; set; } // DEBE ser string para aceptar el UUID
    [Column("cita_id")] public int CitaId { get; set; }
    [Column("frustracion")] public int Frustracion { get; set; }
    [Column("latencia_ms")] public int LatenciaMs { get; set; }
    [Column("presion_toque")] public float PresionToque { get; set; }
    [Column("tiempo_reaccion_ms")] public int TiempoReaccionMs { get; set; }
    [Column("tipo_juego_id")] public int TipoJuegoId { get; set; }
}