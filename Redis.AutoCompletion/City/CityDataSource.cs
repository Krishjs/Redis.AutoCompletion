using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityDataSource
{
    public static class CityDataSource
    {
        private static string cityxmlPath = AppDomain.CurrentDomain.BaseDirectory + @"City\CityList.xml";

        public static List<City> GetCitys()
        {
            var citys = new List<City>();
            using (DataSet citydt = new DataSet())
            {
                citydt.Locale = CultureInfo.InvariantCulture;
                citydt.ReadXml(cityxmlPath);
                citydt.Tables[0].AsEnumerable().ToList().ForEach(dt => citys.Add(GetCityFromDataRow(dt)));
            }
            return citys;
        }

        private static City GetCityFromDataRow(DataRow dr)
        {
            try
            {
                var newCity = new City();
                if (dr["city_pid"] != DBNull.Value)
                    newCity.Id = Convert.ToInt32(dr["city_pid"]);
                if (dr["city_code"] != DBNull.Value)
                    newCity.Code = Convert.ToString(dr["city_code"]);
                if (dr["city_name"] != DBNull.Value)
                    newCity.Name = Convert.ToString(dr["city_name"]);
                if (dr["city_state_id"] != DBNull.Value)
                    newCity.StateInfo.Id = Convert.ToInt32(dr["city_state_id"]);
                if (dr["country_pid"] != DBNull.Value)
                    newCity.StateInfo.CountryInfo.Id = Convert.ToInt32(dr["country_pid"]);
                if (dr["country_code"] != DBNull.Value)
                    newCity.StateInfo.CountryInfo.Code = Convert.ToString(dr["country_code"]);
                if (dr["country_name"] != DBNull.Value)
                    newCity.StateInfo.CountryInfo.Name = Convert.ToString(dr["country_name"]);
                if (dr["state_code"] != DBNull.Value)
                    newCity.StateInfo.Code = Convert.ToString(dr["state_code"]);
                if (dr["state_name"] != DBNull.Value)
                    newCity.StateInfo.Name = Convert.ToString(dr["state_name"]);
                return newCity;
            }
            catch
            {
                throw;
            }
        }
    }
}
