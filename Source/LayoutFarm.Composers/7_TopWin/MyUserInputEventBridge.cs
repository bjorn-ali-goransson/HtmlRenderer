﻿//2014 Apache2, WinterDev
using System;
using System.Collections.Generic;
using System.Text;
using LayoutFarm.Drawing;

namespace LayoutFarm.UI
{

    public class MyUserInputEventBridge : UserInputEventBridge
    {

        readonly MyHitChain hitPointChain = new MyHitChain();
        UIHoverMonitorTask hoverMonitoringTask;

        int msgChainVersion;
        TopWindowRenderBox topwin;

        IEventListener currentKbFocusElem;
        IEventListener currentMouseActiveElement;
        IEventListener currentDragElem;

        public MyUserInputEventBridge()
        {
        }
        public override void Bind(TopWindowRenderBox topwin)
        {
            this.topwin = topwin;
            this.hoverMonitoringTask = new UIHoverMonitorTask(this.topwin, OnMouseHover);

#if DEBUG
            hitPointChain.dbugHitTracker = this.dbugRootGraphic.dbugHitTracker;
#endif
        }

#if DEBUG
        RootGraphic dbugRootGraphic
        {
            get { return topwin.Root; }
        }
#endif


        RootGraphic MyRootGraphic
        {
            get { return topwin.Root; }
        }
        //---------------------------------------------------------------------
        bool DisableGraphicOutputFlush
        {
            get { return this.MyRootGraphic.DisableGraphicOutputFlush; }
            set { this.MyRootGraphic.DisableGraphicOutputFlush = value; }
        }
        void FlushAccumGraphicUpdate()
        {
            this.MyRootGraphic.FlushAccumGraphicUpdate(this.topwin);
        }

        public IEventListener CurrentKeyboardFocusedElement
        {
            get
            {

                return this.currentKbFocusElem;
            }
            set
            {
                //1. lost keyboard focus
                if (this.currentKbFocusElem != null && this.currentKbFocusElem != value)
                {

                }
                //2. keyboard focus
                currentKbFocusElem = value;

                //1. send lost focus to existing ui

                //if (currentKeyboardFocusedElement != null)
                //{
                //    if (currentKeyboardFocusedElement == value)
                //    {
                //        return;
                //    }

                //    UIFocusEventArgs focusEventArg = eventStock.GetFreeFocusEventArgs(value, currentKeyboardFocusedElement);
                //    focusEventArg.SetWinRoot(topwin);
                //    eventStock.ReleaseEventArgs(focusEventArg);
                //}
                //currentKeyboardFocusedElement = value;
                //if (currentKeyboardFocusedElement != null)
                //{
                //    UIFocusEventArgs focusEventArg = eventStock.GetFreeFocusEventArgs(value, currentKeyboardFocusedElement);
                //    focusEventArg.SetWinRoot(topwin);
                //    Point globalLocation = value.GetGlobalLocation();
                //    kbFocusGlobalX = globalLocation.X;
                //    kbFocusGlobalY = globalLocation.Y;
                //    focusEventArg.SetWinRoot(topwin);
                //    eventStock.ReleaseEventArgs(focusEventArg);
                //    if (CurrentFocusElementChanged != null)
                //    {
                //        CurrentFocusElementChanged.Invoke(this, EventArgs.Empty);
                //    }
                //}
                //else
                //{
                //    kbFocusGlobalX = 0;
                //    kbFocusGlobalY = 0;
                //}
            }
        }


        public override void ClearAllFocus()
        {
            CurrentKeyboardFocusedElement = null;

        }
        void HitTestCoreWithPrevChainHint(int x, int y)
        {
            hitPointChain.SetStartTestPoint(x, y);
            RenderElement commonElement = hitPointChain.HitTestOnPrevChain();
            if (commonElement == null)
            {
                commonElement = this.topwin;
            }
            commonElement.HitTestCore(hitPointChain);
        }


        //--------------------------------------------------
        public override void OnDoubleClick(UIMouseEventArgs e)
        {

            HitTestCoreWithPrevChainHint(e.X, e.Y);
            ForEachEventListenerBubbleUp(this.hitPointChain, (hit, listener) =>
            {
                //on double click 
                return true;
            });

            hitPointChain.SwapHitChain();
        }
        public override void OnMouseWheel(UIMouseEventArgs e)
        {
            //only on mouse active element
            if (currentMouseActiveElement != null)
            {
                currentMouseActiveElement.ListenMouseEvent(UIMouseEventName.MouseWheel, e);
            }
        }


