using UnityEngine;

public class Cap3TestPreset : MonoBehaviour
{
    public bool usarPreset = true;

    [Range(0, 10)] public int prep = 2;
    [Range(0, 10)] public int info = 2;
    [Range(0, 10)] public int conf = 5;
    [Range(0, 10)] public int riesgo = 4;

    public bool tieneLibreta;
    public bool reservaAgua;
    public bool panico;
    public bool arriesgo;

    private void Awake()
    {
        if (!usarPreset) return;

        GameState.NuevaPartida();
        GameState.PREP = prep;
        GameState.INFO = info;
        GameState.CONF = conf;
        GameState.RIESGO = riesgo;
        GameState.tieneLibreta = tieneLibreta;
        GameState.reservaAgua = reservaAgua;
        GameState.panico = panico;
        GameState.arriesgo = arriesgo;
    }
}