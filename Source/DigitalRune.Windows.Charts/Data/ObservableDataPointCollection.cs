// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Represents a list of data points that provides notifications when data points are added,
    /// removed, or when the whole list is refreshed.
    /// </summary>
    public class ObservableDataPointCollection : ObservableCollection<DataPoint>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDataPointCollection"/> class.
        /// </summary>
        public ObservableDataPointCollection()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDataPointCollection"/> class that contains
        /// the elements copied from the specified collection.
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied.</param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="collection"/> parameter cannot be <see langword="null"/>.
        /// </exception>
        public ObservableDataPointCollection(IEnumerable<Point> collection)
        {
            if (collection != null)
                foreach (Point point in collection)
                    Add(new DataPoint(point, null));
        }


#if WINDOWS_PHONE
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDataPointCollection"/> class that contains
        /// the elements copied from the specified collection.
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is <see langword="null"/>.
        /// </exception>
        public ObservableDataPointCollection(IEnumerable<DataPoint> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            foreach (DataPoint dataPoint in collection)
                Add(dataPoint);
        }
#else
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDataPointCollection"/> class that contains
        /// the elements copied from the specified collection.
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is <see langword="null"/>.
        /// </exception>
        public ObservableDataPointCollection(IEnumerable<DataPoint> collection)
            : base(collection)
        {
        }
#endif


#if WINDOWS_PHONE
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDataPointCollection"/> class that contains
        /// the elements copied from the specified list.
        /// </summary>
        /// <param name="list">The list from which the elements are copied.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="list"/> is <see langword="null"/>.
        /// </exception>
        public ObservableDataPointCollection(List<DataPoint> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            foreach (DataPoint dataPoint in list)
                Add(dataPoint);
        }
#else
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDataPointCollection"/> class that contains
        /// the elements copied from the specified list.
        /// </summary>
        /// <param name="list">The list from which the elements are copied.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="list"/> is <see langword="null"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public ObservableDataPointCollection(List<DataPoint> list)
            : base(list)
        {
        }
#endif
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------
        #endregion
    }
}
