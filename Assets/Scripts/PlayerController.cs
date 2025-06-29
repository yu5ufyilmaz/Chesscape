using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private float _speed;
    public List<GameObject> placeholders = new List<GameObject>();
    [SerializeField] private InputAction position,press;
    
    private Vector2 initialPosition;
    private Vector2 currentPosition => position.ReadValue<Vector2>();
    private void Awake()
    {
        position.Enable();
        press.Enable();
        press.performed += _ => 
        {
            initialPosition = currentPosition;
        };
        press.canceled += _ => DetectSwipe();
    }
    
    private void DetectSwipe()
    {
        Vector2 swipeDelta = currentPosition - initialPosition;
        if (swipeDelta.magnitude > 0.1f) // Adjust threshold as needed
        {
            Debug.Log("Swipe detected: " + swipeDelta);
            // Handle swipe logic here
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize the speed of the player
        _speed = 5.0f;

        // Find all GameObjects with the tag "Placeholder" and add them to the list
        GameObject[] placeholderObjects = GameObject.FindGameObjectsWithTag("Placeholder");
        foreach (GameObject placeholder in placeholderObjects)
        {
            placeholders.Add(placeholder);
        }

        // Log the number of placeholders found
        Debug.Log("Number of placeholders found: " + placeholders.Count);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