        public override void OnMouseDown(UIMouseEventArgs e)
        {

#if DEBUG
            if (this.dbugRootGraphic.dbugEnableGraphicInvalidateTrace)
            {
                this.dbugRootGraphic.dbugGraphicInvalidateTracer.WriteInfo("================");
                this.dbugRootGraphic.dbugGraphicInvalidateTracer.WriteInfo("MOUSEDOWN");
                this.dbugRootGraphic.dbugGraphicInvalidateTracer.WriteInfo("================");
            }
#endif
            msgChainVersion = 1;
            int local_msgVersion = 1;

            HitTestCoreWithPrevChainHint(e.X, e.Y);
            int hitCount = this.hitPointChain.Count;

            RenderElement hitElement = this.hitPointChain.CurrentHitElement as RenderElement;
            if (hitCount > 0)
            {
                DisableGraphicOutputFlush = true;
                //------------------------------
                //1. for some built-in event
           
                ForEachEventListenerPreviewBubbleUp(this.hitPointChain, (hitobj, listener) =>
                {

                    e.CurrentContextElement = listener;
                    listener.PortalMouseDown(e);

                    var curContextElement = e.CurrentContextElement as IEventListener;
                    if (curContextElement != null && curContextElement.AcceptKeyboardFocus)
                    {
                        this.CurrentKeyboardFocusedElement = curContextElement;
                    }


                    return true;
                });
                //------------------------------
                //use events
                if (!e.CancelBubbling)
                {
                  
                    ForEachEventListenerBubbleUp(this.hitPointChain, (hitobj, listener) =>
                    {
                        e.Location = hitobj.Location;
                        e.SourceHitElement = hitobj.hitObject;
                        e.CurrentContextElement = listener;
                        

                        listener.ListenMouseEvent(UIMouseEventName.MouseDown, e);

                        var curContextElement = e.CurrentContextElement as IEventListener;
                        if (curContextElement != null && curContextElement.AcceptKeyboardFocus)
                        {
                            this.CurrentKeyboardFocusedElement = curContextElement;
                        }
                        return true;
                    });
                }
            }
            //---------------------------------------------------------------

#if DEBUG
            RootGraphic visualroot = this.dbugRootGraphic;
            if (visualroot.dbug_RecordHitChain)
            {
                visualroot.dbug_rootHitChainMsg.Clear();
                int i = 0;
                //foreach (HitPoint hp in hitPointChain.dbugGetHitPairIter())
                //{

                //    RenderElement ve = hp.hitObject as RenderElement;
                //    if (ve != null)
                //    {
                //        ve.dbug_WriteOwnerLayerInfo(visualroot, i);
                //        ve.dbug_WriteOwnerLineInfo(visualroot, i);

                //        string hit_info = new string('.', i) + " [" + i + "] "
                //            + "(" + hp.point.X + "," + hp.point.Y + ") "
                //            + ve.dbug_FullElementDescription();
                //        visualroot.dbug_rootHitChainMsg.AddLast(new dbugLayoutMsg(ve, hit_info));
                //    }
                //    i++;
                //}
            }
#endif
            hitPointChain.SwapHitChain();

            //if (hitElement.ParentLink == null)
            //{
            //    currentMouseActiveElement = null;
            //    return;
            //}

            if (local_msgVersion != msgChainVersion)
            {
                return;
            }
            //if (hitElement.Focusable)
            //{
            //    this.CurrentKeyboardFocusedElement = hitElement.GetController() as IEventListener;
            //}
            DisableGraphicOutputFlush = false;
            FlushAccumGraphicUpdate();

#if DEBUG
            visualroot.dbugHitTracker.Write("stop-mousedown");
            visualroot.dbugHitTracker.Play = false;
#endif

        }

