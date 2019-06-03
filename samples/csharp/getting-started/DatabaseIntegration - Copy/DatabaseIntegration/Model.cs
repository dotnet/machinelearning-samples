using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseIntegration 
{
    public class AdultCensusContext : DbContext
    {
        public DbSet<AdultCensus> AdultCensus { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=mlexample.db");
        }
    }
    public class AdultCensus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AdultCensusId {get; set;}
        public int Age { get; set; }
        public string Workclass {  get; set; }
        public string Education { get; set; }
        public string MaritalStatus { get; set; }
        public string Occupation { get; set; }
        public string Relationship { get; set; }
        public string Race { get; set; }
        public string Sex { get; set; }
        public string CapitalGain { get; set; }
        public string CapitalLoss { get; set; }
        public int HoursPerWeek { get; set; }
        public string NativeCountry { get; set; }
        public bool Label { get; set; }
    }
}
