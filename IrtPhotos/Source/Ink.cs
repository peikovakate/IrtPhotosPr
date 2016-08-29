using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace IrtPhotos.Source
{
    class Ink
    {
        private InkCanvas inkCanvas;
        InkManager inkManager = new InkManager();
        private CanvasControl canvasControl;

        InkSynchronizer inkSynchronizer;

        IReadOnlyList<InkStroke> pendingDry;
        int deferredDryDelay;

        CanvasRenderTarget renderTarget;

        List<Point> selectionPolylinePoints;
        Rect? selectionBoundingRect;



        CanvasTextFormat textFormat;

        bool needToCreateSizeDependentResources;
        bool needToRedrawInkSurface;

        bool showTextLabels;

        public Ink(Canvas background)
        {
            canvasControl = new CanvasControl();
            canvasControl.Width = background.Width;
            canvasControl.Height = background.Height;
            canvasControl.CreateResources += CanvasControl_CreateResources;

            inkCanvas = new InkCanvas();
            inkCanvas.Width = background.Width;
            inkCanvas.Height = background.Height;



            canvasControl.Draw += CanvasControl_Draw;
            background.Children.Add(canvasControl);

            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Touch;
            inkCanvas.InkPresenter.InputProcessingConfiguration.RightDragAction = InkInputRightDragAction.LeaveUnprocessed;

            inkCanvas.InkPresenter.UnprocessedInput.PointerPressed += UnprocessedInput_PointerPressed;
            inkCanvas.InkPresenter.UnprocessedInput.PointerMoved += UnprocessedInput_PointerMoved;
            inkCanvas.InkPresenter.UnprocessedInput.PointerReleased += UnprocessedInput_PointerReleased;

            inkCanvas.InkPresenter.StrokeInput.StrokeStarted += StrokeInput_StrokeStarted;

            inkCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            inkCanvas.InkPresenter.StrokesErased += InkPresenter_StrokesErased;

            inkSynchronizer = inkCanvas.InkPresenter.ActivateCustomDrying();


            background.Children.Add(inkCanvas);
        }

        private void CanvasControl_CreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            renderTarget = new CanvasRenderTarget(canvasControl, canvasControl.Size);
        }

        private void CanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {

            //args.DrawingSession.Clear(Colors.Black);


            if (pendingDry != null && deferredDryDelay == 0)
            {
                // Incremental draw only.
                DrawStrokeCollectionToInkSurface(pendingDry);

                // Register to call EndDry on the next-but-one draw,
                // by which time our dry ink will be visible.
                deferredDryDelay = 1;
                CompositionTarget.Rendering += DeferredEndDry;

            }

            args.DrawingSession.DrawImage(renderTarget);


            //DrawSelectionBoundingRect(args.DrawingSession);
            //DrawSelectionLasso(sender, args.DrawingSession);
        }

        private void DeferredEndDry(object sender, object e)
        {
            Debug.Assert(pendingDry != null);

            if (deferredDryDelay > 0)
            {
                deferredDryDelay--;
            }
            else
            {
                CompositionTarget.Rendering -= DeferredEndDry;

                pendingDry = null;

                inkSynchronizer.EndDry();
            }
        }


        private void DrawStrokeCollectionToInkSurface(IReadOnlyList<InkStroke> strokes)
        {
            using (CanvasDrawingSession ds = renderTarget.CreateDrawingSession())
            {
                ds.DrawInk(strokes);
            }
        }

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            foreach (var s in args.Strokes)
            {
                inkManager.AddStroke(s);
            }
            Debug.Assert(pendingDry == null);
            pendingDry = inkSynchronizer.BeginDry();
            canvasControl.Invalidate();
        }

        private void StrokeInput_StrokeStarted(InkStrokeInput sender, Windows.UI.Core.PointerEventArgs args)
        {
            ClearSelection();

            canvasControl.Invalidate();
        }

        private void InkPresenter_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            var removed = args.Strokes;
            var strokeList = inkManager.GetStrokes().Except(removed).ToList();

            inkManager = new InkManager();
            strokeList.ForEach(inkManager.AddStroke);

            ClearSelection();

            canvasControl.Invalidate();
        }

        private void UnprocessedInput_PointerPressed(InkUnprocessedInput sender, Windows.UI.Core.PointerEventArgs args)
        {
            selectionPolylinePoints = new List<Point>();
            selectionPolylinePoints.Add(args.CurrentPoint.RawPosition);

            canvasControl.Invalidate();
        }

        private void UnprocessedInput_PointerMoved(InkUnprocessedInput sender, Windows.UI.Core.PointerEventArgs args)
        {
            selectionPolylinePoints.Add(args.CurrentPoint.RawPosition);

            canvasControl.Invalidate();
        }

        private void UnprocessedInput_PointerReleased(InkUnprocessedInput sender, Windows.UI.Core.PointerEventArgs args)
        {
            selectionPolylinePoints.Add(args.CurrentPoint.RawPosition);

            selectionBoundingRect = inkManager.SelectWithPolyLine(selectionPolylinePoints);

            selectionPolylinePoints = null;

            canvasControl.Invalidate();
        }

        private void ClearSelection()
        {
            selectionPolylinePoints = null;
            selectionBoundingRect = null;
        }
    }
}
