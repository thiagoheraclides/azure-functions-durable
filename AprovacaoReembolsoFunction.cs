using Durable.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Durable
{
    public static class AprovacaoReembolsoFunction
    {
        [FunctionName("ObterAprovacaoReembolso")]
        public static async Task<ResultadoAprovacaoDeReembolso> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {           
           
            var reembolso = context.GetInput<Reembolso>();
            
            var setorResponsavel = await context.CallActivityAsync<string>(nameof(ObterAprovador), reembolso);
            var resposta = string.Empty;

            if (setorResponsavel == "Diretoria")
            {
                resposta = await context.CallActivityAsync<string>(nameof(ObterAprovacaoDeReembolsoDiretoria), reembolso);
            }
            else
            {
                resposta = await context.CallActivityAsync<string>(nameof(ObterAprovacaoDeReembolsoFinanceiro), reembolso);
            }

            return new ResultadoAprovacaoDeReembolso
            {
                Resultado = resposta,
                Solicitante = reembolso.Solicitante,
                Valor = reembolso.Valor,
                AnalisadoPor = setorResponsavel,
                AnalisadoEm = DateTime.Now
            };

        }

        [FunctionName(nameof(ObterAprovador))]
        public static string ObterAprovador([ActivityTrigger] Reembolso reembolso, ILogger log)
        {
            log.LogInformation("Obtendo setor aprovador...");

            if (reembolso.Valor > 2500m)
                return "Diretoria";

            return "Financeiro";
        }

        [FunctionName(nameof(ObterAprovacaoDeReembolsoDiretoria))]
        public static string ObterAprovacaoDeReembolsoDiretoria([ActivityTrigger] Reembolso reembolso, ILogger log)
        {
            log.LogInformation("Obtendo aprovação da diretoria!");

            if (reembolso.Valor > 2500m & reembolso.Valor < 5001m)
            {
                if (reembolso.DespesaApontadaNoSistema)
                    return "Aprovado!";

                return "Rejeitado!";
            }
            else
            {
                return "Rejeitado!";
            }

        }

        [FunctionName(nameof(ObterAprovacaoDeReembolsoFinanceiro))]
        public static string ObterAprovacaoDeReembolsoFinanceiro([ActivityTrigger] Reembolso reembolso, ILogger log)
        {
            log.LogInformation("Obtendo aprovação ddo financeiro!");

            if (reembolso.Valor > 50m)
            {
                if (reembolso.DespesaApontadaNoSistema)
                    return "Aprovado";

                return "Rejeitado";
            }
            else
            {
                return "Aprovado";
            }
        }

        [FunctionName("SolicitarReembolso")]
        public static async Task<HttpResponseMessage> Start(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var data = await req.Content.ReadAsAsync<Reembolso>();           
            string instanceId = await starter.StartNewAsync<Reembolso>("ObterAprovacaoReembolso", data);
            log.LogInformation("Orquestração iniciada com ID = '{instanceId}'.", instanceId);            
            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}