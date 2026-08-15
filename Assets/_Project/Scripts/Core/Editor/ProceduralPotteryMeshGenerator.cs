using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Project.Core.Editor
{
    /// <summary>
    /// 器(茶碗型)の3Dメッシュを、外部素材に頼らず手続き的に生成するツール。
    /// 断面プロファイル(半径と高さの点列)をY軸中心に回転させて作る、いわゆる「ろくろ」方式。
    /// 外壁・内壁・縁(リム)の3つの面を組み合わせて、口が開いた中空の器として生成する。
    ///
    /// 使い方: Unityメニュー "HapticResearch > Generate Bowl Mesh (Procedural)" を実行すると、
    /// Assets/_Project/Models/Bowl_Procedural.asset というメッシュが生成される。
    /// </summary>
    public static class ProceduralPotteryMeshGenerator
    {
        private struct LatheData
        {
            public Vector3[] vertices;
            public Vector2[] uvs;
            public int[] triangles;
        }

        [MenuItem("HapticResearch/Generate Bowl Mesh (Procedural)")]
        public static void GenerateBowl()
        {
            // 断面プロファイル: (半径[m], 高さ[m])。底の中心から、口に向かって広がる茶碗型
            Vector2[] outerProfile =
            {
                new Vector2(0.000f, 0.000f), // 底の中心
                new Vector2(0.025f, 0.000f), // 底の端(高台)
                new Vector2(0.030f, 0.008f), // 高台からの立ち上がり
                new Vector2(0.038f, 0.020f), // 側面
                new Vector2(0.048f, 0.040f), // 側面、広がり始め
                new Vector2(0.058f, 0.058f), // 口に向けて広がる
                new Vector2(0.065f, 0.070f), // 口の縁(最大幅)
            };

            const float wallThickness = 0.010f; // 器の厚み[m](生乾きの土を想定し、厚めに設定)
            const int segments = 32;

            Mesh mesh = BuildHollowBowlMesh(outerProfile, wallThickness, segments);
            SaveMesh(mesh, "Bowl_Procedural");
        }

        private static Mesh BuildHollowBowlMesh(Vector2[] outerProfile, float wallThickness, int segments)
        {
            var innerProfile = new Vector2[outerProfile.Length];
            for (int i = 0; i < outerProfile.Length; i++)
            {
                float r = Mathf.Max(outerProfile[i].x - wallThickness, 0f);
                innerProfile[i] = new Vector2(r, outerProfile[i].y);
            }

            var outer = BuildLatheData(outerProfile, segments, reverseWinding: false);
            var inner = BuildLatheData(innerProfile, segments, reverseWinding: true);

            Vector2[] rimProfile = { outerProfile[outerProfile.Length - 1], innerProfile[innerProfile.Length - 1] };
            var rim = BuildLatheData(rimProfile, segments, reverseWinding: false);

            return CombineMeshes(outer, inner, rim);
        }

        private static LatheData BuildLatheData(Vector2[] profile, int segments, bool reverseWinding)
        {
            int pointCount = profile.Length;
            var vertices = new Vector3[pointCount * (segments + 1)];
            var uvs = new Vector2[vertices.Length];

            for (int s = 0; s <= segments; s++)
            {
                float angle = (float)s / segments * Mathf.PI * 2f;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                for (int p = 0; p < pointCount; p++)
                {
                    int index = s * pointCount + p;
                    float r = profile[p].x;
                    float y = profile[p].y;

                    vertices[index] = new Vector3(r * cos, y, r * sin);
                    uvs[index] = new Vector2((float)s / segments, (float)p / Mathf.Max(pointCount - 1, 1));
                }
            }

            var triList = new List<int>();
            for (int s = 0; s < segments; s++)
            {
                for (int p = 0; p < pointCount - 1; p++)
                {
                    int a = s * pointCount + p;
                    int b = (s + 1) * pointCount + p;
                    int c = s * pointCount + p + 1;
                    int d = (s + 1) * pointCount + p + 1;

                    if (!reverseWinding)
                    {
                        triList.Add(a); triList.Add(b); triList.Add(c);
                        triList.Add(c); triList.Add(b); triList.Add(d);
                    }
                    else
                    {
                        triList.Add(a); triList.Add(c); triList.Add(b);
                        triList.Add(c); triList.Add(d); triList.Add(b);
                    }
                }
            }

            return new LatheData { vertices = vertices, uvs = uvs, triangles = triList.ToArray() };
        }

        private static Mesh CombineMeshes(params LatheData[] parts)
        {
            int totalVerts = 0;
            foreach (var part in parts) totalVerts += part.vertices.Length;

            var vertices = new Vector3[totalVerts];
            var uvs = new Vector2[totalVerts];
            var triangles = new List<int>();

            int offset = 0;
            foreach (var part in parts)
            {
                System.Array.Copy(part.vertices, 0, vertices, offset, part.vertices.Length);
                System.Array.Copy(part.uvs, 0, uvs, offset, part.uvs.Length);

                foreach (var idx in part.triangles)
                    triangles.Add(idx + offset);

                offset += part.vertices.Length;
            }

            var mesh = new Mesh { name = "ProceduralBowl" };
            if (totalVerts > 65000)
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            return mesh;
        }

        private static void SaveMesh(Mesh mesh, string assetName)
        {
            const string folder = "Assets/_Project/Models";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                Directory.CreateDirectory(folder);
                AssetDatabase.Refresh();
            }

            string path = $"{folder}/{assetName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[ProceduralPotteryMeshGenerator] メッシュを生成しました: {path}");
        }
    }
}
