using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using NUnit.Framework;
using System;

[RequireComponent(typeof(Rigidbody))]

public class DiceRoll : MonoBehaviour
{
    Rigidbody body;

    [SerializeField] private float maxRandomFloat, startRollingForce;

    [SerializeField] private float forceX, forceY, forceZ;

    [Header("Stop Check")]
    [Tooltip("How often (seconds) to check whether the dice has stopped moving. Lower = more responsive, higher = cheaper.")]
    [SerializeField] private float checkInterval = 0.2f;

    // small threshold to consider velocity/ang. velocity as stopped
    [SerializeField] private float stopEpsilon = 0.1f;

    private float checkTimer = 0f;
    private bool hasStopped = false;

    // starting transform for soft-reset
    private Vector3 startPosition;
    private Quaternion startRotation;

    // allow manager to set start transform for unlocked dice
    public void SetStartTransform(Vector3 pos, Quaternion rot)
    {
        startPosition = pos;
        startRotation = rot;
        // immediately apply the position so the die doesn't stay at origin
        transform.position = pos;
        transform.rotation = rot;
    }

    // Apply an upgrade: change material and set payout multiplier (default 2x)
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
    public int diceFaceNum;
    // Event invoked once when this die has stopped and the top face has been determined.
    public event Action<int, DiceRoll> OnDiceStopped;

    public TextMeshProUGUI dicetext;

    public Transform[] faces;

    [Header("Local Axis Face Mapping (fallback)")]
    [Tooltip("Mapping for local axes -> face value. Order: +up, -up, +forward, -forward, +right, -right")] 
    [SerializeField] private int[] localAxisFaceValues = new int[] { 1, 6, 2, 5, 3, 4 };
    [Header("Debug")]
    [Tooltip("Enable to log details about GetTopFace selection and draw gizmos for local axes.")]
    [SerializeField] private bool debugGetTopFace = false;
    [SerializeField] private float gizmoAxisLength = 0.5f;
    private int debugLastBestIdx = -1;
    public int CurrentFace { get; private set; }
    
    [Header("Upgrade")]
    [Tooltip("Material to apply when this die is upgraded (optional).")]
    [SerializeField] private Material upgradedMaterial;

    private Material[] originalMaterials;
    private bool isUpgraded = false;
    public bool IsUpgraded => isUpgraded;
    private float upgradeMultiplier = 2f;
    public float UpgradeMultiplier => upgradeMultiplier;
    // removed per-die roll counter; roll limit managed by DiceManager

    


private void Update()
    {
        // don't re-check if we've already detected stop
        if (hasStopped) return;

        // run stop checks only on an interval to reduce per-frame work
        checkTimer -= Time.deltaTime;
        if (checkTimer > 0f) return;

        // reset timer for next check
        checkTimer = checkInterval;

        if (IsStopped()&& body.isKinematic == false)
        {
            CurrentFace = GetTopFace();
            hasStopped = true;
            Debug.Log("Dice Stopped");
            diceFaceNum = CurrentFace;
            Debug.Log("Dice Result: " + diceFaceNum);
            UpdateUI();
            OnDiceStopped?.Invoke(diceFaceNum, this);
        }
    }

    private void Awake()
    {
        Initialize();
        checkTimer = 0f;
        hasStopped = false;
    }
    
 public void RollDice()
    {
       
        body.isKinematic = false;
        forceX = UnityEngine.Random.Range(0, maxRandomFloat);
        forceY = UnityEngine.Random.Range(0, maxRandomFloat);
        forceZ = UnityEngine.Random.Range(0, maxRandomFloat);
        // reset stop detection when a new roll starts
        hasStopped = false;
        diceFaceNum = 0;
        checkTimer = checkInterval;

        body.AddForce(Vector3.up * startRollingForce);
        body.AddTorque(forceX, forceY, forceZ);
    }

    private void Initialize()
    {
        
        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        // record start transform for soft reset
        startPosition = transform.position;
        startRotation = transform.rotation;
        // randomize initial rotation slightly for variety
        transform.rotation = new Quaternion(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), 0);
    }

    // Soft-reset this die to its starting transform and stop motion
    public void ResetToStart()
    {
        if (body == null) body = GetComponent<Rigidbody>();

        // stop physics
        body.isKinematic = true;
        // reset transform
        transform.position = startPosition;
        transform.rotation = startRotation;

        // clear velocities
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;

        // reset internal state
        hasStopped = true;
        checkTimer = 0f;
        diceFaceNum = 0;
        CurrentFace = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        dicetext.text = "Dice Result: "+ diceFaceNum;
    }

    private int GetTopFace()
    {

        // Fallback: determine which local axis is most aligned with world up
        Vector3[] axes = new Vector3[] { transform.up, -transform.up, transform.forward, -transform.forward, transform.right, -transform.right };
        int bestIdx = 0;
        float bestDot = -1f;
        for (int i = 0; i < axes.Length; i++)
        {
            float d = Vector3.Dot(axes[i], Vector3.up);
            if (d > bestDot)
            {
                bestDot = d;
                bestIdx = i;
            }
        }
        // remember for gizmo/debug
        debugLastBestIdx = bestIdx;

        // Use user-provided mapping if available, otherwise fall back to default mapping for a standard die
        int mapped = -1;
        if (localAxisFaceValues != null && localAxisFaceValues.Length == 6)
        {
            mapped = localAxisFaceValues[bestIdx];
        }
        else
        {
            int[] defaultMap = new int[] { 1, 6, 2, 5, 3, 4 };
            mapped = defaultMap[bestIdx];
        }

        if (debugGetTopFace)
        {
            Debug.Log($"GetTopFace fallback: bestIdx={bestIdx} bestDot={bestDot:F3} axis={axes[bestIdx]} mappedFace={mapped}");
        }

        return mapped;
    }

    private void OnDrawGizmosSelected()
    {
        if (!debugGetTopFace) return;
        // draw the six local axes so you can verify orientation in scene view
        Vector3 origin = transform.position;
        Vector3[] axes = new Vector3[] { transform.up, -transform.up, transform.forward, -transform.forward, transform.right, -transform.right };
        Color[] colors = new Color[] { Color.yellow, Color.magenta, Color.cyan, Color.gray, Color.green, Color.red };
        for (int i = 0; i < axes.Length; i++)
        {
            Gizmos.color = (i == debugLastBestIdx) ? Color.white : colors[i % colors.Length];
            Gizmos.DrawLine(origin, origin + axes[i].normalized * gizmoAxisLength);
            Gizmos.DrawSphere(origin + axes[i].normalized * gizmoAxisLength, gizmoAxisLength * 0.06f);
        }
    }

    private bool IsStopped()
    {
        if (body == null) return true;
        float velSq = body.linearVelocity.sqrMagnitude;
        float angVelSq = body.angularVelocity.sqrMagnitude;
        float threshSq = stopEpsilon * stopEpsilon;
        return velSq <= threshSq && angVelSq <= threshSq;
    }

}