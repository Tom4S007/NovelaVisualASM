
using UnityEngine;

public class ConsultaMapaCapitulo1 : MonoBehaviour
{
    private const string SILOE_VISITED_KEY = "ConsultaSiloeCap1";

    // Este método se llamará desde el botón del marcador.
    public void ConsultarSiloe()
    {
        PlayerPrefs.SetInt(SILOE_VISITED_KEY, 1);
        PlayerPrefs.Save();

        Debug.Log("Siloé fue consultado desde el mapa.");
    }
}

