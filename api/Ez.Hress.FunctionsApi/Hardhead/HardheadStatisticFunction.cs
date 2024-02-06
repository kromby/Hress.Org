using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Hardhead.Entities;
using System.Diagnostics;
using System.Collections.Generic;

namespace Ez.Hress.FunctionsApi.Hardhead
{
    public class HardheadStatisticFunction
    {
        private const string _class = nameof(HardheadStatisticFunction);

        private readonly HardheadStatisticsInteractor _statisticsInteractor;

        public HardheadStatisticFunction(HardheadStatisticsInteractor hardheadStatisticsInteractor)
        {
            _statisticsInteractor = hardheadStatisticsInteractor;
        }

        [FunctionName("hardheadStatisticsAttendance")]
        public async Task<IActionResult> RunAttendance(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/statistics/attendances")] HttpRequest req,
            ILogger log)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            const string method = nameof(RunAttendance);
            log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, method);

            try
            {
                var periodType = PeriodType.All;
                if (req.Query.ContainsKey("periodType"))
                {
                    _ = Enum.TryParse<PeriodType>(req.Query["periodType"], out periodType);
                }

                var list = await _statisticsInteractor.GetAttendanceStatistics(periodType);
                return new OkObjectResult(list);

            }
            catch (ArgumentException aex)
            {
                log.LogError(aex, "[{Class}.{Method}] Invalid input", _class, method);
                return new BadRequestObjectResult(aex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "[{Class}.{Method}] Unhandled error", _class, method);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                log.LogInformation("[{Class}.{Method}] Elapsed: {ElapsedMilliseconds} ms.", _class, method, stopwatch.ElapsedMilliseconds);
            }
        }

            [FunctionName("HardheadStatisticUsers")]
        public async Task<IActionResult> RunUsers(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "hardhead/statistics/users/{id:int?}")] HttpRequest req,
            int? id, ILogger log)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            const string method = nameof(RunUsers);
            log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _class, method);

            try
            {
                var periodType = PeriodType.All;
                if (req.Query.ContainsKey("periodType"))
                {
                    _ = Enum.TryParse<PeriodType>(req.Query["periodType"], out periodType);
                }

                var attendanceTypeID = 0;
                if (req.Query.ContainsKey("attendanceType"))
                {
                    _ = int.TryParse(req.Query["attendanceType"], out attendanceTypeID);
                }

                var list = await _statisticsInteractor.GetUserStatistics(periodType, attendanceTypeID, id);
                return new OkObjectResult(list);

            }
            catch (ArgumentException aex)
            {
                log.LogError(aex, "[{Class}.{Method}] Invalid input", _class, method);
                return new BadRequestObjectResult(aex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "[{Class}.{Method}] Unhandled error", _class, method);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                log.LogInformation("[{Class}.{Method}] Elapsed: {ElapsedMilliseconds} ms.", _class, method, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
