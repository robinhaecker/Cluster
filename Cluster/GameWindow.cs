using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

using System.Diagnostics;
using System.Threading;


using Cluster.Rendering.Appearance;
using Cluster.Mathematics;
using Cluster.Rendering.Draw2D;
using Cluster.GameMechanics;
using Cluster.GameMechanics.Universe;
using Cluster.GameMechanics.Content;
using Cluster.GameMechanics.Interface;
using Cluster.GameMechanics.Universe.CelestialBodies;
using Cluster.GameMechanics.Universe.LivingThings;

namespace Cluster
{


    sealed class GameWindow : OpenTK.GameWindow
    {


        //Allgemeines Zeug
        public const string BASE_FOLDER = "../../../data/";
        public static Random random;
        public static KeyboardState keyboard;
        public static MouseState mouse;
		public static Vec2 mousePos;
        public static GameWindow active;




        //Fensterinfos
        string _title;
        public int width, height;
		public float multX;
	    public float multY;
	    bool _fullscreen;


        //Performance-Messung
        int _renderTime;
	    int _swapbufferTime;
	    Stopwatch watch;


        //Rendering
		public FrameBuffer shadows;








		//1440 900

        public GameWindow(int width=1440, int height=900, string apptitle="Cluster")
            // set window resolution, title, and default behaviour
            : base(width, height, GraphicsMode.Default, apptitle,
            GameWindowFlags.FixedWindow, DisplayDevice.Default,
                // ask for an OpenGL 4.0 forward compatible context
            4, 0, GraphicsContextFlags.ForwardCompatible)
        {
            Console.WriteLine("OpenGL Version: " + GL.GetString(StringName.Version));

            active = this;
            _title = apptitle;
            this.width = width;
            this.height = height;

			multX = 1.0f / (float)width;
			multY = 1.0f / (float)height;

            watch = new Stopwatch();
            random = new Random();
            _renderTime = 0;

            keyboard = Keyboard.GetState();
            mouse = Mouse.GetState();
			mousePos = new Vec2(width*0.5f, height*0.5f);

			//Spielinhalte Laden
			Technology.init();
			Blueprint.init();
			Prototype.init();
	        CombinedGui.combinedGui.init();


            //Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(2); // Uses the second Core or Processor for the Test
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;  	// Prevents "Normal" processes from interrupting Threads
            Thread.CurrentThread.Priority = ThreadPriority.Highest;  	// Prevents "Normal" Threads from interrupting this thread
        }


        protected override void OnResize(EventArgs e)
        {
			GL.Viewport(0, 0, this.Width, this.Height);
			multX = 1.0f / (float)this.Width;
			multY = 1.0f / (float)this.Height;
			width = this.Width;
			height = this.Height;

        }

        protected override void OnLoad(EventArgs e)
        {
            // This is called when the window starts running.
			//####################################################################################################################################################################

            // Grundlegendes OpenGL-Zeug einstellen:
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			// Allgemeines Zeug initialisieren:
			Text.init();
			Primitives.init();
			Image.init();
			Mesh.init();

	        shadows = new FrameBuffer(512, 512);//width, height);//);
            glError("Init");

			//####################################################################################################################################################################

			// Spielinhalte laden
			UserInterface.init();
			Space.init();

			//####################################################################################################################################################################

			
            GameWindow.glError("SetupScene");
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // this is called every frame, put game logic here
            keyboard = Keyboard.GetState();
            mouse = Mouse.GetState();

            if (keyboard.IsKeyDown(Key.Escape))
            {
                Exit();
            }

			/*
			if (keyboard.IsKeyDown(Key.Space))
			{
				Planet.planets.Clear();
				new Planet(0.0f, 0.0f, 25);
			}*/


			Text.drawText("Render Time: " + _renderTime.ToString() + "\nSwap Buffer Time: " + _swapbufferTime.ToString(), width-200, 5);


			Space.update();
			CombinedGui.update();

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            watch.Reset();
            watch.Start();
            // Hier alles Render-relevante
            //##################################################################################################

            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);



			//shadowMatrix = cam.getShadowMatrix();
            //cam.render();
			//

			Space.render();
			Planet.render();
			Unit.render();
			Shot.render();
			Cluster.GameMechanics.Universe.Particle.render();
			CombinedGui.render();


			//PostProcessing.render();


			//2D Rendering (Text usw.)
			Primitives.renderPolys();
			Image.renderImages();
			Text.renderText();
			Mesh.render();


            //##################################################################################################
            // Ab hier nur noch Performance-Messung.
            _renderTime = (int)watch.ElapsedMilliseconds;
            this.SwapBuffers();
			_swapbufferTime = (int)watch.ElapsedMilliseconds;// -render_time;swapbuffer_time.ToString()
            watch.Stop();
			//Console.WriteLine("Render Time: " + render_time.ToString() + ", time to swap buffers: " + swapbuffer_time.ToString());

            glError("onRenderFrame");
        }




        static public void glError(string position = "")
        {
            ErrorCode err = GL.GetError();
            string text = err.ToString();
            Console.ForegroundColor = ConsoleColor.Red;

            if (position != "") position = " (" + position + ")";

            if (text != "NoError") Console.WriteLine("OpenGL-Error"+position+": " + err.ToString());
            Console.ForegroundColor = ConsoleColor.White;
        }


		protected override void OnMouseMove(MouseMoveEventArgs e)
		{
 			base.OnMouseMove(e);
			mousePos.x = e.X;
			mousePos.y = e.Y;
		}









    }
}