using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using static Vector3Serializables;

public class TransformSnapshotTaker
{
    Dictionary<string, Transform> transforms;
    Dictionary<string, Vector3Serializable> localPositions;
    List<Dictionary<string, (Vector3Serializable, Vector3Serializable, Vector3Serializable)>> snapshots;
    Transform rootJoint;
    public int Count => snapshots.Count;
    Vector3 scale;
    private Vector3 vector3;

    public string SerializeSnapshots() =>
        JsonConvert.SerializeObject(
                from snapshot in snapshots select 
                    snapshot.ToDictionary(
                        pair => pair.Key, 
                        pair => new List<float> { 
                            pair.Value.Item1.x, pair.Value.Item1.y, pair.Value.Item1.z,
                            pair.Value.Item2.x, pair.Value.Item2.y, pair.Value.Item2.z,
                            pair.Value.Item3.x, pair.Value.Item3.y, pair.Value.Item3.z
                        }
                    )
            );
    public string SerializePositions() => 
        JsonConvert.SerializeObject(localPositions.ToDictionary(k => k.Key, v => new List<float> { v.Value.x, v.Value.y, v.Value.z }));

    TransformSnapshotTaker(Vector3 scale, Transform rootJoint, params (string, Transform)[] transformTuples)
    {
        transforms = new Dictionary<string, Transform>();
        snapshots = new List<Dictionary<string, (Vector3Serializable, Vector3Serializable, Vector3Serializable)>>();
        localPositions = new Dictionary<string, Vector3Serializable>();
        this.rootJoint = rootJoint;

        foreach (var tuple in transformTuples)
        {
            var scaledLocalPosition = tuple.Item2.localPosition;
            if (ReferenceEquals(tuple.Item2, rootJoint))
                scaledLocalPosition = Vector3.zero;
            var rescaledLocalPosition = Vector3.Scale(scaledLocalPosition, scale);

            localPositions.Add(tuple.Item1, (Vector3Serializable)rescaledLocalPosition);
            transforms.Add(tuple.Item1, tuple.Item2);
        }

        this.scale = scale;
    }

    public TransformSnapshotTaker(Vector3 scale, Animator humanoidAvatar) :
            this(scale, humanoidAvatar.GetBoneTransform(HumanBodyBones.Hips),

                ("Pelvis", humanoidAvatar.GetBoneTransform(HumanBodyBones.Hips)),
                    ("Spine", humanoidAvatar.GetBoneTransform(HumanBodyBones.Spine)),
                        ("Chest", humanoidAvatar.GetBoneTransform(HumanBodyBones.Chest)),
                            ("Neck", humanoidAvatar.GetBoneTransform(HumanBodyBones.Neck)),
                                ("Head", humanoidAvatar.GetBoneTransform(HumanBodyBones.Head)),
                            ("LeftCollar", humanoidAvatar.GetBoneTransform(HumanBodyBones.LeftShoulder)),
                                ("LeftShoulder", humanoidAvatar.GetBoneTransform(HumanBodyBones.LeftUpperArm)),
                                    ("LeftElbow", humanoidAvatar.GetBoneTransform(HumanBodyBones.LeftLowerArm)),
                                        ("LeftWrist", humanoidAvatar.GetBoneTransform(HumanBodyBones.LeftHand)),
                            ("RightCollar", humanoidAvatar.GetBoneTransform(HumanBodyBones.RightShoulder)),
                                ("RightShoulder", humanoidAvatar.GetBoneTransform(HumanBodyBones.RightUpperArm)),
                                    ("RightElbow", humanoidAvatar.GetBoneTransform(HumanBodyBones.RightLowerArm)),
                                        ("RightWrist", humanoidAvatar.GetBoneTransform(HumanBodyBones.RightHand)))
    {
    }

    public void TakeSnapshot()
    {
        var frame = new Dictionary<string, (Vector3Serializable, Vector3Serializable, Vector3Serializable)>();
        
        foreach (var elem in transforms)
        {
            string key = elem.Key;
            Vector3Serializable realPos = (Vector3Serializable)Vector3.Scale(elem.Value.position - rootJoint.position, scale);
            Vector3Serializable angle = (Vector3Serializable)(elem.Value.localRotation.eulerAngles * Mathf.Deg2Rad);
            Vector3Serializable globalAngle = (Vector3Serializable)(elem.Value.rotation.eulerAngles * Mathf.Deg2Rad);
            if (ReferenceEquals(elem.Value, rootJoint))
            {
                angle = (Vector3Serializable)(elem.Value.rotation.eulerAngles * Mathf.Deg2Rad);
                realPos = Vector3Serializable.zero;
            }
            //Debug.Log($"{key}, {realPos}, {angle}");
            frame.Add(key, (realPos, angle, globalAngle));
        }
        snapshots.Add(frame);
        FKTester.TestForwardKinematics(frame, localPositions);
    }




}

