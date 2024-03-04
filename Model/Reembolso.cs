namespace Durable.Model
{
    public class Reembolso
    {
        public string Solicitante { get; set; }
        public decimal Valor { get; set; }
        public bool DespesaApontadaNoSistema { get; set; }
    }
}
