// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Forms\SyncLogsForm.cs

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HungryFastFoodAdmin.Services;

namespace HungryFastFoodAdmin.Forms
{
    public class SyncLogsForm : Form
    {
        private ComboBox cmbStatus;
        private DateTimePicker dtpStart;
        private DateTimePicker dtpEnd;
        private TextBox txtSearch;
        private DataGridView dgvLogs;
        private Button btnRefresh;
        private Button btnRetryFailed;
        private Button btnClearCompleted;
        private DatabaseService _dbService;

        public SyncLogsForm()
        {
            _dbService = new DatabaseService();
            InitializeComponent();
            StyleControls();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1000, 600);
            this.Text = "📜 Synchronization Logs Viewer";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Top Filter Panel
            Panel pnlFilters = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(240, 238, 233),
                Padding = new Padding(15)
            };

            Label lblStatus = new Label { Text = "Status:", Location = new Point(15, 27), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            cmbStatus = new ComboBox
            {
                Location = new Point(70, 24),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f)
            };
            cmbStatus.Items.AddRange(new string[] { "All", "Pending", "Synced", "Failed" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += FilterChanged;

            Label lblDate = new Point(210, 27).CreateLabel("Date Range:", pnlFilters);
            dtpStart = new DateTimePicker
            {
                Location = new Point(295, 24),
                Width = 120,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddDays(-7),
                Font = new Font("Segoe UI", 9.5f)
            };
            dtpStart.ValueChanged += FilterChanged;

            Label lblTo = new Point(425, 27).CreateLabel("to", pnlFilters);
            dtpEnd = new DateTimePicker
            {
                Location = new Point(450, 24),
                Width = 120,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now,
                Font = new Font("Segoe UI", 9.5f)
            };
            dtpEnd.ValueChanged += FilterChanged;

            Label lblSearch = new Point(590, 27).CreateLabel("Search Entity:", pnlFilters);
            txtSearch = new TextBox
            {
                Location = new Point(685, 24),
                Width = 150,
                Font = new Font("Segoe UI", 9.5f)
            };
            txtSearch.TextChanged += FilterChanged;

            pnlFilters.Controls.Add(lblStatus);
            pnlFilters.Controls.Add(cmbStatus);
            pnlFilters.Controls.Add(dtpStart);
            pnlFilters.Controls.Add(dtpEnd);
            pnlFilters.Controls.Add(txtSearch);

            // Grid Panel
            dgvLogs = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvLogs.Columns.Add("OpType", "Operation Type");
            dgvLogs.Columns.Add("EntityType", "Entity Type");
            dgvLogs.Columns.Add("EntityId", "Entity ID/Key");
            dgvLogs.Columns.Add("Status", "Status");
            dgvLogs.Columns.Add("CreatedAt", "Created At");
            dgvLogs.Columns.Add("SyncedAt", "Synced At");
            dgvLogs.Columns.Add("Error", "Error Message");
            dgvLogs.DoubleClick += DgvLogs_DoubleClick;

            // Bottom Actions Panel
            Panel pnlActions = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            btnRefresh = new Button { Text = "🔄 Refresh", Location = new Point(15, 12), Width = 110, Height = 40 };
            btnRefresh.Click += (s, e) => LoadData();

            btnRetryFailed = new Button { Text = "🔁 Retry Failed", Location = new Point(140, 12), Width = 140, Height = 40 };
            btnRetryFailed.Click += BtnRetryFailed_Click;

            btnClearCompleted = new Button { Text = "🧹 Clear Synced (>30d)", Location = new Point(295, 12), Width = 190, Height = 40 };
            btnClearCompleted.Click += BtnClearCompleted_Click;

            Button btnClose = new Button { Text = "Close", Location = new Point(875, 12), Width = 100, Height = 40, DialogResult = DialogResult.Cancel };

            pnlActions.Controls.Add(btnRefresh);
            pnlActions.Controls.Add(btnRetryFailed);
            pnlActions.Controls.Add(btnClearCompleted);
            pnlActions.Controls.Add(btnClose);

            this.Controls.Add(dgvLogs);
            this.Controls.Add(pnlFilters);
            this.Controls.Add(pnlActions);
        }

        private void StyleControls()
        {
            this.Font = new Font("Segoe UI", 10F);
            
            // Buttons Styling
            btnRefresh.BackColor = Color.FromArgb(42, 157, 143); // Teal
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

            btnRetryFailed.BackColor = Color.FromArgb(244, 162, 97); // Orange #F4A261
            btnRetryFailed.ForeColor = Color.White;
            btnRetryFailed.FlatStyle = FlatStyle.Flat;
            btnRetryFailed.FlatAppearance.BorderSize = 0;
            btnRetryFailed.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

            btnClearCompleted.BackColor = Color.FromArgb(108, 117, 125); // Gray
            btnClearCompleted.ForeColor = Color.White;
            btnClearCompleted.FlatStyle = FlatStyle.Flat;
            btnClearCompleted.FlatAppearance.BorderSize = 0;
            btnClearCompleted.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

            // DataGridView Header styling
            dgvLogs.EnableHeadersVisualStyles = false;
            dgvLogs.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(230, 57, 70); // Red
            dgvLogs.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvLogs.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvLogs.RowTemplate.Height = 28;
            dgvLogs.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
        }

        private void LoadData()
        {
            dgvLogs.Rows.Clear();

            string status = cmbStatus.SelectedItem?.ToString();
            string start = dtpStart.Value.ToString("yyyy-MM-dd");
            string end = dtpEnd.Value.ToString("yyyy-MM-dd");
            string search = txtSearch.Text.Trim();

            var logs = _dbService.GetAllSyncLogs(status, start, end, search);

            foreach (var item in logs)
            {
                int index = dgvLogs.Rows.Add(
                    item.OperationType.ToUpper(),
                    item.EntityType.ToUpper(),
                    item.EntityId,
                    GetStyledStatus(item.Status),
                    item.CreatedAt ?? "",
                    item.SyncedAt ?? "",
                    item.ErrorMessage ?? ""
                );

                // Highlight failed logs
                if (item.Status.Equals("failed", StringComparison.OrdinalIgnoreCase))
                {
                    dgvLogs.Rows[index].DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230);
                    dgvLogs.Rows[index].DefaultCellStyle.ForeColor = Color.FromArgb(180, 0, 0);
                }
            }
        }

