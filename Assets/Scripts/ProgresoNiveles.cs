using UnityEngine;

public static class ProgresoNiveles
{
    public const int Total = 5;
    // ponytail: conectar las fases 4 y 5 cuando sus escenas jugables estén terminadas.
    public static readonly string[] Escenas = { "nivel1", "Nivel2", "Nivel3", "", "" };
    public static readonly string[] Nombres = { "Burbujas", "Laberinto", "Burbujas rápidas", "El vigía", "Cima de Tico" };
    public static string Clave => "Tico.Progreso." + PlayerPrefs.GetString("PacienteID",
        PlayerPrefs.GetString("UserEmail", "invitado"));
    public static int Completados => Mathf.Clamp(PlayerPrefs.GetInt(Clave, 0), 0, Total);
    public static bool Desbloqueado(int nivel) => nivel >= 1 && nivel <= Total && nivel <= Completados + 1;

    public static void Completar(int nivel)
    {
        // Solo avanza una fase; repetir una anterior nunca borra el progreso.
        if (nivel != Completados + 1 || nivel > Total) return;
        PlayerPrefs.SetInt(Clave, nivel);
        PlayerPrefs.Save();
    }
}
