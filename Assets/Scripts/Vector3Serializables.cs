using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Mathf;
static class Vector3Serializables
{
    public struct Vector3Serializable
    {
        public float x, y, z;
        public static Vector3Serializable zero => new Vector3Serializable(0, 0, 0);

        public Vector3Serializable(float x, float y, float z) => (this.x, this.y, this.z) = (x, y, z);
        public Vector3Serializable(Vector3 vector3) => (this.x, this.y, this.z) = (vector3.x, vector3.y, vector3.z);

        public static explicit operator Vector3Serializable(Vector3 vector3) => new Vector3Serializable(vector3);
        public static explicit operator Vector3(Vector3Serializable v) => new Vector3(v.x, v.y, v.z);

        public static Vector3Serializable operator +(Vector3Serializable a, Vector3Serializable b)
        {
            return new Vector3Serializable(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        public static Vector3Serializable operator -(Vector3Serializable a, Vector3Serializable b)
        {
            return new Vector3Serializable(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        public override string ToString()
        {
            return $"({x:F5}, {y:F5}, {z:F5})";
        }
    }
    public static void TestRotateZXY()
    {
        int totCount = 512, count = 0;
        for (int i = 0; i < totCount; i++)
        {
            Vector3 vec = UnityEngine.Random.insideUnitSphere.normalized;
            Vector3 rot = UnityEngine.Random.insideUnitSphere * 180;

            var customRot = RotateZXY(vec, rot * Deg2Rad);
            var unityRot =  Quaternion.Euler(rot.x, rot.y, rot.z) * vec;
            var diff = (customRot - unityRot).magnitude;

            Debug.Assert(diff < 0.01f, $"c:{customRot} vs u:{unityRot} = {diff}") ;
            if (diff < 0.1f)
                count++;
        }
        Debug.Log($"ZXY rotation Confirmed ({count}/{totCount} right)");
    }

    // ZXY rotation according to http://www.songho.ca/opengl/gl_anglestoaxes.html
    public static Vector3Serializable RotateZXY(Vector3Serializable a, Vector3Serializable deg)
    {
        float sa = Sin(deg.x), ca = Cos(deg.x);
        float sb = Sin(deg.y), cb = Cos(deg.y);
        float sc = Sin(deg.z), cc = Cos(deg.z);
        return new Vector3Serializable(
                (cb * cc + sb * sa * sc) * a.x      + (-cb * sc + sb * sa * cc) * a.y   + (sb * ca) * a.z,
                (ca * sc) * a.x                     + (ca * cc) * a.y                   + (-sa) * a.z,
                (-sb * cc + cb * sa * sc) * a.x     + (sb * sc + cb * sa * cc) * a.y    + (cb * ca) * a.z
            );
    }
    // ZXY rotation according to http://www.songho.ca/opengl/gl_anglestoaxes.html
    public static Vector3 RotateZXY(Vector3 a, Vector3 deg)
    {
        float sa = Sin(deg.x), ca = Cos(deg.x);
        float sb = Sin(deg.y), cb = Cos(deg.y);
        float sc = Sin(deg.z), cc = Cos(deg.z);
        return new Vector3(
                (cb * cc + sb * sa * sc) * a.x      + (-cb * sc + sb * sa * cc) * a.y   + (sb * ca) * a.z,
                (ca * sc) * a.x                     + (ca * cc) * a.y                   + (-sa) * a.z,
                (-sb * cc + cb * sa * sc) * a.x     + (sb * sc + cb * sa * cc) * a.y    + (cb * ca) * a.z
            );
    }
}
