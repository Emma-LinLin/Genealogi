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

        /// <summary>
        /// Allows you to execute an Sql command, if unsuccessful an error message will be printed.
        /// </summary>
        /// <param name="sqlString"></param>
        /// <param name="parameters"></param>
        /// <returns>Affected rows upon success</returns>
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

        /// <summary>
        /// Allows you to get data from database, if unsuccessful an error message will be printed.
        /// </summary>
        /// <param name="sqlString"></param>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        /// <returns>Datatable</returns>
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

        /// <summary>
        /// Sets the parameters for the sql command using params.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="command"></param>
        private void SetParameters((string, string)[] parameters, SqlCommand command)
        {
            foreach (var parameter in parameters)
            {
                command.Parameters.AddWithValue(parameter.Item1, parameter.Item2);
            }
        }

        /// <summary>
        /// Recieves an person object and inserts data given to the database table, if unsuccessful an error message will be printed.
        /// </summary>
        /// <param name="person"></param>
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

        /// <summary>
        /// Allows you to find the Id of a person in the datatable. 
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns>Id.</returns>
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

        /// <summary>
        /// Searches datatable for Persons where name or lastname is matched with input using parameters.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>A person object or null depending on input</returns>
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

            return GetPerson(row);
        }

        /// <summary>
        /// Searches datatable for Persons where name or lastname is matched with input.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Null or Person objekt depending on input</returns>
        public Person Read(int id)
        {

            var dt = GetDataTable($"SELECT TOP 1 * FROM Persons WHERE Id={id};");

            if (dt.Rows.Count == 0)
            {
                return null;
            }

            var row = dt.Rows[0];

            return GetPerson(row);
        }

        /// <summary>
        /// Reads datatable rows as Person Properties
        /// </summary>
        /// <param name="row"></param>
        /// <returns>Person object</returns>
        private Person GetPerson(DataRow row)
        {
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

        /// <summary>
        /// Updates person object. 
        /// </summary>
        /// <param name="person"></param>
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

        /// <summary>
        /// Deletes person from database. 
        /// </summary>
        /// <param name="person"></param>
        public void Delete(Person person)
        {
            ExecuteSQL("DELETE FROM Persons Where Id=@id", ("@id", person.Id.ToString()));
        }

        /// <summary>
        /// Creates an sql command depending on input and fetches datatable.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="max"></param>
        /// <returns>List of data retrieved</returns>
        public List<Person> ListPersons(string filter = "", string orderBy = "", int max = 0)
        {
            var sqlString = "SELECT";
            if (max > 0)
            {
                sqlString += " TOP " + max.ToString();
            }
            sqlString += "* FROM Persons";
            if (filter!= "")
            {
                sqlString += " WHERE " + filter;
            }
            if(orderBy != "")
            {
                sqlString += " ORDER BY " + orderBy;
            }

            var dataTable = GetDataTable(sqlString);
            var listOfPersons = new List<Person>();

            foreach(DataRow row in dataTable.Rows)
            {
                listOfPersons.Add(GetPerson(row));
            }

            return listOfPersons;
        }
    }
}
