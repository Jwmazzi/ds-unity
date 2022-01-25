// /*******************************************************************************
//  * Copyright 2012-2021 Esri
//  *
//  *  Licensed under the Apache License, Version 2.0 (the "License");
//  *  you may not use this file except in compliance with the License.
//  *  You may obtain a copy of the License at
//  *
//  *  http://www.apache.org/licenses/LICENSE-2.0
//  *
//  *   Unless required by applicable law or agreed to in writing, software
//  *   distributed under the License is distributed on an "AS IS" BASIS,
//  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  *   See the License for the specific language governing permissions and
//  *   limitations under the License.
//  ******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EsriPS.Toolkits
{
	public static class GeometryHelper
	{
		/// <summary>method <c>CreatePlane</c> creates a game object with plane geometry using width and height.
		/// </summary>
		public static GameObject CreatePlane(float width, float height, bool collider, bool isHorizontal = true)
		{
			GameObject go = new GameObject("Plane");
			MeshFilter mf = go.AddComponent<MeshFilter>();
			MeshRenderer mr = go.AddComponent<MeshRenderer>();

			Mesh m = new Mesh();
			if (isHorizontal)
			{
				m.vertices = new Vector3[]
				{
			new Vector3(0, 0, 0),
			new Vector3(width, 0, 0),
			new Vector3(width, 0, height),
			new Vector3(0, 0, height)
				};
			}
			else
			{
				m.vertices = new Vector3[]
				{
			new Vector3(0, 0, 0),
			new Vector3(width, 0, 0),
			new Vector3(width, height, 0),
			new Vector3(0, height, 0)
				};
			}

			m.uv = new Vector2[]
			{
			new Vector3(0, 0),
			new Vector3(0, 1),
			new Vector3(1, 1),
			new Vector3(1, 0)
			};

			if (isHorizontal)
			{
				m.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
			}
			else
			{
				m.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
			}

			mf.mesh = m;
			if (collider)
			{
				(go.AddComponent<MeshCollider>()).sharedMesh = m;
			}

			m.RecalculateBounds();
			m.RecalculateNormals();

			return go;
		}

		/// <summary>method <c>CreatePlane</c> creates a game object with plane geometry from 2 points.
		/// </summary>
		public static GameObject CreatePlane(Vector3 beginPoint, Vector3 endPoint, float width, bool collider, bool isHorizontal = true)
		{
			GameObject go = new GameObject("Plane");
			MeshFilter mf = go.AddComponent<MeshFilter>();
			MeshRenderer mr = go.AddComponent<MeshRenderer>();

			var dirVect = endPoint - beginPoint;
			var normVect = Vector3.Cross(dirVect, Vector3.up).normalized * width;

			var vertices = new Vector3[4];

			Mesh m = new Mesh();
			if (isHorizontal)
			{
				Vector3 vertex1 = beginPoint + normVect;
				Vector3 vertex2 = vertex1 + dirVect;
				Vector3 vertex3 = vertex2 - normVect - normVect;
				Vector3 vertex4 = beginPoint - normVect;
				vertices[0] = vertex1;
				vertices[1] = vertex2;
				vertices[2] = vertex3;
				vertices[3] = vertex4;
				m.vertices = vertices;
			}
			else
			{
				Vector3 vertVect = (Vector3.up * width);
				Vector3 vertex1 = beginPoint + vertVect;
				Vector3 vertex2 = vertex1 + dirVect;
				Vector3 vertex3 = vertex2 - vertVect - vertVect;
				Vector3 vertex4 = beginPoint - vertVect;
				vertices[0] = vertex1;
				vertices[1] = vertex2;
				vertices[2] = vertex3;
				vertices[3] = vertex4;
				m.vertices = vertices;
			}

			m.uv = new Vector2[]
			{
			new Vector3(0, 0),
			new Vector3(0, 1),
			new Vector3(1, 1),
			new Vector3(1, 0)
			};

			m.triangles = new int[] { 0, 1, 2, 0, 2, 3 };

			mf.mesh = m;
			if (collider)
			{
				(go.AddComponent<MeshCollider>()).sharedMesh = m;
			}

			m.RecalculateBounds();
			m.RecalculateNormals();

			return go;
		}

		/// <summary>method <c>CreatePlane</c> creates a game object with plane geometry from an array of vertices.
		/// </summary>
		public static GameObject CreatePlane(Vector3[] poly, Material mat)
		{
			GameObject go = new GameObject("Plane");
			if (poly == null || poly.Length < 3)
			{
				Debug.Log("Define 2D polygon in 'poly' in the the Inspector");
				return go;
			}

			MeshFilter mf = go.AddComponent<MeshFilter>();

			Mesh mesh = new Mesh();
			mf.mesh = mesh;

			Renderer rend = go.AddComponent<MeshRenderer>();
			rend.material = mat;

			Vector3 center = GeometryHelper.FindCenter(poly);

			Vector3[] vertices = new Vector3[poly.Length + 1];
			vertices[0] = Vector3.zero;

			for (int i = 0; i < poly.Length; i++)
			{
				poly[i].z = 0.0f;
				vertices[i + 1] = poly[i] - center;
			}

			mesh.vertices = vertices;

			int[] triangles = new int[poly.Length * 3];

			for (int i = 0; i < poly.Length - 1; i++)
			{
				triangles[i * 3] = i + 2;
				triangles[i * 3 + 1] = 0;
				triangles[i * 3 + 2] = i + 1;
			}

			triangles[(poly.Length - 1) * 3] = 1;
			triangles[(poly.Length - 1) * 3 + 1] = 0;
			triangles[(poly.Length - 1) * 3 + 2] = poly.Length;

			mesh.triangles = triangles;
			mesh.uv = GeometryHelper.BuildUVs(vertices);

			mesh.RecalculateBounds();
			mesh.RecalculateNormals();

			return go;
		}

		private static Vector3 FindCenter(Vector3[] poly)
		{
			Vector3 center = Vector3.zero;
			foreach (Vector3 v3 in poly)
			{
				center += v3;
			}
			return center / poly.Length;
		}

		private static Vector2[] BuildUVs(Vector3[] vertices)
		{

			float xMin = Mathf.Infinity;
			float yMin = Mathf.Infinity;
			float xMax = -Mathf.Infinity;
			float yMax = -Mathf.Infinity;

			foreach (Vector3 v3 in vertices)
			{
				if (v3.x < xMin)
					xMin = v3.x;
				if (v3.y < yMin)
					yMin = v3.y;
				if (v3.x > xMax)
					xMax = v3.x;
				if (v3.y > yMax)
					yMax = v3.y;
			}

			float xRange = xMax - xMin;
			float yRange = yMax - yMin;

			Vector2[] uvs = new Vector2[vertices.Length];
			for (int i = 0; i < vertices.Length; i++)
			{
				uvs[i].x = (vertices[i].x - xMin) / xRange;
				uvs[i].y = (vertices[i].y - yMin) / yRange;

			}
			return uvs;
		}
	}
}
