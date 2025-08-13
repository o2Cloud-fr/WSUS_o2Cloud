namespace WSUS_o2Cloud
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnSearchUpdates;
        private System.Windows.Forms.Button btnInstallUpdates;
        private System.Windows.Forms.TextBox txtDetails;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelBottom;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                updateManager?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnSearchUpdates = new System.Windows.Forms.Button();
            this.btnInstallUpdates = new System.Windows.Forms.Button();
            this.txtDetails = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panelTop = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.panelTop.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSearchUpdates
            // 
            this.btnSearchUpdates.BackColor = System.Drawing.Color.DodgerBlue;
            this.btnSearchUpdates.FlatAppearance.BorderSize = 0;
            this.btnSearchUpdates.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearchUpdates.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSearchUpdates.ForeColor = System.Drawing.Color.White;
            this.btnSearchUpdates.Location = new System.Drawing.Point(20, 80);
            this.btnSearchUpdates.Name = "btnSearchUpdates";
            this.btnSearchUpdates.Size = new System.Drawing.Size(180, 40);
            this.btnSearchUpdates.TabIndex = 2;
            this.btnSearchUpdates.Text = "🔍 Rechercher les MAJ";
            this.btnSearchUpdates.UseVisualStyleBackColor = false;
            this.btnSearchUpdates.Click += new System.EventHandler(this.btnSearchUpdates_Click);
            // 
            // btnInstallUpdates
            // 
            this.btnInstallUpdates.BackColor = System.Drawing.Color.ForestGreen;
            this.btnInstallUpdates.Enabled = false;
            this.btnInstallUpdates.FlatAppearance.BorderSize = 0;
            this.btnInstallUpdates.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInstallUpdates.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnInstallUpdates.ForeColor = System.Drawing.Color.White;
            this.btnInstallUpdates.Location = new System.Drawing.Point(220, 80);
            this.btnInstallUpdates.Name = "btnInstallUpdates";
            this.btnInstallUpdates.Size = new System.Drawing.Size(180, 40);
            this.btnInstallUpdates.TabIndex = 3;
            this.btnInstallUpdates.Text = "⬇️ Installer les MAJ";
            this.btnInstallUpdates.UseVisualStyleBackColor = false;
            this.btnInstallUpdates.Click += new System.EventHandler(this.btnInstallUpdates_Click);
            // 
            // txtDetails
            // 
            this.txtDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDetails.BackColor = System.Drawing.Color.White;
            this.txtDetails.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDetails.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtDetails.ForeColor = System.Drawing.Color.Black;
            this.txtDetails.Location = new System.Drawing.Point(20, 140);
            this.txtDetails.Multiline = true;
            this.txtDetails.Name = "txtDetails";
            this.txtDetails.ReadOnly = true;
            this.txtDetails.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDetails.Size = new System.Drawing.Size(744, 340);
            this.txtDetails.TabIndex = 4;
            this.txtDetails.Text = "Prêt à rechercher les mises à jour...\r\n";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 10);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(520, 23);
            this.progressBar.TabIndex = 5;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStatus.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblStatus.Location = new System.Drawing.Point(550, 10);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(220, 23);
            this.lblStatus.TabIndex = 6;
            this.lblStatus.Text = "Prêt";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblTitle.Location = new System.Drawing.Point(12, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(412, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Gestionnaire de Mises à Jour Windows";
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.LightGray;
            this.panelTop.Controls.Add(this.lblTitle);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(784, 60);
            this.panelTop.TabIndex = 1;
            // 
            // panelBottom
            // 
            this.panelBottom.BackColor = System.Drawing.Color.LightGray;
            this.panelBottom.Controls.Add(this.progressBar);
            this.panelBottom.Controls.Add(this.lblStatus);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 501);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(784, 45);
            this.panelBottom.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(784, 546);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.txtDetails);
            this.Controls.Add(this.btnInstallUpdates);
            this.Controls.Add(this.btnSearchUpdates);
            this.Controls.Add(this.panelTop);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(800, 585);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WSUS o2Cloud - Gestionnaire de Mises à Jour";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}