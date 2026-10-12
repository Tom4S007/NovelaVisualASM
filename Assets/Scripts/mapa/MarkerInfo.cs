using UnityEngine;
using UnityEngine.UI;

public class MarkerInfo : MonoBehaviour
{
    [Header("Información de la ubicación")]
    public string nombreLugar;
    public string barrio;
    public string estado;
    public string evento;
    public string nivelRiesgo;

    [TextArea(3, 6)]
    public string descripcion;

    [TextArea(2, 4)]
    public string fuente;

    [Header("Tipo de marcador")]
    public bool esCapitulo;

    [Header("Escena del capítulo")]
    public string escenaCapitulo;

    [Header("Controlador de la tarjeta")]
    public MarkerInfoCardController cardController;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(OpenInfo);
        }
        else
        {
            Debug.LogWarning(
                "El marcador necesita un componente Button: "
                + gameObject.name
            );
        }
    }

    private void OpenInfo()
    {
        if (cardController != null)
        {
            cardController.ShowInfo(this);
        }
        else
        {
            Debug.LogWarning(
                "Falta asignar MarkerInfoCardController en "
                + gameObject.name
            );
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OpenInfo);
        }
    }
}
