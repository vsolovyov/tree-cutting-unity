using UnityEngine;
using UnityEngine.InputSystem;
using MoreMountains.Feedbacks;

public class TreeCutter : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRadius = 3f;
    public LayerMask treeLayer;

    [Header("References")]
    public CuttingMinigame minigame;
    public CuttingMinigameUI minigameUI;
    public PlayerMovement playerMovement;
    public FirstPersonLook firstPersonLook;

    [Header("Feedbacks")]
    public MMF_Player missFeedback;

    CuttableTree currentTarget;
    CuttableTree previousTarget;
    bool isCutting;

    void Update()
    {
        if (isCutting) return;

        FindNearestTree();
    }

    void FindNearestTree()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, treeLayer);

        CuttableTree nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            var tree = hit.GetComponentInParent<CuttableTree>();
            if (tree == null || !tree.CanBeCut) continue;

            float dist = Vector3.Distance(transform.position, tree.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = tree;
            }
        }

        SetTarget(nearest);
    }

    void SetTarget(CuttableTree tree)
    {
        if (currentTarget == tree) return;

        // Unhighlight previous
        if (currentTarget != null)
        {
            var highlight = currentTarget.GetComponent<TreeHighlight>();
            if (highlight) highlight.SetHighlighted(false);
        }

        currentTarget = tree;

        // Highlight new target
        if (currentTarget != null)
        {
            var highlight = currentTarget.GetComponent<TreeHighlight>();
            if (highlight) highlight.SetHighlighted(true);
        }

        // Update UI prompt
        if (minigameUI)
            minigameUI.ShowInteractPrompt(currentTarget != null);
        else
            Debug.LogWarning("TreeCutter: minigameUI is null!");
    }

    void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;

        if (isCutting)
        {
            AttemptHit();
        }
        else if (currentTarget != null)
        {
            StartCutting();
        }
    }

    void OnCancel(InputValue value)
    {
        if (!value.isPressed) return;

        if (isCutting)
            StopCutting();
    }

    void StartCutting()
    {
        if (currentTarget == null) return;

        isCutting = true;
        currentTarget.StartCutting();
        currentTarget.OnTreeFelled += OnTargetFelled;

        // Lock player
        if (playerMovement) playerMovement.enabled = false;
        if (firstPersonLook) firstPersonLook.enabled = false;

        // Activate minigame
        minigame.Activate();
        if (minigameUI) minigameUI.ShowMinigame(true);
    }

    void StopCutting()
    {
        if (!isCutting) return;

        isCutting = false;

        if (currentTarget != null)
        {
            currentTarget.StopCutting();
            currentTarget.OnTreeFelled -= OnTargetFelled;
        }

        // Unlock player
        if (playerMovement) playerMovement.enabled = true;
        if (firstPersonLook) firstPersonLook.enabled = true;

        // Deactivate minigame
        minigame.Deactivate();
        if (minigameUI) minigameUI.ShowMinigame(false);
    }

    void OnTargetFelled()
    {
        currentTarget.OnTreeFelled -= OnTargetFelled;
        currentTarget = null;
        StopCutting();
    }

    void AttemptHit()
    {
        if (currentTarget == null) return;

        var (result, damage) = minigame.TryHit();

        switch (result)
        {
            case CuttingMinigame.HitResult.Perfect:
                currentTarget.TakeDamage(damage, transform.forward, isPerfect: true);
                break;

            case CuttingMinigame.HitResult.Good:
                currentTarget.TakeDamage(damage, transform.forward, isPerfect: false);
                break;

            case CuttingMinigame.HitResult.Miss:
                missFeedback?.PlayFeedbacks(transform.position);
                break;
        }

        if (minigameUI)
            minigameUI.ShowHitResult(result, minigame.ComboCount);

        // Hide minigame immediately when tree starts falling
        if (currentTarget != null && currentTarget.State == CuttableTree.TreeState.Falling)
            StopCutting();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
