using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InsuranceBot.Dialogs
{
    [Serializable]
    public class IntroDialog : IDialog<string>
    {
        public async Task StartAsync(IDialogContext context)
        {
            //IEnumerable<string> options = new List<string>() { "Schade melden", "Informatie over mijn verzekeringen", "Overstappen op andere verzekeraar" };
            IEnumerable<string> options = new List<string>() { "Report damage", "Information about my insurance", "Switch to another insurer" };
            //PromptDialog.Choice(context, OnOptionSelected, options, "Hi, waarmee kan ik je helpen?", promptStyle: PromptStyle.Keyboard);
            PromptDialog.Choice(context, OnOptionSelected, options, "Hi, how can I help you?", promptStyle: PromptStyle.Keyboard);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            context.Done(await result);
        }
    }
}