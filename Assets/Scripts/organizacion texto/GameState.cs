
using UnityEngine;

public static class GameState
{
    // Variables generales: valores entre 0 y 10.
    public static int PREP = 2;
    public static int INFO = 2;
    public static int CONF = 5;
    public static int RIESGO = 4;

    // Afinidad personal.
    public static int cSebas = 0;
    public static int cCamila = 0;
    public static int cChela = 0;

    // Decisiones que afectan capítulos posteriores.
    public static bool rumorEmitido;
    public static bool reservaAgua;
    public static bool tieneLibreta;
    public static bool franjaLimpia;
    public static bool subioAApagar;
    public static bool panico;
    public static bool arriesgo;

    public static string nombreJugador = "Lucía";

    public static void Sumar(ref int valor, int cantidad)
    {
        valor = Mathf.Clamp(valor + cantidad, 0, 10);
    }

    public static void NuevaPartida()
    {
        PREP = 2;
        INFO = 2;
        CONF = 5;
        RIESGO = 4;

        cSebas = 0;
        cCamila = 0;
        cChela = 0;

        rumorEmitido = false;
        reservaAgua = false;
        tieneLibreta = false;
        franjaLimpia = false;
        subioAApagar = false;
        panico = false;
        arriesgo = false;

        nombreJugador = "Lucía";
    }
}

