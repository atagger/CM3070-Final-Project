using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPath : MonoBehaviour
{
    [SerializeField] Color lineColor = Color.yellow;
    public List<Transform> nodes = new List<Transform>();
    private void OnDrawGizmos()
    {
        Gizmos.color = lineColor;

        Transform[] path_objs = GetComponentsInChildren<Transform>();

        // all the track nodes
        nodes = new List<Transform>();

        // cycle through all objects and add them to the nodes list
        for (int i = 0; i < path_objs.Length; i++)
        {
            // don't add the parent object
            if (path_objs[i] != transform)
            {
                // add everything else
                nodes.Add(path_objs[i]);
            }
        }

        // keep a list of current / previous so we can draw lines between all of them
        for (int i = 0; i < nodes.Count; i++)
        {
            Vector3 currentNode = nodes[i].position;
            Vector3 previousNode = Vector3.zero;

            if (i > 0)
            {
                previousNode = nodes[i - 1].position;
            }
            else if(i == 0 && nodes.Count > 1)
            {
                previousNode = nodes[nodes.Count - 1].position;
            }

            Gizmos.DrawLine(previousNode, currentNode);
            Gizmos.DrawWireSphere(currentNode, 10f);
        }

    }
}
