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
        public int DeathDate { get; set; }
        public string City { get; set; }
        public int Mother { get; set; } = default;
        public int Father { get; set; } = default;
        public Person()
        {

        }
        public Person(string firstName, string lastName, int birthDate, int deathDate, string city)
        {
            FirstName = firstName;
            LastName = lastName;
            BirthDate = birthDate;
            DeathDate = deathDate;
            City = city;
        }
    }
}
