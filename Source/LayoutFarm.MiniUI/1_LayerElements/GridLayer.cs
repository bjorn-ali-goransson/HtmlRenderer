﻿//2014 Apache2, WinterDev
using System;
using System.Collections.Generic;
using LayoutFarm.Drawing;
using System.Text;
using LayoutFarm;

namespace LayoutFarm
{

    public sealed class GridLayer : VisualLayer
    {
        GridTable.GridRowCollection gridRows;
        GridTable.GridColumnCollection gridCols;
        int uniformCellWidth;
        int uniformCellHeight;
        CellSizeStyle cellSizeStyle;
        GridTable gridTable;

        public GridLayer(RenderElement owner, int nColumns, int nRows, CellSizeStyle flexgridType)
        {
            this.OwnerRenderElement = owner;
            this.cellSizeStyle = flexgridType;
            this.gridTable = new GridTable();

            gridRows = gridTable.Rows;
            gridCols = gridTable.Columns;

            int columnWidth = owner.Width;
            if (nColumns > 0)
            {
                columnWidth = columnWidth / nColumns;
                uniformCellWidth = columnWidth;
                if (columnWidth < 1)
                {
                    columnWidth = 1;
                }
            }

            //------------------------------------------------------------             
            int cx = 0;
            for (int c = 0; c < nColumns; c++)
            {
                GridColumn col = new GridColumn(columnWidth);
                col.Width = columnWidth;
                col.Left = cx;

                cx += columnWidth;
                gridCols.Add(col);
            }
            //------------------------------------------------------------

            if (nRows > 0)
            {
                int rowHeight = owner.Height / nRows;
                int cy = 0;
                for (int r = 0; r < nRows; r++)
                {
                    var row = new GridRow(rowHeight);
                    gridRows.Add(row);

                    row.Height = rowHeight;
                    row.Top = cy;
                    cy += rowHeight;
                }
                uniformCellHeight = rowHeight;
            }
            //------------------------------------------------------------
        }
        public override bool HitTestCore(HitChain hitChain)
        {
            int testX;
            int testY;
            hitChain.GetTestPoint(out testX, out testY);
            GridCell cell = GetGridItemByPosition(testX, testY);

            if (cell != null && cell.HasContent)
            {
                hitChain.OffsetTestPoint(-cell.X, -cell.Y);
                var renderE = cell.ContentElement as RenderElement;
                if (renderE != null)
                {
                    renderE.HitTestCore(hitChain);
                }

                hitChain.OffsetTestPoint(cell.X, cell.Y);
                return true;
            }
            return false;
        }
        public override void Clear()
        {
            //clear content in each rows and columns

        }
        public int RowCount
        {
            get
            {
                return gridRows.Count;
            }
        }
        public override void TopDownReArrangeContent()
        {

#if DEBUG
            vinv_dbug_EnterLayerReArrangeContent(this);
#endif
            //--------------------------------- 
            this.BeginLayerLayoutUpdate();
            //---------------------------------
            if (gridCols != null && gridCols.Count > 0)
            {


                int curY = 0;
                foreach (GridRow rowDef in gridRows.GetRowIter())
                {

                    rowDef.AcceptDesiredHeight(curY);
                    curY += rowDef.Height;
                }

                int curX = 0;
                foreach (GridColumn gridCol in gridCols.GetColumnIter())
                {

                    SetLeftAndPerformArrange(gridCol, curX);
                    curX += gridCol.Width;
                }
            }

            ValidateArrangement();
            //---------------------------------
            this.EndLayerLayoutUpdate();

#if DEBUG
            vinv_dbug_ExitLayerReArrangeContent();
#endif
        }


