using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.ProjectOxford.Vision;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace InsuranceBot.Dialogs
{
    [Serializable]
    public class ObjectDialog : IDialog<string>
    {
        private int attempts = 3;
        public async Task StartAsync(IDialogContext context)
        {
            //await context.PostAsync("Kun je een foto opsturen van je schade?");
            await context.PostAsync("Can you send me an image of the damaged object?");
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result as Activity;

            var tagList = new List<string>();
            var imageAttachment = activity.Attachments?.FirstOrDefault(a => a.ContentType.Contains("image"));
            if (imageAttachment != null)
            {
                var client = new ConnectorClient(new Uri(activity.ServiceUrl));
                var visionServiceClient = new VisionServiceClient(ConfigurationManager.AppSettings["VisionApi"], "https://westeurope.api.cognitive.microsoft.com/vision/v1.0");
                var visualFeatures = new VisualFeature[] { VisualFeature.Tags };
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        var url = httpClient.GetStreamAsync(new Uri(imageAttachment.ContentUrl)).Result;
                        var tags = await visionServiceClient.AnalyzeImageAsync(url, visualFeatures);
                        tagList.AddRange(tags.Tags.Select(t => t.Name));
                        Console.WriteLine($"tags = " + tags);
                    }
                }
                catch (Exception ex)
                {
                }

                var serializedTagList = JsonConvert.SerializeObject(tagList);

                context.Done(serializedTagList);
            }
            /* Else, try again by re-prompting the user. */
            else
            {
                --attempts;
                if (attempts > 0)
                {
                    await context.PostAsync("Ik begrijp je helaas niet. Laten we het opnieuw proberen.");

                    context.Wait(MessageReceivedAsync);
                }
                else
                {
                    /* Fails the current dialog, removes it from the dialog stack, and returns the exception to the 
                        parent/calling dialog. */
                    context.Fail(new TooManyAttemptsException("Message was not a valid image."));
                }
            }
        }
    }
}