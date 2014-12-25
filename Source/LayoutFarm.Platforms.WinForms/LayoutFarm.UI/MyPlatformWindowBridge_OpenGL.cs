﻿
//2014 Apache2, WinterDev
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using LayoutFarm.Drawing;

using OpenTK.Graphics;

namespace LayoutFarm.UI
{
    class MyPlatformWindowBridgeOpenGL : MyPlatformWindowBridge
    {
        bool isInitGLControl;
        OpenTK.MyGLControl windowControl;
        Canvas canvas;
        public MyPlatformWindowBridgeOpenGL(TopWindowRenderBox topwin, IUserEventPortal winEventBridge)
            : base(topwin, winEventBridge)
        {
        }
        /// <summary>
        /// bind to gl control
        /// </summary>
        /// <param name="myGLControl"></param>
        public void BindGLControl(OpenTK.MyGLControl myGLControl)
        {
            this.canvas = LayoutFarm.Drawing.DrawingGL.CanvasGLPortal.P.CreateCanvas(0, 0, myGLControl.Width, myGLControl.Height);
              
            this.topwin.MakeCurrent();
            this.windowControl = myGLControl;
            this.canvasViewport = new CanvasViewport(topwin, this.Size.ToSize(), 4);

#if DEBUG
            this.canvasViewport.dbugOutputWindow = this;
#endif
            this.EvaluateScrollbar();
        }
        internal override void OnHostControlLoaded()
        {

            if (!isInitGLControl)
            {

                //init gl after this control is loaded
                //set myGLControl detail
                //1.
                windowControl.InitSetup2d(Screen.PrimaryScreen.Bounds);
                isInitGLControl = true;

                //2.
                windowControl.ClearColor = new OpenTK.Graphics.Color4(1f, 1f, 1f, 1f);
                //3.
                windowControl.SetGLPaintHandler(GLPaintMe);

            }
        }
        void GLPaintMe(Object sender, EventArgs e)
        {
            //gl paint here
            canvas.ClearSurface(Color.White);
            canvas.StrokeColor = LayoutFarm.Drawing.Color.Blue;
            canvas.DrawRectangle(Color.Blue, 20, 20, 200, 200);
            //------------------------
            windowControl.SwapBuffers();
        }
        protected override void PaintToOutputWindow()
        {
             if (isInitGLControl)
             {
                 GLPaintMe(null, null);
             }
        }
        protected override void PaintToOutputWindowIfNeed()
        {

        }
        protected override void ChangeCursorStyle(UIMouseEventArgs mouseEventArg)
        {

        }
        System.Drawing.Size Size
        {
            get { return this.windowControl.Size; }
        }

    }
}