
using TMPro;
using UnityEngine;
using static UnityEditor.Profiling.FrameDataView;

public class MarkerInfoCardController : MonoBehaviour
{
    [Header("Referencias")]
    public RectTransform mapViewport;
    public RectTransform card;

    [Header("Textos de la tarjeta")]
    public TMP_Text TitleText;
    public TMP_Text StateText;
    public TMP_Text NeighborhoodText;
    public TMP_Text EventText;
    public TMP_Text RiskText;
    public TMP_Text DescriptionText;
    public TMP_Text SourceText;

    [Header("Posicionamiento")]
    public Vector2 offset = new Vector2(25f, -25f);
    public float padding = 10f;

    private RectTransform cardRect;

    private void Awake()
    {
        cardRect = card;
        card.gameObject.SetActive(false);
    }

    public void ShowInfo(MarkerInfo marker)
    {
        TitleText.text = marker.nombreLugar;
        StateText.text = marker.estado;
        NeighborhoodText.text = "Barrio: " + marker.barrio;
        EventText.text = "Evento: " + marker.evento;
        RiskText.text = "Nivel de riesgo: " + marker.nivelRiesgo;
        DescriptionText.text = marker.descripcion;
        SourceText.text = "Fuente: " + marker.fuente;

        card.gameObject.SetActive(true);

        // Convertir la posición del marcador al espacio del mapa.
        Vector3 localPosition =
            mapViewport.InverseTransformPoint(marker.transform.position);

        Vector2 target = new Vector2(
            localPosition.x + offset.x,
            localPosition.y + offset.y
        );

        Rect bounds = mapViewport.rect;

        float halfWidth = cardRect.rect.width
            * cardRect.localScale.x / 2f;

        float halfHeight = cardRect.rect.height
            * cardRect.localScale.y / 2f;

        // Mantener la tarjeta dentro del área visible del mapa.
        target.x = Mathf.Clamp(
            target.x,
            bounds.xMin + halfWidth + padding,
            bounds.xMax - halfWidth - padding
        );

        target.y = Mathf.Clamp(
            target.y,
            bounds.yMin + halfHeight + padding,
            bounds.yMax - halfHeight - padding
        );

        cardRect.anchoredPosition = target;
    }

    public void CloseInfo()
    {
        card.gameObject.SetActive(false);
    }
}

