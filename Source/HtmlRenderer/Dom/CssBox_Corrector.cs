﻿//BSD 2014, WinterCore

// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using HtmlRenderer.Entities;
using HtmlRenderer.Handlers;
using HtmlRenderer.Parse;
using HtmlRenderer.Utils;

namespace HtmlRenderer.Dom
{
    partial class CssBox
    {
        /// <summary>
        /// Gets the previous sibling of this box.
        /// </summary>
        /// <returns>Box before this one on the tree. Null if its the first</returns>
        public static CssBox GetPreviousContainingBlockSibling(CssBox box)
        {
            CssBox curBox = box;
            CssBox parentBox = curBox.ParentBox;
            while (curBox.CssDisplay < CssDisplay.__CONTAINER_BEGIN_HERE && parentBox != null)
            {
                //climbing up, find parent box the has container property 
                curBox = parentBox;
                parentBox = curBox.ParentBox;
            }

            //-----------------------------------------------------------
            box = parentBox; 
            CssBox sib = curBox.PrevSibling;
            if (sib != null)
            {
                do
                {
                    if (sib.CssDisplay != CssDisplay.None && !sib.IsAbsolutePosition)
                    {
                        return sib;
                    }
                    sib = sib.PrevSibling;

                } while (sib != null); 
            }
            return null; 
        }
        /// <summary>
        /// Gets the previous sibling of this box.
        /// </summary>
        /// <returns>Box before this one on the tree. Null if its the first</returns>
        public static CssBox GetPreviousSibling(CssBox b)
        {
            if (b.ParentBox != null)
            {

                CssBox sib = b.PrevSibling;
                if (sib != null)
                {
                    do
                    {
                        if (sib.CssDisplay != CssDisplay.None && !sib.IsAbsolutePosition)
                        {
                            return sib;
                        }
                        sib = sib.PrevSibling;
                    } while (sib != null); 
                }
                return null; 
            }
            return null;
        }
        /// <summary>
        /// Correct the DOM tree recursively by replacing  "br" html boxes with anonymous blocks that respect br spec.<br/>
        /// If the "br" tag is after inline box then the anon block will have zero height only acting as newline,
        /// but if it is after block box then it will have min-height of the font size so it will create empty line.
        /// </summary>
        /// <param name="box">the current box to correct its sub-tree</param>
        /// <param name="followingBlock">used to know if the br is following a box so it should create an empty line or not so it only
        /// move to a new line</param>
        public static void CorrectLineBreaksBlocks(CssBox box, ref bool followingBlock)
        {
            //recursive

            followingBlock = followingBlock || box.IsBlock;
            var allChildren = box.Boxes;

            foreach (var childBox in allChildren.GetChildBoxIter())
            {
                //recursive to child first
                CorrectLineBreaksBlocks(childBox, ref followingBlock);
                followingBlock = childBox.Words.Count == 0 && (followingBlock || childBox.IsBlock);
            }

            //-------------------------------------
            int latestCheckIndex = -1;
            CssBox brBox = null;
            do
            {
                brBox = null;//reset each loop
                int j = allChildren.Count;
                int latestFoundBrAt = -1;
                for (int i = latestCheckIndex + 1; i < j; i++)
                {
                    var curBox = allChildren[i];
                    if (curBox.IsBrElement)
                    {
                        brBox = curBox;
                        latestCheckIndex = latestFoundBrAt = i;

                        //check prev box
                        if (i > 0)
                        {
                            var prevBox = allChildren[i - 1];
                            if (prevBox.Words.Count > 0)
                            {
                                followingBlock = false;
                            }
                            else if (prevBox.IsBlock)
                            {
                                followingBlock = true;
                            }
                        }
                        break;
                    }
                }

                if (brBox != null)
                {


                    brBox.CssDisplay = CssDisplay.Block;
                    if (followingBlock)
                    {   // atodo: check the height to min-height when it is supported
                        brBox.Height = new CssLength(0.95f, false, CssUnit.Ems);
                    }
                    ////create new box then add to 'box'  before brbox  
                    //var anonBlock = CssBox.CreateBlock(box, new HtmlTag("br"), latestFoundBrAt);
                    //if (followingBlock)
                    //{
                    //    //anonBlock.Height = ".95em"; // atodo: check the height to min-height when it is supported
                    //    anonBlock.Height = new CssLength(0.95f, false, CssUnit.Ems);
                    //}
                    ////remove this br box from parent *** 
                    //brBox.SetNewParentBox(null); 
                }

            } while (brBox != null);
        }

