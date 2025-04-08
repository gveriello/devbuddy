// File: DevBuddy.Plugins.CronExpression/Services/CronService.cs
using Cronos;
using devbuddy.plugins.CronExpression.Models;

namespace devbuddy.plugins.CronExpression.Business.Services
{
    public class CronService
    {
        public static List<CronPreset> GetCommonPresets()
        {
            return
            [
                new() { Name = "Ogni minuto", Expression = "* * * * *", Description = "Si esegue ogni minuto" },
                new() { Name = "Ogni ora", Expression = "0 * * * *", Description = "Si esegue all'inizio di ogni ora" },
                new() { Name = "Ogni giorno a mezzanotte", Expression = "0 0 * * *", Description = "Si esegue alle 12:00 AM ogni giorno" },
                new() { Name = "Ogni lunedì", Expression = "0 0 * * 1", Description = "Si esegue alle 12:00 AM ogni Lunedì" },
                new() { Name = "Primo di ogni mese", Expression = "0 0 1 * *", Description = "Si esegue alle 12:00 AM il primo giorno di ogni mese" },
                new() { Name = "Ogni 15 minuti", Expression = "*/15 * * * *", Description = "Si esegue ogni 15 minuti" },
                new() { Name = "Giorni feriali alle 9 AM", Expression = "0 9 * * 1-5", Description = "Si esegue alle 9:00 AM da Lunedì a Venerdì" },
                new() { Name = "Weekend alle 10 AM", Expression = "0 10 * * 0,6", Description = "Si esegue alle 10:00 AM solo Sabato e Domenica" },
                new() { Name = "Ogni trimestre", Expression = "0 0 1 1,4,7,10 *", Description = "Si esegue alle 12:00 AM il primo giorno di ogni trimestre (Gennaio, Aprile, Luglio, Ottobre)" },
                new() { Name = "Due volte al giorno", Expression = "0 8,20 * * *", Description = "Si esegue alle 8:00 AM e 8:00 PM ogni giorno" }
            ];
        }

        private static string GetMonthName(int month)
        {
            return month switch
            {
                1 => "Gennaio",
                2 => "Febbraio",
                3 => "Marzo",
                4 => "Aprile",
                5 => "Maggio",
                6 => "Giugno",
                7 => "Luglio",
                8 => "Agosto",
                9 => "Settembre",
                10 => "Ottobre",
                11 => "Novembre",
                12 => "Dicembre",
                _ => month.ToString()
            };
        }

        private static string GetDayName(int day)
        {
            return day switch
            {
                0 => "Domenica",
                1 => "Lunedì",
                2 => "Martedì",
                3 => "Mercoledì",
                4 => "Giovedì",
                5 => "Venerdì",
                6 => "Sabato",
                7 => "Domenica",
                _ => day.ToString()
            };
        }

