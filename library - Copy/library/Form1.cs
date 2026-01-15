﻿using SportsRental.Helpers;
using SportsRental.Models;
using SportsRental.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SportsRental
{
    public class Form1 : Form
    {
        private TabControl tabControl;
        private TextBox txtSearch;
        private TextBox txtName;
        private ComboBox cmbCategory;
        private NumericUpDown numYear;
        private TextBox txtBrand;
        private NumericUpDown numTotalItems;
        private Button btnAdd;
        private Button btnDelete;
        private Button btnSearch;
        private Button btnRentItem;
        private Button btnReturnItem;
        private DataGridView dgvEquipment;
        private DataGridView dgvRentals;
        private DataGridView dgvClients;

        private List<Category> categories = new List<Category>();
        private List<Equipment> equipmentList = new List<Equipment>();
        private List<Equipment> filteredEquipment = new List<Equipment>();
        private List<Client> clients = new List<Client>();
        private List<Rental> activeRentals = new List<Rental>();
        private SqlHelper sqlHelper;
        private decimal rentalPricePerDay = 100;

        public Form1()
        {
            this.Text = "Прокат спортивного спорядження - Система управління";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9);

            sqlHelper = new SqlHelper();
            categories = sqlHelper.GetAllCategories();
            clients = sqlHelper.GetAllClients();
            InitializeControls();
            SetupPlaceholders();

            LoadData();
        }

        private void InitializeControls()
        {
            tabControl = new TabControl
            {
                Location = new Point(10, 10),
                Size = new Size(1060, 650),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            var tabEquipment = new TabPage("Спорядження");
            InitializeEquipmentTab(tabEquipment);

            var tabRentals = new TabPage("Прокат");
            InitializeRentalsTab(tabRentals);

            var tabClients = new TabPage("Клієнти");
            InitializeClientsTab(tabClients);

            tabControl.TabPages.AddRange(new TabPage[] { tabEquipment, tabRentals, tabClients });
            this.Controls.Add(tabControl);
        }

        private void InitializeEquipmentTab(TabPage tab)
        {
            txtSearch = new TextBox
            {
                Location = new Point(20, 20),
                Size = new Size(250, 23),
                ForeColor = Color.Gray
            };

            btnSearch = new Button
            {
                Location = new Point(280, 20),
                Size = new Size(80, 23),
                Text = "Шукати"
            };
            btnSearch.Click += (s, e) => SearchEquipment();

            txtName = new TextBox { Location = new Point(20, 60), Size = new Size(180, 23), ForeColor = Color.Gray };
            cmbCategory = new ComboBox { Location = new Point(210, 60), Size = new Size(150, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            numYear = new NumericUpDown { Location = new Point(370, 60), Size = new Size(70, 23), Minimum = 2000, Maximum = DateTime.Now.Year, Value = DateTime.Now.Year };
            txtBrand = new TextBox { Location = new Point(450, 60), Size = new Size(150, 23), ForeColor = Color.Gray };
            numTotalItems = new NumericUpDown { Location = new Point(610, 60), Size = new Size(70, 23), Minimum = 1, Value = 1 };

            cmbCategory.DataSource = categories;
            cmbCategory.DisplayMember = "Name";
            cmbCategory.ValueMember = "Id";

            btnAdd = new Button { Location = new Point(690, 60), Size = new Size(80, 23), Text = "Додати", BackColor = Color.LightGreen };
            btnAdd.Click += (s, e) => AddEquipment();

            btnDelete = new Button { Location = new Point(780, 60), Size = new Size(80, 23), Text = "Видалити", BackColor = Color.LightCoral };
            btnDelete.Click += (s, e) => DeleteEquipment();

            btnRentItem = new Button { Location = new Point(870, 60), Size = new Size(120, 23), Text = "Орендувати", BackColor = Color.LightBlue };
            btnRentItem.Click += (s, e) => ShowRentItemDialog();

            // Кнопка для очищення всіх даних
            var btnClearAll = new Button
            {
                Location = new Point(20, 100),
                Size = new Size(200, 23),
                Text = "ОЧИСТИТИ ВСІ ДАНІ",
                BackColor = Color.Red,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnClearAll.Click += (s, e) => ClearAllData();

            dgvEquipment = new DataGridView
            {
                Location = new Point(20, 130),
                Size = new Size(1020, 460),
                ColumnHeadersHeight = 25,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            tab.Controls.AddRange(new Control[] {
                txtSearch, btnSearch, txtName, cmbCategory, numYear, txtBrand,
                numTotalItems, btnAdd, btnDelete, btnRentItem, btnClearAll, dgvEquipment
            });

            AddLabel(tab, "Назва:", 20, 40);
            AddLabel(tab, "Категорія:", 210, 40);
            AddLabel(tab, "Рік виг.:", 370, 40);
            AddLabel(tab, "Бренд:", 450, 40);
            AddLabel(tab, "Кількість:", 610, 40);
        }

        private void InitializeRentalsTab(TabPage tab)
        {
            dgvRentals = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(1020, 520),
                ColumnHeadersHeight = 25,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            btnReturnItem = new Button
            {
                Location = new Point(20, 550),
                Size = new Size(120, 30),
                Text = "Повернути",
                BackColor = Color.LightGreen
            };
            btnReturnItem.Click += (s, e) => ReturnItem();

            tab.Controls.AddRange(new Control[] { dgvRentals, btnReturnItem });
        }

        private void InitializeClientsTab(TabPage tab)
        {
            dgvClients = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(1020, 520),
                ColumnHeadersHeight = 25,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var btnAddClient = new Button
            {
                Location = new Point(20, 550),
                Size = new Size(120, 30),
                Text = "Додати клієнта",
                BackColor = Color.LightGreen
            };
            btnAddClient.Click += (s, e) => ShowAddClientDialog();

            tab.Controls.AddRange(new Control[] { dgvClients, btnAddClient });
        }

        private void AddLabel(TabPage tab, string text, int x, int y)
        {
            var label = new Label { Text = text, Location = new Point(x, y), Size = new Size(100, 20), Font = new Font("Segoe UI", 8) };
            tab.Controls.Add(label);
        }

        private void SetupPlaceholders()
        {
            SetPlaceholder(txtSearch, "Пошук за назвою, категорією, брендом...");
            SetPlaceholder(txtName, "Назва спорядження");
            SetPlaceholder(txtBrand, "Бренд");
        }

        private void SetPlaceholder(TextBox textBox, string placeholder)
        {
            textBox.Text = placeholder;
            textBox.ForeColor = Color.Gray;
            textBox.Enter += (s, e) => {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.ForeColor = Color.Black;
                }
            };
            textBox.Leave += (s, e) => {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.ForeColor = Color.Gray;
                }
            };
        }

        private void LoadData()
        {
            try
            {
                equipmentList = sqlHelper.GetAllEquipment();
                clients = sqlHelper.GetAllClients();
                activeRentals = sqlHelper.GetActiveRentals();

                UpdateEquipmentGrid();
                UpdateRentalsGrid();
                UpdateClientsGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні даних: {ex.Message}",
                    "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddEquipment()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || txtName.Text == "Назва спорядження")
            {
                MessageBox.Show("Введіть назву спорядження!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var item = new Equipment()
            {
                Title = txtName.Text,
                CategoryId = (int)cmbCategory.SelectedValue,
                Year = (int)numYear.Value,
                Brand = string.IsNullOrWhiteSpace(txtBrand.Text) || txtBrand.Text == "Бренд" ? "Невідомо" : txtBrand.Text,
                TotalQuantity = (int)numTotalItems.Value,
                AvailableQuantity = (int)numTotalItems.Value
            };

            try
            {
                sqlHelper.AddEquipment(item);
                LoadData();
                ClearFields();
                MessageBox.Show("Спорядження успішно додано!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при додаванні спорядження: {ex.Message}",
                    "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowRentItemDialog()
        {
            if (dgvEquipment.SelectedRows.Count == 0)
            {
                MessageBox.Show("Оберіть спорядження для прокату!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Отримуємо назву спорядження з вибраного рядка
            DataGridViewRow selectedRow = dgvEquipment.SelectedRows[0];
            string equipmentName = selectedRow.Cells["Title"].Value?.ToString();

            if (string.IsNullOrEmpty(equipmentName))
            {
                MessageBox.Show("Не вдалося визначити обране спорядження!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Знаходимо спорядження в filteredEquipment за назвою
            var selectedItem = filteredEquipment.FirstOrDefault(e => e.Title == equipmentName);

            if (selectedItem == null)
            {
                MessageBox.Show("Спорядження не знайдено!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedItem.AvailableQuantity <= 0)
            {
                MessageBox.Show("Немає доступних одиниць цього спорядження!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var rentForm = new RentEquipmentForm(selectedItem, clients, sqlHelper, rentalPricePerDay);
            rentForm.EquipmentRented += (s, e) => LoadData();
            rentForm.ShowDialog();
        }

        private void ReturnItem()
        {
            if (dgvRentals.SelectedRows.Count == 0)
            {
                MessageBox.Show("Оберіть запис про оренду для повернення!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedRental = activeRentals[dgvRentals.SelectedRows[0].Index];

            var result = MessageBox.Show($"Підтвердити повернення '{selectedRental.EquipmentName}' клієнту {selectedRental.ClientName}?",
                "Повернення спорядження", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    sqlHelper.ReturnEquipment(selectedRental.Id);
                    LoadData();
                    MessageBox.Show("Спорядження успішно повернено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при поверненні спорядження: {ex.Message}",
                        "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ShowAddClientDialog()
        {
            var clientForm = new AddClientForm(sqlHelper);
            clientForm.ClientAdded += (s, e) => LoadData();
            clientForm.ShowDialog();
        }

        private void UpdateEquipmentGrid()
        {
            dgvEquipment.Rows.Clear();
            dgvEquipment.Columns.Clear();

            dgvEquipment.Columns.Add("Title", "Назва");
            dgvEquipment.Columns.Add("Category", "Категорія");
            dgvEquipment.Columns.Add("Year", "Рік виг.");
            dgvEquipment.Columns.Add("Brand", "Бренд");
            dgvEquipment.Columns.Add("Total", "Всього");
            dgvEquipment.Columns.Add("Available", "Доступно");
            dgvEquipment.Columns.Add("Status", "Статус");

            filteredEquipment = equipmentList.ToList();

            foreach (var item in filteredEquipment.OrderBy(b => b.Title))
            {
                int rowIndex = dgvEquipment.Rows.Add(
                    item.Title,
                    item.CategoryName,
                    item.Year,
                    item.Brand,
                    item.TotalQuantity,
                    item.AvailableQuantity,
                    item.AvailableQuantity > 0 ? "Доступно" : "Немає в наявності"
                );

                if (item.AvailableQuantity == 0)
                {
                    dgvEquipment.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightPink;
                }
                else if (item.AvailableQuantity < item.TotalQuantity / 2)
                {
                    dgvEquipment.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                }
            }
        }

        private void UpdateRentalsGrid()
        {
            dgvRentals.Rows.Clear();
            dgvRentals.Columns.Clear();

            dgvRentals.Columns.Add("Equipment", "Спорядження");
            dgvRentals.Columns.Add("Client", "Клієнт");
            dgvRentals.Columns.Add("IssueDate", "Дата видачі");
            dgvRentals.Columns.Add("DueDate", "Термін повернення");
            dgvRentals.Columns.Add("Days", "Днів");
            dgvRentals.Columns.Add("Cost", "Вартість");
            dgvRentals.Columns.Add("Status", "Статус");

            foreach (var rental in activeRentals)
            {
                int days = (rental.DueDate - rental.IssueDate).Days;

                // Використовуємо вартість з бази даних
                decimal cost = rental.RentalCost;

                int rowIndex = dgvRentals.Rows.Add(
                    rental.EquipmentName,
                    rental.ClientName,
                    rental.IssueDate.ToShortDateString(),
                    rental.DueDate.ToShortDateString(),
                    days,
                    cost.ToString("C"), // Правильна вартість з бази
                    rental.IsOverdue ? "ПРОСРОЧЕНО" : "Активна"
                );

                if (rental.IsOverdue)
                {
                    dgvRentals.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                    dgvRentals.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                }
            }
        }

        private void UpdateClientsGrid()
        {
            dgvClients.Rows.Clear();
            dgvClients.Columns.Clear();

            dgvClients.Columns.Add("FullName", "ПІБ");
            dgvClients.Columns.Add("Phone", "Телефон");
            dgvClients.Columns.Add("Email", "Email");

            foreach (var client in clients.OrderBy(r => r.FullName))
            {
                dgvClients.Rows.Add(client.FullName, client.Phone, client.Email);
            }
        }

        private void SearchEquipment()
        {
            equipmentList = sqlHelper.GetAllEquipment();

            string searchText = txtSearch.ForeColor == Color.Gray ? "" : txtSearch.Text.ToLower();

            filteredEquipment = string.IsNullOrWhiteSpace(searchText)
                ? equipmentList
                : equipmentList.Where(b =>
                    b.Title.ToLower().Contains(searchText) ||
                    b.Brand.ToLower().Contains(searchText) ||
                    b.CategoryName.ToLower().Contains(searchText) ||
                    b.Year.ToString().Contains(searchText)
                ).ToList();

            UpdateEquipmentGrid();
        }

        private void ClearFields()
        {
            txtName.Text = "Назва спорядження";
            txtName.ForeColor = Color.Gray;
            txtBrand.Text = "Бренд";
            txtBrand.ForeColor = Color.Gray;
            numYear.Value = DateTime.Now.Year;
            numTotalItems.Value = 1;
            cmbCategory.SelectedIndex = 0;
            txtName.Focus();
        }

        private void DeleteEquipment()
        {
            if (dgvEquipment.SelectedRows.Count > 0)
            {
                // Отримуємо назву спорядження з вибраного рядка
                DataGridViewRow selectedRow = dgvEquipment.SelectedRows[0];
                string equipmentName = selectedRow.Cells["Title"].Value?.ToString();

                if (string.IsNullOrEmpty(equipmentName))
                {
                    MessageBox.Show("Не вдалося визначити спорядження!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Знаходимо спорядження за назвою
                var equipment = filteredEquipment.FirstOrDefault(e => e.Title == equipmentName);

                if (equipment == null)
                {
                    MessageBox.Show("Спорядження не знайдено!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var result = MessageBox.Show($"Видалити спорядження '{equipment.Title}'?",
                    "Підтвердження", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        sqlHelper.DeleteEquipment(equipment.Id);
                        LoadData();
                        MessageBox.Show("Спорядження успішно видалено!", "Успіх",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка при видаленні: {ex.Message}",
                            "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Метод для очищення всіх даних
        private void ClearAllData()
        {
            var result = MessageBox.Show(
                "🚨 УВАГА! Ця дія видалить ВСІ дані з бази:\n\n" +
                "• Все спорядження\n" +
                "• Всіх клієнтів\n" +
                "• Всю історію прокатів\n" +
                "• Всі категорії\n\n" +
                "Після видалення будуть створені лише спортивні категорії.\n\n" +
                "Цю дію НЕМОЖЛИВО скасувати!\n\n" +
                "Ви дійсно хочете продовжити?",
                "ПОВНЕ ВИДАЛЕННЯ ВСІХ ДАНИХ",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // Викликаємо метод очищення з SqlHelper
                    sqlHelper.ClearAllData();

                    // Оновлюємо дані на формі
                    categories = sqlHelper.GetAllCategories();
                    clients = sqlHelper.GetAllClients();
                    equipmentList = new List<Equipment>();
                    filteredEquipment = new List<Equipment>();
                    activeRentals = new List<Rental>();

                    // Оновлюємо ComboBox з категоріями
                    cmbCategory.DataSource = categories;
                    cmbCategory.DisplayMember = "Name";
                    cmbCategory.ValueMember = "Id";

                    // Очищаємо всі поля
                    txtSearch.Text = "";
                    txtName.Text = "Назва спорядження";
                    txtName.ForeColor = Color.Gray;
                    txtBrand.Text = "Бренд";
                    txtBrand.ForeColor = Color.Gray;
                    numYear.Value = DateTime.Now.Year;
                    numTotalItems.Value = 1;

                    // Оновлюємо таблиці
                    UpdateEquipmentGrid();
                    UpdateRentalsGrid();
                    UpdateClientsGrid();

                    MessageBox.Show("✅ Базу даних успішно очищено!\n\n" +
                        "Всі дані видалено. Зараз база порожня.\n" +
                        "Додані тільки спортивні категорії.",
                        "Успіх",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Помилка при очищенні бази даних:\n\n{ex.Message}",
                        "Помилка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
    }
}