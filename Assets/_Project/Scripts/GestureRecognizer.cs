using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using PDollarGestureRecognizer;
using System.IO;
using System.Linq;
using UnityEngine.Events;

public class GestureRecognizer : MonoBehaviour {
    [Header("Controller Setup")]
    [SerializeField] ActionBasedController leftController;
    [SerializeField] ActionBasedController rightController;
    // FIXME the movement source should come from either controller
    [SerializeField] Transform movementSource;
    [SerializeField, Range(0.02f, 0.07f)] float positionThreshold = 0.05f;
    
    public GameObject cubePrefab;

    [Header("Gesture Training")]
    [SerializeField] bool creationMode = true;
    [SerializeField] string newGestureName;
    
    [Header("Gesture Recognition")]
    [SerializeField, Range(0.5f, 0.95f)] float recognitionThreshold = 0.9f;
    [SerializeField, Range(1f, 5f)] float strokeTimeout = 4f;
    float strokeTimeLeft;
    int strokeCount;

    string path;

    [SerializeField] Transform strokeContainer;

    [System.Serializable]
    public class UnityStringEvent : UnityEvent<string> { };
    public UnityStringEvent OnRecognized;
    
    List<Gesture> trainingSet = new List<Gesture>();
    
    bool isMoving = false;
    List<Vector3> allPositons = new List<Vector3>();   
    List<Vector3> currentLinePositions = new List<Vector3>();

    LineRenderer currentLine;
    List<GameObject> allLines = new List<GameObject>();
    public float lineWidth = 0.03f;
    public Material lineMaterial;
    
    void Start() {
        SetupControllerActions(leftController);
        SetupControllerActions(rightController);
        
        path = Application.dataPath + "/_Project/Gestures/";
        
        // Load training set
        var gestureFiles = Directory.GetFiles(path, "*.xml");
        foreach (var gesture in gestureFiles) {
            trainingSet.Add(GestureIO.ReadGestureFromFile(gesture));
        }
    }

    void Update() {
        if (strokeTimeLeft > 0) {
            strokeTimeLeft -= Time.deltaTime;
            if (isMoving) {
                UpdateMovement();
            }
        } else if (!isMoving && allPositons.Count > 0 && strokeTimeLeft <= 0) {
            Debug.Log("Recognizing gesture");
            strokeTimeLeft = 0;
            strokeCount = 0;
            isMoving = false;
            HandleGesture();
            
            // FIXME new method, maybe use tags instead of LINQs or a dissolve shader
            var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Stroke");
            foreach (var obj in objects) {
                Destroy(obj);
            }
        }
    }
    
    void SetupControllerActions(ActionBasedController controller) {
        controller.activateAction.action.started += HandleInput;
        controller.activateAction.action.canceled += HandleInput;
    }

    void HandleInput(InputAction.CallbackContext context) {
        Debug.Log(context);
        if (!isMoving && context.started) {
            StartMovement();
        } else if (isMoving && context.canceled) {
            EndMovement();
        }
    }

    void StartMovement() {
        Debug.Log("Start movement");
        isMoving = true;
        strokeTimeLeft = strokeTimeout;
        
        // Clear the position list if the stroke counter is zero
        if (strokeCount == 0) {
            allPositons.Clear();
            allLines.Clear();
        }

        // Solid line
        var lineGameObject = new GameObject("Stroke");
        //lineGameObject.transform.SetParent(strokeContainer);
        currentLine = lineGameObject.AddComponent<LineRenderer>();
        allLines.Add(lineGameObject);
        
        DrawCube();
        DrawLine();
    }

    void EndMovement() {
        Debug.Log("End movement");
        isMoving = false;
        strokeCount++;
        currentLinePositions.Clear();
        currentLine = null;
        Debug.Log("Stroke Time Left: " + strokeTimeLeft + " Stroke Count: " + strokeCount);
    }

    void UpdateMovement() {
        if (!currentLine || currentLinePositions.Count == 0)
            return;
        
        Debug.Log("Update movement");
        var lastPosition = currentLinePositions[currentLinePositions.Count - 1];
        if (Vector3.Distance(lastPosition, movementSource.position) > positionThreshold) {
            DrawCube();
            DrawLine();
        }
    }

    void HandleGesture() {        
        // Create the gesture from the positions
        var pointArray = new Point[allPositons.Count];
        for (var i = 0; i < allPositons.Count; i++) {
            // FIXME Add helper method here to get the Camera
            var screenPoint = Helpers.Camera.WorldToScreenPoint(allPositons[i]);
            pointArray[i] = new Point(screenPoint.x, screenPoint.y, strokeCount);
        }
        
        var newGesture = new Gesture(pointArray);
        
        // Add a new gesture to the training set
        if (creationMode) {
            newGesture.Name = newGestureName;
            trainingSet.Add(newGesture);
            
            var fileName = path + "/" + newGestureName + ".xml";
            GestureIO.WriteGesture(pointArray, newGestureName, fileName);
            Debug.Log(fileName);
            
        }
        // Recognize
        else {
            var result = PointCloudRecognizer.Classify(newGesture, trainingSet.ToArray());
            Debug.Log(result.GestureClass + " " + result.Score);
            
            if (result.Score > recognitionThreshold) {
                OnRecognized?.Invoke(result.GestureClass);
            }
        }
        
        // Reset so that the next stroke can be recorded
        strokeCount = 0;
        allPositons.Clear();
    }

    // DEPRECATED
    void DrawCube() {
        if (cubePrefab != null) {
            //Destroy(Instantiate(cubePrefab, movementSource.position, Quaternion.identity), 3f);
        }
    }
    
    void DrawLine() {
        allPositons.Add(movementSource.position);
        currentLinePositions.Add(movementSource.position);
        currentLine.positionCount = currentLinePositions.Count;
        currentLine.SetPositions(currentLinePositions.ToArray());
        
        currentLine.material = lineMaterial;
        currentLine.startWidth = lineWidth;
    }
}
