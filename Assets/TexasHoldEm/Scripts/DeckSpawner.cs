using UnityEngine;

public class DeckSpawner : MonoBehaviour
{
    public GameObject[] cardPrefabs;   // Assign 52 card prefabs
    public float baseY = 0.05f;        // Height of bottom card
    public float offsetY = 0.001f;     // Stack offset

    private Vector3 cardScale = new Vector3(0.5f, 0.5f, 0.02f);
    private Quaternion faceDownRotation = Quaternion.Euler(270, 0, 0);

    void Start()
    {
        SpawnDeck();
    }

    void SpawnDeck()
    {
        for (int i = 0; i < cardPrefabs.Length; i++)
        {
            Vector3 pos = transform.position + new Vector3(0, baseY + offsetY * i, 0);
            GameObject card = Instantiate(cardPrefabs[i], pos, faceDownRotation, transform);

            // Force correct scale
            card.transform.localScale = cardScale;
        }
    }
}

