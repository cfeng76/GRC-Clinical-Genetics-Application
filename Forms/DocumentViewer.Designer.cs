namespace GRC_Clinical_Genetics_Application
{
    partial class DocumentViewer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.DocumentListComboBox = new System.Windows.Forms.ComboBox();
            this.OpenButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "Document Name:";
            // 
            // DocumentListComboBox
            // 
            this.DocumentListComboBox.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.DocumentListComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
            this.DocumentListComboBox.FormattingEnabled = true;
            this.DocumentListComboBox.Location = new System.Drawing.Point(139, 12);
            this.DocumentListComboBox.Name = "DocumentListComboBox";
            this.DocumentListComboBox.Size = new System.Drawing.Size(260, 150);
            this.DocumentListComboBox.TabIndex = 1;
            // 
            // OpenButton
            // 
            this.OpenButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.OpenButton.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.OpenButton.Location = new System.Drawing.Point(139, 168);
            this.OpenButton.Name = "OpenButton";
            this.OpenButton.Size = new System.Drawing.Size(260, 23);
            this.OpenButton.TabIndex = 2;
            this.OpenButton.Text = "Open";
            this.OpenButton.UseVisualStyleBackColor = false;
            this.OpenButton.Click += new System.EventHandler(this.OpenButton_Click);
            // 
            // DocumentViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(433, 233);
            this.Controls.Add(this.OpenButton);
            this.Controls.Add(this.DocumentListComboBox);
            this.Controls.Add(this.label1);
            this.Name = "DocumentViewer";
            this.Text = "Documents Viewer";
            this.Load += new System.EventHandler(this.DocumentViewer_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox DocumentListComboBox;
        private System.Windows.Forms.Button OpenButton;
    }
}