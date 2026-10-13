
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
    [SerializeField] private GameObject fondoLoteSeco;
    [SerializeField] private GameObject fondoCasaDonHernan;

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

    [Header("Opciones narrativas")]
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

    [Header("Cápsula radial C1")]
    [SerializeField] private GameObject capsulaPanel;
    [SerializeField] private TMP_Text tituloCapsula;
    [SerializeField] private TMP_Text instruccionesCapsula;
    [SerializeField] private TMP_Text contadorCapsula;

    [Header("Botones de palabras")]
    [SerializeField] private Button palabraTapados;
    [SerializeField] private Button palabraFugas;
    [SerializeField] private Button palabraOficial;
    [SerializeField] private Button palabraAcaparar;
    [SerializeField] private Button palabraElSol;

    [Header("Botones de la cápsula")]
    [SerializeField] private Button confirmarCapsula;
    [SerializeField] private Button cerrarCapsula;

    [Header("Apariencia de selección")]
    [SerializeField]
    private Color colorPalabraSeleccionada =
        new Color(1f, 0.82f, 0.45f, 1f);

    private readonly bool[] palabrasSeleccionadas = new bool[5];
    private readonly Color[] coloresOriginales = new Color[5];
    private int cantidadSeleccionada;

    private enum StoryStep
    {
        IntroduccionRio,
        ContextoCabina,
        ElegirInterlocutor,
        DialogoPersonal,

        ExplicacionElNino1,
        ExplicacionElNino2,
        ExplicacionElNino3,
        ExplicacionSequia1,
        ExplicacionSequia2,
        ExplicacionSequia3,
        TareaMapa,

        ContextoD1,
        DecisionD1,
        FinDecisionD1,

        EncuentroDonHernan,
        DialogoDonHernan,
        ContextoD2,
        DecisionD2,
        FinDecisionD2,

        EncuentroLote,
        DialogoLote1,
        DialogoLote2,
        ContextoD3,
        DecisionD3,
        FinDecisionD3,

        IntroduccionCapsula,
        SeleccionCapsula,
        FinCapsula
    }

    private StoryStep currentStep;

    private static bool sesionInicializada;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReiniciarMarcaDeSesion()
    {
        sesionInicializada = false;
    }

    private const string MAP_RETURN_KEY = "Cap1VolvioDelMapa";
    private const string SILOE_VISITED_KEY = "ConsultaSiloeCap1";
    private const string INFO_REWARDED_KEY = "InfoSiloeRecompensada";
    private const string MAP_ENTRY_KEY = "Cap1EntroAlMapa";

    private const string D1_KEY = "Cap1D1";
    private const string D2_KEY = "Cap1D2";
    private const string D3_KEY = "Cap1D3";
    private const string C1_KEY = "Cap1C1Completada";

    private void Awake()
    {
        GuardarPosicionesOriginales();
        PrepararBotonesCapsula();

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

        if (palabraTapados != null)
            palabraTapados.onClick.AddListener(() => AlternarPalabra(0));

        if (palabraFugas != null)
            palabraFugas.onClick.AddListener(() => AlternarPalabra(1));

        if (palabraOficial != null)
            palabraOficial.onClick.AddListener(() => AlternarPalabra(2));

        if (palabraAcaparar != null)
            palabraAcaparar.onClick.AddListener(() => AlternarPalabra(3));

        if (palabraElSol != null)
            palabraElSol.onClick.AddListener(() => AlternarPalabra(4));

        if (confirmarCapsula != null)
            confirmarCapsula.onClick.AddListener(ConfirmarCapsula);

        if (cerrarCapsula != null)
            cerrarCapsula.onClick.AddListener(CerrarCapsula);
    }

    private void Start()
    {
        if (!sesionInicializada)
        {
            InicializarNuevaSesion();
            sesionInicializada = true;
        }

        SetActive(capsulaPanel, false);
        SetActive(fondoLoteSeco, false);
        SetActive(fondoCasaDonHernan, false);

        if (PlayerPrefs.GetInt(MAP_RETURN_KEY, 0) == 1)
        {
            PlayerPrefs.SetInt(MAP_RETURN_KEY, 0);
            PlayerPrefs.Save();

            MostrarSoloFondo(fondoRadioLadera);

            SetActive(botonEntrarRadio != null
                ? botonEntrarRadio.gameObject : null, false);

            SetActive(dialoguePanel, true);
            HideCharacters();

            if (PlayerPrefs.GetInt(SILOE_VISITED_KEY, 0) == 1)
            {
                if (PlayerPrefs.GetInt(INFO_REWARDED_KEY, 0) == 0)
                {
                    GameState.Sumar(ref GameState.INFO, 1);
                    PlayerPrefs.SetInt(INFO_REWARDED_KEY, 1);
                    GuardarEstado();
                }

                MostrarDecisionD1();
            }
            else
            {
                MostrarTareaMapa();
            }

            return;
        }

        // Mostrar el río y el texto inicial juntos.
        SetActive(fondoRio, true);
        SetActive(fondoTextoRio, true);
        SetActive(fondoRadioLadera, false);
        SetActive(fondoTiendaChela, false);
        SetActive(fondoLoteSeco, false);
        SetActive(fondoCasaDonHernan, false);

        HideCharacters();

        SetActive(dialoguePanel, false);
        SetActive(choicesPanel, false);
        SetActive(continueButton != null
            ? continueButton.gameObject : null, false);
        SetActive(consultarMapaButton != null
            ? consultarMapaButton.gameObject : null, false);
        SetActive(botonEntrarRadio != null
            ? botonEntrarRadio.gameObject : null, true);

        SetButtonLabel(botonEntrarRadio, "ENTRAR A RADIO LADERA");
        SetButtonLabel(continueButton, "CONTINUAR");
        SetButtonLabel(consultarMapaButton, "CONSULTAR MAPA");

        currentStep = StoryStep.IntroduccionRio;
    }

    private void InicializarNuevaSesion()
    {
        GameState.NuevaPartida();

        string[] clavesCapitulo1 =
        {
            MAP_RETURN_KEY,
            SILOE_VISITED_KEY,
            INFO_REWARDED_KEY,
            MAP_ENTRY_KEY,
            D1_KEY,
            D2_KEY,
            D3_KEY,
            C1_KEY,
            "Cap1C1Correctas",
            "Cap1C1Dudosas"
        };

        foreach (string clave in clavesCapitulo1)
            PlayerPrefs.DeleteKey(clave);

        PlayerPrefs.Save();
        GameSaveSystem.SaveGameState();

        Debug.Log("Nueva ejecución: progreso del capítulo 1 reiniciado.");
    }

    private void EntrarAlEstudio()
    {
        if (currentStep != StoryStep.IntroduccionRio)
            return;

        MostrarSoloFondo(fondoRadioLadera);

        SetActive(botonEntrarRadio != null
            ? botonEntrarRadio.gameObject : null, false);
        SetActive(dialoguePanel, true);
        SetActive(choicesPanel, false);
        SetActive(continueButton != null
            ? continueButton.gameObject : null, true);

        ShowSpeaker(
            "Narrador", null, Emotion.Normal,
            "La cabina de Radio Ladera está llena de notas, mapas y mensajes de los habitantes. " +
            "Lucía se prepara para hablar sobre la disminución del agua. " +
            "Antes de salir al aire, quiere escuchar a quienes conocen la situación."
        );

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

            case StoryStep.ExplicacionElNino1:
                MostrarExplicacion(
                    StoryStep.ExplicacionElNino2,
                    "Ese calentamiento modifica la circulación atmosférica y puede alterar los patrones de lluvia en diferentes regiones del mundo."
                );
                break;

            case StoryStep.ExplicacionElNino2:
                MostrarExplicacion(
                    StoryStep.ExplicacionElNino3,
                    "En Colombia, El Niño puede favorecer menos lluvias y temperaturas más altas, aunque sus efectos varían según la región."
                );
                break;

            case StoryStep.ExplicacionElNino3:
                MostrarExplicacion(
                    StoryStep.ExplicacionSequia1,
                    "Cuando llueve poco durante semanas o meses, el suelo pierde humedad y disminuye el agua disponible en el entorno."
                );
                break;

            case StoryStep.ExplicacionSequia1:
                MostrarExplicacion(
                    StoryStep.ExplicacionSequia2,
                    "Si la falta de lluvia se prolonga, puede producirse una sequía que afecte ríos, cultivos, vegetación y abastecimiento."
                );
                break;

            case StoryStep.ExplicacionSequia2:
                MostrarExplicacion(
                    StoryStep.ExplicacionSequia3,
                    "Por eso debemos consultar los datos de Siloé y compartir información confiable antes de sacar conclusiones sobre la sequía."
                );
                break;

            case StoryStep.ExplicacionSequia3:
                MostrarTareaMapaFinal();
                break;

            case StoryStep.ContextoD1:
                MostrarOpcionesD1();
                break;

            case StoryStep.FinDecisionD1:
                MostrarEncuentroDonHernan();
                break;

            case StoryStep.EncuentroDonHernan:
                MostrarDialogoDonHernan();
                break;

            case StoryStep.DialogoDonHernan:
                MostrarContextoD2();
                break;

            case StoryStep.ContextoD2:
                MostrarOpcionesD2();
                break;

            case StoryStep.FinDecisionD2:
                MostrarEncuentroLote();
                break;

            case StoryStep.EncuentroLote:
                MostrarDialogoLote1();
                break;

            case StoryStep.DialogoLote1:
                MostrarDialogoLote2();
                break;

            case StoryStep.DialogoLote2:
                MostrarContextoD3();
                break;

            case StoryStep.ContextoD3:
                MostrarOpcionesD3();
                break;

            case StoryStep.FinDecisionD3:
                MostrarIntroduccionCapsula();
                break;

            case StoryStep.IntroduccionCapsula:
                AbrirCapsula();
                break;

            case StoryStep.FinCapsula:
                MostrarCierreCapitulo1();
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

        SetActive(continueButton != null
            ? continueButton.gameObject : null, false);
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
        else if (currentStep == StoryStep.DecisionD2)
            SelectDecisionD2(0);
        else if (currentStep == StoryStep.DecisionD3)
            SelectDecisionD3(0);
    }

    private void OnChoiceB()
    {
        if (currentStep == StoryStep.ElegirInterlocutor)
            SelectCharacter("Camila");
        else if (currentStep == StoryStep.DecisionD1)
            SelectDecisionD1(1);
        else if (currentStep == StoryStep.DecisionD2)
            SelectDecisionD2(1);
        else if (currentStep == StoryStep.DecisionD3)
            SelectDecisionD3(1);
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

        SetActive(choicesPanel, false);
        SetActive(continueButton != null
            ? continueButton.gameObject : null, true);

        switch (character)
        {
            case "Sebas":
                MostrarSoloFondo(fondoRadioLadera);
                ShowSpeaker("Sebas", sebas, Emotion.Pensativo,
                    "La gente está preocupada por el agua. He escuchado varias versiones sobre lo que está pasando.");
                break;

            case "Camila":
                MostrarSoloFondo(fondoRadioLadera);
                ShowSpeaker("Camila", camila, Emotion.Pensativo,
                    "Antes de compartir una alerta, tenemos que revisar qué información está confirmada y qué es un rumor.");
                break;

            case "Chela":
                MostrarSoloFondo(fondoTiendaChela);
                ShowSpeaker("Doña Chela", chela, Emotion.Pensativo,
                    "Lucía, mija, la gente está preocupada por el agua. Necesitamos información clara para el barrio.");
                break;
        }

        currentStep = StoryStep.DialogoPersonal;
    }

    // EXPLICACIÓN DE EL NIÑO Y LA SEQUÍA

    private void MostrarTareaMapa()
    {
        MostrarSoloFondo(fondoRadioLadera);

        ShowSpeaker(
            "Camila", camila, Emotion.Pensativo,
            "Hay algo importante que debemos entender: El Niño es un fenómeno climático relacionado con el calentamiento del océano Pacífico."
        );

        SetActive(consultarMapaButton != null
            ? consultarMapaButton.gameObject : null, false);
        SetActive(continueButton != null
            ? continueButton.gameObject : null, true);

        currentStep = StoryStep.ExplicacionElNino1;
    }

    private void MostrarExplicacion(StoryStep siguiente, string texto)
    {
        ShowSpeaker("Camila", camila, Emotion.Pensativo, texto);

        SetActive(consultarMapaButton != null
            ? consultarMapaButton.gameObject : null, false);
        SetActive(continueButton != null
            ? continueButton.gameObject : null, true);

        currentStep = siguiente;
    }

    private void MostrarTareaMapaFinal()
    {
        ShowSpeaker(
            "Camila", camila, Emotion.Pensativo,
            "Ahora revisemos qué está pasando específicamente en Siloé antes de compartir información con el barrio."
        );

        SetActive(continueButton != null
            ? continueButton.gameObject : null, false);
        SetActive(consultarMapaButton != null
            ? consultarMapaButton.gameObject : null, true);

        SetButtonLabel(consultarMapaButton, "CONSULTAR MAPA");
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

    // D1: ORGANIZACIÓN DEL AGUA

    private void MostrarDecisionD1()
    {
        MostrarSoloFondo(fondoRadioLadera);

        SetActive(dialoguePanel, true);
        SetActive(consultarMapaButton != null
            ? consultarMapaButton.gameObject : null, false);

        ShowSpeaker(
            "Lucía", lucia, Emotion.Pensativo,
            "La falta prolongada de lluvia puede complicar el abastecimiento. " +
            "Debemos organizarnos y consultar información confiable. " +
            "¿Qué estrategia proponemos?"
        );

        MostrarSoloBotonContinuar();
        currentStep = StoryStep.ContextoD1;
    }

    private void MostrarOpcionesD1()
    {
        MostrarDosOpciones(
            "Organizar una reserva comunitaria y establecer turnos",
            "Solo consultar los horarios de abastecimiento"
        );

        currentStep = StoryStep.DecisionD1;
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
            PlayerPrefs.SetInt(D1_KEY, 1);

            ShowSpeaker("Lucía", lucia, Emotion.Pensativo,
                "Organizaremos una reserva comunitaria y turnos para distribuir el agua.");
        }
        else
        {
            GameState.Sumar(ref GameState.INFO, 1);
            PlayerPrefs.SetInt(D1_KEY, 2);

            ShowSpeaker("Lucía", lucia, Emotion.Pensativo,
                "Primero consultaremos los horarios de abastecimiento para orientar a la comunidad.");
        }

        GuardarEstado();
        MostrarSoloBotonContinuar();
        currentStep = StoryStep.FinDecisionD1;
    }

    // D2: DON HERNÁN

    private void MostrarEncuentroDonHernan()
    {
        MostrarSoloFondo(fondoLoteSeco);

        ShowSpeaker(
            "Narrador", null, Emotion.Normal,
            "Después de organizar la información sobre la sequía, Lucía visita a Don Hernán, quien lleva años observando los cambios del entorno."
        );

        MostrarSoloBotonContinuar();
        currentStep = StoryStep.EncuentroDonHernan;
    }

    private void MostrarDialogoDonHernan()
    {
        MostrarSoloFondo(fondoCasaDonHernan);

        ShowSpeaker(
            "Don Hernán", donHernan, Emotion.Pensativo,
            "He vivido aquí durante muchos años. Tengo una libreta donde anoto los cambios que observo y puedo compartir mis recuerdos."
        );

        MostrarSoloBotonContinuar();
        currentStep = StoryStep.DialogoDonHernan;
    }

    private void MostrarContextoD2()
    {
        MostrarSoloFondo(fondoCasaDonHernan);

        ShowSpeaker(
            "Lucía", lucia, Emotion.Pensativo,
            "Los registros escritos ayudan a recordar fechas y cambios. " +
            "Una entrevista aporta experiencias personales. " +
            "Debo elegir cómo recopilar la información de Don Hernán."
        );

        MostrarSoloBotonContinuar();
        currentStep = StoryStep.ContextoD2;
    }

    private void MostrarOpcionesD2()
    {
        MostrarDosOpciones(
            "Revisar su libreta de observaciones",
            "Hacer una entrevista rápida"
        );

        currentStep = StoryStep.DecisionD2;
    }

    private void SelectDecisionD2(int option)
    {
        if (currentStep != StoryStep.DecisionD2)
            return;

        if (option == 0)
        {
            GameState.Sumar(ref GameState.INFO, 2);
            GameState.Sumar(ref GameState.CONF, 1);
            GameState.tieneLibreta = true;
            PlayerPrefs.SetInt(D2_KEY, 1);

            ShowSpeaker("Don Hernán", donHernan, Emotion.Sonriente,
                "Claro, Lucía. Puedes revisar mis anotaciones; allí he registrado los cambios que he observado con el tiempo.");
        }
        else
        {
            GameState.Sumar(ref GameState.INFO, 1);
            PlayerPrefs.SetInt(D2_KEY, 2);

            ShowSpeaker("Don Hernán", donHernan, Emotion.Pensativo,
                "Te cuento lo que recuerdo, aunque en la libreta tengo anotaciones más detalladas.");
        }

        GuardarEstado();
        MostrarSoloBotonContinuar();
        currentStep = StoryStep.FinDecisionD2;
    }

    // D3: LOTE SECO E INCENDIOS

    private void MostrarEncuentroLote()
    {
        MostrarSoloFondo(fondoLoteSeco);

        ShowSpeaker(
            "Narrador", null, Emotion.Normal,
            "Al salir de la casa, Lucía observa el lote seco. El pasto está quebradizo y hay basura acumulada entre la vegetación."
        );

        MostrarSoloBotonContinuar();
        currentStep = StoryStep.EncuentroLote;
    }

    private void MostrarDialogoLote1()
    {
        MostrarSoloFondo(fondoLoteSeco);

        ShowSpeaker(
            "Sebas", sebas, Emotion.Pensativo,
            "Mira cómo está el terreno. Cuando la vegetación pierde humedad, puede arder con más facilidad."
        );

        MostrarSoloBotonContinuar();
        currentStep = StoryStep.DialogoLote1;
    }

    private void MostrarDialogoLote2()
    {
        ShowSpeaker(
            "Camila", camila, Emotion.Pensativo,
            "Una colilla, una fogata o quemar residuos podría iniciar un incendio. Podemos reducir el riesgo evitando quemas y limpiando con cuidado."
        );

        MostrarSoloBotonContinuar();
        currentStep = StoryStep.DialogoLote2;
    }

    private void MostrarContextoD3()
    {
        MostrarSoloFondo(fondoLoteSeco);

        ShowSpeaker(
            "Lucía", lucia, Emotion.Pensativo,
            "La comunidad puede retirar los residuos sin quemarlos. " +
            "Si no actuamos, el terreno seguirá expuesto al riesgo de incendio. " +
            "Debemos decidir qué hacer con el lote."
        );

        MostrarSoloBotonContinuar();
        currentStep = StoryStep.ContextoD3;
    }

    private void MostrarOpcionesD3()
    {
        MostrarDosOpciones(
            "Organizar una jornada de limpieza y pedir que no quemen",
            "Dejar el lote como está"
        );

        currentStep = StoryStep.DecisionD3;
    }

    private void SelectDecisionD3(int option)
    {
        if (currentStep != StoryStep.DecisionD3)
            return;

        if (option == 0)
        {
            GameState.franjaLimpia = true;
            GameState.Sumar(ref GameState.PREP, 1);
            GameState.Sumar(ref GameState.RIESGO, -1);
            PlayerPrefs.SetInt(D3_KEY, 1);

            ShowSpeaker(
                "Lucía", lucia, Emotion.Pensativo,
                "Organizaremos una jornada de limpieza y pediremos que nadie queme residuos ni encienda fogatas cerca de la vegetación seca."
            );
        }
        else
        {
            GameState.Sumar(ref GameState.RIESGO, 2);
            PlayerPrefs.SetInt(D3_KEY, 2);

            ShowSpeaker(
                "Lucía", lucia, Emotion.Pensativo,
                "Por ahora dejaremos el lote como está. La vegetación seca y la basura seguirán siendo factores de riesgo."
            );
        }

        GuardarEstado();
        MostrarSoloBotonContinuar();
        currentStep = StoryStep.FinDecisionD3;
    }

    // C1: CÁPSULA RADIAL

    private void MostrarIntroduccionCapsula()
    {
        MostrarSoloFondo(fondoLoteSeco);

        if (PlayerPrefs.GetInt(C1_KEY, 0) == 1)
        {
            ShowSpeaker(
                "Narrador", null, Emotion.Normal,
                "La cápsula radial sobre la sequía ya fue completada."
            );

            MostrarSoloBotonContinuar();
            currentStep = StoryStep.FinCapsula;
            return;
        }

        ShowSpeaker(
            "Narrador", null, Emotion.Normal,
            "Antes de cerrar la transmisión, Lucía debe preparar recomendaciones para ahorrar agua, revisar fugas y consultar información confiable."
        );

        MostrarSoloBotonContinuar();
        currentStep = StoryStep.IntroduccionCapsula;
    }

    private void AbrirCapsula()
    {
        if (currentStep != StoryStep.IntroduccionCapsula)
            return;

        ReiniciarSeleccionCapsula();

        SetActive(dialoguePanel, false);
        SetActive(choicesPanel, false);
        SetActive(continueButton != null
            ? continueButton.gameObject : null, false);

        SetActive(capsulaPanel, true);

        if (tituloCapsula != null)
            tituloCapsula.text = "CÁPSULA RADIAL · SEQUÍA";

        if (instruccionesCapsula != null)
            instruccionesCapsula.text =
                "Selecciona 3 palabras para preparar un mensaje seguro para la comunidad.";

        ActualizarContadorCapsula();
        currentStep = StoryStep.SeleccionCapsula;
    }

    private void AlternarPalabra(int indice)
    {
        if (currentStep != StoryStep.SeleccionCapsula)
            return;

        if (indice < 0 || indice >= palabrasSeleccionadas.Length)
            return;

        if (palabrasSeleccionadas[indice])
        {
            palabrasSeleccionadas[indice] = false;
            cantidadSeleccionada--;
        }
        else
        {
            if (cantidadSeleccionada >= 3)
            {
                if (instruccionesCapsula != null)
                    instruccionesCapsula.text =
                        "Ya seleccionaste 3 palabras. Deselecciona una para elegir otra.";

                return;
            }

            palabrasSeleccionadas[indice] = true;
            cantidadSeleccionada++;
        }

        ActualizarAparienciaPalabras();
        ActualizarContadorCapsula();
    }

    private void ActualizarContadorCapsula()
    {
        if (contadorCapsula != null)
            contadorCapsula.text =
                "PALABRAS SELECCIONADAS: " + cantidadSeleccionada + "/3";

        if (confirmarCapsula != null)
            confirmarCapsula.interactable = cantidadSeleccionada == 3;
    }

    private void ActualizarAparienciaPalabras()
    {
        Button[] botones =
        {
            palabraTapados, palabraFugas, palabraOficial,
            palabraAcaparar, palabraElSol
        };

        for (int i = 0; i < botones.Length; i++)
        {
            if (botones[i] == null)
                continue;

            Image imagen = botones[i].GetComponent<Image>();

            if (imagen != null)
            {
                imagen.color = palabrasSeleccionadas[i]
                    ? colorPalabraSeleccionada
                    : coloresOriginales[i];
            }
        }
    }

    private void ConfirmarCapsula()
    {
        if (currentStep != StoryStep.SeleccionCapsula ||
            cantidadSeleccionada != 3)
            return;

        int correctas = 0;
        int dudosas = 0;

        for (int i = 0; i < palabrasSeleccionadas.Length; i++)
        {
            if (!palabrasSeleccionadas[i])
                continue;

            switch (i)
            {
                case 0:
                    correctas++;
                    SumarAfinidad(ref GameState.cSebas);
                    break;
                case 1:
                    correctas++;
                    SumarAfinidad(ref GameState.cCamila);
                    break;
                case 2:
                    correctas++;
                    SumarAfinidad(ref GameState.cChela);
                    break;
                case 3:
                    dudosas++;
                    SumarAfinidad(ref GameState.cChela);
                    break;
                case 4:
                    dudosas++;
                    SumarAfinidad(ref GameState.cSebas);
                    break;
            }
        }

        GameState.Sumar(ref GameState.PREP, correctas);
        GameState.Sumar(ref GameState.RIESGO, dudosas);

        PlayerPrefs.SetInt(C1_KEY, 1);
        PlayerPrefs.SetInt("Cap1C1Correctas", correctas);
        PlayerPrefs.SetInt("Cap1C1Dudosas", dudosas);

        GuardarEstado();

        SetActive(capsulaPanel, false);
        SetActive(dialoguePanel, true);
        MostrarSoloFondo(fondoLoteSeco);

        ShowSpeaker(
            "Lucía", lucia, Emotion.Pensativo,
            "La cápsula quedó preparada. Debemos cuidar el agua, compartir información confiable y evitar acciones que puedan provocar incendios."
        );

        MostrarSoloBotonContinuar();
        currentStep = StoryStep.FinCapsula;
    }

    private void CerrarCapsula()
    {
        if (currentStep != StoryStep.SeleccionCapsula)
            return;

        SetActive(capsulaPanel, false);
        SetActive(dialoguePanel, true);

        ShowSpeaker(
            "Narrador", null, Emotion.Normal,
            "La cápsula sigue pendiente. Cuando quieras, puedes volver a preparar el mensaje radial."
        );

        MostrarSoloBotonContinuar();
        currentStep = StoryStep.IntroduccionCapsula;
    }

    private void ReiniciarSeleccionCapsula()
    {
        for (int i = 0; i < palabrasSeleccionadas.Length; i++)
            palabrasSeleccionadas[i] = false;

        cantidadSeleccionada = 0;
        ActualizarAparienciaPalabras();
        ActualizarContadorCapsula();
    }

    private void SumarAfinidad(ref int afinidad)
    {
        afinidad = Mathf.Clamp(afinidad + 1, 0, 3);
    }

    private void MostrarCierreCapitulo1()
    {
        MostrarSoloFondo(fondoLoteSeco);

        ShowSpeaker(
            "Narrador", null, Emotion.Normal,
            "La transmisión sobre la sequía está lista. Pero en el lote seco queda una señal inquietante: el humo llega primero a quien no mira hacia arriba."
        );

        MostrarSoloBotonContinuar();
        Debug.Log("Capítulo 1 completado.");
    }

    // MÉTODOS COMPARTIDOS

    private void MostrarDosOpciones(string textoA, string textoB)
    {
        SetActive(dialoguePanel, true);
        SetActive(choicesPanel, true);

        SetActive(continueButton != null
            ? continueButton.gameObject : null, false);

        SetActive(consultarMapaButton != null
            ? consultarMapaButton.gameObject : null, false);

        SetButtonLabel(choiceA, textoA);
        SetButtonLabel(choiceB, textoB);

        SetActive(choiceA != null ? choiceA.gameObject : null, true);
        SetActive(choiceB != null ? choiceB.gameObject : null, true);
        SetActive(choiceC != null ? choiceC.gameObject : null, false);

        RestaurarPosicionesOpciones();
        DistribuirDosOpciones();
    }

    private void MostrarSoloBotonContinuar()
    {
        SetActive(dialoguePanel, true);
        SetActive(choicesPanel, false);

        SetActive(continueButton != null
            ? continueButton.gameObject : null, true);

        SetActive(consultarMapaButton != null
            ? consultarMapaButton.gameObject : null, false);

        SetButtonLabel(continueButton, "CONTINUAR");
        RestaurarPosicionesOpciones();
    }

    private void DistribuirDosOpciones()
    {
        if (choiceA == null || choiceB == null || choicesPanel == null)
            return;

        RectTransform a = choiceA.GetComponent<RectTransform>();
        RectTransform b = choiceB.GetComponent<RectTransform>();

        float anchoA = a.rect.width;
        float anchoB = b.rect.width;

        a.anchoredPosition = new Vector2(
            -(separacionDosOpciones / 2f + anchoA / 2f),
            posicionOriginalA.y + desplazamientoVerticalOpciones
        );

        b.anchoredPosition = new Vector2(
            separacionDosOpciones / 2f + anchoB / 2f,
            posicionOriginalB.y + desplazamientoVerticalOpciones
        );
    }

    private void RestaurarPosicionesOpciones()
    {
        if (choiceA != null)
            choiceA.GetComponent<RectTransform>().anchoredPosition =
                posicionOriginalA;

        if (choiceB != null)
            choiceB.GetComponent<RectTransform>().anchoredPosition =
                posicionOriginalB;

        if (choiceC != null)
            choiceC.GetComponent<RectTransform>().anchoredPosition =
                posicionOriginalC;
    }

    private void MostrarSoloFondo(GameObject fondoActivo)
    {
        SetActive(fondoRio, fondoRio == fondoActivo);
        SetActive(fondoTextoRio, fondoTextoRio == fondoActivo);
        SetActive(fondoRadioLadera, fondoRadioLadera == fondoActivo);
        SetActive(fondoTiendaChela, fondoTiendaChela == fondoActivo);
        SetActive(fondoLoteSeco, fondoLoteSeco == fondoActivo);
        SetActive(fondoCasaDonHernan, fondoCasaDonHernan == fondoActivo);
    }

    private void ShowSpeaker(
        string visibleName,
        CharacterVisuals visuals,
        Emotion emotion,
        string text)
    {
        if (speakerName != null)
            speakerName.text = visibleName;

        if (dialogueText != null)
            dialogueText.text = text;

        HideCharacters();

        if (visuals == null)
            return;

        Sprite sprite = visuals.GetSprite(emotion);

        if (sprite == null)
            sprite = visuals.normal;

        if (sprite == null)
        {
            Debug.LogWarning("No hay sprite asignado para " + visibleName);
            return;
        }

        Image target =
            visibleName.Contains("Sebas") ||
            visibleName.Contains("Camila")
                ? characterRight
                : characterLeft;

        if (target == null)
            return;

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

    private void GuardarEstado()
    {
        PlayerPrefs.Save();
        GameSaveSystem.SaveGameState();
    }

    private void GuardarPosicionesOriginales()
    {
        if (choiceA != null)
            posicionOriginalA =
                choiceA.GetComponent<RectTransform>().anchoredPosition;

        if (choiceB != null)
            posicionOriginalB =
                choiceB.GetComponent<RectTransform>().anchoredPosition;

        if (choiceC != null)
            posicionOriginalC =
                choiceC.GetComponent<RectTransform>().anchoredPosition;
    }

    private void PrepararBotonesCapsula()
    {
        Button[] botones =
        {
            palabraTapados, palabraFugas, palabraOficial,
            palabraAcaparar, palabraElSol
        };

        for (int i = 0; i < botones.Length; i++)
        {
            if (botones[i] == null)
                continue;

            Image imagen = botones[i].GetComponent<Image>();

            if (imagen != null)
                coloresOriginales[i] = imagen.color;
        }
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

        if (palabraTapados != null)
            palabraTapados.onClick.RemoveAllListeners();

        if (palabraFugas != null)
            palabraFugas.onClick.RemoveAllListeners();

        if (palabraOficial != null)
            palabraOficial.onClick.RemoveAllListeners();

        if (palabraAcaparar != null)
            palabraAcaparar.onClick.RemoveAllListeners();

        if (palabraElSol != null)
            palabraElSol.onClick.RemoveAllListeners();

        if (confirmarCapsula != null)
            confirmarCapsula.onClick.RemoveListener(ConfirmarCapsula);

        if (cerrarCapsula != null)
            cerrarCapsula.onClick.RemoveListener(CerrarCapsula);
    }
}
