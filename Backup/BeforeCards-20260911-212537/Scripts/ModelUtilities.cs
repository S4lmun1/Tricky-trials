using UnityEngine;

public static class ModelUtilities
{
    // Model prefabs supply visuals. The gameplay root owns collision and movement.
    public static void MakeVisualOnly(GameObject model)
    {
        foreach (var collider in model.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
        foreach (var body in model.GetComponentsInChildren<Rigidbody>(true))
        {
            body.isKinematic = true;
            body.detectCollisions = false;
        }
        foreach (var animator in model.GetComponentsInChildren<Animator>(true)) animator.applyRootMotion = false;
    }
}