        public override void OnMouseMove(UIMouseEventArgs e)
        {

            HitTestCoreWithPrevChainHint(e.X, e.Y);
            //-------------------------------------------------------
            //when mousemove -> reset hover!
            hoverMonitoringTask.Reset();
            hoverMonitoringTask.SetEnable(true, this.topwin);
            //-------------------------------------------------------
            DisableGraphicOutputFlush = true;
           
            ForEachEventListenerPreviewBubbleUp(this.hitPointChain, (hitobj, listener) =>
            {
                return false;
            });
            //-------------------------------------------------------
         
            ForEachEventListenerBubbleUp(this.hitPointChain, (hitobj, listener) =>
            {
                if (currentMouseActiveElement != null && currentMouseActiveElement != listener)
                {
                    e.Location = hitobj.Location;
                    currentMouseActiveElement.ListenMouseEvent(
                        UIMouseEventName.MouseLeave, e);
                    currentMouseActiveElement = null;
                }
                if (currentMouseActiveElement == listener)
                {
                    currentMouseActiveElement.ListenMouseEvent(
                           UIMouseEventName.MouseMove, e);
                }
                else
                {
                    currentMouseActiveElement = listener;
                    currentMouseActiveElement.ListenMouseEvent(
                          UIMouseEventName.MouseEnter, e);
                }

                return true;//stop
            });
            DisableGraphicOutputFlush = false;
            hitPointChain.SwapHitChain();
        }
        void OnMouseHover(object sender, EventArgs e)
        {
            return;
            //HitTestCoreWithPrevChainHint(hitPointChain.LastestRootX, hitPointChain.LastestRootY);
            //RenderElement hitElement = this.hitPointChain.CurrentHitElement as RenderElement;
            //if (hitElement != null && hitElement.IsTestable)
            //{
            //    DisableGraphicOutputFlush = true;
            //    Point hitElementGlobalLocation = hitElement.GetGlobalLocation();

            //    UIMouseEventArgs e2 = new UIMouseEventArgs();
            //    e2.WinTop = this.topwin;
            //    e2.Location = hitPointChain.CurrentHitPoint;
            //    e2.SourceHitElement = hitElement;
            //    IEventListener ui = hitElement.GetController() as IEventListener;
            //    if (ui != null)
            //    {
            //        ui.ListenMouseEvent(UIMouseEventName.MouseHover, e2);
            //    }

            //    DisableGraphicOutputFlush = false;
            //    FlushAccumGraphicUpdate();
            //}
            //hitPointChain.SwapHitChain();
            //hoverMonitoringTask.SetEnable(false, this.topwin);
        }
        public override void OnDragStart(UIMouseEventArgs e)
        {

#if DEBUG
            if (this.dbugRootGraphic.dbugEnableGraphicInvalidateTrace)
            {
                this.dbugRootGraphic.dbugGraphicInvalidateTracer.WriteInfo("================");
                this.dbugRootGraphic.dbugGraphicInvalidateTracer.WriteInfo("START_DRAG");
                this.dbugRootGraphic.dbugGraphicInvalidateTracer.WriteInfo("================");
            }
#endif

            HitTestCoreWithPrevChainHint(
              hitPointChain.LastestRootX,
              hitPointChain.LastestRootY);

            DisableGraphicOutputFlush = true;
            this.currentDragElem = null;

            //-----------------------------------------------------------------------
           
            ForEachEventListenerPreviewBubbleUp(this.hitPointChain, (hitobj, listener) =>
            {                 
                listener.PortalMouseMove(e);
                return true;
            });

            //-----------------------------------------------------------------------
             
            ForEachEventListenerBubbleUp(this.hitPointChain, (hit, listener) =>
            {
                currentDragElem = listener;
                listener.ListenDragEvent(UIDragEventName.DragStart, e);
                return true;
            });
            DisableGraphicOutputFlush = false;
            FlushAccumGraphicUpdate();
            //currentDragingElement = this.hitPointChain.CurrentHitElement;

            //if (currentDragingElement != null &&
            //    currentDragingElement != this.topwin)
            //{
            //    DisableGraphicOutputFlush = true;
            //    Point globalLocation = currentDragingElement.GetGlobalLocation();
            //    e.TranslateCanvasOrigin(globalLocation);
            //    e.Location = hitPointChain.CurrentHitPoint;
            //    e.DragingElement = currentDragingElement;
            //    e.SourceHitElement = currentDragingElement;



            //    IEventListener ui = currentDragingElement.GetController() as IEventListener;
            //    if (ui != null)
            //    {
            //        ui.ListenDragEvent(UIDragEventName.DragStart, e);
            //    }
            //    e.TranslateCanvasOriginBack();
            //    DisableGraphicOutputFlush = false;
            //    FlushAccumGraphicUpdate();

            //}
            hitPointChain.SwapHitChain();
        }
        public override void OnDrag(UIMouseEventArgs e)
        {
            if (currentDragElem == null)
            {
                return;
            }

#if DEBUG
            this.dbugRootGraphic.dbugEventIsDragging = true;
#endif

            //if (currentDragingElement == null)
            //{

            //    return;
            //}
            //else
            //{
            //}

            //--------------

            DisableGraphicOutputFlush = true;

            currentDragElem.ListenDragEvent(UIDragEventName.Dragging, e);

            DisableGraphicOutputFlush = false;
            FlushAccumGraphicUpdate();

            //Point globalDragingElementLocation = currentDragingElement.GetGlobalLocation();
            //e.TranslateCanvasOrigin(globalDragingElementLocation);
            //e.SourceHitElement = currentDragingElement;
            //Point dragPoint = hitPointChain.PrevHitPoint;
            //dragPoint.Offset(currentXDistanceFromDragPoint, currentYDistanceFromDragPoint);
            //e.Location = dragPoint;
            //e.DragingElement = currentDragingElement;

            //IEventListener ui = currentDragingElement.GetController() as IEventListener;
            //if (ui != null)
            //{
            //    ui.ListenDragEvent(UIDragEventName.Dragging, e);
            //}
            //e.TranslateCanvasOriginBack();


        }