        static void SetLeftAndPerformArrange(GridColumn col, int left)
        {

            int prevWidth = col.Width;
            if (!col.HasCustomSize)
            {
                col.Width = col.CalculatedWidth;
            }
            col.Left = left;
            int j = col.CellCount;
            int dW = col.Width;
            for (int i = 0; i < j; ++i)
            {

                var content = col.GetCell(i).ContentElement as RenderElement;
                if (content != null)
                {
                    //RenderElement.DirectSetVisualElementWidth(content, dW);
                    //if (content.IsVisualContainerBase)
                    //{

                    //    ArtVisualContainerBase vscont = (ArtVisualContainerBase)content;
                    //    vscont.InvalidateContentArrangementFromContainerSizeChanged();
                    //    vscont.TopDownReArrangeContentIfNeed(vinv);
                    //} 
                }
            }
        }
        public override IEnumerable<RenderElement> GetRenderElementIter()
        {

            if (gridCols != null && gridCols.Count > 0)
            {
                foreach (GridColumn gridCol in gridCols.GetColumnIter())
                {

                    foreach (var gridCell in gridCol.GetTopDownGridCellIter())
                    {
                        var re = gridCell.ContentElement as RenderElement;
                        if (re != null)
                        {
                            yield return re;
                        }
                    }
                }
            }
        }
        public override IEnumerable<RenderElement> GetRenderElementReverseIter()
        {
            if (gridCols != null && gridCols.Count > 0)
            {
                foreach (GridColumn gridCol in gridCols.GetColumnReverseIter())
                {
                    foreach (var gridCell in gridCol.GetTopDownGridCellIter())
                    {
                        var re = gridCell.ContentElement as RenderElement;
                        if (re != null)
                        {
                            yield return re;
                        }
                    }

                }
            }
        }


        public void ChangeColumnWidth(GridColumn targetGridColumn, int newWidth)
        {

            targetGridColumn.Width = newWidth;

            GridColumn prevColumn = targetGridColumn;
            GridColumn currentColumn = targetGridColumn.NextColumn;
            while (currentColumn != null)
            {
                currentColumn.Left = prevColumn.Right;
                prevColumn = currentColumn;
                currentColumn = currentColumn.NextColumn;
            }

            this.OwnerInvalidateGraphicAndStartBubbleUp();
        }
        public int UniformCellWidth
        {
            get
            {
                return uniformCellWidth;
            }
        }
        public int UniformCellHeight
        {
            get
            {
                return uniformCellHeight;
            }
        }

        public CellSizeStyle GridType
        {
            get
            {
                return cellSizeStyle;

            }
        }

        public GridCell GetGridItemByPosition(int x, int y)
        {
            if (y < 0)
            {
                y = 0;
            }
            if (x < 0)
            {
                x = 0;
            }

            switch (cellSizeStyle)
            {
                case CellSizeStyle.UniformWidth:
                    {
                        GridRow row = gridRows.GetRowAtPos(y);
                        if (row != null)
                        {

                            int columnNumber = x / uniformCellWidth;
                            if (columnNumber >= gridCols.Count)
                            {
                                columnNumber = gridCols.Count - 1;
                            }

                            GridColumn column = gridCols[columnNumber];
                            if (column == null)
                            {
                                column = gridCols.Last;
                            }
                            if (column != null)
                            {
                                return column.GetCell(row.RowIndex);
                            }
                        }
                    } break;
                case CellSizeStyle.UniformHeight:
                    {

                        int rowNumber = y / uniformCellHeight;
                        if (rowNumber >= gridRows.Count)
                        {
                            rowNumber = gridRows.Count - 1;
                        }
                        GridRow row = gridRows[rowNumber];
                        if (row != null)
                        {
                            GridColumn column = gridCols.GetColumnAtPosition(x);
                            if (column == null)
                            {
                                column = gridCols.Last;
                            }
                            if (column != null)
                            {
                                return column.GetCell(row.RowIndex);
                            }
                        }
                    } break;
                case CellSizeStyle.UniformCell:
                    {

                        int rowNumber = y / uniformCellHeight;
                        if (rowNumber >= gridRows.Count)
                        {
                            rowNumber = gridRows.Count - 1;
                        }
                        GridRow row = gridRows[rowNumber];

                        if (row != null)
                        {

                            int columnNumber = x / uniformCellWidth;

                            if (columnNumber >= gridCols.Count)
                            {
                                columnNumber = gridCols.Count - 1;
                            }
                            GridColumn column = gridCols[columnNumber];

                            if (column == null)
                            {
                                column = gridCols.Last;
                            }
                            if (column != null)
                            {
                                return column.GetCell(row.RowIndex);
                            }
                        }

                    } break;
                default:
                    {
                        GridRow row = gridRows.GetRowAtPos(y);
                        if (row == null)
                        {

                            row = gridRows.Last;
                        }
                        if (row != null)
                        {
                            GridColumn column = gridCols.GetColumnAtPosition(x);

                            if (column == null)
                            {
                                column = gridCols.Last;
                            }
                            if (column != null)
                            {
                                return column.GetCell(row.RowIndex);
                            }
                        }
                    } break;
            }
            return null;
        }
        public GridCell GetCell(int rowIndex, int columnIndex)
        {

            return gridCols[columnIndex].GetCell(rowIndex);
        }

