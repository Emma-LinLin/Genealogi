using Genealogi.Database;
using Genealogi.FamilyMembers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Genealogi.Programlogic
{
    class FamilyTree
    {
        private List<Person> ListOfPersons = new List<Person>();
        private List<Person> Persons = new List<Person>();
        private string DatabaseName = "Genealogi";

        public void Run()
        {
            var db = new SqlDatabase();
            db.DatabaseName = DatabaseName;

            bool exists = DoesDatabaseExist();
            if(exists)
            {
                DropDatabase();
            }

            CreateDatabase();
            CreateTable();
            GenerateSampleData();
            PrintTable();

            Person person = db.Read("Finn");
            Console.WriteLine($"Chosen person to update: {person.FirstName} {person.LastName}");
            person.LastName = "Barrera";
            db.Update(person);
            Console.WriteLine($"Updated person: {person.FirstName} {person.LastName}");

            person = db.Read("Brevbäraren");
            Console.WriteLine($"Chosen person to delete: {person.FirstName} {person.LastName}");
            db.Delete(person);

            Console.WriteLine("Searching for all persons with last name \"Barrera\": ");
            Persons = db.ListPersons("lastName LIKE '%Barrera%'", "birthDate DESC", 10);
            foreach(var familyMember in Persons)
            {
                Print(familyMember);
            }

            SearchPeople();
        }

        /// <summary>
        /// Checks if database already exists.
        /// </summary>
        /// <returns>Boolean, true or false</returns>
        private bool DoesDatabaseExist()
        {
            var db = new SqlDatabase();
            db.DatabaseName = "Master";

            string parameterName = "@name";
            string parameterValue = "Genealogi";
            var DataTable = db.GetDataTable("SELECT * FROM sys.databases WHERE name=@name", parameterName, parameterValue);

            if (DataTable.Rows.Count < 1)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Drops database and closes connections.
        /// </summary>
        private void DropDatabase()
        {
            var db = new SqlDatabase();
            db.DatabaseName = "Master";

            // Database is being used issue - https://stackoverflow.com/a/20569152/15032536
            db.ExecuteSQL(" alter database [Genealogi] set single_user with rollback immediate");

            db.ExecuteSQL("DROP DATABASE Genealogi");
        }

        /// <summary>
        /// Creates new database with database name Genealogi.
        /// </summary>
        private void CreateDatabase()
        {
            var db = new SqlDatabase();

            db.ExecuteSQL("CREATE DATABASE Genealogi;");
        }

        /// <summary>
        /// Creates table within database Genealogi.
        /// </summary>
        private void CreateTable()
        {
            var db = new SqlDatabase();
            db.DatabaseName = DatabaseName;

            db.ExecuteSQL(@"CREATE TABLE Persons(
Id int NOT NULL Identity (1,1),
firstName nvarchar(50),
lastName nvarchar(50),
birthDate int,
deathDate int,
city nvarchar(50),
mother int,
father int);");
        }

        /// <summary>
        /// Generates sample data of Person and creates them seperately in database with the help of an foreach loop.
        /// </summary>
        public void GenerateSampleData()
        {
            var db = new SqlDatabase();
            db.DatabaseName = DatabaseName;

            ListOfPersons = new List<Person>()
            {
                new Person("Finn", "Linné", 1835, 1928, "Brindleton Bay"),
                new Person("Belinda", "Barrera", 1835, 1928, "Brindleton Bay"),
                new Person("Collin", "Barrera", 1881, 1974, "Brindleton Bay"),
                new Person("Fiona", "Barrera", 1882, 1975, "Brindleton Bay"),
                new Person("Analisa", "Vasquez", 1840, 1933, "Salvadoradia"),
                new Person("Charlie", "Vasquez", 1895, 1988, "Salvadoradia"),
                new Person("Eric", "Eyna", 1840, 1933, "Willow Creek"),
                new Person("Christina", "Eyna", 1840, 1933, "Willow Creek"),
                new Person("Celine", "Eyna", 1884, 1977, "Brindleton Bay"),
                new Person("Emanuel", "Eyna", 1924, 0, "Brindleton Bay"),
                new Person("Noel", "Michaud", 1924, 0, "Willow Creek"),
                new Person("Valentina", "Salas", 1924, 0, "Windenburg"),
                new Person("Paulina", "Salas", 1884, 1977, "Windenburg"),
                new Person("Tim", "Salas", 1964, 0, "Windenburg"),
                new Person("Selina", "Michaud", 1954, 0, "Willow Creek"),
                new Person("Hollie", "Michaud", 1964, 0, "Willow Creek"),
                new Person("Silas", "Michaud", 1970, 0, "Willow Creek"),
                new Person("Mischa", "Michaud", 1970, 0, "Willow Creek"),
                new Person("Brevbäraren", "Michelangelo", 1924, 0, "Willow Creek"),

            };

            foreach (var person in ListOfPersons)
            {
                db.Create(person);
            }

            int father = db.GetPersonId("Finn", "Linné");
            int mother = db.GetPersonId("Belinda", "Barrera");
            SetRelations("Collin", mother, father);
            SetRelations("Fiona", mother, father);
            mother = db.GetPersonId("Analisa", "Vasquez");
            SetRelations("Charlie", mother, father);

            father = db.GetPersonId("Eric", "Eyna");
            mother = db.GetPersonId("Christina", "Eyna");
            SetRelations("Celine", mother, father);

            father = db.GetPersonId("Collin", "Barrera");
            mother = db.GetPersonId("Celine", "Eyna");
            SetRelations("Emanuel", mother, father);

            mother = db.GetPersonId("Paulina", "Salas");
            SetRelations("Valentina", mother);
            
            father = db.GetPersonId("Emanuel", "Eyna");
            mother = db.GetPersonId("Noel", "Michaud");
            SetRelations("Hollie", mother, father);
            SetRelations("Silas", mother, father);
            SetRelations("Mischa", mother, father);
            SetRelations("Selina", mother);
            mother = db.GetPersonId("Valentina", "Salas");
            SetRelations("Tim", mother, father);
        }

        /// <summary>
        /// Sets the mother- father relation to Person.
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="father"></param>
        /// <param name="mother"></param>
        private void SetRelations(string firstName, int father = 0, int mother = 0)
        {
            var db = new SqlDatabase();
            db.DatabaseName = DatabaseName;

            Person person = db.Read(firstName);
            person.Mother = mother;
            person.Father = father;

            db.Update(person);
        }

        /// <summary>
        /// Prints the current table in Database.
        /// </summary>
        public void PrintTable()
        {
            var db = new SqlDatabase();
            db.DatabaseName = DatabaseName;

            var respons = db.GetDataTable(sqlString: "SELECT * FROM [Genealogi].[dbo].[Persons]");

            if (respons != null)
            {
                foreach (DataRow row in respons.Rows)
                {
                    Console.WriteLine($"{row["firstName"]} {row["lastName"]}, {row["birthDate"]} - {row["deathDate"]}, {row["city"]}");
                }
            }
        }
        /// <summary>
        /// Allows you to search for a Person by entering first- or last name.
        /// </summary>
        public void SearchPeople()
        {
            Console.WriteLine();
            Console.Write("Feel free to search for a member by enter first name or last name here: ");
            string userInput = Console.ReadLine();

            var db = new SqlDatabase();
            db.DatabaseName = DatabaseName;

            Person searchedPerson = db.Read(userInput);
            if (searchedPerson == null)
            {
                Console.WriteLine("There's no such familymember");
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"Search result: {searchedPerson.FirstName} {searchedPerson.LastName}");
            Console.WriteLine();

            ListRelations(searchedPerson);

        }
        /// <summary>
        /// Lists relatives based of the Person. 
        /// </summary>
        /// <param name="searchedPerson"></param>
        public void ListRelations(Person searchedPerson)
        {
            var db = new SqlDatabase();
            db.DatabaseName = DatabaseName;

            if (searchedPerson.Mother > 0)
            {
                Person mother = db.Read(searchedPerson.Mother);
                Console.WriteLine($"Mother: {mother.FirstName} {mother.LastName}");
                if (mother.Mother > 0)
                {
                    Person grandmother = db.Read(mother.Mother);
                    Console.WriteLine($"Grandmother on mothers side: {grandmother.FirstName} {grandmother.LastName}");
                }
                if (mother.Father > 0)
                {
                    Person grandfather = db.Read(mother.Father);
                    Console.WriteLine($"Grandfather on mothers side: {grandfather.FirstName} {grandfather.LastName}");
                }
            }
            if (searchedPerson.Father > 0)
            {
                Person father = db.Read(searchedPerson.Father);
                Console.WriteLine($"Father: {father.FirstName} {father.LastName}");
                if (father.Mother > 0)
                {
                    Person grandmother = db.Read(father.Mother);
                    Console.WriteLine($"Grandmother on fathers side: {grandmother.FirstName} {grandmother.LastName}");
                }
                if (father.Father > 0)
                {
                    Person grandfather = db.Read(father.Father);
                    Console.WriteLine($"Grandfather on fathers side: {grandfather.FirstName} {grandfather.LastName}");
                }
            }

            GetSiblings(searchedPerson.Mother, searchedPerson.Father, searchedPerson.Id);
            GetChildren(searchedPerson.Id);
        }

        /// <summary>
        /// Lists Persons with the same mother- and father ID as the search result.
        /// </summary>
        /// <param name="motherID"></param>
        /// <param name="fatherID"></param>
        /// <param name="personID"></param>
        public void GetSiblings(int motherID, int fatherID, int personID)
        {
            var db = new SqlDatabase();
            db.DatabaseName = DatabaseName;


            var respons = db.GetDataTable(@$"SELECT * FROM Persons WHERE NOT Id={personID} AND mother={motherID} AND father={fatherID}");

            if (respons != null)
            {
                foreach (DataRow row in respons.Rows)
                {
                        Console.Write("Sibling: ");
                        Console.WriteLine($"{row["firstName"]} {row["lastName"]}");
                }
            }
        }

        /// <summary>
        /// Lists Persons where the ID matches the mother- or father ID. 
        /// </summary>
        /// <param name="id"></param>
        public void GetChildren(int id)
        {
            var db = new SqlDatabase();
            db.DatabaseName = DatabaseName;

            string parameterName = "@Id";
            string parameterValue = id.ToString();

            var respons = db.GetDataTable(@"SELECT * FROM Persons WHERE father=@Id OR mother=@Id", parameterName, parameterValue);

            if (respons != null)
            {
                foreach (DataRow row in respons.Rows)
                {
                    Console.Write("Child: ");
                    Console.WriteLine($"{row["firstName"]} {row["lastName"]}");
                }
            }
        }

        private void Print(Person person)
        {
            if(person != null)
            {
                Console.WriteLine($"{person.FirstName} {person.LastName}");
                Console.WriteLine("-----------------------");
                Console.WriteLine($"Lifespan: {person.BirthDate} - {person.DeathDate}\nCity: {person.City}");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Person not found.");
            }
        }
    }
}
