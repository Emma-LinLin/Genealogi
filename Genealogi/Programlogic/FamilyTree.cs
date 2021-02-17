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
        private Person person = new Person();
        private string DatabaseName = "Genealogi";

        public void Run()
        {
            var db = new SqlDatabase();
            db.DatabaseName = DatabaseName;

            bool exists = DoesDatabaseExist();
            if(!exists)
            {
                CreateDatabase();
                CreateTable();
                GenerateSampleData();
            }

            Console.WriteLine("This is our current family tree!");
            PrintTable();

            Console.WriteLine();
            person = db.Read("Finn");
            Console.WriteLine($"Chosen person to update: {person.FirstName} {person.LastName}");
            person.LastName = "Barrera";
            long rowsAffected = db.Update(person);
            Console.WriteLine($"Updated person: {person.FirstName} {person.LastName}");
            Console.WriteLine($"{rowsAffected} rows affected!");

            Console.WriteLine();
            person = db.Read("Brevbäraren");
            if(person != null)
            {
                Console.WriteLine($"Chosen person to delete: {person.FirstName} {person.LastName}");
                rowsAffected = db.Delete(person);
                Console.WriteLine($"Deleted person: {rowsAffected} rows affected!");
            }

            Console.WriteLine();
            Console.WriteLine("Searching for all persons with last name \"Barrera\": ");
            Persons = db.ListPersons("lastName LIKE '%Barrera%'", "birthDate DESC", 10);
            foreach(var person in Persons)
            {
                Print(person);
            }

            Console.WriteLine();
            Console.WriteLine("Searching for all persons with birth date \"1935\": ");
            Persons = db.ListPersons("birthDate = 1935", "lastName", 10);
            foreach (var person in Persons)
            {
                Print(person);
            }

            Console.WriteLine();
            Console.WriteLine("Searching for all persons living in \"Windenburg\": ");
            Persons = db.ListPersons("birthCity LIKE '%Windenburg%' OR deathCity LIKE '%Windenburg%'", "lastName", 10);
            foreach (var person in Persons)
            {
                Print(person);
            }

            Console.WriteLine();
            Console.WriteLine("Searching for all persons who were born in \"Egypt\": ");
            Persons = db.ListPersons("birthCountry LIKE '%Egypt%'", "lastName", 10);
            foreach (var person in Persons)
            {
                Print(person);
            }

            FamilyTreeSearch();
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
birthCity nvarchar(50),
deathCity nvarchar(50),
birthCountry nvarchar(50),
deathCountry nvarchar(50),
mother int,
father int);");
        }

        /// <summary>
        /// Generates sample data of Person and creates them seperately in database with the help of an foreach loop.
        /// </summary>
        private void GenerateSampleData()
        {
            var db = new SqlDatabase();
            db.DatabaseName = DatabaseName;

            ListOfPersons = new List<Person>()
            {
                new Person("Finn", "Linné", 1896, 1981, "Brindleton Bay", "Salvadoradia", "Canada", "Egypt"),
                new Person("Belinda", "Barrera", 1905, 1981, "Brindleton Bay", "Brindleton Bay", "Canada", "Canada"),
                new Person("Collin", "Barrera", 1931, 2007, "Brindleton Bay", "Willow Creek", "Canada", "Canada"),
                new Person("Fiona", "Barrera", 1935, 2013, "Brindleton Bay", "Brindleton Bay", "Canada", "Canada"),
                new Person("Analisa", "Vasquez", 1920, 2010, "Salvadoradia", "Salvadoradia", "Egypt", "Egypt"),
                new Person("Charlie", "Vasquez", 1940, 2020, "Salvadoradia", "Salvadoradia", "Egypt", "Egypt"),
                new Person("Eric", "Eyna", 1905, 1990, "Willow Creek", "Willow Creek", "Canada", "Canada"),
                new Person("Christina", "Eyna", 1900, 1985, "Salvadoradia", "Willow Creek", "Egypt", "Canada"),
                new Person("Celine", "Eyna", 1935, 2015, "Willow Creek", "Willow Creek", "Canada", "Canada"),
                new Person("Emanuel", "Eyna", 1971, "Willow Creek", "Canada"),
                new Person("Noel", "Michaud", 1976, "Newcrest", "Canada"),
                new Person("Valentina", "Salas", 1974, "Windenburg", "Russia"),
                new Person("Paulina", "Salas", 1939, 2019, "Windenburg", "Windenburg", "Russia", "Russia"),
                new Person("Tim", "Salas", 2007, "Windenburg", "Russia"),
                new Person("Selina", "Michaud", 2000, "Newcrest", "Canada"),
                new Person("Hollie", "Michaud", 2008, "Willow Creek", "Canada"),
                new Person("Silas", "Michaud", 2011, "Willow Creek", "Canada"),
                new Person("Mischa", "Michaud", 2011, "Willow Creek", "Canada"),
                new Person("Brevbäraren", "Michelangelo", 1970, "Willow Creek", "Canada"),

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
        /// Sets the mother- father relation to Person with help of the person Id.
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="father"></param>
        /// <param name="mother"></param>
        private void SetRelations(string firstName, int mother = 0, int father = 0)
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
        private void PrintTable()
        {
            var db = new SqlDatabase();
            db.DatabaseName = DatabaseName;

            var respons = db.GetDataTable(sqlString: "SELECT * FROM [Genealogi].[dbo].[Persons]");

            if (respons != null)
            {
                foreach (DataRow row in respons.Rows)
                {
                    Console.WriteLine($"{row["firstName"]} {row["lastName"]}, Born: {row["birthDate"]}, Place of birth: {row["birthCity"]}, {row["birthCountry"]}");
                }
            }
        }

        /// <summary>
        /// Opens up the search function 
        /// </summary>
        private void FamilyTreeSearch()
        {
            Console.WriteLine();
            Console.WriteLine("Welcome to the Familytree search function!");
            Console.WriteLine("Feel free to search for a member to see relations, to quit just enter \"Q\"");

            while (true)
            {
                Console.WriteLine();
                Console.Write("Enter first- or lastname here: ");
                string userInput = Console.ReadLine().ToLower();

                if (userInput == "q")
                {
                    return;
                }

                SearchPeopleRelations(userInput);
            }
            
        }
        /// <summary>
        /// Allows you to search for a Person by entering first- or last name.
        /// </summary>
        private void SearchPeopleRelations(string userInput)
        {
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
        private void ListRelations(Person searchedPerson)
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

            GetSiblings(searchedPerson.Id, searchedPerson.Mother, searchedPerson.Father);
            GetChildren(searchedPerson.Id);
        }

        /// <summary>
        /// Lists Persons with the same mother- and father ID as the search result.
        /// </summary>
        /// <param name="motherID"></param>
        /// <param name="fatherID"></param>
        /// <param name="personID"></param>
        private void GetSiblings(int personID, int motherID = 0, int fatherID = 0)
        {
            var db = new SqlDatabase();
            db.DatabaseName = DatabaseName;

            //All person objects with mother- or father ID = 0 will be considered siblings, if statement to avoid this issue.
            if(motherID == 0 || fatherID == 0)
            {
                return;
            }

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
        private void GetChildren(int id)
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

        /// <summary>
        /// Prints person information.
        /// </summary>
        /// <param name="person"></param>
        private void Print(Person person)
        {
            if(person != null)
            {
                Console.WriteLine($"{person.FirstName} {person.LastName}");
                Console.WriteLine("---------------");
                if (person.DeathDate != 0)
                {
                    Console.WriteLine($"Lifespan: {person.BirthDate} - {person.DeathDate}\nPlace of birth: {person.BirthCity}, {person.BirthCountry}");
                }
                else
                {
                    Console.WriteLine($"Birth year: {person.BirthDate}\nPlace of birth: {person.BirthCity}, {person.BirthCountry}");
                }
                
                if(person.DeathCity != "" || person.DeathCountry != "")
                {
                    Console.WriteLine($"Place of death: {person.DeathCity}, {person.DeathCountry}");
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Person not found.");
            }
        }
    }
}
