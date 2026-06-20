using UnityEngine;

public class HeadlampController : MonoBehaviour
{
    [Header("Headlamp Settings")]
    public Light headlampLight; // Drag a Spotlight component here in the inspector
    
    // Internal tracking for the state of the flashlight
    private bool isHeadlampOn = false;

    void Start()
    {
        // Start with the headlamp turned off
        if (headlampLight != null)
        {
            headlampLight.enabled = false;
        }
    }

    void Update()
    {
        // Toggle "F" for headlamp
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleHeadlamp();
        }
    }

    void ToggleHeadlamp()
    {
        if (headlampLight != null)
        {
            isHeadlampOn = !isHeadlampOn;
            headlampLight.enabled = isHeadlampOn;
            
            Debug.Log($"Headlamp toggled: {(isHeadlampOn ? "ON" : "OFF")}");
        }
        else
        {
            Debug.LogWarning("Headlamp Light component is not assigned in the Inspector!");
        }
    }

    // Public helper method so the Zombie AI can check if our light is shining on them
    public bool IsHeadlampActive()
    {
        return isHeadlampOn;
    }
}
