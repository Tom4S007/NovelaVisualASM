using System;
using UnityEngine;

// Datos del guion del Capítulo 2. Se editan desde el Inspector.

public enum Cap2Personaje
{
    Narrador,
    Lucia,
    Sebas,
    Camila,
    Chela,
    DonHernan,
    Fuente7,
    VozDelRio
}

public enum Cap2Condicion
{
    Ninguna,
    RumorEmitido,
    FranjaLimpia,
    SubioAApagar,
    Panico,
    ReservaAgua,
    TieneLibreta,
    Arriesgo,
    SebasTieneMasConfianza,
    CamilaTieneMasConfianza,
    ChelaTieneMasConfianza
}

[Serializable]
public class Cap2Opcion
{
    [TextArea(1, 3)]
    public string texto;

    [Tooltip("Id de la línea a la que se salta al elegir esta opción.")]
    public string siguienteLinea;

    [Header("Números (positivo suma, negativo resta)")]
    public int prep;
    public int info;
    public int conf;
    public int riesgo;

    [Header("Confianza personal (se mantiene entre 0 y 3)")]
    public int cSebas;
    public int cCamila;
    public int cChela;

    [Header("Banderas (marcar para activarlas)")]
    public bool rumorEmitido;
    public bool subioAApagar;
    public bool panico;
    public bool arriesgo;
}

[Serializable]
public class Cap2Palabra
{
    public string texto;

    [Tooltip("Marcar si es una palabra correcta (suma PREP). Si no, suma RIESGO.")]
    public bool correcta;

    [Tooltip("A quién le gusta esta palabra (sube su confianza). Narrador = a nadie.")]
    public Cap2Personaje leGustaA;

    [TextArea(1, 3)]
    [Tooltip("Frase corta con la que reacciona ese personaje. Puede quedar vacía.")]
    public string reaccion;
}

[Serializable]
public class Cap2Linea
{
    [Tooltip("Nombre único de la línea, por ejemplo c2_01.")]
    public string id;

    public Cap2Personaje personaje;

    [Tooltip("Si lo llenas, se muestra este nombre en vez del normal.")]
    public string nombreVisible;

    [TextArea(2, 5)]
    [Tooltip("Puedes escribir {nombre} para que salga el nombre del jugador.")]
    public string texto;

    [Tooltip("Imagen del personaje. Si se deja vacía, no aparece ninguno.")]
    public Sprite expresion;

    [Tooltip("Id de la siguiente línea. Si queda vacío, el capítulo termina.")]
    public string siguienteLinea;

    [Header("Fondo y sonido (opcionales)")]
    public Sprite fondo;
    public AudioClip sonido;
    public AudioClip ambiente;

    [Range(0, 3)]
    [Tooltip("0 = sin glitch. Del 1 al 3 sube la intensidad.")]
    public int glitch;

    [Header("Decisión (de 2 a 5 opciones)")]
    public bool tieneOpciones;
    public Cap2Opcion[] opciones;

    [Header("Salto condicional (línea invisible)")]
    public bool esSalto;
    public Cap2Condicion condicion;
    [Tooltip("Marcar para que el salto ocurra cuando la condición NO se cumple.")]
    public bool invertir;
    [Tooltip("A dónde va si se cumple. Si no, va a 'Siguiente Linea'.")]
    public string siguienteSiCumple;

    [Header("Cápsula radial (las palabras salen como opciones)")]
    public bool esCapsula;
    public int palabrasAElegir = 3;
    public Cap2Palabra[] palabras;
}
