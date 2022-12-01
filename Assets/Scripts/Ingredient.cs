using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    private bool can_drag = true;
    private Vector3 mOffset;
    private float mZCoord;
    public GameObject match;
    private bool has_collided = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    private void OnMouseDown()
    {
        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        mOffset = gameObject.transform.position - GetMouseAsWorldPoint();
    }

    private Vector3 GetMouseAsWorldPoint()
    {
        Vector3 MousePoint = Input.mousePosition;
        MousePoint.z = mZCoord;
        return Camera.main.ScreenToWorldPoint(MousePoint);
    }

    private void OnMouseDrag()
    {
        if (can_drag)
            transform.position = GetMouseAsWorldPoint() + mOffset;
    }
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("collided");
        foreach (ContactPoint contact in collision.contacts)
        {
            var colName = contact.thisCollider.name;
            //Debug.Log(colName);
        }
        if (collision.gameObject == match)
        {
            has_collided = true;
        }
    }
}
