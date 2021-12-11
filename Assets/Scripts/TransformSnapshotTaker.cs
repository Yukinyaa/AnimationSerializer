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
    List<Dictionary<string, (Vector3Serializable, Vector3Serializable)>> snapshots;
    Transform rootJoint;
    public int Count => snapshots.Count;
    Vector3 scale;
    private Vector3 vector3;

    public string Serialize() => JsonConvert.SerializeObject((snapshots, localPositions));


    TransformSnapshotTaker(Vector3 scale, Transform rootJoint, params (string, Transform)[] transformTuples)
    {
        transforms = new Dictionary<string, Transform>();
        snapshots = new List<Dictionary<string, (Vector3Serializable, Vector3Serializable)>>();
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
        var frame = new Dictionary<string, (Vector3Serializable, Vector3Serializable)>();
        
        foreach (var elem in transforms)
        {
            string key = elem.Key;
            Vector3Serializable realPos = (Vector3Serializable)Vector3.Scale(elem.Value.position - rootJoint.position, scale);
            Vector3Serializable angle = (Vector3Serializable)(elem.Value.localRotation.eulerAngles * Mathf.Deg2Rad);
            if (ReferenceEquals(elem.Value, rootJoint))
            {
                angle = (Vector3Serializable)(elem.Value.rotation.eulerAngles * Mathf.Deg2Rad);
                realPos = Vector3Serializable.zero;
            }
            //Debug.Log($"{key}, {realPos}, {angle}");
            frame.Add(key, (realPos, angle));
        }
        snapshots.Add(frame);
        TestForwardKinematics(frame);
    }




    class FKTester
    {
        Dictionary<string, (Vector3Serializable, Vector3Serializable)> frame;
        Dictionary<string, Vector3Serializable> localPositions;
        public Vector3Serializable Position { get; private set; } = Vector3Serializable.zero;

        public FKTester(Vector3Serializable pos, Dictionary<string, (Vector3Serializable, Vector3Serializable)> frame, Dictionary<string, Vector3Serializable> localPositions)
            => (Position, this.frame, this.localPositions) = (pos, frame, localPositions);

        public FKTester ApplyTransform(string jointName)
        {
            Position = RotateZXY(Position, frame[jointName].Item2);
            //Debug.Log($"{Position}+{localPositions[jointName]} = {Position + localPositions[jointName]}");
            Position += localPositions[jointName];

            return this;
        }
        public float DiffWith(string jointName)
        {
            float diff = ((Vector3)Position - (Vector3)frame[jointName].Item1).magnitude;
            return diff;
        }
        public void AssertDiffWith(string jointName, float tolerance)
        {
            float diff = DiffWith(jointName);

// Debug.Log(
            Debug.Assert(diff < tolerance,
@$"FK {jointName} sol {tolerance} < diff({diff:F7}, {Position - frame[jointName].Item1})
Calculated: {Position}, Original: {frame[jointName].Item1}");
        }

    }

    void TestForwardKinematics(Dictionary<string, (Vector3Serializable, Vector3Serializable)> frame)
    {
        new FKTester(localPositions["Head"], frame, localPositions)
            .ApplyTransform("Neck")
            .ApplyTransform("Chest")
            .ApplyTransform("Spine")
            .ApplyTransform("Pelvis").AssertDiffWith("Head", 0.1f);

        new FKTester(Vector3Serializable.zero, frame, localPositions)
            .ApplyTransform("LeftWrist")
            .ApplyTransform("LeftElbow")
            .ApplyTransform("LeftShoulder")
            .ApplyTransform("LeftCollar")
            .ApplyTransform("Chest")
            .ApplyTransform("Spine")
            .ApplyTransform("Pelvis").AssertDiffWith("LeftWrist", 0.1f);


        new FKTester(Vector3Serializable.zero, frame, localPositions)
            .ApplyTransform("RightWrist")
            .ApplyTransform("RightElbow")
            .ApplyTransform("RightShoulder")
            .ApplyTransform("RightCollar")
            .ApplyTransform("Chest")
            .ApplyTransform("Spine")
            .ApplyTransform("Pelvis").AssertDiffWith("RightWrist", 0.1f);


    }

}
