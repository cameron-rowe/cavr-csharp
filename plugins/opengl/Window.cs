using System;
using System.Collections.Generic;
using System.Linq;

using NLog;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Platform;

using cavr;
using cavr.extensions.gfx;
using cavr.extensions.gl;
using cavr.input;
using cavr.math;

namespace opengl
{
    public abstract class Window : IRenderer, IDisposable
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public virtual bool IsStereo { get; }

        private double _near;
        public double Near {
            get {
                return _near;
            }
            set {
                _near = value;
                SetupView(eyeIndex);
            }
        }

        private double _far;
        public double Far { 
            get {
                return _far;
            }
            set {
                _far = value;
                SetupView(eyeIndex);
            }
        }

        public Matrix4f Projection { get; protected set; }
        public Matrix4f View { get; protected set; }
        public Vector3f EyePosition { get; protected set; }

        public IWindowInfo WindowInfo {
            get { return window.WindowInfo; }
        }

        protected int eyeIndex;
        protected int width;
        protected int height;
        protected int xPos;
        protected int yPos;
        protected bool fullscreen;
        protected FunctionCallback renderCallback;

        protected ColorFormat colormap;
        protected GraphicsContext context;
        protected NativeWindow window;
        protected DisplayDevice display;
        protected object contextData;

        public Window()
        {
            window = null;
            colormap = 0;
        }

        public void Dispose() {
            context.MakeCurrent(null);
            window.Dispose();
            GC.SuppressFinalize(this);
        }

        public bool Open(DisplayDevice disp, GraphicsMode config, ref GraphicsContext ctx) {
            display = disp;
            colormap = config.ColorFormat;

            window = new NativeWindow(xPos, yPos, width, height, "caVR Window", fullscreen ? GameWindowFlags.Fullscreen : GameWindowFlags.Default, config, display);

            if(ctx == null) {
                context = ctx = new GraphicsContext(config, window.WindowInfo);
            }
            else {
                context = ctx;
            }

            context.MakeCurrent(window.WindowInfo);
            GL.DrawBuffer(DrawBufferMode.Back);
            GL.Viewport(0, 0, width, height);
            GL.ClearColor(0, 0, 0, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            context.SwapBuffers();
            context.MakeCurrent(null);
            return true;
        }

        public void Update() {
            MakeCurrent();
            GL.Viewport(0, 0, width, height);
            GL.Enable(EnableCap.DepthTest);
            if(IsStereo) {
                GL.DrawBuffer(DrawBufferMode.BackLeft);
                GL.ClearColor(0, 0, 0, 1);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                eyeIndex = 0;
                SetupView(0);
                Renderer.Instance = this;
                renderCallback();
                PostRender();

                GL.DrawBuffer(DrawBufferMode.BackRight);
                GL.ClearColor(0, 0, 0, 1);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                eyeIndex = 1;
                SetupView(1);
                Renderer.Instance = this;
                renderCallback();
                PostRender();
            }
            else {
                GL.DrawBuffer(DrawBufferMode.Back);
                GL.ClearColor(0, 0, 0, 1);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                eyeIndex = 0;
                SetupView(0);
                Renderer.Instance = this;
                renderCallback();
                PostRender();
            }

            context.SwapBuffers();
        }

        public void SetPosition(int x, int y) {
            xPos = x;
            yPos = y;
        }

        public void SetResolution(int w, int h) {
            width = w;
            height = h;
        }

        public void MakeCurrent() {
            context.MakeCurrent(window.WindowInfo);
        }

        public void ProcessEvents() {
            window.ProcessEvents();
        }

        public NativeWindow GetWindow() {
            return window;
        }

        public virtual void SetupInput() {
            
        }

        public abstract void SetupView(int eye);

        public virtual void PostRender() {
        }

        public virtual void SetAnalogs(Analog[] analogs) {
        }

        public virtual bool SetupRenderData() {
            return true;
        }

        public static Window Configure(cavr.config.Configuration config, DisplayDevice display = null) {
            if(display == null) {
                display = DisplayDevice.Default;
            }

            var windowType = config.Get<string>("view");
            Window window = null;
            config.PushPrefix("view.");
            if(windowType == "perspective_render") {
                window = PerspectiveWindow.Configure(config, display);
            }
            else if(windowType == "simulator_view") {
                window = SimulatorWindow.Configure(config, display);
            }
            else {
                log.Error("Unknown view display type: {0}", windowType);
                return null;
            }

            config.PopPrefix();
            window._near = config.Get<double>("near");
            window._far = config.Get<double>("far");
            window.width = (int) config.Get<double>("width");
            window.height = (int) config.Get<double>("height");
            window.xPos = (int) config.Get<double>("x");
            window.yPos = (int) config.Get<double>("y");
            window.fullscreen = config.Get<bool>("fullscreen");

            window.renderCallback = cavr.System.GetCallback(config.Get<string>("render_callback"));

            return window;
        }
    }

