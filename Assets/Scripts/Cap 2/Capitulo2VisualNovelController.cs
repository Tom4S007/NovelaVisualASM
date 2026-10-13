using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Capitulo2VisualNovelController : MonoBehaviour
{
    [Header("Guion")]
    [SerializeField] private string firstLineId = "c2_01";
    [SerializeField] private Cap2Linea[] lines;

    [Header("Textos")]
    [SerializeField] private TMP_Text speakerName;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Imágenes")]
    [SerializeField] private Image background;
    [SerializeField] private Image characterLeft;
    [SerializeField] private Image characterRight;

    [Header("Botones")]
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject choicesPanel;
    [Tooltip("Botones del panel de opciones (de 2 a 5). Sirven para las decisiones y para la cápsula.")]
    [SerializeField] private Button[] opcionesBotones;

    [Header("Sonido")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioClip glitchSound;

    [Header("Al terminar el capítulo")]
    [SerializeField] private string escenaSiguiente = "05_Lluvias";

    private Dictionary<string, Cap2Linea> lineMap;
    private Cap2Linea currentLine;
    private Coroutine glitchRoutine;
    private Vector2 posicionTextoOriginal;

    // Estado de la cápsula radial.
    private Cap2Linea capsulaLinea;
    private List<int> capsulaDisponibles;
    private int capsulaRonda;
    private int capsulaPick;
    private bool esperandoReaccion;

    private void Start()
    {
        // Recupera lo decidido en capítulos anteriores. No reinicia la partida.
        GameSaveSystem.LoadGameState();

        lineMap = new Dictionary<string, Cap2Linea>();

        foreach (Cap2Linea line in lines)
        {
            if (line == null || string.IsNullOrWhiteSpace(line.id))
                continue;

            if (lineMap.ContainsKey(line.id))
            {
                Debug.LogError("ID de diálogo duplicado: " + line.id);
                continue;
            }

            lineMap.Add(line.id, line);
        }

        if (dialogueText != null)
            posicionTextoOriginal = dialogueText.rectTransform.anchoredPosition;

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueDialogue);

        if (opcionesBotones != null)
        {
            for (int i = 0; i < opcionesBotones.Length; i++)
            {
                if (opcionesBotones[i] == null)
                    continue;

                int indice = i;
                opcionesBotones[i].onClick.AddListener(() => SelectChoice(indice));
            }
        }

        if (choicesPanel != null)
            choicesPanel.SetActive(false);

        OcultarPersonajes();

        if (continueButton != null)
            continueButton.gameObject.SetActive(true);

        ShowLine(firstLineId);
    }

    private void ShowLine(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            IrASiguienteEscena();
            return;
        }

        Cap2Linea line;

        if (!lineMap.TryGetValue(id, out line))
        {
            Debug.LogError("No existe la línea: " + id);
            return;
        }

        currentLine = line;

        // Las líneas de salto son invisibles: solo deciden a dónde ir.
        if (line.esSalto)
        {
            bool cumple = Evaluar(line.condicion);

            if (line.invertir)
                cumple = !cumple;

            ShowLine(cumple ? line.siguienteSiCumple : line.siguienteLinea);
            return;
        }

        if (background != null && line.fondo != null)
            background.sprite = line.fondo;

        ReproducirSonidos(line);

        if (line.glitch > 0)
        {
            if (glitchRoutine != null)
                StopCoroutine(glitchRoutine);

            glitchRoutine = StartCoroutine(EfectoGlitch(line.glitch));
        }

        if (line.esCapsula)
        {
            IniciarCapsula(line);
            return;
        }

        if (speakerName != null)
            speakerName.text = NombreDe(line);

        if (dialogueText != null)
            dialogueText.text = ReemplazarVariables(line.texto);

        MostrarPersonaje(line);

        bool conOpciones =
            line.tieneOpciones &&
            line.opciones != null &&
            line.opciones.Length >= 2;

        if (line.tieneOpciones && !conOpciones)
            Debug.LogError("La decisión '" + line.id + "' necesita al menos 2 opciones.");

        if (choicesPanel != null)
            choicesPanel.SetActive(conOpciones);

        if (continueButton != null)
            continueButton.gameObject.SetActive(!conOpciones);

        if (conOpciones)
        {
            string[] textos = new string[line.opciones.Length];

            for (int i = 0; i < textos.Length; i++)
                textos[i] = ReemplazarVariables(line.opciones[i].texto);

            MostrarBotones(textos);
        }
    }

    private void ContinueDialogue()
    {
        // Durante la cápsula, el botón solo sirve para pasar la reacción.
        if (capsulaLinea != null)
        {
            if (esperandoReaccion)
                AvanzarCapsula();

            return;
        }

        if (currentLine == null)
            return;

        bool conOpciones =
            currentLine.tieneOpciones &&
            currentLine.opciones != null &&
            currentLine.opciones.Length >= 2;

        if (conOpciones)
            return;

        ShowLine(currentLine.siguienteLinea);
    }

    // BOTONES DE OPCIONES

    private void MostrarBotones(string[] textos)
    {
        if (opcionesBotones == null)
            return;

        if (textos.Length > opcionesBotones.Length)
            Debug.LogWarning("Hay más opciones que botones: sobran " +
                             (textos.Length - opcionesBotones.Length));

        for (int i = 0; i < opcionesBotones.Length; i++)
        {
            if (opcionesBotones[i] == null)
                continue;

            bool activo = i < textos.Length;
            opcionesBotones[i].gameObject.SetActive(activo);

            if (!activo)
                continue;

            TMP_Text etiqueta = opcionesBotones[i].GetComponentInChildren<TMP_Text>();

            if (etiqueta != null)
                etiqueta.text = textos[i];
        }
    }

    private void SelectChoice(int index)
    {
        // Si hay una cápsula en marcha, el botón elige una palabra.
        if (capsulaLinea != null)
        {
            ElegirPalabra(index);
            return;
        }

        if (currentLine == null ||
            currentLine.opciones == null ||
            index < 0 ||
            index >= currentLine.opciones.Length)
            return;

        Cap2Opcion op = currentLine.opciones[index];

        GameState.Sumar(ref GameState.PREP, op.prep);
        GameState.Sumar(ref GameState.INFO, op.info);
        GameState.Sumar(ref GameState.CONF, op.conf);
        GameState.Sumar(ref GameState.RIESGO, op.riesgo);

        SumarConfianza(ref GameState.cSebas, op.cSebas);
        SumarConfianza(ref GameState.cCamila, op.cCamila);
        SumarConfianza(ref GameState.cChela, op.cChela);

        if (op.rumorEmitido)
            GameState.rumorEmitido = true;

        if (op.subioAApagar)
            GameState.subioAApagar = true;

        if (op.panico)
            GameState.panico = true;

        if (op.arriesgo)
            GameState.arriesgo = true;

        GameSaveSystem.SaveGameState();

        Debug.Log(
            $"Elección: {op.texto} | PREP={GameState.PREP}, INFO={GameState.INFO}, " +
            $"CONF={GameState.CONF}, RIESGO={GameState.RIESGO} | " +
            $"cSebas={GameState.cSebas}, cCamila={GameState.cCamila}, cChela={GameState.cChela}"
        );

        ShowLine(op.siguienteLinea);
    }

    // CÁPSULA RADIAL (las palabras salen como opciones, de una en una)

    private void IniciarCapsula(Cap2Linea line)
    {
        if (line.palabras == null || line.palabras.Length < 2)
        {
            Debug.LogError("La cápsula '" + line.id + "' necesita al menos 2 palabras.");
            ShowLine(line.siguienteLinea);
            return;
        }

        capsulaLinea = line;
        capsulaPick = Mathf.Clamp(line.palabrasAElegir, 1, line.palabras.Length);
        capsulaRonda = 0;
        esperandoReaccion = false;

        capsulaDisponibles = new List<int>();

        for (int i = 0; i < line.palabras.Length; i++)
            capsulaDisponibles.Add(i);

        // Mezclar para que las correctas no salgan siempre primero.
        for (int i = capsulaDisponibles.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = capsulaDisponibles[i];
            capsulaDisponibles[i] = capsulaDisponibles[j];
            capsulaDisponibles[j] = tmp;
        }

        MostrarRondaCapsula();
    }

    private void MostrarRondaCapsula()
    {
        esperandoReaccion = false;
        OcultarPersonajes();

        if (speakerName != null)
            speakerName.text = NombreDe(capsulaLinea);

        if (dialogueText != null)
            dialogueText.text =
                ReemplazarVariables(capsulaLinea.texto) +
                "\n(" + (capsulaRonda + 1) + " de " + capsulaPick + ")";

        if (continueButton != null)
            continueButton.gameObject.SetActive(false);

        if (choicesPanel != null)
            choicesPanel.SetActive(true);

        string[] textos = new string[capsulaDisponibles.Count];

        for (int i = 0; i < textos.Length; i++)
            textos[i] = capsulaLinea.palabras[capsulaDisponibles[i]].texto;

        MostrarBotones(textos);
    }

    private void ElegirPalabra(int boton)
    {
        if (boton < 0 || boton >= capsulaDisponibles.Count)
            return;

        int indice = capsulaDisponibles[boton];
        Cap2Palabra palabra = capsulaLinea.palabras[indice];

        capsulaDisponibles.RemoveAt(boton);
        capsulaRonda++;

        if (palabra.correcta)
            GameState.Sumar(ref GameState.PREP, 1);
        else
            GameState.Sumar(ref GameState.RIESGO, 1);

        switch (palabra.leGustaA)
        {
            case Cap2Personaje.Sebas:
                SumarConfianza(ref GameState.cSebas, 1);
                break;

            case Cap2Personaje.Camila:
                SumarConfianza(ref GameState.cCamila, 1);
                break;

            case Cap2Personaje.Chela:
                SumarConfianza(ref GameState.cChela, 1);
                break;
        }

        GameSaveSystem.SaveGameState();

        Debug.Log(
            $"Cápsula: {palabra.texto} | PREP={GameState.PREP}, RIESGO={GameState.RIESGO}, " +
            $"cSebas={GameState.cSebas}, cCamila={GameState.cCamila}, cChela={GameState.cChela}"
        );

        if (!string.IsNullOrWhiteSpace(palabra.reaccion))
        {
            MostrarReaccion(palabra);
            return;
        }

        AvanzarCapsula();
    }

    private void MostrarReaccion(Cap2Palabra palabra)
    {
        esperandoReaccion = true;
        OcultarPersonajes();

        if (speakerName != null)
            speakerName.text = NombreDePersonaje(palabra.leGustaA);

        if (dialogueText != null)
            dialogueText.text = ReemplazarVariables(palabra.reaccion);

        if (choicesPanel != null)
            choicesPanel.SetActive(false);

        if (continueButton != null)
            continueButton.gameObject.SetActive(true);
    }

    private void AvanzarCapsula()
    {
        esperandoReaccion = false;

        if (capsulaRonda >= capsulaPick || capsulaDisponibles.Count == 0)
        {
            string siguiente = capsulaLinea.siguienteLinea;
            capsulaLinea = null;
            ShowLine(siguiente);
            return;
        }

        MostrarRondaCapsula();
    }

    // CONDICIONES Y TEXTOS

    private void SumarConfianza(ref int valor, int cantidad)
    {
        valor = Mathf.Clamp(valor + cantidad, 0, 3);
    }

    private bool Evaluar(Cap2Condicion condicion)
    {
        switch (condicion)
        {
            case Cap2Condicion.RumorEmitido:
                return GameState.rumorEmitido;

            case Cap2Condicion.FranjaLimpia:
                return GameState.franjaLimpia;

            case Cap2Condicion.SubioAApagar:
                return GameState.subioAApagar;

            case Cap2Condicion.Panico:
                return GameState.panico;

            case Cap2Condicion.ReservaAgua:
                return GameState.reservaAgua;

            case Cap2Condicion.TieneLibreta:
                return GameState.tieneLibreta;

            case Cap2Condicion.Arriesgo:
                return GameState.arriesgo;

            // En caso de empate gana Sebas.
            case Cap2Condicion.SebasTieneMasConfianza:
                return GameState.cSebas >= GameState.cCamila &&
                       GameState.cSebas >= GameState.cChela;

            case Cap2Condicion.CamilaTieneMasConfianza:
                return GameState.cCamila > GameState.cSebas &&
                       GameState.cCamila >= GameState.cChela;

            case Cap2Condicion.ChelaTieneMasConfianza:
                return GameState.cChela > GameState.cSebas &&
                       GameState.cChela > GameState.cCamila;

            default:
                return false;
        }
    }

    private string NombreDe(Cap2Linea line)
    {
        if (!string.IsNullOrWhiteSpace(line.nombreVisible))
            return ReemplazarVariables(line.nombreVisible);

        return NombreDePersonaje(line.personaje);
    }

    private string NombreDePersonaje(Cap2Personaje personaje)
    {
        switch (personaje)
        {
            case Cap2Personaje.Lucia: return GameState.nombreJugador;
            case Cap2Personaje.Sebas: return "Sebas";
            case Cap2Personaje.Camila: return "Camila";
            case Cap2Personaje.Chela: return "Doña Chela";
            case Cap2Personaje.DonHernan: return "Don Hernán";
            case Cap2Personaje.Fuente7: return "Fuente 7";
            case Cap2Personaje.VozDelRio: return "Voz del Río";
            default: return "Narrador";
        }
    }

    private string ReemplazarVariables(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return texto;

        return texto.Replace("{nombre}", GameState.nombreJugador);
    }

    // PERSONAJES, SONIDO Y GLITCH

    private void MostrarPersonaje(Cap2Linea line)
    {
        OcultarPersonajes();

        if (line.expresion == null)
            return;

        Image destino = null;

        switch (line.personaje)
        {
            case Cap2Personaje.Lucia:
            case Cap2Personaje.Chela:
            case Cap2Personaje.DonHernan:
                destino = characterLeft;
                break;

            case Cap2Personaje.Sebas:
            case Cap2Personaje.Camila:
                destino = characterRight;
                break;
        }

        if (destino == null)
            return;

        destino.sprite = line.expresion;
        destino.preserveAspect = true;
        destino.gameObject.SetActive(true);
    }

    private void OcultarPersonajes()
    {
        if (characterLeft != null)
            characterLeft.gameObject.SetActive(false);

        if (characterRight != null)
            characterRight.gameObject.SetActive(false);
    }

    private void ReproducirSonidos(Cap2Linea line)
    {
        if (line.sonido != null && sfxSource != null)
            sfxSource.PlayOneShot(line.sonido);

        if (line.ambiente != null && ambientSource != null &&
            ambientSource.clip != line.ambiente)
        {
            ambientSource.clip = line.ambiente;
            ambientSource.loop = true;
            ambientSource.Play();
        }
    }

    // Glitch de Fuente 7: el texto tiembla y el nombre parpadea.
    private IEnumerator EfectoGlitch(int nivel)
    {
        if (glitchSound != null && sfxSource != null)
            sfxSource.PlayOneShot(glitchSound);

        RectTransform caja =
            dialogueText != null ? dialogueText.rectTransform : null;

        float duracion = 0.35f * nivel;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            if (caja != null)
            {
                Vector2 desvio = new Vector2(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f)) * 4f * nivel;

                caja.anchoredPosition = posicionTextoOriginal + desvio;
            }

            if (speakerName != null)
                speakerName.alpha = Random.value > 0.5f ? 1f : 0.2f;

            tiempo += Time.deltaTime;
            yield return null;
        }

        if (caja != null)
            caja.anchoredPosition = posicionTextoOriginal;

        if (speakerName != null)
            speakerName.alpha = 1f;

        glitchRoutine = null;
    }

    private void IrASiguienteEscena()
    {
        GameSaveSystem.SaveGameState();

        if (string.IsNullOrWhiteSpace(escenaSiguiente) ||
            !Application.CanStreamedLevelBeLoaded(escenaSiguiente))
        {
            Debug.LogError(
                "No se puede cargar la escena '" + escenaSiguiente +
                "'. Comprueba el nombre y que esté en el Build Settings.");
            return;
        }

        SceneManager.LoadScene(escenaSiguiente);
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(ContinueDialogue);

        if (opcionesBotones == null)
            return;

        foreach (Button boton in opcionesBotones)
        {
            if (boton != null)
                boton.onClick.RemoveAllListeners();
        }
    }
}
