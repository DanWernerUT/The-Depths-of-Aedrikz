using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class StatuePose : MonoBehaviour
{
    [Header("Assign a pose AnimationClip (Humanoid or Generic)")]
    public AnimationClip poseClip;

    private PlayableGraph graph;

    void Start()
    {
        if (poseClip == null)
        {
            GetComponent<Animator>().applyRootMotion = false;
            Debug.LogWarning("StatuePose: No pose clip assigned.", this);
            return;
        }

        // Create graph
        graph = PlayableGraph.Create("StatuePoseGraph");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        // Create output
        var output = AnimationPlayableOutput.Create(graph, "Animation", GetComponent<Animator>());

        // Create the playable
        var clipPlayable = AnimationClipPlayable.Create(graph, poseClip);
        clipPlayable.SetApplyFootIK(false);
        clipPlayable.SetApplyPlayableIK(false);

        // Freeze the clip at the last frame
        clipPlayable.SetTime(poseClip.length);
        clipPlayable.SetDuration(poseClip.length);
        clipPlayable.Pause();

        // Connect playable
        output.SetSourcePlayable(clipPlayable);

        // Play the graph once to set pose
        graph.Play();
    }

    void OnDestroy()
    {
        if (graph.IsValid())
        {
            graph.Destroy();
        }
    }
}
