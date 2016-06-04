using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using NUnit.Framework;


namespace DigitalRune.Windows.Charts.Tests
{
    [TestFixture]
    public class ChartDataHelperTest
    {
        [Test]
        [ExpectedException(typeof(ChartDataException))]
        public void InvalidDataSource()
        {
            ChartDataHelper.CreateChartDataSource(new double[] { 1, 2, 3 }, null, null, null, null, null, null);
        }


        [Test]
        public void NullDataSource()
        {
            var chartDataSource = ChartDataHelper.CreateChartDataSource(null, null, null, null, null, null, null);
            Assert.AreEqual(0, chartDataSource.Count);
        }


        [Test]
        public void PointArray()
        {
            var dataSource = new[] { new Point(2, 3), new Point(4, 5), new Point(6, 7) };
            var chartDataSource = ChartDataHelper.CreateChartDataSource(dataSource, null, null, null, null, null, null);
            Assert.AreEqual(3, chartDataSource.Count);
            Assert.AreEqual(new DataPoint(new Point(2, 3), null), chartDataSource[0]);
            Assert.AreEqual(new DataPoint(new Point(4, 5), null), chartDataSource[1]);
            Assert.AreEqual(new DataPoint(new Point(6, 7), null), chartDataSource[2]);
        }


        [Test]
        public void PointList()
        {
            var dataSource = new List<Point> { new Point(2, 3), new Point(4, 5), new Point(6, 7) };
            var chartDataSource = ChartDataHelper.CreateChartDataSource(dataSource, null, null, null, null, null, null);
            Assert.AreEqual(3, chartDataSource.Count);
            Assert.AreEqual(new DataPoint(new Point(2, 3), null), chartDataSource[0]);
            Assert.AreEqual(new DataPoint(new Point(4, 5), null), chartDataSource[1]);
            Assert.AreEqual(new DataPoint(new Point(6, 7), null), chartDataSource[2]);
        }


        [Test]
        public void DataPointCollection()
        {
            var dataSource = new DataPointCollection { new DataPoint(2, 3, null), new DataPoint(4, 5, null), new DataPoint(6, 7, null) };
            var chartDataSource = ChartDataHelper.CreateChartDataSource(dataSource, null, null, null, null, null, null);
            Assert.AreEqual(3, chartDataSource.Count);
            Assert.AreEqual(new DataPoint(new Point(2, 3), null), chartDataSource[0]);
            Assert.AreEqual(new DataPoint(new Point(4, 5), null), chartDataSource[1]);
            Assert.AreEqual(new DataPoint(new Point(6, 7), null), chartDataSource[2]);
        }


        [Test]
        public void ObservableCollectionOfPoints()
        {
            var dataSource = new ObservableCollection<Point> { new Point(2, 3), new Point(4, 5), new Point(6, 7) };
            var chartDataSource = ChartDataHelper.CreateChartDataSource(dataSource, null, null, null, null, null, null);
            Assert.AreEqual(3, chartDataSource.Count);
            Assert.AreEqual(new DataPoint(new Point(2, 3), null), chartDataSource[0]);
            Assert.AreEqual(new DataPoint(new Point(4, 5), null), chartDataSource[1]);
            Assert.AreEqual(new DataPoint(new Point(6, 7), null), chartDataSource[2]);
        }


        class CustomObjectWithPoint
        {
            public Point MyPoint { get; set; }
            public string Text { get; set; }
        }


        [Test]
        public void XYValuePath()
        {
            var dataSource = new List<CustomObjectWithPoint>
            {
                new CustomObjectWithPoint { MyPoint = new Point(2, 3), Text = "Text 1" },
                new CustomObjectWithPoint { MyPoint = new Point(4, 5), Text = "Text 2" },
                new CustomObjectWithPoint { MyPoint = new Point(6, 7), Text = "Text 3" },
            };
            var chartDataSource = ChartDataHelper.CreateChartDataSource(dataSource, null, null, new PropertyPath("MyPoint"), null, null, null);
            Assert.AreEqual(3, chartDataSource.Count);
            Assert.AreEqual(new DataPoint(new Point(2, 3), dataSource[0]), chartDataSource[0]);
            Assert.AreEqual(new DataPoint(new Point(4, 5), dataSource[1]), chartDataSource[1]);
            Assert.AreEqual(new DataPoint(new Point(6, 7), dataSource[2]), chartDataSource[2]);
        }


