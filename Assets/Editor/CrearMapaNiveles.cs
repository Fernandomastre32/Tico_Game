using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class CrearMapaNiveles
{
    private static Sprite redondo, circulo;
    private static TMP_FontAsset fuente;
    private static readonly Color Tinta = Hex("244D49"), Crema = Hex("FFF7E3");
    private static readonly Vector2[] Ruta = {
        new Vector2(-565, -145), new Vector2(-420, -145), new Vector2(-420, 90),
        new Vector2(-205, 90), new Vector2(-205, -65), new Vector2(-90, -65),
        new Vector2(105, -65), new Vector2(255, -65), new Vector2(435, -65),
        new Vector2(435, 145)
    };

    [MenuItem("Tico/Mapa/Crear selector de cinco niveles")]
    public static void Crear()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        redondo = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        circulo = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        fuente = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(0, 0, -10);
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Hex("A8DDD6");
        var canvas = new GameObject("Mapa de Tico", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        var fondo = Caja(canvas.transform, "Mar", Vector2.zero, new Vector2(4000, 4000), Hex("A8DDD6"));
        var safe = Rect(canvas.transform, "Área segura", Vector2.zero, Vector2.zero);
        safe.anchorMin = Vector2.zero; safe.anchorMax = Vector2.one;
        var root = Rect(safe, "Composición 1600 x 900", Vector2.zero, new Vector2(1600, 900));
        var control = canvas.gameObject.AddComponent<SelectorMapaNiveles>();
        control.areaSegura = safe; control.contenido = root;

        for (int i = 0; i < 37; i++)
        {
            float x = -740 + (i * 193 % 1490), y = -260 + (i * 127 % 565);
            Caja(root, "Onda del agua", new Vector2(x, y), new Vector2(28 + i % 3 * 12, 4), Hex("C5EAE1"), true);
        }
        Nube(root, -675, 235, .7f); Nube(root, 665, 230, .9f); Nube(root, 15, 255, .6f);
        Isla(root, "Isla del descubrimiento", new Vector2(-365, -5), new Vector2(680, 425));
        Isla(root, "Isla de la atención", new Vector2(385, 25), new Vector2(590, 490));
        Texto(root, "Pradera", "PRADERA DEL DESCUBRIMIENTO", new Vector2(-360, 170), new Vector2(450, 30), 20, Tinta);
        Texto(root, "Cumbre", "COLINA DE LA ATENCIÓN", new Vector2(380, 245), new Vector2(420, 30), 20, Tinta);

        var sendero = Rect(root, "Sendero por fases", Vector2.zero, Vector2.zero);
        for (int i = 0; i < Ruta.Length - 1; i++) Linea(sendero, "Borde del sendero", Ruta[i], Ruta[i + 1], 34, Hex("A88145"));
        foreach (Vector2 punto in Ruta) Caja(sendero, "Curva", punto, new Vector2(34, 34), Hex("A88145"), false, true);
        for (int i = 0; i < Ruta.Length - 1; i++) Linea(sendero, "Arena", Ruta[i], Ruta[i + 1], 24, Hex("F3D596"));
        foreach (Vector2 punto in Ruta) Caja(sendero, "Unión de arena", punto, new Vector2(24, 24), Hex("F3D596"), false, true);
        Puente(root);
        Arbol(root, -610, 55, 1.1f); Arbol(root, -540, 140, .65f); Arbol(root, -310, -130, .65f);
        Arbol(root, 190, 155, 1); Arbol(root, 585, 45, .85f); Arbol(root, 590, -115, .6f);
        Flores(root, -580, -45); Flores(root, -325, 55); Flores(root, -150, 180);
        Flores(root, 165, -155); Flores(root, 330, 120); Flores(root, 545, 180);
        Oveja(root, -515, -5); Oveja(root, -335, 120);
        Caja(root, "Bandera - mástil", new Vector2(502, 178), new Vector2(6, 88), Hex("715439"), true);
        Caja(root, "Bandera de la cima", new Vector2(525, 205), new Vector2(47, 30), Hex("E59855"), true);

        control.paradas = new[] { 0, 2, 4, 7, 9 };
        control.recorrido = new RectTransform[Ruta.Length];
        var puntos = Rect(root, "Puntos del recorrido de Tico", Vector2.zero, Vector2.zero);
        for (int i = 0; i < Ruta.Length; i++) control.recorrido[i] = Rect(puntos, "Punto " + i, Ruta[i], Vector2.zero);
        control.niveles = new Button[5]; control.estados = new TMP_Text[5];
        control.seleccion = Caja(root, "Anillo de selección", Ruta[0], new Vector2(104, 104), Crema, false, true).rectTransform;
        for (int i = 0; i < 5; i++)
        {
            Vector2 p = Ruta[control.paradas[i]];
            Caja(root, "Base estación " + (i + 1), p + new Vector2(0, -7), new Vector2(90, 90), Hex("4D6D4E"), false, true);
            control.niveles[i] = Boton(root, "Nivel " + (i + 1), (i + 1).ToString(), p, new Vector2(84, 84), i == 0 ? Hex("EFA93A") : Hex("5C7877"), true);
            if (i == 0) control.niveles[i].GetComponentInChildren<TMP_Text>().color = Tinta;
            var label = Caja(root, "Cartel " + (i + 1), p + new Vector2(0, -78), new Vector2(198, 62), Crema, true);
            Texto(label.transform, "Nombre", ProgresoNiveles.Nombres[i], new Vector2(0, 10), new Vector2(190, 27), 22, Tinta);
            control.estados[i] = Texto(label.transform, "Estado", i == 0 ? "DISPONIBLE" : i >= 3 ? "PRÓXIMAMENTE" : "BLOQUEADO", new Vector2(0, -15), new Vector2(190, 20), 14, Tinta);
        }
        var tico = Caja(root, "Tico - viajero", Ruta[0] + new Vector2(0, 65), new Vector2(82, 96), Color.white);
        tico.sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/SpriteSheets/tico/Tico/tico_thinking.png").OfType<Sprite>().First();
        tico.preserveAspect = true;
        control.tico = tico.rectTransform;

        var cabecera = Caja(root, "Cabecera", new Vector2(0, 348), new Vector2(1460, 132), Crema, true).transform;
        Texto(cabecera, "Mundo", "MUNDO 01  /  LA AVENTURA DE TICO", new Vector2(-225, 35), new Vector2(850, 25), 20, Hex("49796C"), TextAlignmentOptions.Left);
        Texto(cabecera, "Título", "Un pequeño paso, una gran aventura", new Vector2(-225, -8), new Vector2(850, 57), 40, Tinta, TextAlignmentOptions.Left);
        control.progreso = Texto(cabecera, "Progreso", "0 / 5 fases completadas", new Vector2(510, 16), new Vector2(355, 40), 25, Tinta);
        Texto(cabecera, "Ayuda", "Explora a tu ritmo", new Vector2(510, -22), new Vector2(340, 30), 21, Hex("49796C"));

        var pie = Caja(root, "Panel del nivel elegido", new Vector2(0, -353), new Vector2(1460, 144), Crema, true).transform;
        control.volver = Boton(pie, "Volver al menú", "Menú", new Vector2(-630, 0), new Vector2(135, 64), Hex("DDE8D7"));
        control.titulo = Texto(pie, "Nivel elegido", "01  /  Burbujas", new Vector2(-150, 27), new Vector2(770, 48), 34, Tinta, TextAlignmentOptions.Left);
        control.descripcion = Texto(pie, "Descripción", "Sigue el sendero con Tico. ¡Vamos a explorar!", new Vector2(-150, -23), new Vector2(770, 38), 23, Tinta, TextAlignmentOptions.Left);
        control.jugar = Boton(pie, "Entrar al nivel", "¡Vamos!", new Vector2(535, 0), new Vector2(285, 78), Hex("28705B"));
        control.textoJugar = control.jugar.GetComponentInChildren<TMP_Text>();
        control.textoJugar.color = Crema;
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        EditorSceneManager.SaveScene(escena, "Assets/Scenes/flujo_Niveles.unity");
        AssetDatabase.SaveAssets();
        Validar();
        Capturar(camera, canvas);
        Debug.Log("TICO_MAPA_OK: selector guardado, validación correcta y vista previa generada.");
    }

    private static void Isla(Transform padre, string nombre, Vector2 centro, Vector2 tamano)
    {
        var isla = Rect(padre, nombre, centro, Vector2.zero);
        Vector2[] posiciones = { new Vector2(-30, 0), new Vector2(35, 45), new Vector2(20, -85) };
        Vector2[] medidas = { tamano, new Vector2(tamano.x * .72f, tamano.y), new Vector2(tamano.x * .68f, tamano.y * .8f) };
        Color[] colores = { Hex("79B8AA"), Hex("776440"), Hex("C4BC7C"), Hex("92B961"), Hex("ACCE79") };
        for (int capa = 0; capa < colores.Length; capa++)
            for (int i = 0; i < posiciones.Length; i++)
                Caja(isla, new[] { "Sombra sobre agua", "Tierra", "Orilla", "Césped", "Pradera" }[capa],
                    posiciones[i] + new Vector2(0, new[] { -28, -19, -8, 0, 7 }[capa]),
                    medidas[i] + Vector2.one * (capa == 0 ? 24 : capa == 4 ? -18 : 0), colores[capa], true);
    }

    private static void Puente(Transform padre)
    {
        var puente = Rect(padre, "Puente de madera", new Vector2(7, -65), Vector2.zero);
        Caja(puente, "Sombra", new Vector2(0, -18), new Vector2(222, 72), Hex("679E8F"), true);
        for (int i = 0; i < 11; i++)
            Caja(puente, "Tablón " + (i + 1), new Vector2(-100 + i * 20, 0), new Vector2(18, 74), i % 2 == 0 ? Hex("D7A665") : Hex("E9BE7F"), true);
        for (int lado = -1; lado <= 1; lado += 2)
        {
            Caja(puente, "Barandal", new Vector2(0, lado * 34), new Vector2(230, 7), Hex("785737"), true);
            for (int i = 0; i < 4; i++)
                Caja(puente, "Poste", new Vector2(-108 + i * 72, lado * 34 + 6), new Vector2(10, 24), Hex("956C3D"), true);
        }
        Texto(puente, "Nombre del puente", "PUENTE DE LOS LOGROS", new Vector2(0, -70), new Vector2(220, 24), 13, Tinta);
    }

    private static void Arbol(Transform padre, float x, float y, float escala)
    {
        var t = Rect(padre, "Árbol", new Vector2(x, y), Vector2.zero); t.localScale = Vector3.one * escala;
        Caja(t, "Sombra", new Vector2(8, -15), new Vector2(85, 24), Hex("83A65E"), false, true);
        Caja(t, "Tronco", new Vector2(0, 18), new Vector2(13, 65), Hex("947044"), true);
        Caja(t, "Copa oscura", new Vector2(0, 62), new Vector2(90, 104), Hex("437D54"), false, true);
        Caja(t, "Copa iluminada", new Vector2(-12, 78), new Vector2(66, 77), Hex("639B58"), false, true);
        Caja(t, "Hoja", new Vector2(-21, 94), new Vector2(27, 37), Hex("83B668"), false, true);
    }

    private static void Flores(Transform padre, float x, float y)
    {
        var f = Rect(padre, "Flores", new Vector2(x, y), Vector2.zero);
        for (int i = 0; i < 3; i++)
        {
            Vector2 p = new Vector2(i * 15, i % 2 * 11);
            Caja(f, "Pétalos", p, new Vector2(12, 12), Crema, false, true);
            Caja(f, "Centro", p, new Vector2(5, 5), Hex("E8AD4C"), false, true);
        }
    }

    private static void Oveja(Transform padre, float x, float y)
    {
        var o = Rect(padre, "Oveja de la pradera", new Vector2(x, y), Vector2.zero);
        Caja(o, "Sombra", new Vector2(3, -8), new Vector2(44, 14), Hex("84A760"), false, true);
        Caja(o, "Patas", new Vector2(0, -5), new Vector2(24, 14), Hex("607269"), true);
        Caja(o, "Lana", new Vector2(0, 5), new Vector2(42, 26), Crema, false, true);
        Caja(o, "Cabeza", new Vector2(20, 7), new Vector2(14, 18), Hex("53665D"), false, true);
    }

    private static void Nube(Transform padre, float x, float y, float escala)
    {
        var nube = Rect(padre, "Nube", new Vector2(x, y), Vector2.zero); nube.localScale = Vector3.one * escala;
        Caja(nube, "Base", Vector2.zero, new Vector2(118, 36), Hex("E5F4E9"), false, true);
        Caja(nube, "Volumen", new Vector2(-16, 20), new Vector2(58, 56), Hex("F4F9EB"), false, true);
        Caja(nube, "Volumen", new Vector2(22, 10), new Vector2(62, 49), Hex("F4F9EB"), false, true);
    }

    private static RectTransform Rect(Transform padre, string nombre, Vector2 pos, Vector2 size)
    {
        var rect = new GameObject(nombre, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(padre, false); rect.anchoredPosition = pos; rect.sizeDelta = size;
        return rect;
    }

    private static Image Caja(Transform padre, string nombre, Vector2 pos, Vector2 size, Color color, bool rounded = false, bool circle = false)
    {
        var img = Rect(padre, nombre, pos, size).gameObject.AddComponent<Image>();
        img.color = color; img.raycastTarget = false;
        if (circle) img.sprite = circulo;
        else if (rounded) { img.sprite = redondo; img.type = Image.Type.Sliced; img.pixelsPerUnitMultiplier = .2f; }
        return img;
    }

    private static void Linea(Transform padre, string nombre, Vector2 a, Vector2 b, float ancho, Color color)
    {
        var img = Caja(padre, nombre, (a + b) / 2, new Vector2(Vector2.Distance(a, b), ancho), color);
        img.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg);
    }

    private static TMP_Text Texto(Transform padre, string nombre, string text, Vector2 pos, Vector2 size, float puntos, Color color, TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        var label = Rect(padre, nombre, pos, size).gameObject.AddComponent<TextMeshProUGUI>();
        label.font = fuente; label.text = text; label.fontSize = puntos;
        label.color = color; label.alignment = align; label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        return label;
    }

    private static Button Boton(Transform padre, string nombre, string text, Vector2 pos, Vector2 size, Color color, bool circle = false)
    {
        var img = Caja(padre, nombre, pos, size, color, !circle, circle); img.raycastTarget = true;
        var boton = img.gameObject.AddComponent<Button>(); boton.targetGraphic = img;
        var colors = boton.colors; colors.selectedColor = new Color(1, .87f, .58f); colors.highlightedColor = new Color(1, .93f, .77f);
        colors.pressedColor = new Color(.75f, .83f, .7f); colors.disabledColor = new Color(.7f, .7f, .7f, .65f); boton.colors = colors;
        Texto(img.transform, "Texto", text, Vector2.zero, size - new Vector2(10, 4), circle ? 40 : 28, circle ? Crema : Tinta);
        return boton;
    }

    private static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out var color); return color; }

    [MenuItem("Tico/Mapa/Validar selector y progreso")]
    public static void Validar()
    {
        var control = Object.FindFirstObjectByType<SelectorMapaNiveles>();
        Exigir(control != null && control.niveles.Length == 5, "Deben existir cinco estaciones.");
        Exigir(control.paradas.Length == 5 && control.recorrido.Length == Ruta.Length, "Recorrido incompleto.");
        for (int i = 0; i < 5; i++)
        {
            Exigir(control.niveles[i] != null && control.estados[i] != null, "Estación sin referencias.");
            Exigir(control.paradas[i] >= 0 && control.paradas[i] < control.recorrido.Length, "Parada fuera de la ruta.");
            if (!string.IsNullOrEmpty(ProgresoNiveles.Escenas[i])) Exigir(EditorBuildSettings.scenes.Any(s => s.enabled && Path.GetFileNameWithoutExtension(s.path) == ProgresoNiveles.Escenas[i]), "Escena jugable ausente del build.");
        }
        Exigir(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length == 1, "Debe haber un EventSystem.");
        bool teniaPaciente = PlayerPrefs.HasKey("PacienteID");
        string paciente = PlayerPrefs.GetString("PacienteID");
        string prueba = "mapa-test-" + Guid.NewGuid();
        try
        {
            PlayerPrefs.SetString("PacienteID", prueba);
            Exigir(ProgresoNiveles.Completados == 0 && ProgresoNiveles.Desbloqueado(1) && !ProgresoNiveles.Desbloqueado(2), "Estado inicial incorrecto.");
            control.Seleccionar(0); Exigir(control.jugar.interactable, "La fase 1 debe permitir jugar.");
            control.Seleccionar(1); Exigir(!control.jugar.interactable && control.descripcion.text.Contains("Completa"), "Fase bloqueada sin explicación.");
            ProgresoNiveles.Completar(3); Exigir(ProgresoNiveles.Completados == 0, "No se deben saltar fases.");
            for (int i = 1; i <= 4; i++) { ProgresoNiveles.Completar(i); Exigir(ProgresoNiveles.Desbloqueado(i + 1), "No desbloquea la siguiente fase."); }
            ProgresoNiveles.Completar(1); Exigir(ProgresoNiveles.Completados == 4, "Repetir no debe borrar progreso.");
            control.Seleccionar(4); Exigir(!control.jugar.interactable && control.textoJugar.text == "Próximamente", "La fase sin escena no debe permitir cargar.");
            control.Jugar();
            Exigir(!ProgresoNiveles.Desbloqueado(0) && !ProgresoNiveles.Desbloqueado(6), "Acepta niveles inexistentes.");
            PlayerPrefs.SetString("PacienteID", prueba + "-otro");
            Exigir(ProgresoNiveles.Completados == 0, "Progreso compartido entre jugadores.");
            control.Seleccionar(0);
        }
        finally
        {
            PlayerPrefs.DeleteKey("Tico.Progreso." + prueba);
            if (teniaPaciente) PlayerPrefs.SetString("PacienteID", paciente); else PlayerPrefs.DeleteKey("PacienteID");
            PlayerPrefs.Save();
        }
        Debug.Log("TICO_TESTS_OK: referencias, escenas, progresión, repetición y aislamiento de jugadores.");
    }

    private static void Exigir(bool condicion, string mensaje) { if (!condicion) throw new Exception(mensaje); }

    private static void Capturar(Camera camera, Canvas canvas)
    {
        var target = new RenderTexture(1600, 900, 24);
        var imagen = new Texture2D(1600, 900, TextureFormat.RGB24, false);
        var anterior = RenderTexture.active;
        try
        {
            camera.targetTexture = target;
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 10;
            Canvas.ForceUpdateCanvases();
            camera.Render(); RenderTexture.active = target;
            imagen.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); imagen.Apply();
            Directory.CreateDirectory("Docs"); File.WriteAllBytes("Docs/mapa-niveles.png", imagen.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = anterior; camera.targetTexture = null;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null;
            Object.DestroyImmediate(imagen); Object.DestroyImmediate(target);
        }
    }
}
