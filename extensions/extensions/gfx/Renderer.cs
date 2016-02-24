using System.Threading;

using cavr.math;

namespace extensions.gfx
{
	public interface IRenderer
	{
		void SetNear(double d);
		void SetFar(double d);
		double GetNear();
		double GetFar();

		Matrix4f GetProjection();
		Matrix4f GetView();
		Vector3f GetEyePosition();
	}

	public static class Renderer {
		private static ThreadLocal<IRenderer> renderer = new ThreadLocal<IRenderer>(() => null);

		public static void SetNear(double d) {
			renderer.Value.SetNear(d);
		}

		public static void SetFar(double d) {
			renderer.Value.SetFar(d);
		}

		public static double GetNear() {
			return renderer.Value.GetNear();
		}

		public static double GetFar() {
			return renderer.Value.GetFar();
		}

		public static Matrix4f GetProjection() {
			return renderer.Value.GetProjection();
		}

		public static Matrix4f GetView() {
			return renderer.Value.GetView();
		}

		public static Vector3f GetEyePosition() {
			return renderer.Value.GetEyePosition();
		}
	}
}