        [Test]
        public void ComplexXValuePathAndYValuePath()
        {
            var dataSource = new List<CustomObjectWithPoint>
            {
                new CustomObjectWithPoint { MyPoint = new Point(2, 3), Text = "Text 1" },
                new CustomObjectWithPoint { MyPoint = new Point(4, 5), Text = "Text 2" },
                new CustomObjectWithPoint { MyPoint = new Point(6, 7), Text = "Text 3" },
            };
            var chartDataSource = ChartDataHelper.CreateChartDataSource(dataSource, new PropertyPath("MyPoint.X"), new PropertyPath("MyPoint.Y"), null, null, null, null);
            Assert.AreEqual(3, chartDataSource.Count);
            Assert.AreEqual(new DataPoint(new Point(2, 3), dataSource[0]), chartDataSource[0]);
            Assert.AreEqual(new DataPoint(new Point(4, 5), dataSource[1]), chartDataSource[1]);
            Assert.AreEqual(new DataPoint(new Point(6, 7), dataSource[2]), chartDataSource[2]);
        }


        class CustomObjectWithXY
        {
            public double MyX { get; set; }
            public double MyY { get; set; }
            public string Text { get; set; }
        }


        [Test]
        public void XValuePathAndYValuePath()
        {
            var dataSource = new List<CustomObjectWithXY>
            {
                new CustomObjectWithXY { MyX = 2, MyY = 3, Text = "Text 1" },
                new CustomObjectWithXY { MyX = 4, MyY = 5, Text = "Text 2" },
                new CustomObjectWithXY { MyX = 6, MyY = 7, Text = "Text 3" },
            };
            var chartDataSource = ChartDataHelper.CreateChartDataSource(dataSource, new PropertyPath("MyX"), new PropertyPath("MyY"), null, null, null, null);
            Assert.AreEqual(3, chartDataSource.Count);
            Assert.AreEqual(new DataPoint(new Point(2, 3), dataSource[0]), chartDataSource[0]);
            Assert.AreEqual(new DataPoint(new Point(4, 5), dataSource[1]), chartDataSource[1]);
            Assert.AreEqual(new DataPoint(new Point(6, 7), dataSource[2]), chartDataSource[2]);
        }


        class CustomObjectWithDateTime
        {
            public DateTime X { get; set; }
            public DateTime Y { get; set; }
            public string Text { get; set; }
        }


