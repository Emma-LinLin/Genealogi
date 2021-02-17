using System;
using System.Collections.Generic;
using System.Text;

namespace Genealogi.FamilyMembers
{
    class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int BirthDate { get; set; }
        public int DeathDate { get; set; } = default;
        public string BirthCity { get; set; }
        public string DeathCity { get; set; } = "";
        public string BirthCountry { get; set; }
        public string DeathCountry { get; set; } = "";
        public int Mother { get; set; } = default;
        public int Father { get; set; } = default;
        public Person()
        {

        }
        public Person(string firstName, string lastName, int birthDate, int deathDate, string birthCity, string deathCity, string birthCountry, string deathCountry)
        {
            FirstName = firstName;
            LastName = lastName;
            BirthDate = birthDate;
            DeathDate = deathDate;
            BirthCity = birthCity;
            DeathCity = deathCity;
            BirthCountry = birthCountry;
            DeathCountry = deathCountry;
        }

        public Person(string firstName, string lastName, int birthDate, string birthCity, string birthCountry)
        {
            FirstName = firstName;
            LastName = lastName;
            BirthDate = birthDate;
            BirthCity = birthCity;
            BirthCountry = birthCountry;
        }

    }
}
