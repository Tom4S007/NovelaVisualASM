using System;
using UnityEngine;

public enum CharacterId
{
    Narrador,
    Lucia,
    Sebas,
    Camila,
    Chela,
    DonHernan,
    Fuente7
}

[Serializable]
public class DialogueLine
{
    public string id;
    public CharacterId character;

    [TextArea(2, 5)]
    public string text;

    public Sprite expression;

    public string nextLineId;
    public bool hasChoices;
    public ChoiceData[] choices;
}

[Serializable]
public class ChoiceData
{
    public string text;
    public string nextLineId;

    public int prep;
    public int info;
    public int conf;
    public int riesgo;

    public bool emitRumor;
    public bool setReservaAgua;
    public bool setTieneLibreta;
    public bool setFranjaLimpia;
    public bool setSubioAApagar;
    public bool setPanico;
    public bool setArriesgo;
}
