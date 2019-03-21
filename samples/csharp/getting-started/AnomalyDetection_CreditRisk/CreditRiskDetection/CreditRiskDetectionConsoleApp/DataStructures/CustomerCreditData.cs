using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnomalyDetection.DataStructures
{
    public class CustomerCreditData
    {
       [LoadColumn(0)]
       public string ExistingCheckingAccountStatus { get; set; }

       [LoadColumn(1)]
       public float NumOfMonths { get; set; }

       [LoadColumn(2)]
       public string CreditHistory { get; set; }

       [LoadColumn(3)]
       public string Purpose { get; set; }

       [LoadColumn(4)]
       public float CreditAmount { get; set; }

       [LoadColumn(5)]
       public string SavingAccountBonds { get; set; }

       [LoadColumn(6)]
       public string EmployedSince { get; set; }

       [LoadColumn(7)]
       public float InstallmentRate { get; set; }

       [LoadColumn(8)]
       public string StatusAndSex { get; set; }

       [LoadColumn(9)]
       public string Gurantors { get; set; }

       [LoadColumn(10)]
       public float ResidentSince { get; set; }
      
       [LoadColumn(11)]
       public string Property { get; set; }
      
       [LoadColumn(12)]
       public float Age { get; set; }
      
       [LoadColumn(13)]
       public string OtherInstallmentPlans { get; set; }
      
       [LoadColumn(14)]
       public string Housing { get; set; }
      
       [LoadColumn(15)]
       public float NumberOfExistingCredits { get; set; }
      
       [LoadColumn(16)]
       public string JobStatus { get; set; }
      
       [LoadColumn(17)]
       public float NumberOfLiablePeople { get; set; }
      
       [LoadColumn(18)]
       public string Telephone { get; set; }
      
       [LoadColumn(19)]
       public string IsForeignWorker { get; set; }
      
       [LoadColumn(20)]
       public float Label { get; set; }
    }
}
