using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Sequencer : MonoBehaviour
{
    [SerializeField] TextTyper typer;
    [SerializeField] TextAsset dialogo;
    [SerializeField] InputActionReference continueAction;
    private string[] lines;
    private bool skip;

    void OnEnable()
    {
        if (continueAction == null) return;
        continueAction.action.performed += OnContinue;
        continueAction.action.Enable();
    }

    void OnDisable()
    {
        if (continueAction == null) return;
        continueAction.action.performed -= OnContinue;
        continueAction.action.Disable();
    }

    void OnContinue(InputAction.CallbackContext ctx)
    {
        skip = true;
    }

    void Start()
    {
        if (typer == null || dialogo == null || continueAction == null)
        {
            Debug.LogError("Sequencer: falta asignar typer, dialogo o continueAction.", this);
            return;
        }
        lines = dialogo.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        foreach (string line in lines)
        {
            skip = false;
            Coroutine typing = StartCoroutine(typer.TypeText(line));
            yield return new WaitUntil(() => typer.isDone || skip);

            if (skip)
            {
                StopCoroutine(typing);
                typer.textUI.text = line;
                typer.isDone = true;
            }

            skip = false;
            yield return new WaitUntil(() => skip);
            yield return new WaitForSeconds(0.3f);
        }
    }
}
