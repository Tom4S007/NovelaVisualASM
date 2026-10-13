
using UnityEngine;

public class MapFilterController : MonoBehaviour
{
    [Header("Grupos de marcadores")]
    [SerializeField] private GameObject droughtMarkers;
    [SerializeField] private GameObject fireMarkers;
    [SerializeField] private GameObject landslideMarkers;
    [SerializeField] private GameObject floodMarkers;
    [SerializeField] private GameObject evacuationMarkers;
    [SerializeField] private GameObject chapterMarkers;

    private void Start()
    {
        // Al abrir el mapa, mostramos los capítulos.
        ShowChapters();
    }

    public void ShowAll()
    {
        SetGroup(droughtMarkers, true);
        SetGroup(fireMarkers, true);
        SetGroup(landslideMarkers, true);
        SetGroup(floodMarkers, true);
        SetGroup(evacuationMarkers, true);
        SetGroup(chapterMarkers, true);
    }

    public void ShowDrought()
    {
        HideAll();
        SetGroup(droughtMarkers, true);
    }

    public void ShowFire()
    {
        HideAll();
        SetGroup(fireMarkers, true);
    }

    public void ShowLandslides()
    {
        HideAll();
        SetGroup(landslideMarkers, true);
    }

    public void ShowFloods()
    {
        HideAll();
        SetGroup(floodMarkers, true);
    }

    public void ShowEvacuation()
    {
        HideAll();
        SetGroup(evacuationMarkers, true);
    }

    public void ShowChapters()
    {
        HideAll();
        SetGroup(chapterMarkers, true);
    }

    private void HideAll()
    {
        SetGroup(droughtMarkers, false);
        SetGroup(fireMarkers, false);
        SetGroup(landslideMarkers, false);
        SetGroup(floodMarkers, false);
        SetGroup(evacuationMarkers, false);
        SetGroup(chapterMarkers, false);
    }

    private void SetGroup(GameObject group, bool visible)
    {
        if (group != null)
            group.SetActive(visible);
    }
}
