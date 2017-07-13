using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SpreadsheetGUI
{
    /// <summary>
    /// Form contains a chart component.
    /// </summary>
    public partial class ChartForm : Form
    {
        /// <summary>
        /// Creates a new form.
        /// </summary>
        public ChartForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the series for the chart.
        /// </summary>
        /// <param name="data">list of datapoints to be added to the series.</param>
        public void SetChartSeries(List<DataPoint> data)
        {
            foreach(DataPoint dp in data)
            {
                chart1.Series["Series1"].Points.Add(dp);
            }
        }
    }
}
