using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TextTyper : MonoBehaviour
{
    [SerializeField] public Text textUI;
    [SerializeField] private float baseTypingSpeed = 0.03f;
    public bool isDone;

    public IEnumerator TypeText(string message)
    {
        isDone = false;
        textUI.text = "";
        foreach (char letter in message)
        {
            textUI.text += letter;
            yield return new WaitForSeconds(baseTypingSpeed);
        }

        isDone = true;
    }
}