
namespace SCADA_WAREHOUSE
{
    partial class FirebaseDataForm
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
            this.components = new System.ComponentModel.Container();
            this.dtgDSHS = new System.Windows.Forms.DataGridView();
            this.btnRetrieveData = new System.Windows.Forms.Button();
            this.btnAddDB = new System.Windows.Forms.Button();
            this.lbStatus = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnTestConnection = new System.Windows.Forms.Button();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dtgDSHS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // dtgDSHS
            // 
            this.dtgDSHS.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtgDSHS.Location = new System.Drawing.Point(698, 434);
            this.dtgDSHS.Name = "dtgDSHS";
            this.dtgDSHS.RowHeadersWidth = 51;
            this.dtgDSHS.RowTemplate.Height = 29;
            this.dtgDSHS.Size = new System.Drawing.Size(530, 83);
            this.dtgDSHS.TabIndex = 25;
            // 
            // btnRetrieveData
            // 
            this.btnRetrieveData.Location = new System.Drawing.Point(670, 271);
            this.btnRetrieveData.Name = "btnRetrieveData";
            this.btnRetrieveData.Size = new System.Drawing.Size(120, 29);
            this.btnRetrieveData.TabIndex = 24;
            this.btnRetrieveData.Text = "Retrieve Data";
            this.btnRetrieveData.UseVisualStyleBackColor = true;
            // 
            // btnAddDB
            // 
            this.btnAddDB.Location = new System.Drawing.Point(503, 271);
            this.btnAddDB.Name = "btnAddDB";
            this.btnAddDB.Size = new System.Drawing.Size(120, 29);
            this.btnAddDB.TabIndex = 23;
            this.btnAddDB.Text = "Add Database";
            this.btnAddDB.UseVisualStyleBackColor = true;
            // 
            // lbStatus
            // 
            this.lbStatus.AutoSize = true;
            this.lbStatus.Location = new System.Drawing.Point(94, 47);
            this.lbStatus.Name = "lbStatus";
            this.lbStatus.Size = new System.Drawing.Size(99, 20);
            this.lbStatus.TabIndex = 22;
            this.lbStatus.Text = "Disconnected";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 20);
            this.label3.TabIndex = 21;
            this.label3.Text = "Status:";
            // 
            // btnTestConnection
            // 
            this.btnTestConnection.Location = new System.Drawing.Point(221, 43);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new System.Drawing.Size(141, 29);
            this.btnTestConnection.TabIndex = 13;
            this.btnTestConnection.Text = "Test Connection";
            this.btnTestConnection.UseVisualStyleBackColor = true;
            // 
            // FirebaseDataForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1902, 1033);
            this.Controls.Add(this.dtgDSHS);
            this.Controls.Add(this.btnRetrieveData);
            this.Controls.Add(this.btnAddDB);
            this.Controls.Add(this.lbStatus);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnTestConnection);
            this.Name = "FirebaseDataForm";
            this.Text = "Firebase Data";
            this.Load += new System.EventHandler(this.FirebaseDataForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dtgDSHS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dtgDSHS;
        private System.Windows.Forms.Button btnRetrieveData;
        private System.Windows.Forms.Button btnAddDB;
        private System.Windows.Forms.Label lbStatus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtBoxValue;
        private System.Windows.Forms.Button btnWrite;
        private System.Windows.Forms.Button btnRead;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBoxAdress;
        private System.Windows.Forms.Button btnTestConnection;
        private System.Windows.Forms.BindingSource bindingSource1;
    }
}