        public void AdjustGridWidth(int nWidthDiff)
        {


            int j = gridCols.Count;
            if (j > 0)
            {
                //just average
                int avgWidth = nWidthDiff / j;
                if (avgWidth > 0)
                {
                    for (int i = j - 1; i > -1; i--)
                    {

                        gridCols[i].Width += avgWidth;

                    }
                }
            }
        }


        public void SetUniformGridItemSize(int cellItemWidth, int cellItemHeight)
        {
            switch (cellSizeStyle)
            {
                case CellSizeStyle.UniformCell:
                    {
                        uniformCellWidth = cellItemWidth;
                        uniformCellHeight = cellItemHeight;
                    } break;
                case CellSizeStyle.UniformHeight:
                    {
                        uniformCellHeight = cellItemHeight;
                    } break;
                case CellSizeStyle.UniformWidth:
                    {
                        uniformCellWidth = cellItemWidth;
                    } break;

            }
        }
        internal GridTable.GridRowCollection Rows
        {
            get
            {
                return gridRows;
            }
        }
        internal GridTable.GridColumnCollection Columns
        {
            get
            {
                return gridCols;
            }
        }
        public void AddNewColumn(int initColumnWidth)
        {
            gridCols.Add(new GridColumn(initColumnWidth));
        }
        public void AddColumn(GridColumn col)
        {
            gridCols.Add(col);
        }
        public void InsertColumn(int index, GridColumn col)
        {
            gridCols.Insert(index, col);
        }
        public void InsertRowAfter(GridRow afterThisRow, GridRow row)
        {
            gridRows.InsertAfter(afterThisRow, row);
        }
        public GridColumn GetColumnByPosition(int x)
        {
            return gridCols.GetColumnAtPosition(x);
        }
        public GridRow GetRowByPosition(int y)
        {
            return gridRows.GetRowAtPos(y);
        }

        public void AddRow(GridRow row)
        {
            gridRows.Add(row);
        }

        public GridRow GetRow(int index)
        {
            return gridRows[index];
        }

        public GridColumn GetColumn(int index)
        {
            return gridCols[index];
        }
        public void AddNewRow(int initRowHeight)
        {

            gridRows.Add(new GridRow(initRowHeight));
        }


        public int ColumnCount
        {
            get
            {
                return gridCols.Count;
            }
        }
        public void MoveRowAfter(GridRow fromRow, GridRow toRow)
        {
            this.gridRows.MoveRowAfter(fromRow, toRow);
            this.OwnerInvalidateGraphic();

        }
        public void MoveColumnAfter(GridColumn tobeMoveColumn, GridColumn afterColumn)
        {
            this.gridCols.MoveColumnAfter(tobeMoveColumn, afterColumn);
            this.OwnerInvalidateGraphic();

        }

        public override void TopDownReCalculateContentSize()
        {
#if DEBUG
            vinv_dbug_EnterLayerReCalculateContent(this);
#endif


            if (this.gridRows == null || gridCols.Count < 1)
            {

                SetPostCalculateLayerContentSize(0, 0);
#if DEBUG
                vinv_dbug_ExitLayerReCalculateContent();
#endif
                return;

            }
            //---------------------------------------------------------- 
            //this.BeginReCalculatingContentSize();
            int sumWidth = 0;
            int maxHeight = 0;
            foreach (GridColumn colDef in gridCols.GetColumnIter())
            {

                ReCalculateColumnSize(colDef);

                if (!colDef.HasCustomSize)
                {
                    sumWidth += colDef.DesiredWidth;
                }
                else
                {
                    sumWidth += colDef.Width;
                }

                if (colDef.DesiredHeight > maxHeight)
                {
                    maxHeight = colDef.DesiredHeight;
                }
            }
            foreach (GridRow rowDef in gridRows.GetRowIter())
            {
                rowDef.CalculateRowHeight();
            }

            if (sumWidth < 1)
            {
                sumWidth = 1;
            }
            if (maxHeight < 1)
            {
                maxHeight = 1;
            }

            SetPostCalculateLayerContentSize(sumWidth, maxHeight);
#if DEBUG
            vinv_dbug_ExitLayerReCalculateContent();
#endif


        }
        static void ReCalculateContentSize(GridCell cell)
        {
            var renderE = cell.ContentElement as RenderElement;
            if (renderE != null && !renderE.HasCalculatedSize)
            {
                renderE.TopDownReCalculateContentSize();
            }
        }

