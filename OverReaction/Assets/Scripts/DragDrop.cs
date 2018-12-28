using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragDrop : MonoBehaviour {
    // Tells whether the card is being dragged
    private bool dragging = false;
    // Tells the index of the card in the controller this card represents
    public int index;
	// Use this for initialization
	void Start () {
	}

    // Called whenever the card is clicked
    void OnMouseDown()
    {
        dragging = true;
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))

            {
                ICardController cardController = Camera.main.GetComponent<MainController>().CardController;
                cardController.PlayCard(index);
                Destroy(gameObject);
            }
        }
    }

    // Called whenever the card stops being clicked
    private void OnMouseUp()
    {
        dragging = false;
    }
    // Update is called once per frame
    void Update()
    {
        

        // Check if the card is being dragged
        if (dragging)
        {
            // Makes a ray from the camera to the point the mouse clicked
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Makes a plane parallel with the face of the card
            Plane dPlane = new Plane(Vector3.forward, transform.position);

            // the distance needs to be something to start with
            float distance = 0;

            // Find the distance the ray intersects with the ray
            if (dPlane.Raycast(ray, out distance))
            {
                // Move the center of the deck to where the mouse is pointing
                transform.position = ray.GetPoint(distance);
            }
        }
        // Check if the card is right clicked
        if(Input.GetMouseButtonDown(1))
        {
            
        }
    }
}
