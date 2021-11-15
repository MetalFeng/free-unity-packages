using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Written By: Wang Hai Feng
/// Date Created: 10/8/2020
/// Last Edit: 04/08/2021 (Wang Hai Feng)
/// 
/// Contributors: Wang Hai Feng.
/// 
/// Dependencies: Rigidbody2D Component attached to GameObject, Canvas and EventSystem GameObjects in scene to prevent movement when tapping UI elements
/// 
/// TouchMovementController allows the attached GameObject with a Rigidbody to be controlled by touching and dragging
/// from any point on screen. The GameOject will move relative to the initial touch position. By default the movement
/// is restrained within the X and Y Axes and freezing the Rigidbody Z Position is highly recommended. This can be
/// modified in the Script to move the GameObject in a different set of Axes.
/// </summary>

public class AnchoredTouchMovement2D : MonoBehaviour
{
    #region References
    [Header("Component References")]

    [Tooltip("Rigidbody that will be used to move the GameObject.")]
    [SerializeField] private Rigidbody2D rigidBody = null;

    #endregion

    #region Touch Variables

    [Header("Touch Settings")]
    [Tooltip("Drag sensitivity when moving the attached GameObject.")]
    [SerializeField] private float dragSensitivity = 1.0f;
    [Tooltip("Set between 0 and 1. The lower the number, the bigger the delay of movement to a new position.")]
    [SerializeField] private float interpolar = 0.75f;  // Interpolar for the LERP used for smooth movement between positions
    [Tooltip("Positive Bounds on the world position X and Y Axes that limit the position of the attached GameObject.")]
    [SerializeField] private Vector2 positiveBounds = new Vector2(3.4f, 5.5f);
    [Tooltip("Negative Bounds on the world position X and Y Axes that limit the position of the attached GameObject.")]
    [SerializeField] private Vector2 negativeBounds = new Vector2(-3.4f, -5.5f);


    private Vector2 touchAnchor;    // Anchor point reference for each touch
    private Vector2 bodyAnchor;     // body position reference for each touch
    private bool isMovementAllowed = true;
    public bool isTouching { get; private set; }    // Boolean of whether the screen is being touched
    
    #endregion

    #region Touch Events
    [Header("Touch Events")]

    [Tooltip("Hook up methods that should be called on the start of a touch.")]
    public UnityEvent onTouchStart;

    [Tooltip("Hook up methods that should be called on the end of a touch.")]
    public UnityEvent onTouchEnd;
    #endregion

    void Start()
    {
        // Get rigidbody if it has not been assigned in the editor, and logs an error if there is rigidbody attached.
        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody2D>();
            
            if(rigidBody == null)
            {
                Debug.LogError("TouchMovementController will not work because " + gameObject.name + " does not have Rigidbody attached.");
                return;
            }
        }

        // Script starts with no touching.
        isTouching = false;

        // Start coroutine for input detection and control.
        StartCoroutine(DetectTouchControl());
    }

    // Runs coroutine to detect input when object is enabled and active.
    private void OnEnable()
    {
        StartCoroutine(DetectTouchControl());
    }

    public void FixedUpdate()
    {
        // Apply movement if isTouching is true
        if (isTouching)
        {
            Vector3 pointerPosition;

            #if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WEBGL || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX

                        pointerPosition = Input.mousePosition;

            #elif UNITY_IOS || UNITY_ANDROID

                        pointerPosition = Input.GetTouch(0);

            #endif

            // Get touchPosition in world point
            Vector2 newTouchPosition = Camera.main.ScreenToWorldPoint(new Vector3(pointerPosition.x, pointerPosition.y, Camera.main.nearClipPlane));

            // Calculate target position by adding the bodyAnchor and the direction vector gotten by subtracting newTouchPosition from touchAnchor
            // Multiply by drag sensitivity to alter the movement speed
            Vector2 targetBodyPosition = bodyAnchor + (newTouchPosition - touchAnchor) * dragSensitivity;
            
            // Clamp the targetPosition to the boundaries of the level
            float xPosition = Mathf.Clamp(targetBodyPosition.x, negativeBounds.x, positiveBounds.x);
            float yPosition = Mathf.Clamp(targetBodyPosition.y, negativeBounds.y, positiveBounds.y);
            targetBodyPosition = new Vector2(xPosition, yPosition);

            // Reset anchor points if position is at the edges of the level
            if (targetBodyPosition.x.Equals(negativeBounds.x) || targetBodyPosition.x.Equals(positiveBounds.x) || targetBodyPosition.y.Equals(negativeBounds.y) || targetBodyPosition.y.Equals(positiveBounds.y))
            {
                touchAnchor = newTouchPosition;
                bodyAnchor = targetBodyPosition;
            }

            // Move Player position and cache current position for velocity checking after rigidbody position update
            rigidBody.MovePosition(Vector2.Lerp(rigidBody.position, targetBodyPosition, interpolar));
        }
    }

    /// <summary>
    /// Detects if pointer is over a UI element like a pause button to prevent movement in such cases.
    /// </summary>
    /// <returns>Bool indicating if the pointer is over a UI element.</returns>
    private bool IsPointerOverUIObject()
    {
        #if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WEBGL || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
        return EventSystem.current.IsPointerOverGameObject();

        #elif UNITY_ANDROID || UNITY_IOS
        return EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId);

        #endif
    }


    /// <summary>
    /// Coroutine to detect input and setup movement anchor points
    /// </summary>
    private IEnumerator DetectTouchControl()
    {
        // Check and setup movement while it is allowed
        while (isMovementAllowed)
        {
            bool isScreenTouched = false;

            // Check if screen is touched or mouse is clicked, and set anchorSet to false if touch or click is ending.
            #if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WEBGL

            if (Input.GetMouseButton(0) && !IsPointerOverUIObject())
            {
                isScreenTouched = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isTouching = false;

                // Call onTouchEnd event to hook up any other desired behaviour
                if (onTouchEnd != null)
                    onTouchEnd.Invoke();
            }

            #elif UNITY_IOS || UNITY_ANDROID

            if (Input.touchCount > 0)
            {
                if (Input.touches[0].phase == TouchPhase.Began && !IsPointerOverUIObject())
                {
                    bool isScreenTouched = false;
                }
            }
            else if (Input.touches[0].phase == TouchPhase.Ended)
            {
                anchorSet = false;

                // Call onTouchEnd event to hook up any other desired behaviour
                if (onTouchEnd != null)
                    onTouchEnd.Invoke();
            }

            #endif

            if (isScreenTouched)
            {
                // Set anchor positions for touch and player body on beginning of click
                if (!isTouching)
                {
                    bodyAnchor = rigidBody.position;
                    touchAnchor = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
                    isTouching = true;

                    // Call onTouchStart event to hook up any other desired behaviour
                    if (onTouchStart != null)
                        onTouchStart.Invoke();
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// Disables movement by setting isMovementAllowed to false
    /// </summary>
    public void DisableMovement()
    {
        isMovementAllowed = false;
    }

    /// <summary>
    /// Enables movement by setting isMovementAllowed to true and starting the TouchControl coroutine
    /// </summary>
    public void EnableMovement()
    {
        isMovementAllowed = true;
        StartCoroutine(DetectTouchControl());
    }
}