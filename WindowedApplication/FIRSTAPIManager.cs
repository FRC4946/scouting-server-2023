using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;

namespace WindowedApplication
{
    internal class FIRSTAPIManager
    {
        public async Task<RestResponse> _getSchedule(string auth, string eventCode)
        {
            var client = new RestClient("https://frc-api.firstinspires.org/v3.0/");
            client.Options.MaxTimeout = -1;
            var request = new RestRequest("{season}/scores/{eventCode}/{tournamentLevel}", Method.Get)
                .AddHeader("Authorization", "Basic " + auth)
                .AddUrlSegment("season", 2023) //do this later you idiot
                .AddUrlSegment("eventCode", eventCode)
                .AddUrlSegment("tournamentLevel", "Qual");
            var cancellationTokenSource = new CancellationTokenSource();
            return await client.GetAsync(request);
        }

        public async Task<RestResponse> _getMatchData(string auth, string eventCode)
        {
            var client = new RestClient("https://frc-api.firstinspires.org/v3.0/");
            client.Options.MaxTimeout = -1;
            var request = new RestRequest("{season}/scores/{eventCode}/{tournamentLevel}", Method.Get)
                .AddHeader("Authorization", "Basic " + auth)
                .AddUrlSegment("season", 2023) //do this later you idiot
                .AddUrlSegment("eventCode", eventCode)
                .AddUrlSegment("tournamentLevel", "Qual");
            var cancellationTokenSource = new CancellationTokenSource();
            return await client.GetAsync(request);
        }
    }
}
 