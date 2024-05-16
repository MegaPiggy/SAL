using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SALT
{
    public class MeshUtils
    {
        public static readonly string meshCachePath = Path.Combine(Path.Combine(Application.persistentDataPath, "Cache"), "Mesh");

        static MeshUtils()
        {
            if (!Directory.Exists(meshCachePath)) Directory.CreateDirectory(meshCachePath);
        }

        public static void CacheMesh(string name, Mesh mesh)
        {
            string path = Path.Combine(meshCachePath, name + ".mesh");
            File.WriteAllBytes(path, WriteMesh(mesh, true));
        }

        public static void OpenFolder() => SALT.OperatingSystem.FilesBrowser.OpenAtPath(meshCachePath);

        public static Mesh GetCachedMesh(string name)
        {
            string path = Path.Combine(meshCachePath, name + ".mesh");
            return File.Exists(path) ? ReadMesh(File.ReadAllBytes(path)) : null;
        }

        public static bool TryGetCachedMesh(string name, out Mesh mesh)
        {
            string path = Path.Combine(meshCachePath, name + ".mesh");
            if (File.Exists(path))
            {
                try
                {
                    mesh = ReadMesh(File.ReadAllBytes(path));
                    return true;
                }
                catch (Exception)
                {
                    mesh = null;
                    return false;
                }
            }
            else
            {
                mesh = null;
                return false;
            }
        }

        /// <summary>
        /// Reads mesh from an array of bytes.
        /// </summary>
        public static Mesh ReadMesh(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 5)
                throw new Exception("Invalid mesh file!");

            var buf = new BinaryReader(new MemoryStream(bytes));

            // read header
            var name = buf.ReadString();
            var vertCount = buf.ReadUInt16();
            var triCount = buf.ReadUInt16();
            var format = buf.ReadByte();

            // sanity check
            if (vertCount < 0)
                throw new Exception("Invalid vertex count in the mesh data!");
            if (triCount < 0)
                throw new Exception("Invalid triangle count in the mesh data!");
            if (format < 1 || (format & 1) == 0 || format > 15)
                throw new Exception("Invalid vertex format in the mesh data!");

            var mesh = new Mesh();
            mesh.name = name;
            int i;

            // Higher than this and we have to use a different indexFormat
            if (vertCount > 65535)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            // positions
            var verts = new Vector3[vertCount];
            ReadVector3Array16Bit(verts, buf);
            mesh.vertices = verts;

            if ((format & 2) != 0) // have normals
            {
                var normals = new Vector3[vertCount];
                ReadVector3ArrayBytes(normals, buf);
                mesh.normals = normals;
            }

            if ((format & 4) != 0) // have tangents
            {
                var tangents = new Vector4[vertCount];
                ReadVector4ArrayBytes(tangents, buf);
                mesh.tangents = tangents;
            }

            if ((format & 8) != 0) // have UVs
            {
                var uvs = new Vector2[vertCount];
                ReadVector2Array16Bit(uvs, buf);
                mesh.uv = uvs;
            }

            // triangle indices
            var tris = new int[triCount * 3];
            for (i = 0; i < triCount; ++i)
            {
                tris[i * 3 + 0] = buf.ReadUInt16();
                tris[i * 3 + 1] = buf.ReadUInt16();
                tris[i * 3 + 2] = buf.ReadUInt16();
            }
            mesh.triangles = tris;

            var cacheDate = DateTime.FromBinary(buf.ReadInt64());

            buf.Close();

            return mesh;
        }

        /// <summary>
        /// Writes mesh to an array of bytes.
        /// </summary>
        public static byte[] WriteMesh(Mesh mesh, bool saveTangents)
        {
            if (!mesh)
                throw new Exception("No mesh given!");

            var name = mesh.name;
            var verts = mesh.vertices;
            var normals = mesh.normals;
            var tangents = mesh.tangents;
            var uvs = mesh.uv;
            var tris = mesh.triangles;

            // figure out vertex format
            byte format = 1;
            if (normals.Length > 0)
                format |= 2;
            if (saveTangents && tangents.Length > 0)
                format |= 4;
            if (uvs.Length > 0)
                format |= 8;

            var stream = new MemoryStream();
            var buf = new BinaryWriter(stream);

            // write header
            buf.Write(name);
            var vertCount = (ushort)verts.Length;
            var triCount = (ushort)(tris.Length / 3);
            buf.Write(vertCount);
            buf.Write(triCount);
            buf.Write(format);
            // vertex components
            WriteVector3Array16Bit(verts, buf);
            WriteVector3ArrayBytes(normals, buf);
            if (saveTangents)
                WriteVector4ArrayBytes(tangents, buf);
            WriteVector2Array16Bit(uvs, buf);
            // triangle indices
            foreach (var idx in tris)
            {
                var idx16 = (ushort)idx;
                buf.Write(idx16);
            }

            buf.Write(DateTime.UtcNow.ToBinary());

            buf.Close();

            return stream.ToArray();
        }

        private static void ReadVector3Array16Bit(Vector3[] arr, BinaryReader buf)
        {
            var n = arr.Length;
            if (n == 0)
                return;

            // read bounding box
            Vector3 bmin;
            Vector3 bmax;
            bmin.x = buf.ReadSingle();
            bmax.x = buf.ReadSingle();
            bmin.y = buf.ReadSingle();
            bmax.y = buf.ReadSingle();
            bmin.z = buf.ReadSingle();
            bmax.z = buf.ReadSingle();

            // decode vectors as 16 bit integer components between the bounds
            for (var i = 0; i < n; ++i)
            {
                ushort ix = buf.ReadUInt16();
                ushort iy = buf.ReadUInt16();
                ushort iz = buf.ReadUInt16();
                float xx = ix / 65535.0f * (bmax.x - bmin.x) + bmin.x;
                float yy = iy / 65535.0f * (bmax.y - bmin.y) + bmin.y;
                float zz = iz / 65535.0f * (bmax.z - bmin.z) + bmin.z;
                arr[i] = new Vector3(xx, yy, zz);
            }
        }

        private static void WriteVector3Array16Bit(Vector3[] arr, BinaryWriter buf)
        {
            if (arr.Length == 0)
                return;

            // calculate bounding box of the array
            var bounds = new Bounds(arr[0], new Vector3(0.001f, 0.001f, 0.001f));
            foreach (var v in arr)
                bounds.Encapsulate(v);

            // write bounds to stream
            var bmin = bounds.min;
            var bmax = bounds.max;
            buf.Write(bmin.x);
            buf.Write(bmax.x);
            buf.Write(bmin.y);
            buf.Write(bmax.y);
            buf.Write(bmin.z);
            buf.Write(bmax.z);

            // encode vectors as 16 bit integer components between the bounds
            foreach (var v in arr)
            {
                var xx = Mathf.Clamp((v.x - bmin.x) / (bmax.x - bmin.x) * 65535.0f, 0.0f, 65535.0f);
                var yy = Mathf.Clamp((v.y - bmin.y) / (bmax.y - bmin.y) * 65535.0f, 0.0f, 65535.0f);
                var zz = Mathf.Clamp((v.z - bmin.z) / (bmax.z - bmin.z) * 65535.0f, 0.0f, 65535.0f);
                var ix = (ushort)xx;
                var iy = (ushort)yy;
                var iz = (ushort)zz;
                buf.Write(ix);
                buf.Write(iy);
                buf.Write(iz);
            }
        }

        private static void ReadVector2Array16Bit(Vector2[] arr, BinaryReader buf)
        {
            var n = arr.Length;
            if (n == 0)
                return;

            // Read bounding box
            Vector2 bmin;
            Vector2 bmax;
            bmin.x = buf.ReadSingle();
            bmax.x = buf.ReadSingle();
            bmin.y = buf.ReadSingle();
            bmax.y = buf.ReadSingle();

            // Decode vectors as 16 bit integer components between the bounds
            for (var i = 0; i < n; ++i)
            {
                ushort ix = buf.ReadUInt16();
                ushort iy = buf.ReadUInt16();
                float xx = ix / 65535.0f * (bmax.x - bmin.x) + bmin.x;
                float yy = iy / 65535.0f * (bmax.y - bmin.y) + bmin.y;
                arr[i] = new Vector2(xx, yy);
            }
        }
        private static void WriteVector2Array16Bit(Vector2[] arr, BinaryWriter buf)
        {
            if (arr.Length == 0)
                return;

            // Calculate bounding box of the array
            Vector2 bmin = arr[0] - new Vector2(0.001f, 0.001f);
            Vector2 bmax = arr[0] + new Vector2(0.001f, 0.001f);
            foreach (var v in arr)
            {
                bmin.x = Mathf.Min(bmin.x, v.x);
                bmin.y = Mathf.Min(bmin.y, v.y);
                bmax.x = Mathf.Max(bmax.x, v.x);
                bmax.y = Mathf.Max(bmax.y, v.y);
            }

            // Write bounds to stream
            buf.Write(bmin.x);
            buf.Write(bmax.x);
            buf.Write(bmin.y);
            buf.Write(bmax.y);

            // Encode vectors as 16 bit integer components between the bounds
            foreach (var v in arr)
            {
                var xx = (v.x - bmin.x) / (bmax.x - bmin.x) * 65535.0f;
                var yy = (v.y - bmin.y) / (bmax.y - bmin.y) * 65535.0f;
                var ix = (ushort)xx;
                var iy = (ushort)yy;
                buf.Write(ix);
                buf.Write(iy);
            }
        }

        private static void ReadVector3ArrayBytes(Vector3[] arr, BinaryReader buf)
        {
            // decode vectors as 8 bit integers components in -1.0f .. 1.0f range
            var n = arr.Length;
            for (var i = 0; i < n; ++i)
            {
                byte ix = buf.ReadByte();
                byte iy = buf.ReadByte();
                byte iz = buf.ReadByte();
                float xx = (ix - 128.0f) / 127.0f;
                float yy = (iy - 128.0f) / 127.0f;
                float zz = (iz - 128.0f) / 127.0f;
                arr[i] = new Vector3(xx, yy, zz);
            }
        }

        private static void WriteVector3ArrayBytes(Vector3[] arr, BinaryWriter buf)
        {
            // encode vectors as 8 bit integers components in -1.0f .. 1.0f range
            foreach (var v in arr)
            {
                var ix = (byte)Mathf.Clamp(v.x * 127.0f + 128.0f, 0.0f, 255.0f);
                var iy = (byte)Mathf.Clamp(v.y * 127.0f + 128.0f, 0.0f, 255.0f);
                var iz = (byte)Mathf.Clamp(v.z * 127.0f + 128.0f, 0.0f, 255.0f);
                buf.Write(ix);
                buf.Write(iy);
                buf.Write(iz);
            }
        }

        private static void ReadVector4ArrayBytes(Vector4[] arr, BinaryReader buf)
        {
            // Decode vectors as 8 bit integers components in -1.0f .. 1.0f range
            var n = arr.Length;
            for (var i = 0; i < n; ++i)
            {
                byte ix = buf.ReadByte();
                byte iy = buf.ReadByte();
                byte iz = buf.ReadByte();
                byte iw = buf.ReadByte();
                float xx = (ix - 128.0f) / 127.0f;
                float yy = (iy - 128.0f) / 127.0f;
                float zz = (iz - 128.0f) / 127.0f;
                float ww = (iw - 128.0f) / 127.0f;
                arr[i] = new Vector4(xx, yy, zz, ww);
            }
        }

        private static void WriteVector4ArrayBytes(Vector4[] arr, BinaryWriter buf)
        {
            // Encode vectors as 8 bit integers components in -1.0f .. 1.0f range
            foreach (var v in arr)
            {
                var ix = (byte)Mathf.Clamp(v.x * 127.0f + 128.0f, 0.0f, 255.0f);
                var iy = (byte)Mathf.Clamp(v.y * 127.0f + 128.0f, 0.0f, 255.0f);
                var iz = (byte)Mathf.Clamp(v.z * 127.0f + 128.0f, 0.0f, 255.0f);
                var iw = (byte)Mathf.Clamp(v.w * 127.0f + 128.0f, 0.0f, 255.0f);
                buf.Write(ix);
                buf.Write(iy);
                buf.Write(iz);
                buf.Write(iw);
            }
        }
    }
}
