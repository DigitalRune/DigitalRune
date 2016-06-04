// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows.Media;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Provides predicates that identify the axes of a <see cref="DefaultChartPanel"/>.
    /// </summary>
    public static class AxisPredicates
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets a predicate that accepts all axes.
        /// </summary>
        /// <value>A predicate that always returns <see langword="true"/>.</value>
        public static Predicate<Axis> AllAxes
        {
            get { return MatchesAll; }
        }


        /// <summary>
        /// Gets a predicate that rejects all axes.
        /// </summary>
        /// <value>A predicate that always returns <see langword="false"/>.</value>
        public static Predicate<Axis> None
        {
            get { return MatchesNone; }
        }


        /// <summary>
        /// Gets a predicate that accepts only x-axis.
        /// </summary>
        /// <value>
        /// A predicate that returns <see langword="true"/> if the given <see cref="Axis"/> is an
        /// x-axis.
        /// </value>
        public static Predicate<Axis> XAxes
        {
            get { return MatchesXAxes; }
        }


        /// <summary>
        /// Gets a predicate that accepts only y-axis.
        /// </summary>
        /// <value>
        /// A predicate that returns <see langword="true"/> if the given <see cref="Axis"/> is an
        /// y-axis.
        /// </value>
        public static Predicate<Axis> YAxes
        {
            get { return MatchesYAxes; }
        }


        /// <summary>
        /// Gets a predicate that accepts only the primary x-axis of a
        /// <see cref="DefaultChartPanel"/>.
        /// </summary>
        /// <value>
        /// A predicate that returns <see langword="true"/> if the given <see cref="Axis"/> is the
        /// primary x-axis of a <see cref="DefaultChartPanel"/>.
        /// </value>
        public static Predicate<Axis> XAxis1
        {
            get { return MatchesXAxis1; }
        }


        /// <summary>
        /// Gets a predicate that accepts only the primary y-axis of a
        /// <see cref="DefaultChartPanel"/>.
        /// </summary>
        /// <value>
        /// A predicate that returns <see langword="true"/> if the given <see cref="Axis"/> is the
        /// primary y-axis of a <see cref="DefaultChartPanel"/>.
        /// </value>
        public static Predicate<Axis> YAxis1
        {
            get { return MatchesYAxis1; }
        }


        /// <summary>
        /// Gets a predicate that accepts only the secondary x-axis of a
        /// <see cref="DefaultChartPanel"/>.
        /// </summary>
        /// <value>
        /// A predicate that returns <see langword="true"/> if the given <see cref="Axis"/> is the
        /// secondary x-axis of a <see cref="DefaultChartPanel"/>.
        /// </value>
        public static Predicate<Axis> XAxis2
        {
            get { return MatchesXAxis2; }
        }


        /// <summary>
        /// Gets a predicate that accepts only the secondary y-axis of a
        /// <see cref="DefaultChartPanel"/>.
        /// </summary>
        /// <value>
        /// A predicate that returns <see langword="true"/> if the given <see cref="Axis"/> is the
        /// secondary y-axis of a <see cref="DefaultChartPanel"/>.
        /// </value>
        public static Predicate<Axis> YAxis2
        {
            get { return MatchesYAxis2; }
        }


        /// <summary>
        /// Gets a predicate that accepts only the primary x- or y-axis of a
        /// <see cref="DefaultChartPanel"/>.
        /// </summary>
        /// <value>
        /// A predicate that returns <see langword="true"/> if the given <see cref="Axis"/> is the
        /// primary x-axis or y-axis of a <see cref="DefaultChartPanel"/>.
        /// </value>
        public static Predicate<Axis> PrimaryAxes
        {
            get { return MatchesPrimaryAxes; }
        }


        /// <summary>
        /// Gets a predicate that accepts only the secondary x- or y-axis of a
        /// <see cref="DefaultChartPanel"/>.
        /// </summary>
        /// <value>
        /// A predicate that returns <see langword="true"/> if the given <see cref="Axis"/> is the
        /// secondary x-axis or y-axis of a <see cref="DefaultChartPanel"/>.
        /// </value>
        public static Predicate<Axis> SecondaryAxes
        {
            get { return MatchesSecondaryAxes; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static bool MatchesAll(Axis axis)
        {
            return true;
        }


        private static bool MatchesNone(Axis axis)
        {
            return false;
        }


        private static bool MatchesXAxes(Axis axis)
        {
            return (axis != null && axis.IsXAxis);
        }


        private static bool MatchesYAxes(Axis axis)
        {
            return (axis != null && axis.IsYAxis);
        }


        private static bool MatchesXAxis1(Axis axis)
        {
            var chartPanel = VisualTreeHelper.GetParent(axis) as DefaultChartPanel;
            if (chartPanel != null)
                return (chartPanel.XAxis1 == axis);

            return false;
        }


        private static bool MatchesXAxis2(Axis axis)
        {
            var chartPanel = VisualTreeHelper.GetParent(axis) as DefaultChartPanel;
            if (chartPanel != null)
                return (chartPanel.XAxis2 == axis);

            return false;
        }


        private static bool MatchesYAxis1(Axis axis)
        {
            var chartPanel = VisualTreeHelper.GetParent(axis) as DefaultChartPanel;
            if (chartPanel != null)
                return (chartPanel.YAxis1 == axis);

            return false;
        }


        private static bool MatchesYAxis2(Axis axis)
        {
            var chartPanel = VisualTreeHelper.GetParent(axis) as DefaultChartPanel;
            if (chartPanel != null)
                return (chartPanel.YAxis2 == axis);

            return false;
        }


        private static bool MatchesPrimaryAxes(Axis axis)
        {
            var chartPanel = VisualTreeHelper.GetParent(axis) as DefaultChartPanel;
            if (chartPanel != null)
                return (chartPanel.XAxis1 == axis || chartPanel.YAxis1 == axis);

            return false;
        }


        private static bool MatchesSecondaryAxes(Axis axis)
        {
            var chartPanel = VisualTreeHelper.GetParent(axis) as DefaultChartPanel;
            if (chartPanel != null)
                return (chartPanel.XAxis2 == axis || chartPanel.YAxis2 == axis);

            return false;
        }
        #endregion
    }
}