        /// <summary>
        /// Check if the given box contains inline and block child boxes.
        /// </summary>
        /// <param name="box">the box to check</param>
        /// <returns>true - has variant child boxes, false - otherwise</returns>
        internal static bool ContainsVariantBoxes(CssBox box, out int mixFlags)
        {
            //bool hasBlock = false;
            //bool hasInline = false;
            //for (int i = 0; i < box.Boxes.Count && (!hasBlock || !hasInline); i++)
            //{
            //    var isBlock = !box.Boxes[i].IsInline;
            //    hasBlock = hasBlock || isBlock;
            //    hasInline = hasInline || !isBlock;
            //}
            mixFlags = 0;

            var children = box.Boxes;
            for (int i = children.Count - 1; i >= 0; --i)
            {
                if ((mixFlags |= children[i].IsInline ? HAS_IN_LINE : HAS_BLOCK)
                    == (HAS_BLOCK | HAS_IN_LINE))
                {
                    return true;
                }
            }

            return false;
            //return checkFlags == (HAS_BLOCK | HAS_IN_LINE);
        }
        const int HAS_BLOCK = 1 << (1 - 1);
        const int HAS_IN_LINE = 1 << (2 - 1);
        /// <summary>
        /// Rearrange the DOM of the box to have block box with boxes before the inner block box and after.
        /// </summary>
        /// <param name="box">the box that has the problem</param>
        internal static void CorrectBlockInsideInlineImp(CssBox box)
        {
            if (box.ChildCount > 1 || box.GetFirstChild().ChildCount > 1)
            {

                var newLeftBlock = CssBox.CreateBlock(box);
                //1. newLeftBlock is Created and add is latest child of the 'box'
                //-------------------------------------------
                var firstChild = box.GetFirstChild();
                while (ContainsInlinesOnlyDeep(firstChild))
                {
                    //if first box has only inline(deep) then
                    //move first child to newLeftBlock ***                     
                    firstChild.SetNewParentBox(newLeftBlock);
                    //next
                    firstChild = box.GetFirstChild();
                }
                //------------------------------------------- 
                //insert left block as leftmost (firstbox) in the line 
                //newLeftBlock.SetBeforeBox(box.GetFirstChild());
                newLeftBlock.ChangeSiblingOrder(0);
                var splitBox = box.Boxes[1];

                splitBox.SetNewParentBox(null);
                CorrectBlockSplitBadBox(box, splitBox, newLeftBlock);

                if (box.ChildCount > 2)
                {
                    // var rightBox = CssBox.CreateBox(box, null, box.Boxes[2]);
                    var rightBox = CssBox.CreateBox(box, null, 2);
                    while (box.ChildCount > 3)
                    {
                        box.Boxes[3].SetNewParentBox(rightBox);
                    }
                }
            }
            else if (box.Boxes[0].CssDisplay == CssDisplay.Inline)
            {
                box.Boxes[0].CssDisplay = CssDisplay.Block;
            }

            if (box.CssDisplay == CssDisplay.Inline)
            {
                box.CssDisplay = CssDisplay.Block;
            }
        }