    public class PerspectiveWindow : Window
    {
        private Marker lowerLeft;
        private Marker lowerRight;
        private Marker upperLeft;
        private List<Marker> eyes;

        public PerspectiveWindow() {
            eyes = new List<Marker>();
        }

        public override bool IsStereo {
            get { return eyes.Count == 2; }
        }

        public override void SetupView(int eye) {
            var ll = lowerLeft.Position;
            var lr = lowerRight.Position;
            var ul = upperLeft.Position;

            var right = (lr - ll).Normalized();
            var up = (ul - ll).Normalized();
            var back = right.Cross(up).Normalized();
            right = up.Cross(back).Normalized();
            up = back.Cross(right).Normalized();

            var screenReference = new Matrix4f(1.0f);
            screenReference.SetRow(0, right);
            screenReference.SetRow(1, up);
            screenReference.SetRow(2, back);
            screenReference.SetRow(3, ll);

            var inverseScreenReference = screenReference.Inverse();
            EyePosition = eyes[eye].Position;
            var screenEye = inverseScreenReference * (new Vector4f(EyePosition.x, EyePosition.y, EyePosition.z, 1.0f));
            var l = (float) (-screenEye.x / screenEye.z * Near);
            var r = (float) (((lr - ll).Length - screenEye.x) / screenEye.z * Near);
            var b = (float) (-screenEye.y / screenEye.z * Near);
            var t = (float) (((ul - ll).Length - screenEye.y) / screenEye.z * Near);
            Projection = Matrix4f.Frustrum(l, r, b, t, (float) Near, (float) Far);
            View = Matrix4f.Translate(-screenEye) * inverseScreenReference;
        }

        public static new PerspectiveWindow Configure(cavr.config.Configuration config, DisplayDevice display = null) {
            var p = new PerspectiveWindow();
            p.lowerLeft = config.Get<Marker>("lower_left");
            p.lowerRight = config.Get<Marker>("lower_right");
            p.upperLeft = config.Get<Marker>("upper_left");

            var eyeType = config.Get<string>("eyes");
            if(eyeType == "stereo") {
                p.eyes.Add(config.Get<Marker>("eyes.left_eye"));
                p.eyes.Add(config.Get<Marker>("eyes.right_eye"));
            }
            else if(eyeType == "mono") {
                p.eyes.Add(config.Get<Marker>("eyes.eye"));
            }

            return p;
        }
    }

    public class SimulatorWindow : Window
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private const float sixdofRadius = 0.25f;

        private double pivotDistance;
        private cavr.math.Vector3d eye;
        private Program simpleProgram;
        private int colorUniform;
        private int projectionUniform;
        private int viewUniform;
        private int modelUniform;
        private VBO cubeVbo;
        private VAO cubeVao;
        private VBO coneVbo;
        private VAO coneVao;
        private int numConeVertices;
        private bool rotating;
        private bool panning;
        private bool tilting;
        private int mouseX;
        private int mouseY;
        private SixDOF highlightedSixdof;
        private SixDOF selectedSixdof;
        private bool[] settingAnalog;
        private Analog[] analogs;
        private bool holdingShift;

        public SimulatorWindow() {
            cubeVbo = null;
            cubeVao = null;
            coneVbo = null;
            coneVao = null;
            rotating = false;
            panning = false;
            tilting = false;
            holdingShift = false;
            highlightedSixdof = null;
            selectedSixdof = null;

            settingAnalog = new bool[3] {false, false, false};
            analogs = new Analog[6];
        }
        public override void SetupView(int eye) {
            Projection = Matrix4f.Perspective((float) Math.PI * 0.5f, (float) width / (float) height, (float) Near, (float) Far);
            EyePosition = this.eye;
        }

