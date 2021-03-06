using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class AnimationSerializer : MonoBehaviour
{
    Animator anim = null;
    public List<AnimatorController> animatorControllers;
    public List<AnimationClip> animations;

    bool isProcessing = false;
    
    string processingTotalClipProgressStr;
    string processingUIClipProgressStr;
    string processingUIClipsPercentage;

    void Start()
    {
        Vector3Serializables.TestRotateZXY();
        StartCoroutine(ExportAnimations());
    }

    void OnGUI()
    {
        if (isProcessing)
        {
            // Make a background box
            GUI.Box(new Rect(10, 10, 300, 100), "Processing Info");
            GUI.Label(new Rect(20, 40, 280, 20), "Processing Clips : " + processingTotalClipProgressStr + " (" + processingUIClipsPercentage + ")");
            GUI.Label(new Rect(20, 70, 280, 20), "Clip Progress : " + processingUIClipProgressStr);
        }
    }

    IEnumerator ExportAnimations()
    {
        isProcessing = true;
        transform.position = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;

        FindAnimator();
        AssertIfHumanoidAvatar();

        List<AnimationClip> clips = FindAnimationClipsFromAnimatorControllers();
        
        Debug.Log($"Found {clips.Count()} clips!");

        float floorheight = this.transform.position.y;
        float headheightFromFloor = anim.GetBoneTransform(HumanBodyBones.Head).position.y - floorheight; //  make sure that model is at upright position

        //normalize model height to 

        TransformSnapshotTaker snapshotTaker = new TransformSnapshotTaker(Vector3.one, anim);

        foreach (var clip in clips)
        {
            float secondsPerFrame = 1 / clip.frameRate;
            float length = clip.length;
            float processingStartTime = Time.realtimeSinceStartup;

            processingTotalClipProgressStr = $"{clips.IndexOf(clip) + 1} / {clips.Count()}({clip.name})";

            for (float time = 0; time <= length; time += secondsPerFrame)
            {
                clip.SampleAnimation(this.gameObject, time);

                transform.position = Vector3.zero;
                transform.localScale = Vector3.one;
                //transform.rotation = Quaternion.identity;
                snapshotTaker.TakeSnapshot();



                if (Time.realtimeSinceStartup - processingStartTime > 1 / 60f)
                {
                    processingUIClipProgressStr = $"{time:F0}/{length:F0} seconds";
                    float progress = (clips.IndexOf(clip)) / clips.Count() + time / length / clips.Count();
                    processingUIClipsPercentage = $"{progress * 100:F2}%";

                    yield return null;
                }
            }
        }
        isProcessing = false;
        var pospath = EditorUtility.SaveFilePanel("Save Position.json", "", "Position.json", "json");
        var snppath = EditorUtility.SaveFilePanel("Save Snapshot.json", "", "Snapshot.json", "json");
        if (pospath.Length != 0 && snppath.Length != 0)
        {
            System.IO.File.WriteAllText(pospath, snapshotTaker.SerializePositions());
            Debug.Log("Finished Saving Positions!");

            System.IO.File.WriteAllText(snppath, snapshotTaker.SerializeSnapshots());
            Debug.Log("Finished Saving Snapshots!");
        }
        else
            Debug.LogWarning("Invalid position!");
    }

    private List<AnimationClip> FindAnimationClipsFromAnimatorControllers()
    {
        var clips = new List<AnimationClip>();

        foreach (var controller in animatorControllers)
        {
            int controllerIdx = animatorControllers.IndexOf(controller) + 1;

            foreach (var clip in controller.animationClips)
            {
                if (!clips.Exists(a => a == clip))
                    clips.Add(clip);
            }
        }
        foreach (var clip in animations)
        {
            if (!clips.Exists(a => a == clip))
                clips.Add(clip);
        }

        return clips;
    }

    void FindAnimator()
    {
        if (anim == null) anim = GetComponent<Animator>();
    }
    private void AssertIfHumanoidAvatar()
    {
        FindAnimator();
        var avatar = anim?.avatar;
        Debug.Assert(avatar != null);
        Debug.Assert(avatar.isHuman);
    }
}
