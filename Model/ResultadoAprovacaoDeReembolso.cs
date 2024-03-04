using System;

namespace Durable.Model
{
    public class ResultadoAprovacaoDeReembolso
    {
        public string Resultado { get; set; }
        public string Solicitante { get; set; }
        public decimal Valor { get; set; }
        public string AnalisadoPor { get; set; }
        public DateTime AnalisadoEm { get; set; } = DateTime.Now;
    }
}
