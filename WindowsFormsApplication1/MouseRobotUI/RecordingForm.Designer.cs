namespace MouseRobotUI
{
    partial class RecordingForm
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
            this.programLabel = new System.Windows.Forms.Label();
            this.countTextBox = new System.Windows.Forms.TextBox();
            this.stopScript = new System.Windows.Forms.Button();
            this.startScript = new System.Windows.Forms.Button();
            this.loadButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.emptyButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.programLabel.AutoSize = true;
            this.programLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.programLabel.Location = new System.Drawing.Point(27, 15);
            this.programLabel.Name = "label1";
            this.programLabel.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.programLabel.Size = new System.Drawing.Size(100, 24);
            this.programLabel.TabIndex = 0;
            this.programLabel.Text = "Repeat for:";
            // 
            // textBox1
            // 
            this.countTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.countTextBox.Location = new System.Drawing.Point(133, 12);
            this.countTextBox.Name = "textBox1";
            this.countTextBox.Size = new System.Drawing.Size(100, 26);
            this.countTextBox.TabIndex = 11;
            this.countTextBox.Text = "10";
            // 
            // button2
            // 
            this.stopScript.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stopScript.Location = new System.Drawing.Point(31, 211);
            this.stopScript.Name = "button2";
            this.stopScript.Size = new System.Drawing.Size(113, 39);
            this.stopScript.TabIndex = 15;
            this.stopScript.Text = "Stop script";
            this.stopScript.UseVisualStyleBackColor = true;
            this.stopScript.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // button1
            // 
            this.startScript.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startScript.Location = new System.Drawing.Point(150, 211);
            this.startScript.Name = "button1";
            this.startScript.Size = new System.Drawing.Size(113, 39);
            this.startScript.TabIndex = 16;
            this.startScript.Text = "Start script";
            this.startScript.UseVisualStyleBackColor = true;
            this.startScript.Click += new System.EventHandler(this.startButton_Click);
            // 
            // button3
            // 
            this.loadButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loadButton.Location = new System.Drawing.Point(359, 121);
            this.loadButton.Name = "button3";
            this.loadButton.Size = new System.Drawing.Size(113, 39);
            this.loadButton.TabIndex = 17;
            this.loadButton.Text = "Load script";
            this.loadButton.UseVisualStyleBackColor = true;
            this.loadButton.Click += new System.EventHandler(this.loadButton_Click);
            // 
            // button4
            // 
            this.saveButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.saveButton.Location = new System.Drawing.Point(359, 166);
            this.saveButton.Name = "button4";
            this.saveButton.Size = new System.Drawing.Size(113, 39);
            this.saveButton.TabIndex = 18;
            this.saveButton.Text = "Save script";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // textBox2
            // 
            this.descriptionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.descriptionTextBox.Location = new System.Drawing.Point(31, 42);
            this.descriptionTextBox.Multiline = true;
            this.descriptionTextBox.Name = "textBox2";
            this.descriptionTextBox.Size = new System.Drawing.Size(344, 55);
            this.descriptionTextBox.TabIndex = 19;
            this.descriptionTextBox.Text = "Move mouse postion to top left corner to start recording and to top right corner " +
    "to stop recording. Use buttons below to start and stop the script.";
            // 
            // button5
            // 
            this.emptyButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.emptyButton.Location = new System.Drawing.Point(359, 211);
            this.emptyButton.Name = "button5";
            this.emptyButton.Size = new System.Drawing.Size(113, 39);
            this.emptyButton.TabIndex = 20;
            this.emptyButton.Text = "Empty script";
            this.emptyButton.UseVisualStyleBackColor = true;
            this.emptyButton.Click += new System.EventHandler(this.emptyButton_Click);
            // 
            // MainForm2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 262);
            this.Controls.Add(this.emptyButton);
            this.Controls.Add(this.descriptionTextBox);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.loadButton);
            this.Controls.Add(this.startScript);
            this.Controls.Add(this.stopScript);
            this.Controls.Add(this.countTextBox);
            this.Controls.Add(this.programLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm2";
            this.Text = "Mouse Robot";
            this.Load += new System.EventHandler(this.stopButton_Click);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label programLabel;
        private System.Windows.Forms.TextBox countTextBox;
        private System.Windows.Forms.Button stopScript;
        private System.Windows.Forms.Button startScript;
        private System.Windows.Forms.Button loadButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.Button emptyButton;
    }
}

