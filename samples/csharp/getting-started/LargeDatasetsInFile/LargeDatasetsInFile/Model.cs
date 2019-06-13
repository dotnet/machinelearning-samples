using Microsoft.EntityFrameworkCore;
using Microsoft.ML.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LargeDatasetsInSqlServer
{
    public class UrlClickContext : DbContext
    {
        public DbSet<UrlClicks> urlClicks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = @"Data Source=(localdb)\MSSQLLocalDb;Initial Catalog=URLClicksDatabasse;Integrated Security=True;Pooling=False";
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
    public class UrlClicks
    {
        [LoadColumn(0)]
        public string Label { get; set; }

        [LoadColumn(1)]
        public string Feat01 { get; set; }

        [LoadColumn(2)]
        public string Feat02 { get; set; }

        [LoadColumn(3)]
        public string Feat03 { get; set; }

        [LoadColumn(4)]
        public string Feat04 { get; set; }

        [LoadColumn(5)]
        public string Feat05 { get; set; }

        [LoadColumn(6)]
        public string Feat06 { get; set; }

        [LoadColumn(7)]
        public string Feat07 { get; set; }

        [LoadColumn(8)]
        public string Feat08 { get; set; }

        [LoadColumn(9)]
        public string Feat09 { get; set; }

        [LoadColumn(10)]
        public string Feat10 { get; set; }

        [LoadColumn(11)]
        public string Feat11 { get; set; }

        [LoadColumn(12)]
        public string Feat12 { get; set; }

        [LoadColumn(13)]
        public string Feat13 { get; set; }

        [LoadColumn(14)]
        public string Cat14 { get; set; }

        [LoadColumn(15)]
        public string Cat15 { get; set; }

        [LoadColumn(16)]
        public string Cat16 { get; set; }

        [LoadColumn(17)]
        public string Cat17 { get; set; }

        [LoadColumn(18)]
        public string Cat18 { get; set; }

        [LoadColumn(19)]
        public string Cat19 { get; set; }

        [LoadColumn(20)]
        public string Cat20 { get; set; }

        [LoadColumn(21)]
        public string Cat21 { get; set; }

        [LoadColumn(22)]
        public string Cat22 { get; set; }

        [LoadColumn(23)]
        public string Cat23 { get; set; }

        [LoadColumn(24)]
        public string Cat24 { get; set; }

        [LoadColumn(25)]
        public string Cat25 { get; set; }

        [LoadColumn(26)]
        public string Cat26 { get; set; }

        [LoadColumn(27)]
        public string Cat27 { get; set; }

        [LoadColumn(28)]
        public string Cat28 { get; set; }

        [LoadColumn(29)]
        public string Cat29 { get; set; }

        [LoadColumn(30)]
        public string Cat30 { get; set; }

        [LoadColumn(31)]
        public string Cat31 { get; set; }

        [LoadColumn(32)]
        public string Cat32 { get; set; }

        [LoadColumn(33)]
        public string Cat33 { get; set; }

        [LoadColumn(34)]
        public string Cat34 { get; set; }
        [LoadColumn(35)]
        public string Cat35 { get; set; }

        [LoadColumn(36)]
        public string Cat36 { get; set; }

        [LoadColumn(37)]
        public string Cat37 { get; set; }

        [LoadColumn(38)]
        public string Cat38 { get; set; }

        [LoadColumn(39)]
        public string Cat391 { get; set; }
    }


}
