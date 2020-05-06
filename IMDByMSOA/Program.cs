using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace IMDByMSOA
{
    class Program
    {
        static void Main(string[] args)
        {
            // You may also be interested in the components of deprivation data. But I'm not using that here.
            // https://opendatacommunities.org/resource?uri=http%3A%2F%2Fopendatacommunities.org%2Fdata%2Fsocietal-wellbeing%2Fimd2019%2Findices

            // https://www.ons.gov.uk/peoplepopulationandcommunity/populationandmigration/populationestimates/datasets/lowersuperoutputareamidyearpopulationestimates
            Dictionary<string, double> LSOAPopulationsDictionary = new Dictionary<string, double>();
            using (StreamReader reader = new StreamReader(@"LSOAPopulation2018.csv"))
            {
                using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    LSOAPopulationsDictionary = csv.GetRecords<LSOAPopulation>().ToDictionary(x => x.LSOACode, x => x.MY2018AllPopulation);
                }
            }

            // https://www.ons.gov.uk/employmentandlabourmarket/peopleinwork/earningsandworkinghours/datasets/smallareaincomeestimatesformiddlelayersuperoutputareasenglandandwales
            Dictionary<string, IncomeByMSOA> IncomeByMSOADictionary = new Dictionary<string, IncomeByMSOA>();
            using (StreamReader reader = new StreamReader(@"netannualincomeafterhousingcosts2018.csv"))
            {
                using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    IncomeByMSOADictionary = csv.GetRecords<IncomeByMSOA>().ToDictionary(x => x.MSOAcode, x => x);
                }
            }

            // https://geoportal.statistics.gov.uk/datasets/output-area-to-lsoa-to-msoa-to-local-authority-district-december-2017-lookup-with-area-classifications-in-great-britain
            Dictionary<string, string> LSOAToMSOA = new Dictionary<string, string>();
            using (StreamReader reader = new StreamReader(@"Output_Area_to_LSOA_to_MSOA_to_Local_Authority_District_(December_2017)_Lookup_with_Area_Classifications_in_Great_Britain.csv"))
            {
                using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    List<OAToLSOAToMSOA> OALinks = csv.GetRecords<OAToLSOAToMSOA>().ToList();
                    foreach (OAToLSOAToMSOA oalink in OALinks)
                    {
                        if (LSOAToMSOA.ContainsKey(oalink.LSOA11CD) == false)
                        {
                            LSOAToMSOA.Add(oalink.LSOA11CD, oalink.MSOA11CD);
                        }
                    }
                }
            }

            // http://geoportal1-ons.opendata.arcgis.com/datasets/3db665d50b1441bc82bb1fee74ccc95a_0
            List<IMDByLSOA> IMDsByLSOA = new List<IMDByLSOA>();
            using (StreamReader reader = new StreamReader(@"Index_of_Multiple_Deprivation_(December_2019)_Lookup_in_England.csv"))
            {
                using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    IMDsByLSOA = csv.GetRecords<IMDByLSOA>().ToList();
                }
            }

            // Calculate a population-weighted deprivation score for each MSOA.
            List<IMDByMSOA> IMDsByMSOA = new List<IMDByMSOA>();
            foreach (string MSOA in LSOAToMSOA.Values.Distinct())
            {
                List<string> LSOAsInThisMSOA = LSOAToMSOA.Where(x => x.Value == MSOA).Select(x => x.Key).ToList();
                if (LSOAsInThisMSOA.Count > 0 && IMDsByLSOA.Where(x => LSOAsInThisMSOA.Contains(x.LSOA11CD)).Count() > 0)
                {
                    List<IMDByLSOA> LSOAsInMSOA = IMDsByLSOA.Where(x => LSOAsInThisMSOA.Contains(x.LSOA11CD)).ToList();

                    double MSOAPopulation = 0;
                    double MSOAPopulationXIMD19 = 0;
                    foreach(IMDByLSOA lsoa in LSOAsInMSOA)
                    {
                        double population = LSOAPopulationsDictionary[lsoa.LAD19CD];
                        double IMD19 = lsoa.IMD19;
                        MSOAPopulation += population;
                        MSOAPopulationXIMD19 += IMD19 * population;
                    }
                    IMDByMSOA iMDByMSOA = new IMDByMSOA()
                    {
                        IMD19 = MSOAPopulationXIMD19/MSOAPopulation,
                        MSOA = MSOA
                    };
                    IMDsByMSOA.Add(iMDByMSOA);
                }
            }

            // Join MSOAs with income and region data
            foreach(IMDByMSOA msoa in IMDsByMSOA)
            {
                IncomeByMSOA msoatojoin = IncomeByMSOADictionary[msoa.MSOA];
                msoa.MSOAname = msoatojoin.MSOAname;
                msoa.MSOAcode = msoatojoin.MSOAcode;
                msoa.LAcode = msoatojoin.LAcode;
                msoa.LAname = msoatojoin.LAname;
                msoa.Regioncode = msoatojoin.Regioncode;
                msoa.Regionname = msoatojoin.Regionname;
                msoa.NetAnnualIncomeAfterHousingCosts2018 = msoatojoin.NETAIAHC;
            }

            // Calculate deciles for income
            int MSOACount = IMDsByMSOA.Count;
            IMDsByMSOA = IMDsByMSOA.OrderBy(x => x.NetAnnualIncomeAfterHousingCosts2018).ToList();
            for (int i = 0; i < 10; i++)
            {
                IMDsByMSOA.Skip((MSOACount * i) / 10).Take(MSOACount / 10).ToList().ForEach(x => x.IncomeDecile = i + 1);
            }
            IMDsByMSOA.Where(x => x.IncomeDecile == 0).ToList().ForEach(x => x.IncomeDecile = 10);

            // Calculate deciles for IMD
            MSOACount = IMDsByMSOA.Count;
            IMDsByMSOA = IMDsByMSOA.OrderBy(x => x.IMD19).ToList();
            for (int i = 0; i < 10; i++)
            {
                IMDsByMSOA.Skip((MSOACount * i) / 10).Take(MSOACount / 10).ToList().ForEach(x => x.DeprivationDecile = i + 1);
            }
            // Sometimes one list element gets left with a decile of 0
            IMDsByMSOA.Where(x => x.DeprivationDecile == 0).ToList().ForEach(x => x.DeprivationDecile = 10);

            // Write the result
            using (TextWriter textWriter = File.CreateText(@"IMDByMSOA.csv"))
            {
                CsvWriter CSVwriter = new CsvWriter(textWriter, CultureInfo.InvariantCulture);
                CSVwriter.WriteRecords(IMDsByMSOA);
            }
        }
    }

    public class IncomeByMSOA
    {
        public string MSOAcode { get; set; }
        public string MSOAname { get; set; }
        public string LAcode { get; set; }
        public string LAname { get; set; }
        public string Regioncode { get; set; }
        public string Regionname { get; set; }
        public int NETAIAHC { get; set; }
    }

    public class LSOAPopulation
    {
        public string LSOACode { get; set; }
        public double MY2018AllPopulation { get; set; }
    }

    public class IMDByMSOA
    {
        public string MSOA { get; set; }
        public int Population2018 { get; set; }
        public int PopulationOver70 { get; set; }
        public double NetAnnualIncomeAfterHousingCosts2018 { get; set; }
        public int IncomeDecile { get; set; }
        public string MSOAcode { get; set; }
        public string MSOAname { get; set; }
        public string LAcode { get; set; }
        public string LAname { get; set; }
        public string Regioncode { get; set; }
        public string Regionname { get; set; }
        public double IMD19 { get; set; }
        public int DeprivationDecile { get; set; }
    }

    public class IMDByLSOA
    {
        public string LSOA11CD { get; set; }
        public string LSOA11NM { get; set; }
        public string LAD19CD { get; set; }
        public string LAD19NM { get; set; }
        public double IMD19 { get; set; }
        public int FID { get; set; }
    }

    public class OAToLSOAToMSOA
    {
        public string OA11CD { get; set; }
        public string OAC11CD { get; set; }
        public string OAC11NM { get; set; }
        public string LSOA11CD { get; set; }
        public string LSOA11NM { get; set; }
        public string SOAC11CD { get; set; }
        public string SOAC11NM { get; set; }
        public string MSOA11CD { get; set; }
        public string MSOA11NM { get; set; }
        public string LAD17CD { get; set; }
        public string LAD17NM { get; set; }
        public string LACCD { get; set; }
        public string LACNM { get; set; }
        public string RGN11CD { get; set; }
        public string RGN11NM { get; set; }
        public string CTRY11CD { get; set; }
        public string CTRY11NM { get; set; }
        public int FID { get; set; }
    }

}
