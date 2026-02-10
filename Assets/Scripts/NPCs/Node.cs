using UnityEngine;

public class Node : MonoBehaviour
{
    [SerializeField]
    private Color nodeColor;


    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "pestNode.png", true, nodeColor);
    }

}
