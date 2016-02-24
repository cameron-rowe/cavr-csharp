using System;
using System.Collections.Generic;

using cavr.math;

using VertexList = System.Collections.Generic.List<cavr.math.Vector3f>;

namespace extensions.gfx
{
	public static class Shapes
	{
		public static VertexList SolidSphere(int numLats, int numLons) {
			var result = new VertexList();

			var fPI = (float) Math.PI;

			var radsPerLat = fPI / (float) numLats;
			var radsPerLon = 2.0f * fPI / (float) numLons;

			for(int lat = 0; lat < numLats; lat++) {
				var topRad = radsPerLat * lat - fPI / 2.0f;
				var bottomRad = topRad + radsPerLat;
				var topY = (float) Math.Sin(topRad);
				var topXZ = (float) Math.Cos(topRad);
				var bottomY = (float) Math.Sin(bottomRad);
				var bottomXZ = (float)  Math.Cos(bottomRad);
				for(int lon = 0; lon < numLons; lon++) {
					var leftRad = radsPerLon * lon;
					var rightRad = leftRad + radsPerLon;
					var sinL = (float) Math.Sin(leftRad);
					var cosL = (float) Math.Cos(leftRad);
					var sinR = (float) Math.Sin(rightRad);
					var cosR = (float) Math.Cos(rightRad);

					var ll = new Vector4f(bottomXZ * cosL, bottomY, bottomXZ * sinL, 1.0f);
					var lr = new Vector4f(bottomXZ * cosR, bottomY, bottomXZ * sinR, 1.0f);
					var ul = new Vector4f(topXZ * cosL, topY, topXZ * sinL, 1.0f);
					var ur = new Vector4f(topXZ * cosR, topY, topXZ * sinR, 1.0f);

					result.Add(ll);
					result.Add(lr);
					result.Add(ul);
					result.Add(ul);
					result.Add(lr);
					result.Add(ur);
				}
			}

			return result;
		}

		public static VertexList WireCube() {
			var leftBottomFront = new Vector4f(-1, -1, -1, 1);
			var rightBottomFront = new Vector4f(1, -1, -1, 1);
			var leftTopFront = new Vector4f(-1, 1, -1, 1);
			var rightTopFront = new Vector4f(1, 1, -1, 1);
			var leftBottomBack = new Vector4f(-1, -1, 1, 1);
			var rightBottomBack = new Vector4f(1, -1, 1, 1);
			var leftTopBack = new Vector4f(-1, 1, 1, 1);
			var rightTopBack = new Vector4f(1, 1, 1, 1);

			var cubeLines = new VertexList {
				// left to right
				leftBottomFront, rightBottomFront,
				leftTopFront, rightTopFront,
				leftBottomBack, rightBottomBack,
				leftTopBack, rightTopBack,

				// bottom to top
				leftBottomFront, leftTopFront,
				rightBottomFront, rightTopFront,
				leftBottomBack, leftTopBack,
				rightBottomBack, rightTopBack,

				// front to back
				leftBottomBack, leftBottomFront,
				rightBottomFront, rightTopFront,
				leftTopBack, leftTopFront,
				rightTopBack, rightTopFront
			};

			return cubeLines;
		}

		public static VertexList SolidCone(int numSegments, float height, float radius) {
			var result = new VertexList();
			var center = new Vector4f(0, 0, 0, 1);
			var tip = new Vector4f(0, 0, height, 1);
			var radsPerSegment = (float) (2.0 * Math.PI / (double) numSegments);

			for(int i = 0; i < numSegments; i++) {
				var r1 = radsPerSegment * i;
				var r2 = r1 + radsPerSegment;

				var r = new Vector4f(radius * (float) Math.Cos(r1), radius * (float) Math.Sin(r1), 0, 1);
				var l = new Vector4f(radius * (float) Math.Cos(r2), radius * (float) Math.Sin(r2), 0, 1);

				result.Add(center);
				result.Add(l);
				result.Add(r);
				result.Add(r);
				result.Add(l);
				result.Add(tip);
			}

			if(height < 0.0f) {
				result.Reverse();
			}

			return result;
		}

		public static VertexList SolidCylinder(int numSegments, float height, float radius) {
			var result = new VertexList();
			var bottomCenter = new Vector4f(0, 0, 0, 1);
			var topCenter = new Vector4f(0, 0, height, 1);
			var radsPerSegment = (float) (2.0 * Math.PI / (double) numSegments);

			for(int i = 0; i < numSegments; i++) {
				var r1 = radsPerSegment * i;
				var r2 = r1 + radsPerSegment;

				var bottomR = new Vector4f(radius * (float) Math.Cos(r1), radius * (float) Math.Sin(r1), 0, 1);
				var bottomL = new Vector4f(radius * (float) Math.Cos(r2), radius * (float) Math.Sin(r2), 0, 1);
				var topR = new Vector4f(radius * (float) Math.Cos(r1), radius * (float) Math.Sin(r1), height, 1);
				var topL = new Vector4f(radius * (float) Math.Cos(r2), radius * (float) Math.Sin(r2), height, 1);

				result.Add(bottomCenter);
				result.Add(bottomL);
				result.Add(bottomR);
				result.Add(topCenter);
				result.Add(topR);
				result.Add(topL);
				result.Add(bottomR);
				result.Add(bottomL);
				result.Add(topR);
				result.Add(topR);
				result.Add(bottomL);
				result.Add(topL);
			}

			return result;
		}
	}
}