        static void ReCalculateColumnSize(GridColumn col)
        {

            int j = col.CellCount;
            if (j > 0)
            {
                col.DesiredHeight = 0;
                bool firstFoundContentCell = false;
                int local_desired_width = 0;
                for (int i = 0; i < j; i++)
                {
                    GridCell cell = col.GetCell(i);
                    ReCalculateContentSize(cell);

                    int cellDesiredWidth = col.Width;
                    int cellDesiredHeight = cell.Height;

                    var content = cell.ContentElement as RenderElement;
                    if (content != null)
                    {
                        if (content.ElementDesiredWidth > cellDesiredWidth)
                        {
                            cellDesiredWidth = content.ElementDesiredWidth;
                        }
                        if (content.ElementDesiredHeight > cellDesiredHeight)
                        {
                            cellDesiredHeight = content.ElementDesiredHeight;
                        }
                    }

                    col.DesiredHeight += cellDesiredHeight;

                    if (!firstFoundContentCell)
                    {
                        firstFoundContentCell = cell.HasContent;
                    }
                    if (cellDesiredWidth > local_desired_width)
                    {
                        if (firstFoundContentCell)
                        {
                            if (cell.HasContent)
                            {
                                local_desired_width = cellDesiredWidth;
                            }
                        }
                        else
                        {
                            local_desired_width = cellDesiredWidth;
                        }

                    }
                }
                col.CalculatedWidth = local_desired_width;
            }
            else
            {
                col.CalculatedWidth = col.Width;
            }
        }


#if DEBUG
        public override string ToString()
        {

            return "grid layer (L" + dbug_layer_id + this.dbugLayerState + ") postcal:" +
                 this.PostCalculateContentSize.ToString() +
                " of " + this.OwnerRenderElement.dbug_FullElementDescription();
        }

#endif

