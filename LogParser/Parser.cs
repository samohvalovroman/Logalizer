using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace LogParser
{
    public class Parser
    {
        public IEnumerable<AccessLog> Accesslog { get; set; }
        public IEnumerable<ErrorLog> Errorlog { get; set; }
        public string AccessFolderPath { get; set; }
        public string ErrorFolderPath { get; set; }
        public DateTime AccessDateFrom { get; set; }
        public DateTime AccessDateTo { get; set; }
        public DateTime ErrorDateFrom { get; set; }
        public DateTime ErrorDateTo { get; set; }
        public int TotalAccessEntries { get; set; }
        public int UniqueAccessEntries { get; set; }
        public int AvgAccessSize { get; set; }
        public int AvgAccessSession { get; set; }
        public int NotFoundErrorCount { get; set; }
        public int TotalErrorEntries { get; set; }
        public string SortedErrors { get; set; }
        public string TopErrorClient { get; set; }

        public void CountTotalAccessEntries()
        {
            TotalAccessEntries = Accesslog.Count();
        }

        public void CountUniqueAccessEntries()
        {
            UniqueAccessEntries = Accesslog.GroupBy(x => x.ClientIp).Count();
        }

        public void CountAvgAccessSize()
        {
            AvgAccessSize = (int)Accesslog.Average(x => x.Bytes);
        }

        public void CountAccessSession()
        {
            AvgAccessSession = (int)Accesslog.GroupBy(x => x.ClientIp).Average(x => (x.Last().DateTime - x.First().DateTime).TotalSeconds);
        }

        public void CountNotFoundError()
        {
            NotFoundErrorCount = Accesslog.Where(x => x.StatusCode == 404).Count();
        }

        public void CountTotalErrorEntries()
        {
            TotalErrorEntries = Errorlog.Count();
        }

        public void CountSortedErrors()
        {
            SortedErrors = String.Empty;
            var tmp = Errorlog.GroupBy(x => x.ErrorType);
            foreach (var t in tmp) SortedErrors += t.Count() + " ";
        }

        public void CountTopErrorClient()
        {
            TopErrorClient = Errorlog.GroupBy(x => x.ClientIp).OrderByDescending(x => x.Count()).First().Key;
        }

        public static GeoData GetCoords(string address)
        {
            var coords = new GeoData() { Ip = address, Latitude = 0.0, Longitude = 0.0 };
            var requestUri = "http://freegeoip.net/xml/" + address;
            WebRequest request;

            try
            {
                request = WebRequest.Create(requestUri);
                request.Timeout = 1000;
                request.GetResponse();
            }
            catch (Exception) { return coords; }

            var response = request.GetResponse();
            var xdoc = XDocument.Load(response.GetResponseStream());


            var xmlResponse = xdoc.Element("Response");
            if (xmlResponse != null)
            {
                coords.Ip = address;
                var xElement1 = xmlResponse.Element("CountryName");
                if (xElement1 != null)
                {
                    var country = xElement1.Value;
                    coords.CountryName = country;

                }
                var xElement2 = xmlResponse.Element("RegionName");
                if (xElement2 != null)
                {
                    var region = xElement2.Value;
                    coords.Region = region;

                }
                var xElement3 = xmlResponse.Element("City");
                if (xElement3 != null)
                {
                    var city = xElement3.Value;
                    coords.City = city;

                }
                var xElement4 = xmlResponse.Element("Latitude");
                if (xElement4 != null)
                {
                    var lat = xElement4.Value;
                    coords.Latitude = Convert.ToDouble(lat, CultureInfo.InvariantCulture.NumberFormat);

                }
                var xElement5 = xmlResponse.Element("Longitude");
                if (xElement5 != null)
                {
                    var lng = xElement5.Value;
                    coords.Longitude = Convert.ToDouble(lng, CultureInfo.InvariantCulture.NumberFormat);
                }
            }

            return coords;
        }
    }

    public class GeoData
    {
        public string Ip { get; set; }
        public string CountryName { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

}
