
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

    private Dictionary<string, DialogueLine> lineMap;
    private DialogueLine currentLine;

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

        continueButton.onClick.AddListener(ContinueDialogue);
        choiceA.onClick.AddListener(() => SelectChoice(0));
        choiceB.onClick.AddListener(() => SelectChoice(1));

        choicesPanel.SetActive(false);

        if (characterLeft != null)
            characterLeft.gameObject.SetActive(false);

        if (characterRight != null)
            characterRight.gameObject.SetActive(false);

        ShowLine(firstLineId);
    }

    private void ShowLine(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            if (!string.IsNullOrWhiteSpace(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
                return;
            }

            Debug.Log("Fin de esta secuencia narrativa.");
            speakerName.text = "";
            dialogueText.text = "Fin de la escena.";
            continueButton.gameObject.SetActive(false);
            choicesPanel.SetActive(false);
            return;
        }

        if (!lineMap.TryGetValue(id, out currentLine))
        {
            Debug.LogError("No existe la línea: " + id);
            return;
        }

        if (currentLine.isRouter)
        {
            string destino = EvaluarCondicion(currentLine)
                ? currentLine.nextLineIfTrue
                : currentLine.nextLineId;
            ShowLine(destino);
            return;
        }

        GameState.Sumar(ref GameState.RIESGO, currentLine.onEnterRiesgo);

        if (background != null && currentLine.background != null)
            background.sprite = currentLine.background;

        if (currentLine.isCapsule && capsuleView != null)
        {
            speakerName.text = GetSpeakerName(currentLine.character);
            dialogueText.text = currentLine.text;
            choicesPanel.SetActive(false);
            continueButton.gameObject.SetActive(false);

            string siguiente = currentLine.nextLineId;
            capsuleView.Show(currentLine.capsuleWords,
                             currentLine.capsulePick,
                             () => ShowLine(siguiente));
            return;
        }


        speakerName.text = GetSpeakerName(currentLine.character);
        dialogueText.text = currentLine.text;

        ShowCharacter(currentLine);

        choicesPanel.SetActive(currentLine.hasChoices);
        continueButton.gameObject.SetActive(!currentLine.hasChoices);

        if (currentLine.hasChoices)
            ConfigureChoices();
    }

    private string GetSpeakerName(CharacterId character)
    {
        switch (character)
        {
            case CharacterId.Lucia: return GameState.nombreJugador;
            case CharacterId.Sebas: return "Sebas";
            case CharacterId.Camila: return "Camila";
            case CharacterId.Chela: return "Doña Chela (teléfono)";
            case CharacterId.DonHernan: return "Don Hernán";
            case CharacterId.Fuente7: return "Fuente 7";
            default: return "Narrador";
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
        if (currentLine == null || currentLine.hasChoices)
            return;

        ShowLine(currentLine.nextLineId);
    }

    private void ConfigureChoices()
    {
        if (currentLine.choices == null ||
            currentLine.choices.Length < 2)
        {
            Debug.LogError(
                "La decisión necesita al menos dos opciones.");
            choicesPanel.SetActive(false);
            continueButton.gameObject.SetActive(true);
            return;
        }

        ConfigurarBoton(choiceA, currentLine.choices[0]);
        ConfigurarBoton(choiceB, currentLine.choices[1]);
    }

    private void SetButtonText(Button button, string text)
    {
        TMP_Text label = button.GetComponentInChildren<TMP_Text>();

        if (label != null)
            label.text = text;
    }

    private void SelectChoice(int index)
    {
        if (currentLine == null ||
            currentLine.choices == null ||
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

        Debug.Log(
            $"Elección: {choice.text} | " +
            $"PREP={GameState.PREP}, " +
            $"INFO={GameState.INFO}, " +
            $"CONF={GameState.CONF}, " +
            $"RIESGO={GameState.RIESGO} | " +
            $"arriesgo={GameState.arriesgo}, panico={GameState.panico}, " +
            $"cSebas={GameState.cSebas}, cCamila={GameState.cCamila}, cChela={GameState.cChela}");

        ShowLine(choice.nextLineId);
    }

    private void ConfigurarBoton(Button boton, ChoiceData opcion)
    {
        bool bloqueada = opcion.requiresLibreta && !GameState.tieneLibreta;

        string textoBoton = bloqueada && !string.IsNullOrEmpty(opcion.lockedHint)
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

}
