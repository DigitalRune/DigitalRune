// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows.Media;
using DigitalRune.Windows.Controls;


namespace DigitalRune.Windows.Themes
{
    /// <summary>
    /// Provides a set of predefined <see cref="MultiColorGlyph" /> objects.
    /// </summary>
    public static class MultiColorGlyphs
    {
        private static readonly FontFamily FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "/DigitalRune.Windows.Themes;component/Resources/#DigitalRune Icons");

        // Icon theme colors. (Defined in theme resource dictionaries.)
        private const string DarkGray = "Icon.DarkGray";
        private const string LightGray = "Icon.LightGray";
        private const string Red = "Icon.Red";
        private const string Orange = "Icon.Orange";
        private const string Green = "Icon.Green";
        private const string Blue = "Icon.Blue";


        /// <summary>
        /// Gets the "Open" symbol.
        /// </summary>
        /// <value>The "Open" symbol.</value>
        public static MultiColorGlyph Open { get; } = Create("\xE005", "\xE006");

        /// <summary>
        /// Gets the "Reload" symbol.
        /// </summary>
        /// <value>The "Reload" symbol.</value>
        public static MultiColorGlyph Reload { get; } = Create("\xE017", "\xE018", "\xE08A", "\xE08B", Green);

        /// <summary>
        /// Gets the "Close document" symbol.
        /// </summary>
        /// <value>The "Close document" symbol.</value>
        public static MultiColorGlyph Close { get; } = Create("\xE013", "\xE014", "\xE07C", "\xE07D", Blue);

        /// <summary>
        /// Gets the "Close all documents" symbol.
        /// </summary>
        /// <value>The "Close all documents" symbol.</value>
        public static MultiColorGlyph CloseAll { get; } = Create("\xE015", "\xE016", "\xE07C", "\xE07D", Blue);

        /// <summary>
        /// Gets the "Save document" symbol.
        /// </summary>
        /// <value>The "Save document" symbol.</value>
        public static MultiColorGlyph Save { get; } = Create("\xE00D", "\xE00E");

        /// <summary>
        /// Gets the "Save document as" symbol.
        /// </summary>
        /// <value>The "Save document as" symbol.</value>
        public static MultiColorGlyph SaveAs { get; } = Create("\xE00F", "\xE010", "\xE07A", "\xE07B", Blue);

        /// <summary>
        /// Gets the "Save all documents" symbol.
        /// </summary>
        /// <value>The "Save all documents" symbol.</value>
        public static MultiColorGlyph SaveAll { get; } = Create("\xE011", "\xE012");

        /// <summary>
        /// Gets the "Print preview" symbol.
        /// </summary>
        /// <value>The "Print preview" symbol.</value>
        public static MultiColorGlyph PrintPreview { get; } = Create("\xE017", "\xE018", "\xE082", "\xE083", Blue);

        /// <summary>
        /// Gets the "Print" symbol.
        /// </summary>
        /// <value>The "Print" symbol.</value>
        public static MultiColorGlyph Print { get; } = Create("\xE019", "\xE01A");

        /// <summary>
        /// Gets the "Exit" symbol.
        /// </summary>
        /// <value>The "Exit" symbol.</value>
        public static MultiColorGlyph Exit { get; } = Create("\xE01B", "\xE01C");

        /// <summary>
        /// Gets the "Undo" symbol.
        /// </summary>
        /// <value>The "Undo" symbol.</value>
        public static MultiColorGlyph Undo { get; } = Create("\xE04B", "\xE04C");

        /// <summary>
        /// Gets the "Redo" symbol.
        /// </summary>
        /// <value>The "Redo" symbol.</value>
        public static MultiColorGlyph Redo { get; } = Create("\xE04D", "\xE04E");

        /// <summary>
        /// Gets the "Cut" symbol.
        /// </summary>
        /// <value>The "Cut" symbol.</value>
        public static MultiColorGlyph Cut { get; } = Create("\xE04F", "\xE050");

        /// <summary>
        /// Gets the "Copy" symbol.
        /// </summary>
        /// <value>The "Copy" symbol.</value>
        public static MultiColorGlyph Copy { get; } = Create("\xE051", "\xE052");

        /// <summary>
        /// Gets the "Paste" symbol.
        /// </summary>
        /// <value>The "Paste" symbol.</value>
        public static MultiColorGlyph Paste { get; } = Create("\xE053", "\xE054");

