using UnityEngine;

/// <summary>
/// Casts a short-range ray from the camera every frame to detect interactable
/// objects (purchase walls, etc.) directly in front of the player.
/// Press E to interact when something interactable is in range.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public Camera playerCamera;
    public float interactRange = 3f;

    [Header("UI Prompt (optional)")]
    [Tooltip("A small 'Press E' text element. Will auto show/hide based on raycast hits.")]
    public GameObject interactPromptUI;

    private WeaponPurchaseWall currentWall;

    void Update()
    {
        CheckForInteractable();

        if (currentWall != null && Input.GetKeyDown(KeyCode.E))
        {
            currentWall.AttemptInteraction(gameObject);
        }
    }

    void CheckForInteractable()
    {
        if (playerCamera == null)
        {
            currentWall = null;
            if (interactPromptUI != null) interactPromptUI.SetActive(false);
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            WeaponPurchaseWall wall = hit.transform.GetComponent<WeaponPurchaseWall>();

            if (wall != null)
            {
                currentWall = wall;
                if (interactPromptUI != null) interactPromptUI.SetActive(true);
                return;
            }
        }

        // Nothing interactable in view
        currentWall = null;
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
    }
}
