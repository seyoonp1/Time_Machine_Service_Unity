using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem; // ⭐ Input System 필수

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Components")]
    public GameObject dialoguePanel; 
    public TextMeshProUGUI nameText; 
    public TextMeshProUGUI dialogueText;
    public Image portraitImage; // (초상화 기능 사용 시)

    [Header("Settings")]
    public float typingSpeed = 0.05f;

    private Queue<string> sentences;
    private bool isTyping = false; 
    private string currentSentence; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        sentences = new Queue<string>();
    }

    void Start()
    {
        // 시작할 때 무조건 끄기
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    // ⭐ [추가됨] 매 프레임 입력을 감시합니다.
    void Update()
    {
        // 1. 대화창이 꺼져있으면 아무것도 안 함
        if (dialoguePanel == null || !dialoguePanel.activeSelf) return;

        // 2. 스페이스바 입력 감지 (Input System)
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            DisplayNextSentence();
        }
        
        // (선택사항) F키나 엔터키도 같이 쓰고 싶다면?
        // if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame)
        // {
        //     DisplayNextSentence();
        // }
    }

    public void StartDialogue(string name, string[] lines, Sprite portrait = null)
    {
        dialoguePanel.SetActive(true);
        nameText.text = name;

        // 초상화 처리
        if (portraitImage != null)
        {
            portraitImage.gameObject.SetActive(portrait != null);
            portraitImage.sprite = portrait;
        }
        
        sentences.Clear();
        foreach (string line in lines)
        {
            sentences.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        // 타이핑 중이면 즉시 완성 (스킵 기능)
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = currentSentence;
            isTyping = false;
            return;
        }

        // 문장이 없으면 종료
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentSentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentSentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
    }
}