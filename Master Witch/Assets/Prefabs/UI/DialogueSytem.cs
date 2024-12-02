using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueSystem : SingletonNetwork<DialogueSystem>
{
    public TextMeshProUGUI dialogueText, chefName;
    public float typingSpeed = 0.03f;
    
    

    // Corrotina para digitar o texto letra por letra
    public IEnumerator StartDialogue(string text)
    {
        
        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        yield return new WaitForSeconds(0.5f);
        dialogueText.text = "";

    }
}
