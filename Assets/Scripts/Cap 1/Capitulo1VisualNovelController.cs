
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Capitulo1VisualNovelController : MonoBehaviour
{
    public enum Emotion
    {
        Normal, Sonriente, Triste, Enojado, Sorprendido, Pensativo
    }

    [Serializable]
    public class CharacterVisuals
    {
        public Sprite normal;
        public Sprite sonriente;
        public Sprite triste;
        public Sprite enojado;
        public Sprite sorprendido;
        public Sprite pensativo;

        public Sprite GetSprite(Emotion emotion)
        {
            switch (emotion)
            {
                case Emotion.Sonriente: return sonriente;
                case Emotion.Triste: return triste;
                case Emotion.Enojado: return enojado;
                case Emotion.Sorprendido: return sorprendido;
                case Emotion.Pensativo: return pensativo;
                default: return normal;
            }
        }
    }

    [Header("Fondos")]
    [SerializeField] private GameObject fondoRio;
    [SerializeField] private GameObject fondoTextoRio;
    [SerializeField] private GameObject fondoRadioLadera;
    [SerializeField] private GameObject fondoTiendaChela;

    [Header("Imágenes de personajes")]
    [SerializeField] private Image characterLeft;
    [SerializeField] private Image characterRight;

    [Header("Sprites por personaje")]
    [SerializeField] private CharacterVisuals lucia;
    [SerializeField] private CharacterVisuals sebas;
    [SerializeField] private CharacterVisuals camila;
    [SerializeField] private CharacterVisuals chela;
    [SerializeField] private CharacterVisuals donHernan;

    [Header("Botón de entrada")]
    [SerializeField] private Button botonEntrarRadio;

    [Header("Panel de diálogo")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerName;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button continueButton;

    [Header("Botón de consulta del mapa")]
    [SerializeField] private Button consultarMapaButton;

    [Header("Opciones")]
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private Button choiceA;
    [SerializeField] private Button choiceB;
    [SerializeField] private Button choiceC;

    [Header("Distribución de opciones")]
    [SerializeField] private float separacionDosOpciones = 300f;
    [SerializeField] private float desplazamientoVerticalOpciones = 0f;

    private Vector2 posicionOriginalA;
    private Vector2 posicionOriginalB;
    private Vector2 posicionOriginalC;

    private enum StoryStep
    {
        IntroduccionRio,
        ContextoCabina,
        ElegirInterlocutor,
        DialogoPersonal,
        TareaMapa,
        ContextoD1,
        DecisionD1,
        FinDecisionD1
    }

    private StoryStep currentStep;
    private string personajeElegido = "";

    private const string MAP_RETURN_KEY = "Cap1VolvioDelMapa";
    private const string SILOE_VISITED_KEY = "ConsultaSiloeCap1";
    private const string INFO_REWARDED_KEY = "InfoSiloeRecompensada";
    private const string MAP_ENTRY_KEY = "Cap1EntroAlMapa";

    private void Awake()
    {
        GuardarPosicionesOriginales();

        if (botonEntrarRadio != null)
            botonEntrarRadio.onClick.AddListener(EntrarAlEstudio);

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueStory);

        if (consultarMapaButton != null)
            consultarMapaButton.onClick.AddListener(AbrirMapa);

        if (choiceA != null)
            choiceA.onClick.AddListener(OnChoiceA);

        if (choiceB != null)
            choiceB.onClick.AddListener(OnChoiceB);

        if (choiceC != null)
            choiceC.onClick.AddListener(OnChoiceC);
    }

    private void GuardarPosicionesOriginales()
    {
        if (choiceA != null)
            posicionOriginalA = choiceA.GetComponent<RectTransform>().anchoredPosition;

        if (choiceB != null)
            posicionOriginalB = choiceB.GetComponent<RectTransform>().anchoredPosition;

        if (choiceC != null)
            posicionOriginalC = choiceC.GetComponent<RectTransform>().anchoredPosition;
    }

    private void Start()
    {
        if (PlayerPrefs.GetInt(MAP_RETURN_KEY, 0) == 1)
        {
            PlayerPrefs.SetInt(MAP_RETURN_KEY, 0);
            PlayerPrefs.Save();

            SetActive(fondoRio, false);
            SetActive(fondoTextoRio, false);
            SetActive(fondoRadioLadera, true);
            SetActive(fondoTiendaChela, false);

            SetActive(botonEntrarRadio != null ? botonEntrarRadio.gameObject : null, false);
            SetActive(dialoguePanel, true);
            SetActive(consultarMapaButton != null ? consultarMapaButton.gameObject : null, false);

            HideCharacters();

            if (PlayerPrefs.GetInt(SILOE_VISITED_KEY, 0) == 1)
            {
                if (PlayerPrefs.GetInt(INFO_REWARDED_KEY, 0) == 0)
                {
                    GameState.Sumar(ref GameState.INFO, 1);
                    PlayerPrefs.SetInt(INFO_REWARDED_KEY, 1);
                    PlayerPrefs.Save();
                    Debug.Log("Consulta de Siloé completada: INFO +1.");
                }

                MostrarDecisionD1();
            }
            else
            {
                MostrarTareaMapa();
            }

            return;
        }

        SetActive(fondoRio, true);
        SetActive(fondoTextoRio, true);
        SetActive(fondoRadioLadera, false);
        SetActive(fondoTiendaChela, false);

        HideCharacters();

        SetActive(dialoguePanel, false);
        SetActive(choicesPanel, false);
        SetActive(continueButton != null ? continueButton.gameObject : null, false);
        SetActive(consultarMapaButton != null ? consultarMapaButton.gameObject : null, false);
        SetActive(botonEntrarRadio != null ? botonEntrarRadio.gameObject : null, true);

        SetButtonLabel(botonEntrarRadio, "ENTRAR A RADIO LADERA");
        SetButtonLabel(continueButton, "CONTINUAR");
        SetButtonLabel(consultarMapaButton, "CONSULTAR MAPA");

        currentStep = StoryStep.IntroduccionRio;
    }

    private void EntrarAlEstudio()
    {
        if (currentStep != StoryStep.IntroduccionRio)
            return;

        SetActive(fondoRio, false);
        SetActive(fondoTextoRio, false);
        SetActive(fondoRadioLadera, true);
        SetActive(fondoTiendaChela, false);

        SetActive(botonEntrarRadio != null ? botonEntrarRadio.gameObject : null, false);
        SetActive(dialoguePanel, true);
        SetActive(choicesPanel, false);
        SetActive(continueButton != null ? continueButton.gameObject : null, true);

        HideCharacters();

        if (speakerName != null)
            speakerName.text = "Narrador";

        if (dialogueText != null)
        {
            dialogueText.text =
                "La cabina de Radio Ladera está llena de notas, mapas " +
                "y mensajes de los habitantes. Lucía se prepara para " +
                "hablar sobre la disminución del agua en la zona. " +
                "Antes de salir al aire, quiere escuchar a quienes " +
                "conocen la situación de primera mano.";
        }

        currentStep = StoryStep.ContextoCabina;
    }

    private void ContinueStory()
    {
        switch (currentStep)
        {
            case StoryStep.ContextoCabina:
                MostrarElecciones();
                break;

            case StoryStep.DialogoPersonal:
                MostrarTareaMapa();
                break;

            case StoryStep.ContextoD1:
                MostrarOpcionesD1();
                break;

            case StoryStep.FinDecisionD1:
                Debug.Log("D1 registrada. El siguiente paso será la escena de Don Hernán.");
                break;
        }
    }

    private void MostrarElecciones()
    {
        HideCharacters();

        if (speakerName != null)
            speakerName.text = "Radio Ladera";

        if (dialogueText != null)
            dialogueText.text = "¿Con quién quieres hablar antes de salir al aire?";

        SetActive(continueButton != null ? continueButton.gameObject : null, false);
        SetActive(choicesPanel, true);

        SetButtonLabel(choiceA, "Hablar con Sebas");
        SetButtonLabel(choiceB, "Hablar con Camila");
        SetButtonLabel(choiceC, "Llamar a Chela");

        RestaurarPosicionesOpciones();
        SetActive(choiceC != null ? choiceC.gameObject : null, true);

        currentStep = StoryStep.ElegirInterlocutor;
    }

    private void OnChoiceA()
    {
        if (currentStep == StoryStep.ElegirInterlocutor)
            SelectCharacter("Sebas");
        else if (currentStep == StoryStep.DecisionD1)
            SelectDecisionD1(0);
    }

    private void OnChoiceB()
    {
        if (currentStep == StoryStep.ElegirInterlocutor)
            SelectCharacter("Camila");
        else if (currentStep == StoryStep.DecisionD1)
            SelectDecisionD1(1);
    }

    private void OnChoiceC()
    {
        if (currentStep == StoryStep.ElegirInterlocutor)
            SelectCharacter("Chela");
    }

    private void SelectCharacter(string character)
    {
        if (currentStep != StoryStep.ElegirInterlocutor)
            return;

        personajeElegido = character;

        SetActive(choicesPanel, false);
        SetActive(continueButton != null ? continueButton.gameObject : null, true);
        SetActive(consultarMapaButton != null ? consultarMapaButton.gameObject : null, false);

        RestaurarPosicionesOpciones();

        switch (character)
        {
            case "Sebas":
                SetActive(fondoRadioLadera, true);
                SetActive(fondoTiendaChela, false);
                ShowSpeaker("Sebas", sebas, Emotion.Pensativo,
                    "La gente está preocupada por el agua. He escuchado varias versiones sobre lo que está pasando.");
                break;

            case "Camila":
                SetActive(fondoRadioLadera, true);
                SetActive(fondoTiendaChela, false);
                ShowSpeaker("Camila", camila, Emotion.Pensativo,
                    "Antes de compartir una alerta, tenemos que revisar qué información está confirmada y qué es un rumor.");
                break;

            case "Chela":
                SetActive(fondoRadioLadera, false);
                SetActive(fondoTiendaChela, true);
                ShowSpeaker("Doña Chela", chela, Emotion.Pensativo,
                    "Lucía, mija, la gente está preocupada por el agua. Necesitamos información clara para el barrio.");
                break;
        }

        currentStep = StoryStep.DialogoPersonal;
    }

    private void MostrarTareaMapa()
    {
        SetActive(fondoTiendaChela, false);
        SetActive(fondoRadioLadera, true);

        ShowSpeaker("Camila", camila, Emotion.Pensativo,
            "Antes de hablar al aire, debemos verificar la situación. El fenómeno de El Niño puede favorecer condiciones secas, pero necesitamos revisar los datos de la zona. Consulta el marcador de sequía de Siloé en el mapa.");

        SetActive(continueButton != null ? continueButton.gameObject : null, false);
        SetActive(choicesPanel, false);
        SetActive(consultarMapaButton != null ? consultarMapaButton.gameObject : null, true);

        currentStep = StoryStep.TareaMapa;
    }

    private void AbrirMapa()
    {
        if (currentStep != StoryStep.TareaMapa)
            return;

        PlayerPrefs.SetInt(MAP_ENTRY_KEY, 1);
        PlayerPrefs.SetInt(MAP_RETURN_KEY, 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene("01_MapaCali");
    }

    private void MostrarDecisionD1()
    {
        SetActive(fondoRio, false);
        SetActive(fondoTextoRio, false);
        SetActive(fondoRadioLadera, true);
        SetActive(fondoTiendaChela, false);

        SetActive(dialoguePanel, true);
        SetActive(choicesPanel, false);
        SetActive(consultarMapaButton != null ? consultarMapaButton.gameObject : null, false);
        SetActive(continueButton != null ? continueButton.gameObject : null, true);

        ShowSpeaker("Lucía", lucia, Emotion.Pensativo,
            "Ahora sabemos que Siloé enfrenta dificultades con el abastecimiento de agua. Pero conocer el problema no es suficiente: debemos decidir cómo ayudar a la comunidad sin generar más preocupación.");

        SetButtonLabel(continueButton, "CONTINUAR");

        RestaurarPosicionesOpciones();

        currentStep = StoryStep.ContextoD1;
    }

    private void MostrarOpcionesD1()
    {
        ShowSpeaker("Lucía", lucia, Emotion.Pensativo,
            "¿Qué estrategia deberíamos recomendar a la comunidad?");

        DistribuirDosOpciones();

        SetActive(choicesPanel, true);
        SetActive(continueButton != null ? continueButton.gameObject : null, false);

        SetButtonLabel(choiceA, "Organizar una reserva comunitaria y establecer turnos");
        SetButtonLabel(choiceB, "Solo consultar los horarios de abastecimiento");

        SetActive(choiceA != null ? choiceA.gameObject : null, true);
        SetActive(choiceB != null ? choiceB.gameObject : null, true);
        SetActive(choiceC != null ? choiceC.gameObject : null, false);

        currentStep = StoryStep.DecisionD1;
    }

    private void DistribuirDosOpciones()
    {
        if (choiceA == null || choiceB == null)
            return;

        RectTransform a = choiceA.GetComponent<RectTransform>();
        RectTransform b = choiceB.GetComponent<RectTransform>();

        // Centrar los dos botones usando el centro del panel de opciones.
        RectTransform parentRect = choicesPanel.GetComponent<RectTransform>();

        float anchoA = a.rect.width;
        float anchoB = b.rect.width;
        float espacio = separacionDosOpciones;

        float centroA = -(espacio / 2f + anchoA / 2f);
        float centroB = espacio / 2f + anchoB / 2f;

        a.anchoredPosition = new Vector2(
            centroA,
            posicionOriginalA.y + desplazamientoVerticalOpciones
        );

        b.anchoredPosition = new Vector2(
            centroB,
            posicionOriginalB.y + desplazamientoVerticalOpciones
        );
    }

    private void RestaurarPosicionesOpciones()
    {
        if (choiceA != null)
            choiceA.GetComponent<RectTransform>().anchoredPosition = posicionOriginalA;

        if (choiceB != null)
            choiceB.GetComponent<RectTransform>().anchoredPosition = posicionOriginalB;

        if (choiceC != null)
            choiceC.GetComponent<RectTransform>().anchoredPosition = posicionOriginalC;
    }

    private void SelectDecisionD1(int option)
    {
        if (currentStep != StoryStep.DecisionD1)
            return;

        if (option == 0)
        {
            GameState.Sumar(ref GameState.PREP, 2);
            GameState.Sumar(ref GameState.CONF, 1);
            GameState.reservaAgua = true;

            PlayerPrefs.SetInt("Cap1D1", 1);
            Debug.Log("D1: reserva comunitaria. PREP +2, CONF +1.");
        }
        else
        {
            GameState.Sumar(ref GameState.INFO, 1);

            PlayerPrefs.SetInt("Cap1D1", 2);
            Debug.Log("D1: consultar horarios. INFO +1.");
        }

        PlayerPrefs.Save();

        SetActive(choicesPanel, false);
        SetActive(continueButton != null ? continueButton.gameObject : null, true);

        ShowSpeaker("Lucía", lucia, Emotion.Pensativo,
            option == 0
                ? "Organizaremos una reserva comunitaria y turnos para distribuir el agua."
                : "Primero consultaremos los horarios de abastecimiento para orientar a la comunidad.");

        SetButtonLabel(continueButton, "CONTINUAR");

        RestaurarPosicionesOpciones();

        currentStep = StoryStep.FinDecisionD1;
    }

    private void ShowSpeaker(string visibleName, CharacterVisuals visuals, Emotion emotion, string text)
    {
        if (speakerName != null)
            speakerName.text = visibleName;

        if (dialogueText != null)
            dialogueText.text = text;

        HideCharacters();

        if (visuals == null)
        {
            Debug.LogWarning("Faltan sprites para " + visibleName);
            return;
        }

        Sprite sprite = visuals.GetSprite(emotion);
        if (sprite == null)
            sprite = visuals.normal;

        if (sprite == null)
        {
            Debug.LogWarning("No hay sprite asignado para " + visibleName);
            return;
        }

        Image target =
            visibleName.Contains("Sebas") || visibleName.Contains("Camila")
                ? characterRight
                : characterLeft;

        if (target == null)
        {
            Debug.LogWarning("No hay Image asignado para " + visibleName);
            return;
        }

        target.sprite = sprite;
        target.preserveAspect = true;
        target.gameObject.SetActive(true);
    }

    private void HideCharacters()
    {
        if (characterLeft != null)
            characterLeft.gameObject.SetActive(false);

        if (characterRight != null)
            characterRight.gameObject.SetActive(false);
    }

    private void SetButtonLabel(Button button, string label)
    {
        if (button == null)
            return;

        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = label;
    }

    private void SetActive(GameObject obj, bool active)
    {
        if (obj != null)
            obj.SetActive(active);
    }

    private void OnDestroy()
    {
        if (botonEntrarRadio != null)
            botonEntrarRadio.onClick.RemoveListener(EntrarAlEstudio);

        if (continueButton != null)
            continueButton.onClick.RemoveListener(ContinueStory);

        if (consultarMapaButton != null)
            consultarMapaButton.onClick.RemoveListener(AbrirMapa);

        if (choiceA != null)
            choiceA.onClick.RemoveListener(OnChoiceA);

        if (choiceB != null)
            choiceB.onClick.RemoveListener(OnChoiceB);

        if (choiceC != null)
            choiceC.onClick.RemoveListener(OnChoiceC);
    }
}
