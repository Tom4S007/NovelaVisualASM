using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuPrincipalController : MonoBehaviour
{
    // =========================================================
    // ELEMENTOS DEL MENÚ
    // =========================================================

    [Header("ELEMENTOS DEL MENU")]
    [SerializeField] private CanvasGroup logoUAO;
    [SerializeField] private CanvasGroup textoPresentacion;
    [SerializeField] private CanvasGroup titulo;
    [SerializeField] private CanvasGroup subTitulo;
    [SerializeField] private CanvasGroup prologo;
    [SerializeField] private CanvasGroup barra1;
    [SerializeField] private CanvasGroup barra2;


    // =========================================================
    // BOTONES
    // =========================================================

    [Header("BOTONES")]
    [SerializeField] private CanvasGroup iniciarJuegoButton;
    [SerializeField] private CanvasGroup salirJuegoButton;


    // =========================================================
    // CREDITOS / NOMBRES
    // =========================================================

    [Header("CREDITOS")]
    [SerializeField] private CanvasGroup nombre1;
    [SerializeField] private CanvasGroup nombre2;
    [SerializeField] private CanvasGroup nombre3;
    [SerializeField] private CanvasGroup nombre4;
    [SerializeField] private CanvasGroup nombre5;


    // =========================================================
    // ANIMACION
    // =========================================================

    [Header("ANIMACION DEL MENU")]
    [SerializeField] private float duracionFade = 0.6f;

    [SerializeField] private float esperaEntreElementos = 0.15f;


    // =========================================================
    // ESCENA
    // =========================================================

    [Header("ESCENA DEL PROLOGO")]
    [SerializeField] private string escenaPrologo = "Prologo";


    // =========================================================
    // TRANSICION
    // =========================================================

    [Header("TRANSICION")]
    [SerializeField] private CanvasGroup transitionPanel;

    [SerializeField] private float duracionTransicion = 1f;


    // =========================================================
    // VARIABLES INTERNAS
    // =========================================================

    private bool menuTerminado = false;

    private bool cambiandoEscena = false;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        PrepararElementos();

        // Preparar la pantalla de transición
        if (transitionPanel != null)
        {
            transitionPanel.alpha = 0f;
            transitionPanel.interactable = false;
            transitionPanel.blocksRaycasts = false;
        }

        // Comenzar presentación
        StartCoroutine(MostrarMenu());
    }


    // =========================================================
    // PREPARAR ELEMENTOS
    // =========================================================

    private void PrepararElementos()
    {
        CanvasGroup[] elementos =
        {
            logoUAO,
            textoPresentacion,
            titulo,
            subTitulo,
            prologo,
            barra1,
            barra2,

            iniciarJuegoButton,
            salirJuegoButton,

            nombre1,
            nombre2,
            nombre3,
            nombre4,
            nombre5
        };


        foreach (CanvasGroup elemento in elementos)
        {
            if (elemento != null)
            {
                elemento.alpha = 0f;

                elemento.interactable = false;

                elemento.blocksRaycasts = false;
            }
        }
    }


    // =========================================================
    // MOSTRAR MENU
    // =========================================================

    private IEnumerator MostrarMenu()
    {
        // -----------------------------------------
        // LOGO UAO
        // -----------------------------------------

        yield return FadeIn(logoUAO);

        yield return new WaitForSeconds(
            esperaEntreElementos
        );


        // -----------------------------------------
        // TEXTO PRESENTACION
        // -----------------------------------------

        yield return FadeIn(textoPresentacion);

        yield return new WaitForSeconds(
            esperaEntreElementos
        );


        // -----------------------------------------
        // TITULO
        // -----------------------------------------

        yield return FadeIn(titulo);

        yield return new WaitForSeconds(
            esperaEntreElementos
        );


        // -----------------------------------------
        // SUBTITULO
        // -----------------------------------------

        yield return FadeIn(subTitulo);

        yield return new WaitForSeconds(
            esperaEntreElementos
        );


        // -----------------------------------------
        // PROLOGO
        // -----------------------------------------

        yield return FadeIn(prologo);

        yield return new WaitForSeconds(
            esperaEntreElementos
        );


        // -----------------------------------------
        // BARRAS
        // -----------------------------------------

        yield return FadeIn(barra1);

        yield return new WaitForSeconds(
            esperaEntreElementos
        );

        yield return FadeIn(barra2);

        yield return new WaitForSeconds(
            0.25f
        );


        // -----------------------------------------
        // BOTON INICIAR
        // -----------------------------------------

        yield return FadeIn(iniciarJuegoButton);

        yield return new WaitForSeconds(
            0.15f
        );


        // -----------------------------------------
        // BOTON SALIR
        // -----------------------------------------

        yield return FadeIn(salirJuegoButton);

        yield return new WaitForSeconds(
            0.2f
        );


        // -----------------------------------------
        // CREDITOS
        // -----------------------------------------

        yield return FadeIn(nombre1);

        yield return FadeIn(nombre2);

        yield return FadeIn(nombre3);

        yield return FadeIn(nombre4);

        yield return FadeIn(nombre5);


        // -----------------------------------------
        // ACTIVAR BOTONES
        // -----------------------------------------

        ActivarBoton(iniciarJuegoButton);

        ActivarBoton(salirJuegoButton);


        menuTerminado = true;
    }


    // =========================================================
    // FADE IN
    // =========================================================

    private IEnumerator FadeIn(CanvasGroup elemento)
    {
        if (elemento == null)
            yield break;


        elemento.alpha = 0f;

        elemento.interactable = false;

        elemento.blocksRaycasts = false;


        float tiempo = 0f;


        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;


            elemento.alpha = Mathf.Lerp(
                0f,
                1f,
                tiempo / duracionFade
            );


            yield return null;
        }


        elemento.alpha = 1f;
    }


    // =========================================================
    // FADE OUT
    // =========================================================

    private IEnumerator FadeOut(CanvasGroup elemento)
    {
        if (elemento == null)
            yield break;


        float tiempo = 0f;

        float alphaInicial = elemento.alpha;


        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;


            elemento.alpha = Mathf.Lerp(
                alphaInicial,
                0f,
                tiempo / duracionFade
            );


            yield return null;
        }


        elemento.alpha = 0f;
    }


    // =========================================================
    // ACTIVAR BOTON
    // =========================================================

    private void ActivarBoton(CanvasGroup boton)
    {
        if (boton == null)
            return;


        boton.interactable = true;

        boton.blocksRaycasts = true;
    }


    // =========================================================
    // INICIAR JUEGO
    // =========================================================

    public void IniciarJuego()
    {
        // Evitar múltiples clics
        if (!menuTerminado)
            return;


        if (cambiandoEscena)
            return;


        cambiandoEscena = true;


        StartCoroutine(
            CargarPrologo()
        );
    }


    // =========================================================
    // CARGAR PROLOGO
    // =========================================================

    private IEnumerator CargarPrologo()
    {
        // Bloquear botones
        if (iniciarJuegoButton != null)
        {
            iniciarJuegoButton.interactable = false;
            iniciarJuegoButton.blocksRaycasts = false;
        }


        if (salirJuegoButton != null)
        {
            salirJuegoButton.interactable = false;
            salirJuegoButton.blocksRaycasts = false;
        }


        // Pequeña pausa antes de comenzar
        yield return new WaitForSeconds(
            0.15f
        );


        // -----------------------------------------
        // FADE A NEGRO
        // -----------------------------------------

        if (transitionPanel != null)
        {
            yield return StartCoroutine(
                FadeToBlack()
            );


            // Pequeña pausa con pantalla negra
            yield return new WaitForSeconds(
                0.25f
            );
        }


        // -----------------------------------------
        // CARGAR PROLOGO
        // -----------------------------------------

        SceneManager.LoadScene(
            escenaPrologo
        );
    }


    // =========================================================
    // FADE A NEGRO
    // =========================================================

    private IEnumerator FadeToBlack()
    {
        float tiempo = 0f;


        transitionPanel.alpha = 0f;

        transitionPanel.blocksRaycasts = true;


        while (tiempo < duracionTransicion)
        {
            tiempo += Time.deltaTime;


            transitionPanel.alpha = Mathf.Lerp(
                0f,
                1f,
                tiempo / duracionTransicion
            );


            yield return null;
        }


        transitionPanel.alpha = 1f;
    }


    // =========================================================
    // SALIR DEL JUEGO
    // =========================================================

    public void SalirJuego()
    {
        Debug.Log(
            "Saliendo del juego..."
        );


#if UNITY_EDITOR

        // Si estamos dentro del Editor,
        // detenemos Play Mode.

        EditorApplication.isPlaying = false;

#else

        // Si es un juego compilado,
        // cerramos la aplicación.

        Application.Quit();

#endif
    }
}