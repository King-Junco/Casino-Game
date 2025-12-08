using UnityEngine;
using UnityEngine.UI;

public class Betting : MonoBehaviour
{
    [SerializeField] private int bet = 0;
    [SerializeField] public Text betText;
    private bool isUpgraded = false;
    private float upgradeMultiplier = 2f;
    private Material[] originalMaterials;
    [SerializeField] private Material upgradedMaterial;
    public int payOut(int bet, int mult)
    {
        return bet * mult;
    }

    public int GetTopFace()
    {
        Vector3[] axes = new Vector3[] { transform.up, -transform.up, transform.forward, -transform.forward, transform.right, -transform.right };
        int bestIdx = 0;
        float bestDot = -1f;
        int mapped = -1;
        for (int i = 0; i < axes.Length; i++)
        {
            float d = Vector3.Dot(axes[i], Vector3.up);
            if (d > bestDot)
            {
                bestDot = d;
                bestIdx = i;
            }
        }

        int[] defaultMap = new int[] { 1, 6, 2, 5, 3, 4 };
            mapped = defaultMap[bestIdx];
        return mapped;
    }

    public void UpdateBetText()
    {
        betText.text = "Bet: $" + bet.ToString();
    }

    public void Upgrade(Material mat = null, float multiplier = 2f)
    {
        if (isUpgraded) return;
        isUpgraded = true;
        upgradeMultiplier = multiplier;

        if (mat != null)
        {
            var rend = GetComponent<Renderer>();
            if (rend != null)
            {
                // store original materials so upgrade can be reverted if needed
                originalMaterials = rend.materials;
                var mats = rend.materials;
                for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                rend.materials = mats;
            }
        }
        else if (upgradedMaterial != null)
        {
            var rend = GetComponent<Renderer>();
            if (rend != null)
            {
                originalMaterials = rend.materials;
                var mats = rend.materials;
                for (int i = 0; i < mats.Length; i++) mats[i] = upgradedMaterial;
                rend.materials = mats;
            }
        }
    }

}
