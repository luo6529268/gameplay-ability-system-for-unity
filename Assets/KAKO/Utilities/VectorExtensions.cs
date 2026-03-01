using UnityEngine;

namespace Kako.Utilities
{
	public static class VectorExtensions
	{
		public static Vector3 GetProjectionPoint(this Vector3 position, Transform plane) => position.GetProjectionPoint(plane.position, plane.forward);
		public static Vector3 GetProjectionPoint(this Vector3 position, Vector3 planeCenter, Vector3 planeNormal)
		{
			Plane plane = new Plane(planeNormal, planeCenter);
			return plane.ClosestPointOnPlane(position);
		}
		public static Bounds GetBounds(this Vector3[] positions)
		{
			Bounds positionBounds = new Bounds(positions[0], Vector3.zero);

			for (int i = 1; i < positions.Length; i++)
			{
				positionBounds.Encapsulate(positions[i]);
			}

			return positionBounds;
		}

		public static Vector3 GetAbsolute(this Vector3 vector) => new (Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
		public static Vector2 GetAbsolute(this Vector2 vector) => new (Mathf.Abs(vector.x), Mathf.Abs(vector.y));
	}
}