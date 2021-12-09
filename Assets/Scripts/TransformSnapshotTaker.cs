using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using static SerializeHelper;

public class TransformSnapshotTaker
{
    Dictionary<string, Transform> transforms;
    Dictionary<string, Vector3Serializable> localPositions;
    List<Dictionary<string, (Vector3Serializable, Vector3Serializable)>> snapshots;

    public int Count => snapshots.Count;
    Vector3 scale;
    private Vector3 vector3;

    public string Serialize() => JsonConvert.SerializeObject((snapshots, localPositions));
    TransformSnapshotTaker(Vector3 scale, params (string, Transform)[] transformTuples)
    {
        transforms = new Dictionary<string, Transform>();
        snapshots = new List<Dictionary<string, (Vector3Serializable, Vector3Serializable)>>();
        localPositions = new Dictionary<string, Vector3Serializable>();


        foreach (var tuple in transformTuples)
        {
            var scaledLocalPosition = Vector3.Scale(tuple.Item2.localPosition, tuple.Item2.localScale);
            var rescaledLocalPosition = Vector3.Scale(scaledLocalPosition, scale);

            localPositions.Add(tuple.Item1, (Vector3Serializable)rescaledLocalPosition);
            transforms.Add(tuple.Item1, tuple.Item2);
        }

        this.scale = scale;
    }

    public TransformSnapshotTaker(Vector3 scale, Animator humanoidAvatar) :
            this(scale,
                ("Pelvis", humanoidAvatar.GetBoneTransform(HumanBodyBones.Hips)),
                    ("Spine", humanoidAvatar.GetBoneTransform(HumanBodyBones.Spine)),
                        ("Chest", humanoidAvatar.GetBoneTransform(HumanBodyBones.Chest)),
                            ("Neck", humanoidAvatar.GetBoneTransform(HumanBodyBones.Neck)),
                                ("Head", humanoidAvatar.GetBoneTransform(HumanBodyBones.Head)),
                        ("LeftShoulder", humanoidAvatar.GetBoneTransform(HumanBodyBones.LeftUpperArm)),
                            ("LeftElbow", humanoidAvatar.GetBoneTransform(HumanBodyBones.LeftLowerArm)),
                                ("LeftWrist", humanoidAvatar.GetBoneTransform(HumanBodyBones.LeftHand)),
                        ("RightShoulder", humanoidAvatar.GetBoneTransform(HumanBodyBones.RightUpperArm)),
                            ("RightElbow", humanoidAvatar.GetBoneTransform(HumanBodyBones.RightLowerArm)),
                                ("RightWrist", humanoidAvatar.GetBoneTransform(HumanBodyBones.RightHand)))
    {
    }

    public void TakeSnapshot()
    {
        var frame = new Dictionary<string, (Vector3Serializable, Vector3Serializable)>();
        
        foreach (var elem in transforms)
        {
            string key = elem.Key;
            Vector3Serializable pos = (Vector3Serializable)Vector3.Scale(elem.Value.position, scale);
            Vector3Serializable eulerAngles = (Vector3Serializable)elem.Value.rotation.eulerAngles;
            frame.Add(key, (pos, eulerAngles));
        }
        //TestForward(elem);
        snapshots.Add(frame);
    }
    
}
