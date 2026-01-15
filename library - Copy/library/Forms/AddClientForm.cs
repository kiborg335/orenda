using SportsRental.Helpers;
using SportsRental.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SportsRental.Forms
{
    public partial class AddClientForm : Form
    {
        private SqlHelper sqlHelper;

        public event EventHandler ClientAdded;

        private TextBox txtFullName;
        private TextBox txtPhone;
        private TextBox txtEmail;
        private Button btnAdd;
        private Button btnCancel;

        public AddClientForm(SqlHelper sqlHelper)
        {
            this.sqlHelper = sqlHelper;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Додати клієнта";
            this.Size = new Size(350, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            var lblFullName = new Label { Text = "ПІБ:", Location = new Point(20, 20), Size = new Size(80, 20) };
            var lblPhone = new Label { Text = "Телефон:", Location = new Point(20, 50), Size = new Size(80, 20) };
            var lblEmail = new Label { Text = "Email:", Location = new Point(20, 80), Size = new Size(80, 20) };

            txtFullName = new TextBox { Location = new Point(100, 20), Size = new Size(200, 23) };
            txtPhone = new TextBox { Location = new Point(100, 50), Size = new Size(200, 23) };
            txtEmail = new TextBox { Location = new Point(100, 80), Size = new Size(200, 23) };

            btnAdd = new Button { Location = new Point(100, 120), Size = new Size(80, 30), Text = "Додати", BackColor = Color.LightGreen };
            btnAdd.Click += BtnAdd_Click;

            btnCancel = new Button { Location = new Point(190, 120), Size = new Size(80, 30), Text = "Скасувати" };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblFullName, lblPhone, lblEmail, txtFullName, txtPhone, txtEmail, btnAdd, btnCancel
            });
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Введіть ПІБ клієнта!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var client = new Client
            {
                FullName = txtFullName.Text,
                Phone = txtPhone.Text,
                Email = txtEmail.Text
            };

            try
            {
                sqlHelper.AddClient(client);
                ClientAdded?.Invoke(this, EventArgs.Empty);
                MessageBox.Show("Клієнта успішно додано!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при додаванні клієнта: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}