        /// <summary>
        /// Gets the "Delete" symbol.
        /// </summary>
        /// <value>The "Delete" symbol.</value>
        public static MultiColorGlyph Delete { get; } = Create("\xE055", "\xE056");

        /// <summary>
        /// Gets the "Find" symbol.
        /// </summary>
        /// <value>The "Find" symbol.</value>
        public static MultiColorGlyph Find { get; } = Create("\xE057", "\xE058");

        /// <summary>
        /// Gets the "Find" symbol (without outline).
        /// </summary>
        /// <value>The "Find" symbol (without outline).</value>
        public static MultiColorGlyph Find2 { get; } = Create(null, "\xE058");

        /// <summary>
        /// Gets the "Find next" symbol.
        /// </summary>
        /// <value>The "Find next" symbol.</value>
        public static MultiColorGlyph FindNext { get; } = Create("\xE059", "\xE05A", "\xE07E", "\xE07F", Blue);

        /// <summary>
        /// Gets the "Find previous" symbol.
        /// </summary>
        /// <value>The "Find previous" symbol.</value>
        public static MultiColorGlyph FindPrevious { get; } = Create("\xE059", "\xE05A", "\xE080", "\xE081", Blue);

        /// <summary>
        /// Gets the "Increase indentation" symbol.
        /// </summary>
        /// <value>The "Increase indentation" symbol.</value>
        public static MultiColorGlyph IncreaseIndentation { get; } = Create("\xE068", "\xE069", "\xE084", "\xE085", Blue);

        /// <summary>
        /// Gets the "Decrease indentation" symbol.
        /// </summary>
        /// <value>The "Decrease indentation" symbol.</value>
        public static MultiColorGlyph DecreaseIndentation { get; } = Create("\xE068", "\xE069", "\xE086", "\xE087", Blue);

        /// <summary>
        /// Gets the "Comment" symbol.
        /// </summary>
        /// <value>The "Comment" symbol.</value>
        public static MultiColorGlyph Comment { get; } = Create("\xE06A", "\xE06B");

        /// <summary>
        /// Gets the "Uncomment" symbol.
        /// </summary>
        /// <value>The "Uncomment" symbol.</value>
        public static MultiColorGlyph Uncomment { get; } = Create("\xE06C", "\xE06D", "\xE088", "\xE089", Blue);

        /// <summary>
        /// Gets the "Zoom in" symbol.
        /// </summary>
        /// <value>The "Zoom in" symbol.</value>
        public static MultiColorGlyph ZoomIn { get; } = Create("\xE05B", "\xE05C");

        /// <summary>
        /// Gets the "Zoom out" symbol.
        /// </summary>
        /// <value>The "Zoom out" symbol.</value>
        public static MultiColorGlyph ZoomOut { get; } = Create("\xE05B", "\xE05D");

        /// <summary>
        /// Gets the "Show actual size" symbol.
        /// </summary>
        /// <value>The "Show actual size" symbol.</value>
        public static MultiColorGlyph ShowActualSize { get; } = Create("\xE062", "\xE063");

        /// <summary>
        /// Gets the "Fit page" symbol.
        /// </summary>
        /// <value>The "Fit page" symbol.</value>
        public static MultiColorGlyph FitPageWidth { get; } = Create("\xE064", "\xE065", "\xE08F", "\xE090", Blue);

        /// <summary>
        /// Gets the "Show whole page" symbol.
        /// </summary>
        /// <value>The "Show whole page" symbol.</value>
        public static MultiColorGlyph ShowWholePage { get; } = Create("\xE05E", "\xE05F");

        /// <summary>
        /// Gets the "Show two pages" symbol.
        /// </summary>
        /// <value>The "Show two pages" symbol.</value>
        public static MultiColorGlyph ShowTwoPages { get; } = Create("\xE060", "\xE061");

        /// <summary>
        /// Gets the "Color palette" symbol.
        /// </summary>
        /// <value>The "Color palette" symbol.</value>
        public static MultiColorGlyph ColorPalette { get; } = Create("\xE021", "\xE022");

        /// <summary>
        /// Gets the "Eye dropper" symbol.
        /// </summary>
        /// <value>The "Eye dropper" symbol.</value>
        public static MultiColorGlyph EyeDropper { get; } = Create("\xE032", "\xE033");

