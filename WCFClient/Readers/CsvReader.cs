using Common.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace WCFClient.Readers
{
    public class CsvReader : IDisposable
    {
        private FileStream _fileStream;
        private StreamReader _streamReader;
        private StreamWriter _rejectedWriter;
        private bool _disposed = false;

        private readonly string _countryCode;
        private int _timeIndex;
        private int _actualIndex;
        private int _forecastIndex;

        public CsvReader(string filePath, string countryCode, string rejectedPath)
        {
            _countryCode = countryCode;
            _fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            _streamReader = new StreamReader(_fileStream);
            _rejectedWriter = new StreamWriter(rejectedPath, false);

            ReadHeader();
        }

        private void ReadHeader()
        {
            string header = _streamReader.ReadLine();
            if(header == null) throw new InvalidDataException("CSV file is empty.");

            string[] cols = header.Split(',');

            _timeIndex = FindIndex(cols, "utc_timestamp");
            _actualIndex = FindIndex(cols, _countryCode + "_load_actual_entsoe_transparency");
            _forecastIndex = FindIndex(cols, _countryCode + "_load_forecast_entsoe_transparency");

            if (_timeIndex < 0) throw new InvalidDataException("CSV does not contain col utc_timestamp.");

            if (_actualIndex < 0 || _forecastIndex < 0) throw new InvalidDataException("CSV does not contain cols for country: " + _countryCode);
        }

        private static int FindIndex(string[] cols, string name)
        {
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i].Trim() == name)
                    return i;
            }
            return -1;
        }

        public List<DailyConsumptionSample> LoadSamples()
        {
            Dictionary<DateTime, List<IntervalData>> dailyGroups = new Dictionary<DateTime, List<IntervalData>>();
            int rowNum = 2;

            string line;
            while ((line = _streamReader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    rowNum++;
                    continue;
                }

                string[] parts = line.Split(',');

                DateTime time;
                if (!TryParseTime(parts[_timeIndex], out time))
                {
                    _rejectedWriter.WriteLine(line + "," + line + ", impossible to parse timestamp");
                    rowNum++;
                    continue;
                }

                double actual = ParseValue(parts[_actualIndex]);
                double forecast = ParseValue(parts[_forecastIndex]);

                if (double.IsNaN(actual) && double.IsNaN(forecast))
                {
                    _rejectedWriter.WriteLine(rowNum + ", " + line + ", both fields are not a number");
                    rowNum++;
                    continue;
                }

                DateTime day = time.Date;
                if (!dailyGroups.ContainsKey(day)) dailyGroups[day] = new List<IntervalData>();

                dailyGroups[day].Add(new IntervalData { Time = time, Actual = actual, Forecast = forecast });
                rowNum++;
            }

            return AggregateDays(dailyGroups);
        }

        private List<DailyConsumptionSample> AggregateDays(Dictionary<DateTime, List<IntervalData>> groups)
        {
            List<DailyConsumptionSample> samples = new List<DailyConsumptionSample>();
            int sampleIndex = 1;

            foreach (DateTime day in groups.Keys.OrderBy(d => d))
            {
                List<IntervalData> intervals = groups[day];

                double totalActual = 0;
                double totalForecast = 0;
                double peakActual = double.NaN;
                DateTime peakTime = day;

                foreach (IntervalData interval in intervals)
                {
                    if (!double.IsNaN(interval.Actual))
                    {
                        totalActual += interval.Actual * 0.5;

                        if (double.IsNaN(peakActual) || interval.Actual > peakActual)
                        {
                            peakActual = interval.Actual;
                            peakTime = interval.Time;
                        }
                    }

                    if (!double.IsNaN(interval.Forecast)) totalForecast += interval.Forecast * 0.5;
                }

                if (double.IsNaN(peakActual)) peakActual = 0;

                samples.Add(new DailyConsumptionSample
                {
                    Date = day,
                    TotalActualMWh = totalActual,
                    TotalForecastMWh = totalForecast,
                    PeakTime = peakTime,
                    PeakActualMW = peakActual,
                    CountryCode = _countryCode,
                    RowIndex = sampleIndex++
                });
            }

            return samples;
        }

        private static bool TryParseTime(string value, out DateTime result)
        {
            value = value.Trim().Trim('"');
            return DateTime.TryParse(value, null, DateTimeStyles.None, out result);
        }

        private static double ParseValue(string value)
        {
            value = value.Trim().Trim('"');
            if (string.IsNullOrEmpty(value)) return double.NaN;

            double result;
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result)) return double.NaN;

            return result;
        }

        public void Dispose()
        {
            if (_disposed) return;

            if (_streamReader != null) _streamReader.Dispose();

            if (_fileStream != null) _fileStream.Dispose();

            if (_rejectedWriter != null) _rejectedWriter.Dispose();

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private class IntervalData
        {
            public DateTime Time { get; set; }
            public double Actual { get; set; }
            public double Forecast { get; set; }
        }
    }
}