        [Test]
        public void DateSourceWithDateTime()
        {
            var dataSource = new List<CustomObjectWithDateTime>
            {
                new CustomObjectWithDateTime { X = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc), Y = new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc), Text = "Text 1" },
                new CustomObjectWithDateTime { X = new DateTime(2003, 1, 1, 0, 0, 0, DateTimeKind.Utc), Y = new DateTime(2004, 1, 1, 0, 0, 0, DateTimeKind.Utc), Text = "Text 2" },
                new CustomObjectWithDateTime { X = new DateTime(2005, 1, 1, 0, 0, 0, DateTimeKind.Utc), Y = new DateTime(2006, 1, 1, 0, 0, 0, DateTimeKind.Utc), Text = "Text 3" },
            };
            var chartDataSource = ChartDataHelper.CreateChartDataSource(dataSource, new PropertyPath("X"), new PropertyPath("Y"), null, null, null, null);
            Assert.AreEqual(3, chartDataSource.Count);
            Assert.AreEqual(new DataPoint(new Point(new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks, new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks), dataSource[0]), chartDataSource[0]);
            Assert.AreEqual(new DataPoint(new Point(new DateTime(2003, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks, new DateTime(2004, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks), dataSource[1]), chartDataSource[1]);
            Assert.AreEqual(new DataPoint(new Point(new DateTime(2005, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks, new DateTime(2006, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks), dataSource[2]), chartDataSource[2]);
        }


        class CustomObjectWithStrings
        {
            public string X { get; set; }
            public string Y { get; set; }
            public string Text { get; set; }
        }


        [Test]
        public void DateSourceWithDateTimeStrings()
        {
            var dataSource = new List<CustomObjectWithStrings>
            {
                new CustomObjectWithStrings { X = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToString(), Y = new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToString(), Text = "Text 1" },
                new CustomObjectWithStrings { X = new DateTime(2003, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToString(), Y = new DateTime(2004, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToString(), Text = "Text 2" },
                new CustomObjectWithStrings { X = new DateTime(2005, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToString(), Y = new DateTime(2006, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToString(), Text = "Text 3" },
            };
            var chartDataSource = ChartDataHelper.CreateChartDataSource(dataSource, new PropertyPath("X"), new PropertyPath("Y"), null, null, null, null);
            Assert.AreEqual(3, chartDataSource.Count);
            Assert.AreEqual(new DataPoint(new Point(new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks, new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks), dataSource[0]), chartDataSource[0]);
            Assert.AreEqual(new DataPoint(new Point(new DateTime(2003, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks, new DateTime(2004, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks), dataSource[1]), chartDataSource[1]);
            Assert.AreEqual(new DataPoint(new Point(new DateTime(2005, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks, new DateTime(2006, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks), dataSource[2]), chartDataSource[2]);
        }


        [Test]
        public void DateSourceWithDoubleStrings()
        {
            var dataSource = new List<CustomObjectWithStrings>
            {
                new CustomObjectWithStrings { X = "2.00000", Y = "3", Text = "Text 1" },
                new CustomObjectWithStrings { X = "04", Y = "5", Text = "Text 2" },
                new CustomObjectWithStrings { X = "+6.0", Y = "7e0", Text = "Text 3" },
            };
            var chartDataSource = ChartDataHelper.CreateChartDataSource(dataSource, new PropertyPath("X"), new PropertyPath("Y"), null, CultureInfo.InvariantCulture, null, null);
            Assert.AreEqual(3, chartDataSource.Count);
            Assert.AreEqual(new DataPoint(new Point(2, 3), dataSource[0]), chartDataSource[0]);
            Assert.AreEqual(new DataPoint(new Point(4, 5), dataSource[1]), chartDataSource[1]);
            Assert.AreEqual(new DataPoint(new Point(6, 7), dataSource[2]), chartDataSource[2]);
        }


        [Test]
        public void DateSourceWithDoubleStringsGerman()
        {
            var dataSource = new List<CustomObjectWithStrings>
            {
                new CustomObjectWithStrings { X = "2,00000", Y = "3,0", Text = "Text 1" },
                new CustomObjectWithStrings { X = "0.004", Y = "5", Text = "Text 2" },
                new CustomObjectWithStrings { X = "+6,0", Y = "7e0", Text = "Text 3" },
            };
            var chartDataSource = ChartDataHelper.CreateChartDataSource(dataSource, new PropertyPath("X"), new PropertyPath("Y"), null, new CultureInfo("de-AT"), null, null);
            Assert.AreEqual(3, chartDataSource.Count);
            Assert.AreEqual(new DataPoint(new Point(2, 3), dataSource[0]), chartDataSource[0]);
            Assert.AreEqual(new DataPoint(new Point(4, 5), dataSource[1]), chartDataSource[1]);
            Assert.AreEqual(new DataPoint(new Point(6, 7), dataSource[2]), chartDataSource[2]);
        }


        [Test]
        public void CompositeDataSource()
        {
            var xValues = new List<CustomObjectWithStrings>
            {
                new CustomObjectWithStrings { X = "2.00000", Text = "Text 1" },
                new CustomObjectWithStrings { X = "04", Text = "Text 2" },
                new CustomObjectWithStrings { X = "+6.0", Text = "Text 3" },
            };
            var yValues = new List<CustomObjectWithXY>
            {
                new CustomObjectWithXY { MyY = 3, Text = "Text 1" },
                new CustomObjectWithXY { MyY = 5, Text = "Text 2" },
                new CustomObjectWithXY { MyY = 7, Text = "Text 3" },
            };
            var dataSource = new CompositeDataSource
            {
                XValues = xValues,
                YValues = yValues,
            };
            var chartDataSource = ChartDataHelper.CreateChartDataSource(dataSource, new PropertyPath("X"), new PropertyPath("MyY"), null, CultureInfo.InvariantCulture, null, null);
            Assert.AreEqual(3, chartDataSource.Count);
            Assert.AreEqual(new Point(2, 3), chartDataSource[0].Point);
            Assert.AreEqual(new Point(4, 5), chartDataSource[1].Point);
            Assert.AreEqual(new Point(6, 7), chartDataSource[2].Point);
            Assert.AreEqual(new CompositeData(xValues[0], yValues[0]), chartDataSource[0].DataContext);
            Assert.AreEqual(new CompositeData(xValues[1], yValues[1]), chartDataSource[1].DataContext);
            Assert.AreEqual(new CompositeData(xValues[2], yValues[2]), chartDataSource[2].DataContext);
        }


        [Test]
        public void DateSourceWithTextLabelsForX()
        {
            var textLabels = new List<TextLabel>
            {
                new TextLabel(2.0, "Label2"),
                new TextLabel(4.0, "Label4"),
            };
            var dataSource = new List<CustomObjectWithStrings>
            {
                new CustomObjectWithStrings { X = "1", Y = "3", Text = "Text 1" },
                new CustomObjectWithStrings { X = "Label2", Y = "5", Text = "Text 2" },
                new CustomObjectWithStrings { X = "Label4", Y = "7", Text = "Text 3" },
                new CustomObjectWithStrings { X = "Label5", Y = "9", Text = "Text 4" },
            };

            var chartDataSource = ChartDataHelper.CreateChartDataSource(dataSource, new PropertyPath("X"), new PropertyPath("Y"), null, CultureInfo.InvariantCulture, textLabels, null);

            Assert.AreEqual(3, textLabels.Count);
            Assert.AreEqual(new TextLabel(2.0, "Label2"), textLabels[0]);
            Assert.AreEqual(new TextLabel(4.0, "Label4"), textLabels[1]);
            Assert.AreEqual(new TextLabel(5.0, "Label5"), textLabels[2]);

            Assert.AreEqual(4, chartDataSource.Count);
            Assert.AreEqual(new DataPoint(new Point(1, 3), dataSource[0]), chartDataSource[0]);
            Assert.AreEqual(new DataPoint(new Point(2, 5), dataSource[1]), chartDataSource[1]);
            Assert.AreEqual(new DataPoint(new Point(4, 7), dataSource[2]), chartDataSource[2]);
            Assert.AreEqual(new DataPoint(new Point(5, 9), dataSource[3]), chartDataSource[3]);
        }


        [Test]
        public void DateSourceWithTextLabelsForY()
        {
            var textLabels = new List<TextLabel>
            {
                new TextLabel(2.0, "Label2"),
                new TextLabel(4.0, "Label4"),
            };
            var dataSource = new List<CustomObjectWithStrings>
            {
                new CustomObjectWithStrings { X = "1", Y = "3", Text = "Text 1" },
                new CustomObjectWithStrings { X = "2", Y = "Label2", Text = "Text 2" },
                new CustomObjectWithStrings { X = "4", Y = "Label4", Text = "Text 3" },
                new CustomObjectWithStrings { X = "5", Y = "Label5", Text = "Text 4" },
            };

            var chartDataSource = ChartDataHelper.CreateChartDataSource(dataSource, new PropertyPath("X"), new PropertyPath("Y"), null, CultureInfo.InvariantCulture, null, textLabels);

            Assert.AreEqual(3, textLabels.Count);
            Assert.AreEqual(new TextLabel(2.0, "Label2"), textLabels[0]);
            Assert.AreEqual(new TextLabel(4.0, "Label4"), textLabels[1]);
            Assert.AreEqual(new TextLabel(5.0, "Label5"), textLabels[2]);

            Assert.AreEqual(4, chartDataSource.Count);
            Assert.AreEqual(new DataPoint(new Point(1, 3), dataSource[0]), chartDataSource[0]);
            Assert.AreEqual(new DataPoint(new Point(2, 2), dataSource[1]), chartDataSource[1]);
            Assert.AreEqual(new DataPoint(new Point(4, 4), dataSource[2]), chartDataSource[2]);
            Assert.AreEqual(new DataPoint(new Point(5, 5), dataSource[3]), chartDataSource[3]);
        }


        [Test]
        public void DateSourceWithTextLabelsForXAndY()
        {
            var xLabels = new List<TextLabel>
            {
                new TextLabel(1.0, "x1"),
                new TextLabel(2.0, "x2"),
                new TextLabel(3.0, "x3"),
                new TextLabel(4.0, "x4"),
            };
            var yLabels = new List<TextLabel>
            {
                new TextLabel(1.0, "y1"),
                new TextLabel(2.0, "y2"),
                new TextLabel(3.0, "y3"),
                new TextLabel(4.0, "y4"),
            };
            var dataSource = new List<CustomObjectWithStrings>
            {
                new CustomObjectWithStrings { X = "x1", Y = "3", Text = "Text 1" },
                new CustomObjectWithStrings { X = "x2", Y = "y1", Text = "Text 2" },
                new CustomObjectWithStrings { X = "x3", Y = "y4", Text = "Text 3" },
                new CustomObjectWithStrings { X = "x4", Y = "y3", Text = "Text 4" },
            };

            var chartDataSource = ChartDataHelper.CreateChartDataSource(dataSource, new PropertyPath("X"), new PropertyPath("Y"), null, CultureInfo.InvariantCulture, xLabels, yLabels);

            Assert.AreEqual(4, xLabels.Count);
            Assert.AreEqual(4, yLabels.Count);
            Assert.AreEqual(4, chartDataSource.Count);
            Assert.AreEqual(new DataPoint(new Point(1, 3), dataSource[0]), chartDataSource[0]);
            Assert.AreEqual(new DataPoint(new Point(2, 1), dataSource[1]), chartDataSource[1]);
            Assert.AreEqual(new DataPoint(new Point(3, 4), dataSource[2]), chartDataSource[2]);
            Assert.AreEqual(new DataPoint(new Point(4, 3), dataSource[3]), chartDataSource[3]);
        }
    }
}
