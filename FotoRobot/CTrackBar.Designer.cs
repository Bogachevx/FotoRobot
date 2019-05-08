namespace FotoRobot
{
    partial class CTrackBar
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.textValue = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.trackValue = new System.Windows.Forms.TrackBar();
            this.trackLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackValue)).BeginInit();
            this.SuspendLayout();
            // 
            // textValue
            // 
            this.textValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textValue.Location = new System.Drawing.Point(161, 23);
            this.textValue.Name = "textValue";
            this.textValue.Size = new System.Drawing.Size(54, 20);
            this.textValue.TabIndex = 0;
            this.textValue.Text = "0";
            this.textValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textValue.TextChanged += new System.EventHandler(this.textValue_TextChanged);
            this.textValue.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textValue_KeyDown);
            this.textValue.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textValue_KeyPress);
            this.textValue.MouseLeave += new System.EventHandler(this.textValue_MouseLeave);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.Controls.Add(this.textValue, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.trackValue, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.trackLabel, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.MaximumSize = new System.Drawing.Size(0, 45);
            this.tableLayoutPanel1.MinimumSize = new System.Drawing.Size(0, 45);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(218, 45);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // trackValue
            // 
            this.trackValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trackValue.Location = new System.Drawing.Point(3, 23);
            this.trackValue.Name = "trackValue";
            this.trackValue.Size = new System.Drawing.Size(152, 24);
            this.trackValue.TabIndex = 1;
            this.trackValue.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackValue.Value = 3;
            this.trackValue.ValueChanged += new System.EventHandler(this.trackValue_ValueChanged);
            // 
            // trackLabel
            // 
            this.trackLabel.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.trackLabel, 2);
            this.trackLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trackLabel.Location = new System.Drawing.Point(3, 0);
            this.trackLabel.Name = "trackLabel";
            this.trackLabel.Size = new System.Drawing.Size(212, 20);
            this.trackLabel.TabIndex = 2;
            this.trackLabel.Text = "trackLabel";
            this.trackLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CTrackBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximumSize = new System.Drawing.Size(100000000, 45);
            this.MinimumSize = new System.Drawing.Size(0, 45);
            this.Name = "CTrackBar";
            this.Size = new System.Drawing.Size(218, 45);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackValue)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textValue;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TrackBar trackValue;
        private System.Windows.Forms.Label trackLabel;

    }
}
