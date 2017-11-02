namespace RobotEditor
{
    partial class ScreenPreviewWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScreenPreviewWindow));
            this.pictureDrawBox = new RobotEditor.CustomControls.PictureDrawBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureDrawBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureDrawBox
            // 
            this.pictureDrawBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureDrawBox.Image = null;
            this.pictureDrawBox.ImagePosition = ((System.Drawing.PointF)(resources.GetObject("pictureDrawBox.ImagePosition")));
            this.pictureDrawBox.Location = new System.Drawing.Point(0, 0);
            this.pictureDrawBox.Name = "pictureDrawBox";
            this.pictureDrawBox.Size = new System.Drawing.Size(430, 406);
            this.pictureDrawBox.TabIndex = 0;
            this.pictureDrawBox.TabStop = false;
            // 
            // ScreenPreviewWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 406);
            this.Controls.Add(this.pictureDrawBox);
            this.Name = "ScreenPreviewWindow";
            this.Text = "Image Preview";
            ((System.ComponentModel.ISupportInitialize)(this.pictureDrawBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private CustomControls.PictureDrawBox pictureDrawBox;
    }
}