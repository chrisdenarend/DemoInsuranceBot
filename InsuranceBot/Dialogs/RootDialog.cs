using InsuranceBot.Helpers;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InsuranceBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private IEnumerable<string> _tagList;
        private string _imageObject;

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            context.Call(new IntroDialog(), IntroDialogResumeAfter);
        }

        private async Task IntroDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                switch (result.GetAwaiter().GetResult())
                {
                    //case "Schade melden":
                    case "Report damage":
                        //await context.PostAsync("Wat vervelend voor u, is er iets defect geraakt?");
                        await context.PostAsync("Oh that's a pity, did you have some damage?");
                        context.Call(new ObjectDialog(), ObjectDialogResumeAfter);
                        break;
                    //case "Informatie over mijn verzekeringen":
                    case "Information about my insurance":
                        //await context.PostAsync("Deze functie ondersteunen we helaas nog niet, maak een andere keus.");
                        await context.PostAsync("This function is not supported yet, please select another one.");
                        context.Call(new IntroDialog(), IntroDialogResumeAfter);
                        break;
                    //case "Overstappen op andere verzekeraar":
                    case "Switch to another insurer":
                        //await context.PostAsync("Deze functie ondersteunen we helaas nog niet, maak een andere keus.");
                        await context.PostAsync("This function is not supported yet, please select another one.");
                        context.Call(new IntroDialog(), IntroDialogResumeAfter);
                        break;
                    default:
                        context.Call(new IntroDialog(), IntroDialogResumeAfter);
                        break;
                }
            }
            catch (TooManyAttemptsException)
            {
                //await context.PostAsync("Ik begrijp je helaas niet. Laten we het opnieuw proberen.");
                await context.PostAsync("I don't understand, let's try again.");
                await StartAsync(context);
            }
        }

        private async Task ObjectDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                _tagList = JsonConvert.DeserializeObject<IEnumerable<string>>(result.GetAwaiter().GetResult());

                //Translate tags
                //var translator = new Translator();
                //var translatedTags = translator.Translate(_tagList);

                //PromptDialog.Choice(context, PriceDialogResumeAfter, translatedTags, "Welk object uit de foto bedoelt u?", "Sorry dat begrijp ik niet, probeer het opnieuw.", 3);
                PromptDialog.Choice(context, PriceDialogResumeAfter, _tagList, "Which object from the image did you mean?", "I don't understand, let's try again.", 3);
            }
            catch (TooManyAttemptsException)
            {
                //await context.PostAsync("Ik begrijp je helaas niet. Laten we het opnieuw proberen.");
                await context.PostAsync("I don't understand, let's try again.");
                await StartAsync(context);
            }
        }

        private async Task PriceDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                _imageObject = await result;

                //await context.PostAsync($"Geen probleem, we zorgen voor uw { _imageObject }.");
                await context.PostAsync($"Don't worry, we'll take care of your damaged { _imageObject }.");
                context.Call(new PriceDialog(_imageObject), PriceDialogResultResumeAfter);

            }
            catch (TooManyAttemptsException)
            {
                //await context.PostAsync("Ik begrijp je helaas niet. Laten we het opnieuw proberen.");
                await context.PostAsync("I don't understand, let's try again.");
                await StartAsync(context);
            }
        }

        private async Task PriceDialogResultResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                //await context.PostAsync("we zijn klaar");
                await context.PostAsync("Damage is reported, compensation is on its way.");
            }
            catch (TooManyAttemptsException)
            {
                //await context.PostAsync("Ik begrijp je helaas niet. Laten we het opnieuw proberen.");
                await context.PostAsync("I don't understand, let's try again.");
                await StartAsync(context);
            }
        }
    }
}