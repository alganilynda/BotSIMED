﻿using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace TrevorBot.Dialogs
{
    [Serializable]
    public class InscriptionDialog : IDialog<string> //dialogue d'inscrption
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Merci de bien vouloir t'inscrire !");

            var inscriptionFormDialog = FormDialog.FromForm(this.BuildInscriptionForm, FormOptions.PromptInStart); //initialise un form flow (le truc qui va appeler tes questions)

            context.Call(inscriptionFormDialog, this.ResumeAfterInscriptionFormDialog);
        }

        private IForm<InscriptionQuery> BuildInscriptionForm() //IForm de type inscription query
        {

            OnCompletionAsyncDelegate<InscriptionQuery> processRegister = async (context, state) =>
            {
              
                await context.PostAsync($"Ok. Voici l'adresse mail que tu as entré : {state.Mail}, ainsi que le pseudonyme : {state.Pseudo} est-ce exact? ");
                context.Wait(this.MessageReceivedAsync);
                context.Call(new SESForm(), this.ResumeAfterValidationInscrptionDialog);

            };

            return new FormBuilder<InscriptionQuery>() // là où on peut customiser le FormBuiler avec des Field, des onCompletion ou validate/confirm

                .AddRemainingFields()        
               // .OnCompletion(processRegister)
                .Confirm("Ok. Voici l'adresse mail que tu as entré : {Mail}, ainsi que le pseudonyme : {Pseudo} est-ce exact? ")
                .Build();
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            Console.WriteLine("Dans la fonction");
            var message = await result;
           
            context.Call(new SESForm(), this.ResumeAfterValidationInscrptionDialog);

            if (message.Text != null)
            {
                  
                await context.PostAsync("Retour au FirstDialog");

              //  context.Call(new SESForm(), this.ResumeAfterInscriptionFormDialog);
                context.Done(message.Text);
            }
        }

        private async Task ResumeAfterInscriptionFormDialog(IDialogContext context, IAwaitable<InscriptionQuery> result)
        {
            var message = await result;
            await context.PostAsync(" Merci de t'être inscrit ");
            context.Call(new SESForm(), ResumeAfterValidationInscrptionDialog);

        }

        private async Task ResumeAfterValidationInscrptionDialog(IDialogContext context, IAwaitable<string> result) // obligatoire d'avoir une ResumeAfter... (sinon il sait pas quoi faire à la fin du dialogue)
        {
            var message = await result;
            context.Wait(this.MessageReceivedAsync);
            await context.PostAsync(" Merci de t'être inscrit Validation d'inscription");
            context.Call(new SESForm(), ResumeAfterValidationInscrptionDialog);
        }
    }
}