using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CharacterPortrait
{
    public CharacterId character;
    public Sprite defaultSprite;
}

public class VisualNovelController : MonoBehaviour
{
    // =========================================================
    // TEXTOS
    // =========================================================

    [Header("Textos")]
    [SerializeField] private TMP_Text speakerName;
    [SerializeField] private TMP_Text dialogueText;


    // =========================================================
    // PERSONAJES
    // =========================================================

    [Header("Personajes")]
    [SerializeField] private Image characterLeft;
    [SerializeField] private Image characterRight;


    // =========================================================
    // BOTONES
    // =========================================================

    [Header("Botones")]
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private Button choiceA;
    [SerializeField] private Button choiceB;


    // =========================================================
    // GUION
    // =========================================================

    [Header("Guion")]
    [SerializeField] private DialogueLine[] lines;
    [SerializeField] private string firstLineId = "p1";


    // =========================================================
    // EXTRAS CAP 3
    // =========================================================

    [Header("Extras Cap 3")]
    [SerializeField] private Image background;
    [SerializeField] private string nextSceneName;
    [SerializeField] private CapsuleView capsuleView;


    // =========================================================
    // RETRATOS POR DEFECTO
    // =========================================================

    [Header("Retratos por defecto")]
    [SerializeField] private CharacterPortrait[] portraits;


    // =========================================================
    // FINAL DE LA ESCENA
    // =========================================================

    [Header("Final de la escena")]
    [SerializeField] private string nombreEscenaMapa = "01_MapaCali";

    [SerializeField]
    private string mensajeFinal =
        "La historia continúa en Cali. Vamos al mapa para explorar los capítulos y descubrir qué sucede en cada lugar.";

    [SerializeField] private string textoBotonMapa = "IR AL MAPA";


    // =========================================================
    // VARIABLES INTERNAS
    // =========================================================

    private Dictionary<string, DialogueLine> lineMap;

    private DialogueLine currentLine;

    private bool mostrandoFinal;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        lineMap = new Dictionary<string, DialogueLine>();

        foreach (DialogueLine line in lines)
        {
            if (line != null &&
                !string.IsNullOrWhiteSpace(line.id))
            {
                if (lineMap.ContainsKey(line.id))
                {
                    Debug.LogError(
                        "ID de diálogo duplicado: " + line.id
                    );

                    continue;
                }

                lineMap.Add(line.id, line);
            }
        }


