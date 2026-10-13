
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ZoomMapaController : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IScrollHandler
{
    [Header("Área visible del mapa")]
    [SerializeField] private RectTransform mapViewport;

    [Header("Contenido que se ampliará y moverá")]
    [SerializeField] private RectTransform mapContent;

    [Header("Botones de zoom")]
    [SerializeField] private Button botonZoomIn;
    [SerializeField] private Button botonZoomOut;

    [Header("Configuración del zoom")]
    [SerializeField] private float zoomMinimo = 1f;
    [SerializeField] private float zoomMaximo = 1.45f;
    [SerializeField] private float incrementoZoom = 0.05f;
    [SerializeField] private float velocidadZoom = 8f;
    [SerializeField] private float zoomRueda = 0.08f;

    [Header("Configuración del movimiento")]
    [SerializeField] private bool permitirArrastre = true;

    private Vector3 escalaInicial;
    private Vector3 escalaObjetivo;
    private Vector2 posicionInicial;
    private Canvas canvas;

    private void Awake()
    {
        if (mapContent == null)
        {
            Debug.LogError(
                "ZoomMapaController: asigna MapContent."
            );
            enabled = false;
            return;
        }

        if (mapViewport == null)
            mapViewport = GetComponent<RectTransform>();

        if (mapViewport == null)
        {
            Debug.LogError(
                "ZoomMapaController: asigna el RectTransform de MapViewport."
            );
            enabled = false;
            return;
        }

        escalaInicial = mapContent.localScale;
        escalaObjetivo = escalaInicial;
        posicionInicial = mapContent.anchoredPosition;

        canvas = GetComponentInParent<Canvas>();

        if (botonZoomIn != null)
            botonZoomIn.onClick.AddListener(Acercar);

        if (botonZoomOut != null)
            botonZoomOut.onClick.AddListener(Alejar);

        LimitarPosicion();
    }

    private void Update()
    {
        if (mapContent == null)
            return;

        mapContent.localScale = Vector3.Lerp(
            mapContent.localScale,
            escalaObjetivo,
            velocidadZoom * Time.unscaledDeltaTime
        );

        LimitarPosicion();
    }

    // Acercar con el botón +
    public void Acercar()
    {
        CambiarZoom(incrementoZoom);
    }

    // Alejar con el botón -
    public void Alejar()
    {
        CambiarZoom(-incrementoZoom);
    }

    // Zoom con la rueda del mouse
    public void OnScroll(PointerEventData eventData)
    {
        CambiarZoom(eventData.scrollDelta.y > 0
            ? zoomRueda
            : -zoomRueda);
    }

    private void CambiarZoom(float cantidad)
    {
        float escalaActual = escalaObjetivo.x / escalaInicial.x;

        float nuevaEscala = Mathf.Clamp(
            escalaActual + cantidad,
            zoomMinimo,
            zoomMaximo
        );

        escalaObjetivo = escalaInicial * nuevaEscala;

        // Evita que el mapa quede fuera del área visible.
        LimitarPosicion();
    }

    // Comienza el arrastre del mapa.
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!permitirArrastre)
            return;
    }

    // Mover el mapa mientras se mantiene presionado el botón izquierdo.
    public void OnDrag(PointerEventData eventData)
    {
        if (!permitirArrastre || mapContent == null)
            return;

        float escalaCanvas = canvas != null
            ? canvas.scaleFactor
            : 1f;

        mapContent.anchoredPosition +=
            eventData.delta / escalaCanvas;

        LimitarPosicion();
    }

    // Mantener el contenido dentro del área visible.
    private void LimitarPosicion()
    {
        if (mapContent == null || mapViewport == null)
            return;

        float factorEscala = escalaInicial.x != 0f
            ? mapContent.localScale.x / escalaInicial.x
            : 1f;

        float anchoContenido =
            mapContent.rect.width * factorEscala;

        float altoContenido =
            mapContent.rect.height * factorEscala;

        float anchoViewport = mapViewport.rect.width;
        float altoViewport = mapViewport.rect.height;

        float limiteX = Mathf.Max(
            0f,
            (anchoContenido - anchoViewport) / 2f
        );

        float limiteY = Mathf.Max(
            0f,
            (altoContenido - altoViewport) / 2f
        );

        Vector2 posicion = mapContent.anchoredPosition;

        posicion.x = Mathf.Clamp(
            posicion.x,
            posicionInicial.x - limiteX,
            posicionInicial.x + limiteX
        );

        posicion.y = Mathf.Clamp(
            posicion.y,
            posicionInicial.y - limiteY,
            posicionInicial.y + limiteY
        );

        mapContent.anchoredPosition = posicion;
    }

    // Volver al tamaño y posición originales.
    public void RestablecerZoom()
    {
        escalaObjetivo = escalaInicial;
        mapContent.anchoredPosition = posicionInicial;
    }

    private void OnDestroy()
    {
        if (botonZoomIn != null)
            botonZoomIn.onClick.RemoveListener(Acercar);

        if (botonZoomOut != null)
            botonZoomOut.onClick.RemoveListener(Alejar);
    }
}
