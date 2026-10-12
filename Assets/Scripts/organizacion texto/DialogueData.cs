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

public enum RouterCondition
{
    Ninguna,
    PrepMayorOIgual,
    Panico,
    TieneLibreta,
    SinReservaAgua,
    Arriesgo
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

    [Header("Extras Cap 3")]
    public Sprite background;          // cambia el fondo al mostrar esta línea
    public int onEnterRiesgo;          // suma/resta RIESGO al entrar a la línea

    [Header("Salto condicional (línea invisible)")]
    public bool isRouter;
    public RouterCondition condition;
    public int conditionValue;
    public string nextLineIfTrue;

    [Header("Cápsula radial")]
    public bool isCapsule;
    public int capsulePick = 3;
    public CapsuleWord[] capsuleWords;

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


    public int cSebas;
    public int cCamila;
    public int cChela;

    public bool requiresLibreta;
    public string lockedHint;


}

[Serializable]
public class CapsuleWord
{
    public string text;
    public bool correct;
    public CharacterId likes;   // a quién "le gusta" esta palabra
}
