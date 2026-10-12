
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VolverCapitulo1DesdeMapa : MonoBehaviour
{
    [SerializeField] private Button volverButton;

    private const string MAP_ENTRY_KEY = "Cap1EntroAlMapa";
    private const string MAP_RETURN_KEY = "Cap1VolvioDelMapa";

    private void Awake()
    {
        if (volverButton == null)
            volverButton = GetComponent<Button>();

        if (volverButton != null)
            volverButton.onClick.AddListener(VolverAlCapitulo);
    }

    private void Start()
    {
        // Mostrar este botón solo si llegamos desde el capítulo 1.
        bool vieneDelCapitulo =
            PlayerPrefs.GetInt(MAP_ENTRY_KEY, 0) == 1;

        if (volverButton != null)
            volverButton.gameObject.SetActive(vieneDelCapitulo);
    }

    private void VolverAlCapitulo()
    {
        if (PlayerPrefs.GetInt(MAP_ENTRY_KEY, 0) != 1)
            return;

        PlayerPrefs.SetInt(MAP_ENTRY_KEY, 0);
        PlayerPrefs.SetInt(MAP_RETURN_KEY, 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("03_Sequia");
    }

    private void OnDestroy()
    {
        if (volverButton != null)
            volverButton.onClick.RemoveListener(VolverAlCapitulo);
    }
}
