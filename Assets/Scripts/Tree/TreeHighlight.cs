using UnityEngine;

public class TreeHighlight : MonoBehaviour
{
    bool isHighlighted;

    // Simple flag for now - visual feedback handled by MMFeedbacks on hit
    public bool IsHighlighted => isHighlighted;

    public void SetHighlighted(bool highlighted)
    {
        isHighlighted = highlighted;
    }
}
