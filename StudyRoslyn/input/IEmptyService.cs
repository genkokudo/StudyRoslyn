using System;
using System.Collections.Generic;
using System.Text;

namespace StudyRoslyn.input
{
    public interface ITestService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        public (string hashed, string salt) Hash(string raw);
    }

    public class TestService : ITestService
    {
        public TestService()
        {
        }

        public (string hashed, string salt) Hash(string raw)
        {
            var hashed = string.Empty;
            var salt = string.Empty;

            return (hashed, salt);
        }
    }


}