        // -----------------------------------------
        // BOTÓN CONTINUAR
        // -----------------------------------------

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueDialogue);


        // -----------------------------------------
        // BOTONES DE DECISIÓN
        // -----------------------------------------

        if (choiceA != null)
            choiceA.onClick.AddListener(() => SelectChoice(0));

        if (choiceB != null)
            choiceB.onClick.AddListener(() => SelectChoice(1));


        // -----------------------------------------
        // ESTADO INICIAL
        // -----------------------------------------

        if (choicesPanel != null)
            choicesPanel.SetActive(false);

        if (characterLeft != null)
            characterLeft.gameObject.SetActive(false);

        if (characterRight != null)
            characterRight.gameObject.SetActive(false);

        if (continueButton != null)
            continueButton.gameObject.SetActive(true);


        // -----------------------------------------
        // PRIMERA LÍNEA
        // -----------------------------------------

        ShowLine(firstLineId);
    }


    // =========================================================
    // MOSTRAR LÍNEA
    // =========================================================

    private void ShowLine(string id)
    {
        // Si no hay otra línea,
        // mostrar la invitación a ir al mapa.

        if (string.IsNullOrWhiteSpace(id))
        {
            MostrarFinalEscena();
            return;
        }


        // Buscar línea

        if (!lineMap.TryGetValue(id, out currentLine))
        {
            Debug.LogError(
                "No existe la línea: [" + id + "]"
            );

            return;
        }


        // -----------------------------------------
        // LÍNEAS ROUTER
        // -----------------------------------------

        // Las líneas router son invisibles
        // y solo evalúan condiciones.

        if (currentLine.isRouter)
        {
            string destino = EvaluarCondicion(currentLine)
                ? currentLine.nextLineIfTrue
                : currentLine.nextLineId;

            ShowLine(destino);

            return;
        }


        // -----------------------------------------
        // MODIFICAR RIESGO
        // -----------------------------------------

        GameState.Sumar(
            ref GameState.RIESGO,
            currentLine.onEnterRiesgo
        );


        // -----------------------------------------
        // CAMBIAR FONDO
        // -----------------------------------------

        if (background != null &&
            currentLine.background != null)
        {
            background.sprite = currentLine.background;
        }


        // -----------------------------------------
        // CAPSULA
        // -----------------------------------------

        if (currentLine.isCapsule &&
            capsuleView != null)
        {
            if (speakerName != null)
            {
                speakerName.text =
                    GetSpeakerName(currentLine.character);
            }

            if (dialogueText != null)
            {
                dialogueText.text =
                    currentLine.text;
            }

            if (choicesPanel != null)
                choicesPanel.SetActive(false);

            if (continueButton != null)
                continueButton.gameObject.SetActive(false);


            string siguiente =
                currentLine.nextLineId;


            capsuleView.Show(
                currentLine.capsuleWords,
                currentLine.capsulePick,
                () => ShowLine(siguiente)
            );

            return;
        }


        // -----------------------------------------
        // NOMBRE DEL PERSONAJE
        // -----------------------------------------

        if (speakerName != null)
        {
            speakerName.text =
                GetSpeakerName(currentLine.character);
        }


        // -----------------------------------------
        // DIÁLOGO
        // -----------------------------------------

        if (dialogueText != null)
        {
            dialogueText.text =
                currentLine.text;
        }


        // -----------------------------------------
        // PERSONAJE
        // -----------------------------------------

        ShowCharacter(currentLine);


        // -----------------------------------------
        // DECISIONES
        // -----------------------------------------

        if (choicesPanel != null)
        {
            choicesPanel.SetActive(
                currentLine.hasChoices
            );
        }


        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(
                !currentLine.hasChoices
            );
        }


        if (currentLine.hasChoices)
        {
            ConfigureChoices();
        }
    }


    // =========================================================
    // FINAL DE ESCENA
    // =========================================================

    private void MostrarFinalEscena()
    {
        mostrandoFinal = true;

        currentLine = null;


        // -----------------------------------------
        // NARRADOR
        // -----------------------------------------

        if (speakerName != null)
            speakerName.text = "Narrador";


        // -----------------------------------------
        // MENSAJE FINAL
        // -----------------------------------------

        if (dialogueText != null)
            dialogueText.text = mensajeFinal;


        // -----------------------------------------
        // OCULTAR DECISIONES
        // -----------------------------------------

        if (choicesPanel != null)
            choicesPanel.SetActive(false);


        // -----------------------------------------
        // OCULTAR PERSONAJES
        // -----------------------------------------

        if (characterLeft != null)
            characterLeft.gameObject.SetActive(false);

        if (characterRight != null)
            characterRight.gameObject.SetActive(false);


        // -----------------------------------------
        // BOTÓN MAPA
        // -----------------------------------------

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);


            TMP_Text etiqueta =
                continueButton.GetComponentInChildren<TMP_Text>();


            if (etiqueta != null)
            {
                etiqueta.text =
                    textoBotonMapa;
            }
        }


        Debug.Log(
            "Secuencia narrativa terminada. " +
            "Esperando para ir al mapa."
        );
    }


    // =========================================================
    // NOMBRE DEL PERSONAJE
    // =========================================================

    private string GetSpeakerName(CharacterId character)
    {
        switch (character)
        {
            case CharacterId.Lucia:

                return GameState.nombreJugador;


            case CharacterId.Sebas:

                return "Sebas";


            case CharacterId.Camila:

                return "Camila";


            case CharacterId.Chela:

                return "Doña Chela (teléfono)";


            case CharacterId.DonHernan:

                return "Don Hernán";


            case CharacterId.Fuente7:

                return "Fuente 7";


            default:

                return "Narrador";
        }
    }


    // =========================================================
    // MOSTRAR PERSONAJE
    // =========================================================

    private void ShowCharacter(DialogueLine line)
    {
        // -----------------------------------------
        // OCULTAR PERSONAJES ANTERIORES
        // -----------------------------------------

        if (characterLeft != null)
            characterLeft.gameObject.SetActive(false);

        if (characterRight != null)
            characterRight.gameObject.SetActive(false);


        // -----------------------------------------
        // OBTENER SPRITE
        // -----------------------------------------

        Sprite sprite =
            line.expression != null
                ? line.expression
                : GetDefaultPortrait(line.character);


        if (sprite == null)
            return;


        // -----------------------------------------
        // DETERMINAR POSICIÓN
        // -----------------------------------------

        Image target = null;


        switch (line.character)
        {
            case CharacterId.Lucia:

            case CharacterId.Chela:

            case CharacterId.DonHernan:

                target = characterLeft;

                break;


            case CharacterId.Sebas:

            case CharacterId.Camila:

                target = characterRight;

                break;
        }


        // -----------------------------------------
        // MOSTRAR SPRITE
        // -----------------------------------------

        if (target != null)
        {
            target.sprite = sprite;

            target.preserveAspect = true;

            target.gameObject.SetActive(true);
        }
    }


    // =========================================================
    // RETRATO POR DEFECTO
    // =========================================================

    private Sprite GetDefaultPortrait(
        CharacterId character)
    {
        if (portraits == null)
            return null;


        foreach (CharacterPortrait p in portraits)
        {
            if (p.character == character)
                return p.defaultSprite;
        }


        return null;
    }


    // =========================================================
    // CONTINUAR DIÁLOGO
    // =========================================================

    private void ContinueDialogue()
    {
        // -----------------------------------------
        // BOTÓN FINAL → MAPA
        // -----------------------------------------

        if (mostrandoFinal)
        {
            IrAlMapa();

            return;
        }


        // -----------------------------------------
        // VALIDAR
        // -----------------------------------------

        if (currentLine == null ||
            currentLine.hasChoices)
        {
            return;
        }


        // -----------------------------------------
        // SIGUIENTE LÍNEA
        // -----------------------------------------

        ShowLine(
            currentLine.nextLineId
        );
    }


    // =========================================================
    // IR AL MAPA
    // =========================================================

    private void IrAlMapa()
    {
        // Si hay un nombre de escena configurado
        // en el Inspector, se usa ese.
        // De lo contrario, se usa el mapa de Cali.

        string escenaDestino =
            !string.IsNullOrWhiteSpace(nextSceneName)
                ? nextSceneName
                : nombreEscenaMapa;


        if (Application.CanStreamedLevelBeLoaded(
            escenaDestino))
        {
            SceneManager.LoadScene(
                escenaDestino
            );
        }
        else
        {
            Debug.LogError(
                "No se puede cargar la escena '" +
                escenaDestino +
                "'. Comprueba su nombre y que esté " +
                "incluida en Build Profiles."
            );
        }
    }


    // =========================================================
    // CONFIGURAR DECISIONES
    // =========================================================

    private void ConfigureChoices()
    {
        if (currentLine.choices == null ||
            currentLine.choices.Length < 2)
        {
            Debug.LogError(
                "La decisión necesita al menos dos opciones."
            );


            if (choicesPanel != null)
                choicesPanel.SetActive(false);


            if (continueButton != null)
                continueButton.gameObject.SetActive(true);


            return;
        }


        ConfigurarBoton(
            choiceA,
            currentLine.choices[0]
        );


        ConfigurarBoton(
            choiceB,
            currentLine.choices[1]
        );
    }


    // =========================================================
    // TEXTO DE BOTÓN
    // =========================================================

    private void SetButtonText(
        Button button,
        string text)
    {
        if (button == null)
            return;


        TMP_Text label =
            button.GetComponentInChildren<TMP_Text>();


        if (label != null)
            label.text = text;
    }


    // =========================================================
    // SELECCIONAR DECISIÓN
    // =========================================================

    private void SelectChoice(int index)
    {
        if (currentLine == null ||
            currentLine.choices == null ||
            index < 0 ||
            index >= currentLine.choices.Length)
        {
            return;
        }


        ChoiceData choice =
            currentLine.choices[index];


        // -----------------------------------------
        // ESTADÍSTICAS
        // -----------------------------------------

        GameState.Sumar(
            ref GameState.PREP,
            choice.prep
        );


        GameState.Sumar(
            ref GameState.INFO,
            choice.info
        );


        GameState.Sumar(
            ref GameState.CONF,
            choice.conf
        );


        GameState.Sumar(
            ref GameState.RIESGO,
            choice.riesgo
        );


        // -----------------------------------------
        // AFINIDADES
        // -----------------------------------------

        GameState.Sumar(
            ref GameState.cSebas,
            choice.cSebas
        );


        GameState.Sumar(
            ref GameState.cCamila,
            choice.cCamila
        );


        GameState.Sumar(
            ref GameState.cChela,
            choice.cChela
        );


        // -----------------------------------------
        // ESTADOS
        // -----------------------------------------

        if (choice.emitRumor)
            GameState.rumorEmitido = true;


        if (choice.setReservaAgua)
            GameState.reservaAgua = true;


        if (choice.setTieneLibreta)
            GameState.tieneLibreta = true;


        if (choice.setFranjaLimpia)
            GameState.franjaLimpia = true;


        if (choice.setSubioAApagar)
            GameState.subioAApagar = true;


        if (choice.setPanico)
            GameState.panico = true;


        if (choice.setArriesgo)
            GameState.arriesgo = true;


        // -----------------------------------------
        // GUARDAR PARTIDA
        // -----------------------------------------

        GameSaveSystem.SaveGameState();


        // -----------------------------------------
        // DEBUG
        // -----------------------------------------

        Debug.Log(
            $"Elección: {choice.text} | " +
            $"PREP={GameState.PREP}, " +
            $"INFO={GameState.INFO}, " +
            $"CONF={GameState.CONF}, " +
            $"RIESGO={GameState.RIESGO} | " +
            $"arriesgo={GameState.arriesgo}, " +
            $"panico={GameState.panico}, " +
            $"cSebas={GameState.cSebas}, " +
            $"cCamila={GameState.cCamila}, " +
            $"cChela={GameState.cChela}"
        );


        // -----------------------------------------
        // SIGUIENTE LÍNEA
        // -----------------------------------------

        ShowLine(
            choice.nextLineId
        );
    }


    // =========================================================
    // CONFIGURAR BOTÓN DE DECISIÓN
    // =========================================================

    private void ConfigurarBoton(
        Button boton,
        ChoiceData opcion)
    {
        if (boton == null ||
            opcion == null)
        {
            return;
        }


        bool bloqueada =
            opcion.requiresLibreta &&
            !GameState.tieneLibreta;


        string textoBoton =
            bloqueada &&
            !string.IsNullOrEmpty(
                opcion.lockedHint
            )
                ? opcion.lockedHint
                : opcion.text;


        SetButtonText(
            boton,
            textoBoton
        );


        boton.interactable =
            !bloqueada;
    }


    // =========================================================
    // EVALUAR CONDICIONES
    // =========================================================

    private bool EvaluarCondicion(
        DialogueLine linea)
    {
        switch (linea.condition)
        {
            case RouterCondition.PrepMayorOIgual:

                return GameState.PREP >=
                       linea.conditionValue;


            case RouterCondition.Panico:

                return GameState.panico;


            case RouterCondition.TieneLibreta:

                return GameState.tieneLibreta;


            case RouterCondition.SinReservaAgua:

                return !GameState.reservaAgua;


            case RouterCondition.Arriesgo:

                return GameState.arriesgo;


            case RouterCondition.IRMayorOIgual:

                return CalcularIR() >=
                       linea.conditionValue;


            case RouterCondition.IRMenorQue:

                return CalcularIR() <
                       linea.conditionValue;


            case RouterCondition.ArriesgoYRiesgoAlto:

                return GameState.arriesgo &&
                       GameState.RIESGO >=
                       linea.conditionValue;


            case RouterCondition.RumorEmitido:

                return GameState.rumorEmitido;


            case RouterCondition.FranjaLimpia:

                return GameState.franjaLimpia;


            case RouterCondition.SubioAApagar:

                return GameState.subioAApagar;


            case RouterCondition.ReservaAgua:

                return GameState.reservaAgua;


            default:

                return false;
        }
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(
                ContinueDialogue
            );
        }


        if (choiceA != null)
        {
            choiceA.onClick.RemoveAllListeners();
        }


        if (choiceB != null)
        {
            choiceB.onClick.RemoveAllListeners();
        }
    }


    // =========================================================
    // CALCULAR IR
    // =========================================================

    private int CalcularIR()
    {
        return GameState.PREP +
               GameState.INFO +
               GameState.CONF -
               GameState.RIESGO;
    }
}