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

    bool isProcessing;
    
    string processingTotalClipProgressStr;
    string processingUIClipProgressStr;
    string processingUIClipsPercentage;


    void OnGUI()
    {
        // Make a background box
        GUI.Box(new Rect(0, 0, 100, 50), "Processing Info");
        GUI.Label(new Rect(10, 30, 100, 20), "Processing Clips : " + processingTotalClipProgressStr + " (" + processingUIClipsPercentage + ")");
        GUI.Label(new Rect(40, 30, 100, 20), "Clip Progress : " + processingUIClipProgressStr);
    }
    IEnumerator ExportAnimations()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        FindAnimator();
        AssertIfHumanoidAvatar();

        List<AnimationClip> clips = FindAnimationClipsFromAnimatorControllers();
        
        Debug.Log($"Found {clips.Count()} clips!");

        float floorheight = this.transform.position.y;
        float headheightFromFloor = anim.GetBoneTransform(HumanBodyBones.Head).position.y - floorheight; //  make sure that model is at upright position

        //normalize model height to 
        transform.localScale /= headheightFromFloor;

        TransformSnapshotTaker snapshotTaker = new TransformSnapshotTaker(Vector3.one * headheightFromFloor, anim);

        foreach (var clip in clips)
        {
            float secondsPerFrame = 1 / clip.frameRate;
            float length = clip.length;
            float processingStartTime = Time.realtimeSinceStartup;

            for (float time = 0; time <= length; time += secondsPerFrame)
            {
                clip.SampleAnimation(this.gameObject, time);
                snapshotTaker.TakeSnapshot();


                processingTotalClipProgressStr = $"{clips.IndexOf(clip) + 1} / {clips.Count()}";

                if (Time.realtimeSinceStartup - processingStartTime > 1 / 60f)
                {
                    processingUIClipProgressStr = $"{time:F0}/{length:F0} seconds";
                    float progress = (clips.IndexOf(clip) + 1) / clips.Count() + time / length / clips.Count();
                    processingUIClipsPercentage = $"{progress * 100:F2}%";

                    yield return null;
                }
            }
        }
        
        Debug.Log(snapshotTaker.Serialize());
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

        return clips;
    }

    void Start()
    {
        StartCoroutine(ExportAnimations()); 
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


    // Update is called once per frame
    void Update()
    {
        
    }
}
