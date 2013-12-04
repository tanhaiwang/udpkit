/*
* The MIT License (MIT)
* 
* Copyright (c) 2012-2013 Fredrik Holmstrom (fredrik.johan.holmstrom@gmail.com)
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using UnityEngine;

namespace UdpKit {
    public static class UdpBitStreamExt {
        public static void WriteColor32RGBA (ref UdpBitStream stream, Color32 value) {
            stream.WriteByte(value.r, 8);
            stream.WriteByte(value.g, 8);
            stream.WriteByte(value.b, 8);
            stream.WriteByte(value.a, 8);
        }

        public static Color32 ReadColor32RGBA (ref UdpBitStream stream) {
            return new Color32(stream.ReadByte(8), stream.ReadByte(8), stream.ReadByte(8), stream.ReadByte(8));
        }

        public static void WriteColor32RGB (ref UdpBitStream stream, Color32 value) {
            stream.WriteByte(value.r, 8);
            stream.WriteByte(value.g, 8);
            stream.WriteByte(value.b, 8);
        }

        public static Color32 ReadColor32RGB (ref UdpBitStream stream) {
            return new Color32(stream.ReadByte(8), stream.ReadByte(8), stream.ReadByte(8), 0xFF);
        }

        public static void WriteVector2 (ref UdpBitStream stream, Vector2 value) {
            stream.WriteFloat(value.x);
            stream.WriteFloat(value.y);
        }

        public static Vector2 ReadVector2 (ref UdpBitStream stream) {
            return new Vector2(stream.ReadFloat(), stream.ReadFloat());
        }

        public static void WriteVector2Half (ref UdpBitStream stream, Vector2 value) {
            stream.WriteHalf(value.x);
            stream.WriteHalf(value.y);
        }

        public static Vector2 ReadVector2Half (ref UdpBitStream stream) {
            return new Vector2(stream.ReadHalf(), stream.ReadHalf());
        }

        public static void WriteVector3 (ref UdpBitStream stream, Vector3 value) {
            stream.WriteFloat(value.x);
            stream.WriteFloat(value.y);
            stream.WriteFloat(value.z);
        }

        public static Vector3 ReadVector3 (ref UdpBitStream stream) {
            return new Vector3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
        }

        public static void WriteVector3Half (ref UdpBitStream stream, Vector3 value) {
            stream.WriteHalf(value.x);
            stream.WriteHalf(value.y);
            stream.WriteHalf(value.z);
        }

        public static Vector3 ReadVector3Half (ref UdpBitStream stream) {
            return new Vector3(stream.ReadHalf(), stream.ReadHalf(), stream.ReadHalf());
        }

        public static void WriteColorRGB (ref UdpBitStream stream, Color value) {
            stream.WriteFloat(value.r);
            stream.WriteFloat(value.g);
            stream.WriteFloat(value.b);
        }

        public static Color ReadColorRGB (ref UdpBitStream stream) {
            return new Color(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
        }

        public static void WriteColorRGBHalf (ref UdpBitStream stream, Color value) {
            stream.WriteHalf(value.r);
            stream.WriteHalf(value.g);
            stream.WriteHalf(value.b);
        }

        public static Color ReadColorRGBHalf (ref UdpBitStream stream) {
            return new Color(stream.ReadHalf(), stream.ReadHalf(), stream.ReadHalf());
        }

        public static void WriteVector4 (ref UdpBitStream stream, Vector4 value) {
            stream.WriteFloat(value.x);
            stream.WriteFloat(value.y);
            stream.WriteFloat(value.z);
            stream.WriteFloat(value.w);
        }

        public static Vector4 ReadVector4 (ref UdpBitStream stream) {
            return new Vector4(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
        }

        public static void WriteVector4Half (ref UdpBitStream stream, Vector4 value) {
            stream.WriteHalf(value.x);
            stream.WriteHalf(value.y);
            stream.WriteHalf(value.z);
            stream.WriteHalf(value.w);
        }

        public static Vector4 ReadVector4Half (ref UdpBitStream stream) {
            return new Vector4(stream.ReadHalf(), stream.ReadHalf(), stream.ReadHalf(), stream.ReadHalf());
        }

        public static void WriteColorRGBA (ref UdpBitStream stream, Color value) {
            stream.WriteFloat(value.r);
            stream.WriteFloat(value.g);
            stream.WriteFloat(value.b);
            stream.WriteFloat(value.a);
        }

        public static Color ReadColorRGBA (ref UdpBitStream stream) {
            return new Color(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
        }

        public static void WriteColorRGBAHalf (ref UdpBitStream stream, Color value) {
            stream.WriteHalf(value.r);
            stream.WriteHalf(value.g);
            stream.WriteHalf(value.b);
            stream.WriteHalf(value.a);
        }

        public static Color ReadColorRGBAHalf (ref UdpBitStream stream) {
            return new Color(stream.ReadHalf(), stream.ReadHalf(), stream.ReadHalf(), stream.ReadHalf());
        }

        public static void WriteQuaternion (ref UdpBitStream stream, Quaternion value) {
            stream.WriteFloat(value.x);
            stream.WriteFloat(value.y);
            stream.WriteFloat(value.z);
            stream.WriteFloat(value.w);
        }

        public static Quaternion ReadQuaternion (ref UdpBitStream stream) {
            return new Quaternion(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
        }

        public static void WriteQuaternionHalf (ref UdpBitStream stream, Quaternion value) {
            stream.WriteHalf(value.x);
            stream.WriteHalf(value.y);
            stream.WriteHalf(value.z);
            stream.WriteHalf(value.w);
        }

        public static Quaternion ReadQuaternionHalf (ref UdpBitStream stream) {
            return new Quaternion(stream.ReadHalf(), stream.ReadHalf(), stream.ReadHalf(), stream.ReadHalf());
        }

        public static void WriteTransform (ref UdpBitStream stream, Transform transform) {
            UdpBitStreamExt.WriteVector3(ref stream, transform.position);
            UdpBitStreamExt.WriteQuaternion(ref stream, transform.rotation);
        }

        public static void ReadTransform (ref UdpBitStream stream, Transform transform) {
            transform.position = UdpBitStreamExt.ReadVector3(ref stream);
            transform.rotation = UdpBitStreamExt.ReadQuaternion(ref stream);
        }

        public static void ReadTransform (ref UdpBitStream stream, out Vector3 position, out Quaternion rotation) {
            position = UdpBitStreamExt.ReadVector3(ref stream);
            rotation = UdpBitStreamExt.ReadQuaternion(ref stream);
        }

        public static void WriteTransformHalf (ref UdpBitStream stream, Transform transform) {
            UdpBitStreamExt.WriteVector3Half(ref stream, transform.position);
            UdpBitStreamExt.WriteQuaternionHalf(ref stream, transform.rotation);
        }

        public static void ReadTransformHalf (ref UdpBitStream stream, Transform transform) {
            transform.position = UdpBitStreamExt.ReadVector3Half(ref stream);
            transform.rotation = UdpBitStreamExt.ReadQuaternionHalf(ref stream);
        }

        public static void ReadTransformHalf (ref UdpBitStream stream, out Vector3 position, out Quaternion rotation) {
            position = UdpBitStreamExt.ReadVector3Half(ref stream);
            rotation = UdpBitStreamExt.ReadQuaternionHalf(ref stream);
        }

        public static void WriteRigidbody (ref UdpBitStream stream, Rigidbody rigidbody) {
            UdpBitStreamExt.WriteVector3(ref stream, rigidbody.position);
            UdpBitStreamExt.WriteQuaternion(ref stream, rigidbody.rotation);
            UdpBitStreamExt.WriteVector3(ref stream, rigidbody.velocity);
            UdpBitStreamExt.WriteVector3(ref stream, rigidbody.angularVelocity);
        }

        public static void ReadRigidbody (ref UdpBitStream stream, Rigidbody rigidbody) {
            rigidbody.position = UdpBitStreamExt.ReadVector3(ref stream);
            rigidbody.rotation = UdpBitStreamExt.ReadQuaternion(ref stream);
            rigidbody.velocity = UdpBitStreamExt.ReadVector3(ref stream);
            rigidbody.angularVelocity = UdpBitStreamExt.ReadVector3(ref stream);
        }

        public static void ReadRigidbody (ref UdpBitStream stream, out Vector3 position, out Quaternion rotation, out Vector3 velocity, out Vector3 angularVelocity) {
            position = UdpBitStreamExt.ReadVector3(ref stream);
            rotation = UdpBitStreamExt.ReadQuaternion(ref stream);
            velocity = UdpBitStreamExt.ReadVector3(ref stream);
            angularVelocity = UdpBitStreamExt.ReadVector3(ref stream);
        }

        public static void WriteRigidbodyHalf (ref UdpBitStream stream, Rigidbody rigidbody) {
            UdpBitStreamExt.WriteVector3Half(ref stream, rigidbody.position);
            UdpBitStreamExt.WriteQuaternionHalf(ref stream, rigidbody.rotation);
            UdpBitStreamExt.WriteVector3Half(ref stream, rigidbody.velocity);
            UdpBitStreamExt.WriteVector3Half(ref stream, rigidbody.angularVelocity);
        }

        public static void ReadRigidbodyHalf (ref UdpBitStream stream, Rigidbody rigidbody) {
            rigidbody.position = UdpBitStreamExt.ReadVector3Half(ref stream);
            rigidbody.rotation = UdpBitStreamExt.ReadQuaternionHalf(ref stream);
            rigidbody.velocity = UdpBitStreamExt.ReadVector3Half(ref stream);
            rigidbody.angularVelocity = UdpBitStreamExt.ReadVector3Half(ref stream);
        }

        public static void ReadRigidbodyHalf (ref UdpBitStream stream, out Vector3 position, out Quaternion rotation, out Vector3 velocity, out Vector3 angularVelocity) {
            position = UdpBitStreamExt.ReadVector3Half(ref stream);
            rotation = UdpBitStreamExt.ReadQuaternionHalf(ref stream);
            velocity = UdpBitStreamExt.ReadVector3Half(ref stream);
            angularVelocity = UdpBitStreamExt.ReadVector3Half(ref stream);
        }

        public static void WriteBounds (ref UdpBitStream stream, Bounds b) {
            UdpBitStreamExt.WriteVector3(ref stream, b.center);
            UdpBitStreamExt.WriteVector3(ref stream, b.size);
        }

        public static Bounds ReadBounds (ref UdpBitStream stream) {
            return new Bounds(UdpBitStreamExt.ReadVector3(ref stream), UdpBitStreamExt.ReadVector3(ref stream));
        }

        public static void WriteBoundsHalf (ref UdpBitStream stream, Bounds b) {
            UdpBitStreamExt.WriteVector3Half(ref stream, b.center);
            UdpBitStreamExt.WriteVector3Half(ref stream, b.size);
        }

        public static Bounds ReadBoundsHalf (ref UdpBitStream stream) {
            return new Bounds(UdpBitStreamExt.ReadVector3Half(ref stream), UdpBitStreamExt.ReadVector3Half(ref stream));
        }

        public static void WriteRect (ref UdpBitStream stream, Rect rect) {
            stream.WriteFloat(rect.xMin);
            stream.WriteFloat(rect.yMin);
            stream.WriteFloat(rect.width);
            stream.WriteFloat(rect.height);
        }

        public static Rect ReadRect (ref UdpBitStream stream) {
            return new Rect(
                stream.ReadFloat(),
                stream.ReadFloat(),
                stream.ReadFloat(),
                stream.ReadFloat()
            );
        }

        public static void WriteRectHalf (ref UdpBitStream stream, Rect rect) {
            stream.WriteHalf(rect.xMin);
            stream.WriteHalf(rect.yMin);
            stream.WriteHalf(rect.width);
            stream.WriteHalf(rect.height);
        }

        public static Rect ReadRectHalf (ref UdpBitStream stream) {
            return new Rect(
                stream.ReadHalf(),
                stream.ReadHalf(),
                stream.ReadHalf(),
                stream.ReadHalf()
            );
        }

        public static void WriteRay (ref UdpBitStream stream, Ray ray) {
            UdpBitStreamExt.WriteVector3(ref stream, ray.origin);
            UdpBitStreamExt.WriteVector3(ref stream, ray.direction);
        }

        public static Ray ReadRay (ref UdpBitStream stream) {
            return new Ray(
                UdpBitStreamExt.ReadVector3(ref stream),
                UdpBitStreamExt.ReadVector3(ref stream)
            );
        }

        public static void WriteRayHalf (ref UdpBitStream stream, Ray ray) {
            UdpBitStreamExt.WriteVector3Half(ref stream, ray.origin);
            UdpBitStreamExt.WriteVector3Half(ref stream, ray.direction);
        }

        public static Ray ReadRayHalf (ref UdpBitStream stream) {
            return new Ray(
                UdpBitStreamExt.ReadVector3Half(ref stream),
                UdpBitStreamExt.ReadVector3Half(ref stream)
            );
        }

        public static void WritePlane (ref UdpBitStream stream, Plane plane) {
            UdpBitStreamExt.WriteVector3(ref stream, plane.normal);
            stream.WriteFloat(plane.distance);
        }

        public static Plane ReadPlane (ref UdpBitStream stream) {
            return new Plane(
                UdpBitStreamExt.ReadVector3(ref stream),
                stream.ReadFloat()
            );
        }

        public static void WritePlaneHalf (ref UdpBitStream stream, Plane plane) {
            UdpBitStreamExt.WriteVector3Half(ref stream, plane.normal);
            stream.WriteHalf(plane.distance);
        }

        public static Plane ReadPlaneHalf (ref UdpBitStream stream) {
            return new Plane(
                UdpBitStreamExt.ReadVector3Half(ref stream),
                stream.ReadHalf()
            );
        }

        public static void WriteLayerMask (ref UdpBitStream stream, LayerMask mask) {
            stream.WriteInt(mask.value, 32);
        }

        public static LayerMask ReadLayerMask (ref UdpBitStream stream) {
            return stream.ReadInt(32);
        }

        public static void WriteMatrix4x4 (ref UdpBitStream stream, ref Matrix4x4 m) {
            stream.WriteFloat(m.m00);
            stream.WriteFloat(m.m01);
            stream.WriteFloat(m.m02);
            stream.WriteFloat(m.m03);
            stream.WriteFloat(m.m10);
            stream.WriteFloat(m.m11);
            stream.WriteFloat(m.m12);
            stream.WriteFloat(m.m13);
            stream.WriteFloat(m.m20);
            stream.WriteFloat(m.m21);
            stream.WriteFloat(m.m22);
            stream.WriteFloat(m.m23);
            stream.WriteFloat(m.m30);
            stream.WriteFloat(m.m31);
            stream.WriteFloat(m.m32);
            stream.WriteFloat(m.m33);
        }

        public static Matrix4x4 ReadMatrix4x4 (ref UdpBitStream stream) {
            Matrix4x4 m = default(Matrix4x4);
            m.m00 = stream.ReadFloat();
            m.m01 = stream.ReadFloat();
            m.m02 = stream.ReadFloat();
            m.m03 = stream.ReadFloat();
            m.m10 = stream.ReadFloat();
            m.m11 = stream.ReadFloat();
            m.m12 = stream.ReadFloat();
            m.m13 = stream.ReadFloat();
            m.m20 = stream.ReadFloat();
            m.m21 = stream.ReadFloat();
            m.m22 = stream.ReadFloat();
            m.m23 = stream.ReadFloat();
            m.m30 = stream.ReadFloat();
            m.m31 = stream.ReadFloat();
            m.m32 = stream.ReadFloat();
            m.m33 = stream.ReadFloat();
            return m;
        }

        public static void WriteMatrix4x4Half (ref UdpBitStream stream, ref Matrix4x4 m) {
            stream.WriteHalf(m.m00);
            stream.WriteHalf(m.m01);
            stream.WriteHalf(m.m02);
            stream.WriteHalf(m.m03);
            stream.WriteHalf(m.m10);
            stream.WriteHalf(m.m11);
            stream.WriteHalf(m.m12);
            stream.WriteHalf(m.m13);
            stream.WriteHalf(m.m20);
            stream.WriteHalf(m.m21);
            stream.WriteHalf(m.m22);
            stream.WriteHalf(m.m23);
            stream.WriteHalf(m.m30);
            stream.WriteHalf(m.m31);
            stream.WriteHalf(m.m32);
            stream.WriteHalf(m.m33);
        }

        public static Matrix4x4 ReadMatrix4x4Half (ref UdpBitStream stream) {
            Matrix4x4 m = default(Matrix4x4);
            m.m00 = stream.ReadHalf();
            m.m01 = stream.ReadHalf();
            m.m02 = stream.ReadHalf();
            m.m03 = stream.ReadHalf();
            m.m10 = stream.ReadHalf();
            m.m11 = stream.ReadHalf();
            m.m12 = stream.ReadHalf();
            m.m13 = stream.ReadHalf();
            m.m20 = stream.ReadHalf();
            m.m21 = stream.ReadHalf();
            m.m22 = stream.ReadHalf();
            m.m23 = stream.ReadHalf();
            m.m30 = stream.ReadHalf();
            m.m31 = stream.ReadHalf();
            m.m32 = stream.ReadHalf();
            m.m33 = stream.ReadHalf();
            return m;
        }
    }
}