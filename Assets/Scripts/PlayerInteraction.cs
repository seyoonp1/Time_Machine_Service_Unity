using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 1.5f;
    public LayerMask interactLayer;
    public Vector2 centerOffset;

    [Header("UI Settings")]
    public GameObject promptSpriteObject;

    private IInteractable currentInteractable;

    private void Update()
    {
        UpdatePromptUI();

        if (Keyboard.current == null)
        {
            
            return;
        }

        if (TimeSlotSelectorUI.IsMenuOpen)
        {
            return;
        }

        bool isTransitioning = ScreenFadeController.Instance != null && ScreenFadeController.Instance.IsTransitioning;
        if (isTransitioning)
        {
            return;
        }

        DialogueManager dialogueManager = DialogueManager.Instance;
        bool hasOpenDialogue = dialogueManager != null && dialogueManager.IsDialogueOpen;

        if (hasOpenDialogue)
        {
            if (Keyboard.current.fKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                dialogueManager.RequestAdvance();
            }
            return;
        }

        if (currentInteractable != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            currentInteractable.OnInteract();
        }
    }

    private void UpdatePromptUI()
    {
        bool isTransitioning = ScreenFadeController.Instance != null && ScreenFadeController.Instance.IsTransitioning;
        if (isTransitioning)
        {
            currentInteractable = null;
            if (promptSpriteObject != null && promptSpriteObject.activeSelf)
            {
                promptSpriteObject.SetActive(false);
            }
            return;
        }

        if (TimeSlotSelectorUI.IsMenuOpen)
        {
            currentInteractable = null;
            if (promptSpriteObject != null && promptSpriteObject.activeSelf)
            {
                promptSpriteObject.SetActive(false);
            }
            return;
        }

        DialogueManager dialogueManager = DialogueManager.Instance;
        if (dialogueManager != null && dialogueManager.IsDialogueOpen)
        {
            currentInteractable = null;
            if (promptSpriteObject != null && promptSpriteObject.activeSelf)
            {
                promptSpriteObject.SetActive(false);
            }
            return;
        }

        Vector2 interactionPos = (Vector2)transform.position + centerOffset;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(interactionPos, interactRange, interactLayer);

        Collider2D targetCollider = null;
        currentInteractable = null;
        IInteractable fallbackInteractable = null;
        Collider2D fallbackCollider = null;

        if (hitColliders.Length > 0)
        {
            foreach (Collider2D hit in hitColliders)
            {
                IInteractable interactable = hit.GetComponent<IInteractable>();
                if (interactable == null)
                {
                    continue;
                }

                BoxDialogueInteraction toyBoxInteraction = hit.GetComponent<BoxDialogueInteraction>();
                if (toyBoxInteraction != null)
                {
                    currentInteractable = toyBoxInteraction;
                    targetCollider = hit;
                    break;
                }

                if (fallbackInteractable == null)
                {
                    fallbackInteractable = interactable;
                    fallbackCollider = hit;
                }
            }

            if (currentInteractable == null)
            {
                currentInteractable = fallbackInteractable;
                targetCollider = fallbackCollider;
            }
        }

        if (currentInteractable != null && promptSpriteObject != null)
        {
            if (!promptSpriteObject.activeSelf)
            {
                promptSpriteObject.SetActive(true);
            }

            promptSpriteObject.transform.position = targetCollider.bounds.center;
            return;
        }

        if (promptSpriteObject != null && promptSpriteObject.activeSelf)
        {
            promptSpriteObject.SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 drawPos = transform.position + (Vector3)centerOffset;
        Gizmos.DrawWireSphere(drawPos, interactRange);
    }
}
