using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Una entrada de la lista de musica: que cancion suena en que escena.
/// </summary>
[Serializable]
public class SceneMusic
{
    [Tooltip("Nombre exacto de la escena (como aparece en Build Settings).")]
    public string sceneName;

    [Tooltip("Cancion de esta escena. Si esta vacia (y no es silencio), se ignora la entrada.")]
    public AudioClip clip;

    [Range(0f, 1f)]
    [Tooltip("Volumen de esta cancion (se multiplica por el volumen general de la musica).")]
    public float volume = 1f;

    [Tooltip("Marcar para que en esta escena no suene musica.")]
    public bool silencio;
}

/// <summary>
/// Sound manager singleton: sonido de clic (en cualquier escena), glitch de Fuente 7
/// y musica de fondo por escena.
///
/// Musica: la lista "Scene Music" define que cancion suena en cada escena.
/// Si una escena NO esta en la lista, sigue sonando la musica anterior
/// (asi el mapa continua con la cancion de la escena de la que vienes).
/// Si la escena pide la misma cancion que ya suena, no se reinicia.
///
/// Se crea solo al iniciar el juego y sobrevive a los cambios de escena.
/// Los slots se editan en Assets/Resources/SoundManager.prefab.
/// </summary>
public class SoundManager : MonoBehaviour
{
    const string PrefabName = "SoundManager";
    const string ClickResourceName = "ClickSound";

    public static SoundManager Instance { get; private set; }

    [Header("Clic")]
    [SerializeField] AudioClip clickClip;
    //[Range(0f, 1f)] [SerializeField] float clickVolume = 1f;

    [Header("Glitch (Fuente 7)")]
    [SerializeField] AudioClip glitchClip;
    //[Range(0f, 1f)] [SerializeField] float glitchVolume = 1f;

    [Range(0f, 1f)] [SerializeField] float SfxVol = 1f;

    
[Header("Musica de fondo")]
    [Tooltip("Cancion por escena. Las escenas que no aparezcan aqui siguen con la musica actual.")]
    [SerializeField] List<SceneMusic> sceneMusic = new List<SceneMusic>();
    [Range(0f, 1f)] [SerializeField] float musicVolume = 0.5f;
    [Tooltip("Duracion del fundido al cambiar de cancion entre escenas (segundos).")]
    [SerializeField] float sceneFade = 1f;

    AudioSource sfxSource;
    AudioSource musicSource;
    Coroutine fadeRoutine;
    float musicScale = 1f;

    float TargetMusicVolume => musicVolume * musicScale;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;

        var prefab = Resources.Load<GameObject>(PrefabName);
        if (prefab != null)
        {
            Instantiate(prefab);
        }
        else
        {
            new GameObject("SoundManager").AddComponent<SoundManager>();
        }
    }

void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = TargetMusicVolume;

        if (clickClip == null)
            clickClip = Resources.Load<AudioClip>(ClickResourceName);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }




    void Update()
    {
        if (ClickedThisFrame())
            PlayClick();
    }

    // ---------- Clic ----------

    public void PlayClick()
    {
        if (clickClip != null)
            sfxSource.PlayOneShot(clickClip, SfxVol);
    }

/// <summary>Sonido de glitch, usado cuando habla Fuente 7.</summary>
    public void PlayGlitch()
    {
        if (glitchClip != null)
            sfxSource.PlayOneShot(glitchClip, SfxVol);
    }

// ---------- Musica por escena ----------

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) return;
        ApplySceneMusic(scene.name);
    }

    void ApplySceneMusic(string sceneName)
    {
        if (sceneMusic == null) return;

        foreach (var entry in sceneMusic)
        {
            if (entry == null) continue;
            if (!string.Equals(entry.sceneName, sceneName, StringComparison.OrdinalIgnoreCase)) continue;

            if (entry.silencio)
                StopMusic(sceneFade);
            else if (entry.clip != null)
                PlayMusic(entry.clip, sceneFade, entry.volume);

            return;
        }

        // La escena no esta en la lista: sigue la musica que ya suena.
    }



    // ---------- Musica ----------

    /// <summary>Cambia la musica de fondo (con fundido). Usa fade = 0 para cambio inmediato.</summary>
public void PlayMusic(AudioClip clip, float fade = 0.5f, float volumeScale = 1f)
    {
        if (clip == null) return;

        musicScale = Mathf.Clamp01(volumeScale);

        // Misma cancion ya sonando: no se reinicia.
        if (musicSource.isPlaying && musicSource.clip == clip)
        {
            if (fadeRoutine == null)
                musicSource.volume = TargetMusicVolume;
            return;
        }

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(SwitchMusic(clip, fade));
    }

public void StopMusic(float fade = 0.5f)
    {
        if (!musicSource.isPlaying) return;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeOutAndStop(fade));
    }

public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        if (fadeRoutine == null)
            musicSource.volume = TargetMusicVolume;
    }

    public void SetClickVolume(float value)
    {
        SfxVol = Mathf.Clamp01(value);
    }

    public void MuteMusic(bool mute)
    {
        musicSource.mute = mute;
    }

IEnumerator SwitchMusic(AudioClip clip, float fade)
    {
        if (musicSource.isPlaying && fade > 0f)
            yield return FadeVolume(musicSource.volume, 0f, fade);

        musicSource.clip = clip;
        musicSource.volume = fade > 0f ? 0f : TargetMusicVolume;
        musicSource.Play();

        if (fade > 0f)
            yield return FadeVolume(0f, TargetMusicVolume, fade);

        fadeRoutine = null;
    }

IEnumerator FadeOutAndStop(float fade)
    {
        if (fade > 0f)
            yield return FadeVolume(musicSource.volume, 0f, fade);

        musicSource.Stop();
        musicSource.clip = null;
        musicSource.volume = TargetMusicVolume;
        fadeRoutine = null;
    }

    IEnumerator FadeVolume(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        musicSource.volume = to;
    }

    // ---------- Input ----------

    static bool ClickedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
        return false;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }
}
