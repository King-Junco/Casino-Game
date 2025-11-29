using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class ScoringZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [SerializeField] private float multiplier = 1f; // 0 = loss, 1 = break even, 2+ = win

    [Header("Visual Feedback")]
    [SerializeField] private TextMeshPro multiplierText; // 3D text in world space
    [SerializeField] private Color zoneColor = Color.green;
    [SerializeField] private Material zoneMaterial;

    private PachinkoMachine machine;
    private MeshRenderer meshRenderer;

    void Start()
    {
        // Make sure the collider is a trigger
        GetComponent<Collider>().isTrigger = true;

        // Setup visual feedback
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer && zoneMaterial)
        {
            meshRenderer.material = zoneMaterial;
            meshRenderer.material.color = zoneColor;
        }

        // Display multiplier text
        if (multiplierText)
        {
            multiplierText.text = $"{multiplier}x";
        }
    }

    public void RegisterMachine(PachinkoMachine parentMachine)
    {
        machine = parentMachine;
    }

    public void OnBallEntered(PachinkoBall ball, float betAmount)
    {
        if (machine != null)
        {
            machine.OnBallScored(multiplier, betAmount);
        }

        // Visual feedback (optional)
        StartCoroutine(FlashZone());
    }

    System.Collections.IEnumerator FlashZone()
    {
        if (meshRenderer)
        {
            Color original = meshRenderer.material.color;
            meshRenderer.material.color = Color.white;
            yield return new WaitForSeconds(0.2f);
            meshRenderer.material.color = original;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // The ball script handles the scoring, but you can add extra effects here
        if (other.CompareTag("PachinkoBall"))
        {
            Debug.Log($"Ball entered {multiplier}x zone");
        }
    }
}