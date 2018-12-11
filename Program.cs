using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

namespace Tracking
{
    class Program
    {
        class Employee : ITrackable
        {
            public string Name { get; set; }
            public DateTime BirthDate { get; set; }

            public IEnumerable<Property> TrackedProperties()
            {
                yield return new Property(nameof(Name), Name);
                yield return new Property(nameof(BirthDate), BirthDate);
            }
        }

        class Project : ITrackable
        {
            public DateTime DueDate { get; set; }
            public decimal Charges { get; set; }

            public IEnumerable<Property> TrackedProperties()
            {
                yield return new Property(nameof(DueDate), DueDate);
                yield return new Property(nameof(Charges), Charges);
            }
        }

        class Generator
        {
            Random _rng;
            int _lastId;

            static string[] FirstNames = new[]
            {
                "John",
                "Jacob",
                "Billy",
                "Bob",
            };

            static string[] LastNames = new[]
            {
                "Jingleheimer",
                "Smith",
                "Jones",
                "Barnaby",
            };

            static DateTime[] BirthDates = new[]
            {
                new DateTime(1916, 5, 31),
                new DateTime(1066, 10, 14),
                new DateTime(1794, 6, 1),
                new DateTime(1805, 10, 21),
            };

            public Generator()
            {
                _rng = new Random();
                _lastId = 0;
            }

            public Employee Employee()
            {
                return new Employee
                {
                    Name = $"{GetLastName()}, {GetFirstName()}",
                    BirthDate = GetDate(),
                };
            }

            public Update EmployeeUpdate()
            {
                if (_rng.Next(2) == 0)
                {
                    return new Update(_rng.Next(3) + 1, ApiType.Employee, (++_lastId).ToString(), Employee());
                }
                else
                {
                    return new Update(_rng.Next(3) + 1, ApiType.Employee, (++_lastId).ToString(), Employee(), Employee());
                }
            }

            public Project Project()
            {
                return new Project
                {
                    DueDate = GetDate(),
                    Charges = ((decimal)_rng.Next(10000) + 1) / 100,
                };
            }

            public Update ProjectUpdate()
            {
                if (_rng.Next(2) == 0)
                {
                    return new Update(_rng.Next(3) + 1, ApiType.Project, (++_lastId).ToString(), Project());
                }
                else
                {
                    return new Update(_rng.Next(3) + 1, ApiType.Project, (++_lastId).ToString(), Project(), Project());
                }
            }

            private string GetFirstName()
            {
                return FirstNames[_rng.Next(FirstNames.Length)];
            }

            private string GetLastName()
            {
                return LastNames[_rng.Next(LastNames.Length)];
            }

            private DateTime GetDate()
            {
                return BirthDates[_rng.Next(BirthDates.Length)];
            }
        }

        static void Main(string[] args)
        {
            using (var cx = new SqlConnection(ReadConnectionString()))
            using (var tx = cx.BeginTransaction())
            {
                var generator = new Generator();

                for (var i = 0; i < 300; ++i)
                {
                    var employeeUpdate = generator.EmployeeUpdate();
                    if (employeeUpdate.PropertyChanges.Any())
                    {
                        cx.Execute(CreateInsertCommand(tx, employeeUpdate));
                    }

                    var projectUpdate = generator.ProjectUpdate();
                    if (employeeUpdate.PropertyChanges.Any())
                    {
                        cx.Execute(CreateInsertCommand(tx, projectUpdate));
                    }
                }
            }
        }

        static CommandDefinition CreateInsertCommand(IDbTransaction tx, Update update)
        {
            return new CommandDefinition(
                transaction: tx,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 300,
                commandText: "[dbo].[usp_insertApiChange]",
                parameters: new
                {
                    companyId = update.TenantId,
                    apiType = update.Api,
                    matterNumber = update.MatterNumber,
                    timestamp = update.Timestamp,
                    serializedChanges = JsonConvert.SerializeObject(update),
                }
            );
        }

        /// <summary>
        /// Try running cat file-containing-connection-string | dotnet run
        /// </summary>
        static string ReadConnectionString()
        {
            return Console.In.ReadToEnd();
        }
    }
}
