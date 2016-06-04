namespace InteropSample
{
  partial class WinForm
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
      this.PresentationTarget0 = new DigitalRune.Graphics.Interop.FormsPresentationTarget();
      this.ClearButton = new System.Windows.Forms.Button();
      this.TextBox = new System.Windows.Forms.TextBox();
      this.PresentationTarget1 = new DigitalRune.Graphics.Interop.FormsPresentationTarget();
      this.SuspendLayout();
      // 
      // PresentationTarget0
      // 
      this.PresentationTarget0.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.PresentationTarget0.Location = new System.Drawing.Point(168, 12);
      this.PresentationTarget0.Name = "PresentationTarget0";
      this.PresentationTarget0.Size = new System.Drawing.Size(504, 232);
      this.PresentationTarget0.TabIndex = 0;
      // 
      // ClearButton
      // 
      this.ClearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.ClearButton.Location = new System.Drawing.Point(12, 427);
      this.ClearButton.Name = "ClearButton";
      this.ClearButton.Size = new System.Drawing.Size(150, 23);
      this.ClearButton.TabIndex = 1;
      this.ClearButton.Text = "Clear TextBox";
      this.ClearButton.UseVisualStyleBackColor = true;
      this.ClearButton.Click += new System.EventHandler(this.OnClearButtonClick);
      // 
      // TextBox
      // 
      this.TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.TextBox.Location = new System.Drawing.Point(12, 12);
      this.TextBox.Multiline = true;
      this.TextBox.Name = "TextBox";
      this.TextBox.Size = new System.Drawing.Size(150, 409);
      this.TextBox.TabIndex = 2;
      // 
      // PresentationTarget1
      // 
      this.PresentationTarget1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.PresentationTarget1.Location = new System.Drawing.Point(168, 250);
      this.PresentationTarget1.Name = "PresentationTarget1";
      this.PresentationTarget1.Size = new System.Drawing.Size(504, 200);
      this.PresentationTarget1.TabIndex = 3;
      this.PresentationTarget1.Text = "PresentationTarget1";
      // 
      // WinForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(684, 462);
      this.Controls.Add(this.PresentationTarget1);
      this.Controls.Add(this.TextBox);
      this.Controls.Add(this.ClearButton);
      this.Controls.Add(this.PresentationTarget0);
      this.Name = "WinForm";
      this.Text = "WinForm";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private DigitalRune.Graphics.Interop.FormsPresentationTarget PresentationTarget0;
    private System.Windows.Forms.Button ClearButton;
    private System.Windows.Forms.TextBox TextBox;
    private DigitalRune.Graphics.Interop.FormsPresentationTarget PresentationTarget1;
  }
}