using SportsRental.Helpers;
using SportsRental.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SportsRental.Forms
{
    public partial class RentEquipmentForm : Form
    {
        private Equipment equipment;
        private List<Client> clients;
        private SqlHelper sqlHelper;
        private decimal pricePerDay;

        public event EventHandler EquipmentRented;

        private ComboBox cmbClient;
        private DateTimePicker dtpDueDate;
        private Label lblCost;
        private Button btnRent;
        private Button btnCancel;
        private NumericUpDown numDays;

        public RentEquipmentForm(Equipment equipment, List<Client> clients, SqlHelper sqlHelper, decimal pricePerDay)
        {
            this.equipment = equipment;
            this.clients = clients;
            this.sqlHelper = sqlHelper;
            this.pricePerDay = pricePerDay;

            InitializeComponent();
            CalculateCost();
        }

        private void InitializeComponent()
        {
            this.Text = "Оренда спорядження";
            this.Size = new Size(450, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            var lblEquipment = new Label
            {
                Text = $"Спорядження: {equipment.Title}",
                Location = new Point(20, 20),
                Size = new Size(400, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            var lblClient = new Label { Text = "Клієнт:", Location = new Point(20, 50), Size = new Size(60, 20) };
            var lblDays = new Label { Text = "Кількість днів:", Location = new Point(20, 80), Size = new Size(100, 20) };
            var lblDueDate = new Label { Text = "Дата повернення:", Location = new Point(20, 110), Size = new Size(120, 20) };
            var lblTotalCost = new Label { Text = "Загальна вартість:", Location = new Point(20, 140), Size = new Size(120, 20) };

            cmbClient = new ComboBox
            {
                Location = new Point(150, 50),
                Size = new Size(250, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbClient.DisplayMember = "FullName";
            cmbClient.DataSource = clients;

            numDays = new NumericUpDown
            {
                Location = new Point(150, 80),
                Size = new Size(80, 23),
                Minimum = 1,
                Maximum = 365,
                Value = 7
            };
            numDays.ValueChanged += (s, e) =>
            {
                dtpDueDate.Value = DateTime.Now.AddDays((int)numDays.Value);
                CalculateCost();
            };

            dtpDueDate = new DateTimePicker
            {
                Location = new Point(150, 110),
                Size = new Size(120, 23),
                Value = DateTime.Now.AddDays(7)
            };
            dtpDueDate.ValueChanged += (s, e) =>
            {
                int days = (dtpDueDate.Value.Date - DateTime.Now.Date).Days;
                numDays.Value = days > 0 ? days : 1;
                CalculateCost();
            };

            lblCost = new Label
            {
                Location = new Point(150, 140),
                Size = new Size(150, 23),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Green
            };

            btnRent = new Button
            {
                Location = new Point(150, 180),
                Size = new Size(100, 30),
                Text = "Орендувати",
                BackColor = Color.LightGreen
            };
            btnRent.Click += BtnRent_Click;

            btnCancel = new Button
            {
                Location = new Point(260, 180),
                Size = new Size(100, 30),
                Text = "Скасувати"
            };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblEquipment, lblClient, lblDays, lblDueDate, lblTotalCost,
                cmbClient, numDays, dtpDueDate, lblCost, btnRent, btnCancel
            });

            CalculateCost();
        }

        private void CalculateCost()
        {
            int days = (int)numDays.Value;
            decimal totalCost = days * pricePerDay;
            lblCost.Text = totalCost.ToString("C");
        }

        private void BtnRent_Click(object sender, EventArgs e)
        {
            if (cmbClient.SelectedItem == null)
            {
                MessageBox.Show("Оберіть клієнта!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedClient = (Client)cmbClient.SelectedItem;
            int days = (int)numDays.Value;
            decimal totalCost = days * pricePerDay; // Правильний розрахунок

            var confirmResult = MessageBox.Show(
                $"Підтвердити оренду?\n\n" +
                $"Спорядження: {equipment.Title}\n" +
                $"Клієнт: {selectedClient.FullName}\n" +
                $"Термін: {days} днів\n" +
                $"Вартість: {totalCost:C}\n" +
                $"Дата повернення: {dtpDueDate.Value.ToShortDateString()}",
                "Підтвердження оренди",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    // Передаємо правильну вартість
                    sqlHelper.RentEquipment(equipment.Id, selectedClient.Id, dtpDueDate.Value, totalCost);
                    EquipmentRented?.Invoke(this, EventArgs.Empty);

                    MessageBox.Show($"Оренду успішно оформлено!\nВартість: {totalCost:C}",
                        "Успіх",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при оформленні оренди: {ex.Message}",
                        "Помилка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
    }
}