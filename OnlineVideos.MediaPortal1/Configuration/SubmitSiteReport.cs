﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OnlineVideos.MediaPortal1
{
    public partial class SubmitSiteReport : Form
    {
        public string SiteName { get; set; }

        public SubmitSiteReport()
        {
            InitializeComponent();

            cbType.Items.AddRange(Enum.GetNames(typeof(WebService.ReportType)));
            cbType.SelectedItem = WebService.ReportType.Broken.ToString();
        }

        private void SubmitSiteReport_Load(object sender, EventArgs e)
        {
            Text = "Submit a report for site: " + SiteName;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (tbxMessage.Text.Trim().Length < 10)
            {
                MessageBox.Show("You must enter a short text! No Report submitted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                WebService.ReportType type = (WebService.ReportType)Enum.Parse(typeof(WebService.ReportType), cbType.SelectedItem.ToString());
                WebService.OnlineVideosService ws = new OnlineVideos.WebService.OnlineVideosService();
                string msg = "";
                bool success = ws.SubmitReport(SiteName, tbxMessage.Text, type, out msg);
                MessageBox.Show(msg, success ? "Success" : "Error", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LinkForumClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://forum.team-mediaportal.com/forums/onlinevideos.244/");
        }

        private void LinkReportsClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            WebService.OnlineVideosService ws = new OnlineVideos.WebService.OnlineVideosService();
            System.Diagnostics.Process.Start(
                string.Format(ws.SiteReportsUrl(SiteName)));
        }
    }
}