        /// <summary>
        /// Split bad box that has inline and block boxes into two parts, the left - before the block box
        /// and right - after the block box.
        /// </summary>
        /// <param name="parentBox">the parent box that has the problem</param>
        /// <param name="badBox">the box to split into different boxes</param>
        /// <param name="leftBlock">the left block box that is created for the split</param>
        private static void CorrectBlockSplitBadBox(CssBox parentBox, CssBox badBox, CssBox leftBlock)
        {//recursive

            var leftBox = CssBox.CreateBox(leftBlock, badBox.HtmlTag);
            leftBox.InheritStyle(badBox, true);

            bool had_new_leftbox = false;
            while (badBox.GetFirstChild().IsInline && ContainsInlinesOnlyDeep(badBox.GetFirstChild()))
            {
                had_new_leftbox = true;
                //move element that has only inline(deep) to leftbox
                badBox.GetFirstChild().SetNewParentBox(leftBox);
            }

            var splitBox = badBox.GetFirstChild();

            if (!ContainsInlinesOnlyDeep(splitBox))
            {
                //recursive
                CorrectBlockSplitBadBox(parentBox, splitBox, leftBlock);
                splitBox.SetNewParentBox(null);
            }
            else
            {
                splitBox.SetNewParentBox(parentBox);
            }

            if (badBox.ChildCount > 0)
            {
                CssBox rightBox;
                if (splitBox.ParentBox != null || parentBox.ChildCount < 3)
                {
                    rightBox = CssBox.CreateBox(parentBox, badBox.HtmlTag);
                    rightBox.InheritStyle(badBox, true);

                    if (parentBox.ChildCount > 2)
                    {
                        //rightBox.SetBeforeBox(parentBox.Boxes[1]);
                        rightBox.ChangeSiblingOrder(1);
                    }
                    if (splitBox.ParentBox != null)
                    {
                        splitBox.ChangeSiblingOrder(1);
                        //splitBox.SetBeforeBox(rightBox);
                    }
                }
                else
                {
                    rightBox = parentBox.Boxes[2];
                }

                while (badBox.ChildCount > 0)
                {   //move all children to right box 
                    badBox.Boxes[0].SetNewParentBox(rightBox);
                }
            }
            else if (splitBox.ParentBox != null && parentBox.ChildCount > 1)
            {
                //splitBox.SetBeforeBox(parentBox.Boxes[1]);
                splitBox.ChangeSiblingOrder(1);
                //if (splitBox.HtmlTag != null && splitBox.HtmlTag.Name == "br" && (hadLeft || leftBlock.Boxes.Count > 1))
                if (splitBox.WellknownTagName == WellknownHtmlTagName.BR
                    && (had_new_leftbox || leftBlock.ChildCount > 1))
                {
                    splitBox.CssDisplay = CssDisplay.Inline;
                    //splitBox.Display = CssConstants.Inline;
                }
            }
        }