        private string GetStyledStatus(string status)
        {
            if (string.IsNullOrEmpty(status)) return "";
            if (status.Equals("synced", StringComparison.OrdinalIgnoreCase)) return "Synced ✅";
            if (status.Equals("failed", StringComparison.OrdinalIgnoreCase)) return "Failed ❌";
            return "Pending ⏳";
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void BtnRetryFailed_Click(object sender, EventArgs e)
        {
            try
            {
                _dbService.RetryFailedSyncs();
                MessageBox.Show("All failed sync operations have been reset to 'pending' state.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to retry syncs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClearCompleted_Click(object sender, EventArgs e)
        {
            try
            {
                _dbService.CleanSyncLogs(30);
                MessageBox.Show("Cleared completed sync logs older than 30 days.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to clear logs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvLogs_DoubleClick(object sender, EventArgs e)
        {
            if (dgvLogs.SelectedRows.Count == 0) return;
            var row = dgvLogs.SelectedRows[0];
            string error = row.Cells["Error"].Value?.ToString();

            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show(error, "Sync Log Error Details", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public static class ControlExtensions
    {
        public static Label CreateLabel(this Point point, string text, Control parent)
        {
            Label lbl = new Label
            {
                Text = text,
                Location = point,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            parent.Controls.Add(lbl);
            return lbl;
        }
    }
}
