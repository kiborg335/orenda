using SportsRental.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace SportsRental.Helpers
{
    public class SqlHelper
    {
        private string connectionString = "Server=localhost\\SQLEXPRESS02;Database=SportsRental;Trusted_Connection=True;";

        public SqlHelper()
        {
            EnsureDatabaseExists();
            EnsureTableExists();
        }

        public List<Equipment> GetAllEquipment()
        {
            var equipment = new List<Equipment>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT e.Id, e.Title, c.Name AS CategoryName, e.Year, e.Brand, 
                           e.TotalQuantity, e.AvailableQuantity, e.CategoryId
                    FROM Equipment e
                    LEFT JOIN Categories c ON e.CategoryId = c.Id
                ", conn);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        equipment.Add(new Equipment
                        {
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            CategoryName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Year = reader.GetInt32(3),
                            Brand = reader.GetString(4),
                            TotalQuantity = reader.GetInt32(5),
                            AvailableQuantity = reader.GetInt32(6),
                            CategoryId = reader.GetInt32(7)
                        });
                    }
                }
            }

            return equipment;
        }

        public void AddEquipment(Equipment equipment)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Equipment (Title, CategoryId, Year, Brand, TotalQuantity, AvailableQuantity) " +
                    "VALUES (@Title, @CategoryId, @Year, @Brand, @TotalQuantity, @AvailableQuantity)", conn);

                cmd.Parameters.AddWithValue("@Title", equipment.Title);
                cmd.Parameters.AddWithValue("@CategoryId", equipment.CategoryId);
                cmd.Parameters.AddWithValue("@Year", equipment.Year);
                cmd.Parameters.AddWithValue("@Brand", equipment.Brand);
                cmd.Parameters.AddWithValue("@TotalQuantity", equipment.TotalQuantity);
                cmd.Parameters.AddWithValue("@AvailableQuantity", equipment.AvailableQuantity);

                cmd.ExecuteNonQuery();
            }
        }

        public bool HasActiveRentals(int equipmentId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Rentals WHERE EquipmentId = @EquipmentId AND ReturnDate IS NULL",
                    conn);
                cmd.Parameters.AddWithValue("@EquipmentId", equipmentId);
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        public void DeleteEquipment(int id)
        {
            if (HasActiveRentals(id))
            {
                throw new InvalidOperationException("Не можна видалити спорядження, яке має активні прокати!");
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Rentals WHERE EquipmentId = @Id AND ReturnDate IS NOT NULL",
                    conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand("DELETE FROM Equipment WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public List<Client> GetAllClients()
        {
            var clients = new List<Client>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Id, FullName, Phone, Email FROM Clients ORDER BY FullName", conn);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clients.Add(new Client
                        {
                            Id = reader.GetInt32(0),
                            FullName = reader.GetString(1),
                            Phone = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Email = reader.IsDBNull(3) ? "" : reader.GetString(3)
                        });
                    }
                }
            }

            return clients;
        }

        public void AddClient(Client client)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Clients (FullName, Phone, Email) VALUES (@FullName, @Phone, @Email)", conn);

                cmd.Parameters.AddWithValue("@FullName", client.FullName);
                cmd.Parameters.AddWithValue("@Phone", (object)client.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)client.Email ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }

        public void RentEquipment(int equipmentId, int clientId, DateTime dueDate, decimal cost)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Rentals (EquipmentId, ClientId, IssueDate, DueDate, RentalCost) " +
                    "VALUES (@EquipmentId, @ClientId, @IssueDate, @DueDate, @RentalCost)", conn);

                cmd.Parameters.AddWithValue("@EquipmentId", equipmentId);
                cmd.Parameters.AddWithValue("@ClientId", clientId);
                cmd.Parameters.AddWithValue("@IssueDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@DueDate", dueDate);
                cmd.Parameters.AddWithValue("@RentalCost", cost); // Зберігаємо правильну вартість

                cmd.ExecuteNonQuery();

                cmd = new SqlCommand(
                    "UPDATE Equipment SET AvailableQuantity = AvailableQuantity - 1 WHERE Id = @EquipmentId AND AvailableQuantity > 0", conn);
                cmd.Parameters.AddWithValue("@EquipmentId", equipmentId);
                cmd.ExecuteNonQuery();
            }
        }

        public void ReturnEquipment(int rentalId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT EquipmentId FROM Rentals WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", rentalId);
                int equipmentId = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand(
                    "UPDATE Rentals SET ReturnDate = @ReturnDate WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@ReturnDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@Id", rentalId);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand(
                    "UPDATE Equipment SET AvailableQuantity = AvailableQuantity + 1 WHERE Id = @EquipmentId", conn);
                cmd.Parameters.AddWithValue("@EquipmentId", equipmentId);
                cmd.ExecuteNonQuery();
            }
        }

        public List<Rental> GetActiveRentals()
        {
            var rentals = new List<Rental>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT r.Id, r.EquipmentId, r.ClientId, r.IssueDate, r.DueDate, r.ReturnDate, r.RentalCost,
                           c.FullName, e.Title
                    FROM Rentals r
                    INNER JOIN Clients c ON r.ClientId = c.Id
                    INNER JOIN Equipment e ON r.EquipmentId = e.Id
                    WHERE r.ReturnDate IS NULL
                    ORDER BY r.DueDate
                ", conn);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rentals.Add(new Rental
                        {
                            Id = reader.GetInt32(0),
                            EquipmentId = reader.GetInt32(1),
                            ClientId = reader.GetInt32(2),
                            IssueDate = reader.GetDateTime(3),
                            DueDate = reader.GetDateTime(4),
                            ReturnDate = reader.IsDBNull(5) ? null : (DateTime?)reader.GetDateTime(5),
                            RentalCost = reader.GetDecimal(6), // Правильно читаємо вартість
                            ClientName = reader.GetString(7),
                            EquipmentName = reader.GetString(8)
                        });
                    }
                }
            }

            return rentals;
        }

        public List<Category> GetAllCategories()
        {
            var categories = new List<Category>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Id, Name FROM Categories ORDER BY Name", conn);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new Category
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }

            return categories;
        }

        private void EnsureDatabaseExists()
        {
            string masterConn = "Server=localhost\\SQLEXPRESS02;Database=master;Trusted_Connection=True;";
            using (SqlConnection conn = new SqlConnection(masterConn))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SportsRental')
                BEGIN
                    CREATE DATABASE SportsRental
                END", conn);
                cmd.ExecuteNonQuery();
            }
        }

        private void EnsureTableExists()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Categories')
                    BEGIN
                        CREATE TABLE Categories (
                            Id INT PRIMARY KEY IDENTITY(1,1),
                            Name NVARCHAR(100) NOT NULL UNIQUE
                        )
                    END

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Equipment')
                    BEGIN
                        CREATE TABLE Equipment (
                            Id INT PRIMARY KEY IDENTITY(1,1),
                            Title NVARCHAR(200) NOT NULL,
                            CategoryId INT,
                            Year INT,
                            Brand NVARCHAR(100),
                            TotalQuantity INT DEFAULT 1,
                            AvailableQuantity INT DEFAULT 1,
                            FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
                        )
                    END

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Clients')
                    BEGIN
                        CREATE TABLE Clients (
                            Id INT PRIMARY KEY IDENTITY(1,1),
                            FullName NVARCHAR(100) NOT NULL,
                            Phone NVARCHAR(20),
                            Email NVARCHAR(100)
                        )
                    END

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Rentals')
                    BEGIN
                        CREATE TABLE Rentals (
                            Id INT PRIMARY KEY IDENTITY(1,1),
                            EquipmentId INT NOT NULL,
                            ClientId INT NOT NULL,
                            IssueDate DATETIME NOT NULL,
                            DueDate DATETIME NOT NULL,
                            ReturnDate DATETIME NULL,
                            RentalCost DECIMAL(10,2) NOT NULL DEFAULT 0,
                            FOREIGN KEY (EquipmentId) REFERENCES Equipment(Id),
                            FOREIGN KEY (ClientId) REFERENCES Clients(Id)
                        )
                    END
                ", conn);

                cmd.ExecuteNonQuery();
            }

            var existingCategories = GetAllCategories();

            var sportCategories = new List<string> {
                "Лижі", "Сноуборди", "Ковзани", "Санки",
                "Велосипеди гірські", "Велосипеди шосейні", "Самокати",
                "Роликові ковзани", "Водні лижі", "Дошки для серфінгу",
                "Каяки", "Палатки", "Спальні мішки", "Рюкзаки",
                "Футбольні м'ячі", "Баскетбольні м'ячі", "Волейбольні м'ячі",
                "Тенісні ракетки", "Гантелі", "Штанги", "Тренажери бігові",
                "Велотренажери", "Йога-матами", "Шоломи", "Наколінники",
                "Спортивні костюми", "Кросівки"
            };

            var bookGenres = new List<string> {
                "Роман", "Детектив", "Фантастика", "Фентезі", "Трилер",
                "Пригоди", "Історичний", "Біографія", "Наукова література",
                "Поезія", "Драма", "Комедія", "Жахи", "Мемуари", "Публіцистика",
                "Дитяча література"
            };

            foreach (var genre in bookGenres)
            {
                if (existingCategories.Any(c => c.Name == genre))
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(
                            "DELETE FROM Categories WHERE Name = @Name", conn);
                        cmd.Parameters.AddWithValue("@Name", genre);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            existingCategories = GetAllCategories();

            foreach (var category in sportCategories)
            {
                if (!existingCategories.Any(g => g.Name == category))
                {
                    AddCategory(category);
                }
            }
        }

        private void AddCategory(string name)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Categories (Name) VALUES (@Name)", conn);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.ExecuteNonQuery();
            }
        }

        public void ClearAllData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                var commands = new string[]
                {
                    "DELETE FROM Rentals",
                    "DELETE FROM Equipment",
                    "DELETE FROM Clients",
                    "DELETE FROM Categories",
                    "DBCC CHECKIDENT ('Rentals', RESEED, 0)",
                    "DBCC CHECKIDENT ('Equipment', RESEED, 0)",
                    "DBCC CHECKIDENT ('Clients', RESEED, 0)",
                    "DBCC CHECKIDENT ('Categories', RESEED, 0)"
                };

                foreach (var cmdText in commands)
                {
                    SqlCommand cmd = new SqlCommand(cmdText, conn);
                    cmd.ExecuteNonQuery();
                }

                EnsureTableExists();
            }
        }
    }
}