        /// <summary>
        /// Gets the "Output" symbol.
        /// </summary>
        /// <value>The "Output" symbol.</value>
        public static MultiColorGlyph Output { get; } = Create("\xE023", "\xE024");

        /// <summary>
        /// Gets the "Properties" symbol.
        /// </summary>
        /// <value>The "Properties" symbol.</value>
        public static MultiColorGlyph Properties { get; } = Create("\xE049", "\xE04A");

        /// <summary>
        /// Gets the "Options" symbol.
        /// </summary>
        /// <value>The "Options" symbol.</value>
        public static MultiColorGlyph Options { get; } = Create("\xE047", "\xE048"); // Cogwheel

        /// <summary>
        /// Gets the "Refresh" symbol.
        /// </summary>
        /// <value>The "Refresh" symbol.</value>
        public static MultiColorGlyph Refresh { get; } = Create("\xE066", "\xE067");

        /// <summary>
        /// Gets the "Build" symbol.
        /// </summary>
        /// <value>The "Build" symbol.</value>
        public static MultiColorGlyph Build { get; } = Create("\xE06E", "\xE06F", "\xE08D", "\xE08E", Blue);

        /// <summary>
        /// Gets the "Build all" symbol.
        /// </summary>
        /// <value>The "Build all" symbol.</value>
        public static MultiColorGlyph BuildAll { get; } = Create("\xE06E", "\xE070", "\xE08D", "\xE08E", Blue);

        /// <summary>
        /// Gets the "Left arrow" symbol.
        /// </summary>
        /// <value>The "Left arrow" symbol.</value>
        public static MultiColorGlyph ArrowLeft { get; } = Create("\xE03F", "\xE040");

        /// <summary>
        /// Gets the "Right arrow" symbol.
        /// </summary>
        /// <value>The "Right arrow" symbol.</value>
        public static MultiColorGlyph ArrowRight { get; } = Create("\xE041", "\xE042");

        /// <summary>
        /// Gets the "Up arrow" symbol.
        /// </summary>
        /// <value>The "Up arrow" symbol.</value>
        public static MultiColorGlyph ArrowUp { get; } = Create("\xE043", "\xE044");

        /// <summary>
        /// Gets the "Down arrow" symbol.
        /// </summary>
        /// <value>The "Down arrow" symbol.</value>
        public static MultiColorGlyph ArrowDown { get; } = Create("\xE045", "\xE046");

        /// <summary>
        /// Gets the "New window" symbol.
        /// </summary>
        /// <value>The "New window" symbol.</value>
        public static MultiColorGlyph NewWindow { get; } = Create("\xE01D", "\xE01E", "\xE078", "\xE079", Green);

        /// <summary>
        /// Gets the "Split window" symbol.
        /// </summary>
        /// <value>The "Split window" symbol.</value>
        public static MultiColorGlyph SplitWindow { get; } = Create("\xE01F", "\xE020");

        /// <summary>
        /// Gets the "Group by category" symbol.
        /// </summary>
        /// <value>The "Group by category" symbol.</value>
        public static MultiColorGlyph Categorize { get; } = Create("\xE02D", "\xE02E");

        /// <summary>
        /// Gets the "Sort ascending" symbol.
        /// </summary>
        /// <value>The "Sort ascending" symbol.</value>
        public static MultiColorGlyph SortAscending { get; } = Create("\xE02F", "\xE030", null, "\xE031", Blue);

        /// <summary>
        /// Gets the "View extra large icons" symbol.
        /// </summary>
        /// <value>The "View extra large icons" symbol.</value>
        public static MultiColorGlyph ViewExtraLargeIcons { get; } = Create("\xE025", "\xE026");

        /// <summary>
        /// Gets the "View large icons" symbol.
        /// </summary>
        /// <value>The "View large icons" symbol.</value>
        public static MultiColorGlyph ViewLargeIcons { get; } = Create("\xE025", "\xE027");

        /// <summary>
        /// Gets the "View medium icons" symbol.
        /// </summary>
        /// <value>The "View medium icons" symbol.</value>
        public static MultiColorGlyph ViewMediumIcons { get; } = Create("\xE025", "\xE028");

