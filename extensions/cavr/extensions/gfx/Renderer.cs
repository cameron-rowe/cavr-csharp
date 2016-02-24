using System.Threading;

using cavr.math;

namespace cavr.extensions.gfx
{
	public interface IRenderer
	{
        double Near { get; set; }
        double Far { get; set; }

        Matrix4f Projection { get; }
        Matrix4f View { get; }
        Vector3f EyePosition { get; }
	}

	public static class Renderer {
        private static ThreadLocal<IRenderer> renderer = null;

        public static IRenderer Instance {
            get { return renderer.Value; }
            set { renderer = new ThreadLocal<IRenderer>(() => value); }
        }

		public static void SetNear(double d) {
            renderer.Value.Near = d;
		}

		public static void SetFar(double d) {
            renderer.Value.Far = d;
		}

		public static double GetNear() {
            return renderer.Value.Near;
		}

		public static double GetFar() {
            return renderer.Value.Far;
		}

		public static Matrix4f GetProjection() {
            return renderer.Value.Projection;
		}

		public static Matrix4f GetView() {
            return renderer.Value.View;
		}

		public static Vector3f GetEyePosition() {
            return renderer.Value.EyePosition;
		}
	}
}

