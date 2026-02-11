using PatientApi.Entities;

namespace PatientApi.Services
{
    public class DateSearchService : IDateSearchService
    {
        public IQueryable<Patient> ApplyDateFilter(IQueryable<Patient> query, string birthDate)
        {
            if (string.IsNullOrEmpty(birthDate))
            {
                throw new ArgumentException("birthDate parameter is required", nameof(birthDate));
            }

            var (prefix, dateString) = ParsePrefix(birthDate);
            var (parsedDate, startOfDay, endOfDay) = ParseDate(dateString);

            return ApplySearchLogic(query, prefix, parsedDate, startOfDay, endOfDay);
        }

        private (string prefix, string dateString) ParsePrefix(string birthDate)
        {
            if (birthDate.Length >= 2)
            {
                var possiblePrefix = birthDate.Substring(0, 2).ToLower();
                var validPrefixes = new[] { "eq", "ne", "lt", "gt", "le", "ge", "sa", "eb", "ap" };

                if (validPrefixes.Contains(possiblePrefix))
                {
                    return (possiblePrefix, birthDate.Substring(2));
                }
            }

            return ("", birthDate);
        }

        private (DateTime? parsedDate, DateTime? startOfDay, DateTime? endOfDay) ParseDate(string dateString)
        {
            DateTime? parsedDate = null;
            DateTime? startOfDay = null;
            DateTime? endOfDay = null;

            DateTime CreateUtcDateTime(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, int millisecond = 0)
            {
                return new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);
            }

            if (DateTime.TryParse(dateString, out var dt))
            {
                var dtUtc = dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime();
                parsedDate = DateTime.SpecifyKind(dtUtc, DateTimeKind.Utc);

                if (dt.TimeOfDay == TimeSpan.Zero)
                {
                    startOfDay = CreateUtcDateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0);
                    var nextDayStart = DateTime.SpecifyKind(startOfDay.Value.AddDays(1), DateTimeKind.Utc);
                    var endDay = CreateUtcDateTime(nextDayStart.Year, nextDayStart.Month, nextDayStart.Day, 0, 0, 0, 0);
                    endOfDay = DateTime.SpecifyKind(endDay.AddTicks(-1), DateTimeKind.Utc);
                }
            }
            else if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var dateOnly))
            {
                startOfDay = CreateUtcDateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, 0, 0, 0, 0);
                var nextDayStart = DateTime.SpecifyKind(startOfDay.Value.AddDays(1), DateTimeKind.Utc);
                var endDay = CreateUtcDateTime(nextDayStart.Year, nextDayStart.Month, nextDayStart.Day, 0, 0, 0, 0);
                endOfDay = DateTime.SpecifyKind(endDay.AddTicks(-1), DateTimeKind.Utc);
            }
            else if (DateTime.TryParseExact(dateString, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out var yearMonth))
            {
                startOfDay = CreateUtcDateTime(yearMonth.Year, yearMonth.Month, 1, 0, 0, 0, 0);
                var nextMonthStart = DateTime.SpecifyKind(startOfDay.Value.AddMonths(1), DateTimeKind.Utc);
                var endMonth = CreateUtcDateTime(nextMonthStart.Year, nextMonthStart.Month, nextMonthStart.Day, 0, 0, 0, 0);
                endOfDay = DateTime.SpecifyKind(endMonth.AddTicks(-1), DateTimeKind.Utc);
            }
            else if (DateTime.TryParseExact(dateString, "yyyy", null, System.Globalization.DateTimeStyles.None, out var year))
            {
                startOfDay = CreateUtcDateTime(year.Year, 1, 1, 0, 0, 0, 0);
                endOfDay = CreateUtcDateTime(year.Year, 12, 31, 23, 59, 59, 999);
            }
            else
            {
                throw new ArgumentException(
                    "Invalid date format. Expected formats: yyyy-MM-ddTHH:mm:ss, yyyy-MM-dd, yyyy-MM, or yyyy",
                    nameof(dateString));
            }

            return (parsedDate, startOfDay, endOfDay);
        }

        private IQueryable<Patient> ApplySearchLogic(
            IQueryable<Patient> query,
            string prefix,
            DateTime? parsedDate,
            DateTime? startOfDay,
            DateTime? endOfDay)
        {
            if (startOfDay.HasValue && endOfDay.HasValue)
            {
                var startUtc = EnsureUtc(startOfDay.Value);
                var endUtc = EnsureUtc(endOfDay.Value);

                return prefix switch
                {
                    "eq" => query.Where(p => p.BirthDate >= startUtc && p.BirthDate < endUtc),
                    "ne" => query.Where(p => p.BirthDate < startUtc || p.BirthDate >= endUtc),
                    "lt" => query.Where(p => p.BirthDate < startUtc),
                    "gt" => query.Where(p => p.BirthDate >= endUtc),
                    "le" => query.Where(p => p.BirthDate < endUtc),
                    "ge" => query.Where(p => p.BirthDate >= startUtc),
                    "sa" => query.Where(p => p.BirthDate >= endUtc),
                    "eb" => query.Where(p => p.BirthDate < startUtc),
                    "ap" => HandleApproximateSearch(query, startUtc),
                    _ => query.Where(p => p.BirthDate >= startUtc && p.BirthDate < endUtc)
                };
            }
            else if (parsedDate.HasValue)
            {
                var parsedUtc = EnsureUtc(parsedDate.Value);

                return prefix switch
                {
                    "eq" => query.Where(p => p.BirthDate == parsedUtc),
                    "ne" => query.Where(p => p.BirthDate != parsedUtc),
                    "lt" => query.Where(p => p.BirthDate < parsedUtc),
                    "gt" => query.Where(p => p.BirthDate > parsedUtc),
                    "le" => query.Where(p => p.BirthDate <= parsedUtc),
                    "ge" => query.Where(p => p.BirthDate >= parsedUtc),
                    "sa" => query.Where(p => p.BirthDate > parsedUtc),
                    "eb" => query.Where(p => p.BirthDate < parsedUtc),
                    "ap" => HandleApproximateSearch(query, parsedUtc),
                    _ => query.Where(p => p.BirthDate == parsedUtc)
                };
            }
            else
            {
                throw new InvalidOperationException("Either startOfDay/endOfDay or parsedDate must have a value");
            }
        }

        private DateTime EnsureUtc(DateTime dateTime)
        {
            var utc = dateTime.Kind == DateTimeKind.Utc 
                ? dateTime 
                : DateTime.SpecifyKind(dateTime.ToUniversalTime(), DateTimeKind.Utc);
            return new DateTime(utc.Ticks, DateTimeKind.Utc);
        }

        private IQueryable<Patient> HandleApproximateSearch(IQueryable<Patient> query, DateTime targetDate)
        {
            var targetDateUtc = EnsureUtc(targetDate);
            var nowUtc = DateTime.UtcNow;
            var tolerance = TimeSpan.FromDays(Math.Max(1, Math.Abs((nowUtc - targetDateUtc).TotalDays) * 0.10));
            var lowerBound = EnsureUtc(targetDateUtc - tolerance);
            var upperBound = EnsureUtc(targetDateUtc + tolerance);

            return query.Where(p => p.BirthDate >= lowerBound && p.BirthDate <= upperBound);
        }
    }
}