        public override bool PrepareDrawingChain(VisualDrawingChain chain)
        {

            GridCell leftTopGridItem = GetGridItemByPosition(chain.UpdateAreaX, chain.UpdateAreaY);
            if (leftTopGridItem == null)
            {
                return false;
            }


            GridCell rightBottomGridItem = GetGridItemByPosition(chain.UpdateAreaRight, chain.UpdateAreaBottom);
            if (rightBottomGridItem == null)
            {
                return false;
            }

            GridColumn startColumn = leftTopGridItem.column;
            GridColumn currentColumn = startColumn;
            GridRow startRow = leftTopGridItem.row;
            GridColumn stopColumn = rightBottomGridItem.column.NextColumn;
            GridRow stopRow = rightBottomGridItem.row.NextRow;

            int startRowId = startRow.RowIndex;
            int stopRowId = 0;
            if (stopRow == null)
            {
                stopRowId = gridRows.Count;
            }
            else
            {
                stopRowId = stopRow.RowIndex;
            }

            //----------------------------------------------------------------------------
            //draw border
            //Pen autoBorderPen = new Pen(Color.DarkGray);
            //autoBorderPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            //int n = 0;
            //do
            //{

            //   
            //   
            //    GridItem startGridItemInColumn = currentColumn.GetCell(startRowId);
            //    GridItem stopGridItemInColumn = currentColumn.GetCell(stopRowId - 1);

            //    
            //   
            //    //canvasPage.DrawLine(Color.DarkGray,
            //    //    startGridItemInColumn.RightTopCorner,
            //    //    stopGridItemInColumn.RightBottomCorner);

            //    if (n == 0)
            //    {
            //        
            //        int horizontalLineWidth = rightBottomGridItem.Right - startGridItemInColumn.X;
            //      
            //        
            //        for (int i = startRowId; i < stopRowId; i++)
            //        {
            //           
            //            
            //            GridItem gridItem = currentColumn.GetCell(i);
            //            int x = gridItem.X;
            //            int gBottom = gridItem.Bottom;

            //            
            //            canvasPage.DrawLine(
            //                Color.DarkGray,
            //                x, gBottom,
            //                x + horizontalLineWidth, gBottom);

            //        }
            //        n = 1; 
            //    }
            //   
            //    currentColumn = currentColumn.NextColumn;
            //} while (currentColumn != stopColumn);

            //autoBorderPen.Dispose();

            currentColumn = startColumn;
            //----------------------------------------------------------------------------
            do
            {


                for (int i = startRowId; i < stopRowId; i++)
                {

                    GridCell gridItem = currentColumn.GetCell(i);

                    if (gridItem != null && gridItem.HasContent)
                    {
                        var renderE = gridItem.ContentElement as RenderElement;
                        if (renderE.PrepareDrawingChain(chain))
                        {
                            return true;
                        }
                    }
#if DEBUG
                    //else
                    //{
                    //    canvasPage.DrawText(new char[] { '0' }, gridItem.X, gridItem.Y);
                    //}
#endif


                }

                currentColumn = currentColumn.NextColumn;

            } while (currentColumn != stopColumn);

            return false;

        }
        public override void DrawChildContent(Canvas canvasPage, Rect updateArea)
        {
            GridCell leftTopGridItem = GetGridItemByPosition(updateArea._left, updateArea._top);
            if (leftTopGridItem == null)
            {
                return;

            }
            GridCell rightBottomGridItem = GetGridItemByPosition(updateArea._right, updateArea._bottom);
            if (rightBottomGridItem == null)
            {
                return;
            }


            this.BeginDrawingChildContent();

            GridColumn startColumn = leftTopGridItem.column;
            GridColumn currentColumn = startColumn;
            GridRow startRow = leftTopGridItem.row;
            GridColumn stopColumn = rightBottomGridItem.column.NextColumn;
            GridRow stopRow = rightBottomGridItem.row.NextRow;

            int startRowId = startRow.RowIndex;
            int stopRowId = 0;
            if (stopRow == null)
            {
                stopRowId = gridRows.Count;
            }
            else
            {
                stopRowId = stopRow.RowIndex;
            }


            int n = 0;
            do
            {

                GridCell startGridItemInColumn = currentColumn.GetCell(startRowId);
                GridCell stopGridItemInColumn = currentColumn.GetCell(stopRowId - 1);


                canvasPage.DrawLine(Color.Gray,
                    startGridItemInColumn.RightTopCorner,
                    stopGridItemInColumn.RightBottomCorner);

                if (n == 0)
                {

                    int horizontalLineWidth = rightBottomGridItem.Right - startGridItemInColumn.X;

                    for (int i = startRowId; i < stopRowId; i++)
                    {

                        GridCell gridItem = currentColumn.GetCell(i);
                        int x = gridItem.X;
                        int gBottom = gridItem.Bottom;


                        canvasPage.DrawLine(
                            Color.Gray,
                            x, gBottom,
                            x + horizontalLineWidth, gBottom);

                    }
                    n = 1;
                }
                currentColumn = currentColumn.NextColumn;
            } while (currentColumn != stopColumn);

            currentColumn = startColumn;
            //----------------------------------------------------------------------------
            do
            {


                for (int i = startRowId; i < stopRowId; i++)
                {

                    GridCell gridItem = currentColumn.GetCell(i);
                    if (gridItem != null && gridItem.HasContent)
                    {

                        int x = gridItem.X;
                        int y = gridItem.Y;
                        canvasPage.OffsetCanvasOrigin(x, y);
                        updateArea.Offset(-x, -y);
                        var renderContent = gridItem.ContentElement as RenderElement;
                        if (renderContent != null)
                        {

                            if (canvasPage.PushClipArea(gridItem.Width, gridItem.Height, updateArea))
                            {
                                renderContent.DrawToThisPage(canvasPage, updateArea);
                            }
                            canvasPage.PopClipArea();
                        }


                        canvasPage.OffsetCanvasOrigin(-x, -y);
                        updateArea.Offset(x, y);
                    }
#if DEBUG
                    else
                    {
                        canvasPage.DrawText(new char[] { '.' }, gridItem.X, gridItem.Y);
                    }
#endif
                }

                currentColumn = currentColumn.NextColumn;

            } while (currentColumn != stopColumn);
            this.FinishDrawingChildContent();
        }

#if  DEBUG
        public override void dbug_DumpElementProps(dbugLayoutMsgWriter writer)
        {
            writer.Add(new dbugLayoutMsg(this, this.ToString()));
        }
#endif

    }
}