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
        //TODO: make smt to vibe check all prior results for updates.
        //TODO: make this secure if i can cause ik imma fuck this up somehow, also a way to edit the calls later tho shh
        public RestResponse _getSchedule(string auth, string eventCode)
        {
            var client = new RestClient("https://frc-api.firstinspires.org/v3.0/");
            client.Options.MaxTimeout = -1;
            var request = new RestRequest("{season}/schedule/{eventCode}")
                .AddHeader("Authorization", "Basic " + auth)
                .AddUrlSegment("season", 2023) //do this later you idiot
                .AddUrlSegment("eventCode", eventCode)
                .AddParameter("tournamentLevel", "Qualification");
            return client.Execute(request);
        }

        public async Task<RestResponse> _getMatchData(string auth, string eventCode)
        {
            var client = new RestClient("https://frc-api.firstinspires.org/v3.0/");
            client.Options.MaxTimeout = -1;
            var request = new RestRequest("{season}/scores/{eventCode}", Method.Get)
                .AddHeader("Authorization", "Basic " + auth)
                .AddUrlSegment("season", 2015) //do this later you idiot
                .AddUrlSegment("eventCode", eventCode)
                .AddParameter("tournamentLevel", "Qualification");
            var cancellationTokenSource = new CancellationTokenSource();
            return await client.GetAsync(request);
        }
    }
}
 