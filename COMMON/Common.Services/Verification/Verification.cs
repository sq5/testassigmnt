using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Verify.V2.Service;

namespace CloudArchive.Services
{
    public interface IVerification
    {
        Task<VerificationResult> StartVerificationAsync(string phoneNumber, string channel);

        Task<VerificationResult> CheckVerificationAsync(string phoneNumber, string code);
    }

    public class Verification : IVerification
    {
        private readonly Configuration.TwilioConfig _config;

        public Verification(IConfiguration cfg)
        {
            _config = new Configuration.TwilioConfig();
            _config.AccountSid = cfg["Twilio:TWILIO_ACCOUNT_SID"];
            _config.AuthToken = cfg["Twilio:TWILIO_AUTH_TOKEN"];
            _config.VerificationSid = cfg["Twilio:TWILIO_VERIFICATION_SID"];
            TwilioClient.Init(_config.AccountSid, _config.AuthToken);
        }

        public async Task<VerificationResult> StartVerificationAsync(string phoneNumber, string channel)
        {
            try
            {
                var verificationResource = await VerificationResource.CreateAsync(
                    to: phoneNumber,
                    channel: channel,
                    pathServiceSid: _config.VerificationSid,
                    locale: "ru"
                );
                return new VerificationResult(verificationResource.Sid);
            }
            catch (TwilioException e)
            {
                return new VerificationResult(new List<string>{e.Message});
            }
        }

        public async Task<VerificationResult> CheckVerificationAsync(string phoneNumber, string code)
        {
            try
            {
                var verificationCheckResource = await VerificationCheckResource.CreateAsync(
                    to: phoneNumber,
                    code: code,
                    pathServiceSid: _config.VerificationSid
                );
                return verificationCheckResource.Status.Equals("approved") ?
                    new VerificationResult(verificationCheckResource.Sid) :
                    new VerificationResult(new List<string>{"Некорректный код. Попробуйте еще раз"});
            }
            catch (TwilioException e)
            {
                return new VerificationResult(new List<string>{e.Message});
            }
        }
    }
}
