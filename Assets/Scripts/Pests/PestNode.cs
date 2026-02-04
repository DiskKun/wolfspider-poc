using UnityEngine;

public class PestNode : MonoBehaviour
{
    [SerializeField]
    private Color nodeColor;


    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "pestNode.png", true, nodeColor);
    }

}