        /// <summary>
        /// Makes block boxes be among only block boxes and all inline boxes have block parent box.<br/>
        /// Inline boxes should live in a pool of Inline boxes only so they will define a single block.<br/>
        /// At the end of this process a block box will have only block siblings and inline box will have
        /// only inline siblings.
        /// </summary>
        /// <param name="box">the current box to correct its sub-tree</param>
        internal static void CorrectInlineBoxesParent(CssBox box)
        {
            //------------------------------------------------
            //recursive 
            int mixFlags;
            var allChildren = box.Boxes;
            if (ContainsVariantBoxes(box, out mixFlags))
            {
                //if box contains both inline and block 
                //then correct it                
                for (int i = 0; i < allChildren.Count; i++)
                {
                    var curBox = allChildren[i];
                    if (curBox.IsInline)
                    {
                        //1. creat new box anonymous block (no html tag) then
                        //  add it before this box 
                        //var newbox = CssBox.CreateBlock(box, null, curBox);
                        var newbox = CssBox.CreateBlock(box, null, i);
                        //2. skip newly add box 
                        i++;
                        //3. move next child that is inline element to new box                     
                        CssBox tomoveBox = null;
                        while (i < allChildren.Count && ((tomoveBox = allChildren[i]).IsInline))
                        {
                            tomoveBox.SetNewParentBox(newbox);
                            ///tomoveBox.ParentBox = newbox;
                        }
                        //so new box contains inline that move from current line
                    }
                }
                //after correction , now all children in this box are block element 
            }
            //------------------------------------------------
            if (mixFlags != HAS_IN_LINE)
            {
                foreach (var childBox in allChildren)
                {
                    //recursive
                    CorrectInlineBoxesParent(childBox);
                }
            }
            //if (!DomUtils.ContainsInlinesOnly(box))
            //{ 
            //    foreach (var childBox in box.Boxes)
            //    {
            //        CorrectInlineBoxesParent(childBox);
            //    }
            //}
        }
        ///// <summary>
        ///// Gets the next sibling of this box.
        ///// </summary>
        ///// <returns>Box before this one on the tree. Null if its the first</returns>
        //public static CssBox GetNextSibling(CssBox b)
        //{
        //    CssBox sib = null;
        //    if (b.ParentBox != null)
        //    {
        //        var index = b.ParentBox.Boxes.IndexOf(b) + 1;
        //        while (index <= b.ParentBox.ChildCount - 1)
        //        {
        //            var pSib = b.ParentBox.Boxes[index]; 
        //            if (pSib.CssDisplay != CssDisplay.None && !pSib.IsAbsolutePosition)
        //            {
        //                sib = pSib;
        //                break;
        //            }
        //            index++;
        //        }
        //    }
        //    return sib;
        //}
        /// <summary>
        /// Check if the given box contains only inline child boxes in all subtree.
        /// </summary>
        /// <param name="box">the box to check</param>
        /// <returns>true - only inline child boxes, false - otherwise</returns>
        internal static bool ContainsInlinesOnlyDeep(CssBox box)
        {
            //if (box.PassTestInlineOnlyDeep && box.InlineOnlyDeepResult == true)
            //{
            //    return true;
            //}

            //recursive
            foreach (var childBox in box.GetChildBoxIter())
            {
                //if box is inline then check its sub tree
                if (!childBox.IsInline || !ContainsInlinesOnlyDeep(childBox))
                {
                    // box.SetInlineOnlyDeepResult(false);
                    return false;
                }
            }

            //box.SetInlineOnlyDeepResult(true);
            return true;
        }


        /// <summary>
        /// Go over all the text boxes (boxes that have some text that will be rendered) and
        /// remove all boxes that have only white-spaces but are not 'preformatted' so they do not effect
        /// the rendered html.
        /// </summary>
        /// <param name="box">the current box to correct its sub-tree</param>
        internal static void CorrectTextBoxes(CssBox box)
        {

            for (int i = box.ChildCount - 1; i >= 0; i--)
            {
                var childBox = box.Boxes[i];
                if (childBox.HasTextContent)
                {
                    // is the box has text
                    var keepBox = !childBox.TextContentIsWhitespaceOrEmptyText;

                    // is the box is pre-formatted
                    //keepBox = keepBox || childBox.WhiteSpace == CssConstants.Pre || childBox.WhiteSpace == CssConstants.PreWrap;
                    keepBox = keepBox || childBox.WhiteSpace == CssWhiteSpace.Pre || childBox.WhiteSpace == CssWhiteSpace.PreWrap;

                    // is the box is only one in the parent
                    keepBox = keepBox || box.ChildCount == 1;

                    // is it a whitespace between two inline boxes
                    keepBox = keepBox || (i > 0 && i < box.ChildCount - 1 && box.Boxes[i - 1].IsInline && box.Boxes[i + 1].IsInline);

                    // is first/last box where is in inline box and it's next/previous box is inline
                    keepBox = keepBox || (i == 0 && box.ChildCount > 1 && box.Boxes[1].IsInline && box.IsInline) ||
                        (i == box.ChildCount - 1 && box.ChildCount > 1 && box.Boxes[i - 1].IsInline && box.IsInline);

                    if (keepBox)
                    {
                        // valid text box, parse it to words
                        childBox.ParseToWords();
                    }
                    else
                    {
                        // remove text box that has no 
                        childBox.ParentBox.Boxes.RemoveAt(i);
                    }
                }
                else
                {
                    // recursive
                    CorrectTextBoxes(childBox);
                }
            }
        }

    }
}