using System;
using System.Windows.Forms;


namespace ChicagoCrime
{
  public partial class Form1 : Form
  {
     public Form1()
    {
      InitializeComponent();
    }

    private bool doesFileExist(string filename)
    {
      if (!System.IO.File.Exists(filename))
      {
        string msg = string.Format("Input file not found: '{0}'",
          filename);

        MessageBox.Show(msg);
        return false;
      }

      // exists!
      return true;
    }

    private void clearForm()
    {
      // 
      // if a chart is being displayed currently, remove it:
      //
      if (this.graphPanel.Controls.Count > 0)
      {
        this.graphPanel.Controls.RemoveAt(0);
        this.graphPanel.Refresh();
      }
    }

    private void cmdByYear_Click(object sender, EventArgs e)
    {
      string filename = this.txtFilename.Text;

      if (!doesFileExist(filename))
        return;

      clearForm();

      //
      // Call over to F# code to analyze data and return a 
      // chart to display:
      //
      this.Cursor = Cursors.WaitCursor;

      var chart = FSAnalysis.CrimesByYear(filename);

      this.Cursor = Cursors.Default;

      //
      // we have chart, display it:
      //
      this.graphPanel.Controls.Add(chart);
      this.graphPanel.Refresh();
    }

    //arrest %
    private void button1_Click(object sender, EventArgs e)
    {
        string filename = this.txtFilename.Text;
        if (!doesFileExist(filename))
            return;
        clearForm();
            
        // Call over to F# code to analyze data and return a chart to display:
        this.Cursor = Cursors.WaitCursor;
        var chart = FSAnalysis.CrimesVsArrests(filename);
        this.Cursor = Cursors.Default;

        // we have chart, display it:
        this.graphPanel.Controls.Add(chart);
        this.graphPanel.Refresh();
    }

    //by crime
    private void button2_Click(object sender, EventArgs e)
    {
            string filename = this.txtFilename.Text;
            string filename2 = "IUCR-codes.csv";
            string iucr = this.textBox1.Text;

            if (!doesFileExist(filename))
                return;

            if (!doesFileExist(filename2))
                return;
            clearForm();

            // Call over to F# code to analyze data and return a chart to display:
            this.Cursor = Cursors.WaitCursor;
            var chart = FSAnalysis.GivenCrimeByYear(filename, filename2, iucr);
            this.Cursor = Cursors.Default;

            // we have chart, display it:
            this.graphPanel.Controls.Add(chart);
            this.graphPanel.Refresh();
        }

    
    private void textBox1_TextChanged(object sender, EventArgs e)
    {
            
    }

        private void txtFilename_TextChanged(object sender, EventArgs e)
        {

        }


    //by area
    private void button3_Click(object sender, EventArgs e)
    {
            string filename = this.txtFilename.Text;
            string filename2 = "Areas.csv";
            string community = this.textBox2.Text;

            if (!doesFileExist(filename))
                return;

            if (!doesFileExist(filename2))
                return;
            clearForm();

            // Call over to F# code to analyze data and return a chart to display:
            this.Cursor = Cursors.WaitCursor;
            var chart = FSAnalysis.CrimesByArea(filename, filename2, community);
            this.Cursor = Cursors.Default;

            // we have chart, display it:
            this.graphPanel.Controls.Add(chart);
            this.graphPanel.Refresh();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string filename = this.txtFilename.Text;
            string filename2 = "Areas.csv";

            if (!doesFileExist(filename))
                return;

            if (!doesFileExist(filename2))
                return;
            clearForm();

            // Call over to F# code to analyze data and return a chart to display:
            this.Cursor = Cursors.WaitCursor;
            var chart = FSAnalysis.CrimesChicago(filename, filename2);
            this.Cursor = Cursors.Default;

            // we have chart, display it:
            this.graphPanel.Controls.Add(chart);
            this.graphPanel.Refresh();
        }
    }//class
}//namespace
