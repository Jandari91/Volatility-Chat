using Akka.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RealTimeChat.Common
{
    public class HoconHelper
    {
        /// <summary>
        ///  현재 호출된 실행 파일명과 동일한 .hocon 파일을 찾아서 Config로 오픈한다.
        ///  ex) Mirero.MLS.TestApp.Service.exe -> Mirero.MLS.TestApp.Service.hocon 파일을 로딩한다.
        /// </summary>
        /// <returns></returns>
        public static Config ReadConfigurationFromHoconFile(Assembly assembly)
        {
            return ReadConfigurationFromHoconFile(assembly, "hocon");
        }

        public static Config ReadConfigurationFromHoconFile(Assembly assembly, string hoconFileExtension)
        {
            var assemblyFilePath = new Uri(assembly.GetName().CodeBase).LocalPath;
            var assemblyDirectoryPath = Path.GetDirectoryName(assemblyFilePath);
            var hoconFileName = Path.GetFileNameWithoutExtension(assemblyFilePath);
            var hoconFilePath = $@"{assemblyDirectoryPath}{Path.DirectorySeparatorChar}{hoconFileName}.{hoconFileExtension}";
            return ConfigurationFactory.ParseString(File.ReadAllText(hoconFilePath));
        }
    }
}
