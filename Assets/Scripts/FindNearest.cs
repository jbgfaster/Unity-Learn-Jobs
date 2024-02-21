using UnityEngine;

public class FindNearest : MonoBehaviour
{
    public void Update()
    {
        // Find the nearest Target.
        // When comparing distances, it's cheaper to compare
        // the squares of the distances because doing so
        // avoids computing square roots.
        foreach (var seekerTransform in Spawner.SeekerTransforms)
        {
            Vector3 seekerPos = seekerTransform.localPosition;
            Vector3 nearestTargetPos = default;
            float nearestDistSq = float.MaxValue;
            foreach (var targetTransform in Spawner.TargetTransforms)
            {
                Vector3 offset = targetTransform.localPosition - seekerPos;
                float distSq = offset.sqrMagnitude;

                if (distSq < nearestDistSq)
                {
                    nearestDistSq = distSq;
                    nearestTargetPos = targetTransform.localPosition;
                }
            }
 
            Debug.DrawLine(seekerPos, nearestTargetPos);
        }        
    }
}