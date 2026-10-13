using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MarkerInfoCardController : MonoBehaviour
{
    [Header("Referencias del mapa")]
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

    [Header("Botones")]
    public Button viewChapterButton;

    [Header("Posicionamiento")]
    public Vector2 offset = new Vector2(25f, -25f);
    public float padding = 10f;

    private RectTransform cardRect;
    private string currentChapterScene = "";

    private void Awake()
    {
        cardRect = card;

        if (card != null)
            card.gameObject.SetActive(false);

        if (viewChapterButton != null)
        {
            viewChapterButton.onClick.AddListener(OpenChapter);
            viewChapterButton.gameObject.SetActive(false);
        }
    }

    public void ShowInfo(MarkerInfo marker)
    {
        if (marker == null || card == null || mapViewport == null)
        {
            Debug.LogWarning(
                "MarkerInfoCardController: faltan referencias.");
            return;
        }

        SetField(TitleText, marker.nombreLugar, true);
        SetField(DescriptionText, marker.descripcion, true);

        if (marker.esCapitulo)
        {
            SetField(StateText, "", false);
            SetField(NeighborhoodText, "", false);
            SetField(EventText, "", false);
            SetField(RiskText, "", false);
            SetField(SourceText, "", false);

            currentChapterScene = marker.escenaCapitulo;

            if (viewChapterButton != null)
            {
                viewChapterButton.gameObject.SetActive(true);
                viewChapterButton.interactable =
                    !string.IsNullOrEmpty(currentChapterScene);
            }
        }
        else
        {
            SetField(StateText, marker.estado, true);
            SetField(
                NeighborhoodText,
                "Barrio: " + marker.barrio, true);
            SetField(
                EventText,
                "Evento: " + marker.evento, true);
            SetField(
                RiskText,
                "Nivel de riesgo: " + marker.nivelRiesgo, true);
            SetField(
                SourceText,
                "Fuente: " + marker.fuente, true);

            currentChapterScene = "";

            if (viewChapterButton != null)
                viewChapterButton.gameObject.SetActive(false);
        }

        card.gameObject.SetActive(true);

        Canvas.ForceUpdateCanvases();

        Vector3 localPosition =
            mapViewport.InverseTransformPoint(
                marker.transform.position);

        Vector2 target = new Vector2(
            localPosition.x + offset.x,
            localPosition.y + offset.y
        );

        Rect bounds = mapViewport.rect;

        float halfWidth =
            cardRect.rect.width * cardRect.localScale.x / 2f;

        float halfHeight =
            cardRect.rect.height * cardRect.localScale.y / 2f;

        float minX = bounds.xMin + halfWidth + padding;
        float maxX = bounds.xMax - halfWidth - padding;
        float minY = bounds.yMin + halfHeight + padding;
        float maxY = bounds.yMax - halfHeight - padding;

        target.x = minX <= maxX
            ? Mathf.Clamp(target.x, minX, maxX)
            : bounds.center.x;

        target.y = minY <= maxY
            ? Mathf.Clamp(target.y, minY, maxY)
            : bounds.center.y;

        cardRect.anchoredPosition = target;
    }

    private void SetField(
        TMP_Text textField,
        string value,
        bool visible)
    {
        if (textField == null)
            return;

        textField.text = value;
        textField.gameObject.SetActive(visible);
    }

    public void CloseInfo()
    {
        if (card != null)
            card.gameObject.SetActive(false);
    }

    public void OpenChapter()
    {
        if (string.IsNullOrEmpty(currentChapterScene))
        {
            Debug.LogWarning(
                "Este capítulo no tiene una escena asignada.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(currentChapterScene))
        {
            Debug.LogError(
                "No se puede cargar la escena: "
                + currentChapterScene
                + ". Verifica que esté incluida en Build Profiles.");
            return;
        }

        SceneManager.LoadScene(currentChapterScene);
    }

    private void OnDestroy()
    {
        if (viewChapterButton != null)
            viewChapterButton.onClick.RemoveListener(OpenChapter);
    }
}

