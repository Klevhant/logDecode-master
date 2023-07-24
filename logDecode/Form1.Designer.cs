namespace logDecode
{
    partial class logDecodeForm
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
        	this.btnOpenFile = new System.Windows.Forms.Button();
        	this.cbMapList = new System.Windows.Forms.ComboBox();
        	this.btnRefreshMap = new System.Windows.Forms.Button();
        	this.SuspendLayout();
        	// 
        	// btnOpenFile
        	// 
        	this.btnOpenFile.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.btnOpenFile.Location = new System.Drawing.Point(12, 54);
        	this.btnOpenFile.Name = "btnOpenFile";
        	this.btnOpenFile.Size = new System.Drawing.Size(95, 34);
        	this.btnOpenFile.TabIndex = 0;
        	this.btnOpenFile.Text = "Open File";
        	this.btnOpenFile.UseVisualStyleBackColor = true;
        	this.btnOpenFile.Click += new System.EventHandler(this.btnOpenFile_Click);
        	// 
        	// cbMapList
        	// 
        	this.cbMapList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        	this.cbMapList.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.cbMapList.FormattingEnabled = true;
        	this.cbMapList.Location = new System.Drawing.Point(12, 12);
        	this.cbMapList.Name = "cbMapList";
        	this.cbMapList.Size = new System.Drawing.Size(300, 27);
        	this.cbMapList.TabIndex = 1;
        	// 
        	// btnRefreshMap
        	// 
        	this.btnRefreshMap.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.btnRefreshMap.Location = new System.Drawing.Point(217, 54);
        	this.btnRefreshMap.Name = "btnRefreshMap";
        	this.btnRefreshMap.Size = new System.Drawing.Size(95, 34);
        	this.btnRefreshMap.TabIndex = 0;
        	this.btnRefreshMap.Text = "Refresh";
        	this.btnRefreshMap.UseVisualStyleBackColor = true;
        	this.btnRefreshMap.Click += new System.EventHandler(this.btnOpenFile_Click);
        	// 
        	// logDecodeForm
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.ClientSize = new System.Drawing.Size(624, 261);
        	this.Controls.Add(this.cbMapList);
        	this.Controls.Add(this.btnRefreshMap);
        	this.Controls.Add(this.btnOpenFile);
        	this.Name = "logDecodeForm";
        	this.Text = "logDecode";
        	this.Load += new System.EventHandler(this.logDecodeForm_Load);
        	this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.ComboBox cbMapList;
        private System.Windows.Forms.Button btnRefreshMap;
    }
}

