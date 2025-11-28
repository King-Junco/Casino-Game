using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PortalGate : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Name of the scene to load (must be added in Build Settings)")]
    [SerializeField]
    private string sceneToLoad;

    [Header("Transition Settings")]
    [Tooltip("Delay before loading scene (seconds)")]
    [SerializeField]
    private float transitionDelay = 0.5f;

    [Header("Activation Settings")]
    [Tooltip("Distance at which portal activates")]
    [SerializeField]
    private float activationDistance = 5f;

    [Header("References")]
    [SerializeField]
    private Transform playerTransform;

    private PortalGate_Controller portalController;
    private bool isTransitioning = false;
    private bool portalActivated = false;

    void Start()
    {
        // Try to find the portal controller on parent or this object
        portalController = GetComponentInParent<PortalGate_Controller>();
        if (portalController == null)
        {
            portalController = GetComponent<PortalGate_Controller>();
        }

        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    void Update()
    {
        // Check distance to player and activate/deactivate portal
        if (playerTransform != null && portalController != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);

            // Activate portal when player gets close
            if (distance <= activationDistance && !portalActivated)
            {
                portalController.F_TogglePortalGate(true);
                portalActivated = true;
            }
            // Deactivate portal when player moves away
            else if (distance > activationDistance && portalActivated)
            {
                portalController.F_TogglePortalGate(false);
                portalActivated = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only teleport if player walks INTO the portal
        if (other.CompareTag("Player") && !isTransitioning && portalActivated)
        {
            StartCoroutine(LoadSceneWithDelay());
        }
    }

    private IEnumerator LoadSceneWithDelay()
    {
        isTransitioning = true;

        // Wait for the specified delay
        yield return new WaitForSeconds(transitionDelay);

        // Load the new scene
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("Scene name is not set in the Portal!");
        }
    }

    // Optional: Draw the activation radius in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationDistance);
    }
}