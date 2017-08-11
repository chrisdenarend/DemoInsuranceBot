using Microsoft.Azure.Documents.Client;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Linq;
using System.Threading.Tasks;
using InsuranceBot.Helpers;
using Microsoft.Azure;

namespace InsuranceBot.Dialogs
{
    [Serializable]
    public class PriceDialog : IDialog<string>
    {
        private string _objectName;

        private string EndpointUrl = CloudConfigurationManager.GetSetting("CosmosDBEndpointUrl");
        private string PrimaryKey = CloudConfigurationManager.GetSetting("CosmosDBPrimaryKey");

        public PriceDialog(string objectName)
        {
            _objectName = objectName;
        }

        public async Task StartAsync(IDialogContext context)
        {
            DocumentClient client;
            try
            {
                var collectionLink = UriFactory.CreateDocumentCollectionUri("Objects", "Damage");
                client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);

                var queryObject = client.CreateDocumentQuery<PriceObject>(collectionLink)
                                                .Where(so => so.name == _objectName)
                                                .AsEnumerable()
                                                .FirstOrDefault();


                //await context.PostAsync($"U heeft schade aan een {_objectName} van ongeveer {queryObject.price} euro.");
                await context.PostAsync($"The {_objectName} is damaged and the value is about {queryObject.price} euro.");

                //PromptDialog.Confirm(context, OnConfirmationSelected, "Klopt deze informatie?");
                PromptDialog.Confirm(context, OnConfirmationSelected, "Is this information correct?");
            }
            catch (Exception ex)
            {
            }
        }

        private async Task OnConfirmationSelected(IDialogContext context, IAwaitable<bool> result)
        {
            context.Done((await result).ToString());
        }
    }
}