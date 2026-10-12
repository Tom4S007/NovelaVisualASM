
using UnityEngine;

public static class GameSaveSystem
{
    private const string PREFIX = "SenalLadera_";

    // Guardar todos los datos compartidos de la partida.
    public static void SaveGameState()
    {
        PlayerPrefs.SetInt(PREFIX + "PREP", GameState.PREP);
        PlayerPrefs.SetInt(PREFIX + "INFO", GameState.INFO);
        PlayerPrefs.SetInt(PREFIX + "CONF", GameState.CONF);
        PlayerPrefs.SetInt(PREFIX + "RIESGO", GameState.RIESGO);

        PlayerPrefs.SetInt(PREFIX + "cSebas", GameState.cSebas);
        PlayerPrefs.SetInt(PREFIX + "cCamila", GameState.cCamila);
        PlayerPrefs.SetInt(PREFIX + "cChela", GameState.cChela);

        PlayerPrefs.SetInt(PREFIX + "rumorEmitido",
            GameState.rumorEmitido ? 1 : 0);

        PlayerPrefs.SetInt(PREFIX + "reservaAgua",
            GameState.reservaAgua ? 1 : 0);

        PlayerPrefs.SetInt(PREFIX + "tieneLibreta",
            GameState.tieneLibreta ? 1 : 0);

        PlayerPrefs.SetInt(PREFIX + "franjaLimpia",
            GameState.franjaLimpia ? 1 : 0);

        PlayerPrefs.SetInt(PREFIX + "subioAApagar",
            GameState.subioAApagar ? 1 : 0);

        PlayerPrefs.SetInt(PREFIX + "panico",
            GameState.panico ? 1 : 0);

        PlayerPrefs.SetInt(PREFIX + "arriesgo",
            GameState.arriesgo ? 1 : 0);

        PlayerPrefs.SetString(
            PREFIX + "nombreJugador",
            GameState.nombreJugador
        );

        PlayerPrefs.SetInt(PREFIX + "partidaGuardada", 1);

        PlayerPrefs.Save();

        Debug.Log("Partida guardada correctamente.");
    }

    // Cargar los datos guardados.
    public static void LoadGameState()
    {
        if (PlayerPrefs.GetInt(PREFIX + "partidaGuardada", 0) == 0)
        {
            Debug.Log("No existe una partida guardada.");
            return;
        }

        GameState.PREP = PlayerPrefs.GetInt(PREFIX + "PREP", 2);
        GameState.INFO = PlayerPrefs.GetInt(PREFIX + "INFO", 2);
        GameState.CONF = PlayerPrefs.GetInt(PREFIX + "CONF", 5);
        GameState.RIESGO = PlayerPrefs.GetInt(PREFIX + "RIESGO", 4);

        GameState.cSebas = PlayerPrefs.GetInt(PREFIX + "cSebas", 0);
        GameState.cCamila = PlayerPrefs.GetInt(PREFIX + "cCamila", 0);
        GameState.cChela = PlayerPrefs.GetInt(PREFIX + "cChela", 0);

        GameState.rumorEmitido =
            PlayerPrefs.GetInt(PREFIX + "rumorEmitido", 0) == 1;

        GameState.reservaAgua =
            PlayerPrefs.GetInt(PREFIX + "reservaAgua", 0) == 1;

        GameState.tieneLibreta =
            PlayerPrefs.GetInt(PREFIX + "tieneLibreta", 0) == 1;

        GameState.franjaLimpia =
            PlayerPrefs.GetInt(PREFIX + "franjaLimpia", 0) == 1;

        GameState.subioAApagar =
            PlayerPrefs.GetInt(PREFIX + "subioAApagar", 0) == 1;

        GameState.panico =
            PlayerPrefs.GetInt(PREFIX + "panico", 0) == 1;

        GameState.arriesgo =
            PlayerPrefs.GetInt(PREFIX + "arriesgo", 0) == 1;

        GameState.nombreJugador =
            PlayerPrefs.GetString(PREFIX + "nombreJugador", "Lucía");

        Debug.Log("Partida cargada correctamente.");
    }

    // Comprobar si existe una partida guardada.
    public static bool HasSavedGame()
    {
        return PlayerPrefs.GetInt(PREFIX + "partidaGuardada", 0) == 1;
    }

    // Iniciar una partida nueva y guardar sus valores iniciales.
    public static void StartNewGame()
    {
        GameState.NuevaPartida();

        // Eliminar el progreso narrativo anterior.
        PlayerPrefs.DeleteKey("Cap1VolvioDelMapa");
        PlayerPrefs.DeleteKey("ConsultaSiloeCap1");
        PlayerPrefs.DeleteKey("InfoSiloeRecompensada");
        PlayerPrefs.DeleteKey("Cap1EntroAlMapa");
        PlayerPrefs.DeleteKey("Cap1D1");

        SaveGameState();

        Debug.Log("Nueva partida iniciada.");
    }

    // Borrar todos los datos guardados de Señal de Ladera.
    public static void DeleteSavedGame()
    {
        PlayerPrefs.DeleteKey(PREFIX + "PREP");
        PlayerPrefs.DeleteKey(PREFIX + "INFO");
        PlayerPrefs.DeleteKey(PREFIX + "CONF");
        PlayerPrefs.DeleteKey(PREFIX + "RIESGO");

        PlayerPrefs.DeleteKey(PREFIX + "cSebas");
        PlayerPrefs.DeleteKey(PREFIX + "cCamila");
        PlayerPrefs.DeleteKey(PREFIX + "cChela");

        PlayerPrefs.DeleteKey(PREFIX + "rumorEmitido");
        PlayerPrefs.DeleteKey(PREFIX + "reservaAgua");
        PlayerPrefs.DeleteKey(PREFIX + "tieneLibreta");
        PlayerPrefs.DeleteKey(PREFIX + "franjaLimpia");
        PlayerPrefs.DeleteKey(PREFIX + "subioAApagar");
        PlayerPrefs.DeleteKey(PREFIX + "panico");
        PlayerPrefs.DeleteKey(PREFIX + "arriesgo");
        PlayerPrefs.DeleteKey(PREFIX + "nombreJugador");
        PlayerPrefs.DeleteKey(PREFIX + "partidaGuardada");

        PlayerPrefs.DeleteKey("Cap1VolvioDelMapa");
        PlayerPrefs.DeleteKey("ConsultaSiloeCap1");
        PlayerPrefs.DeleteKey("InfoSiloeRecompensada");
        PlayerPrefs.DeleteKey("Cap1EntroAlMapa");
        PlayerPrefs.DeleteKey("Cap1D1");

        PlayerPrefs.Save();

        GameState.NuevaPartida();

        Debug.Log("Datos guardados eliminados.");
    }
}
