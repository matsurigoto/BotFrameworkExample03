using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using RestSharp;
using System.Collections.Generic;

namespace PMBotApplication.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            if (activity.Text.IndexOf("空氣品質") >= 0 || activity.Text.IndexOf("PM2.5") >= 0)
            {
                var client = new RestClient("http://opendata.epa.gov.tw");
                var request = new RestRequest("ws/Data/ATM00625/?$format=json", Method.GET);

                var response = await client.ExecuteTaskAsync<List<RootObject>>(request);

                Dictionary<string, RootObject> dictionary = response.Data.ToDictionary(x => x.Site);
                await context.PostAsync($"{dictionary["西屯"].DataCreationDate} - 西屯 PM2.5 = {dictionary["西屯"].PM25} {dictionary["西屯"].ItemUnit}");
            }

            context.Wait(MessageReceivedAsync);
        }

        public class RootObject
        {
            public string Site { get; set; }
            public string county { get; set; }
            public string PM25 { get; set; }
            public string DataCreationDate { get; set; }
            public string ItemUnit { get; set; }
        }  
    }
}