using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using ARCHIVE.COMMON.Entities;

namespace CloudArchive.Services.GenerationPassService
{
        public static class GenerationPass
        {
        public static string GenerationPassword()
        
        {

            int length = 10;
            StringBuilder password = new StringBuilder();
            Random random = new Random();
            string[] randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    
            "abcdefghijkmnopqrstuvwxyz",    
            "0123456789",                   
            "!@$?_-" };
            while (password.Length < length)
            {
                foreach (string str in randomChars)
                {
                var c = str[random.Next(0, str.Length)];
                password.Append(c);
                if (password.Length == length)
                break;
                }
            }
            return password.ToString();
        }
        
    }
}
