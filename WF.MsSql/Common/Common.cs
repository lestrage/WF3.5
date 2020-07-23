using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WF.MsSql
{
    public static class Common
    {
        public static string GetConfig(string code)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                                                              .Build();
            var value = configuration[code];
            return value;
        }
    }

    public class DataUpdateFinish
    {
        public Guid HoSoId { get; set; }
    }

    public class DocumentByFinish
    {
        public List<Guid> DataDocumentId { get; set; }
        public bool? IsFinish { get; set; }
    }
}
