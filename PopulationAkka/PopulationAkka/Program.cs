using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopulationAkka
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();

            // Begin timing.
            stopwatch.Start();

            var claims = new List<Claim>();


            for (int i = 0; i < 100000; i++)
            {
                claims.Add(GetClaim(i));
            }

            var listBulkClaim = new List<List<Claim>>();
            for (int i = 0; i < 100; i++)
            {
                listBulkClaim.Add(claims.Skip(i * 1000).Take(1000).ToList());
            }
            Console.WriteLine($"listBulkClaim: {listBulkClaim.Count()}");

            stopwatch.Stop();

            // Write result.
            Console.WriteLine("Time elapsed Query: {0}", stopwatch.Elapsed);

            stopwatch = new Stopwatch();
            stopwatch.Start();
            Process(listBulkClaim);
            stopwatch.Stop();
            Console.WriteLine("Time elapsed Insert: {0}", stopwatch.Elapsed);

            Console.ReadLine();
        }

        static Claim GetClaim(int i)
        {
            System.Threading.Thread.Sleep(1);
            return new Claim
            {
                Id = i,
                CaseName = $"Case name {i}",
                ScheduleNumber = $"Schedule {i}",
                Number = (i + DateTime.Now.Second).ToString(),
                ProjectId = i
            };
        }

        static void Process(List<List<Claim>> list)
        {
            var resultCollection = new ConcurrentBag<int>();
            Parallel.ForEach(list,
                     new ParallelOptions() { MaxDegreeOfParallelism = 100 },
                     (chunkRequest) =>
                     {
                         var chunkResponse = Bulk(chunkRequest);
                         if (chunkResponse.IsValid)
                         {
                             resultCollection.Add(chunkRequest.Count);
                             Console.WriteLine("Documents updated: {0}", chunkRequest.Count);
                         }
                         else
                         {
                             Console.WriteLine("Error updating chunk - items with errors: 0");
                         }
                     }
                    );
            Console.WriteLine($"Document Processed:{resultCollection.Sum()}");
            //return documentsUpdated;
        }

        static Response Bulk(List<Claim> list)
        {
            System.Threading.Thread.Sleep(2000);
            return new Response { IsValid = true, ServerError = null };
        }
    }

    class Response
    {
        public bool IsValid { get; internal set; }
        public int ItemsCount { get; internal set; }
        public object ServerError { get; internal set; }
    }

    class Claim
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Number { get; set; }
        public string ScheduleNumber { get; set; }
        public string CaseName { get; set; }
    }
}
