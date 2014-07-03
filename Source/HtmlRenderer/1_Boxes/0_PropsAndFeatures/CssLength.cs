//BSD 2014, WinterFarm
//ArthurHub

using System;
using System.Globalization;

namespace HtmlRenderer.Dom
{


    /// <summary>
    /// Represents and gets info about a CSS Length
    /// </summary>
    /// <remarks>
    /// http://www.w3.org/TR/CSS21/syndata.html#length-units
    /// </remarks>
    public struct CssLength
    {
        //has 2 instance fields
        //================================     

        //8 least sig bits (1 byte) : store CssUnit,or some predefined CssValue (like border-thickness,font name,background-pos)
        //24 upper bits (3 byte) : store other flags
        readonly int _flags;
        //for number
        readonly float _number;
        //================================   
        //for upper 24 bits of _flags
        public const int IS_ASSIGN = 1 << (11 - 1);
        public const int IS_AUTO = 1 << (12 - 1);
        public const int IS_RELATIVE = 1 << (13 - 1);
        public const int NORMAL = 1 << (14 - 1);
        public const int NONE_VALUE = 1 << (15 - 1);
        public const int HAS_ERROR = 1 << (16 - 1);
        //-------------------------------------
        //when used as border thickeness name
        public const int IS_BORDER_THICKNESS_NAME = 1 << (20 - 1);
        //when used as font size
        public const int IS_FONT_SIZE_NAME = 1 << (21 - 1);
        //------------------------------------- 
        //when used as background position
        public const int IS_BACKGROUND_POS_NAME = 1 << (22 - 1);
        //-------------------------------------   

        public static readonly CssLength AutoLength = new CssLength(IS_ASSIGN | IS_AUTO | (int)CssUnitOrNames.AutoLength);
        public static readonly CssLength NotAssign = new CssLength(0);
        public static readonly CssLength NormalWordOrLine = new CssLength(IS_ASSIGN | NORMAL | (int)CssUnitOrNames.NormalLength);


        public static readonly CssLength ZeroNoUnit = CssLength.MakeZeroLengthNoUnit();
        public static readonly CssLength ZeroPx = CssLength.MakePixelLength(0);

        //-----------------------------------------------------------------------------------------
        public static readonly CssLength Medium = new CssLength(IS_ASSIGN | IS_BORDER_THICKNESS_NAME | (int)CssUnitOrNames.BorderMedium);
        public static readonly CssLength Thick = new CssLength(IS_ASSIGN | IS_BORDER_THICKNESS_NAME | (int)CssUnitOrNames.BorderThick);
        public static readonly CssLength Thin = new CssLength(IS_ASSIGN | IS_BORDER_THICKNESS_NAME | (int)CssUnitOrNames.BorderThin);
        //-----------------------------------------------------------------------------------------
        public static readonly CssLength FontSizeMedium = new CssLength(IS_ASSIGN | IS_FONT_SIZE_NAME | (int)CssUnitOrNames.FONTSIZE_MEDIUM);
        public static readonly CssLength FontSizeXXSmall = new CssLength(IS_ASSIGN | IS_FONT_SIZE_NAME | (int)CssUnitOrNames.FONTSIZE_XX_SMALL);
        public static readonly CssLength FontSizeXSmall = new CssLength(IS_ASSIGN | IS_FONT_SIZE_NAME | (int)CssUnitOrNames.FONTSIZE_X_SMALL);
        public static readonly CssLength FontSizeSmall = new CssLength(IS_ASSIGN | IS_FONT_SIZE_NAME | (int)CssUnitOrNames.FONTSIZE_SMALL);
        public static readonly CssLength FontSizeLarge = new CssLength(IS_ASSIGN | IS_FONT_SIZE_NAME | (int)CssUnitOrNames.FONTSIZE_LARGE);
        public static readonly CssLength FontSizeXLarge = new CssLength(IS_ASSIGN | IS_FONT_SIZE_NAME | (int)CssUnitOrNames.FONTSIZE_X_LARGE);
        public static readonly CssLength FontSizeXXLarge = new CssLength(IS_ASSIGN | IS_FONT_SIZE_NAME | (int)CssUnitOrNames.FONTSIZE_XX_LARGE);
        public static readonly CssLength FontSizeSmaller = new CssLength(IS_ASSIGN | IS_FONT_SIZE_NAME | (int)CssUnitOrNames.FONTSIZE_SMALLER);
        public static readonly CssLength FontSizeLarger = new CssLength(IS_ASSIGN | IS_FONT_SIZE_NAME | (int)CssUnitOrNames.FONTSIZE_LARGE);
        //-----------------------------------------------------------------------------------------
        public static readonly CssLength BackgroundPosLeft = new CssLength(IS_ASSIGN | IS_BACKGROUND_POS_NAME | (int)CssUnitOrNames.LEFT);
        public static readonly CssLength BackgroundPosTop = new CssLength(IS_ASSIGN | IS_BACKGROUND_POS_NAME | (int)CssUnitOrNames.TOP);
        public static readonly CssLength BackgroundPosRight = new CssLength(IS_ASSIGN | IS_BACKGROUND_POS_NAME | (int)CssUnitOrNames.RIGHT);
        public static readonly CssLength BackgroundPosBottom = new CssLength(IS_ASSIGN | IS_BACKGROUND_POS_NAME | (int)CssUnitOrNames.BOTTOM);
        public static readonly CssLength BackgroundPosCenter = new CssLength(IS_ASSIGN | IS_BACKGROUND_POS_NAME | (int)CssUnitOrNames.CENTER);
        //-----------------------------------------------------------------------------------------


