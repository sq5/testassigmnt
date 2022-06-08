using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudArchive.Services
{
    public interface IBackgroundServiceLog
    {
        void AddInfo( string message, string service, int clientid = 0);
        void AddInfoProcFol( int clientId,string message, string service);
        void AddError(string message, string service, int clientId =  0);       
    }
       
}
       
    

