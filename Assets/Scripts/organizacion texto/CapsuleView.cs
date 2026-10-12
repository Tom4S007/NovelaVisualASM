using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CapsuleView : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button[] wordButtons;
    [SerializeField] private Button recordButton;
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(1f, 0.8f, 0.2f);

    private CapsuleWord[] words;
    private int[] order;
    private bool[] selected;
    private int pick;
    private Action onDone;

    public void Show(CapsuleWord[] capsuleWords, int pickCount, Action done)
    {
        words = capsuleWords;
        pick = pickCount;
        onDone = done;

        int n = Mathf.Min(words.Length, wordButtons.Length);
        order = new int[n];
        selected = new bool[n];
        for (int i = 0; i < n; i++) order[i] = i;

        // Mezclar para que las correctas no siempre salgan primero
        for (int i = n - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            int tmp = order[i]; order[i] = order[j]; order[j] = tmp;
        }

        for (int i = 0; i < wordButtons.Length; i++)
        {
            bool activo = i < n;
            wordButtons[i].gameObject.SetActive(activo);
            if (!activo) continue;

            int slot = i;
            wordButtons[i].onClick.RemoveAllListeners();
            wordButtons[i].onClick.AddListener(() => Toggle(slot));

            TMP_Text label = wordButtons[i].GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = words[order[i]].text;

            wordButtons[i].image.color = normalColor;
        }

        recordButton.onClick.RemoveAllListeners();
        recordButton.onClick.AddListener(Record);

        panel.SetActive(true);
        Refresh();
    }

    private void Toggle(int slot)
    {
        if (selected[slot])
        {
            selected[slot] = false;
        }
        else if (CountSelected() < pick)
        {
            selected[slot] = true;
        }

        wordButtons[slot].image.color = selected[slot] ? selectedColor : normalColor;
        Refresh();
    }

    private int CountSelected()
    {
        int c = 0;
        foreach (bool s in selected) if (s) c++;
        return c;
    }

    private void Refresh()
    {
        int c = CountSelected();
        if (counterText != null) counterText.text = "Elegidas: " + c + "/" + pick;
        recordButton.interactable = (c == pick);
    }

    private void Record()
    {
        for (int i = 0; i < selected.Length; i++)
        {
            if (!selected[i]) continue;

            CapsuleWord w = words[order[i]];

            if (w.correct) GameState.Sumar(ref GameState.PREP, 1);
            else GameState.Sumar(ref GameState.RIESGO, 1);

            switch (w.likes)
            {
                case CharacterId.Sebas: GameState.Sumar(ref GameState.cSebas, 1); break;
                case CharacterId.Camila: GameState.Sumar(ref GameState.cCamila, 1); break;
                case CharacterId.Chela: GameState.Sumar(ref GameState.cChela, 1); break;
            }
        }

        Debug.Log($"Cápsula grabada | PREP={GameState.PREP}, RIESGO={GameState.RIESGO}, " +
                  $"cSebas={GameState.cSebas}, cCamila={GameState.cCamila}, cChela={GameState.cChela}");

        panel.SetActive(false);
        onDone?.Invoke();
    }
}