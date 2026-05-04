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
        private StreamWriter _odbaceniWriter;
        private bool _disposed = false;

        private readonly string _kodDrzave;
        private int _indeksVremena;
        private int _indeksAktuelnog;
        private int _indeksPrognoze;

        public CsvReader(string putanjaFajla, string kodDrzave, string putanjaOdbacenih)
        {
            _kodDrzave = kodDrzave;
            _fileStream = new FileStream(putanjaFajla, FileMode.Open, FileAccess.Read);
            _streamReader = new StreamReader(_fileStream);
            _odbaceniWriter = new StreamWriter(putanjaOdbacenih, false);

            ProcitajZaglavlje();
        }

        private void ProcitajZaglavlje()
        {
            string zaglavlje = _streamReader.ReadLine();
            if (zaglavlje == null)
                throw new InvalidDataException("CSV fajl je prazan.");

            string[] kolone = zaglavlje.Split(',');

            _indeksVremena = PronadjiIndeks(kolone, "utc_timestamp");
            _indeksAktuelnog = PronadjiIndeks(kolone, _kodDrzave + "_load_actual_entsoe_transparency");
            _indeksPrognoze = PronadjiIndeks(kolone, _kodDrzave + "_load_forecast_entsoe_transparency");

            if (_indeksVremena < 0)
                throw new InvalidDataException("CSV ne sadrzi kolonu utc_timestamp.");

            if (_indeksAktuelnog < 0 || _indeksPrognoze < 0)
                throw new InvalidDataException("CSV ne sadrzi kolone za drzavu: " + _kodDrzave);
        }

        private static int PronadjiIndeks(string[] kolone, string naziv)
        {
            for (int i = 0; i < kolone.Length; i++)
            {
                if (kolone[i].Trim() == naziv)
                    return i;
            }
            return -1;
        }

        public List<DailyConsumptionSample> UcitajUzorke()
        {
            var dnevneGrupe = new Dictionary<DateTime, List<IntervalPodaci>>();
            int brojReda = 2;

            string linija;
            while ((linija = _streamReader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(linija))
                {
                    brojReda++;
                    continue;
                }

                string[] delovi = linija.Split(',');

                DateTime vreme;
                if (!TryParsirajVreme(delovi[_indeksVremena], out vreme))
                {
                    _odbaceniWriter.WriteLine(brojReda + "," + linija + ",Nije moguce parsirati timestamp");
                    brojReda++;
                    continue;
                }

                double aktuelno = ParsirajVrednost(delovi[_indeksAktuelnog]);
                double prognoza = ParsirajVrednost(delovi[_indeksPrognoze]);

                if (double.IsNaN(aktuelno) && double.IsNaN(prognoza))
                {
                    _odbaceniWriter.WriteLine(brojReda + "," + linija + ",Oba polja su NaN");
                    brojReda++;
                    continue;
                }

                DateTime dan = vreme.Date;
                if (!dnevneGrupe.ContainsKey(dan))
                    dnevneGrupe[dan] = new List<IntervalPodaci>();

                dnevneGrupe[dan].Add(new IntervalPodaci { Vreme = vreme, Aktuelno = aktuelno, Prognoza = prognoza });
                brojReda++;
            }

            return AgregirajDane(dnevneGrupe);
        }

        private List<DailyConsumptionSample> AgregirajDane(Dictionary<DateTime, List<IntervalPodaci>> grupe)
        {
            var uzorci = new List<DailyConsumptionSample>();
            int indeksUzorka = 1;

            foreach (DateTime dan in grupe.Keys.OrderBy(d => d))
            {
                List<IntervalPodaci> intervali = grupe[dan];

                double ukupnoAktuelno = 0;
                double ukupnoPrognoza = 0;
                double vrhAktuelno = double.NaN;
                DateTime vrhVreme = dan;

                foreach (IntervalPodaci interval in intervali)
                {
                    if (!double.IsNaN(interval.Aktuelno))
                    {
                        ukupnoAktuelno += interval.Aktuelno * 0.5;

                        if (double.IsNaN(vrhAktuelno) || interval.Aktuelno > vrhAktuelno)
                        {
                            vrhAktuelno = interval.Aktuelno;
                            vrhVreme = interval.Vreme;
                        }
                    }

                    if (!double.IsNaN(interval.Prognoza))
                        ukupnoPrognoza += interval.Prognoza * 0.5;
                }

                if (double.IsNaN(vrhAktuelno))
                    vrhAktuelno = 0;

                uzorci.Add(new DailyConsumptionSample
                {
                    Date = dan,
                    TotalActualMWh = ukupnoAktuelno,
                    TotalForecastMWh = ukupnoPrognoza,
                    PeakTime = vrhVreme,
                    PeakActualMW = vrhAktuelno,
                    CountryCode = _kodDrzave,
                    RowIndex = indeksUzorka++
                });
            }

            return uzorci;
        }

        private static bool TryParsirajVreme(string vrednost, out DateTime rezultat)
        {
            vrednost = vrednost.Trim().Trim('"');
            return DateTime.TryParse(vrednost, null, DateTimeStyles.None, out rezultat);
        }

        private static double ParsirajVrednost(string vrednost)
        {
            vrednost = vrednost.Trim().Trim('"');
            if (string.IsNullOrEmpty(vrednost))
                return double.NaN;

            double rezultat;
            if (!double.TryParse(vrednost, NumberStyles.Float, CultureInfo.InvariantCulture, out rezultat))
                return double.NaN;

            return rezultat;
        }

        public void Dispose()
        {
            if (_disposed) return;

            if (_streamReader != null)
                _streamReader.Dispose();

            if (_fileStream != null)
                _fileStream.Dispose();

            if (_odbaceniWriter != null)
                _odbaceniWriter.Dispose();

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private class IntervalPodaci
        {
            public DateTime Vreme { get; set; }
            public double Aktuelno { get; set; }
            public double Prognoza { get; set; }
        }
    }
}
