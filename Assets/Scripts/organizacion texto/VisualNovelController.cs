
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class VisualNovelController : MonoBehaviour
{
    [Header("Textos")]
    [SerializeField] private TMP_Text speakerName;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Personajes")]
    [SerializeField] private Image characterLeft;
    [SerializeField] private Image characterRight;

    [Header("Botones")]
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private Button choiceA;
    [SerializeField] private Button choiceB;

    [Header("Guion")]
    [SerializeField] private DialogueLine[] lines;
    [SerializeField] private string firstLineId = "p1";

    [Header("Extras Cap 3")]
    [SerializeField] private Image background;
    [SerializeField] private string nextSceneName;
    [SerializeField] private CapsuleView capsuleView;

    [Header("Final de la escena")]
    [SerializeField] private string nombreEscenaMapa = "01_MapaCali";
    [SerializeField]
    private string mensajeFinal =
        "La historia continúa en Cali. Vamos al mapa para explorar los capítulos y descubrir qué sucede en cada lugar.";
    [SerializeField] private string textoBotonMapa = "IR AL MAPA";

    private Dictionary<string, DialogueLine> lineMap;
    private DialogueLine currentLine;
    private bool mostrandoFinal;

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
                        "ID de diálogo duplicado: " + line.id);
                    continue;
                }

                lineMap.Add(line.id, line);
            }
        }

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueDialogue);

        if (choiceA != null)
            choiceA.onClick.AddListener(() => SelectChoice(0));

        if (choiceB != null)
            choiceB.onClick.AddListener(() => SelectChoice(1));

        if (choicesPanel != null)
            choicesPanel.SetActive(false);

        if (characterLeft != null)
            characterLeft.gameObject.SetActive(false);

        if (characterRight != null)
            characterRight.gameObject.SetActive(false);

        if (continueButton != null)
            continueButton.gameObject.SetActive(true);

        ShowLine(firstLineId);
    }

    private void ShowLine(string id)
    {
        // Si no hay otra línea, mostrar la invitación a ir al mapa.
        if (string.IsNullOrWhiteSpace(id))
        {
            MostrarFinalEscena();
            return;
        }

        if (!lineMap.TryGetValue(id, out currentLine))
        {
            Debug.LogError("No existe la línea: " + id);
            return;
        }

        // Las líneas router son invisibles y solo evalúan condiciones.
        if (currentLine.isRouter)
        {
            string destino = EvaluarCondicion(currentLine)
                ? currentLine.nextLineIfTrue
                : currentLine.nextLineId;

            ShowLine(destino);
            return;
        }

        GameState.Sumar(
            ref GameState.RIESGO,
            currentLine.onEnterRiesgo
        );

        if (background != null && currentLine.background != null)
            background.sprite = currentLine.background;

        if (currentLine.isCapsule && capsuleView != null)
        {
            if (speakerName != null)
                speakerName.text = GetSpeakerName(currentLine.character);

            if (dialogueText != null)
                dialogueText.text = currentLine.text;

            if (choicesPanel != null)
                choicesPanel.SetActive(false);

            if (continueButton != null)
                continueButton.gameObject.SetActive(false);

            string siguiente = currentLine.nextLineId;

            capsuleView.Show(
                currentLine.capsuleWords,
                currentLine.capsulePick,
                () => ShowLine(siguiente)
            );

            return;
        }

        if (speakerName != null)
            speakerName.text = GetSpeakerName(currentLine.character);

        if (dialogueText != null)
            dialogueText.text = currentLine.text;

        ShowCharacter(currentLine);

        if (choicesPanel != null)
            choicesPanel.SetActive(currentLine.hasChoices);

        if (continueButton != null)
            continueButton.gameObject.SetActive(!currentLine.hasChoices);

        if (currentLine.hasChoices)
            ConfigureChoices();
    }

    private void MostrarFinalEscena()
    {
        mostrandoFinal = true;
        currentLine = null;

        if (speakerName != null)
            speakerName.text = "Narrador";

        if (dialogueText != null)
            dialogueText.text = mensajeFinal;

        if (choicesPanel != null)
            choicesPanel.SetActive(false);

        if (characterLeft != null)
            characterLeft.gameObject.SetActive(false);

        if (characterRight != null)
            characterRight.gameObject.SetActive(false);

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);

            TMP_Text etiqueta =
                continueButton.GetComponentInChildren<TMP_Text>();

            if (etiqueta != null)
                etiqueta.text = textoBotonMapa;
        }

        Debug.Log("Secuencia narrativa terminada. Esperando para ir al mapa.");
    }

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

    private void ShowCharacter(DialogueLine line)
    {
        if (characterLeft != null)
            characterLeft.gameObject.SetActive(false);

        if (characterRight != null)
            characterRight.gameObject.SetActive(false);

        if (line.expression == null)
            return;

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

        if (target != null)
        {
            target.sprite = line.expression;
            target.preserveAspect = true;
            target.gameObject.SetActive(true);
        }
    }

    private void ContinueDialogue()
    {
        // El botón final ahora lleva al mapa.
        if (mostrandoFinal)
        {
            IrAlMapa();
            return;
        }

        if (currentLine == null || currentLine.hasChoices)
            return;

        ShowLine(currentLine.nextLineId);
    }

    private void IrAlMapa()
    {
        // Si hay un nombre de escena configurado en el Inspector,
        // se usa ese; de lo contrario, se usa el mapa de Cali.
        string escenaDestino = !string.IsNullOrWhiteSpace(nextSceneName)
            ? nextSceneName
            : nombreEscenaMapa;

        if (Application.CanStreamedLevelBeLoaded(escenaDestino))
        {
            SceneManager.LoadScene(escenaDestino);
        }
        else
        {
            Debug.LogError(
                "No se puede cargar la escena '" + escenaDestino +
                "'. Comprueba su nombre y que esté incluida en Build Profiles."
            );
        }
    }

    private void ConfigureChoices()
    {
        if (currentLine.choices == null ||
            currentLine.choices.Length < 2)
        {
            Debug.LogError(
                "La decisión necesita al menos dos opciones.");

            if (choicesPanel != null)
                choicesPanel.SetActive(false);

            if (continueButton != null)
                continueButton.gameObject.SetActive(true);

            return;
        }

        ConfigurarBoton(choiceA, currentLine.choices[0]);
        ConfigurarBoton(choiceB, currentLine.choices[1]);
    }

    private void SetButtonText(Button button, string text)
    {
        if (button == null)
            return;

        TMP_Text label = button.GetComponentInChildren<TMP_Text>();

        if (label != null)
            label.text = text;
    }

    private void SelectChoice(int index)
    {
        if (currentLine == null ||
            currentLine.choices == null ||
            index < 0 ||
            index >= currentLine.choices.Length)
            return;

        ChoiceData choice = currentLine.choices[index];

        GameState.Sumar(ref GameState.PREP, choice.prep);
        GameState.Sumar(ref GameState.INFO, choice.info);
        GameState.Sumar(ref GameState.CONF, choice.conf);
        GameState.Sumar(ref GameState.RIESGO, choice.riesgo);

        GameState.Sumar(ref GameState.cSebas, choice.cSebas);
        GameState.Sumar(ref GameState.cCamila, choice.cCamila);
        GameState.Sumar(ref GameState.cChela, choice.cChela);

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

        // Persistir los indicadores después de tomar la decisión.
        GameSaveSystem.SaveGameState();

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

        ShowLine(choice.nextLineId);
    }

    private void ConfigurarBoton(Button boton, ChoiceData opcion)
    {
        if (boton == null || opcion == null)
            return;

        bool bloqueada =
            opcion.requiresLibreta && !GameState.tieneLibreta;

        string textoBoton =
            bloqueada && !string.IsNullOrEmpty(opcion.lockedHint)
                ? opcion.lockedHint
                : opcion.text;

        SetButtonText(boton, textoBoton);
        boton.interactable = !bloqueada;
    }

    private bool EvaluarCondicion(DialogueLine linea)
    {
        switch (linea.condition)
        {
            case RouterCondition.PrepMayorOIgual:
                return GameState.PREP >= linea.conditionValue;

            case RouterCondition.Panico:
                return GameState.panico;

            case RouterCondition.TieneLibreta:
                return GameState.tieneLibreta;

            case RouterCondition.SinReservaAgua:
                return !GameState.reservaAgua;

            case RouterCondition.Arriesgo:
                return GameState.arriesgo;

            default:
                return false;
        }
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(ContinueDialogue);

        if (choiceA != null)
            choiceA.onClick.RemoveAllListeners();

        if (choiceB != null)
            choiceB.onClick.RemoveAllListeners();
    }
}
