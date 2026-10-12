# Mapa de niveles de Tico

Abre `Assets/Scenes/flujo_Niveles.unity` y pulsa Play. El botón Jugar del menú principal ya conduce a esta escena.

El mapa tiene dos islas, una ruta continua y un puente de madera. Todos los elementos son objetos editables con RectTransform, Image, Button y TextMeshPro; no es una imagen plana. El personaje reutiliza el sprite de Tico del proyecto.

| Fase | Lugar | Juego / escena | Desbloqueo |
| --- | --- | --- | --- |
| 1 | Entrada a la pradera | Burbujas / `nivel1` | Disponible desde el inicio |
| 2 | Sendero del bosque | Laberinto / `Nivel2` | Terminar la fase 1 |
| 3 | Claro de las flores | Burbujas rápidas / `Nivel3` | Terminar la fase 2 |
| 4 | Al otro lado del puente | El vigía / `nivel4` incompleta | Próximamente; se alcanza al terminar la fase 3 |
| 5 | Bandera en la colina | Cima de Tico / pendiente | Próximamente; se alcanza al terminar la fase 4 |

Selecciona una estación para ver su estado. Pulsa **¡Vamos!** para que Tico recorra el sendero hasta el nivel seleccionado. Las estaciones bloqueadas explican qué fase completar antes. Las completadas se pueden repetir. El botón Siguiente de cada juego vuelve al mapa.

El progreso se guarda localmente con PlayerPrefs, separado por PacienteID (o por UserEmail/invitado cuando no hay paciente). Se registra al finalizar cada actividad, sin exigir puntuación mínima, y nunca al abandonarla. No se sincroniza entre dispositivos.

## Edición en Unity

- En `Mapa de Tico > Área segura > Composición 1600 x 900` están las islas, puente, árboles, sendero, estaciones y paneles.
- Para cambiar la ruta, ajusta los puntos de `Puntos del recorrido de Tico` y los segmentos visuales correspondientes. `SelectorMapaNiveles.paradas` contiene los índices de las cinco estaciones.
- El Canvas se adapta a la resolución y al área segura del dispositivo. La composición es horizontal; en vertical se mantiene completa, a menor tamaño.
- **Tico > Mapa > Crear selector de cinco niveles** reconstruye y guarda la escena desde `CrearMapaNiveles.cs`. Reemplaza las modificaciones manuales de esta escena.
- **Tico > Mapa > Validar selector y progreso** comprueba referencias, escenas del build, desbloqueo secuencial, repetición y separación entre jugadores, usando una identidad de prueba temporal.
- Para integrar las fases 4 y 5, termina sus escenas, añádelas a Build Profiles y asigna sus nombres exactos en `ProgresoNiveles.Escenas[3]` y `[4]`. Llama a `ProgresoNiveles.Completar(5)` cuando termine la quinta; la llamada para la cuarta ya está preparada. El selector habilita las entradas automáticamente.

El nivel 4 existente es un borrador: tiene un GameManager de burbujas con referencias incompletas y todavía no usa GameManager_Nivel4. Por eso no se abre desde el mapa. Se corrigió el error de compilación de GameManager_Nivel4: dependía de la clase inexistente SpawnerBosque aunque solo necesitaba activar/desactivar un GameObject. Las métricas se conservan; se añade el registro local de finalización.

Referencias: [Canvas Scaler de Unity UI 2.0](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-CanvasScaler.html) y [UI con Input System 1.18](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.18/manual/UISupport.html).
