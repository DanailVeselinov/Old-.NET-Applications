using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetFairBot
{
    public class CountryCode
    {
        string countryCode;

        public string CountryCode1
        {
            get { return countryCode; }
            set { countryCode = value; }
        }
        string countryName;

        public string CountryName
        {
            get { return countryName; }
            set { countryName = value; }
        }
        public CountryCode(string code, string country)
        {
            this.countryCode = code;
            this.countryName = country;
        }
    }

    public class Market
    {
        public string Name { get; set; }
        public string TypeCode { get; set; }
        public Market(string name, string type)
        {
            Name = name;
            TypeCode = type;
        }
    }

    public class MarketOptions
    {
        public string MarketName { get; set; }
        public string MarketTypeName { get; set; }

        public List<string> MarketOptionsList { get; set; }
        public MarketOptions(string name, string marketType, List<string> options)
        {
            MarketName = name;
            MarketTypeName = marketType;
            MarketOptionsList = options;
        }

        public override bool Equals(object obj)
        {
            try
            {
                var marketOption = obj as MarketOptions;
                if (marketOption == null)
                {
                    return false;
                }
                if (this.MarketTypeName == marketOption.MarketTypeName)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