        public static List<CronScheduleResult> GetNextOccurrences(string expression, int count = 5)
        {
            var results = new List<CronScheduleResult>();

            try
            {
                // Parse the expression
                var cronExpression = Cronos.CronExpression.Parse(expression);

                // Get the next 'count' occurrences
                DateTime? nextUtc = DateTime.UtcNow;
                for (int i = 0; i < count; i++)
                {
                    nextUtc = cronExpression.GetNextOccurrence(nextUtc.Value);

                    if (nextUtc.HasValue)
                    {
                        results.Add(new CronScheduleResult
                        {
                            DateTime = nextUtc.Value.ToLocalTime(),
                            IsValid = true
                        });

                        // Add 1 second to get the next occurrence after this one
                        nextUtc = nextUtc.Value.AddSeconds(1);
                    }
                    else
                    {
                        break;  // No more occurrences
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                // Return a single result with error details
                return new List<CronScheduleResult>
                {
                    new CronScheduleResult
                    {
                        IsValid = false,
                        ErrorMessage = $"Cron expression non valida: {ex.Message}"
                    }
                };
            }
        }

        public static string GetExpressionDescription(string expression)
        {
            try
            {
                var parts = expression.Split(' ');
                if (parts.Length < 5) return "Il formato della cron expression non è valida.";

                var minute = DescribeMinuteExpression(parts[0]);
                var hour = DescribeHourExpression(parts[1]);
                var dayOfMonth = DescribeDayOfMonthExpression(parts[2]);
                var month = DescribeMonthExpression(parts[3]);
                var dayOfWeek = DescribeDayOfWeekExpression(parts[4]);

                return $"Eseguito {minute}, {hour}, {dayOfMonth}, {month}, {dayOfWeek}";
            }
            catch (Exception)
            {
                return "Mi dispiace ma non sono riuscito a parsare la cron expression.";
            }
        }

        private static string DescribeMinuteExpression(string minute)
        {
            if (minute == "*") return "ogni minuto";  
            if (minute == "0") return "all'inizio dell'ora";  
            if (minute.StartsWith("*/"))
            {
                var interval = int.Parse(minute.Substring(2));
                return $"ogni {interval} minut{(interval > 1 ? "i" : "o")}";  
            }
            if (minute.Contains(","))
            {
                var minutes = minute.Split(',');
                return $"al minut{(minutes.Length > 1 ? "i" : "o")} {string.Join(", ", minutes)}";  
            }
            if (minute.Contains("-"))
            {
                var range = minute.Split('-');
                return $"ogni minuto da {range[0]} a {range[1]}";  
            }
            return $"al minuto {minute}"; 
        }

        private static string DescribeHourExpression(string hour)
        {
            if (hour == "*") return "ogni ora";  
            if (hour == "0") return "a mezzanotte";  
            if (hour == "12") return "a mezzogiorno";
            if (hour.StartsWith("*/"))
            {
                var interval = int.Parse(hour.Substring(2));
                return $"ogni {interval} or{(interval > 1 ? "e" : "a")}"; 
            }
            if (hour.Contains(","))
            {
                var hours = hour.Split(',');
                return $"all'or{(hours.Length > 1 ? "e" : "a")} {string.Join(", ", hours)}"; 
            }
            if (hour.Contains("-"))
            {
                var range = hour.Split('-');
                return $"ogni ora da {range[0]} a {range[1]}"; 
            }
            return $"alle {hour}:00"; 
        }

        private static string DescribeDayOfMonthExpression(string dayOfMonth)
        {
            if (dayOfMonth == "*") return "ogni giorno del mese"; 
            if (dayOfMonth.StartsWith("*/"))
            {
                var interval = int.Parse(dayOfMonth.Substring(2));
                return $"ogni {interval} giorn{(interval > 1 ? "i" : "o")} del mese";  
            }
            if (dayOfMonth.Contains(","))
            {
                var days = dayOfMonth.Split(',');
                return $"il giorn{(days.Length > 1 ? "i" : "o")} {string.Join(", ", days)} del mese";  
            }
            if (dayOfMonth.Contains("-"))
            {
                var range = dayOfMonth.Split('-');
                return $"nei giorni dal {range[0]} al {range[1]} del mese";  
            }
            return $"il giorno {dayOfMonth} del mese";  
        }

        private static string DescribeMonthExpression(string month)
        {
            if (month == "*") return "ogni mese";  
            if (month.StartsWith("*/"))
            {
                var interval = int.Parse(month.Substring(2));
                return $"ogni {interval} mes{(interval > 1 ? "i" : "e")}";  
            }
            if (month.Contains(","))
            {
                var months = month.Split(',');
                var monthNames = months.Select(m => GetMonthName(int.Parse(m))).ToList();
                return $"in {string.Join(", ", monthNames)}";  
            }
            if (month.Contains("-"))
            {
                var range = month.Split('-');
                return $"da {GetMonthName(int.Parse(range[0]))} a {GetMonthName(int.Parse(range[1]))}";  
            }
            return $"in {GetMonthName(int.Parse(month))}"; 
        }

        private static string DescribeDayOfWeekExpression(string dayOfWeek)
        {
            if (dayOfWeek == "*") return "ogni giorno della settimana"; 
            if (dayOfWeek.StartsWith("*/"))
            {
                var interval = int.Parse(dayOfWeek.Substring(2));
                return $"ogni {interval} giorn{(interval > 1 ? "i" : "o")} della settimana";  
            }
            if (dayOfWeek.Contains(","))
            {
                var days = dayOfWeek.Split(',');
                var dayNames = days.Select(d => GetDayName(int.Parse(d))).ToList();
                return $"il {string.Join(", ", dayNames)}";  
            }
            if (dayOfWeek.Contains("-"))
            {
                var range = dayOfWeek.Split('-');
                return $"dal {GetDayName(int.Parse(range[0]))} al {GetDayName(int.Parse(range[1]))}";  
            }
            return $"il {GetDayName(int.Parse(dayOfWeek))}";  
        }

        public static bool ValidateCronExpression(string expression)
        {
            try
            {
                Cronos.CronExpression.Parse(expression);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}