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

            // Deaths involving Covid-19, exceptional release 01 May 2020
            // https://www.ons.gov.uk/peoplepopulationandcommunity/birthsdeathsandmarriages/deaths/datasets/deathsinvolvingcovid19bylocalareaanddeprivation
            Dictionary<string, CVDeaths> CVDeathsMayDictionary = new Dictionary<string, CVDeaths>();
            using (StreamReader reader = new StreamReader(@"DeathsByMSOA_ONS1MayCovid19MortalityRelease.csv"))
            {
                using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    CVDeathsMayDictionary = csv.GetRecords<CVDeaths>().ToDictionary(x => x.MSOAcode, x => x);
                }
            }

            // Deaths involving Covid-19, exceptional release 12 June 2020.
            // https://www.ons.gov.uk/peoplepopulationandcommunity/birthsdeathsandmarriages/deaths/datasets/deathsinvolvingcovid19bylocalareaanddeprivation
            Dictionary<string, CVDeaths> CVDeathsJuneDictionary = new Dictionary<string, CVDeaths>();
            using (StreamReader reader = new StreamReader(@"DeathsByMSOA_ONS12JuneCovid19MortalityRelease.csv"))
            {
                using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    CVDeathsJuneDictionary = csv.GetRecords<CVDeaths>().ToDictionary(x => x.MSOAcode, x => x);
                }
            }

            // Historic death count by MSOA for England & Wales
            // https://www.ons.gov.uk/peoplepopulationandcommunity/birthsdeathsandmarriages/deaths/adhocs/006979numberofdeathsoccurringbymsoaand5yearagegroupsenglandandwales2004to2015
            Dictionary<string, HistoricDeathsByMSOA> HistoricDeathDictionary = new Dictionary<string, HistoricDeathsByMSOA>();
            using (StreamReader reader = new StreamReader(@"201320142015DeathsByMSOA.csv"))
            {
                using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    HistoricDeathDictionary = csv.GetRecords<HistoricDeathsByMSOA>().ToDictionary(x => x.MSOAcode, x => x);
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
            Dictionary<string, OAToLSOAToMSOA> LSOAToMSOADictionary = new Dictionary<string, OAToLSOAToMSOA>();
            using (StreamReader reader = new StreamReader(@"Output_Area_to_LSOA_to_MSOA_to_Local_Authority_District_(December_2017)_Lookup_with_Area_Classifications_in_Great_Britain.csv"))
            {
                using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    List<OAToLSOAToMSOA> OALinks = csv.GetRecords<OAToLSOAToMSOA>().ToList();
                    foreach (OAToLSOAToMSOA oalink in OALinks)
                    {
                        if (LSOAToMSOADictionary.ContainsKey(oalink.LSOA11CD) == false)
                        {
                            LSOAToMSOADictionary.Add(oalink.LSOA11CD, oalink);
                        }
                    }
                }
            }

            // MSOA Centroids and Boundaries (if you want them later for mapping or something. I'm not working with them here)
            // https://data.gov.uk/dataset/2cf1f346-2f74-4c06-bd4b-30d7e4df5ae7/middle-layer-super-output-area-msoa-boundaries


            // https://www.ons.gov.uk/peoplepopulationandcommunity/populationandmigration/populationestimates/datasets/middlesuperoutputareamidyearpopulationestimates
            Dictionary<string, MSOAPopulation> MSOAPopulationDictionary = new Dictionary<string, MSOAPopulation>();
            using (StreamReader reader = new StreamReader(@"MSOAPopulation2018.csv"))
            {
                using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    MSOAPopulationDictionary = csv.GetRecords<MSOAPopulation>().ToDictionary(x => x.MSOAcode, x => x);
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
            foreach (string MSOA in LSOAToMSOADictionary.Values.Select(x => x.MSOA11CD).Distinct())
            {
                List<string> LSOAsInThisMSOA = LSOAToMSOADictionary.Where(x => x.Value.MSOA11CD == MSOA).Select(x => x.Key).ToList();
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
                        IMD19Score = MSOAPopulationXIMD19/MSOAPopulation,
                        MSOA = MSOA
                    };
                    IMDsByMSOA.Add(iMDByMSOA);
                }
            }

            // Join MSOAs with income and region data. Really sorry but we lose Welsh data at this step, because IMDs for Wales are different.
            foreach(IMDByMSOA msoa in IMDsByMSOA)
            {
                // income and region data
                IncomeByMSOA msoatojoin = IncomeByMSOADictionary[msoa.MSOA];
                msoa.MSOAname = msoatojoin.MSOAname;
                msoa.MSOAcode = msoatojoin.MSOAcode;
                msoa.LAcode = msoatojoin.LAcode;
                msoa.LAname = msoatojoin.LAname;
                msoa.Regioncode = msoatojoin.Regioncode;
                msoa.Regionname = msoatojoin.Regionname;
                msoa.NetAnnualIncomeAfterHousingCosts2018 = msoatojoin.NETAIAHC;

                // historic death rate by msoa
                HistoricDeathsByMSOA historicdeathtojoin = HistoricDeathDictionary[msoa.MSOA];
                msoa.AllDeaths20132014And2015 = historicdeathtojoin.DeathsAllAges;
                msoa.ExpectedDeaths1MarTo17Apr2020 = msoa.AllDeaths20132014And2015 * 48 / (365 * 3);
                msoa.ExpectedDeathsMarAprMay2020 = msoa.AllDeaths20132014And2015 / (4 * 3); // https://twitter.com/thomasforth/status/1271458772402216961

                // Covid19 period deaths
                CVDeaths cvdeathsMay = CVDeathsMayDictionary[msoa.MSOA];
                msoa.ConfirmedCovid19Deaths1MarTo17Apr2020 = cvdeathsMay.DeathsCOVID19;
                msoa.AllDeaths1MarTo17Apr2020 = cvdeathsMay.DeathsAllCauses;

                CVDeaths cvdeathsJune = CVDeathsJuneDictionary[msoa.MSOA];
                msoa.ConfirmedCovid19DeathsMarAprMay2020 = cvdeathsJune.DeathsCOVID19;
                msoa.AllDeathsMarAprMay2020 = cvdeathsJune.DeathsAllCauses;                

                msoa.ExcessDeaths1MarTo17Apr2020 = msoa.AllDeaths1MarTo17Apr2020 - msoa.ExpectedDeaths1MarTo17Apr2020;
                msoa.DeathsAsPercentOfExpected1MarTo17Apr2020 = msoa.AllDeaths1MarTo17Apr2020 / msoa.ExpectedDeaths1MarTo17Apr2020;

                msoa.ExcessDeathsMarAprMay2020 = msoa.AllDeathsMarAprMay2020 - msoa.ExpectedDeathsMarAprMay2020;
                msoa.DeathsAsPercentOfExpectedMarAprMay2020 = msoa.AllDeathsMarAprMay2020 / msoa.ExpectedDeathsMarAprMay2020;

                // msoa population structure
                MSOAPopulation msoapop = MSOAPopulationDictionary[msoa.MSOA];
                msoa.Population2018 = msoapop.Population;
                msoa.PopulationOver70 = msoapop.Population70AndOver;
            }

            // Calculate national deciles (England) for income
            int MSOACount = IMDsByMSOA.Count;
            IMDsByMSOA = IMDsByMSOA.OrderBy(x => x.NetAnnualIncomeAfterHousingCosts2018).ToList();
            decimal remainder = 0;
            for (int i = 0; i < 10; i++)
            {
                int totake = (int)((MSOACount* i / 10) + remainder);
                remainder = (totake % 10) / 10;
                IMDsByMSOA.Skip(totake).ToList().ForEach(x => x.IncomeDecile_withinEngland = i + 1);
            }

            // Calculate national (England) deciles for IMD
            IMDsByMSOA = IMDsByMSOA.OrderBy(x => x.IMD19Score).ToList();
            remainder = 0;
            for (int i = 0; i < 10; i++)
            {
                int totake = (int)((MSOACount * i / 10) + remainder);
                remainder = (totake % 10) / 10;
                IMDsByMSOA.Skip(totake).ToList().ForEach(x => x.DeprivationDecile_withinEngland = i + 1);
            }            

            List<string> UniqueRegions = IMDsByMSOA.Select(x => x.Regioncode).Distinct().ToList();           
            foreach (string regioncode in UniqueRegions)
            {
                // Calculate regional deciles (England) for income
                List<IMDByMSOA> MSOAsInRegion = IMDsByMSOA.Where(x => x.Regioncode == regioncode).ToList();
                int MSOACountInRegion = MSOAsInRegion.Count;
                MSOAsInRegion = MSOAsInRegion.OrderBy(x => x.NetAnnualIncomeAfterHousingCosts2018).ToList();
                remainder = 0;
                for (int i = 0; i < 10; i++)
                {
                    int totake = (int)((MSOACountInRegion * i / 10) + remainder);
                    remainder = (totake % 10) / 10;
                    MSOAsInRegion.Skip(totake).ToList().ForEach(x => x.IncomeDecile_withinRegion = i + 1);
                }

                // Calculate regional (England) deciles for IMD
                MSOAsInRegion = MSOAsInRegion.OrderBy(x => x.IMD19Score).ToList();
                remainder = 0;
                for (int i = 0; i < 10; i++)
                {
                    int totake = (int)((MSOACountInRegion * i / 10) + remainder);
                    remainder = (totake % 10) / 10;
                    MSOAsInRegion.Skip(totake).ToList().ForEach(x => x.DeprivationDecile_withinRegion = i + 1);
                }
            }

            List<string> UniqueLAs = IMDsByMSOA.Select(x => x.LAcode).Distinct().ToList();
            foreach (string LAcode in UniqueLAs)
            {
                // Calculate LA deciles (England) for income
                List<IMDByMSOA> MSOAsInLA = IMDsByMSOA.Where(x => x.LAcode == LAcode).ToList();
                int MSOACountInLA = MSOAsInLA.Count;
                MSOAsInLA = MSOAsInLA.OrderBy(x => x.NetAnnualIncomeAfterHousingCosts2018).ToList();
                remainder = 0;
                for (int i = 0; i < 10; i++)
                {
                    int totake = (int)((MSOACountInLA * i / 10) + remainder);
                    remainder = (totake % 10) / 10;
                    MSOAsInLA.Skip(totake).ToList().ForEach(x => x.IncomeDecile_withinLA = i + 1);
                }
               
                // Calculate LA deciles (England) for IMD
                MSOAsInLA = MSOAsInLA.OrderBy(x => x.IMD19Score).ToList();
                remainder = 0;
                for (int i = 0; i < 10; i++)
                {
                    int totake = (int)((MSOACountInLA * i / 10) + remainder); 
                    remainder = (totake % 10)/10;
                    MSOAsInLA.Skip(totake).ToList().ForEach(x => x.DeprivationDecile_withinLA = i + 1);
                }
            }

            // Write the result
            using (TextWriter textWriter = File.CreateText(@"IMDByMSOA_AndCovidData_AndIncomeData.csv"))
            {
                CsvWriter CSVwriter = new CsvWriter(textWriter, CultureInfo.InvariantCulture);
                CSVwriter.WriteRecords(IMDsByMSOA);
            }
        }
    }

    public class MSOAPopulation
    {
        public string MSOAcode { get; set; }
        public int Population { get; set; }
        public int Population70AndOver { get; set; }
    }

    public class HistoricDeathsByMSOA
    {
        public string Years { get; set; }
        public string MSOAcode { get; set; }
        public long DeathsAllAges { get; set; }
    }

    public class CVDeaths
    {
        public string MSOAcode { get; set; }
        public string MSOAname { get; set; }
        public long DeathsAllCauses { get; set; }
        public long DeathsCOVID19 { get; set; }
    }

    public class IncomeByMSOA
    {
        public string MSOAcode { get; set; }
        public string MSOAname { get; set; }
        public string LAcode { get; set; }
        public string LAname { get; set; }
        public string Regioncode { get; set; }
        public string Regionname { get; set; }
        public long NETAIAHC { get; set; }
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
        public int IncomeDecile_withinEngland { get; set; }
        public int IncomeDecile_withinRegion { get; set; }
        public int IncomeDecile_withinLA { get; set; }
        public string MSOAcode { get; set; }
        public string MSOAname { get; set; }
        public string LAcode { get; set; }
        public string LAname { get; set; }
        public string Regioncode { get; set; }
        public string Regionname { get; set; }
        public double IMD19Score { get; set; }
        public int DeprivationDecile_withinEngland { get; set; }
        public int DeprivationDecile_withinRegion { get; set; }
        public int DeprivationDecile_withinLA { get; set; }
        public double DeathsAsPercentOfExpected1MarTo17Apr2020 { get; set; }
        public double DeathsAsPercentOfExpectedMarAprMay2020 { get; set; }
        public double ExcessDeaths1MarTo17Apr2020 { get; set; }
        public double ExpectedDeaths1MarTo17Apr2020 { get; set; }
        public double ExcessDeathsMarAprMay2020 { get; set; }
        public double ExpectedDeathsMarAprMay2020 { get; set; }
        public long AllDeaths1MarTo17Apr2020 { get; set; }
        public long ConfirmedCovid19Deaths1MarTo17Apr2020 { get; set; }
        public long AllDeathsMarAprMay2020 { get; set; }
        public long ConfirmedCovid19DeathsMarAprMay2020 { get; set; }
        public long AllDeaths20132014And2015 { get; set; }
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