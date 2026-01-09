using UnityEngine;
using MoreMountains.Feedbacks;

public class CuttableTree : MonoBehaviour
{
    public enum TreeState { Intact, BeingCut, Falling, Dead }

    [Header("Health")]
    public int maxHealth = 10;
    int currentHealth;

    [Header("Fall Settings")]
    public float leanDuration = 2.5f;      // Phase 1: slow lean with cracking
    public float fallDuration = 3.0f;      // Phase 2: actual fall
    public float settleDuration = 1.0f;    // Phase 3: settle/roll on ground
    public float leanAngle = 15f;          // Angle at end of lean phase
    public float fallAngle = 85f;          // Final angle
    public float cutHeight = 1.2f;         // Height where axe hits (for particles)

    [Header("Prefabs")]
    public GameObject stumpPrefab;
    public GameObject cutTreePrefab;       // Tree trunk without roots (falls)
    public GameObject logsPrefab;

    [Header("Feedbacks")]
    public MMF_Player hitFeedback;
    public MMF_Player perfectHitFeedback;
    public MMF_Player fallFeedback;

    public TreeState State { get; private set; } = TreeState.Intact;

    Vector3 accumulatedCutDirection;
    float fallTimer;
    Quaternion fallStartRotation;
    Quaternion fallLeanRotation;
    Quaternion fallEndRotation;
    Vector3 rotationAxis;
    TreeHighlight highlight;

    GameObject fallingTrunk;  // The spawned cut tree that actually falls

    public event System.Action OnTreeFelled;

    void Awake()
    {
        currentHealth = maxHealth;
        highlight = GetComponent<TreeHighlight>();
    }

    public bool CanBeCut => State == TreeState.Intact || State == TreeState.BeingCut;

    public void StartCutting()
    {
        if (State == TreeState.Intact)
            State = TreeState.BeingCut;
    }

    public void StopCutting()
    {
        if (State == TreeState.BeingCut)
            State = TreeState.Intact;
    }

    public void TakeDamage(int amount, Vector3 hitDirection, bool isPerfect)
    {
        if (!CanBeCut) return;

        currentHealth -= amount;
        accumulatedCutDirection += hitDirection;

        // Cut position: at cut height, offset to the side of the cut
        Vector3 cutPos = transform.position + Vector3.up * cutHeight;
        Vector3 hitDirFlat = hitDirection;
        hitDirFlat.y = 0;
        hitDirFlat.Normalize();
        Vector3 sideOffset = Vector3.Cross(Vector3.up, hitDirFlat);
        cutPos += sideOffset * 0.3f;

        if (isPerfect)
            perfectHitFeedback?.PlayFeedbacks(cutPos);
        else
            hitFeedback?.PlayFeedbacks(cutPos);

        if (currentHealth <= 0)
            StartFalling();
    }

    void StartFalling()
    {
        State = TreeState.Falling;
        fallTimer = 0f;

        if (highlight) highlight.SetHighlighted(false);

        Vector3 pos = transform.position;

        // Spawn stump immediately
        if (stumpPrefab)
            Instantiate(stumpPrefab, pos, Quaternion.identity);

        // Spawn cut tree (trunk without roots) - this will fall
        if (cutTreePrefab)
        {
            fallingTrunk = Instantiate(cutTreePrefab, pos, transform.rotation);
        }

        // Hide original tree visuals
        foreach (var renderer in GetComponentsInChildren<Renderer>())
            renderer.enabled = false;

        // Calculate fall direction
        Vector3 fallDir = accumulatedCutDirection.normalized;
        if (fallDir.sqrMagnitude < 0.01f)
            fallDir = -transform.forward;

        fallDir.y = 0;
        fallDir.Normalize();

        rotationAxis = Vector3.Cross(Vector3.up, fallDir);

        // Use the falling trunk's rotation as base
        Transform target = fallingTrunk ? fallingTrunk.transform : transform;
        fallStartRotation = target.rotation;
        fallLeanRotation = Quaternion.AngleAxis(leanAngle, rotationAxis) * fallStartRotation;
        fallEndRotation = Quaternion.AngleAxis(fallAngle, rotationAxis) * fallStartRotation;

        fallFeedback?.PlayFeedbacks(pos);
    }

    void Update()
    {
        if (State != TreeState.Falling) return;

        fallTimer += Time.deltaTime;
        float totalDuration = leanDuration + fallDuration + settleDuration;

        if (fallTimer >= totalDuration)
        {
            CompleteFall();
            return;
        }

        // Target is the falling trunk (or self if no cut prefab)
        Transform target = fallingTrunk ? fallingTrunk.transform : transform;

        // Phase 1: Lean
        if (fallTimer < leanDuration)
        {
            float t = fallTimer / leanDuration;
            float easedT = 1f - (1f - t) * (1f - t);
            target.rotation = Quaternion.Slerp(fallStartRotation, fallLeanRotation, easedT);
        }
        // Phase 2: Fall
        else if (fallTimer < leanDuration + fallDuration)
        {
            float t = (fallTimer - leanDuration) / fallDuration;
            float easedT = t * t;
            target.rotation = Quaternion.Slerp(fallLeanRotation, fallEndRotation, easedT);
        }
        // Phase 3: Settle
        else
        {
            float t = (fallTimer - leanDuration - fallDuration) / settleDuration;
            float wobble = Mathf.Sin(t * Mathf.PI * 2f) * (1f - t) * 2f;
            Quaternion settleRotation = Quaternion.AngleAxis(fallAngle + wobble, rotationAxis) * fallStartRotation;
            target.rotation = settleRotation;
        }
    }

    void CompleteFall()
    {
        State = TreeState.Dead;

        Vector3 pos = transform.position;

        if (logsPrefab)
        {
            Vector3 logPos = pos + accumulatedCutDirection.normalized * 2f;
            logPos.y = pos.y;
            Instantiate(logsPrefab, logPos, Quaternion.identity);
        }

        OnTreeFelled?.Invoke();

        if (fallingTrunk)
            Destroy(fallingTrunk);
        Destroy(gameObject);
    }

    public float GetHealthPercent() => (float)currentHealth / maxHealth;
}
