using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using DG.Tweening;
using System.Net.Security;
using UI;
using Game.UI;

public class NewCamController : SingletonNetwork<NewCamController>
{
    private Transform initialPosition;
    private Quaternion initialRotation;
    public Transform target;
    public bool finishIntro;
    public GameObject gameManagerObj;
    public DialogueSystem dialogueSystem;
    [Header("Movement Configs")]
    [SerializeField] private float speedMovement;
    public NetworkVariable<float> minXHorizontal;
    public NetworkVariable<float> maxXHorizontal;
    public NetworkVariable<float> minZ;
    public NetworkVariable<float> maxZ;

    [Header("Rotation Configs")]

    [SerializeField] private float speedRotation;
    [SerializeField] private float minAngle;
    [SerializeField] private float maxAngle;

    public Vector3 offset;

    void Start()
    {
        initialPosition = this.transform;
        initialRotation = this.transform.rotation;
    }
    void LateUpdate()
    {
        if (finishIntro)
        {
            if (target != null)
            {
                //LookAtTarget();
                FollowTarget();
            }
            else
            {
                transform.position = initialPosition.position;
            }
        }

    }

    void LookAtTarget()
    {
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
        float xRotation = Mathf.Clamp(targetRotation.eulerAngles.x, minAngle, maxAngle);
        Quaternion newTargetRotation = Quaternion.Euler(new Vector3(xRotation, transform.eulerAngles.y, targetRotation.eulerAngles.z));

        transform.rotation = Quaternion.Slerp(transform.rotation, newTargetRotation, speedRotation * Time.deltaTime);
    }

    void FollowTarget()
    {
        float boundX = Mathf.Clamp(target.position.x + offset.x, minXHorizontal.Value, maxXHorizontal.Value);
        float boundZ = Mathf.Clamp(target.position.z + offset.z, minZ.Value, maxZ.Value);
        Vector3 targetPosition = new Vector3(boundX, target.position.y + offset.y, boundZ);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, speedMovement * Time.deltaTime);

        transform.position = smoothedPosition;
    }

    public void IntroClient(bool tutorialIntro)
    {
        if (GameManager.Instance.skipIntro)
        {
            OnIntroFinish();
            return;
        }
        if (tutorialIntro)
            StartCoroutine(TutorialIntroCoroutine());
        else
            StartCoroutine(IntroCoroutine());
    }

    public IEnumerator IntroCoroutine()
    {
        yield return transform.DOMoveY(7f, 2f);
        yield return transform.DOMoveZ(4f, 2f);
        yield return transform.DORotate(new Vector3(14, 0, 0), 1f);
        gameManagerObj.GetComponent<DialogueSystem>().chefName.text = GameManager.Instance.chefsGO[0].name.Replace("(Clone)", "").Trim();
        yield return transform.DOMoveX(GameManager.Instance.chefsGO[0].transform.position.x, 2f).WaitForCompletion();
        yield return StartCoroutine(dialogueSystem.StartDialogue(GameManager.Instance.chefsGO[0].GetComponent<Dialogue>().dialogueText[Random.Range(0, GameManager.Instance.chefsGO[0].GetComponent<Dialogue>().dialogueText.Count)]));

        //yield return transform.DOMoveY(7f, 3f);

        //yield return transform.DOLookAt(GameManager.Instance.chefsGO[1].transform.position, 2f);
        gameManagerObj.GetComponent<DialogueSystem>().chefName.text = GameManager.Instance.chefsGO[1].name.Replace("(Clone)", "").Trim();
        yield return transform.DOMoveX(GameManager.Instance.chefsGO[1].transform.position.x, 2f).WaitForCompletion();
        yield return StartCoroutine(dialogueSystem.StartDialogue(GameManager.Instance.chefsGO[1].GetComponent<Dialogue>().dialogueText[Random.Range(0, GameManager.Instance.chefsGO[1].GetComponent<Dialogue>().dialogueText.Count)]));

        //yield return transform.DOMoveY(7f, 3f);
        //yield return transform.DOLookAt(GameManager.Instance.chefsGO[2].transform.position, 2f);
        gameManagerObj.GetComponent<DialogueSystem>().chefName.text = GameManager.Instance.chefsGO[2].name.Replace("(Clone)", "").Trim();
        yield return transform.DOMoveX(GameManager.Instance.chefsGO[2].transform.position.x, 2f).WaitForCompletion();
        yield return StartCoroutine(dialogueSystem.StartDialogue(GameManager.Instance.chefsGO[2].GetComponent<Dialogue>().dialogueText[Random.Range(0, GameManager.Instance.chefsGO[2].GetComponent<Dialogue>().dialogueText.Count)]));

        yield return transform.DOMove(initialPosition.position, 1f);
        yield return transform.DORotateQuaternion(initialRotation, 1f);
        OnIntroFinish();
    }
    IEnumerator TutorialIntroCoroutine()
    {
        yield return transform.DOMoveY(7f, 2f);
        yield return transform.DOMoveZ(4f, 2f);
        yield return transform.DORotate(new Vector3(14, 0, 0), 1f);
        gameManagerObj.GetComponent<DialogueSystem>().chefName.text = GameManager.Instance.chefsGO[0].name.Replace("(Clone)", "").Trim();
        yield return transform.DOMoveX(GameManager.Instance.chefsGO[0].transform.position.x, 2f).WaitForCompletion();
        yield return StartCoroutine(dialogueSystem.StartDialogue(TutorialController.Instance.ChefMessages));

        yield return transform.DOMove(initialPosition.position, 1f);
        yield return transform.DORotateQuaternion(initialRotation, 1f);
        OnIntroFinish();

    }
    void OnIntroFinish()
    {
        finishIntro = true;
        GameInterfaceManager.Instance.clock.SetActive(true);
        GameInterfaceManager.Instance.recipeSteps.SetActive(true);
        if (IsServer) StartCoroutine(TransitionController.Instance.TransitionMarketScene());
    }
}