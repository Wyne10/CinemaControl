using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CinemaControl.Services
{
    public interface IWeeklyReportService
    {
        Task<IEnumerable<string>> GetReportFilesAsync(DateTime startDate, DateTime endDate);
    }
} 