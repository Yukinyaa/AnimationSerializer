using System.Collections.Generic;
using UnityEngine;
using static Vector3Serializables;

public class FKTester
{
    Dictionary<string, (Vector3Serializable, Vector3Serializable, Vector3Serializable)> frame;
    Dictionary<string, Vector3Serializable> localPositions;
    public Vector3Serializable Position { get; private set; } = Vector3Serializable.zero;

    public FKTester(Vector3Serializable pos, Dictionary<string, (Vector3Serializable, Vector3Serializable, Vector3Serializable)> frame, Dictionary<string, Vector3Serializable> localPositions)
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

    public static void TestForwardKinematics(Dictionary<string, (Vector3Serializable, Vector3Serializable, Vector3Serializable)> frame, Dictionary<string, Vector3Serializable> localPositions)
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