        /// <summary>
        /// Gets the "View small icons" symbol.
        /// </summary>
        /// <value>The "View small icons" symbol.</value>
        public static MultiColorGlyph ViewSmallIcons { get; } = Create("\xE025", "\xE029");

        /// <summary>
        /// Gets the "View tiles" symbol.
        /// </summary>
        /// <value>The "View tiles" symbol.</value>
        public static MultiColorGlyph ViewTiles { get; } = Create("\xE025", "\xE02A");

        /// <summary>
        /// Gets the "View list" symbol.
        /// </summary>
        /// <value>The "View list" symbol.</value>
        public static MultiColorGlyph ViewList { get; } = Create("\xE025", "\xE02B");

        /// <summary>
        /// Gets the "View details" symbol.
        /// </summary>
        /// <value>The "View details" symbol.</value>
        public static MultiColorGlyph ViewDetails { get; } = Create("\xE025", "\xE02C");

        /// <summary>
        /// Gets the "Document" symbol.
        /// </summary>
        /// <value>The "Document" symbol.</value>
        public static MultiColorGlyph Document { get; } = Create("\xE017", "\xE018");

        /// <summary>
        /// Gets the "Document" symbol.
        /// </summary>
        /// <value>The "Document" symbol.</value>
        public static MultiColorGlyph DocumentXml { get; } = Create("\xE036", "\xE037");

        /// <summary>
        /// Gets the "Effect" symbol.
        /// </summary>
        /// <value>The "Effect" symbol.</value>
        public static MultiColorGlyph DocumentEffect { get; } = Create("\xE036", "\xE038");

        /// <summary>
        /// Gets the "Image" symbol.
        /// </summary>
        /// <value>The "Image" symbol.</value>
        public static MultiColorGlyph Image { get; } = Create("\xE034", "\xE035");

        /// <summary>
        /// Gets the "Texture" symbol.
        /// </summary>
        /// <value>The "Texture" symbol.</value>
        public static MultiColorGlyph Texture { get; } = Create("\xE039", "\xE03A");

        /// <summary>
        /// Gets the "Model" symbol.
        /// </summary>
        /// <value>The "Model" symbol.</value>
        public static MultiColorGlyph Model { get; } = Create("\xE03B", "\xE03C");

        /// <summary>
        /// Gets the "Stop playback" symbol.
        /// </summary>
        /// <value>The "Stop playback" symbol.</value>
        public static MultiColorGlyph Stop { get; } = Create("\xE091", "\xE092");

        /// <summary>
        /// Gets the "Pause playback" symbol.
        /// </summary>
        /// <value>The "Pause playback" symbol.</value>
        public static MultiColorGlyph Pause { get; } = Create("\xE093", "\xE094");

        /// <summary>
        /// Gets the "Play" symbol.
        /// </summary>
        /// <value>The "Play" symbol.</value>
        public static MultiColorGlyph Play { get; } = Create("\xE095", "\xE096");

        /// <summary>
        /// Gets the "Play reverse" symbol.
        /// </summary>
        /// <value>The "Play reverse" symbol.</value>
        public static MultiColorGlyph PlayReverse { get; } = Create("\xE097", "\xE098");

        /// <summary>
        /// Gets the "Record" symbol.
        /// </summary>
        /// <value>The "Record" symbol.</value>
        public static MultiColorGlyph Record { get; } = Create("\xE099", null, "\xE09A", Red);

        /// <summary>
        /// Gets the "Faster" symbol.
        /// </summary>
        /// <value>The "Faster" symbol.</value>
        public static MultiColorGlyph Faster { get; } = Create("\xE09B", "\xE09C");

        /// <summary>
        /// Gets the "Slower" symbol.
        /// </summary>
        /// <value>The "Slower" symbol.</value>
        public static MultiColorGlyph Slower { get; } = Create("\xE09D", "\xE09E");

        /// <summary>
        /// Gets the "Forward/Next" symbol.
        /// </summary>
        /// <value>The "Forward/Next" symbol.</value>
        public static MultiColorGlyph Next { get; } = Create("\xE09F", "\xE0A0");

        /// <summary>
        /// Gets the "Backward/Previous" symbol.
        /// </summary>
        /// <value>The "Backward/Previous" symbol.</value>
        public static MultiColorGlyph Previous { get; } = Create("\xE0A1", "\xE0A2");

