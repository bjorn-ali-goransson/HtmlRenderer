﻿
//2014 Apache2, WinterDev
using System;
using System.Collections.Generic;
using System.Text;
using LayoutFarm.Drawing;
namespace LayoutFarm
{


    public abstract class TopWindowRenderBox : RenderBoxBase
    {
         
        VisualPlainLayer groundLayer;
        public event EventHandler<EventArgs> CanvasForcePaint;
        public TopWindowRenderBox(RootGraphic rootGfx, int width, int height)
            : base(rootGfx, width, height)
        {
            
            groundLayer = new VisualPlainLayer(this);
            this.Layers = new VisualLayerCollection();
            this.Layers.AddLayer(groundLayer);
            SetIsWindowRoot(this, true);
            this.HasSpecificSize = true;
        }
        
        public abstract void SetCanvasInvalidateRequest(CanvasInvalidateRequestDelegate canvasInvaliddateReqDel);
        public abstract void ChangeRootGraphicSize(int width, int height);

        public void AddChild(RenderElement renderE)
        {
            groundLayer.AddChild(renderE);
        }
        public void ForcePaint()
        {
            if (this.CanvasForcePaint != null)
            {
                CanvasForcePaint(this, EventArgs.Empty);
            }
        }     

        
        protected override void BoxDrawContent(Canvas canvasPage, Rect updateArea)
        {
            canvasPage.FillRectangle(Color.White, new RectangleF(0, 0, this.Width, this.Height));
            base.BoxDrawContent(canvasPage, updateArea);
        }
       
        
        //----------------------------------------------------------------------------
        //public abstract void RootBeginGraphicUpdate();
        //public abstract void RootEndGraphicUpdate();
        public abstract void AddToLayoutQueue(RenderElement vs);
        public abstract void FlushGraphic(Rectangle rect);
        public abstract void PrepareRender();
        //---------------------------------------------------------------------------- 
        public override void ClearAllChildren()
        {
            this.groundLayer.Clear();
        }
        public void MakeCurrent()
        {
            CurrentTopWindowRenderBox = this;
        }
        public static TopWindowRenderBox CurrentTopWindowRenderBox
        {
            get;
            set;
        }
      
#if DEBUG
        public abstract void dbugShowRenderPart(Canvas canvasPage, Rect updateArea);
        public RootGraphic dbugVisualRoot
        {
            get
            {
                return this.Root;
            }
        }
#endif
    }
}