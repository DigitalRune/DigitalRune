// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Defines a color palette where data value are mapped to colors.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="Palette"/> associates data values with colors. The color for a value can be
    /// queried using <see cref="GetColor"/>. The <see cref="PaletteMode"/> defines how the data
    /// values are mapped to the registered colors.
    /// </para>
    /// <para>
    /// A palette is basically a list of <see cref="PaletteEntry"/> objects. A palette can contain
    /// multiple entries for the same data value. In this case only the first
    /// <see cref="PaletteEntry"/> with a matching data value is used.
    /// </para>
    /// <para>
    /// The <see cref="PaletteEntry"/> can be inserted in any order. They need not be sorted by the
    /// data value.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class Palette : Collection<PaletteEntry>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

#if SILVERLIGHT || WINDOWS_PHONE
        private List<PaletteEntry> _sortedPaletteEntries;
#else
        private SortedList<double, PaletteEntry> _sortedPaletteEntries;
#endif
        private Palette _convertedPalette;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the palette mode.
        /// </summary>
        /// <value>
        /// A enumeration value of <see cref="PaletteMode"/>. The default value is
        /// <see cref="PaletteMode.Interpolate"/>
        /// </value>
        public PaletteMode Mode { get; set; }


        /// <summary>
        /// Gets or sets a <see cref="System.Windows.Media.ColorInterpolationMode"/> enumeration
        /// that specifies how the gradient's colors are interpolated.
        /// </summary>
        /// <value>
        /// Specifies how the colors in a gradient are interpolated. The default is
        /// <see cref="System.Windows.Media.ColorInterpolationMode.SRgbLinearInterpolation"/>.
        /// </value>
        public ColorInterpolationMode ColorInterpolationMode { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="Palette"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="Palette"/> class.
        /// </summary>
        public Palette()
            : this(PaletteMode.Interpolate)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Palette"/> class.
        /// </summary>
        /// <param name="mode">The palette mode.</param>
        public Palette(PaletteMode mode)
        {
            Mode = mode;
            ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Palette"/> class.
        /// </summary>
        /// <param name="paletteEntries">The palette that is wrapped by this palette.</param>
        public Palette(IList<PaletteEntry> paletteEntries)
            : this(paletteEntries, PaletteMode.Interpolate)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Palette"/> class.
        /// </summary>
        /// <param name="paletteEntries">The palette that is wrapped by this palette.</param>
        /// <param name="mode">The palette mode.</param>
        public Palette(IList<PaletteEntry> paletteEntries, PaletteMode mode)
            : base(paletteEntries)
        {
            Mode = mode;
            ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Removes all elements from the <see cref="Collection{T}"/>.
        /// </summary>
        protected override void ClearItems()
        {
            base.ClearItems();
            _sortedPaletteEntries = null;
            _convertedPalette = null;
        }


        /// <summary>
        /// Inserts an element into the <see cref="Collection{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which <paramref name="item"/> should be inserted.
        /// </param>
        /// <param name="item">
        /// The object to insert. The value can be null for reference types.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. Or <paramref name="index"/> is greater than
        /// <see cref="Collection{T}.Count"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        protected override void InsertItem(int index, PaletteEntry item)
        {
            if (item == null)
                throw new ArgumentNullException("item", "Palette entries in a palette must not be null.");

            base.InsertItem(index, item);
            _sortedPaletteEntries = null;
            _convertedPalette = null;
        }


        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">
        /// The new value for the element at the specified index. The value can be null for
        /// reference types.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. Or <paramref name="index"/> is greater than
        /// <see cref="Collection{T}.Count"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        protected override void SetItem(int index, PaletteEntry item)
        {
            if (item == null)
                throw new ArgumentNullException("item", "Palette entries in a palette must not be null.");

            base.SetItem(index, item);
            _sortedPaletteEntries = null;
            _convertedPalette = null;
        }


        /// <summary>
        /// Removes the element at the specified index of the <see cref="Collection{T}"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. Or <paramref name="index"/> is equal to or
        /// greater than <see cref="Collection{T}.Count"/>.
        /// </exception>
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            _sortedPaletteEntries = null;
            _convertedPalette = null;
        }


        /// <summary>
        /// Adds the specified color entry.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="color">The color.</param>
        public void Add(double value, Color color)
        {
            Add(new PaletteEntry(value, color));
        }


        /// <summary>
        /// Determines whether palette contains an entry for the specified data value.
        /// </summary>
        /// <param name="value">The data value.</param>
        /// <returns>
        /// <see langword="true"/> if palette contains a color that is associated with
        /// <paramref name="value"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(double value)
        {
            if (_sortedPaletteEntries != null)
            {
                // Sorted list is available.
#if SILVERLIGHT || WINDOWS_PHONE
            foreach (PaletteEntry paletteEntry in _sortedPaletteEntries)
            {
                int result = value.CompareTo(paletteEntry.Value);
                if (result == 0)
                    return true;
                else if (result > 0)
                    return false;
            }
#else
                int index = _sortedPaletteEntries.IndexOfKey(value);
                return index != -1;
#endif
            }

            // Sorted list is not available.
            // Make brute force search.
            foreach (PaletteEntry paletteEntry in this)
            {
                if (paletteEntry.Value == value)
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Removes the entry for the specified data value.
        /// </summary>
        /// <param name="value">The data value.</param>
        /// <returns>
        /// <see langword="true"/> if color is successfully removed; otherwise,
        /// <see langword="false"/>. This method also returns <see langword="false"/> if entry was
        /// not found in the palette.
        /// </returns>
        /// <remarks>
        /// A <see cref="Palette"/> can contain multiple color entries with the same data value. In
        /// this case all entries that match <paramref name="value"/> are removed from the
        /// <see cref="Palette"/>.
        /// </remarks>
        public bool Remove(double value)
        {
            bool entryRemoved = false;

            // Iterate through palette in reverse order. (This is necessary because we remove items
            // from the collection while iterating through it. Going through the collection in
            // reverse order, we do not have to adjust the indices, when an entry is removed.)
            for (int i = Count - 1; i >= 0; i--)
            {
                if (base[i].Value == value)
                {
                    RemoveAt(i);
                    entryRemoved = true;
                }
            }

            return entryRemoved;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void BuildInternalSortedList()
        {
            // Create an internal sorted list with all relevant entries.
            if (_sortedPaletteEntries == null)
            {
#if SILVERLIGHT || WINDOWS_PHONE
                _sortedPaletteEntries = new List<PaletteEntry>(this);
                _sortedPaletteEntries.Sort((x, y) => x.Value.CompareTo(y.Value));
#else
                _sortedPaletteEntries = new SortedList<double, PaletteEntry>(Count);
                foreach (PaletteEntry paletteEntry in this)
                {
                    // ReSharper disable EmptyGeneralCatchClause
                    try
                    {
                        _sortedPaletteEntries.Add(paletteEntry.Value, paletteEntry);
                    }
                    catch
                    {
                        // Entry already in SortedList. Ignore it.
                    }

                    // ReSharper restore EmptyGeneralCatchClause
                }
#endif
            }
        }


        /// <summary>
        /// Converts the current palette with mode <see cref="PaletteMode.Closest"/> to a palette with
        /// mode <see cref="PaletteMode.LessOrEqual"/>.
        /// </summary>
        private void ConvertPalette()
        {
            Debug.Assert(Mode == PaletteMode.Closest, "PaletteMode needs to be set to Closest.");

            if (_convertedPalette == null)
            {
                _convertedPalette = new Palette { Mode = PaletteMode.LessOrEqual };

                int numberOfEntries = _sortedPaletteEntries.Count;
                if (numberOfEntries > 0)
                {
                    // Insert first palette entry at -∞.
#if SILVERLIGHT || WINDOWS_PHONE
                    PaletteEntry entry = _sortedPaletteEntries[0];
#else
                    PaletteEntry entry = _sortedPaletteEntries.Values[0];
#endif
                    _convertedPalette.Add(Double.NegativeInfinity, entry.Color);
                    PaletteEntry previousEntry = entry;

                    // Insert remaining palette entries.
                    for (int i = 1; i < numberOfEntries; i++)
                    {
                        entry = _sortedPaletteEntries[i];
                        _convertedPalette.Add((previousEntry.Value + entry.Value) / 2, entry.Color);
                        previousEntry = entry;
                    }
                }
            }
        }


        private PaletteEntry GetPaletteEntry(double value)
        {
            if (Count == 0)
                throw new PaletteException("The palette is empty.");

            // Try direct index access. This is faster for palettes where the index of a
            // PaletteEntry is equal to the value of the entry.
            if (value >= 0 && (int)value < Count)
            {
                PaletteEntry entry = this[(int)value];
                if (Numeric.AreEqual(entry.Value, value))
                    return entry;
            }

            BuildInternalSortedList();

            PaletteEntry? lessEntry = null;      // Entry whose data value is less than value.
            PaletteEntry? greaterEntry = null;   // Entry whose data value is greater than value.
            PaletteEntry? equalEntry = null;     // Entry whose data value exactly matches value.

            for (int i = 0; i < _sortedPaletteEntries.Count; i++)
            {
#if SILVERLIGHT || WINDOWS_PHONE
                PaletteEntry entry = _sortedPaletteEntries[i];
#else
                PaletteEntry entry = _sortedPaletteEntries.Values[i];
#endif
                int comparisonResult = Numeric.Compare(entry.Value, value);
                if (comparisonResult < 0)
                {
                    lessEntry = entry;
                }
                else if (comparisonResult == 0)
                {
                    equalEntry = entry;
                }
                else
                {
                    Debug.Assert(comparisonResult > 0, "Sanity check.");
                    greaterEntry = entry;
                    break;
                }
            }

            if (equalEntry != null)
            {
                // Exact match!
                return equalEntry.Value;
            }

            if (lessEntry != null && greaterEntry != null)
            {
                // value lies between two colors.
                switch (Mode)
                {
                    case PaletteMode.LessOrEqual:
                        return lessEntry.Value;
                    case PaletteMode.GreaterOrEqual:
                        return greaterEntry.Value;
                    case PaletteMode.Closest:
                        return (value - lessEntry.Value.Value < greaterEntry.Value.Value - value) ? lessEntry.Value : greaterEntry.Value;
                    case PaletteMode.Interpolate:
                        Color interpolatedColor = GetInterpolatedColor(value, lessEntry.Value, greaterEntry.Value);
                        PaletteEntry interpolatedEntry = new PaletteEntry(value, interpolatedColor);
                        return interpolatedEntry;
                }
            }

            if (lessEntry != null && greaterEntry == null)
            {
                // value is greater than the colors in the palette.
                switch (Mode)
                {
                    case PaletteMode.LessOrEqual:
                    case PaletteMode.Closest:
                    case PaletteMode.Interpolate:
                        return lessEntry.Value;
                    case PaletteMode.GreaterOrEqual:
                        throw new PaletteException("No suitable color found. Specified value is greater than all color entries in the palette.");
                }
            }

            if (lessEntry == null && greaterEntry != null)
            {
                // value is less than the colors in the palette.
                switch (Mode)
                {
                    case PaletteMode.GreaterOrEqual:
                    case PaletteMode.Closest:
                    case PaletteMode.Interpolate:
                        return greaterEntry.Value;
                    case PaletteMode.LessOrEqual:
                        throw new PaletteException("No suitable color found. Specified value is less than all color entries in the palette.");
                }
            }

            throw new PaletteException("No suitable color found.");
        }


        private Color GetInterpolatedColor(double value, PaletteEntry lessEntry, PaletteEntry greaterEntry)
        {
            Color interpolatedColor;
            float f = (float)((value - lessEntry.Value) / (greaterEntry.Value - lessEntry.Value));

#if SILVERLIGHT || WINDOWS_PHONE
              // Note: Silverlight does not support reading the ScRGB values of a Color.
              byte a = (byte)((greaterEntry.Color.A - lessEntry.Color.A) * f + lessEntry.Color.A);
              byte r = (byte)((greaterEntry.Color.R - lessEntry.Color.R) * f + lessEntry.Color.R);
              byte g = (byte)((greaterEntry.Color.G - lessEntry.Color.G) * f + lessEntry.Color.G);
              byte b = (byte)((greaterEntry.Color.B - lessEntry.Color.B) * f + lessEntry.Color.B);
              interpolatedColor = Color.FromArgb(a, r, g, b);
#else
            if (ColorInterpolationMode == ColorInterpolationMode.ScRgbLinearInterpolation)
            {
                float a = (greaterEntry.Color.ScA - lessEntry.Color.ScA) * f + lessEntry.Color.ScA;
                float r = (greaterEntry.Color.ScR - lessEntry.Color.ScR) * f + lessEntry.Color.ScR;
                float g = (greaterEntry.Color.ScG - lessEntry.Color.ScG) * f + lessEntry.Color.ScG;
                float b = (greaterEntry.Color.ScB - lessEntry.Color.ScB) * f + lessEntry.Color.ScB;
                interpolatedColor = Color.FromScRgb(a, r, g, b);
            }
            else
            {
                byte a = (byte)((greaterEntry.Color.A - lessEntry.Color.A) * f + lessEntry.Color.A);
                byte r = (byte)((greaterEntry.Color.R - lessEntry.Color.R) * f + lessEntry.Color.R);
                byte g = (byte)((greaterEntry.Color.G - lessEntry.Color.G) * f + lessEntry.Color.G);
                byte b = (byte)((greaterEntry.Color.B - lessEntry.Color.B) * f + lessEntry.Color.B);
                interpolatedColor = Color.FromArgb(a, r, g, b);
            }
#endif

            return interpolatedColor;
        }


        /// <summary>
        /// Gets the color for the given data value.
        /// </summary>
        /// <param name="value">The data value.</param>
        /// <returns>The color.</returns>
        /// <exception cref="PaletteException">
        /// The palette is empty or there is no suitable color.
        /// </exception>
        public Color GetColor(double value)
        {
            return GetPaletteEntry(value).Color;
        }


        /// <summary>
        /// Gets the color for the given data value.
        /// </summary>
        /// <param name="value">The data value.</param>
        /// <param name="color">The resulting color.</param>
        /// <returns>
        /// <see langword="true"/> if a suitable color is found in the <see cref="Palette"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="PaletteException">
        /// The palette is empty or there is no suitable color.
        /// </exception>
        public bool TryGetColor(double value, out Color color)
        {
            try
            {
                color = GetColor(value);
                return true;
            }
            catch (PaletteException)
            {
                color = new Color();
                return false;
            }
        }


        /// <summary>
        /// Gets the color gradient for the given start and end value.
        /// </summary>
        /// <param name="startValue">The start data value.</param>
        /// <param name="endValue">The end data value.</param>
        /// <returns>A color gradient defined by a <see cref="GradientStopCollection"/>.</returns>
        /// <remarks>
        /// <para>
        /// <see cref="Mode"/> specifies how data values are mapped to the entries in the palette.
        /// (See <see cref="PaletteMode"/> for a list of all available mappings.)
        /// </para>
        /// <para>
        /// The mode defines the gradient that is created. For example: 
        /// <list type="bullet">
        /// <item>
        /// <see cref="PaletteMode.Interpolate"/> creates a smooth continuous gradient as defined by
        /// the entries in the palette.
        /// </item>
        /// <item>
        /// <see cref="PaletteMode.Closest"/> creates gradient with discrete blocks of color. The
        /// area closest to data value <i>n</i> is filled with color <i>n</i> ("centered steps").
        /// </item>
        /// <item>
        /// <see cref="PaletteMode.GreaterOrEqual"/> creates gradient with discrete blocks of color.
        /// The area from data value <i>(n-1)</i> to data value <i>(n)</i> is filled with color
        /// <i>n</i> ("left steps").
        /// </item>
        /// <item>
        /// <see cref="PaletteMode.LessOrEqual"/> creates gradient with discrete blocks of color.
        /// The area from data value <i>n</i> to data value <i>(n+1)</i> is filled with color
        /// <i>n</i> ("right steps").
        /// </item>
        /// <item>
        /// <see cref="PaletteMode.Equal"/> is not supported.
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="PaletteException">
        /// The palette is empty or there is no suitable color for the start value or the end value.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The specified palette mode (<see cref="PaletteMode.Equal"/>) is not supported.
        /// </exception>
        public GradientStopCollection GetGradient(double startValue, double endValue)
        {
            if (Count == 0)
                throw new PaletteException("The palette is empty.");

            if (Mode == PaletteMode.Equal)
                throw new NotSupportedException("The specified mode is not supported.");

            BuildInternalSortedList();

            if (Mode == PaletteMode.Closest)
            {
                // Special case: Internally convert this palette from PaletteMode.Closest to
                // PaletteMode.LessOrEqual.
                ConvertPalette();
                return _convertedPalette.GetGradient(startValue, endValue);
            }

            bool reverseGradient = false;
            if (startValue > endValue)
            {
                double dummy = startValue;
                startValue = endValue;
                endValue = dummy;
                reverseGradient = true;
            }

            Color startColor = GetColor(startValue);
            Color endColor = GetColor(endValue);
            List<PaletteEntry> intermediateEntries = GetIntermediateEntries(startValue, endValue);

            GradientStopCollection gradientStops = new GradientStopCollection();

            // Add first GradientStop for startValue.
            GradientStop gradientStop = new GradientStop { Color = startColor, Offset = 0 };
            gradientStops.Add(gradientStop);
            PaletteEntry previousEntry = new PaletteEntry(startValue, startColor);

            // Add values for intermediate palette entries
            if (intermediateEntries != null)
            {
                for (int i = 0; i < intermediateEntries.Count; i++)
                {
                    PaletteEntry entry = intermediateEntries[i];
                    AddGradientStops(gradientStops, startValue, previousEntry, entry, endValue, Mode);
                    previousEntry = entry;
                }
            }

            // Add GradientStop between last and previous entry.
            PaletteEntry endEntry = new PaletteEntry(endValue, endColor);
            AddGradientStops(gradientStops, startValue, previousEntry, endEntry, endValue, Mode);

            CleanUpGradient(gradientStops);

            if (reverseGradient)
                gradientStops = ReverseGradient(gradientStops);

            return gradientStops;
        }


        /// <summary>
        /// Gets the intermediate palette entries.
        /// </summary>
        /// <param name="startValue">The start value.</param>
        /// <param name="endValue">The end value.</param>
        /// <returns>A sorted list of palette entries.</returns>
        private List<PaletteEntry> GetIntermediateEntries(double startValue, double endValue)
        {
            List<PaletteEntry> intermediateEntries = new List<PaletteEntry>();
            for (int i = 0; i < _sortedPaletteEntries.Count; i++)
            {
#if SILVERLIGHT || WINDOWS_PHONE
                PaletteEntry entry = _sortedPaletteEntries[i];
#else
                PaletteEntry entry = _sortedPaletteEntries.Values[i];
#endif
                if (startValue < entry.Value && entry.Value < endValue)
                    intermediateEntries.Add(entry);
            }

            return intermediateEntries;
        }


        /// <summary>
        /// Adds the required <see cref="GradientStop"/>s to the gradient.
        /// </summary>
        /// <param name="gradientStops">The gradient.</param>
        /// <param name="startValue">The start value that corresponds to gradient offset 0.</param>
        /// <param name="previousEntry">The previous entry.</param>
        /// <param name="entry">The entry.</param>
        /// <param name="endValue">The end value that corresponds to gradient offset 1.</param>
        /// <param name="mode">The palette mode.</param>
        /// <remarks>
        /// This method adds all required <see cref="GradientStop"/>s from the previous up to the
        /// current entry.
        /// </remarks>
        private static void AddGradientStops(GradientStopCollection gradientStops, double startValue, PaletteEntry previousEntry, PaletteEntry entry, double endValue, PaletteMode mode)
        {
            Debug.Assert(gradientStops != null, "gradientStops must not be null.");
            Debug.Assert(startValue <= endValue, "startValue must be less than or equal to endValue.");

            double previousValue = previousEntry.Value;
            double nextValue = entry.Value;
            double offset;

            switch (mode)
            {
                case PaletteMode.Interpolate:
                    offset = GetGradientStopOffset(startValue, previousValue, nextValue, endValue, 1.0);
                    gradientStops.Add(new GradientStop { Color = entry.Color, Offset = offset });
                    break;
                case PaletteMode.GreaterOrEqual:
                    offset = GetGradientStopOffset(startValue, previousValue, nextValue, endValue, 0.0);
                    gradientStops.Add(new GradientStop { Color = entry.Color, Offset = offset });
                    offset = GetGradientStopOffset(startValue, previousValue, nextValue, endValue, 1.0);
                    gradientStops.Add(new GradientStop { Color = entry.Color, Offset = offset });
                    break;
                case PaletteMode.LessOrEqual:
                    offset = GetGradientStopOffset(startValue, previousValue, nextValue, endValue, 1.0);
                    gradientStops.Add(new GradientStop { Color = previousEntry.Color, Offset = offset });
                    offset = GetGradientStopOffset(startValue, previousValue, nextValue, endValue, 1.0);
                    gradientStops.Add(new GradientStop { Color = entry.Color, Offset = offset });
                    break;
                default:
                    throw new ArgumentException("Unsupported palette mode.");
            }
        }


        /// <summary>
        /// Gets the offset of the <see cref="GradientStop"/>.
        /// </summary>
        /// <param name="startValue">The start value that corresponds to offset 0.</param>
        /// <param name="previousValue">The previous value.</param>
        /// <param name="nextValue">The next value.</param>
        /// <param name="endValue">The end value that corresponds to offset 1.</param>
        /// <param name="relativePosition">
        /// The relative position between <paramref name="previousValue"/> (0) and
        /// <paramref name="nextValue"/> (1).
        /// </param>
        /// <returns>The offset in the final gradient.</returns>
        private static double GetGradientStopOffset(double startValue, double previousValue, double nextValue, double endValue, double relativePosition)
        {
            Debug.Assert(startValue <= previousValue, "startValue must be less than or equal to previousValue.");
            Debug.Assert(previousValue <= nextValue, "previousValue must be less than or equal to nextValue.");
            Debug.Assert(nextValue <= endValue, "nextValue must be less than or equal to endValue.");
            Debug.Assert(0 <= relativePosition && relativePosition <= 1, "relativePosition must be in the range [0, 1].");

            if (Numeric.AreEqual(startValue, endValue))
                return 1.0;

            double value = previousValue + relativePosition * (nextValue - previousValue);
            double normalizedValue = (value - startValue) / (endValue - startValue);
            return normalizedValue;
        }


        /// <summary>
        /// Cleans up gradient (removes redundant gradients stops).
        /// </summary>
        /// <param name="gradientStops">The gradient.</param>
        private static void CleanUpGradient(GradientStopCollection gradientStops)
        {
            for (int i = gradientStops.Count - 2; i > 0; i--)
            {
                Color previousColor = gradientStops[i - 1].Color;
                Color currentColor = gradientStops[i].Color;
                Color nextColor = gradientStops[i + 1].Color;
                if (previousColor == currentColor && currentColor == nextColor)
                    gradientStops.RemoveAt(i);
            }
        }


        /// <summary>
        /// Reverses the gradient.
        /// </summary>
        /// <param name="gradient">The gradient.</param>
        /// <returns>A gradient in which the <see cref="GradientStop"/>s are reversed.</returns>
        private static GradientStopCollection ReverseGradient(GradientStopCollection gradient)
        {
            GradientStopCollection reversedGradient = new GradientStopCollection();
            for (int i = gradient.Count - 1; i >= 0; i--)
            {
                GradientStop gradientStop = gradient[i];
                GradientStop reversedGradientStop = new GradientStop
                {
                    Color = gradientStop.Color,
                    Offset = 1.0 - gradientStop.Offset
                };
                reversedGradient.Add(reversedGradientStop);
            }

            return reversedGradient;
        }
        #endregion
    }
}