        /// <summary>
        /// Gets the "Plugin" symbol.
        /// </summary>
        /// <value>The "Plugin" symbol.</value>
        public static MultiColorGlyph Plugin { get; } = Create("\xE0A3", "\xE0A4");

        /// <summary>
        /// Gets the "Scene node" symbol.
        /// </summary>
        /// <value>The "Scene node" symbol.</value>
        public static MultiColorGlyph SceneNode { get; } = Create("\xE0A5", "\xE0A6");

        /// <summary>
        /// Gets the "Skeleton" symbol.
        /// </summary>
        /// <value>The "Skeleton" symbol.</value>
        public static MultiColorGlyph Skeleton { get; } = Create("\xE0A7", "\xE0A8");

        /// <summary>
        /// Gets the "Bone" symbol.
        /// </summary>
        /// <value>The "Bone" symbol.</value>
        public static MultiColorGlyph Bone { get; } = Create("\xE0A9", "\xE0AA");

        /// <summary>
        /// Gets the "Wireframe" symbol.
        /// </summary>
        /// <value>The "Wireframe" symbol.</value>
        public static MultiColorGlyph Wireframe { get; } = Create("\xE0AB", "\xE0AC");

        /// <summary>
        /// Gets the "Bounds" symbol.
        /// </summary>
        /// <value>The "Bounds" symbol.</value>
        public static MultiColorGlyph Bounds { get; } = Create("\xE0AD", "\xE0AE");

        /// <summary>
        /// Gets the "Textured" symbol.
        /// </summary>
        /// <value>The "Textured" symbol.</value>
        public static MultiColorGlyph Textured { get; } = Create("\xE0AF", "\xE0B0");

        /// <summary>
        /// Gets the "Home" symbol.
        /// </summary>
        /// <value>The "Home" symbol.</value>
        public static MultiColorGlyph Home { get; } = Create("\xE0B1", "\xE0B2");

        /// <summary>
        /// Gets the "Ragdoll" symbol.
        /// </summary>
        /// <value>The "Ragdoll" symbol.</value>
        public static MultiColorGlyph Ragdoll { get; } = Create("\xE0B3", "\xE0B4");

        /// <summary>
        /// Gets the "Aabb" symbol.
        /// </summary>
        /// <value>The "Aabb" symbol.</value>
        public static MultiColorGlyph Aabb { get; } = Create("\xE0B5", "\xE0B6");

        /// <summary>
        /// Gets the "Mesh" symbol.
        /// </summary>
        /// <value>The "Mesh" symbol.</value>
        public static MultiColorGlyph Mesh { get; } = Create("\xE0B7", "\xE0B8");

        /// <summary>
        /// Gets the "Error list" symbol.
        /// </summary>
        /// <value>The "Error list" symbol.</value>
        public static MultiColorGlyph ErrorList { get; } = Create("\xE0B9", null, "\xE0BA", null, "\xE0BB", /*LightGray,*/ "\xE0BC" /*, Red */);

        /// <summary>
        /// Gets the "Outline" symbol.
        /// </summary>
        /// <value>The "Outline" symbol.</value>
        public static MultiColorGlyph Outline { get; } = Create("\xE0BD", "\xE0BE");

        /// <summary>
        /// Gets the "Animation" symbol.
        /// </summary>
        /// <value>The "Animation" symbol.</value>
        public static MultiColorGlyph Animation { get; } = Create("\xE0BF", "\xE0C0");

        /// <summary>
        /// Gets the "Grid" symbol.
        /// </summary>
        /// <value>The "Grid" symbol.</value>
        public static MultiColorGlyph Grid { get; } = Create("\xE0C1", "\xE0C2");

        /// <summary>
        /// Gets the "Interface" symbol.
        /// </summary>
        /// <value>The "Interface" symbol.</value>
        public static MultiColorGlyph Interface { get; } = Create("\xE0C3", "\xE0C4");

        /// <summary>
        /// Gets the "Class" symbol.
        /// </summary>
        /// <value>The "Class" symbol.</value>
        public static MultiColorGlyph Class { get; } = Create("\xE0C5", "\xE0C6");

        /// <summary>
        /// Gets the "Struct" symbol.
        /// </summary>
        /// <value>The "Structure" symbol.</value>
        public static MultiColorGlyph Struct { get; } = Create("\xE0C7", "\xE0C8");