        void BroadcastDragHitEvents(UIMouseEventArgs e)
        {


            //Point globalDragingElementLocation = currentDragingElement.GetGlobalLocation();
            //Rectangle dragRect = currentDragingElement.GetGlobalRect();

            //VisualDrawingChain drawingChain = this.WinRootPrepareRenderingChain(dragRect);

            //List<RenderElement> selVisualElements = drawingChain.selectedVisualElements;
            //int j = selVisualElements.Count;
            //LinkedList<RenderElement> underlyingElements = new LinkedList<RenderElement>();
            //for (int i = j - 1; i > -1; --i)
            //{

            //    if (selVisualElements[i].ListeningDragEvent)
            //    {
            //        underlyingElements.AddLast(selVisualElements[i]);
            //    }
            //}

            //if (underlyingElements.Count > 0)
            //{
            //    foreach (RenderElement underlyingUI in underlyingElements)
            //    {

            //        if (underlyingUI.IsDragedOver)
            //        {   
            //            hitPointChain.RemoveDragHitElement(underlyingUI);
            //            underlyingUI.IsDragedOver = false;
            //        }
            //    }
            //}
            //UIMouseEventArgs d_eventArg = UIMouseEventArgs.GetFreeDragEventArgs();

            //if (hitPointChain.DragHitElementCount > 0)
            //{
            //    foreach (RenderElement elem in hitPointChain.GetDragHitElementIter())
            //    {
            //        Point globalLocation = elem.GetGlobalLocation();
            //        d_eventArg.TranslateCanvasOrigin(globalLocation);
            //        d_eventArg.SourceVisualElement = elem;
            //        var script = elem.GetController();
            //        if (script != null)
            //        {
            //        }
            //        d_eventArg.TranslateCanvasOriginBack();
            //    }
            //}
            //hitPointChain.ClearDragHitElements();

            //foreach (RenderElement underlyingUI in underlyingElements)
            //{

            //    hitPointChain.AddDragHitElement(underlyingUI);
            //    if (underlyingUI.IsDragedOver)
            //    {
            //        Point globalLocation = underlyingUI.GetGlobalLocation();
            //        d_eventArg.TranslateCanvasOrigin(globalLocation);
            //        d_eventArg.SourceVisualElement = underlyingUI;

            //        var script = underlyingUI.GetController();
            //        if (script != null)
            //        {
            //        }

            //        d_eventArg.TranslateCanvasOriginBack();
            //    }
            //    else
            //    {
            //        underlyingUI.IsDragedOver = true;
            //        Point globalLocation = underlyingUI.GetGlobalLocation();
            //        d_eventArg.TranslateCanvasOrigin(globalLocation);
            //        d_eventArg.SourceVisualElement = underlyingUI;

            //        var script = underlyingUI.GetController();
            //        if (script != null)
            //        {
            //        }

            //        d_eventArg.TranslateCanvasOriginBack();
            //    }
            //}
            //UIMouseEventArgs.ReleaseEventArgs(d_eventArg);
        }
        public override void OnDragStop(UIMouseEventArgs e)
        {

            if (currentDragElem == null)
            {
                return;
            }
#if DEBUG
            this.dbugRootGraphic.dbugEventIsDragging = false;
#endif

            DisableGraphicOutputFlush = true;

            currentDragElem.ListenDragEvent(UIDragEventName.DragStop, e);

            DisableGraphicOutputFlush = false;
            FlushAccumGraphicUpdate();

            //if (currentDragingElement == null)
            //{
            //    return;
            //}

            //DisableGraphicOutputFlush = true;

            //Point globalDragingElementLocation = currentDragingElement.GetGlobalLocation();
            //e.TranslateCanvasOrigin(globalDragingElementLocation);

            //Point dragPoint = hitPointChain.PrevHitPoint;
            //dragPoint.Offset(currentXDistanceFromDragPoint, currentYDistanceFromDragPoint);
            //e.Location = dragPoint;

            //e.SourceHitElement = currentDragingElement;
            //var script = currentDragingElement.GetController() as IEventListener;
            //if (script != null)
            //{
            //    script.ListenDragEvent(UIDragEventName.DragStop, e);
            //}

            //e.TranslateCanvasOriginBack();

            //UIMouseEventArgs d_eventArg = new UIMouseEventArgs();
            //if (hitPointChain.DragHitElementCount > 0)
            //{
            //    ForEachDraggingObjects(this.hitPointChain, (hitobj, listener) =>
            //    {
            //        //d_eventArg.TranslateCanvasOrigin(globalLocation);
            //        //d_eventArg.SourceHitElement = elem;
            //        //d_eventArg.DragingElement = currentDragingElement;

            //        //var script2 = elem.GetController();
            //        //if (script2 != null)
            //        //{
            //        //}

            //        //d_eventArg.TranslateCanvasOriginBack();
            //        return true;
            //    });
            //    //foreach (RenderElement elem in hitPointChain.GetDragHitElementIter())
            //    //{
            //    //    Point globalLocation = elem.GetGlobalLocation();
            //    //    d_eventArg.TranslateCanvasOrigin(globalLocation);
            //    //    d_eventArg.SourceHitElement = elem;
            //    //    d_eventArg.DragingElement = currentDragingElement;

            //    //    var script2 = elem.GetController();
            //    //    if (script2 != null)
            //    //    {
            //    //    }

            //    //    d_eventArg.TranslateCanvasOriginBack();
            //    //}
            //} 
            DisableGraphicOutputFlush = false;
            FlushAccumGraphicUpdate();
        }
        public override void OnGotFocus(UIFocusEventArgs e)
        {


        }
        public override void OnLostFocus(UIFocusEventArgs e)
        {

        }
        public override void OnMouseUp(UIMouseEventArgs e)
        {

#if DEBUG

            if (this.dbugRootGraphic.dbugEnableGraphicInvalidateTrace)
            {
                this.dbugRootGraphic.dbugGraphicInvalidateTracer.WriteInfo("================");
                this.dbugRootGraphic.dbugGraphicInvalidateTracer.WriteInfo("MOUSEUP");
                this.dbugRootGraphic.dbugGraphicInvalidateTracer.WriteInfo("================");
            }
#endif

            HitTestCoreWithPrevChainHint(e.X, e.Y);

            int hitCount = this.hitPointChain.Count;
            if (hitCount > 0)
            {

                DisableGraphicOutputFlush = true;
                //---------------------------------------------------------------
                
                ForEachEventListenerPreviewBubbleUp(this.hitPointChain, (hitobj, listener) =>
                {
                    e.CurrentContextElement = listener;
                     
                    listener.PortalMouseUp(e);
                    var curContextElement = e.CurrentContextElement as IEventListener;
                    if (curContextElement != null && curContextElement.AcceptKeyboardFocus)
                    {
                        this.CurrentKeyboardFocusedElement = curContextElement;
                    }
                    return true;
                });
                //---------------------------------------------------------------
               
                ForEachEventListenerBubbleUp(this.hitPointChain, (hitobj, listener) =>
                {
                    e.Location = hitobj.Location;
                    e.SourceHitElement = hitobj.hitObject;
                    e.CurrentContextElement = hitobj.hitObject;

                    listener.ListenMouseEvent(UIMouseEventName.MouseUp, e);

                    var curContextElement = e.CurrentContextElement as IEventListener;
                    if (curContextElement != null && curContextElement.AcceptKeyboardFocus)
                    {
                        this.CurrentKeyboardFocusedElement = curContextElement;
                    }
                    return true;
                });
                //---------------------------------------------------------------
                DisableGraphicOutputFlush = false;
                FlushAccumGraphicUpdate();
            }

            hitPointChain.SwapHitChain();
        }
        public override void OnKeyDown(UIKeyEventArgs e)
        {
            if (currentKbFocusElem != null)
            {
                e.SourceHitElement = currentKbFocusElem;
                currentKbFocusElem.ListenKeyEvent(UIKeyEventName.KeyDown, e);
            }
        }
        public override void OnKeyUp(UIKeyEventArgs e)
        {
            if (currentKbFocusElem != null)
            {
                e.SourceHitElement = currentKbFocusElem;
                currentKbFocusElem.ListenKeyEvent(UIKeyEventName.KeyUp, e);
            }
        }
        public override void OnKeyPress(UIKeyEventArgs e)
        {

            if (currentKbFocusElem != null)
            {
                e.SourceHitElement = currentKbFocusElem;
                currentKbFocusElem.ListenKeyPressEvent(e);
            }
        }
        public override bool OnProcessDialogKey(UIKeyEventArgs e)
        {
            bool result = false;
            if (currentKbFocusElem != null)
            {
                e.SourceHitElement = currentKbFocusElem;
                result = currentKbFocusElem.ListenProcessDialogKey(e);
            }
            return result;
        }

