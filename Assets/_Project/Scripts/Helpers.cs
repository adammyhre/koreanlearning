using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Helpers {

  static Camera cam;
  // Cache Main Camera reference
  public static Camera Camera {
    get {
      if (cam == null) cam = Camera.main;
      return cam;
    }
  }

  // Non-allocating WaitForSeconds
  static readonly Dictionary<float, WaitForSeconds> WaitDictionary = new Dictionary<float, WaitForSeconds>();
  public static WaitForSeconds GetWait(float time) {
    if (WaitDictionary.TryGetValue(time, out var wait)) return wait;

    WaitDictionary[time] = new WaitForSeconds(time);
    return WaitDictionary[time];
  }

  // Is the pointer over the UI
  static PointerEventData eventDataCurrentPos;
  static List<RaycastResult> results;
  public static bool IsOverUI() {
    eventDataCurrentPos = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
    results = new List<RaycastResult>();
    EventSystem.current.RaycastAll(eventDataCurrentPos, results);
    return results.Count > 0;
  }

  // Find world point of canvas element (spawn an item on the canvas)
  // https://youtu.be/JOABOQMurZo?t=210
  public static Vector2 GetWorldPositionOfCanvasElement(RectTransform element) {
    RectTransformUtility.ScreenPointToWorldPointInRectangle(element, element.position, Camera, out var result);
    return result;
  }

  // Quickly destroy all child objects
  public static void DeleteChildren(this Transform t) {
    foreach (Transform child in t) Object.Destroy(child.gameObject);
  }
  
  // Remap a float value to a new range
  public static float Remap(float value, float aLow, float aHigh, float bLow, float bHigh) {
      var normalized = Mathf.InverseLerp(aLow, aHigh, value);
      return Mathf.Lerp(bLow, bHigh, normalized);
  }

}