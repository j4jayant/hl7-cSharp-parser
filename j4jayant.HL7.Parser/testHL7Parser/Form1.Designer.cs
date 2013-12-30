namespace testHL7Parser
{
    partial class Form1
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
            this.txtResult = new System.Windows.Forms.RichTextBox();
            this.btnGetPID3 = new System.Windows.Forms.Button();
            this.btnAddPV110 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtResult
            // 
            this.txtResult.Location = new System.Drawing.Point(13, 13);
            this.txtResult.Name = "txtResult";
            this.txtResult.Size = new System.Drawing.Size(720, 271);
            this.txtResult.TabIndex = 0;
            this.txtResult.Text = "";
            // 
            // btnGetPID3
            // 
            this.btnGetPID3.Location = new System.Drawing.Point(13, 291);
            this.btnGetPID3.Name = "btnGetPID3";
            this.btnGetPID3.Size = new System.Drawing.Size(75, 23);
            this.btnGetPID3.TabIndex = 1;
            this.btnGetPID3.Text = "Get PID-3";
            this.btnGetPID3.UseVisualStyleBackColor = true;
            // 
            // btnAddPV110
            // 
            this.btnAddPV110.Location = new System.Drawing.Point(95, 291);
            this.btnAddPV110.Name = "btnAddPV110";
            this.btnAddPV110.Size = new System.Drawing.Size(75, 23);
            this.btnAddPV110.TabIndex = 2;
            this.btnAddPV110.Text = "Add PV1-10";
            this.btnAddPV110.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(851, 373);
            this.Controls.Add(this.btnAddPV110);
            this.Controls.Add(this.btnGetPID3);
            this.Controls.Add(this.txtResult);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox txtResult;
        private System.Windows.Forms.Button btnGetPID3;
        private System.Windows.Forms.Button btnAddPV110;
    }
}