        //===================================================================
        delegate bool EventListenerAction(HitInfo hitInfo, IEventListener listener);
        delegate bool EventPortalAction(HitInfo hitInfo, IUserEventPortal evPortal);

        struct HitInfo
        {
            public readonly object hitObject;
            public readonly int x;
            public readonly int y;
            public HitInfo(object hitObject, int x, int y)
            {
                this.x = x;
                this.y = y;
                this.hitObject = hitObject;
            }
            public Point Location
            {
                get { return new Point(x, y); }
            }
        }

        static void ForEachEventListenerPreviewBubbleUp(MyHitChain hitPointChain, EventPortalAction evaluateListener)
        {
            //only listener that need tunnel down 
            for (int i = hitPointChain.Count - 1; i >= 0; --i)
            {
                HitPoint hitPoint = hitPointChain.GetHitPoint(i);
                RenderElement hitElem = hitPoint.hitObject as RenderElement;
                if (hitElem != null)
                {
                    IUserEventPortal listener = hitElem.GetController() as IUserEventPortal;
                    if (listener != null &&
                        evaluateListener(new HitInfo(hitElem, hitPoint.point.X, hitPoint.point.Y), listener))
                    {
                        return;
                    }
                }
                //else
                //{
                //    var boxChain = hitPoint.hitObject as HtmlRenderer.Boxes.CssBoxHitChain;
                //    if (boxChain != null)
                //    {
                //        //loop 
                //        for (int n = boxChain.Count - 1; n >= 0; --n)
                //        {
                //            var hitInfo = boxChain.GetHitInfo(n);
                //            var cssbox = hitInfo.hitObject as HtmlRenderer.Boxes.CssBox;
                //            if (cssbox != null)
                //            {
                //                var listener = HtmlRenderer.Boxes.CssBox.UnsafeGetController(cssbox) as IEventListener;
                //                if (listener != null && listener.NeedPreviewBubbleUp &&
                //                    evaluateListener(new HitInfo(cssbox, hitInfo.localX, hitInfo.localY), listener))
                //                {
                //                    return;
                //                }
                //            }
                //        }

                //    }
                //}
            }

        }
        static void ForEachEventListenerBubbleUp(MyHitChain hitPointChain, EventListenerAction evaluateListener)
        {

            for (int i = hitPointChain.Count - 1; i >= 0; --i)
            {
                HitPoint hitPoint = hitPointChain.GetHitPoint(i);
                RenderElement hitElem = hitPoint.hitObject as RenderElement;

                if (hitElem != null)
                {
                    IEventListener listener = hitElem.GetController() as IEventListener;
                    if (listener != null && evaluateListener(new HitInfo(hitElem, hitPoint.point.X, hitPoint.point.Y), listener))
                    {
                        return;
                    }
                }
                else
                {
                    var boxChain = hitPoint.hitObject as HtmlRenderer.Boxes.CssBoxHitChain;

                    if (boxChain != null)
                    {
                        //loop 
                        for (int n = boxChain.Count - 1; n >= 0; --n)
                        {
                            var hitInfo = boxChain.GetHitInfo(n);
                            var cssbox = hitInfo.hitObject as HtmlRenderer.Boxes.CssBox;
                            if (cssbox != null)
                            {
                                var listener = HtmlRenderer.Boxes.CssBox.UnsafeGetController(cssbox) as IEventListener;
                                if (listener != null &&
                                    evaluateListener(new HitInfo(cssbox, hitInfo.localX, hitInfo.localY), listener))
                                {
                                    return;
                                }
                            }
                        }

                    }
                }
            }
        }



    }


}