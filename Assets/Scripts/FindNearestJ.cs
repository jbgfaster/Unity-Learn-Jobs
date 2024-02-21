using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class FindNearestJ : MonoBehaviour
{
    // The size of our arrays does not need to vary, so rather than create
    // new arrays every field, we'll create the arrays in Awake() and store them
    // in these fields.
    NativeArray<float3> TargetPositions;
    NativeArray<float3> SeekerPositions;
    NativeArray<float3> NearestTargetPositions;

    public void Start()
    {
        Spawner spawner = Object.FindObjectOfType<Spawner>();
        // We use the Persistent allocator because these arrays must
        // exist for the run of the program.
        TargetPositions = new NativeArray<float3>(spawner.NumTargets, Allocator.Persistent);
        SeekerPositions = new NativeArray<float3>(spawner.NumSeekers, Allocator.Persistent);
        NearestTargetPositions = new NativeArray<float3>(spawner.NumSeekers, Allocator.Persistent);
    }

    // We are responsible for disposing of our allocations
    // when we no longer need them.
    public void OnDestroy()
    {
        TargetPositions.Dispose();
        SeekerPositions.Dispose();
        NearestTargetPositions.Dispose();
    }

    public void Update()
    {
        // Copy every target transform to a NativeArray.
        for (int i = 0; i < TargetPositions.Length; i++)
        {
            // Vector3 is implicitly converted to float3
            TargetPositions[i] = Spawner.TargetTransforms[i].localPosition;
        }

        // Copy every seeker transform to a NativeArray.
        for (int i = 0; i < SeekerPositions.Length; i++)
        {
            // Vector3 is implicitly converted to float3
            SeekerPositions[i] = Spawner.SeekerTransforms[i].localPosition;
        }

        SortJob<float3, AxisXComparer> sortJob = TargetPositions.SortJob(new AxisXComparer { });
        JobHandle sortHandle = sortJob.Schedule();

        FindNearestJob findJob = new FindNearestJob
        {
            TargetPositions = TargetPositions,
            SeekerPositions = SeekerPositions,
            NearestTargetPositions = NearestTargetPositions,
        };

        // By passing the sort job handle to Schedule(), the find job will depend
        // upon the sort job, meaning the find job will not start executing until 
        // after the sort job has finished.
        // The find nearest job needs to wait for the sorting, 
        // so it must depend upon the sorting jobs. 
        JobHandle findHandle = findJob.Schedule(SeekerPositions.Length, 100, sortHandle);
        findHandle.Complete();

        // Draw a debug line from each seeker to its nearest target.
        for (int i = 0; i < SeekerPositions.Length; i++)
        {
            // float3 is implicitly converted to Vector3
            Debug.DrawLine(SeekerPositions[i], NearestTargetPositions[i]);
        }
    }
}