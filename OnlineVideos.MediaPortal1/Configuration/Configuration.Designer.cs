﻿namespace OnlineVideos.MediaPortal1
{
	partial class Configuration
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Panel siteNameIconPanel;
            BrightIdeasSoftware.OLVColumn siteColumnName;
            BrightIdeasSoftware.OLVColumn siteColumnDescription;
            BrightIdeasSoftware.OLVColumn siteColumnAdult;
            BrightIdeasSoftware.OLVColumn siteColumnUpdated;
            BrightIdeasSoftware.OLVColumn siteColumnPlayer;
            BrightIdeasSoftware.OLVColumn sitevColumnUtil;
            BrightIdeasSoftware.OLVColumn siteColumnEnabled;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Configuration));
            this.iconSite = new System.Windows.Forms.PictureBox();
            this.lblSelectedSite = new System.Windows.Forms.Label();
            this.siteList = new BrightIdeasSoftware.DataListView();
            this.siteColumnLanguage = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.bindingSourceSiteSettings = new System.Windows.Forms.BindingSource(this.components);
            this.imageListSiteIcons = new System.Windows.Forms.ImageList(this.components);
            this.txtDownloadDir = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.propertyGridUserConfig = new System.Windows.Forms.PropertyGrid();
            this.txtFilters = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtThumbLoc = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.label5 = new System.Windows.Forms.Label();
            this.chkUseAgeConfirmation = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.tbxPin = new System.Windows.Forms.TextBox();
            this.tbxScreenName = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnBrowseForDlFolder = new System.Windows.Forms.Button();
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.chkStoreLayoutPerCategory = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.pictureBox6 = new System.Windows.Forms.PictureBox();
            this.groupBoxLatestVideos = new System.Windows.Forms.GroupBox();
            this.chkLatestVideosRandomize = new System.Windows.Forms.CheckBox();
            this.label53 = new System.Windows.Forms.Label();
            this.label52 = new System.Windows.Forms.Label();
            this.tbxLatestVideosGuiRefresh = new System.Windows.Forms.TextBox();
            this.tbxLatestVideosOnlineRefresh = new System.Windows.Forms.TextBox();
            this.tbxLatestVideosAmount = new System.Windows.Forms.TextBox();
            this.label51 = new System.Windows.Forms.Label();
            this.label50 = new System.Windows.Forms.Label();
            this.label49 = new System.Windows.Forms.Label();
            this.tbxUpdatePeriod = new System.Windows.Forms.TextBox();
            this.label45 = new System.Windows.Forms.Label();
            this.label44 = new System.Windows.Forms.Label();
            this.btnWiki = new System.Windows.Forms.Button();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label21 = new System.Windows.Forms.Label();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label34 = new System.Windows.Forms.Label();
            this.rbOff = new System.Windows.Forms.RadioButton();
            this.rbLastSearch = new System.Windows.Forms.RadioButton();
            this.rbExtendedSearchHistory = new System.Windows.Forms.RadioButton();
            this.label38 = new System.Windows.Forms.Label();
            this.nUPSearchHistoryItemCount = new System.Windows.Forms.NumericUpDown();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label33 = new System.Windows.Forms.Label();
            this.chkUseQuickSelect = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label48 = new System.Windows.Forms.Label();
            this.label47 = new System.Windows.Forms.Label();
            this.tbxCategoriesTimeout = new System.Windows.Forms.TextBox();
            this.label32 = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tbxWebCacheTimeout = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.tbxUtilTimeout = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkUseMPUrlSourceSplitter = new System.Windows.Forms.CheckBox();
            this.chkAdaptRefreshRate = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.udPlayBuffer = new System.Windows.Forms.DomainUpDown();
            this.label16 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.tbxWMPBuffer = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.chkDoAutoUpdate = new System.Windows.Forms.CheckBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblVersion = new System.Windows.Forms.Label();
            this.Thumbnails = new System.Windows.Forms.GroupBox();
            this.label36 = new System.Windows.Forms.Label();
            this.tbxThumbAge = new System.Windows.Forms.TextBox();
            this.label35 = new System.Windows.Forms.Label();
            this.bntBrowseFolderForThumbs = new System.Windows.Forms.Button();
            this.tabGroups = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.chkFavFirst = new System.Windows.Forms.CheckBox();
            this.chkAutoGroupByLang = new System.Windows.Forms.CheckBox();
            this.btnBrowseSitesGroupThumb = new System.Windows.Forms.Button();
            this.tbxSitesGroupDesc = new System.Windows.Forms.TextBox();
            this.bindingSourceSitesGroup = new System.Windows.Forms.BindingSource(this.components);
            this.label43 = new System.Windows.Forms.Label();
            this.tbxSitesGroupThumb = new System.Windows.Forms.TextBox();
            this.label42 = new System.Windows.Forms.Label();
            this.tbxSitesGroupName = new System.Windows.Forms.TextBox();
            this.label39 = new System.Windows.Forms.Label();
            this.toolStripContainer4 = new System.Windows.Forms.ToolStripContainer();
            this.listBoxSitesGroups = new System.Windows.Forms.ListBox();
            this.toolStripSitesGroupLeft = new System.Windows.Forms.ToolStrip();
            this.btnSitesGroupUp = new System.Windows.Forms.ToolStripButton();
            this.btnSitesGroupDown = new System.Windows.Forms.ToolStripButton();
            this.toolStripSitesGroupTop = new System.Windows.Forms.ToolStrip();
            this.btnAddSitesGroup = new System.Windows.Forms.ToolStripButton();
            this.btnDeleteSitesGroup = new System.Windows.Forms.ToolStripButton();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.listViewSitesNotInGroup = new System.Windows.Forms.ListView();
            this.label40 = new System.Windows.Forms.Label();
            this.listViewSitesInGroup = new System.Windows.Forms.ListView();
            this.label41 = new System.Windows.Forms.Label();
            this.tabSites = new System.Windows.Forms.TabPage();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.panel1 = new System.Windows.Forms.Panel();
            this.toolStripSites = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownBtnImport = new System.Windows.Forms.ToolStripDropDownButton();
            this.btnImportXml = new System.Windows.Forms.ToolStripMenuItem();
            this.btnImportGlobal = new System.Windows.Forms.ToolStripMenuItem();
            this.btnAddSite = new System.Windows.Forms.ToolStripButton();
            this.btnSiteUp = new System.Windows.Forms.ToolStripButton();
            this.btnSiteDown = new System.Windows.Forms.ToolStripButton();
            this.btnDeleteSite = new System.Windows.Forms.ToolStripButton();
            this.btnReportSite = new System.Windows.Forms.ToolStripButton();
            this.btnEditSite = new System.Windows.Forms.ToolStripButton();
            this.btnPublishSite = new System.Windows.Forms.ToolStripButton();
            this.btnCreateSite = new System.Windows.Forms.ToolStripButton();
            this.tabHosters = new System.Windows.Forms.TabPage();
            this.propertyGridHoster = new System.Windows.Forms.PropertyGrid();
            this.panel2 = new System.Windows.Forms.Panel();
            this.sourceLabel = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.listBoxHosters = new System.Windows.Forms.ListBox();
            this.tabSourceFilter = new System.Windows.Forms.TabPage();
            this.tabProtocols = new System.Windows.Forms.TabControl();
            this.tabPageNotDetectedFilter = new System.Windows.Forms.TabPage();
            this.linkLabelFilterDownload = new System.Windows.Forms.LinkLabel();
            this.label10 = new System.Windows.Forms.Label();
            this.tabPageHttp = new System.Windows.Forms.TabPage();
            this.groupBoxHttpProxyServerAuthentication = new System.Windows.Forms.GroupBox();
            this.checkBoxHttpProxyServerAuthentication = new System.Windows.Forms.CheckBox();
            this.labelHttpProxyType = new System.Windows.Forms.Label();
            this.comboBoxHttpProxyServerType = new System.Windows.Forms.ComboBox();
            this.labelHttpProxyServerPassword = new System.Windows.Forms.Label();
            this.labelHttpProxyServerUserName = new System.Windows.Forms.Label();
            this.labelHttpProxyServerPort = new System.Windows.Forms.Label();
            this.labelHttpProxyServer = new System.Windows.Forms.Label();
            this.textBoxHttpProxyServerPassword = new System.Windows.Forms.TextBox();
            this.textBoxHttpProxyServerUserName = new System.Windows.Forms.TextBox();
            this.textBoxHttpProxyServerPort = new System.Windows.Forms.TextBox();
            this.textBoxHttpProxyServer = new System.Windows.Forms.TextBox();
            this.groupBoxHttpServerAuthentication = new System.Windows.Forms.GroupBox();
            this.checkBoxHttpServerAuthentication = new System.Windows.Forms.CheckBox();
            this.labelHttpServerPassword = new System.Windows.Forms.Label();
            this.textBoxHttpServerPassword = new System.Windows.Forms.TextBox();
            this.textBoxHttpServerUserName = new System.Windows.Forms.TextBox();
            this.labelHttpServerUserName = new System.Windows.Forms.Label();
            this.groupBoxHttpCommonParameters = new System.Windows.Forms.GroupBox();
            this.label25 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.labelHttpTotalReopenConnectionTimeout = new System.Windows.Forms.Label();
            this.labelHttpOpenConnectionSleepTime = new System.Windows.Forms.Label();
            this.textBoxHttpTotalReopenConnectionTimeout = new System.Windows.Forms.TextBox();
            this.textBoxHttpOpenConnectionSleepTime = new System.Windows.Forms.TextBox();
            this.textBoxHttpOpenConnectionTimeout = new System.Windows.Forms.TextBox();
            this.labelHttpOpenConnectionTimeout = new System.Windows.Forms.Label();
            this.labelHttpNetworkInterface = new System.Windows.Forms.Label();
            this.comboBoxHttpPreferredNetworkInterface = new System.Windows.Forms.ComboBox();
            this.tabPageRtmp = new System.Windows.Forms.TabPage();
            this.groupBoxRtmpCommonParameters = new System.Windows.Forms.GroupBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.labelRtmpTotalReopenConnectionTimeout = new System.Windows.Forms.Label();
            this.labelRtmpOpenConnectionSleepTime = new System.Windows.Forms.Label();
            this.textBoxRtmpTotalReopenConnectionTimeout = new System.Windows.Forms.TextBox();
            this.textBoxRtmpOpenConnectionSleepTime = new System.Windows.Forms.TextBox();
            this.textBoxRtmpOpenConnectionTimeout = new System.Windows.Forms.TextBox();
            this.labelRtmpOpenConnectionTimeout = new System.Windows.Forms.Label();
            this.labelRtmpNetworkInterface = new System.Windows.Forms.Label();
            this.comboBoxRtmpPreferredNetworkInterface = new System.Windows.Forms.ComboBox();
            this.tabPageRtsp = new System.Windows.Forms.TabPage();
            this.groupBoxRtspCommonParameters = new System.Windows.Forms.GroupBox();
            this.label59 = new System.Windows.Forms.Label();
            this.textBoxRtspClientPortMax = new System.Windows.Forms.TextBox();
            this.labelRtspConnectionRange = new System.Windows.Forms.Label();
            this.textBoxRtspClientPortMin = new System.Windows.Forms.TextBox();
            this.label26 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.labelRtspTotalReopenConnectionTimeout = new System.Windows.Forms.Label();
            this.labelRtspOpenConnectionSleepTime = new System.Windows.Forms.Label();
            this.textBoxRtspTotalReopenConnectionTimeout = new System.Windows.Forms.TextBox();
            this.textBoxRtspOpenConnectionSleepTime = new System.Windows.Forms.TextBox();
            this.textBoxRtspOpenConnectionTimeout = new System.Windows.Forms.TextBox();
            this.labelRtspOpenConnectionTimeout = new System.Windows.Forms.Label();
            this.labelRtspNetworkInterface = new System.Windows.Forms.Label();
            this.comboBoxRtspPreferredNetworkInterface = new System.Windows.Forms.ComboBox();
            this.tabPageUdpRtp = new System.Windows.Forms.TabPage();
            this.groupBoxUdpRtpCommonParameters = new System.Windows.Forms.GroupBox();
            this.label56 = new System.Windows.Forms.Label();
            this.labelUdpRtpReceiveDataCheckInterval = new System.Windows.Forms.Label();
            this.textBoxUdpRtpReceiveDataCheckInterval = new System.Windows.Forms.TextBox();
            this.label37 = new System.Windows.Forms.Label();
            this.label54 = new System.Windows.Forms.Label();
            this.label55 = new System.Windows.Forms.Label();
            this.labelUdpRtpTotalReopenConnectionTimeout = new System.Windows.Forms.Label();
            this.labelUdpRtpOpenConnectionSleepTime = new System.Windows.Forms.Label();
            this.textBoxUdpRtpTotalReopenConnectionTimeout = new System.Windows.Forms.TextBox();
            this.textBoxUdpRtpOpenConnectionSleepTime = new System.Windows.Forms.TextBox();
            this.textBoxUdpRtpOpenConnectionTimeout = new System.Windows.Forms.TextBox();
            this.labelUdpRtpOpenConnectionTimeout = new System.Windows.Forms.Label();
            this.labelUdpRtpNetworkInterface = new System.Windows.Forms.Label();
            this.comboBoxUdpRtpPreferredNetworkInterface = new System.Windows.Forms.ComboBox();
            this.tabPageQuality = new System.Windows.Forms.TabPage();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.checkBoxAutoVideoSelection = new System.Windows.Forms.CheckBox();
            this.comboBoxVideoResolution = new System.Windows.Forms.ComboBox();
            this.checkBoxAllowHDR = new System.Windows.Forms.CheckBox();
            this.checkBoxAllow3D = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.preferredListControlAudioCodec = new OnlineVideos.MediaPortal1.PreferredListControl();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.preferredListControlVideoCodec = new OnlineVideos.MediaPortal1.PreferredListControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.preferredListControlContainer = new OnlineVideos.MediaPortal1.PreferredListControl();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            siteNameIconPanel = new System.Windows.Forms.Panel();
            siteColumnName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            siteColumnDescription = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            siteColumnAdult = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            siteColumnUpdated = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            siteColumnPlayer = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            sitevColumnUtil = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            siteColumnEnabled = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            siteNameIconPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconSite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.siteList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceSiteSettings)).BeginInit();
            this.mainTabControl.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).BeginInit();
            this.groupBoxLatestVideos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUPSearchHistoryItemCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.Thumbnails.SuspendLayout();
            this.tabGroups.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceSitesGroup)).BeginInit();
            this.toolStripContainer4.ContentPanel.SuspendLayout();
            this.toolStripContainer4.LeftToolStripPanel.SuspendLayout();
            this.toolStripContainer4.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer4.SuspendLayout();
            this.toolStripSitesGroupLeft.SuspendLayout();
            this.toolStripSitesGroupTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tabSites.SuspendLayout();
            this.panel1.SuspendLayout();
            this.toolStripSites.SuspendLayout();
            this.tabHosters.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabSourceFilter.SuspendLayout();
            this.tabProtocols.SuspendLayout();
            this.tabPageNotDetectedFilter.SuspendLayout();
            this.tabPageHttp.SuspendLayout();
            this.groupBoxHttpProxyServerAuthentication.SuspendLayout();
            this.groupBoxHttpServerAuthentication.SuspendLayout();
            this.groupBoxHttpCommonParameters.SuspendLayout();
            this.tabPageRtmp.SuspendLayout();
            this.groupBoxRtmpCommonParameters.SuspendLayout();
            this.tabPageRtsp.SuspendLayout();
            this.groupBoxRtspCommonParameters.SuspendLayout();
            this.tabPageUdpRtp.SuspendLayout();
            this.groupBoxUdpRtpCommonParameters.SuspendLayout();
            this.tabPageQuality.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // siteNameIconPanel
            // 
            siteNameIconPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            siteNameIconPanel.Controls.Add(this.iconSite);
            siteNameIconPanel.Controls.Add(this.lblSelectedSite);
            siteNameIconPanel.Dock = System.Windows.Forms.DockStyle.Left;
            siteNameIconPanel.Location = new System.Drawing.Point(0, 0);
            siteNameIconPanel.Name = "siteNameIconPanel";
            siteNameIconPanel.Size = new System.Drawing.Size(64, 134);
            siteNameIconPanel.TabIndex = 32;
            // 
            // iconSite
            // 
            this.iconSite.Location = new System.Drawing.Point(1, 72);
            this.iconSite.Name = "iconSite";
            this.iconSite.Size = new System.Drawing.Size(60, 60);
            this.iconSite.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.iconSite.TabIndex = 30;
            this.iconSite.TabStop = false;
            // 
            // lblSelectedSite
            // 
            this.lblSelectedSite.AutoEllipsis = true;
            this.lblSelectedSite.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSelectedSite.Location = new System.Drawing.Point(1, 5);
            this.lblSelectedSite.Name = "lblSelectedSite";
            this.lblSelectedSite.Size = new System.Drawing.Size(60, 60);
            this.lblSelectedSite.TabIndex = 31;
            this.lblSelectedSite.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // siteColumnName
            // 
            siteColumnName.AspectName = "Name";
            siteColumnName.GroupWithItemCountFormat = "";
            siteColumnName.GroupWithItemCountSingularFormat = "";
            siteColumnName.Hideable = false;
            siteColumnName.ImageAspectName = "Name";
            siteColumnName.IsEditable = false;
            siteColumnName.MinimumWidth = 130;
            siteColumnName.Text = "Name";
            siteColumnName.UseInitialLetterForGroup = true;
            siteColumnName.Width = 130;
            siteColumnName.WordWrap = true;
            // 
            // siteColumnDescription
            // 
            siteColumnDescription.AspectName = "Description";
            siteColumnDescription.FillsFreeSpace = true;
            siteColumnDescription.Groupable = false;
            siteColumnDescription.IsEditable = false;
            siteColumnDescription.Sortable = false;
            siteColumnDescription.Text = "Description";
            siteColumnDescription.UseFiltering = false;
            siteColumnDescription.Width = 100;
            // 
            // siteColumnAdult
            // 
            siteColumnAdult.AspectName = "ConfirmAge";
            siteColumnAdult.CheckBoxes = true;
            siteColumnAdult.HeaderTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            siteColumnAdult.IsEditable = false;
            siteColumnAdult.Text = "Adult";
            siteColumnAdult.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            siteColumnAdult.Width = 40;
            // 
            // siteColumnUpdated
            // 
            siteColumnUpdated.AspectName = "LastUpdated";
            siteColumnUpdated.AspectToStringFormat = "{0:g}";
            siteColumnUpdated.DisplayIndex = 4;
            siteColumnUpdated.IsEditable = false;
            siteColumnUpdated.IsVisible = false;
            siteColumnUpdated.Text = "Updated";
            siteColumnUpdated.Width = 70;
            siteColumnUpdated.WordWrap = true;
            // 
            // siteColumnPlayer
            // 
            siteColumnPlayer.AspectName = "Player";
            siteColumnPlayer.DisplayIndex = 6;
            siteColumnPlayer.IsEditable = false;
            siteColumnPlayer.IsVisible = false;
            siteColumnPlayer.Text = "Player";
            siteColumnPlayer.Width = 50;
            // 
            // sitevColumnUtil
            // 
            sitevColumnUtil.AspectName = "UtilName";
            sitevColumnUtil.DisplayIndex = 5;
            sitevColumnUtil.IsEditable = false;
            sitevColumnUtil.IsVisible = false;
            sitevColumnUtil.Text = "C# Util";
            sitevColumnUtil.Width = 70;
            sitevColumnUtil.WordWrap = true;
            // 
            // siteColumnEnabled
            // 
            siteColumnEnabled.AspectName = "IsEnabled";
            siteColumnEnabled.CheckBoxes = true;
            siteColumnEnabled.DisplayIndex = 7;
            siteColumnEnabled.HeaderTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            siteColumnEnabled.IsEditable = false;
            siteColumnEnabled.IsVisible = false;
            siteColumnEnabled.Text = "Active";
            siteColumnEnabled.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            siteColumnEnabled.Width = 50;
            // 
            // siteList
            // 
            this.siteList.AllColumns.Add(siteColumnName);
            this.siteList.AllColumns.Add(this.siteColumnLanguage);
            this.siteList.AllColumns.Add(siteColumnAdult);
            this.siteList.AllColumns.Add(siteColumnDescription);
            this.siteList.AllColumns.Add(siteColumnUpdated);
            this.siteList.AllColumns.Add(sitevColumnUtil);
            this.siteList.AllColumns.Add(siteColumnPlayer);
            this.siteList.AllColumns.Add(siteColumnEnabled);
            this.siteList.AllowColumnReorder = true;
            this.siteList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            siteColumnName,
            this.siteColumnLanguage,
            siteColumnAdult,
            siteColumnDescription});
            this.siteList.DataSource = this.bindingSourceSiteSettings;
            this.siteList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.siteList.FullRowSelect = true;
            this.siteList.HideSelection = false;
            this.siteList.LargeImageList = this.imageListSiteIcons;
            this.siteList.Location = new System.Drawing.Point(3, 28);
            this.siteList.Name = "siteList";
            this.siteList.OwnerDraw = true;
            this.siteList.OwnerDrawnHeader = true;
            this.siteList.RenderNonEditableCheckboxesAsDisabled = true;
            this.siteList.ShowCommandMenuOnRightClick = true;
            this.siteList.ShowGroups = false;
            this.siteList.ShowItemCountOnGroups = true;
            this.siteList.Size = new System.Drawing.Size(698, 372);
            this.siteList.SmallImageList = this.imageListSiteIcons;
            this.siteList.SortGroupItemsByPrimaryColumn = false;
            this.siteList.TabIndex = 0;
            this.siteList.UseCompatibleStateImageBehavior = false;
            this.siteList.UseFiltering = true;
            this.siteList.View = System.Windows.Forms.View.Details;
            this.siteList.SelectionChanged += new System.EventHandler(this.siteList_SelectionChanged);
            // 
            // siteColumnLanguage
            // 
            this.siteColumnLanguage.AspectName = "Language";
            this.siteColumnLanguage.IsEditable = false;
            this.siteColumnLanguage.MinimumWidth = 80;
            this.siteColumnLanguage.Text = "Language";
            this.siteColumnLanguage.Width = 80;
            // 
            // bindingSourceSiteSettings
            // 
            this.bindingSourceSiteSettings.DataSource = typeof(OnlineVideos.SiteSettings);
            // 
            // imageListSiteIcons
            // 
            this.imageListSiteIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageListSiteIcons.ImageSize = new System.Drawing.Size(28, 28);
            this.imageListSiteIcons.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // txtDownloadDir
            // 
            this.txtDownloadDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDownloadDir.Location = new System.Drawing.Point(225, 35);
            this.txtDownloadDir.Name = "txtDownloadDir";
            this.txtDownloadDir.Size = new System.Drawing.Size(429, 20);
            this.txtDownloadDir.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Download Folder";
            // 
            // propertyGridUserConfig
            // 
            this.propertyGridUserConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridUserConfig.Location = new System.Drawing.Point(64, 0);
            this.propertyGridUserConfig.Name = "propertyGridUserConfig";
            this.propertyGridUserConfig.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.propertyGridUserConfig.Size = new System.Drawing.Size(634, 134);
            this.propertyGridUserConfig.TabIndex = 29;
            this.propertyGridUserConfig.ToolbarVisible = false;
            // 
            // txtFilters
            // 
            this.txtFilters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFilters.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFilters.Location = new System.Drawing.Point(225, 50);
            this.txtFilters.Name = "txtFilters";
            this.txtFilters.Size = new System.Drawing.Size(460, 20);
            this.txtFilters.TabIndex = 9;
            this.toolTip1.SetToolTip(this.txtFilters, "Comma seperated list of words that will be used to filter out videos.");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(6, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(155, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Filter out videos with these tags";
            // 
            // txtThumbLoc
            // 
            this.txtThumbLoc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtThumbLoc.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtThumbLoc.Location = new System.Drawing.Point(285, 23);
            this.txtThumbLoc.Name = "txtThumbLoc";
            this.txtThumbLoc.Size = new System.Drawing.Size(369, 20);
            this.txtThumbLoc.TabIndex = 20;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(222, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Location";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(6, 25);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(109, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Use Age Confirmation";
            // 
            // chkUseAgeConfirmation
            // 
            this.chkUseAgeConfirmation.AutoSize = true;
            this.chkUseAgeConfirmation.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkUseAgeConfirmation.Location = new System.Drawing.Point(225, 25);
            this.chkUseAgeConfirmation.Name = "chkUseAgeConfirmation";
            this.chkUseAgeConfirmation.Size = new System.Drawing.Size(15, 14);
            this.chkUseAgeConfirmation.TabIndex = 7;
            this.chkUseAgeConfirmation.UseVisualStyleBackColor = true;
            this.chkUseAgeConfirmation.CheckedChanged += new System.EventHandler(this.chkUseAgeConfirmation_CheckedChanged);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSave.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.Save;
            this.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSave.Location = new System.Drawing.Point(499, 512);
            this.btnSave.Name = "btnSave";
            this.btnSave.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.btnSave.Size = new System.Drawing.Size(90, 25);
            this.btnSave.TabIndex = 26;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            // 
            // tbxPin
            // 
            this.tbxPin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxPin.Enabled = false;
            this.tbxPin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbxPin.Location = new System.Drawing.Point(276, 22);
            this.tbxPin.Margin = new System.Windows.Forms.Padding(2);
            this.tbxPin.Name = "tbxPin";
            this.tbxPin.PasswordChar = '*';
            this.tbxPin.Size = new System.Drawing.Size(409, 20);
            this.tbxPin.TabIndex = 8;
            // 
            // tbxScreenName
            // 
            this.tbxScreenName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxScreenName.Location = new System.Drawing.Point(225, 9);
            this.tbxScreenName.Name = "tbxScreenName";
            this.tbxScreenName.Size = new System.Drawing.Size(460, 20);
            this.tbxScreenName.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 11);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(129, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "BasicHome Screen Name";
            // 
            // btnBrowseForDlFolder
            // 
            this.btnBrowseForDlFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseForDlFolder.AutoSize = true;
            this.btnBrowseForDlFolder.Location = new System.Drawing.Point(655, 34);
            this.btnBrowseForDlFolder.Margin = new System.Windows.Forms.Padding(2);
            this.btnBrowseForDlFolder.Name = "btnBrowseForDlFolder";
            this.btnBrowseForDlFolder.Size = new System.Drawing.Size(30, 23);
            this.btnBrowseForDlFolder.TabIndex = 3;
            this.btnBrowseForDlFolder.Text = "...";
            this.btnBrowseForDlFolder.UseVisualStyleBackColor = true;
            this.btnBrowseForDlFolder.Click += new System.EventHandler(this.btnBrowseForDlFolder_Click);
            // 
            // mainTabControl
            // 
            this.mainTabControl.Controls.Add(this.tabGeneral);
            this.mainTabControl.Controls.Add(this.tabGroups);
            this.mainTabControl.Controls.Add(this.tabSites);
            this.mainTabControl.Controls.Add(this.tabHosters);
            this.mainTabControl.Controls.Add(this.tabSourceFilter);
            this.mainTabControl.Controls.Add(this.tabPageQuality);
            this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabControl.Location = new System.Drawing.Point(0, 0);
            this.mainTabControl.Margin = new System.Windows.Forms.Padding(2);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.Size = new System.Drawing.Size(712, 568);
            this.mainTabControl.TabIndex = 0;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.chkStoreLayoutPerCategory);
            this.tabGeneral.Controls.Add(this.label8);
            this.tabGeneral.Controls.Add(this.pictureBox6);
            this.tabGeneral.Controls.Add(this.groupBoxLatestVideos);
            this.tabGeneral.Controls.Add(this.tbxUpdatePeriod);
            this.tabGeneral.Controls.Add(this.label45);
            this.tabGeneral.Controls.Add(this.label44);
            this.tabGeneral.Controls.Add(this.btnWiki);
            this.tabGeneral.Controls.Add(this.pictureBox5);
            this.tabGeneral.Controls.Add(this.groupBox5);
            this.tabGeneral.Controls.Add(this.pictureBox4);
            this.tabGeneral.Controls.Add(this.groupBox4);
            this.tabGeneral.Controls.Add(this.pictureBox3);
            this.tabGeneral.Controls.Add(this.pictureBox2);
            this.tabGeneral.Controls.Add(this.pictureBox1);
            this.tabGeneral.Controls.Add(this.label33);
            this.tabGeneral.Controls.Add(this.chkUseQuickSelect);
            this.tabGeneral.Controls.Add(this.groupBox3);
            this.tabGeneral.Controls.Add(this.groupBox2);
            this.tabGeneral.Controls.Add(this.label23);
            this.tabGeneral.Controls.Add(this.chkDoAutoUpdate);
            this.tabGeneral.Controls.Add(this.btnCancel);
            this.tabGeneral.Controls.Add(this.lblVersion);
            this.tabGeneral.Controls.Add(this.btnBrowseForDlFolder);
            this.tabGeneral.Controls.Add(this.btnSave);
            this.tabGeneral.Controls.Add(this.tbxScreenName);
            this.tabGeneral.Controls.Add(this.label7);
            this.tabGeneral.Controls.Add(this.label3);
            this.tabGeneral.Controls.Add(this.txtDownloadDir);
            this.tabGeneral.Controls.Add(this.Thumbnails);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Margin = new System.Windows.Forms.Padding(2);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(2);
            this.tabGeneral.Size = new System.Drawing.Size(704, 542);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // chkStoreLayoutPerCategory
            // 
            this.chkStoreLayoutPerCategory.AutoSize = true;
            this.chkStoreLayoutPerCategory.Location = new System.Drawing.Point(335, 81);
            this.chkStoreLayoutPerCategory.Name = "chkStoreLayoutPerCategory";
            this.chkStoreLayoutPerCategory.Size = new System.Drawing.Size(15, 14);
            this.chkStoreLayoutPerCategory.TabIndex = 74;
            this.toolTip1.SetToolTip(this.chkStoreLayoutPerCategory, "If enabled, your chosen layout (list, small icons, large icons) will be remembere" +
        "d per category and site.");
            this.chkStoreLayoutPerCategory.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(355, 81);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(130, 13);
            this.label8.TabIndex = 73;
            this.label8.Text = "Store Layout per Category";
            // 
            // pictureBox6
            // 
            this.pictureBox6.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.Latest;
            this.pictureBox6.Location = new System.Drawing.Point(5, 442);
            this.pictureBox6.Name = "pictureBox6";
            this.pictureBox6.Size = new System.Drawing.Size(24, 24);
            this.pictureBox6.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox6.TabIndex = 72;
            this.pictureBox6.TabStop = false;
            // 
            // groupBoxLatestVideos
            // 
            this.groupBoxLatestVideos.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLatestVideos.Controls.Add(this.chkLatestVideosRandomize);
            this.groupBoxLatestVideos.Controls.Add(this.label53);
            this.groupBoxLatestVideos.Controls.Add(this.label52);
            this.groupBoxLatestVideos.Controls.Add(this.tbxLatestVideosGuiRefresh);
            this.groupBoxLatestVideos.Controls.Add(this.tbxLatestVideosOnlineRefresh);
            this.groupBoxLatestVideos.Controls.Add(this.tbxLatestVideosAmount);
            this.groupBoxLatestVideos.Controls.Add(this.label51);
            this.groupBoxLatestVideos.Controls.Add(this.label50);
            this.groupBoxLatestVideos.Controls.Add(this.label49);
            this.groupBoxLatestVideos.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxLatestVideos.Location = new System.Drawing.Point(0, 447);
            this.groupBoxLatestVideos.Name = "groupBoxLatestVideos";
            this.groupBoxLatestVideos.Size = new System.Drawing.Size(699, 56);
            this.groupBoxLatestVideos.TabIndex = 71;
            this.groupBoxLatestVideos.TabStop = false;
            this.groupBoxLatestVideos.Text = "      Latest Videos";
            // 
            // chkLatestVideosRandomize
            // 
            this.chkLatestVideosRandomize.AutoSize = true;
            this.chkLatestVideosRandomize.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkLatestVideosRandomize.Location = new System.Drawing.Point(200, 24);
            this.chkLatestVideosRandomize.Name = "chkLatestVideosRandomize";
            this.chkLatestVideosRandomize.Size = new System.Drawing.Size(79, 17);
            this.chkLatestVideosRandomize.TabIndex = 49;
            this.chkLatestVideosRandomize.Text = "Randomize";
            this.toolTip1.SetToolTip(this.chkLatestVideosRandomize, "After retrieving all latest videos from all sites randomize them before displayin" +
        "g.");
            this.chkLatestVideosRandomize.UseVisualStyleBackColor = true;
            // 
            // label53
            // 
            this.label53.AutoSize = true;
            this.label53.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label53.Location = new System.Drawing.Point(638, 25);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(24, 13);
            this.label53.TabIndex = 48;
            this.label53.Text = "sec";
            // 
            // label52
            // 
            this.label52.AutoSize = true;
            this.label52.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label52.Location = new System.Drawing.Point(459, 25);
            this.label52.Name = "label52";
            this.label52.Size = new System.Drawing.Size(23, 13);
            this.label52.TabIndex = 48;
            this.label52.Text = "min";
            // 
            // tbxLatestVideosGuiRefresh
            // 
            this.tbxLatestVideosGuiRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbxLatestVideosGuiRefresh.Location = new System.Drawing.Point(574, 22);
            this.tbxLatestVideosGuiRefresh.Name = "tbxLatestVideosGuiRefresh";
            this.tbxLatestVideosGuiRefresh.Size = new System.Drawing.Size(43, 20);
            this.tbxLatestVideosGuiRefresh.TabIndex = 24;
            this.tbxLatestVideosGuiRefresh.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.tbxLatestVideosGuiRefresh, "Seconds after which latest video items rotata to show all items when more are ava" +
        "ilable than shown concurrently.");
            this.tbxLatestVideosGuiRefresh.Validating += new System.ComponentModel.CancelEventHandler(this.CheckValidNumber);
            // 
            // tbxLatestVideosOnlineRefresh
            // 
            this.tbxLatestVideosOnlineRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbxLatestVideosOnlineRefresh.Location = new System.Drawing.Point(387, 22);
            this.tbxLatestVideosOnlineRefresh.Name = "tbxLatestVideosOnlineRefresh";
            this.tbxLatestVideosOnlineRefresh.Size = new System.Drawing.Size(53, 20);
            this.tbxLatestVideosOnlineRefresh.TabIndex = 23;
            this.tbxLatestVideosOnlineRefresh.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.tbxLatestVideosOnlineRefresh, "Minutes after which all configured sites will be asked for new latest videos to d" +
        "isplay.");
            this.tbxLatestVideosOnlineRefresh.Validating += new System.ComponentModel.CancelEventHandler(this.CheckValidNumber);
            // 
            // tbxLatestVideosAmount
            // 
            this.tbxLatestVideosAmount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbxLatestVideosAmount.Location = new System.Drawing.Point(108, 22);
            this.tbxLatestVideosAmount.Name = "tbxLatestVideosAmount";
            this.tbxLatestVideosAmount.Size = new System.Drawing.Size(53, 20);
            this.tbxLatestVideosAmount.TabIndex = 22;
            this.tbxLatestVideosAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.tbxLatestVideosAmount, "Number of latest video items to set data concurrently. Default is 3. Set to 0 to " +
        "disable this feature.");
            this.tbxLatestVideosAmount.Validating += new System.ComponentModel.CancelEventHandler(this.CheckValidNumber);
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label51.Location = new System.Drawing.Point(502, 25);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(66, 13);
            this.label51.TabIndex = 2;
            this.label51.Text = "Refresh GUI";
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label50.Location = new System.Drawing.Point(311, 25);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(70, 13);
            this.label50.TabIndex = 1;
            this.label50.Text = "Refresh Data";
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label49.Location = new System.Drawing.Point(6, 25);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(80, 13);
            this.label49.TabIndex = 0;
            this.label49.Text = "Display Amount";
            // 
            // tbxUpdatePeriod
            // 
            this.tbxUpdatePeriod.Location = new System.Drawing.Point(309, 58);
            this.tbxUpdatePeriod.Name = "tbxUpdatePeriod";
            this.tbxUpdatePeriod.Size = new System.Drawing.Size(40, 20);
            this.tbxUpdatePeriod.TabIndex = 5;
            this.toolTip1.SetToolTip(this.tbxUpdatePeriod, "Automatic update and thumbnail deletion on startup will only run after this many " +
        "hours passed since the last run.");
            this.tbxUpdatePeriod.Validating += new System.ComponentModel.CancelEventHandler(this.CheckValidNumber);
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Location = new System.Drawing.Point(355, 61);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(33, 13);
            this.label45.TabIndex = 69;
            this.label45.Text = "hours";
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Location = new System.Drawing.Point(246, 61);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(57, 13);
            this.label44.TabIndex = 68;
            this.label44.Text = "Only every";
            // 
            // btnWiki
            // 
            this.btnWiki.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnWiki.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.help;
            this.btnWiki.Location = new System.Drawing.Point(4, 516);
            this.btnWiki.Name = "btnWiki";
            this.btnWiki.Size = new System.Drawing.Size(23, 23);
            this.btnWiki.TabIndex = 25;
            this.toolTip1.SetToolTip(this.btnWiki, "Open the OnlineVideos Wiki.");
            this.btnWiki.UseVisualStyleBackColor = true;
            this.btnWiki.Click += new System.EventHandler(this.btnWiki_Click);
            // 
            // pictureBox5
            // 
            this.pictureBox5.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.key;
            this.pictureBox5.Location = new System.Drawing.Point(4, 100);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(24, 24);
            this.pictureBox5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox5.TabIndex = 66;
            this.pictureBox5.TabStop = false;
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.BackColor = System.Drawing.Color.Transparent;
            this.groupBox5.Controls.Add(this.label5);
            this.groupBox5.Controls.Add(this.chkUseAgeConfirmation);
            this.groupBox5.Controls.Add(this.label21);
            this.groupBox5.Controls.Add(this.tbxPin);
            this.groupBox5.Controls.Add(this.label2);
            this.groupBox5.Controls.Add(this.txtFilters);
            this.groupBox5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox5.Location = new System.Drawing.Point(0, 105);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(699, 80);
            this.groupBox5.TabIndex = 65;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "      Adult content";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label21.Location = new System.Drawing.Point(246, 25);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(22, 13);
            this.label21.TabIndex = 0;
            this.label21.Text = "Pin";
            // 
            // pictureBox4
            // 
            this.pictureBox4.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.search;
            this.pictureBox4.Location = new System.Drawing.Point(5, 187);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(24, 24);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox4.TabIndex = 64;
            this.pictureBox4.TabStop = false;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.label34);
            this.groupBox4.Controls.Add(this.rbOff);
            this.groupBox4.Controls.Add(this.rbLastSearch);
            this.groupBox4.Controls.Add(this.rbExtendedSearchHistory);
            this.groupBox4.Controls.Add(this.label38);
            this.groupBox4.Controls.Add(this.nUPSearchHistoryItemCount);
            this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox4.Location = new System.Drawing.Point(0, 192);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(699, 56);
            this.groupBox4.TabIndex = 63;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "      Search history";
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label34.Location = new System.Drawing.Point(6, 26);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(34, 13);
            this.label34.TabIndex = 0;
            this.label34.Text = "Mode";
            // 
            // rbOff
            // 
            this.rbOff.AutoSize = true;
            this.rbOff.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbOff.Location = new System.Drawing.Point(225, 24);
            this.rbOff.Name = "rbOff";
            this.rbOff.Size = new System.Drawing.Size(39, 17);
            this.rbOff.TabIndex = 10;
            this.rbOff.TabStop = true;
            this.rbOff.Text = "Off";
            this.rbOff.UseVisualStyleBackColor = true;
            this.rbOff.CheckedChanged += new System.EventHandler(this.searchType_CheckedChanged);
            // 
            // rbLastSearch
            // 
            this.rbLastSearch.AutoSize = true;
            this.rbLastSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbLastSearch.Location = new System.Drawing.Point(270, 24);
            this.rbLastSearch.Name = "rbLastSearch";
            this.rbLastSearch.Size = new System.Drawing.Size(80, 17);
            this.rbLastSearch.TabIndex = 11;
            this.rbLastSearch.TabStop = true;
            this.rbLastSearch.Text = "Last search";
            this.rbLastSearch.UseVisualStyleBackColor = true;
            this.rbLastSearch.CheckedChanged += new System.EventHandler(this.searchType_CheckedChanged);
            // 
            // rbExtendedSearchHistory
            // 
            this.rbExtendedSearchHistory.AutoSize = true;
            this.rbExtendedSearchHistory.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbExtendedSearchHistory.Location = new System.Drawing.Point(356, 24);
            this.rbExtendedSearchHistory.Name = "rbExtendedSearchHistory";
            this.rbExtendedSearchHistory.Size = new System.Drawing.Size(107, 17);
            this.rbExtendedSearchHistory.TabIndex = 12;
            this.rbExtendedSearchHistory.TabStop = true;
            this.rbExtendedSearchHistory.Text = "Extended (dialog)";
            this.rbExtendedSearchHistory.UseVisualStyleBackColor = true;
            this.rbExtendedSearchHistory.CheckedChanged += new System.EventHandler(this.searchType_CheckedChanged);
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label38.Location = new System.Drawing.Point(638, 26);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(32, 13);
            this.label38.TabIndex = 0;
            this.label38.Text = "Items";
            // 
            // nUPSearchHistoryItemCount
            // 
            this.nUPSearchHistoryItemCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nUPSearchHistoryItemCount.Location = new System.Drawing.Point(541, 21);
            this.nUPSearchHistoryItemCount.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.nUPSearchHistoryItemCount.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nUPSearchHistoryItemCount.Name = "nUPSearchHistoryItemCount";
            this.nUPSearchHistoryItemCount.Size = new System.Drawing.Size(76, 20);
            this.nUPSearchHistoryItemCount.TabIndex = 13;
            this.toolTip1.SetToolTip(this.nUPSearchHistoryItemCount, "Defines the number of search strings stored per Site.");
            this.nUPSearchHistoryItemCount.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.thumbnail;
            this.pictureBox3.Location = new System.Drawing.Point(5, 380);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(24, 24);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox3.TabIndex = 62;
            this.pictureBox3.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.cache;
            this.pictureBox2.Location = new System.Drawing.Point(5, 315);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(24, 24);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox2.TabIndex = 50;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.timeout;
            this.pictureBox1.Location = new System.Drawing.Point(5, 251);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(24, 24);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 45;
            this.pictureBox1.TabStop = false;
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(6, 81);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(90, 13);
            this.label33.TabIndex = 0;
            this.label33.Text = "Use Quick Select";
            // 
            // chkUseQuickSelect
            // 
            this.chkUseQuickSelect.AutoSize = true;
            this.chkUseQuickSelect.Location = new System.Drawing.Point(225, 81);
            this.chkUseQuickSelect.Name = "chkUseQuickSelect";
            this.chkUseQuickSelect.Size = new System.Drawing.Size(15, 14);
            this.chkUseQuickSelect.TabIndex = 6;
            this.toolTip1.SetToolTip(this.chkUseQuickSelect, "Allows you to quickly select entries that start with the letter or number you pre" +
        "ssed in the list.");
            this.chkUseQuickSelect.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.groupBox3.Controls.Add(this.label48);
            this.groupBox3.Controls.Add(this.label47);
            this.groupBox3.Controls.Add(this.tbxCategoriesTimeout);
            this.groupBox3.Controls.Add(this.label32);
            this.groupBox3.Controls.Add(this.label31);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.tbxWebCacheTimeout);
            this.groupBox3.Controls.Add(this.label15);
            this.groupBox3.Controls.Add(this.tbxUtilTimeout);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(0, 256);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(699, 56);
            this.groupBox3.TabIndex = 50;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "      Timeouts";
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label48.Location = new System.Drawing.Point(222, 26);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(57, 13);
            this.label48.TabIndex = 47;
            this.label48.Text = "Categories";
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label47.Location = new System.Drawing.Point(358, 26);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(23, 13);
            this.label47.TabIndex = 46;
            this.label47.Text = "min";
            // 
            // tbxCategoriesTimeout
            // 
            this.tbxCategoriesTimeout.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbxCategoriesTimeout.Location = new System.Drawing.Point(285, 23);
            this.tbxCategoriesTimeout.Name = "tbxCategoriesTimeout";
            this.tbxCategoriesTimeout.Size = new System.Drawing.Size(53, 20);
            this.tbxCategoriesTimeout.TabIndex = 15;
            this.tbxCategoriesTimeout.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.tbxCategoriesTimeout, "Minutes after which a sites dynamic categories are retrieved from the website aga" +
        "in. Default is 300 (5h).");
            this.tbxCategoriesTimeout.Validating += new System.ComponentModel.CancelEventHandler(this.CheckValidNumber);
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label32.Location = new System.Drawing.Point(638, 26);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(24, 13);
            this.label32.TabIndex = 44;
            this.label32.Text = "sec";
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label31.Location = new System.Drawing.Point(178, 26);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(23, 13);
            this.label31.TabIndex = 43;
            this.label31.Text = "min";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(6, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(91, 13);
            this.label6.TabIndex = 40;
            this.label6.Text = "Cached Webdata";
            // 
            // tbxWebCacheTimeout
            // 
            this.tbxWebCacheTimeout.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbxWebCacheTimeout.Location = new System.Drawing.Point(108, 23);
            this.tbxWebCacheTimeout.Name = "tbxWebCacheTimeout";
            this.tbxWebCacheTimeout.Size = new System.Drawing.Size(53, 20);
            this.tbxWebCacheTimeout.TabIndex = 14;
            this.tbxWebCacheTimeout.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.tbxWebCacheTimeout, "WebRequests are cached internally. This number determines the minutes after which" +
        " the cached data becomes invalid. Set to 0 to disable.");
            this.tbxWebCacheTimeout.Validating += new System.ComponentModel.CancelEventHandler(this.CheckValidNumber);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(465, 26);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(70, 13);
            this.label15.TabIndex = 42;
            this.label15.Text = "Webrequests";
            // 
            // tbxUtilTimeout
            // 
            this.tbxUtilTimeout.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbxUtilTimeout.Location = new System.Drawing.Point(541, 23);
            this.tbxUtilTimeout.Name = "tbxUtilTimeout";
            this.tbxUtilTimeout.Size = new System.Drawing.Size(76, 20);
            this.tbxUtilTimeout.TabIndex = 16;
            this.tbxUtilTimeout.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.tbxUtilTimeout, "When the GUI request data from the web you can specifiy how many seconds to wait " +
        "before a timeout will occur.");
            this.tbxUtilTimeout.Validating += new System.ComponentModel.CancelEventHandler(this.CheckValidNumber);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.chkUseMPUrlSourceSplitter);
            this.groupBox2.Controls.Add(this.chkAdaptRefreshRate);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label30);
            this.groupBox2.Controls.Add(this.label29);
            this.groupBox2.Controls.Add(this.udPlayBuffer);
            this.groupBox2.Controls.Add(this.label16);
            this.groupBox2.Controls.Add(this.label24);
            this.groupBox2.Controls.Add(this.tbxWMPBuffer);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(0, 321);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(699, 56);
            this.groupBox2.TabIndex = 49;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "      Playback";
            // 
            // chkUseMPUrlSourceSplitter
            // 
            this.chkUseMPUrlSourceSplitter.AutoSize = true;
            this.chkUseMPUrlSourceSplitter.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkUseMPUrlSourceSplitter.Location = new System.Drawing.Point(545, 24);
            this.chkUseMPUrlSourceSplitter.Name = "chkUseMPUrlSourceSplitter";
            this.chkUseMPUrlSourceSplitter.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkUseMPUrlSourceSplitter.Size = new System.Drawing.Size(143, 17);
            this.chkUseMPUrlSourceSplitter.TabIndex = 52;
            this.chkUseMPUrlSourceSplitter.Text = "Use MPUrlSourceSplitter";
            this.toolTip1.SetToolTip(this.chkUseMPUrlSourceSplitter, "If checked, MPUrlSourceSplitter will be used as source for building graph\r\nIf unc" +
        "hecked, LAV Splitter Source will be used");
            this.chkUseMPUrlSourceSplitter.UseVisualStyleBackColor = true;
            // 
            // chkAdaptRefreshRate
            // 
            this.chkAdaptRefreshRate.AutoSize = true;
            this.chkAdaptRefreshRate.Location = new System.Drawing.Point(113, 25);
            this.chkAdaptRefreshRate.Name = "chkAdaptRefreshRate";
            this.chkAdaptRefreshRate.Size = new System.Drawing.Size(15, 14);
            this.chkAdaptRefreshRate.TabIndex = 51;
            this.toolTip1.SetToolTip(this.chkAdaptRefreshRate, "Enable dynamic adaption of the display refresh rate to the clips fPS at start of " +
        "playback (uses MediaPortal settings)");
            this.chkAdaptRefreshRate.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(6, 26);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(98, 13);
            this.label4.TabIndex = 50;
            this.label4.Text = "Adapt RefreshRate";
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label30.Location = new System.Drawing.Point(284, 26);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(15, 13);
            this.label30.TabIndex = 49;
            this.label30.Text = "%";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label29.Location = new System.Drawing.Point(507, 26);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(32, 13);
            this.label29.TabIndex = 44;
            this.label29.Text = "msec";
            // 
            // udPlayBuffer
            // 
            this.udPlayBuffer.BackColor = System.Drawing.SystemColors.Window;
            this.udPlayBuffer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.udPlayBuffer.Items.Add("20");
            this.udPlayBuffer.Items.Add("19");
            this.udPlayBuffer.Items.Add("18");
            this.udPlayBuffer.Items.Add("17");
            this.udPlayBuffer.Items.Add("16");
            this.udPlayBuffer.Items.Add("15");
            this.udPlayBuffer.Items.Add("14");
            this.udPlayBuffer.Items.Add("13");
            this.udPlayBuffer.Items.Add("12");
            this.udPlayBuffer.Items.Add("11");
            this.udPlayBuffer.Items.Add("10");
            this.udPlayBuffer.Items.Add("9");
            this.udPlayBuffer.Items.Add("8");
            this.udPlayBuffer.Items.Add("7");
            this.udPlayBuffer.Items.Add("6");
            this.udPlayBuffer.Items.Add("5");
            this.udPlayBuffer.Items.Add("4");
            this.udPlayBuffer.Items.Add("3");
            this.udPlayBuffer.Items.Add("2");
            this.udPlayBuffer.Items.Add("1");
            this.udPlayBuffer.Location = new System.Drawing.Point(225, 23);
            this.udPlayBuffer.Name = "udPlayBuffer";
            this.udPlayBuffer.ReadOnly = true;
            this.udPlayBuffer.Size = new System.Drawing.Size(53, 20);
            this.udPlayBuffer.TabIndex = 17;
            this.udPlayBuffer.Text = "1";
            this.udPlayBuffer.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.udPlayBuffer, "Percentage of the file to buffer from web before starting playback.");
            // 
            // label16
            // 
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(303, 18);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(112, 28);
            this.label16.TabIndex = 43;
            this.label16.Text = "Windows Media Player VLC Media Player";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label24
            // 
            this.label24.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label24.Location = new System.Drawing.Point(138, 18);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(81, 26);
            this.label24.TabIndex = 48;
            this.label24.Text = "Internal Player Buffer";
            this.label24.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbxWMPBuffer
            // 
            this.tbxWMPBuffer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbxWMPBuffer.Location = new System.Drawing.Point(438, 23);
            this.tbxWMPBuffer.Name = "tbxWMPBuffer";
            this.tbxWMPBuffer.Size = new System.Drawing.Size(59, 20);
            this.tbxWMPBuffer.TabIndex = 18;
            this.tbxWMPBuffer.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.tbxWMPBuffer, "Number of milliseconds to use as buffer for playback with Windows Media Player.");
            this.tbxWMPBuffer.Validating += new System.ComponentModel.CancelEventHandler(this.CheckValidNumber);
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(6, 61);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(125, 13);
            this.label23.TabIndex = 0;
            this.label23.Text = "Update Sites on first load";
            // 
            // chkDoAutoUpdate
            // 
            this.chkDoAutoUpdate.AutoSize = true;
            this.chkDoAutoUpdate.Checked = true;
            this.chkDoAutoUpdate.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.chkDoAutoUpdate.Location = new System.Drawing.Point(225, 61);
            this.chkDoAutoUpdate.Name = "chkDoAutoUpdate";
            this.chkDoAutoUpdate.Size = new System.Drawing.Size(15, 14);
            this.chkDoAutoUpdate.TabIndex = 4;
            this.chkDoAutoUpdate.ThreeState = true;
            this.toolTip1.SetToolTip(this.chkDoAutoUpdate, "If checked plugin will perform an autoupdate the first time it is started each Me" +
        "diaPortal Session. If indeterminate, plugin will ask.");
            this.chkDoAutoUpdate.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.CausesValidation = false;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.No;
            this.btnCancel.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.delete;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(595, 512);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 25);
            this.btnCancel.TabIndex = 27;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblVersion
            // 
            this.lblVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblVersion.Location = new System.Drawing.Point(32, 520);
            this.lblVersion.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(200, 14);
            this.lblVersion.TabIndex = 37;
            this.lblVersion.Text = "Version";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Thumbnails
            // 
            this.Thumbnails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Thumbnails.Controls.Add(this.label36);
            this.Thumbnails.Controls.Add(this.tbxThumbAge);
            this.Thumbnails.Controls.Add(this.label35);
            this.Thumbnails.Controls.Add(this.label1);
            this.Thumbnails.Controls.Add(this.bntBrowseFolderForThumbs);
            this.Thumbnails.Controls.Add(this.txtThumbLoc);
            this.Thumbnails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Thumbnails.Location = new System.Drawing.Point(0, 384);
            this.Thumbnails.Name = "Thumbnails";
            this.Thumbnails.Size = new System.Drawing.Size(699, 56);
            this.Thumbnails.TabIndex = 56;
            this.Thumbnails.TabStop = false;
            this.Thumbnails.Text = "      Thumbnails";
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label36.Location = new System.Drawing.Point(178, 26);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(29, 13);
            this.label36.TabIndex = 45;
            this.label36.Text = "days";
            // 
            // tbxThumbAge
            // 
            this.tbxThumbAge.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbxThumbAge.Location = new System.Drawing.Point(108, 23);
            this.tbxThumbAge.Name = "tbxThumbAge";
            this.tbxThumbAge.Size = new System.Drawing.Size(53, 20);
            this.tbxThumbAge.TabIndex = 19;
            this.tbxThumbAge.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip1.SetToolTip(this.tbxThumbAge, "Thumbnails older than this will be deleted on first OnlineVideos start each time " +
        "MediaPortal runs. Set to 0 to delete all and -1 to keep all.");
            this.tbxThumbAge.Validating += new System.ComponentModel.CancelEventHandler(this.CheckValidInteger);
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label35.Location = new System.Drawing.Point(6, 26);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(73, 13);
            this.label35.TabIndex = 56;
            this.label35.Text = "Maximum Age";
            // 
            // bntBrowseFolderForThumbs
            // 
            this.bntBrowseFolderForThumbs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bntBrowseFolderForThumbs.AutoSize = true;
            this.bntBrowseFolderForThumbs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bntBrowseFolderForThumbs.Location = new System.Drawing.Point(655, 21);
            this.bntBrowseFolderForThumbs.Margin = new System.Windows.Forms.Padding(2);
            this.bntBrowseFolderForThumbs.Name = "bntBrowseFolderForThumbs";
            this.bntBrowseFolderForThumbs.Size = new System.Drawing.Size(30, 23);
            this.bntBrowseFolderForThumbs.TabIndex = 21;
            this.bntBrowseFolderForThumbs.Text = "...";
            this.bntBrowseFolderForThumbs.UseVisualStyleBackColor = true;
            this.bntBrowseFolderForThumbs.Click += new System.EventHandler(this.bntBrowseFolderForThumbs_Click);
            // 
            // tabGroups
            // 
            this.tabGroups.Controls.Add(this.splitContainer1);
            this.tabGroups.Location = new System.Drawing.Point(4, 22);
            this.tabGroups.Name = "tabGroups";
            this.tabGroups.Size = new System.Drawing.Size(704, 542);
            this.tabGroups.TabIndex = 3;
            this.tabGroups.Text = "Groups";
            this.tabGroups.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.chkFavFirst);
            this.splitContainer1.Panel1.Controls.Add(this.chkAutoGroupByLang);
            this.splitContainer1.Panel1.Controls.Add(this.btnBrowseSitesGroupThumb);
            this.splitContainer1.Panel1.Controls.Add(this.tbxSitesGroupDesc);
            this.splitContainer1.Panel1.Controls.Add(this.label43);
            this.splitContainer1.Panel1.Controls.Add(this.tbxSitesGroupThumb);
            this.splitContainer1.Panel1.Controls.Add(this.label42);
            this.splitContainer1.Panel1.Controls.Add(this.tbxSitesGroupName);
            this.splitContainer1.Panel1.Controls.Add(this.label39);
            this.splitContainer1.Panel1.Controls.Add(this.toolStripContainer4);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(704, 542);
            this.splitContainer1.SplitterDistance = 240;
            this.splitContainer1.TabIndex = 0;
            // 
            // chkFavFirst
            // 
            this.chkFavFirst.AutoSize = true;
            this.chkFavFirst.Location = new System.Drawing.Point(264, 194);
            this.chkFavFirst.Name = "chkFavFirst";
            this.chkFavFirst.Size = new System.Drawing.Size(277, 17);
            this.chkFavFirst.TabIndex = 18;
            this.chkFavFirst.Text = "Favorites and Downloads first instead of last in the list";
            this.chkFavFirst.UseVisualStyleBackColor = true;
            // 
            // chkAutoGroupByLang
            // 
            this.chkAutoGroupByLang.AutoSize = true;
            this.chkAutoGroupByLang.Location = new System.Drawing.Point(264, 162);
            this.chkAutoGroupByLang.Name = "chkAutoGroupByLang";
            this.chkAutoGroupByLang.Size = new System.Drawing.Size(353, 17);
            this.chkAutoGroupByLang.TabIndex = 17;
            this.chkAutoGroupByLang.Text = "Automatically group all sites by their language if no groups are defined";
            this.chkAutoGroupByLang.UseVisualStyleBackColor = true;
            // 
            // btnBrowseSitesGroupThumb
            // 
            this.btnBrowseSitesGroupThumb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseSitesGroupThumb.AutoSize = true;
            this.btnBrowseSitesGroupThumb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBrowseSitesGroupThumb.Location = new System.Drawing.Point(666, 49);
            this.btnBrowseSitesGroupThumb.Margin = new System.Windows.Forms.Padding(2);
            this.btnBrowseSitesGroupThumb.Name = "btnBrowseSitesGroupThumb";
            this.btnBrowseSitesGroupThumb.Size = new System.Drawing.Size(30, 23);
            this.btnBrowseSitesGroupThumb.TabIndex = 16;
            this.btnBrowseSitesGroupThumb.Text = "...";
            this.btnBrowseSitesGroupThumb.UseVisualStyleBackColor = true;
            this.btnBrowseSitesGroupThumb.Click += new System.EventHandler(this.btnBrowseSitesGroupThumb_Click);
            // 
            // tbxSitesGroupDesc
            // 
            this.tbxSitesGroupDesc.AcceptsReturn = true;
            this.tbxSitesGroupDesc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxSitesGroupDesc.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceSitesGroup, "Description", true));
            this.tbxSitesGroupDesc.Location = new System.Drawing.Point(264, 77);
            this.tbxSitesGroupDesc.Multiline = true;
            this.tbxSitesGroupDesc.Name = "tbxSitesGroupDesc";
            this.tbxSitesGroupDesc.Size = new System.Drawing.Size(432, 60);
            this.tbxSitesGroupDesc.TabIndex = 6;
            // 
            // bindingSourceSitesGroup
            // 
            this.bindingSourceSitesGroup.DataSource = typeof(OnlineVideos.MediaPortal1.SitesGroup);
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Location = new System.Drawing.Point(185, 80);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(60, 13);
            this.label43.TabIndex = 5;
            this.label43.Text = "Description";
            // 
            // tbxSitesGroupThumb
            // 
            this.tbxSitesGroupThumb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxSitesGroupThumb.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceSitesGroup, "Thumbnail", true));
            this.tbxSitesGroupThumb.Location = new System.Drawing.Point(264, 51);
            this.tbxSitesGroupThumb.Name = "tbxSitesGroupThumb";
            this.tbxSitesGroupThumb.Size = new System.Drawing.Size(397, 20);
            this.tbxSitesGroupThumb.TabIndex = 4;
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.Location = new System.Drawing.Point(185, 54);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(40, 13);
            this.label42.TabIndex = 3;
            this.label42.Text = "Thumb";
            // 
            // tbxSitesGroupName
            // 
            this.tbxSitesGroupName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxSitesGroupName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceSitesGroup, "Name", true));
            this.tbxSitesGroupName.Location = new System.Drawing.Point(264, 22);
            this.tbxSitesGroupName.Name = "tbxSitesGroupName";
            this.tbxSitesGroupName.Size = new System.Drawing.Size(432, 20);
            this.tbxSitesGroupName.TabIndex = 2;
            // 
            // label39
            // 
            this.label39.AutoSize = true;
            this.label39.Location = new System.Drawing.Point(185, 25);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(35, 13);
            this.label39.TabIndex = 1;
            this.label39.Text = "Name";
            // 
            // toolStripContainer4
            // 
            // 
            // toolStripContainer4.ContentPanel
            // 
            this.toolStripContainer4.ContentPanel.Controls.Add(this.listBoxSitesGroups);
            this.toolStripContainer4.ContentPanel.Size = new System.Drawing.Size(155, 215);
            this.toolStripContainer4.Dock = System.Windows.Forms.DockStyle.Left;
            // 
            // toolStripContainer4.LeftToolStripPanel
            // 
            this.toolStripContainer4.LeftToolStripPanel.Controls.Add(this.toolStripSitesGroupLeft);
            this.toolStripContainer4.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer4.Name = "toolStripContainer4";
            this.toolStripContainer4.Size = new System.Drawing.Size(179, 240);
            this.toolStripContainer4.TabIndex = 0;
            this.toolStripContainer4.Text = "toolStripContainer4";
            // 
            // toolStripContainer4.TopToolStripPanel
            // 
            this.toolStripContainer4.TopToolStripPanel.Controls.Add(this.toolStripSitesGroupTop);
            // 
            // listBoxSitesGroups
            // 
            this.listBoxSitesGroups.DataSource = this.bindingSourceSitesGroup;
            this.listBoxSitesGroups.DisplayMember = "Name";
            this.listBoxSitesGroups.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxSitesGroups.FormattingEnabled = true;
            this.listBoxSitesGroups.Location = new System.Drawing.Point(0, 0);
            this.listBoxSitesGroups.Name = "listBoxSitesGroups";
            this.listBoxSitesGroups.Size = new System.Drawing.Size(155, 215);
            this.listBoxSitesGroups.TabIndex = 0;
            this.listBoxSitesGroups.SelectedValueChanged += new System.EventHandler(this.listBoxSitesGroups_SelectedValueChanged);
            // 
            // toolStripSitesGroupLeft
            // 
            this.toolStripSitesGroupLeft.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripSitesGroupLeft.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripSitesGroupLeft.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSitesGroupUp,
            this.btnSitesGroupDown});
            this.toolStripSitesGroupLeft.Location = new System.Drawing.Point(0, 0);
            this.toolStripSitesGroupLeft.Name = "toolStripSitesGroupLeft";
            this.toolStripSitesGroupLeft.Size = new System.Drawing.Size(24, 240);
            this.toolStripSitesGroupLeft.Stretch = true;
            this.toolStripSitesGroupLeft.TabIndex = 0;
            // 
            // btnSitesGroupUp
            // 
            this.btnSitesGroupUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSitesGroupUp.Enabled = false;
            this.btnSitesGroupUp.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.Up;
            this.btnSitesGroupUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSitesGroupUp.Name = "btnSitesGroupUp";
            this.btnSitesGroupUp.Size = new System.Drawing.Size(22, 20);
            this.btnSitesGroupUp.Text = "Move Group Up";
            this.btnSitesGroupUp.Click += new System.EventHandler(this.btnSitesGroupUp_Click);
            // 
            // btnSitesGroupDown
            // 
            this.btnSitesGroupDown.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnSitesGroupDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSitesGroupDown.Enabled = false;
            this.btnSitesGroupDown.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.Down;
            this.btnSitesGroupDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSitesGroupDown.Name = "btnSitesGroupDown";
            this.btnSitesGroupDown.Size = new System.Drawing.Size(22, 20);
            this.btnSitesGroupDown.Text = "Move Group Down";
            this.btnSitesGroupDown.Click += new System.EventHandler(this.btnSitesGroupDown_Click);
            // 
            // toolStripSitesGroupTop
            // 
            this.toolStripSitesGroupTop.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripSitesGroupTop.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripSitesGroupTop.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnAddSitesGroup,
            this.btnDeleteSitesGroup});
            this.toolStripSitesGroupTop.Location = new System.Drawing.Point(3, 0);
            this.toolStripSitesGroupTop.Name = "toolStripSitesGroupTop";
            this.toolStripSitesGroupTop.Size = new System.Drawing.Size(49, 25);
            this.toolStripSitesGroupTop.TabIndex = 0;
            // 
            // btnAddSitesGroup
            // 
            this.btnAddSitesGroup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAddSitesGroup.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.Add;
            this.btnAddSitesGroup.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddSitesGroup.Name = "btnAddSitesGroup";
            this.btnAddSitesGroup.Size = new System.Drawing.Size(23, 22);
            this.btnAddSitesGroup.Text = "Add Group";
            this.btnAddSitesGroup.Click += new System.EventHandler(this.btnAddSitesGroup_Click);
            // 
            // btnDeleteSitesGroup
            // 
            this.btnDeleteSitesGroup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDeleteSitesGroup.Enabled = false;
            this.btnDeleteSitesGroup.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.delete;
            this.btnDeleteSitesGroup.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDeleteSitesGroup.Name = "btnDeleteSitesGroup";
            this.btnDeleteSitesGroup.Size = new System.Drawing.Size(23, 22);
            this.btnDeleteSitesGroup.Text = "Delete Group";
            this.btnDeleteSitesGroup.Click += new System.EventHandler(this.btnDeleteSitesGroup_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.listViewSitesNotInGroup);
            this.splitContainer2.Panel1.Controls.Add(this.label40);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.listViewSitesInGroup);
            this.splitContainer2.Panel2.Controls.Add(this.label41);
            this.splitContainer2.Size = new System.Drawing.Size(704, 298);
            this.splitContainer2.SplitterDistance = 352;
            this.splitContainer2.SplitterWidth = 1;
            this.splitContainer2.TabIndex = 0;
            // 
            // listViewSitesNotInGroup
            // 
            this.listViewSitesNotInGroup.BackColor = System.Drawing.Color.AliceBlue;
            this.listViewSitesNotInGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewSitesNotInGroup.HideSelection = false;
            this.listViewSitesNotInGroup.LargeImageList = this.imageListSiteIcons;
            this.listViewSitesNotInGroup.Location = new System.Drawing.Point(0, 23);
            this.listViewSitesNotInGroup.Name = "listViewSitesNotInGroup";
            this.listViewSitesNotInGroup.Size = new System.Drawing.Size(352, 275);
            this.listViewSitesNotInGroup.TabIndex = 0;
            this.listViewSitesNotInGroup.UseCompatibleStateImageBehavior = false;
            this.listViewSitesNotInGroup.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBoxSitesNotInGroup_MouseDoubleClick);
            // 
            // label40
            // 
            this.label40.Dock = System.Windows.Forms.DockStyle.Top;
            this.label40.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label40.Location = new System.Drawing.Point(0, 0);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(352, 23);
            this.label40.TabIndex = 1;
            this.label40.Text = "Not in Group (doubleclick to add)";
            this.label40.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // listViewSitesInGroup
            // 
            this.listViewSitesInGroup.BackColor = System.Drawing.Color.AliceBlue;
            this.listViewSitesInGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewSitesInGroup.HideSelection = false;
            this.listViewSitesInGroup.LargeImageList = this.imageListSiteIcons;
            this.listViewSitesInGroup.Location = new System.Drawing.Point(0, 23);
            this.listViewSitesInGroup.Name = "listViewSitesInGroup";
            this.listViewSitesInGroup.Size = new System.Drawing.Size(351, 275);
            this.listViewSitesInGroup.TabIndex = 0;
            this.listViewSitesInGroup.UseCompatibleStateImageBehavior = false;
            this.listViewSitesInGroup.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBoxSitesInGroup_MouseDoubleClick);
            // 
            // label41
            // 
            this.label41.Dock = System.Windows.Forms.DockStyle.Top;
            this.label41.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label41.Location = new System.Drawing.Point(0, 0);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(351, 23);
            this.label41.TabIndex = 1;
            this.label41.Text = "In Group (doubleclick to remove)";
            this.label41.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabSites
            // 
            this.tabSites.Controls.Add(this.siteList);
            this.tabSites.Controls.Add(this.splitter2);
            this.tabSites.Controls.Add(this.panel1);
            this.tabSites.Controls.Add(this.toolStripSites);
            this.tabSites.Location = new System.Drawing.Point(4, 22);
            this.tabSites.Name = "tabSites";
            this.tabSites.Padding = new System.Windows.Forms.Padding(3);
            this.tabSites.Size = new System.Drawing.Size(704, 542);
            this.tabSites.TabIndex = 5;
            this.tabSites.Text = "Sites";
            this.tabSites.UseVisualStyleBackColor = true;
            // 
            // splitter2
            // 
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter2.Location = new System.Drawing.Point(3, 400);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(698, 5);
            this.splitter2.TabIndex = 5;
            this.splitter2.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.propertyGridUserConfig);
            this.panel1.Controls.Add(siteNameIconPanel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(3, 405);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(698, 134);
            this.panel1.TabIndex = 1;
            // 
            // toolStripSites
            // 
            this.toolStripSites.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripSites.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownBtnImport,
            this.btnAddSite,
            this.btnSiteUp,
            this.btnSiteDown,
            this.btnDeleteSite,
            this.btnReportSite,
            this.btnEditSite,
            this.btnPublishSite,
            this.btnCreateSite});
            this.toolStripSites.Location = new System.Drawing.Point(3, 3);
            this.toolStripSites.Name = "toolStripSites";
            this.toolStripSites.Size = new System.Drawing.Size(698, 25);
            this.toolStripSites.TabIndex = 0;
            // 
            // toolStripDropDownBtnImport
            // 
            this.toolStripDropDownBtnImport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownBtnImport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnImportXml,
            this.btnImportGlobal});
            this.toolStripDropDownBtnImport.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.Import;
            this.toolStripDropDownBtnImport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownBtnImport.Name = "toolStripDropDownBtnImport";
            this.toolStripDropDownBtnImport.Size = new System.Drawing.Size(29, 22);
            this.toolStripDropDownBtnImport.ToolTipText = "Import";
            // 
            // btnImportXml
            // 
            this.btnImportXml.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.ImportXml;
            this.btnImportXml.Name = "btnImportXml";
            this.btnImportXml.Size = new System.Drawing.Size(123, 24);
            this.btnImportXml.Text = "XML";
            this.btnImportXml.ToolTipText = "Import from Xml";
            this.btnImportXml.Click += new System.EventHandler(this.btnImportSite_Click);
            // 
            // btnImportGlobal
            // 
            this.btnImportGlobal.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.ImportGlobal;
            this.btnImportGlobal.Name = "btnImportGlobal";
            this.btnImportGlobal.Size = new System.Drawing.Size(123, 24);
            this.btnImportGlobal.Text = "Global";
            this.btnImportGlobal.ToolTipText = "Import from global List";
            this.btnImportGlobal.Click += new System.EventHandler(this.btnImportGlobal_Click);
            // 
            // btnAddSite
            // 
            this.btnAddSite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAddSite.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.Add;
            this.btnAddSite.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddSite.Name = "btnAddSite";
            this.btnAddSite.Size = new System.Drawing.Size(23, 22);
            this.btnAddSite.Text = "Add";
            this.btnAddSite.ToolTipText = "Add a new Site";
            this.btnAddSite.Click += new System.EventHandler(this.btnAddSite_Click);
            // 
            // btnSiteUp
            // 
            this.btnSiteUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSiteUp.Enabled = false;
            this.btnSiteUp.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.Up;
            this.btnSiteUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSiteUp.Name = "btnSiteUp";
            this.btnSiteUp.Size = new System.Drawing.Size(23, 22);
            this.btnSiteUp.Text = "Move Site Up";
            this.btnSiteUp.ToolTipText = "Move Site Up (Removes sorting and grouping)";
            this.btnSiteUp.Click += new System.EventHandler(this.btnSiteUp_Click);
            // 
            // btnSiteDown
            // 
            this.btnSiteDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSiteDown.Enabled = false;
            this.btnSiteDown.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.Down;
            this.btnSiteDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSiteDown.Name = "btnSiteDown";
            this.btnSiteDown.Size = new System.Drawing.Size(23, 22);
            this.btnSiteDown.Text = "Move Site Down";
            this.btnSiteDown.ToolTipText = "Move Site Down (Removes sorting and grouping)";
            this.btnSiteDown.Click += new System.EventHandler(this.btnSiteDown_Click);
            // 
            // btnDeleteSite
            // 
            this.btnDeleteSite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDeleteSite.Enabled = false;
            this.btnDeleteSite.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.delete;
            this.btnDeleteSite.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDeleteSite.Name = "btnDeleteSite";
            this.btnDeleteSite.Size = new System.Drawing.Size(23, 22);
            this.btnDeleteSite.Text = "Delete";
            this.btnDeleteSite.Click += new System.EventHandler(this.btnDeleteSite_Click);
            // 
            // btnReportSite
            // 
            this.btnReportSite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnReportSite.Enabled = false;
            this.btnReportSite.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.NewReport;
            this.btnReportSite.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnReportSite.Name = "btnReportSite";
            this.btnReportSite.Size = new System.Drawing.Size(23, 22);
            this.btnReportSite.Text = "Write report";
            this.btnReportSite.ToolTipText = "Submit a message to the creator";
            this.btnReportSite.Click += new System.EventHandler(this.btnReportSite_Click);
            // 
            // btnEditSite
            // 
            this.btnEditSite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnEditSite.Enabled = false;
            this.btnEditSite.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.edit;
            this.btnEditSite.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEditSite.Name = "btnEditSite";
            this.btnEditSite.Size = new System.Drawing.Size(23, 22);
            this.btnEditSite.Text = "Edit";
            this.btnEditSite.Click += new System.EventHandler(this.btnEditSite_Click);
            // 
            // btnPublishSite
            // 
            this.btnPublishSite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPublishSite.Enabled = false;
            this.btnPublishSite.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.PublishToWeb;
            this.btnPublishSite.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPublishSite.Name = "btnPublishSite";
            this.btnPublishSite.Size = new System.Drawing.Size(23, 22);
            this.btnPublishSite.Text = "Publish to Web";
            this.btnPublishSite.Click += new System.EventHandler(this.btnPublishSite_Click);
            // 
            // btnCreateSite
            // 
            this.btnCreateSite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCreateSite.Image = global::OnlineVideos.MediaPortal1.Properties.Resources.CreateSite;
            this.btnCreateSite.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCreateSite.Name = "btnCreateSite";
            this.btnCreateSite.Size = new System.Drawing.Size(23, 22);
            this.btnCreateSite.Text = "Create/Edit Generic Site";
            this.btnCreateSite.Click += new System.EventHandler(this.btnCreateSite_Click);
            // 
            // tabHosters
            // 
            this.tabHosters.Controls.Add(this.propertyGridHoster);
            this.tabHosters.Controls.Add(this.panel2);
            this.tabHosters.Controls.Add(this.splitter1);
            this.tabHosters.Controls.Add(this.listBoxHosters);
            this.tabHosters.Location = new System.Drawing.Point(4, 22);
            this.tabHosters.Name = "tabHosters";
            this.tabHosters.Size = new System.Drawing.Size(704, 542);
            this.tabHosters.TabIndex = 4;
            this.tabHosters.Text = "Hosters";
            this.tabHosters.UseVisualStyleBackColor = true;
            // 
            // propertyGridHoster
            // 
            this.propertyGridHoster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridHoster.Location = new System.Drawing.Point(123, 0);
            this.propertyGridHoster.Name = "propertyGridHoster";
            this.propertyGridHoster.Size = new System.Drawing.Size(581, 518);
            this.propertyGridHoster.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.sourceLabel);
            this.panel2.Controls.Add(this.label9);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(123, 518);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(581, 24);
            this.panel2.TabIndex = 3;
            // 
            // sourceLabel
            // 
            this.sourceLabel.AutoSize = true;
            this.sourceLabel.Location = new System.Drawing.Point(56, 6);
            this.sourceLabel.Name = "sourceLabel";
            this.sourceLabel.Size = new System.Drawing.Size(53, 13);
            this.sourceLabel.TabIndex = 1;
            this.sourceLabel.Text = "Unknown";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 6);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(44, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Source:";
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(120, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 542);
            this.splitter1.TabIndex = 2;
            this.splitter1.TabStop = false;
            // 
            // listBoxHosters
            // 
            this.listBoxHosters.Dock = System.Windows.Forms.DockStyle.Left;
            this.listBoxHosters.FormattingEnabled = true;
            this.listBoxHosters.Location = new System.Drawing.Point(0, 0);
            this.listBoxHosters.Name = "listBoxHosters";
            this.listBoxHosters.Size = new System.Drawing.Size(120, 542);
            this.listBoxHosters.TabIndex = 0;
            this.listBoxHosters.SelectedValueChanged += new System.EventHandler(this.listBoxHosters_SelectedValueChanged);
            // 
            // tabSourceFilter
            // 
            this.tabSourceFilter.Controls.Add(this.tabProtocols);
            this.tabSourceFilter.Location = new System.Drawing.Point(4, 22);
            this.tabSourceFilter.Name = "tabSourceFilter";
            this.tabSourceFilter.Padding = new System.Windows.Forms.Padding(3);
            this.tabSourceFilter.Size = new System.Drawing.Size(704, 542);
            this.tabSourceFilter.TabIndex = 6;
            this.tabSourceFilter.Text = "Source filter";
            this.tabSourceFilter.UseVisualStyleBackColor = true;
            // 
            // tabProtocols
            // 
            this.tabProtocols.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabProtocols.Controls.Add(this.tabPageNotDetectedFilter);
            this.tabProtocols.Controls.Add(this.tabPageHttp);
            this.tabProtocols.Controls.Add(this.tabPageRtmp);
            this.tabProtocols.Controls.Add(this.tabPageRtsp);
            this.tabProtocols.Controls.Add(this.tabPageUdpRtp);
            this.tabProtocols.Location = new System.Drawing.Point(3, 6);
            this.tabProtocols.Multiline = true;
            this.tabProtocols.Name = "tabProtocols";
            this.tabProtocols.SelectedIndex = 0;
            this.tabProtocols.Size = new System.Drawing.Size(698, 533);
            this.tabProtocols.TabIndex = 0;
            // 
            // tabPageNotDetectedFilter
            // 
            this.tabPageNotDetectedFilter.Controls.Add(this.linkLabelFilterDownload);
            this.tabPageNotDetectedFilter.Controls.Add(this.label10);
            this.tabPageNotDetectedFilter.Location = new System.Drawing.Point(4, 22);
            this.tabPageNotDetectedFilter.Name = "tabPageNotDetectedFilter";
            this.tabPageNotDetectedFilter.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageNotDetectedFilter.Size = new System.Drawing.Size(690, 507);
            this.tabPageNotDetectedFilter.TabIndex = 0;
            this.tabPageNotDetectedFilter.Text = "Filter not detected";
            this.tabPageNotDetectedFilter.UseVisualStyleBackColor = true;
            // 
            // linkLabelFilterDownload
            // 
            this.linkLabelFilterDownload.AutoSize = true;
            this.linkLabelFilterDownload.Location = new System.Drawing.Point(11, 27);
            this.linkLabelFilterDownload.Name = "linkLabelFilterDownload";
            this.linkLabelFilterDownload.Size = new System.Drawing.Size(497, 13);
            this.linkLabelFilterDownload.TabIndex = 1;
            this.linkLabelFilterDownload.TabStop = true;
            this.linkLabelFilterDownload.Text = "http://www.team-mediaportal.com/index.php?option=com_mtree&task=att_download&link" +
    "_id=327&cf_id=24";
            this.linkLabelFilterDownload.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelFilterDownload_LinkClicked);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(11, 14);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(580, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "MediaPortal IPTV filter and url source splitter not detected. Use MediaPortal Ext" +
    "ensions Installer to install or download from";
            // 
            // tabPageHttp
            // 
            this.tabPageHttp.Controls.Add(this.groupBoxHttpProxyServerAuthentication);
            this.tabPageHttp.Controls.Add(this.groupBoxHttpServerAuthentication);
            this.tabPageHttp.Controls.Add(this.groupBoxHttpCommonParameters);
            this.tabPageHttp.Location = new System.Drawing.Point(4, 22);
            this.tabPageHttp.Name = "tabPageHttp";
            this.tabPageHttp.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageHttp.Size = new System.Drawing.Size(690, 507);
            this.tabPageHttp.TabIndex = 1;
            this.tabPageHttp.Text = "HTTP";
            this.tabPageHttp.UseVisualStyleBackColor = true;
            // 
            // groupBoxHttpProxyServerAuthentication
            // 
            this.groupBoxHttpProxyServerAuthentication.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxHttpProxyServerAuthentication.Controls.Add(this.checkBoxHttpProxyServerAuthentication);
            this.groupBoxHttpProxyServerAuthentication.Controls.Add(this.labelHttpProxyType);
            this.groupBoxHttpProxyServerAuthentication.Controls.Add(this.comboBoxHttpProxyServerType);
            this.groupBoxHttpProxyServerAuthentication.Controls.Add(this.labelHttpProxyServerPassword);
            this.groupBoxHttpProxyServerAuthentication.Controls.Add(this.labelHttpProxyServerUserName);
            this.groupBoxHttpProxyServerAuthentication.Controls.Add(this.labelHttpProxyServerPort);
            this.groupBoxHttpProxyServerAuthentication.Controls.Add(this.labelHttpProxyServer);
            this.groupBoxHttpProxyServerAuthentication.Controls.Add(this.textBoxHttpProxyServerPassword);
            this.groupBoxHttpProxyServerAuthentication.Controls.Add(this.textBoxHttpProxyServerUserName);
            this.groupBoxHttpProxyServerAuthentication.Controls.Add(this.textBoxHttpProxyServerPort);
            this.groupBoxHttpProxyServerAuthentication.Controls.Add(this.textBoxHttpProxyServer);
            this.groupBoxHttpProxyServerAuthentication.Location = new System.Drawing.Point(6, 225);
            this.groupBoxHttpProxyServerAuthentication.Name = "groupBoxHttpProxyServerAuthentication";
            this.groupBoxHttpProxyServerAuthentication.Size = new System.Drawing.Size(678, 134);
            this.groupBoxHttpProxyServerAuthentication.TabIndex = 3;
            this.groupBoxHttpProxyServerAuthentication.TabStop = false;
            this.groupBoxHttpProxyServerAuthentication.Text = "Proxy server authentication";
            // 
            // checkBoxHttpProxyServerAuthentication
            // 
            this.checkBoxHttpProxyServerAuthentication.AutoSize = true;
            this.checkBoxHttpProxyServerAuthentication.Location = new System.Drawing.Point(11, 19);
            this.checkBoxHttpProxyServerAuthentication.Name = "checkBoxHttpProxyServerAuthentication";
            this.checkBoxHttpProxyServerAuthentication.Size = new System.Drawing.Size(189, 17);
            this.checkBoxHttpProxyServerAuthentication.TabIndex = 0;
            this.checkBoxHttpProxyServerAuthentication.Text = "Enable proxy server authentication";
            this.checkBoxHttpProxyServerAuthentication.UseVisualStyleBackColor = true;
            // 
            // labelHttpProxyType
            // 
            this.labelHttpProxyType.AutoSize = true;
            this.labelHttpProxyType.Location = new System.Drawing.Point(8, 97);
            this.labelHttpProxyType.Name = "labelHttpProxyType";
            this.labelHttpProxyType.Size = new System.Drawing.Size(56, 13);
            this.labelHttpProxyType.TabIndex = 9;
            this.labelHttpProxyType.Text = "Proxy type";
            // 
            // comboBoxHttpProxyServerType
            // 
            this.comboBoxHttpProxyServerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxHttpProxyServerType.FormattingEnabled = true;
            this.comboBoxHttpProxyServerType.Items.AddRange(new object[] {
            "HTTP",
            "HTTP 1.0",
            "SOCKS4",
            "SOCKS5",
            "SOCKS4A",
            "SOCKS5 with hostname"});
            this.comboBoxHttpProxyServerType.Location = new System.Drawing.Point(89, 94);
            this.comboBoxHttpProxyServerType.Name = "comboBoxHttpProxyServerType";
            this.comboBoxHttpProxyServerType.Size = new System.Drawing.Size(182, 21);
            this.comboBoxHttpProxyServerType.TabIndex = 10;
            // 
            // labelHttpProxyServerPassword
            // 
            this.labelHttpProxyServerPassword.AutoSize = true;
            this.labelHttpProxyServerPassword.Location = new System.Drawing.Point(245, 71);
            this.labelHttpProxyServerPassword.Name = "labelHttpProxyServerPassword";
            this.labelHttpProxyServerPassword.Size = new System.Drawing.Size(53, 13);
            this.labelHttpProxyServerPassword.TabIndex = 7;
            this.labelHttpProxyServerPassword.Text = "Password";
            // 
            // labelHttpProxyServerUserName
            // 
            this.labelHttpProxyServerUserName.AutoSize = true;
            this.labelHttpProxyServerUserName.Location = new System.Drawing.Point(8, 71);
            this.labelHttpProxyServerUserName.Name = "labelHttpProxyServerUserName";
            this.labelHttpProxyServerUserName.Size = new System.Drawing.Size(58, 13);
            this.labelHttpProxyServerUserName.TabIndex = 5;
            this.labelHttpProxyServerUserName.Text = "User name";
            // 
            // labelHttpProxyServerPort
            // 
            this.labelHttpProxyServerPort.AutoSize = true;
            this.labelHttpProxyServerPort.Location = new System.Drawing.Point(375, 45);
            this.labelHttpProxyServerPort.Name = "labelHttpProxyServerPort";
            this.labelHttpProxyServerPort.Size = new System.Drawing.Size(26, 13);
            this.labelHttpProxyServerPort.TabIndex = 3;
            this.labelHttpProxyServerPort.Text = "Port";
            // 
            // labelHttpProxyServer
            // 
            this.labelHttpProxyServer.AutoSize = true;
            this.labelHttpProxyServer.Location = new System.Drawing.Point(8, 45);
            this.labelHttpProxyServer.Name = "labelHttpProxyServer";
            this.labelHttpProxyServer.Size = new System.Drawing.Size(65, 13);
            this.labelHttpProxyServer.TabIndex = 1;
            this.labelHttpProxyServer.Text = "Proxy server";
            // 
            // textBoxHttpProxyServerPassword
            // 
            this.textBoxHttpProxyServerPassword.Location = new System.Drawing.Point(307, 68);
            this.textBoxHttpProxyServerPassword.Name = "textBoxHttpProxyServerPassword";
            this.textBoxHttpProxyServerPassword.Size = new System.Drawing.Size(150, 20);
            this.textBoxHttpProxyServerPassword.TabIndex = 8;
            // 
            // textBoxHttpProxyServerUserName
            // 
            this.textBoxHttpProxyServerUserName.Location = new System.Drawing.Point(89, 68);
            this.textBoxHttpProxyServerUserName.Name = "textBoxHttpProxyServerUserName";
            this.textBoxHttpProxyServerUserName.Size = new System.Drawing.Size(150, 20);
            this.textBoxHttpProxyServerUserName.TabIndex = 6;
            // 
            // textBoxHttpProxyServerPort
            // 
            this.textBoxHttpProxyServerPort.Location = new System.Drawing.Point(407, 42);
            this.textBoxHttpProxyServerPort.Name = "textBoxHttpProxyServerPort";
            this.textBoxHttpProxyServerPort.Size = new System.Drawing.Size(47, 20);
            this.textBoxHttpProxyServerPort.TabIndex = 4;
            // 
            // textBoxHttpProxyServer
            // 
            this.textBoxHttpProxyServer.Location = new System.Drawing.Point(89, 42);
            this.textBoxHttpProxyServer.Name = "textBoxHttpProxyServer";
            this.textBoxHttpProxyServer.Size = new System.Drawing.Size(267, 20);
            this.textBoxHttpProxyServer.TabIndex = 2;
            // 
            // groupBoxHttpServerAuthentication
            // 
            this.groupBoxHttpServerAuthentication.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxHttpServerAuthentication.Controls.Add(this.checkBoxHttpServerAuthentication);
            this.groupBoxHttpServerAuthentication.Controls.Add(this.labelHttpServerPassword);
            this.groupBoxHttpServerAuthentication.Controls.Add(this.textBoxHttpServerPassword);
            this.groupBoxHttpServerAuthentication.Controls.Add(this.textBoxHttpServerUserName);
            this.groupBoxHttpServerAuthentication.Controls.Add(this.labelHttpServerUserName);
            this.groupBoxHttpServerAuthentication.Location = new System.Drawing.Point(6, 145);
            this.groupBoxHttpServerAuthentication.Name = "groupBoxHttpServerAuthentication";
            this.groupBoxHttpServerAuthentication.Size = new System.Drawing.Size(678, 74);
            this.groupBoxHttpServerAuthentication.TabIndex = 2;
            this.groupBoxHttpServerAuthentication.TabStop = false;
            this.groupBoxHttpServerAuthentication.Text = "Remote server authentication";
            // 
            // checkBoxHttpServerAuthentication
            // 
            this.checkBoxHttpServerAuthentication.AutoSize = true;
            this.checkBoxHttpServerAuthentication.Location = new System.Drawing.Point(11, 19);
            this.checkBoxHttpServerAuthentication.Name = "checkBoxHttpServerAuthentication";
            this.checkBoxHttpServerAuthentication.Size = new System.Drawing.Size(161, 17);
            this.checkBoxHttpServerAuthentication.TabIndex = 0;
            this.checkBoxHttpServerAuthentication.Text = "Enable server authentication";
            this.checkBoxHttpServerAuthentication.UseVisualStyleBackColor = true;
            // 
            // labelHttpServerPassword
            // 
            this.labelHttpServerPassword.AutoSize = true;
            this.labelHttpServerPassword.Location = new System.Drawing.Point(245, 45);
            this.labelHttpServerPassword.Name = "labelHttpServerPassword";
            this.labelHttpServerPassword.Size = new System.Drawing.Size(53, 13);
            this.labelHttpServerPassword.TabIndex = 3;
            this.labelHttpServerPassword.Text = "Password";
            // 
            // textBoxHttpServerPassword
            // 
            this.textBoxHttpServerPassword.Location = new System.Drawing.Point(304, 42);
            this.textBoxHttpServerPassword.Name = "textBoxHttpServerPassword";
            this.textBoxHttpServerPassword.Size = new System.Drawing.Size(150, 20);
            this.textBoxHttpServerPassword.TabIndex = 4;
            // 
            // textBoxHttpServerUserName
            // 
            this.textBoxHttpServerUserName.Location = new System.Drawing.Point(89, 42);
            this.textBoxHttpServerUserName.Name = "textBoxHttpServerUserName";
            this.textBoxHttpServerUserName.Size = new System.Drawing.Size(150, 20);
            this.textBoxHttpServerUserName.TabIndex = 2;
            // 
            // labelHttpServerUserName
            // 
            this.labelHttpServerUserName.AutoSize = true;
            this.labelHttpServerUserName.Location = new System.Drawing.Point(6, 45);
            this.labelHttpServerUserName.Name = "labelHttpServerUserName";
            this.labelHttpServerUserName.Size = new System.Drawing.Size(58, 13);
            this.labelHttpServerUserName.TabIndex = 1;
            this.labelHttpServerUserName.Text = "User name";
            // 
            // groupBoxHttpCommonParameters
            // 
            this.groupBoxHttpCommonParameters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxHttpCommonParameters.Controls.Add(this.label25);
            this.groupBoxHttpCommonParameters.Controls.Add(this.label22);
            this.groupBoxHttpCommonParameters.Controls.Add(this.label20);
            this.groupBoxHttpCommonParameters.Controls.Add(this.labelHttpTotalReopenConnectionTimeout);
            this.groupBoxHttpCommonParameters.Controls.Add(this.labelHttpOpenConnectionSleepTime);
            this.groupBoxHttpCommonParameters.Controls.Add(this.textBoxHttpTotalReopenConnectionTimeout);
            this.groupBoxHttpCommonParameters.Controls.Add(this.textBoxHttpOpenConnectionSleepTime);
            this.groupBoxHttpCommonParameters.Controls.Add(this.textBoxHttpOpenConnectionTimeout);
            this.groupBoxHttpCommonParameters.Controls.Add(this.labelHttpOpenConnectionTimeout);
            this.groupBoxHttpCommonParameters.Controls.Add(this.labelHttpNetworkInterface);
            this.groupBoxHttpCommonParameters.Controls.Add(this.comboBoxHttpPreferredNetworkInterface);
            this.groupBoxHttpCommonParameters.Location = new System.Drawing.Point(6, 10);
            this.groupBoxHttpCommonParameters.Name = "groupBoxHttpCommonParameters";
            this.groupBoxHttpCommonParameters.Size = new System.Drawing.Size(678, 129);
            this.groupBoxHttpCommonParameters.TabIndex = 1;
            this.groupBoxHttpCommonParameters.TabStop = false;
            this.groupBoxHttpCommonParameters.Text = "Common configuration parameters for HTTP protocol";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(278, 101);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(63, 13);
            this.label25.TabIndex = 10;
            this.label25.Text = "milliseconds";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(278, 75);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(63, 13);
            this.label22.TabIndex = 9;
            this.label22.Text = "milliseconds";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(278, 49);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(63, 13);
            this.label20.TabIndex = 8;
            this.label20.Text = "milliseconds";
            // 
            // labelHttpTotalReopenConnectionTimeout
            // 
            this.labelHttpTotalReopenConnectionTimeout.AutoSize = true;
            this.labelHttpTotalReopenConnectionTimeout.Location = new System.Drawing.Point(6, 101);
            this.labelHttpTotalReopenConnectionTimeout.Name = "labelHttpTotalReopenConnectionTimeout";
            this.labelHttpTotalReopenConnectionTimeout.Size = new System.Drawing.Size(160, 13);
            this.labelHttpTotalReopenConnectionTimeout.TabIndex = 7;
            this.labelHttpTotalReopenConnectionTimeout.Text = "Total reopen connection timeout";
            // 
            // labelHttpOpenConnectionSleepTime
            // 
            this.labelHttpOpenConnectionSleepTime.AutoSize = true;
            this.labelHttpOpenConnectionSleepTime.Location = new System.Drawing.Point(6, 75);
            this.labelHttpOpenConnectionSleepTime.Name = "labelHttpOpenConnectionSleepTime";
            this.labelHttpOpenConnectionSleepTime.Size = new System.Drawing.Size(139, 13);
            this.labelHttpOpenConnectionSleepTime.TabIndex = 6;
            this.labelHttpOpenConnectionSleepTime.Text = "Open connection sleep time";
            // 
            // textBoxHttpTotalReopenConnectionTimeout
            // 
            this.textBoxHttpTotalReopenConnectionTimeout.Location = new System.Drawing.Point(172, 98);
            this.textBoxHttpTotalReopenConnectionTimeout.Name = "textBoxHttpTotalReopenConnectionTimeout";
            this.textBoxHttpTotalReopenConnectionTimeout.Size = new System.Drawing.Size(100, 20);
            this.textBoxHttpTotalReopenConnectionTimeout.TabIndex = 5;
            // 
            // textBoxHttpOpenConnectionSleepTime
            // 
            this.textBoxHttpOpenConnectionSleepTime.Location = new System.Drawing.Point(172, 72);
            this.textBoxHttpOpenConnectionSleepTime.Name = "textBoxHttpOpenConnectionSleepTime";
            this.textBoxHttpOpenConnectionSleepTime.Size = new System.Drawing.Size(100, 20);
            this.textBoxHttpOpenConnectionSleepTime.TabIndex = 4;
            // 
            // textBoxHttpOpenConnectionTimeout
            // 
            this.textBoxHttpOpenConnectionTimeout.Location = new System.Drawing.Point(172, 46);
            this.textBoxHttpOpenConnectionTimeout.Name = "textBoxHttpOpenConnectionTimeout";
            this.textBoxHttpOpenConnectionTimeout.Size = new System.Drawing.Size(100, 20);
            this.textBoxHttpOpenConnectionTimeout.TabIndex = 3;
            // 
            // labelHttpOpenConnectionTimeout
            // 
            this.labelHttpOpenConnectionTimeout.AutoSize = true;
            this.labelHttpOpenConnectionTimeout.Location = new System.Drawing.Point(6, 49);
            this.labelHttpOpenConnectionTimeout.Name = "labelHttpOpenConnectionTimeout";
            this.labelHttpOpenConnectionTimeout.Size = new System.Drawing.Size(126, 13);
            this.labelHttpOpenConnectionTimeout.TabIndex = 2;
            this.labelHttpOpenConnectionTimeout.Text = "Open connection timeout";
            // 
            // labelHttpNetworkInterface
            // 
            this.labelHttpNetworkInterface.AutoSize = true;
            this.labelHttpNetworkInterface.Location = new System.Drawing.Point(6, 22);
            this.labelHttpNetworkInterface.Name = "labelHttpNetworkInterface";
            this.labelHttpNetworkInterface.Size = new System.Drawing.Size(135, 13);
            this.labelHttpNetworkInterface.TabIndex = 1;
            this.labelHttpNetworkInterface.Text = "Preferred network interface";
            // 
            // comboBoxHttpPreferredNetworkInterface
            // 
            this.comboBoxHttpPreferredNetworkInterface.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxHttpPreferredNetworkInterface.FormattingEnabled = true;
            this.comboBoxHttpPreferredNetworkInterface.Location = new System.Drawing.Point(172, 19);
            this.comboBoxHttpPreferredNetworkInterface.Name = "comboBoxHttpPreferredNetworkInterface";
            this.comboBoxHttpPreferredNetworkInterface.Size = new System.Drawing.Size(500, 21);
            this.comboBoxHttpPreferredNetworkInterface.TabIndex = 0;
            // 
            // tabPageRtmp
            // 
            this.tabPageRtmp.Controls.Add(this.groupBoxRtmpCommonParameters);
            this.tabPageRtmp.Location = new System.Drawing.Point(4, 22);
            this.tabPageRtmp.Name = "tabPageRtmp";
            this.tabPageRtmp.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageRtmp.Size = new System.Drawing.Size(690, 507);
            this.tabPageRtmp.TabIndex = 2;
            this.tabPageRtmp.Text = "RTMP (Real Time Messaging Protocol)";
            this.tabPageRtmp.UseVisualStyleBackColor = true;
            // 
            // groupBoxRtmpCommonParameters
            // 
            this.groupBoxRtmpCommonParameters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxRtmpCommonParameters.Controls.Add(this.label17);
            this.groupBoxRtmpCommonParameters.Controls.Add(this.label18);
            this.groupBoxRtmpCommonParameters.Controls.Add(this.label19);
            this.groupBoxRtmpCommonParameters.Controls.Add(this.labelRtmpTotalReopenConnectionTimeout);
            this.groupBoxRtmpCommonParameters.Controls.Add(this.labelRtmpOpenConnectionSleepTime);
            this.groupBoxRtmpCommonParameters.Controls.Add(this.textBoxRtmpTotalReopenConnectionTimeout);
            this.groupBoxRtmpCommonParameters.Controls.Add(this.textBoxRtmpOpenConnectionSleepTime);
            this.groupBoxRtmpCommonParameters.Controls.Add(this.textBoxRtmpOpenConnectionTimeout);
            this.groupBoxRtmpCommonParameters.Controls.Add(this.labelRtmpOpenConnectionTimeout);
            this.groupBoxRtmpCommonParameters.Controls.Add(this.labelRtmpNetworkInterface);
            this.groupBoxRtmpCommonParameters.Controls.Add(this.comboBoxRtmpPreferredNetworkInterface);
            this.groupBoxRtmpCommonParameters.Location = new System.Drawing.Point(6, 10);
            this.groupBoxRtmpCommonParameters.Name = "groupBoxRtmpCommonParameters";
            this.groupBoxRtmpCommonParameters.Size = new System.Drawing.Size(678, 129);
            this.groupBoxRtmpCommonParameters.TabIndex = 2;
            this.groupBoxRtmpCommonParameters.TabStop = false;
            this.groupBoxRtmpCommonParameters.Text = "Common configuration parameters for RTMP protocol";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(278, 101);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(63, 13);
            this.label17.TabIndex = 10;
            this.label17.Text = "milliseconds";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(278, 75);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(63, 13);
            this.label18.TabIndex = 9;
            this.label18.Text = "milliseconds";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(278, 49);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(63, 13);
            this.label19.TabIndex = 8;
            this.label19.Text = "milliseconds";
            // 
            // labelRtmpTotalReopenConnectionTimeout
            // 
            this.labelRtmpTotalReopenConnectionTimeout.AutoSize = true;
            this.labelRtmpTotalReopenConnectionTimeout.Location = new System.Drawing.Point(6, 101);
            this.labelRtmpTotalReopenConnectionTimeout.Name = "labelRtmpTotalReopenConnectionTimeout";
            this.labelRtmpTotalReopenConnectionTimeout.Size = new System.Drawing.Size(160, 13);
            this.labelRtmpTotalReopenConnectionTimeout.TabIndex = 7;
            this.labelRtmpTotalReopenConnectionTimeout.Text = "Total reopen connection timeout";
            // 
            // labelRtmpOpenConnectionSleepTime
            // 
            this.labelRtmpOpenConnectionSleepTime.AutoSize = true;
            this.labelRtmpOpenConnectionSleepTime.Location = new System.Drawing.Point(6, 75);
            this.labelRtmpOpenConnectionSleepTime.Name = "labelRtmpOpenConnectionSleepTime";
            this.labelRtmpOpenConnectionSleepTime.Size = new System.Drawing.Size(139, 13);
            this.labelRtmpOpenConnectionSleepTime.TabIndex = 6;
            this.labelRtmpOpenConnectionSleepTime.Text = "Open connection sleep time";
            // 
            // textBoxRtmpTotalReopenConnectionTimeout
            // 
            this.textBoxRtmpTotalReopenConnectionTimeout.Location = new System.Drawing.Point(172, 98);
            this.textBoxRtmpTotalReopenConnectionTimeout.Name = "textBoxRtmpTotalReopenConnectionTimeout";
            this.textBoxRtmpTotalReopenConnectionTimeout.Size = new System.Drawing.Size(100, 20);
            this.textBoxRtmpTotalReopenConnectionTimeout.TabIndex = 5;
            // 
            // textBoxRtmpOpenConnectionSleepTime
            // 
            this.textBoxRtmpOpenConnectionSleepTime.Location = new System.Drawing.Point(172, 72);
            this.textBoxRtmpOpenConnectionSleepTime.Name = "textBoxRtmpOpenConnectionSleepTime";
            this.textBoxRtmpOpenConnectionSleepTime.Size = new System.Drawing.Size(100, 20);
            this.textBoxRtmpOpenConnectionSleepTime.TabIndex = 4;
            // 
            // textBoxRtmpOpenConnectionTimeout
            // 
            this.textBoxRtmpOpenConnectionTimeout.Location = new System.Drawing.Point(172, 46);
            this.textBoxRtmpOpenConnectionTimeout.Name = "textBoxRtmpOpenConnectionTimeout";
            this.textBoxRtmpOpenConnectionTimeout.Size = new System.Drawing.Size(100, 20);
            this.textBoxRtmpOpenConnectionTimeout.TabIndex = 3;
            // 
            // labelRtmpOpenConnectionTimeout
            // 
            this.labelRtmpOpenConnectionTimeout.AutoSize = true;
            this.labelRtmpOpenConnectionTimeout.Location = new System.Drawing.Point(6, 49);
            this.labelRtmpOpenConnectionTimeout.Name = "labelRtmpOpenConnectionTimeout";
            this.labelRtmpOpenConnectionTimeout.Size = new System.Drawing.Size(126, 13);
            this.labelRtmpOpenConnectionTimeout.TabIndex = 2;
            this.labelRtmpOpenConnectionTimeout.Text = "Open connection timeout";
            // 
            // labelRtmpNetworkInterface
            // 
            this.labelRtmpNetworkInterface.AutoSize = true;
            this.labelRtmpNetworkInterface.Location = new System.Drawing.Point(6, 22);
            this.labelRtmpNetworkInterface.Name = "labelRtmpNetworkInterface";
            this.labelRtmpNetworkInterface.Size = new System.Drawing.Size(135, 13);
            this.labelRtmpNetworkInterface.TabIndex = 1;
            this.labelRtmpNetworkInterface.Text = "Preferred network interface";
            // 
            // comboBoxRtmpPreferredNetworkInterface
            // 
            this.comboBoxRtmpPreferredNetworkInterface.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRtmpPreferredNetworkInterface.FormattingEnabled = true;
            this.comboBoxRtmpPreferredNetworkInterface.Location = new System.Drawing.Point(172, 19);
            this.comboBoxRtmpPreferredNetworkInterface.Name = "comboBoxRtmpPreferredNetworkInterface";
            this.comboBoxRtmpPreferredNetworkInterface.Size = new System.Drawing.Size(500, 21);
            this.comboBoxRtmpPreferredNetworkInterface.TabIndex = 0;
            // 
            // tabPageRtsp
            // 
            this.tabPageRtsp.Controls.Add(this.groupBoxRtspCommonParameters);
            this.tabPageRtsp.Location = new System.Drawing.Point(4, 22);
            this.tabPageRtsp.Name = "tabPageRtsp";
            this.tabPageRtsp.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageRtsp.Size = new System.Drawing.Size(690, 507);
            this.tabPageRtsp.TabIndex = 3;
            this.tabPageRtsp.Text = "RTSP (Real Time Streaming Protocol)";
            this.tabPageRtsp.UseVisualStyleBackColor = true;
            // 
            // groupBoxRtspCommonParameters
            // 
            this.groupBoxRtspCommonParameters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxRtspCommonParameters.Controls.Add(this.label59);
            this.groupBoxRtspCommonParameters.Controls.Add(this.textBoxRtspClientPortMax);
            this.groupBoxRtspCommonParameters.Controls.Add(this.labelRtspConnectionRange);
            this.groupBoxRtspCommonParameters.Controls.Add(this.textBoxRtspClientPortMin);
            this.groupBoxRtspCommonParameters.Controls.Add(this.label26);
            this.groupBoxRtspCommonParameters.Controls.Add(this.label27);
            this.groupBoxRtspCommonParameters.Controls.Add(this.label28);
            this.groupBoxRtspCommonParameters.Controls.Add(this.labelRtspTotalReopenConnectionTimeout);
            this.groupBoxRtspCommonParameters.Controls.Add(this.labelRtspOpenConnectionSleepTime);
            this.groupBoxRtspCommonParameters.Controls.Add(this.textBoxRtspTotalReopenConnectionTimeout);
            this.groupBoxRtspCommonParameters.Controls.Add(this.textBoxRtspOpenConnectionSleepTime);
            this.groupBoxRtspCommonParameters.Controls.Add(this.textBoxRtspOpenConnectionTimeout);
            this.groupBoxRtspCommonParameters.Controls.Add(this.labelRtspOpenConnectionTimeout);
            this.groupBoxRtspCommonParameters.Controls.Add(this.labelRtspNetworkInterface);
            this.groupBoxRtspCommonParameters.Controls.Add(this.comboBoxRtspPreferredNetworkInterface);
            this.groupBoxRtspCommonParameters.Location = new System.Drawing.Point(6, 10);
            this.groupBoxRtspCommonParameters.Name = "groupBoxRtspCommonParameters";
            this.groupBoxRtspCommonParameters.Size = new System.Drawing.Size(678, 151);
            this.groupBoxRtspCommonParameters.TabIndex = 3;
            this.groupBoxRtspCommonParameters.TabStop = false;
            this.groupBoxRtspCommonParameters.Text = "Common configuration parameters for RTSP protocol";
            // 
            // label59
            // 
            this.label59.AutoSize = true;
            this.label59.Location = new System.Drawing.Point(278, 127);
            this.label59.Name = "label59";
            this.label59.Size = new System.Drawing.Size(10, 13);
            this.label59.TabIndex = 16;
            this.label59.Text = "-";
            // 
            // textBoxRtspClientPortMax
            // 
            this.textBoxRtspClientPortMax.Location = new System.Drawing.Point(294, 124);
            this.textBoxRtspClientPortMax.Name = "textBoxRtspClientPortMax";
            this.textBoxRtspClientPortMax.Size = new System.Drawing.Size(100, 20);
            this.textBoxRtspClientPortMax.TabIndex = 15;
            // 
            // labelRtspConnectionRange
            // 
            this.labelRtspConnectionRange.AutoSize = true;
            this.labelRtspConnectionRange.Location = new System.Drawing.Point(6, 127);
            this.labelRtspConnectionRange.Name = "labelRtspConnectionRange";
            this.labelRtspConnectionRange.Size = new System.Drawing.Size(84, 13);
            this.labelRtspConnectionRange.TabIndex = 14;
            this.labelRtspConnectionRange.Text = "Client port range";
            // 
            // textBoxRtspClientPortMin
            // 
            this.textBoxRtspClientPortMin.Location = new System.Drawing.Point(172, 124);
            this.textBoxRtspClientPortMin.Name = "textBoxRtspClientPortMin";
            this.textBoxRtspClientPortMin.Size = new System.Drawing.Size(100, 20);
            this.textBoxRtspClientPortMin.TabIndex = 13;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(278, 101);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(63, 13);
            this.label26.TabIndex = 10;
            this.label26.Text = "milliseconds";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(278, 75);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(63, 13);
            this.label27.TabIndex = 9;
            this.label27.Text = "milliseconds";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(278, 49);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(63, 13);
            this.label28.TabIndex = 8;
            this.label28.Text = "milliseconds";
            // 
            // labelRtspTotalReopenConnectionTimeout
            // 
            this.labelRtspTotalReopenConnectionTimeout.AutoSize = true;
            this.labelRtspTotalReopenConnectionTimeout.Location = new System.Drawing.Point(6, 101);
            this.labelRtspTotalReopenConnectionTimeout.Name = "labelRtspTotalReopenConnectionTimeout";
            this.labelRtspTotalReopenConnectionTimeout.Size = new System.Drawing.Size(160, 13);
            this.labelRtspTotalReopenConnectionTimeout.TabIndex = 7;
            this.labelRtspTotalReopenConnectionTimeout.Text = "Total reopen connection timeout";
            // 
            // labelRtspOpenConnectionSleepTime
            // 
            this.labelRtspOpenConnectionSleepTime.AutoSize = true;
            this.labelRtspOpenConnectionSleepTime.Location = new System.Drawing.Point(6, 75);
            this.labelRtspOpenConnectionSleepTime.Name = "labelRtspOpenConnectionSleepTime";
            this.labelRtspOpenConnectionSleepTime.Size = new System.Drawing.Size(139, 13);
            this.labelRtspOpenConnectionSleepTime.TabIndex = 6;
            this.labelRtspOpenConnectionSleepTime.Text = "Open connection sleep time";
            // 
            // textBoxRtspTotalReopenConnectionTimeout
            // 
            this.textBoxRtspTotalReopenConnectionTimeout.Location = new System.Drawing.Point(172, 98);
            this.textBoxRtspTotalReopenConnectionTimeout.Name = "textBoxRtspTotalReopenConnectionTimeout";
            this.textBoxRtspTotalReopenConnectionTimeout.Size = new System.Drawing.Size(100, 20);
            this.textBoxRtspTotalReopenConnectionTimeout.TabIndex = 5;
            // 
            // textBoxRtspOpenConnectionSleepTime
            // 
            this.textBoxRtspOpenConnectionSleepTime.Location = new System.Drawing.Point(172, 72);
            this.textBoxRtspOpenConnectionSleepTime.Name = "textBoxRtspOpenConnectionSleepTime";
            this.textBoxRtspOpenConnectionSleepTime.Size = new System.Drawing.Size(100, 20);
            this.textBoxRtspOpenConnectionSleepTime.TabIndex = 4;
            // 
            // textBoxRtspOpenConnectionTimeout
            // 
            this.textBoxRtspOpenConnectionTimeout.Location = new System.Drawing.Point(172, 46);
            this.textBoxRtspOpenConnectionTimeout.Name = "textBoxRtspOpenConnectionTimeout";
            this.textBoxRtspOpenConnectionTimeout.Size = new System.Drawing.Size(100, 20);
            this.textBoxRtspOpenConnectionTimeout.TabIndex = 3;
            // 
            // labelRtspOpenConnectionTimeout
            // 
            this.labelRtspOpenConnectionTimeout.AutoSize = true;
            this.labelRtspOpenConnectionTimeout.Location = new System.Drawing.Point(6, 49);
            this.labelRtspOpenConnectionTimeout.Name = "labelRtspOpenConnectionTimeout";
            this.labelRtspOpenConnectionTimeout.Size = new System.Drawing.Size(126, 13);
            this.labelRtspOpenConnectionTimeout.TabIndex = 2;
            this.labelRtspOpenConnectionTimeout.Text = "Open connection timeout";
            // 
            // labelRtspNetworkInterface
            // 
            this.labelRtspNetworkInterface.AutoSize = true;
            this.labelRtspNetworkInterface.Location = new System.Drawing.Point(6, 22);
            this.labelRtspNetworkInterface.Name = "labelRtspNetworkInterface";
            this.labelRtspNetworkInterface.Size = new System.Drawing.Size(135, 13);
            this.labelRtspNetworkInterface.TabIndex = 1;
            this.labelRtspNetworkInterface.Text = "Preferred network interface";
            // 
            // comboBoxRtspPreferredNetworkInterface
            // 
            this.comboBoxRtspPreferredNetworkInterface.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRtspPreferredNetworkInterface.FormattingEnabled = true;
            this.comboBoxRtspPreferredNetworkInterface.Location = new System.Drawing.Point(172, 19);
            this.comboBoxRtspPreferredNetworkInterface.Name = "comboBoxRtspPreferredNetworkInterface";
            this.comboBoxRtspPreferredNetworkInterface.Size = new System.Drawing.Size(500, 21);
            this.comboBoxRtspPreferredNetworkInterface.TabIndex = 0;
            // 
            // tabPageUdpRtp
            // 
            this.tabPageUdpRtp.Controls.Add(this.groupBoxUdpRtpCommonParameters);
            this.tabPageUdpRtp.Location = new System.Drawing.Point(4, 22);
            this.tabPageUdpRtp.Name = "tabPageUdpRtp";
            this.tabPageUdpRtp.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageUdpRtp.Size = new System.Drawing.Size(690, 507);
            this.tabPageUdpRtp.TabIndex = 4;
            this.tabPageUdpRtp.Text = "UDP or RTP";
            this.tabPageUdpRtp.UseVisualStyleBackColor = true;
            // 
            // groupBoxUdpRtpCommonParameters
            // 
            this.groupBoxUdpRtpCommonParameters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxUdpRtpCommonParameters.Controls.Add(this.label56);
            this.groupBoxUdpRtpCommonParameters.Controls.Add(this.labelUdpRtpReceiveDataCheckInterval);
            this.groupBoxUdpRtpCommonParameters.Controls.Add(this.textBoxUdpRtpReceiveDataCheckInterval);
            this.groupBoxUdpRtpCommonParameters.Controls.Add(this.label37);
            this.groupBoxUdpRtpCommonParameters.Controls.Add(this.label54);
            this.groupBoxUdpRtpCommonParameters.Controls.Add(this.label55);
            this.groupBoxUdpRtpCommonParameters.Controls.Add(this.labelUdpRtpTotalReopenConnectionTimeout);
            this.groupBoxUdpRtpCommonParameters.Controls.Add(this.labelUdpRtpOpenConnectionSleepTime);
            this.groupBoxUdpRtpCommonParameters.Controls.Add(this.textBoxUdpRtpTotalReopenConnectionTimeout);
            this.groupBoxUdpRtpCommonParameters.Controls.Add(this.textBoxUdpRtpOpenConnectionSleepTime);
            this.groupBoxUdpRtpCommonParameters.Controls.Add(this.textBoxUdpRtpOpenConnectionTimeout);
            this.groupBoxUdpRtpCommonParameters.Controls.Add(this.labelUdpRtpOpenConnectionTimeout);
            this.groupBoxUdpRtpCommonParameters.Controls.Add(this.labelUdpRtpNetworkInterface);
            this.groupBoxUdpRtpCommonParameters.Controls.Add(this.comboBoxUdpRtpPreferredNetworkInterface);
            this.groupBoxUdpRtpCommonParameters.Location = new System.Drawing.Point(6, 10);
            this.groupBoxUdpRtpCommonParameters.Name = "groupBoxUdpRtpCommonParameters";
            this.groupBoxUdpRtpCommonParameters.Size = new System.Drawing.Size(678, 153);
            this.groupBoxUdpRtpCommonParameters.TabIndex = 2;
            this.groupBoxUdpRtpCommonParameters.TabStop = false;
            this.groupBoxUdpRtpCommonParameters.Text = "Common configuration parameters for UDP or RTP protocol";
            // 
            // label56
            // 
            this.label56.AutoSize = true;
            this.label56.Location = new System.Drawing.Point(278, 127);
            this.label56.Name = "label56";
            this.label56.Size = new System.Drawing.Size(63, 13);
            this.label56.TabIndex = 13;
            this.label56.Text = "milliseconds";
            // 
            // labelUdpRtpReceiveDataCheckInterval
            // 
            this.labelUdpRtpReceiveDataCheckInterval.AutoSize = true;
            this.labelUdpRtpReceiveDataCheckInterval.Location = new System.Drawing.Point(6, 127);
            this.labelUdpRtpReceiveDataCheckInterval.Name = "labelUdpRtpReceiveDataCheckInterval";
            this.labelUdpRtpReceiveDataCheckInterval.Size = new System.Drawing.Size(141, 13);
            this.labelUdpRtpReceiveDataCheckInterval.TabIndex = 12;
            this.labelUdpRtpReceiveDataCheckInterval.Text = "Receive data check interval";
            // 
            // textBoxUdpRtpReceiveDataCheckInterval
            // 
            this.textBoxUdpRtpReceiveDataCheckInterval.Location = new System.Drawing.Point(172, 124);
            this.textBoxUdpRtpReceiveDataCheckInterval.Name = "textBoxUdpRtpReceiveDataCheckInterval";
            this.textBoxUdpRtpReceiveDataCheckInterval.Size = new System.Drawing.Size(100, 20);
            this.textBoxUdpRtpReceiveDataCheckInterval.TabIndex = 11;
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(278, 101);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(63, 13);
            this.label37.TabIndex = 10;
            this.label37.Text = "milliseconds";
            // 
            // label54
            // 
            this.label54.AutoSize = true;
            this.label54.Location = new System.Drawing.Point(278, 75);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(63, 13);
            this.label54.TabIndex = 9;
            this.label54.Text = "milliseconds";
            // 
            // label55
            // 
            this.label55.AutoSize = true;
            this.label55.Location = new System.Drawing.Point(278, 49);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(63, 13);
            this.label55.TabIndex = 8;
            this.label55.Text = "milliseconds";
            // 
            // labelUdpRtpTotalReopenConnectionTimeout
            // 
            this.labelUdpRtpTotalReopenConnectionTimeout.AutoSize = true;
            this.labelUdpRtpTotalReopenConnectionTimeout.Location = new System.Drawing.Point(6, 101);
            this.labelUdpRtpTotalReopenConnectionTimeout.Name = "labelUdpRtpTotalReopenConnectionTimeout";
            this.labelUdpRtpTotalReopenConnectionTimeout.Size = new System.Drawing.Size(160, 13);
            this.labelUdpRtpTotalReopenConnectionTimeout.TabIndex = 7;
            this.labelUdpRtpTotalReopenConnectionTimeout.Text = "Total reopen connection timeout";
            // 
            // labelUdpRtpOpenConnectionSleepTime
            // 
            this.labelUdpRtpOpenConnectionSleepTime.AutoSize = true;
            this.labelUdpRtpOpenConnectionSleepTime.Location = new System.Drawing.Point(6, 75);
            this.labelUdpRtpOpenConnectionSleepTime.Name = "labelUdpRtpOpenConnectionSleepTime";
            this.labelUdpRtpOpenConnectionSleepTime.Size = new System.Drawing.Size(139, 13);
            this.labelUdpRtpOpenConnectionSleepTime.TabIndex = 6;
            this.labelUdpRtpOpenConnectionSleepTime.Text = "Open connection sleep time";
            // 
            // textBoxUdpRtpTotalReopenConnectionTimeout
            // 
            this.textBoxUdpRtpTotalReopenConnectionTimeout.Location = new System.Drawing.Point(172, 98);
            this.textBoxUdpRtpTotalReopenConnectionTimeout.Name = "textBoxUdpRtpTotalReopenConnectionTimeout";
            this.textBoxUdpRtpTotalReopenConnectionTimeout.Size = new System.Drawing.Size(100, 20);
            this.textBoxUdpRtpTotalReopenConnectionTimeout.TabIndex = 5;
            // 
            // textBoxUdpRtpOpenConnectionSleepTime
            // 
            this.textBoxUdpRtpOpenConnectionSleepTime.Location = new System.Drawing.Point(172, 72);
            this.textBoxUdpRtpOpenConnectionSleepTime.Name = "textBoxUdpRtpOpenConnectionSleepTime";
            this.textBoxUdpRtpOpenConnectionSleepTime.Size = new System.Drawing.Size(100, 20);
            this.textBoxUdpRtpOpenConnectionSleepTime.TabIndex = 4;
            // 
            // textBoxUdpRtpOpenConnectionTimeout
            // 
            this.textBoxUdpRtpOpenConnectionTimeout.Location = new System.Drawing.Point(172, 46);
            this.textBoxUdpRtpOpenConnectionTimeout.Name = "textBoxUdpRtpOpenConnectionTimeout";
            this.textBoxUdpRtpOpenConnectionTimeout.Size = new System.Drawing.Size(100, 20);
            this.textBoxUdpRtpOpenConnectionTimeout.TabIndex = 3;
            // 
            // labelUdpRtpOpenConnectionTimeout
            // 
            this.labelUdpRtpOpenConnectionTimeout.AutoSize = true;
            this.labelUdpRtpOpenConnectionTimeout.Location = new System.Drawing.Point(6, 49);
            this.labelUdpRtpOpenConnectionTimeout.Name = "labelUdpRtpOpenConnectionTimeout";
            this.labelUdpRtpOpenConnectionTimeout.Size = new System.Drawing.Size(126, 13);
            this.labelUdpRtpOpenConnectionTimeout.TabIndex = 2;
            this.labelUdpRtpOpenConnectionTimeout.Text = "Open connection timeout";
            // 
            // labelUdpRtpNetworkInterface
            // 
            this.labelUdpRtpNetworkInterface.AutoSize = true;
            this.labelUdpRtpNetworkInterface.Location = new System.Drawing.Point(6, 22);
            this.labelUdpRtpNetworkInterface.Name = "labelUdpRtpNetworkInterface";
            this.labelUdpRtpNetworkInterface.Size = new System.Drawing.Size(135, 13);
            this.labelUdpRtpNetworkInterface.TabIndex = 1;
            this.labelUdpRtpNetworkInterface.Text = "Preferred network interface";
            // 
            // comboBoxUdpRtpPreferredNetworkInterface
            // 
            this.comboBoxUdpRtpPreferredNetworkInterface.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxUdpRtpPreferredNetworkInterface.FormattingEnabled = true;
            this.comboBoxUdpRtpPreferredNetworkInterface.Location = new System.Drawing.Point(172, 19);
            this.comboBoxUdpRtpPreferredNetworkInterface.Name = "comboBoxUdpRtpPreferredNetworkInterface";
            this.comboBoxUdpRtpPreferredNetworkInterface.Size = new System.Drawing.Size(500, 21);
            this.comboBoxUdpRtpPreferredNetworkInterface.TabIndex = 0;
            // 
            // tabPageQuality
            // 
            this.tabPageQuality.Controls.Add(this.groupBox8);
            this.tabPageQuality.Controls.Add(this.groupBox6);
            this.tabPageQuality.Controls.Add(this.groupBox7);
            this.tabPageQuality.Controls.Add(this.groupBox1);
            this.tabPageQuality.Location = new System.Drawing.Point(4, 22);
            this.tabPageQuality.Name = "tabPageQuality";
            this.tabPageQuality.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageQuality.Size = new System.Drawing.Size(704, 542);
            this.tabPageQuality.TabIndex = 7;
            this.tabPageQuality.Text = "Quality Options";
            this.tabPageQuality.UseVisualStyleBackColor = true;
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.checkBoxAutoVideoSelection);
            this.groupBox8.Controls.Add(this.comboBoxVideoResolution);
            this.groupBox8.Controls.Add(this.checkBoxAllowHDR);
            this.groupBox8.Controls.Add(this.checkBoxAllow3D);
            this.groupBox8.Controls.Add(this.label11);
            this.groupBox8.Location = new System.Drawing.Point(8, 16);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(447, 90);
            this.groupBox8.TabIndex = 13;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Video Quality";
            // 
            // checkBoxAutoVideoSelection
            // 
            this.checkBoxAutoVideoSelection.AutoSize = true;
            this.checkBoxAutoVideoSelection.Location = new System.Drawing.Point(22, 28);
            this.checkBoxAutoVideoSelection.Name = "checkBoxAutoVideoSelection";
            this.checkBoxAutoVideoSelection.Size = new System.Drawing.Size(203, 17);
            this.checkBoxAutoVideoSelection.TabIndex = 4;
            this.checkBoxAutoVideoSelection.Text = "Automatic Video Resolution Selection";
            this.checkBoxAutoVideoSelection.UseVisualStyleBackColor = true;
            // 
            // comboBoxVideoResolution
            // 
            this.comboBoxVideoResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxVideoResolution.FormattingEnabled = true;
            this.comboBoxVideoResolution.Location = new System.Drawing.Point(158, 49);
            this.comboBoxVideoResolution.Name = "comboBoxVideoResolution";
            this.comboBoxVideoResolution.Size = new System.Drawing.Size(67, 21);
            this.comboBoxVideoResolution.TabIndex = 3;
            // 
            // checkBoxAllowHDR
            // 
            this.checkBoxAllowHDR.AutoSize = true;
            this.checkBoxAllowHDR.Location = new System.Drawing.Point(341, 53);
            this.checkBoxAllowHDR.Name = "checkBoxAllowHDR";
            this.checkBoxAllowHDR.Size = new System.Drawing.Size(78, 17);
            this.checkBoxAllowHDR.TabIndex = 5;
            this.checkBoxAllowHDR.Text = "Allow HDR";
            this.checkBoxAllowHDR.UseVisualStyleBackColor = true;
            // 
            // checkBoxAllow3D
            // 
            this.checkBoxAllow3D.AutoSize = true;
            this.checkBoxAllow3D.Location = new System.Drawing.Point(341, 30);
            this.checkBoxAllow3D.Name = "checkBoxAllow3D";
            this.checkBoxAllow3D.Size = new System.Drawing.Size(68, 17);
            this.checkBoxAllow3D.TabIndex = 6;
            this.checkBoxAllow3D.Text = "Allow 3D";
            this.checkBoxAllow3D.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(19, 55);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(133, 13);
            this.label11.TabIndex = 7;
            this.label11.Text = "Preferred Video Resolution";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.preferredListControlAudioCodec);
            this.groupBox6.Location = new System.Drawing.Point(310, 112);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(145, 200);
            this.groupBox6.TabIndex = 12;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Preffered Audio Codecs";
            // 
            // preferredListControlAudioCodec
            // 
            this.preferredListControlAudioCodec.Location = new System.Drawing.Point(22, 18);
            this.preferredListControlAudioCodec.Name = "preferredListControlAudioCodec";
            this.preferredListControlAudioCodec.Size = new System.Drawing.Size(103, 176);
            this.preferredListControlAudioCodec.TabIndex = 2;
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.preferredListControlVideoCodec);
            this.groupBox7.Location = new System.Drawing.Point(159, 112);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(145, 200);
            this.groupBox7.TabIndex = 12;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Preffered Video Codecs";
            // 
            // preferredListControlVideoCodec
            // 
            this.preferredListControlVideoCodec.Location = new System.Drawing.Point(22, 18);
            this.preferredListControlVideoCodec.Name = "preferredListControlVideoCodec";
            this.preferredListControlVideoCodec.Size = new System.Drawing.Size(103, 176);
            this.preferredListControlVideoCodec.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.preferredListControlContainer);
            this.groupBox1.Location = new System.Drawing.Point(8, 112);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(145, 200);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Preffered Containers";
            // 
            // preferredListControlContainer
            // 
            this.preferredListControlContainer.Location = new System.Drawing.Point(22, 18);
            this.preferredListControlContainer.Name = "preferredListControlContainer";
            this.preferredListControlContainer.Size = new System.Drawing.Size(103, 176);
            this.preferredListControlContainer.TabIndex = 0;
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Image File|*.jpg;*.jpeg;*.png;*.gif";
            // 
            // Configuration
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(712, 568);
            this.Controls.Add(this.mainTabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Configuration";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "OnlineVideos Configuration";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigurationFormClosing);
            this.Load += new System.EventHandler(this.Configuration_Load);
            siteNameIconPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.iconSite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.siteList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceSiteSettings)).EndInit();
            this.mainTabControl.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).EndInit();
            this.groupBoxLatestVideos.ResumeLayout(false);
            this.groupBoxLatestVideos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUPSearchHistoryItemCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.Thumbnails.ResumeLayout(false);
            this.Thumbnails.PerformLayout();
            this.tabGroups.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceSitesGroup)).EndInit();
            this.toolStripContainer4.ContentPanel.ResumeLayout(false);
            this.toolStripContainer4.LeftToolStripPanel.ResumeLayout(false);
            this.toolStripContainer4.LeftToolStripPanel.PerformLayout();
            this.toolStripContainer4.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer4.TopToolStripPanel.PerformLayout();
            this.toolStripContainer4.ResumeLayout(false);
            this.toolStripContainer4.PerformLayout();
            this.toolStripSitesGroupLeft.ResumeLayout(false);
            this.toolStripSitesGroupLeft.PerformLayout();
            this.toolStripSitesGroupTop.ResumeLayout(false);
            this.toolStripSitesGroupTop.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.tabSites.ResumeLayout(false);
            this.tabSites.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.toolStripSites.ResumeLayout(false);
            this.toolStripSites.PerformLayout();
            this.tabHosters.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tabSourceFilter.ResumeLayout(false);
            this.tabProtocols.ResumeLayout(false);
            this.tabPageNotDetectedFilter.ResumeLayout(false);
            this.tabPageNotDetectedFilter.PerformLayout();
            this.tabPageHttp.ResumeLayout(false);
            this.groupBoxHttpProxyServerAuthentication.ResumeLayout(false);
            this.groupBoxHttpProxyServerAuthentication.PerformLayout();
            this.groupBoxHttpServerAuthentication.ResumeLayout(false);
            this.groupBoxHttpServerAuthentication.PerformLayout();
            this.groupBoxHttpCommonParameters.ResumeLayout(false);
            this.groupBoxHttpCommonParameters.PerformLayout();
            this.tabPageRtmp.ResumeLayout(false);
            this.groupBoxRtmpCommonParameters.ResumeLayout(false);
            this.groupBoxRtmpCommonParameters.PerformLayout();
            this.tabPageRtsp.ResumeLayout(false);
            this.groupBoxRtspCommonParameters.ResumeLayout(false);
            this.groupBoxRtspCommonParameters.PerformLayout();
            this.tabPageUdpRtp.ResumeLayout(false);
            this.groupBoxUdpRtpCommonParameters.ResumeLayout(false);
            this.groupBoxUdpRtpCommonParameters.PerformLayout();
            this.tabPageQuality.ResumeLayout(false);
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);

        }
		private System.Windows.Forms.TextBox txtDownloadDir;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtFilters;
		private System.Windows.Forms.TextBox txtThumbLoc;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		//private System.Windows.Forms.TabPage General_Tab;
		//private System.Windows.Forms.TabControl tabControl1;
		
		void CheckBox1CheckedChanged(object sender, System.EventArgs e)
		{
			
		}

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox chkUseAgeConfirmation;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox tbxPin;
        private System.Windows.Forms.TextBox tbxScreenName;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnBrowseForDlFolder;
        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.BindingSource bindingSourceSiteSettings;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.PropertyGrid propertyGridUserConfig;
        private System.Windows.Forms.TextBox tbxWebCacheTimeout;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.TextBox tbxUtilTimeout;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox tbxWMPBuffer;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.CheckBox chkDoAutoUpdate;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.DomainUpDown udPlayBuffer;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.CheckBox chkUseQuickSelect;
        private System.Windows.Forms.Button bntBrowseFolderForThumbs;
        private System.Windows.Forms.GroupBox Thumbnails;
        private System.Windows.Forms.TextBox tbxThumbAge;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.RadioButton rbExtendedSearchHistory;
        private System.Windows.Forms.RadioButton rbLastSearch;
        private System.Windows.Forms.RadioButton rbOff;
        private System.Windows.Forms.NumericUpDown nUPSearchHistoryItemCount;
        private System.Windows.Forms.Label label38;
        private System.Windows.Forms.TabPage tabGroups;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripContainer toolStripContainer4;
        private System.Windows.Forms.ToolStrip toolStripSitesGroupLeft;
        private System.Windows.Forms.ToolStripButton btnSitesGroupUp;
        private System.Windows.Forms.ToolStripButton btnSitesGroupDown;
        private System.Windows.Forms.ToolStrip toolStripSitesGroupTop;
        private System.Windows.Forms.ToolStripButton btnAddSitesGroup;
        private System.Windows.Forms.ToolStripButton btnDeleteSitesGroup;
        private System.Windows.Forms.ListBox listBoxSitesGroups;
        private System.Windows.Forms.TextBox tbxSitesGroupName;
        private System.Windows.Forms.Label label39;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ListView listViewSitesNotInGroup;
        private System.Windows.Forms.ListView listViewSitesInGroup;
        private System.Windows.Forms.Label label40;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.BindingSource bindingSourceSitesGroup;
        private System.Windows.Forms.TextBox tbxSitesGroupThumb;
        private System.Windows.Forms.Label label42;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.TextBox tbxSitesGroupDesc;
        private System.Windows.Forms.Label label43;
        private System.Windows.Forms.Button btnBrowseSitesGroupThumb;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.CheckBox chkAutoGroupByLang;
        private System.Windows.Forms.Button btnWiki;
        private System.Windows.Forms.TextBox tbxUpdatePeriod;
        private System.Windows.Forms.Label label45;
        private System.Windows.Forms.Label label44;
        private System.Windows.Forms.CheckBox chkFavFirst;
        private System.Windows.Forms.TabPage tabHosters;
        private System.Windows.Forms.ListBox listBoxHosters;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.PropertyGrid propertyGridHoster;
		private System.Windows.Forms.Label label48;
		private System.Windows.Forms.Label label47;
		private System.Windows.Forms.TextBox tbxCategoriesTimeout;
		private System.Windows.Forms.GroupBox groupBoxLatestVideos;
		private System.Windows.Forms.Label label51;
		private System.Windows.Forms.Label label50;
		private System.Windows.Forms.Label label49;
		private System.Windows.Forms.PictureBox pictureBox6;
		private System.Windows.Forms.Label label53;
		private System.Windows.Forms.Label label52;
		private System.Windows.Forms.TextBox tbxLatestVideosGuiRefresh;
		private System.Windows.Forms.TextBox tbxLatestVideosOnlineRefresh;
		private System.Windows.Forms.TextBox tbxLatestVideosAmount;
		private System.Windows.Forms.CheckBox chkLatestVideosRandomize;
        private System.Windows.Forms.TabPage tabSites;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStrip toolStripSites;
        private BrightIdeasSoftware.DataListView siteList;
        private System.Windows.Forms.ImageList imageListSiteIcons;
        private System.Windows.Forms.Splitter splitter2;
        private System.Windows.Forms.PictureBox iconSite;
        private System.Windows.Forms.Label lblSelectedSite;
        private System.Windows.Forms.ToolStripButton btnDeleteSite;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownBtnImport;
        private System.Windows.Forms.ToolStripMenuItem btnImportXml;
        private System.Windows.Forms.ToolStripMenuItem btnImportGlobal;
        private System.Windows.Forms.ToolStripButton btnAddSite;
        private System.Windows.Forms.ToolStripButton btnPublishSite;
        private System.Windows.Forms.ToolStripButton btnReportSite;
        private System.Windows.Forms.ToolStripButton btnCreateSite;
        private System.Windows.Forms.ToolStripButton btnSiteUp;
        private System.Windows.Forms.ToolStripButton btnSiteDown;
        private System.Windows.Forms.ToolStripButton btnEditSite;
        private BrightIdeasSoftware.OLVColumn siteColumnLanguage;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkAdaptRefreshRate;
		private System.Windows.Forms.CheckBox chkStoreLayoutPerCategory;
		private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label sourceLabel;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TabPage tabSourceFilter;
        private System.Windows.Forms.TabControl tabProtocols;
        private System.Windows.Forms.TabPage tabPageNotDetectedFilter;
        private System.Windows.Forms.TabPage tabPageHttp;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.LinkLabel linkLabelFilterDownload;
        private System.Windows.Forms.TabPage tabPageRtmp;
        private System.Windows.Forms.TabPage tabPageRtsp;
        private System.Windows.Forms.TabPage tabPageUdpRtp;
        private System.Windows.Forms.GroupBox groupBoxHttpCommonParameters;
        private System.Windows.Forms.Label labelHttpNetworkInterface;
        private System.Windows.Forms.ComboBox comboBoxHttpPreferredNetworkInterface;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label labelHttpTotalReopenConnectionTimeout;
        private System.Windows.Forms.Label labelHttpOpenConnectionSleepTime;
        private System.Windows.Forms.TextBox textBoxHttpTotalReopenConnectionTimeout;
        private System.Windows.Forms.TextBox textBoxHttpOpenConnectionSleepTime;
        private System.Windows.Forms.TextBox textBoxHttpOpenConnectionTimeout;
        private System.Windows.Forms.Label labelHttpOpenConnectionTimeout;
        private System.Windows.Forms.GroupBox groupBoxRtmpCommonParameters;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label labelRtmpTotalReopenConnectionTimeout;
        private System.Windows.Forms.Label labelRtmpOpenConnectionSleepTime;
        private System.Windows.Forms.TextBox textBoxRtmpTotalReopenConnectionTimeout;
        private System.Windows.Forms.TextBox textBoxRtmpOpenConnectionSleepTime;
        private System.Windows.Forms.TextBox textBoxRtmpOpenConnectionTimeout;
        private System.Windows.Forms.Label labelRtmpOpenConnectionTimeout;
        private System.Windows.Forms.Label labelRtmpNetworkInterface;
        private System.Windows.Forms.ComboBox comboBoxRtmpPreferredNetworkInterface;
        private System.Windows.Forms.GroupBox groupBoxRtspCommonParameters;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label labelRtspTotalReopenConnectionTimeout;
        private System.Windows.Forms.Label labelRtspOpenConnectionSleepTime;
        private System.Windows.Forms.TextBox textBoxRtspTotalReopenConnectionTimeout;
        private System.Windows.Forms.TextBox textBoxRtspOpenConnectionSleepTime;
        private System.Windows.Forms.TextBox textBoxRtspOpenConnectionTimeout;
        private System.Windows.Forms.Label labelRtspOpenConnectionTimeout;
        private System.Windows.Forms.Label labelRtspNetworkInterface;
        private System.Windows.Forms.ComboBox comboBoxRtspPreferredNetworkInterface;
        private System.Windows.Forms.GroupBox groupBoxUdpRtpCommonParameters;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.Label label54;
        private System.Windows.Forms.Label label55;
        private System.Windows.Forms.Label labelUdpRtpTotalReopenConnectionTimeout;
        private System.Windows.Forms.Label labelUdpRtpOpenConnectionSleepTime;
        private System.Windows.Forms.TextBox textBoxUdpRtpTotalReopenConnectionTimeout;
        private System.Windows.Forms.TextBox textBoxUdpRtpOpenConnectionSleepTime;
        private System.Windows.Forms.TextBox textBoxUdpRtpOpenConnectionTimeout;
        private System.Windows.Forms.Label labelUdpRtpOpenConnectionTimeout;
        private System.Windows.Forms.Label labelUdpRtpNetworkInterface;
        private System.Windows.Forms.ComboBox comboBoxUdpRtpPreferredNetworkInterface;
        private System.Windows.Forms.Label label56;
        private System.Windows.Forms.Label labelUdpRtpReceiveDataCheckInterval;
        private System.Windows.Forms.TextBox textBoxUdpRtpReceiveDataCheckInterval;
        private System.Windows.Forms.Label label59;
        private System.Windows.Forms.TextBox textBoxRtspClientPortMax;
        private System.Windows.Forms.Label labelRtspConnectionRange;
        private System.Windows.Forms.TextBox textBoxRtspClientPortMin;
        private System.Windows.Forms.GroupBox groupBoxHttpProxyServerAuthentication;
        private System.Windows.Forms.CheckBox checkBoxHttpProxyServerAuthentication;
        private System.Windows.Forms.Label labelHttpProxyType;
        private System.Windows.Forms.ComboBox comboBoxHttpProxyServerType;
        private System.Windows.Forms.Label labelHttpProxyServerPassword;
        private System.Windows.Forms.Label labelHttpProxyServerUserName;
        private System.Windows.Forms.Label labelHttpProxyServerPort;
        private System.Windows.Forms.Label labelHttpProxyServer;
        private System.Windows.Forms.TextBox textBoxHttpProxyServerPassword;
        private System.Windows.Forms.TextBox textBoxHttpProxyServerUserName;
        private System.Windows.Forms.TextBox textBoxHttpProxyServerPort;
        private System.Windows.Forms.TextBox textBoxHttpProxyServer;
        private System.Windows.Forms.GroupBox groupBoxHttpServerAuthentication;
        private System.Windows.Forms.CheckBox checkBoxHttpServerAuthentication;
        private System.Windows.Forms.Label labelHttpServerPassword;
        private System.Windows.Forms.TextBox textBoxHttpServerPassword;
        private System.Windows.Forms.TextBox textBoxHttpServerUserName;
        private System.Windows.Forms.Label labelHttpServerUserName;
        private System.Windows.Forms.CheckBox chkUseMPUrlSourceSplitter;
        private System.Windows.Forms.TabPage tabPageQuality;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox checkBoxAllow3D;
        private System.Windows.Forms.CheckBox checkBoxAllowHDR;
        private System.Windows.Forms.CheckBox checkBoxAutoVideoSelection;
        private System.Windows.Forms.ComboBox comboBoxVideoResolution;
        private PreferredListControl preferredListControlAudioCodec;
        private PreferredListControl preferredListControlVideoCodec;
        private PreferredListControl preferredListControlContainer;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox8;
    }
}
