// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Forms\BaseForm.cs

using System;
using System.Drawing;
using System.Windows.Forms;

namespace HungryFastFoodAdmin.Forms
{
    public class BaseForm : Form
    {
        public BaseForm()
        {
            ApplyBaseStyling();
        }

        private void ApplyBaseStyling()
        {
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = Color.FromArgb(250, 249, 246); // Cream background #FAF9F6
            this.ForeColor = Color.FromArgb(53, 57, 59); // Text Primary #35393b

            // Load and apply the custom window and taskbar logo icon dynamically
            try
            {
                if (System.IO.File.Exists("logo.ico"))
                {
                    this.Icon = new Icon("logo.ico");
                }
            }
            catch
            {
                // Fallback silently if logo.ico is missing or corrupt
            }

            this.Load += (s, e) => ApplyControlStyles(this);
        }

        public static void ApplyControlStyles(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl.HasChildren)
                {
                    ApplyControlStyles(ctrl);
                }

                if (ctrl is Button btn)
                {
                    // Set standard button styling if not custom painted
                    if (btn.FlatStyle != FlatStyle.Flat)
                    {
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderSize = 0;
                        btn.BackColor = Color.FromArgb(253, 175, 38); // Golden Orange #FDAF26
                        btn.ForeColor = Color.White;
                    }
                    btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                }
                else if (ctrl is TextBox txt)
                {
                    txt.Font = new Font("Segoe UI", 10F);
                }
                else if (ctrl is ComboBox cmb)
                {
                    cmb.Font = new Font("Segoe UI", 10F);
                }
                else if (ctrl is Label lbl)
                {
                    if (lbl.Font.Size >= 14)
                    {
                        lbl.Font = new Font("Segoe UI", lbl.Font.Size, FontStyle.Bold);
                        lbl.ForeColor = Color.FromArgb(53, 57, 59); // Primary Text #35393b
                    }
                    else
                    {
                        lbl.Font = new Font("Segoe UI", lbl.Font.Size);
                    }
                }
                else if (ctrl is DataGridView dgv)
                {
                    dgv.Font = new Font("Segoe UI", 9.5f);
                    dgv.BackgroundColor = Color.White;
                    dgv.BorderStyle = BorderStyle.None;
                    dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                    dgv.RowHeadersVisible = false;
                    dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 249, 246); // Alternating Cream
                    
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(253, 175, 38); // Golden Orange header #FDAF26
                    dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(253, 175, 38);
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                    dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(254, 234, 196); // Light orange select
                    dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(53, 57, 59);
                    dgv.EnableHeadersVisualStyles = false;
                }
            }
        }
    }
}
