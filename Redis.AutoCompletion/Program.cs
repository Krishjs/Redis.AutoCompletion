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
            using (var muxer = ConnectionMultiplexer.Connect(configuration: "127.0.0.1:6379"))
            {
                IDatabase conn = muxer.GetDatabase();
                muxer.Wait(conn.PingAsync());

                List<City> citys = CityDataSource.GetCitys();

                citys.ForEach(c =>
                {

                    conn.HashSetAsync("Citys:Data:" + c.Id.ToString(), c.ToHashEntries());


                    List<string> prefix = GetPrefix(c.Name);

                    prefix.Concat(GetPrefix(c.Code));

                    if (!string.IsNullOrEmpty(c.Name))
                        conn.SortedSetAdd("CityName", c.Name, 0);
                    if (!string.IsNullOrEmpty(c.Code))
                        conn.SortedSetAdd("CityCode", c.Code, 0);

                    foreach (var p in prefix)
                    {
                        conn.SortedSetAdd("Citys:index:" + p, c.Id, 0);
                    }
                });

                var namesval = conn.SortedSetRangeByRank("CityName");
                var codeval = conn.SortedSetRangeByRank("CityCode");
                foreach (var r in namesval)
                {
                    Console.WriteLine(r);
                }

                foreach (var r in codeval)
                {
                    Console.WriteLine(r);
                }

                Console.WriteLine(value: "Enter the search term");
                string s = Console.ReadLine();

                RedisValue[] rvs = conn.SortedSetRangeByRank("Citys:index:" + s);

                foreach (var r in rvs)
                {
                    RedisValue rvh = conn.HashGet("Citys:Data:" + r, "Name");
                    Console.WriteLine(rvh);
                }
                Console.ReadKey();
            }
        }

        public static List<string> GetPrefix(string word)
        {

            if (string.IsNullOrEmpty(word))
                return new List<string>();

            var hs = new List<string>();

            string[] wordsSplit = word.Split(separator: new char[] { ' ' });

            foreach (var w in wordsSplit)
            {
                int i = 2;
                for (; i <= w.Length;)
                {
                    hs.Add(w.Substring(0, i++));
                }

            }

            return hs;
        }
    }

    public static class RedisUtils
    {
        //Serialize in Redis format:
        public static HashEntry[] ToHashEntries(this object obj)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            return properties.Select(property => new HashEntry(property.Name, property.GetValue(obj) == null ? string.Empty : property.GetValue(obj).ToString())).ToArray();
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
