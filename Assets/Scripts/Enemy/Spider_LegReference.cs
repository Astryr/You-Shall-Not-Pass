using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider_LegReference : MonoBehaviour
{
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float contactPointRadius = .05f;

    public Vector3 ContactPoint()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, Mathf.Infinity,whatIsGround))
            return hitInfo.point;


        return transform.position;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.position, ContactPoint());
        Gizmos.DrawWireSphere(ContactPoint(),contactPointRadius);
    }
}
