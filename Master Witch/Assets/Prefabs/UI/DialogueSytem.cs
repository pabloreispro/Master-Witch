using System.Collections;
using UnityEngine;
using TMPro;
using Game.UI;
using DG.Tweening;

public class DialogueSystem : SingletonNetwork<DialogueSystem>
{
    public TextMeshProUGUI dialogueText, chefName;
    public float typingSpeed = 0.03f;
    
    
    public IEnumerator OpenDialogue()
    {
        yield return GameInterfaceManager.Instance.dialogueBox.transform.DOScale(1, 0.3f);
        yield return new WaitForSeconds(0.3f);
        yield return GameInterfaceManager.Instance.dialogueBox.transform.DOScale(0.5f, 0.2f);
        yield return new WaitForSeconds(0.2f);
        yield return GameInterfaceManager.Instance.dialogueBox.transform.DOScale(1, 0.2f);
        yield return new WaitForSeconds(0.2f);
    }
    public IEnumerator CloseDialogue()
    {
        yield return GameInterfaceManager.Instance.dialogueBox.transform.DOScale(0, 1);
        yield return new WaitForSeconds(0.5f);
    }
    // Corrotina para digitar o texto letra por letra
    public IEnumerator StartDialogue(string[] texts)
    {
        yield return StartCoroutine(OpenDialogue());
        foreach (var item in texts)
        {
            yield return StartCoroutine(StartDialogue(item, false, false));
        }
        yield return new WaitForSeconds(0.5f);
        dialogueText.text = "";
        yield return StartCoroutine(CloseDialogue());
    }
    public IEnumerator StartDialogue(string text, bool open = true, bool close = true)
    {
        if(open)
            yield return StartCoroutine(OpenDialogue());
        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        yield return new WaitForSeconds(0.5f);
        dialogueText.text = "";
        if(close)
            yield return StartCoroutine(CloseDialogue());
    }
}
