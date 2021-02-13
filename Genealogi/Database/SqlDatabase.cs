using Genealogi.FamilyMembers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Genealogi.Database
{
    class SqlDatabase
    {
        private string ConnectionString = @"Data Source=.\SQLExpress;Integrated Security=true;database={0}";

        public string DatabaseName { get; set; }

        public SqlDatabase()
        {

        }

        public long ExecuteSQL(string sqlString, params (string, string)[] parameters)
        {
            long rowsAffected = 0;
            try
            {
                var connString = string.Format(ConnectionString, DatabaseName);
                using (var cnn = new SqlConnection(connString))
                {
                    cnn.Open();
                    using (var command = new SqlCommand(sqlString, cnn))
                    {
                        SetParameters(parameters, command);
                        rowsAffected = command.ExecuteNonQuery();
                    }
                    cnn.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return rowsAffected;
        }
        public DataTable GetDataTable(string sqlString, string parameterName = "", string parameterValue = "")
        {
            var dt = new DataTable();
            try
            {
                using (var cnn = new SqlConnection(string.Format(ConnectionString, DatabaseName)))
                {
                    cnn.Open();
                    using (var command = new SqlCommand(sqlString, cnn))
                    {
                        command.Parameters.AddWithValue(parameterName, parameterValue);
                        using (var adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dt);
                        }
                    }
                    cnn.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return dt;
        }
        public void SetParameters((string, string)[] parameters, SqlCommand command)
        {
            foreach (var parameter in parameters)
            {
                command.Parameters.AddWithValue(parameter.Item1, parameter.Item2);
            }
        }
        public void Create(Person person)
        {
            try
            {
                var connString = string.Format(ConnectionString, DatabaseName);
                using (var cnn = new SqlConnection(connString))
                {
                    cnn.Open();
                    var sql = "INSERT INTO Persons(firstName, lastName, birthDate, deathDate, city, mother, father) VALUES (@firstName, @lastName, @birthDate, @deathDate, @city, @mother, @father)";
                    using (var command = new SqlCommand(sql, cnn))
                    {
                        command.Parameters.AddWithValue("@firstName", person.FirstName);
                        command.Parameters.AddWithValue("@lastName", person.LastName);
                        command.Parameters.AddWithValue("@birthDate", person.BirthDate);
                        command.Parameters.AddWithValue("@deathDate", person.DeathDate);
                        command.Parameters.AddWithValue("@city", person.City);
                        command.Parameters.AddWithValue("@mother", person.Mother);
                        command.Parameters.AddWithValue("@father", person.Father);
                        command.ExecuteNonQuery();
                    }
                    cnn.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public int GetPersonId(string firstName, string lastName)
        {
            var sqlString = "SELECT TOP 1 * FROM Persons WHERE firstName=@firstName AND lastName=@lastName;";
            var dt = new DataTable();
            try
            {
                using (var cnn = new SqlConnection(string.Format(ConnectionString, DatabaseName)))
                {
                    cnn.Open();
                    using (var command = new SqlCommand(sqlString, cnn))
                    {
                        command.Parameters.AddWithValue("@firstname", firstName);
                        command.Parameters.AddWithValue("@lastname", lastName);
                        using (var adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dt);
                        }
                    }
                    cnn.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (dt.Rows.Count == 0) return 0;

            var row = dt.Rows[0];
            var id = (int)row["Id"];
            return id;
        }
        public Person Read(string name)
        {

            string parameterName = "@name";
            string parameterValue = name;

            var dt = GetDataTable("SELECT TOP 1 * from Persons Where firstName LIKE @name OR lastName LIKE @name", parameterName, parameterValue);

            if (dt.Rows.Count == 0)
            {
                return null;
            }

            var row = dt.Rows[0];

            return new Person
            {
                Id = (int)row["Id"],
                FirstName = row["firstName"].ToString(),
                LastName = row["lastName"].ToString(),
                BirthDate = (int)row["birthDate"],
                DeathDate = (int)row["deathDate"],
                City = row["city"].ToString(),
                Mother = (int)row["mother"],
                Father = (int)row["father"]
            };
        }
        public Person Read(int id)
        {

            var dt = GetDataTable($"SELECT TOP 1 * FROM Persons WHERE Id={id};");

            if (dt.Rows.Count == 0)
            {
                return null;
            }

            var row = dt.Rows[0];

            return new Person
            {
                Id = (int)row["Id"],
                FirstName = row["firstName"].ToString(),
                LastName = row["lastName"].ToString(),
                BirthDate = (int)row["birthDate"],
                DeathDate = (int)row["deathDate"],
                City = row["city"].ToString(),
                Mother = (int)row["mother"],
                Father = (int)row["father"]
            };
        }
        public void Update(Person person)
        {

            ExecuteSQL(@"Update Persons SET
lastName=@LastName, firstName=@FirstName, birthDate=@BirthDate, deathDate=@DeathDate, City=@City, mother=@Mother, father=@Father
WHERE Id = @Id",
("@FirstName", person.FirstName),
("@LastName", person.LastName),
("@BirthDate", person.BirthDate.ToString()),
("@DeathDate", person.DeathDate.ToString()),
("@City", person.City),
("@Mother", person.Mother.ToString()),
("@Father", person.Father.ToString()),
("@Id", person.Id.ToString()));
        }
        public void Delete(Person person)
        {
            ExecuteSQL("DELETE FROM Persons Where Id=@id", ("@id", person.Id.ToString()));
        }
    }
}