        public override bool SetupRenderData() {
            simpleProgram = Program.CreateSimple();
            if(simpleProgram == null) {
                log.Error("Failed to load rendering shader");
                return false;
            }

            var positionLocation = simpleProgram.GetAttribute("in_position");
            if(positionLocation == -1) {
                log.Error("in_position does not exist");
                return false;
            }

            colorUniform = simpleProgram.GetUniform("color");
            projectionUniform = simpleProgram.GetUniform("projection");
            viewUniform = simpleProgram.GetUniform("view");
            modelUniform = simpleProgram.GetUniform("model");
            if(colorUniform == -1 || projectionUniform == -1 
               || viewUniform == -1 || modelUniform == 1) {
                log.Error("Failed to get uniform locations");
                return false;
            }

            var cubeLines = Shapes.WireCube();
            cubeVbo = new VBO(cubeLines.Cast<VectorBasef>().ToList());
            cubeVao = new VAO();
            cubeVao.SetAttribute(positionLocation, cubeVbo, 4, VertexAttribPointerType.Float, false, 0, 0);

            var coneVerts = Shapes.SolidCone(30, -sixdofRadius * 1.5f, sixdofRadius);
            numConeVertices = coneVerts.Count;
            coneVbo = new VBO(coneVerts.Cast<VectorBasef>().ToList());
            coneVao = new VAO();
            coneVao.SetAttribute(positionLocation, coneVbo, 4, VertexAttribPointerType.Float, false, 0, 0);
            return true;
        }

        public override void PostRender() {
            simpleProgram.Begin();
            cubeVao.Bind();
            GL.UniformMatrix4(projectionUniform, 1, false, Projection);
            GL.UniformMatrix4(viewUniform, 1, false, View);
            GL.UniformMatrix4(modelUniform, 1, false, Matrix4f.Translate(0, 1, 0));
            GL.Uniform3(colorUniform, 1, 1, 1);
            GL.DrawArrays(PrimitiveType.Lines, 0, 24);

            coneVao.Bind();
            var deviceSixdofNames = InputManager.GetSixDOF.GetDeviceNames();
            foreach(var n in deviceSixdofNames) {
                var sixdof = InputManager.GetSixDOF.ByDeviceName(n);
                var m = sixdof.Matrix;
                if(highlightedSixdof == sixdof) {
                    GL.Uniform3(colorUniform, 0, 1, 0);
                }
                else {
                    GL.Uniform3(colorUniform, 0.4f, 0.4f, 0.4f);
                }
                GL.UniformMatrix4(modelUniform, 1, false, m);
                GL.DrawArrays(PrimitiveType.Triangles, 0, numConeVertices);
            }
            GL.BindVertexArray(0);
            simpleProgram.End();
        }

        public override void SetupInput() {
            window.MouseDown += (sender, e) => {
                var clicked = e.IsPressed;
                selectedSixdof = clicked ? highlightedSixdof : null;

                if(e.Button == MouseButton.Left) {
                    tilting &= clicked && !holdingShift;
                    rotating &= !clicked;
                    panning &= !clicked;
                    settingAnalog[0] = clicked && holdingShift;
                }

                else if(e.Button == MouseButton.Right) {
                    panning = clicked && !holdingShift;
                    rotating &= !clicked;
                    tilting &= !clicked;
                    settingAnalog[1] = clicked && holdingShift;
                }

                else if(e.Button == MouseButton.Middle) {
                    panning = clicked && !holdingShift;
                    rotating &= !clicked;
                    tilting &= !clicked;
                    settingAnalog[2] = clicked && holdingShift;
                }
            };

            window.MouseWheel += (sender, e) => {
                //var cameraDistance = View.Row(3).xyz.Length;
                //var newDistance = e.DeltaPrecise < 0.0f ? Math.Min(cameraDistance, 10.0f) : Math.Max(cameraDistance, 0.5f);
                var translation = Matrix4f.Translate(0, 0, e.DeltaPrecise);
                View = translation * View;
            };

            window.MouseMove += (sender, e) => {
                log.Info("mouse moved!!");
            };
        }

        public override void SetAnalogs(Analog[] a) {
            for(int i = 0; i < analogs.Length; i++) {
                analogs[i] = a[i];
            }
        }

        public static new SimulatorWindow Configure(cavr.config.Configuration config, DisplayDevice display = null) {
            var s = new SimulatorWindow();

            s.pivotDistance = 5.0;
            s.EyePosition = (new Vector3f(1, 1, 1)).Normalized() * (float) s.pivotDistance;
            s.View = Matrix4f.LookAt(s.EyePosition, new Vector3f(), new Vector3f(0, 1.0f, 0));

            return s;
        }
    }
}

