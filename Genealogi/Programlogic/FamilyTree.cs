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
        private string DatabaseName = "Genealogi";

        public void Run()
        {
            var db = new SqlDatabase();
            db.DatabaseName = DatabaseName;

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

            SearchPeople();
        }
        private void CreateDatabase()
        {
            var db = new SqlDatabase();

            db.ExecuteSQL("CREATE DATABASE Genealogi;");
        }
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

        public void GenerateSampleData()
        {
            var db = new SqlDatabase();
            db.DatabaseName = DatabaseName;

            ListOfPersons = new List<Person>()
            {
                new Person("Finn", "Linné", 1835, 1928, "Brindleton Bay", 0, 0),
                new Person("Belinda", "Barrera", 1835, 1928, "Brindleton Bay", 0, 0),
                new Person("Collin", "Barrera", 1881, 1974, "Brindleton Bay", 0, 0),
                new Person("Fiona", "Barrera", 1882, 1975, "Brindleton Bay", 0, 0),
                new Person("Analisa", "Vasquez", 1840, 1933, "Salvadoradia", 0, 0),
                new Person("Charlie", "Vasquez", 1895, 1988, "Salvadoradia", 0, 0),
                new Person("Eric", "Eyna", 1840, 1933, "Willow Creek", 0, 0),
                new Person("Christina", "Eyna", 1840, 1933, "Willow Creek", 0, 0),
                new Person("Celine", "Eyna", 1884, 1977, "Brindleton Bay", 0, 0),
                new Person("Emanuel", "Eyna", 1924, 0, "Brindleton Bay", 0, 0),
                new Person("Noel", "Michaud", 1924, 0, "Willow Creek", 0, 0),
                new Person("Valentina", "Salas", 1924, 0, "Windenburg", 0, 0),
                new Person("Paulina", "Salas", 1884, 1977, "Windenburg", 0, 0),
                new Person("Tim", "Salas", 1964, 0, "Windenburg", 0, 0),
                new Person("Selina", "Michaud", 1954, 0, "Willow Creek", 0, 0),
                new Person("Hollie", "Michaud", 1964, 0, "Willow Creek", 0, 0),
                new Person("Silas", "Michaud", 1970, 0, "Willow Creek", 0, 0),
                new Person("Mischa", "Michaud", 1970, 0, "Willow Creek", 0, 0),
                new Person("Brevbäraren", "Michelangelo", 1924, 0, "Willow Creek", 0, 0),

            };

            foreach (var person in ListOfPersons)
            {
                db.Create(person);
            }

            SetRelations("Collin", "Barrera", "Belinda", "Barrera", "Finn", "Linné");
            SetRelations("Fiona", "Barrera", "Belinda", "Barrera", "Finn", "Linné");
            SetRelations("Charlie", "Vasquez", "Analisa", "Vasquez", "Finn", "Linné");
            SetRelations("Celine", "Eyna", "Christina", "Eyna", "Eric", "Eyna");
            SetRelations("Emanuel", "Eyna", "Celine", "Eyna", "Collin", "Barrera");
            SetRelations("Valentina", "Salas", "Paulina", "Salas");
            SetRelations("Selina", "Michaud", "Noel", "Michaud");
            SetRelations("Tim", "Salas", "Valentina", "Salas", "Emanuel", "Eyna");
            SetRelations("Hollie", "Michaud", "Noel", "Michaud", "Emanuel", "Eyna");
            SetRelations("Silas", "Michaud", "Noel", "Michaud", "Emanuel", "Eyna");
            SetRelations("Mischa", "Michaud", "Noel", "Michaud", "Emanuel", "Eyna");

        }
        public void SetRelations(string firstName, string lastName, string motherFirstName = "", string motherLastName = "", string fatherFirstName = "", string fatherLastName = "")
        {
            var db = new SqlDatabase();
            db.DatabaseName = DatabaseName;

            var motherID = db.GetPersonId(motherFirstName, motherLastName);
            var fatherID = db.GetPersonId(fatherFirstName, fatherLastName);
            var childID = db.GetPersonId(firstName, lastName);

            UpdateRelations(childID, motherID, fatherID);

        }
        public void UpdateRelations(int childID, int motherID = 0, int fatherID = 0)
        {
            var db = new SqlDatabase();
            db.DatabaseName = DatabaseName;
            db.ExecuteSQL($"UPDATE Persons SET mother={motherID},father={fatherID} WHERE Id={childID};");
        }
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

            GetSiblings(searchedPerson.Mother, searchedPerson.Father);
            GetChildren(searchedPerson.Id);
        }
        public void GetSiblings(int motherID, int fatherID)
        {
            var db = new SqlDatabase();
            db.DatabaseName = DatabaseName;


            var respons = db.GetDataTable(@$"SELECT * FROM Persons WHERE mother={motherID} AND father={fatherID}");

            if (respons != null)
            {
                foreach (DataRow row in respons.Rows)
                {
                    Console.Write("Sibling: ");
                    Console.WriteLine($"{row["firstName"]} {row["lastName"]}");
                }
            }
        }
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
    }
}