        #region Ctor


        public CssLength(float num, CssUnitOrNames unit)
        {
            this._number = num;
            this._flags = (int)unit | IS_ASSIGN;
            switch (unit)
            {
                case CssUnitOrNames.Pixels:
                case CssUnitOrNames.Ems:
                case CssUnitOrNames.Ex:
                case CssUnitOrNames.EmptyValue:
                    this._flags |= IS_RELATIVE;
                    break;
                case CssUnitOrNames.Unknown:
                    this._flags |= HAS_ERROR;
                    return;
                default:

                    break;
            }
        }
        private CssLength(int internalFlags)
        {
            this._number = 0;
            this._flags = internalFlags;
            if (this.HasError)
            {

            }
        }

        public static bool IsEq(CssLength len1, CssLength len2)
        {
            return (len1._number == len2.Number) && (len1._flags == len2._flags);
        }

        #endregion


        #region Props


        public static CssUnitOrNames GetCssUnit(string u)
        {
            switch (u)
            {
                case CssConstants.Em:
                    return CssUnitOrNames.Ems;
                case CssConstants.Ex:
                    return CssUnitOrNames.Ex;
                case CssConstants.Px:
                    return CssUnitOrNames.Pixels;
                case CssConstants.Mm:
                    return CssUnitOrNames.Milimeters;
                case CssConstants.Cm:
                    return CssUnitOrNames.Centimeters;
                case CssConstants.In:
                    return CssUnitOrNames.Inches;
                case CssConstants.Pt:
                    return CssUnitOrNames.Points;
                case CssConstants.Pc:
                    return CssUnitOrNames.Picas;
                default:
                    return CssUnitOrNames.Unknown;
            }
        }

        public static CssLength MakePixelLength(float pixel)
        {
            return new CssLength(pixel, CssUnitOrNames.Pixels);
        }
        public static CssLength MakeZeroLengthNoUnit()
        {
            return new CssLength(0, CssUnitOrNames.EmptyValue);
        }
        public static CssLength MakeFontSizePtUnit(float pointUnit)
        {
            return new CssLength(pointUnit, CssUnitOrNames.Points);
        }
        /// <summary>
        /// Gets the number in the length
        /// </summary>
        public float Number
        {
            get { return _number; }
        }

        /// <summary>
        /// Gets if the length has some parsing error
        /// </summary>
        public bool HasError
        {
            // get { return _hasError; }
            get { return (this._flags & HAS_ERROR) != 0; }
        }


