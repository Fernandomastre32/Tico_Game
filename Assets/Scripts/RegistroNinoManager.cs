using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Supabase;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class RegistroNinoManager : MonoBehaviour
{
    private string supabaseUrl = "https://gflucxpldvijkagerlzb.supabase.co";
    private string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImdmbHVjeHBsZHZpamthZ2VybHpiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzM0NDk2OTQsImV4cCI6MjA4OTAyNTY5NH0.vYYELn2ofGJRHPsFE4ZmCsq9a6-DMVLNQ6vn7zMc4vo"; 

    [Header("Datos de Texto")]
    [SerializeField] private TMP_InputField nombreInput;
    [SerializeField] private TMP_InputField apellidoPaternoInput;
    [SerializeField] private TMP_InputField apellidoMaternoInput;

    [Header("Desplegables de Fecha")]
    [SerializeField] private TMP_Dropdown dropdownDia;
    [SerializeField] private TMP_Dropdown dropdownMes;
    [SerializeField] private TMP_Dropdown dropdownAnio;

    [Header("Controles")]
    [SerializeField] private Button guardarBtn;
    [SerializeField] private TextMeshProUGUI mensajeFeedback;

    private Supabase.Client _supabase;

    async void Start()
    {
        var options = new SupabaseOptions { AutoRefreshToken = true };
        _supabase = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);
        await _supabase.InitializeAsync();

        LlenarDesplegables();

        if (guardarBtn != null)
        {
            guardarBtn.onClick.AddListener(ProcesarRegistro);
        }
    }

    #region Lógica de Fechas
    private void LlenarDesplegables()
    {
        dropdownDia.ClearOptions();
        List<string> dias = new List<string> { "Día" };
        for (int i = 1; i <= 31; i++) dias.Add(i.ToString("D02"));
        dropdownDia.AddOptions(dias);

        dropdownMes.ClearOptions();
        List<string> meses = new List<string> { "Mes" };
        for (int i = 1; i <= 12; i++) meses.Add(i.ToString("D02"));
        dropdownMes.AddOptions(meses);

        dropdownAnio.ClearOptions();
        List<string> anios = new List<string> { "Año" };
        int anioActual = DateTime.Now.Year;
        for (int i = anioActual; i >= anioActual - 20; i--) anios.Add(i.ToString());
        dropdownAnio.AddOptions(anios);
    }

    private string ObtenerFechaFormateada()
    {
        if (dropdownAnio.value == 0 || dropdownMes.value == 0 || dropdownDia.value == 0) return null; 

        string anio = dropdownAnio.options[dropdownAnio.value].text;
        string mes = dropdownMes.options[dropdownMes.value].text;
        string dia = dropdownDia.options[dropdownDia.value].text;

        return $"{anio}-{mes}-{dia}"; 
    }
    #endregion

    #region Lógica de Base de Datos
    private async void ProcesarRegistro()
    {
        if (string.IsNullOrWhiteSpace(nombreInput.text) || 
            string.IsNullOrWhiteSpace(apellidoPaternoInput.text) || 
            string.IsNullOrWhiteSpace(apellidoMaternoInput.text))
        {
            MostrarMensaje("Por favor llena todos los nombres.", Color.red);
            return;
        }

        string fechaNacimiento = ObtenerFechaFormateada();
        if (fechaNacimiento == null)
        {
            MostrarMensaje("Por favor selecciona una fecha completa.", Color.red);
            return;
        }

        int idTutor = PlayerPrefs.GetInt("TutorId", -1);
        
        if (idTutor == -1)
        {
            MostrarMensaje("Error: No se detectó la sesión del tutor.", Color.red);
            return;
        }

        MostrarMensaje("Guardando registro...", Color.yellow);
        guardarBtn.interactable = false; 

        try
        {
            var nuevoNino = new Paciente
            {
                Nombre = nombreInput.text.Trim(),
                ApellidoPaterno = apellidoPaternoInput.text.Trim(),
                ApellidoMaterno = apellidoMaternoInput.text.Trim(),
                FechaNacimiento = fechaNacimiento,
                TutorId = idTutor, 
                EstadoActivo = true
            };

            var respuestaInsert = await _supabase.From<Paciente>().Insert(nuevoNino);

            if (respuestaInsert.Models.Count > 0)
            {
                // Guardamos el Id real (UUID) recién creado como un texto
string idNuevoNino = respuestaInsert.Models[0].Id;
PlayerPrefs.SetString("PacienteID", idNuevoNino);
PlayerPrefs.Save();
            }

            MostrarMensaje("¡Registro exitoso! Entrando al juego...", Color.green);
            
            StartCoroutine(CargarEscenaMenuSeguro());
        }
        catch (Exception ex)
        {
            guardarBtn.interactable = true;
            MostrarMensaje("Error al guardar: " + ex.Message, Color.red);
            Debug.LogError(ex);
        }
    }

    private System.Collections.IEnumerator CargarEscenaMenuSeguro()
    {
        yield return new WaitForSeconds(1.5f); 
        SceneManager.LoadScene("Flujo_Menu"); 
    }

    private void MostrarMensaje(string mensaje, Color color)
    {
        if (mensajeFeedback != null)
        {
            mensajeFeedback.text = mensaje;
            mensajeFeedback.color = color;
        }
    }
    #endregion
}