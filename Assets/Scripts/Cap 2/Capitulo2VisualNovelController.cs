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

    [Header("Cápsula radial (panel)")]
    [SerializeField] private GameObject capsulaPanel;
    [SerializeField] private Button[] palabrasBotones;
    [SerializeField] private TMP_Text contadorCapsula;
    [SerializeField] private Button confirmarCapsula;
    [SerializeField] private Color colorSeleccion = Color.yellow;

    [Header("Sonido")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambientSource;
    [Header("Al terminar el capítulo")]
    [SerializeField] private string escenaSiguiente = "05_Lluvias";

    private Dictionary<string, Cap2Linea> lineMap;
    private Cap2Linea currentLine;
    private Coroutine glitchRoutine;
    private Vector2 posicionTextoOriginal;

    // Estado de la cápsula radial.
    private Cap2Linea capsulaLinea;
    private bool[] capsulaElegidas;
    private int capsulaPick;

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

        PrepararPanelCapsula();

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

        // Cada vez que habla Fuente 7 suena el glitch (desde el SoundManager).
        if (line.personaje == Cap2Personaje.Fuente7 && SoundManager.Instance != null)
            SoundManager.Instance.PlayGlitch();

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

    // CÁPSULA RADIAL (panel sencillo: elegir, quitar y confirmar)

private void IniciarCapsula(Cap2Linea line)
    {
        if (capsulaPanel == null || palabrasBotones == null || line.palabras == null)
        {
            Debug.LogError("Falta el panel de la cápsula o las palabras de '" + line.id + "'.");
            ShowLine(line.siguienteLinea);
            return;
        }

        capsulaLinea = line;
        capsulaPick = line.palabrasAElegir;
        capsulaElegidas = new bool[palabrasBotones.Length];

        for (int i = 0; i < palabrasBotones.Length; i++)
        {
            bool hayPalabra = i < line.palabras.Length;
            palabrasBotones[i].gameObject.SetActive(hayPalabra);

            if (hayPalabra)
                palabrasBotones[i].GetComponentInChildren<TMP_Text>().text = line.palabras[i].texto;
        }

        OcultarPersonajes();

        if (choicesPanel != null)
            choicesPanel.SetActive(false);

        if (continueButton != null)
            continueButton.gameObject.SetActive(false);

        capsulaPanel.SetActive(true);
        ActualizarCapsula();
    }

private void PrepararPanelCapsula()
    {
        if (capsulaPanel != null)
            capsulaPanel.SetActive(false);

        if (palabrasBotones != null)
        {
            for (int i = 0; i < palabrasBotones.Length; i++)
            {
                int indice = i;
                palabrasBotones[i].onClick.AddListener(() => AlternarPalabra(indice));
            }
        }

        if (confirmarCapsula != null)
            confirmarCapsula.onClick.AddListener(ConfirmarCapsula);
    }

    // Elegir o quitar una palabra, hasta el máximo permitido.
    private void AlternarPalabra(int i)
    {
        if (capsulaLinea == null)
            return;

        if (capsulaElegidas[i])
            capsulaElegidas[i] = false;
        else if (CantidadElegidas() < capsulaPick)
            capsulaElegidas[i] = true;

        ActualizarCapsula();
    }

    private int CantidadElegidas()
    {
        int total = 0;

        foreach (bool elegida in capsulaElegidas)
        {
            if (elegida)
                total++;
        }

        return total;
    }

    private void ActualizarCapsula()
    {
        for (int i = 0; i < palabrasBotones.Length; i++)
            palabrasBotones[i].image.color = capsulaElegidas[i] ? colorSeleccion : Color.white;

        int elegidas = CantidadElegidas();

        if (contadorCapsula != null)
            contadorCapsula.text = elegidas + "/" + capsulaPick + " seleccionadas";

        if (confirmarCapsula != null)
            confirmarCapsula.interactable = elegidas == capsulaPick;
    }

private void ConfirmarCapsula()
    {
        if (capsulaLinea == null || CantidadElegidas() != capsulaPick)
            return;

        for (int i = 0; i < capsulaElegidas.Length; i++)
        {
            if (!capsulaElegidas[i])
                continue;

            Cap2Palabra palabra = capsulaLinea.palabras[i];

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
        }

        GameSaveSystem.SaveGameState();
        capsulaPanel.SetActive(false);

        string siguiente = capsulaLinea.siguienteLinea;
        capsulaLinea = null;
        ShowLine(siguiente);
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