        /// <summary>
        /// Gets if the length represents a precentage (not actually a length)
        /// </summary>
        public bool IsPercentage
        {

            get { return this.UnitOrNames == CssUnitOrNames.Percent; }
        }
        public bool IsAuto
        {
            get { return (this._flags & IS_AUTO) != 0; }
        }
        public bool IsEmpty
        {
            get { return this.UnitOrNames == CssUnitOrNames.EmptyValue; }
            //get { return (this._flags & IS_ASSIGN) == 0; }
        }
        public bool IsEmptyOrAuto
        {
            //range usage *** 
            get { return this.UnitOrNames <= CssUnitOrNames.AutoLength; }
        }
        public bool IsNormalWordSpacing
        {
            get
            {
                return (this._flags & NORMAL) != 0;
            }
        }
        public bool IsNormalLineHeight
        {
            get
            {
                return (this._flags & NORMAL) != 0;
            }
        }
        /// <summary>
        /// Gets if the length is specified in relative units
        /// </summary>
        public bool IsRelative
        {
            get { return (this._flags & IS_RELATIVE) != 0; }

        }

        /// <summary>
        /// Gets the unit of the length
        /// </summary>
        public CssUnitOrNames UnitOrNames
        {
            get { return (CssUnitOrNames)(this._flags & 0xFF); }
        }

        //-------------------------------------------------
        public bool IsFontSizeName
        {
            get { return (this._flags & IS_FONT_SIZE_NAME) != 0; }
        }

        //-------------------------------------------------
        public bool IsBackgroundPositionName
        {
            get { return (this._flags & IS_BACKGROUND_POS_NAME) != 0; }
        }

        //-------------------------------------------------
        public bool IsBorderThicknessName
        {
            get { return (this._flags & IS_BORDER_THICKNESS_NAME) != 0; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// If length is in Ems, returns its value in points
        /// </summary>
        /// <param name="emSize">Em size factor to multiply</param>
        /// <returns>Points size of this em</returns>
        /// <exception cref="InvalidOperationException">If length has an error or isn't in ems</exception>
        public CssLength ConvertEmToPoints(float emSize)
        {
            if (HasError) throw new InvalidOperationException("Invalid length");
            if (UnitOrNames != CssUnitOrNames.Ems) throw new InvalidOperationException("Length is not in ems");

            return new CssLength(Number * emSize, CssUnitOrNames.Points);
            //return new CssLength(string.Format("{0}pt", Convert.ToSingle(Number * emSize).ToString("0.0", NumberFormatInfo.InvariantInfo)));
        }

        /// <summary>
        /// If length is in Ems, returns its value in pixels
        /// </summary>
        /// <param name="pixelFactor">Pixel size factor to multiply</param>
        /// <returns>Pixels size of this em</returns>
        /// <exception cref="InvalidOperationException">If length has an error or isn't in ems</exception>
        public CssLength ConvertEmToPixels(float pixelFactor)
        {
            if (HasError) throw new InvalidOperationException("Invalid length");
            if (UnitOrNames != CssUnitOrNames.Ems) throw new InvalidOperationException("Length is not in ems");

            return new CssLength(Number * pixelFactor, CssUnitOrNames.Pixels);
            //return new CssLength(string.Format("{0}px", Convert.ToSingle(Number * pixelFactor).ToString("0.0", NumberFormatInfo.InvariantInfo)));
        }

        /// <summary>
        /// Returns the length formatted ready for CSS interpreting.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (HasError)
            {
                return string.Empty;
            }
            else
            {
                string u = string.Empty;

                switch (UnitOrNames)
                {
                    case CssUnitOrNames.Percent:
                        return string.Format(NumberFormatInfo.InvariantInfo, "{0}%", Number);

                    case CssUnitOrNames.EmptyValue:
                        break;
                    case CssUnitOrNames.Ems:
                        u = "em";
                        break;
                    case CssUnitOrNames.Pixels:
                        u = "px";
                        break;
                    case CssUnitOrNames.Ex:
                        u = "ex";
                        break;
                    case CssUnitOrNames.Inches:
                        u = "in";
                        break;
                    case CssUnitOrNames.Centimeters:
                        u = "cm";
                        break;
                    case CssUnitOrNames.Milimeters:
                        u = "mm";
                        break;
                    case CssUnitOrNames.Points:
                        u = "pt";
                        break;
                    case CssUnitOrNames.Picas:
                        u = "pc";
                        break;

                }

                return string.Format(NumberFormatInfo.InvariantInfo, "{0}{1}", Number, u);
            }
        }

        #endregion
    }
}