        /// <summary>
        /// Gets the "Method" symbol.
        /// </summary>
        /// <value>The "Method" symbol.</value>
        public static MultiColorGlyph Method { get; } = Create("\xE0C9", "\xE0CA");

        /// <summary>
        /// Gets the "Field" symbol.
        /// </summary>
        /// <value>The "Field" symbol.</value>
        public static MultiColorGlyph Field { get; } = Create("\xE0CB", "\xE0CC");

        /// <summary>
        /// Gets the "Enum" symbol.
        /// </summary>
        /// <value>The "Enum" symbol.</value>
        public static MultiColorGlyph Enum { get; } = Create("\xE0CD", "\xE0CE");

        /// <summary>
        /// Gets the "Snippet" symbol.
        /// </summary>
        /// <value>The "Snippet" symbol.</value>
        public static MultiColorGlyph Snippet { get; } = Create("\xE0CF", "\xE0D0");

        /// <summary>
        /// Gets the "Guess" symbol.
        /// </summary>
        /// <value>The "Guess" symbol.</value>
        public static MultiColorGlyph Guess { get; } = Create("\xE0D1", "\xE0D2");

        /// <summary>
        /// Gets the "Macro" symbol.
        /// </summary>
        /// <value>The "Macro" symbol.</value>
        public static MultiColorGlyph Macro { get; } = Create("\xE0D3", "\xE0D4");


        #region ----- Message symbols -----

        /// <summary>
        /// Gets the information symbol (i).
        /// </summary>
        /// <value>The information symbol (i).</value>
        public static MultiColorGlyph MessageInformation { get; } = Create("\xE071", LightGray, "\xE072", Blue);

        /// <summary>
        /// Gets the question symbol (?).
        /// </summary>
        /// <value>The question symbol (?).</value>
        public static MultiColorGlyph MessageQuestion { get; } = Create("\xE071", LightGray, "\xE073", Blue);

        /// <summary>
        /// Gets the warning symbol (!).
        /// </summary>
        /// <value>The warning symbol (!).</value>
        public static MultiColorGlyph MessageWarning { get; } = Create("\xE074", LightGray, "\xE075", Orange, null, "\xE076", DarkGray);

        /// <summary>
        /// Gets the error/stop symbol (X).
        /// </summary>
        /// <value>The error/stop symbol (X).</value>
        public static MultiColorGlyph MessageError { get; } = Create("\xE071", LightGray, "\xE077", Red);
        #endregion


        private static MultiColorGlyph Create(string backgroundGlyph, string foregroundGlyph)
        {
            return Create(backgroundGlyph, null, foregroundGlyph, null, null, null, null);
        }


        private static MultiColorGlyph Create(string backgroundGlyph, string foregroundGlyph, string overlayBackgroundGlyph, string overlayForegroundGlyph, object overlayBrushKey)
        {
            return Create(backgroundGlyph, null, foregroundGlyph, null, overlayBackgroundGlyph, overlayForegroundGlyph, overlayBrushKey);
        }


        private static MultiColorGlyph Create(string backgroundGlyph, object backgroundBrushKey, string foregroundGlyph, object foregroundBrushKey, string overlayBackgroundGlyph = null, string overlayForegroundGlyph = null, object overlayBrushKey = null)
        {
            return Create(backgroundGlyph, backgroundBrushKey, foregroundGlyph, foregroundBrushKey, overlayBackgroundGlyph, null, overlayForegroundGlyph, overlayBrushKey);
        }


        private static MultiColorGlyph Create(string backgroundGlyph, object backgroundBrushKey, string foregroundGlyph, object foregroundBrushKey, string overlayBackgroundGlyph, object overlayBackgroundBrushKey, string overlayForegroundGlyph, object overlayBrushKey)
        {
            return new MultiColorGlyph
            {
                FontFamily = FontFamily,
                BackgroundBrushKey = backgroundBrushKey,
                BackgroundGlyph = backgroundGlyph,
                ForegroundBrushKey = foregroundBrushKey,
                ForegroundGlyph = foregroundGlyph,
                OverlayBackgroundBrushKey = overlayBackgroundBrushKey,
                OverlayForegroundBrushKey = overlayBrushKey,
                OverlayBackgroundGlyph = overlayBackgroundGlyph,
                OverlayForegroundGlyph = overlayForegroundGlyph
            };
        }
    }
}
