namespace Redis.AutoCompletion
{
    using CityDataSource;
    using StackExchange.Redis;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class Program
    {
        static void Main(string[] args)
        {
            using (var muxer = ConnectionMultiplexer.Connect("127.0.0.1:6379"))
            {   
                IDatabase conn = muxer.GetDatabase();
                muxer.Wait(conn.PingAsync());

                List<City> citys = CityDataSource.GetCitys();

                citys.ForEach(c => 
                {
                    conn. c.ToHashEntries()
                });
                
            }
        }     
    }

    public static class RedisUtils
    {
        //Serialize in Redis format:
        public static HashEntry[] ToHashEntries(this object obj)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            return properties.Select(property => new HashEntry(property.Name, property.GetValue(obj).ToString())).ToArray();
        }
        //Deserialize from Redis format
        public static T ConvertFromRedis<T>(this HashEntry[] hashEntries)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            object obj = Activator.CreateInstance(typeof(T));
            foreach (var property in properties)
            {
                HashEntry entry = hashEntries.FirstOrDefault(g => g.Name.ToString().Equals(property.Name));
                if (entry.Equals(new HashEntry())) continue;
                property.SetValue(obj, Convert.ChangeType(entry.Value.ToString(), property.PropertyType));
            }
            return (T)obj;
        }
    }
}
