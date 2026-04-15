namespace IA_Financas;

using Microsoft.SemanticKernel;
using System.ComponentModel;

public class FinancasPessoalPlugin
{
    [KernelFunction]
    [Description("Calcula rendimento de investimentos com juros compostos")]
    public async Task<object> CalcularInvestimentoAsync(
        [Description("Valor inicial do investimento")] decimal valor,
        [Description("Prazo do investimento em meses")] int meses,
        [Description("Taxa de juros anual em percentual")] double jurosAnuais)
    {
        await Task.Delay(100); // Simula processamento

        var taxaMensal = Math.Pow(1 + jurosAnuais / 100, 1.0 / 12) - 1;
        var montante = valor * (decimal)Math.Pow(1 + taxaMensal, meses);
        var rendimento = montante - valor;
        var rendimentoPercentual = (rendimento / valor) * 100;

        var projecaoMensal = new List<object>();
        var valorAtual = valor;

        for (int i = 1; i <= meses; i++)
        {
            valorAtual *= (decimal)(1 + taxaMensal);
            if (i % 6 == 0 || i == meses) // Mostra a cada 6 meses e no final
            {
                projecaoMensal.Add(new
                {
                    Mes = i,
                    Valor = Math.Round(valorAtual, 2),
                    Rendimento = Math.Round(valorAtual - valor, 2)
                });
            }
        }

        return new
        {
            InvestimentoInicial = Math.Round(valor, 2),
            PrazoMeses = meses,
            TaxaJurosAnual = jurosAnuais,
            TaxaJurosMensal = Math.Round(taxaMensal * 100, 4),
            MontanteFinal = Math.Round(montante, 2),
            RendimentoTotal = Math.Round(rendimento, 2),
            RendimentoPercentual = Math.Round((double)rendimentoPercentual, 2),
            Projecao = projecaoMensal,
            Mensagem = GetMensagemInvestimento(rendimento, meses)
        };
    }

    [KernelFunction]
    [Description("Analisa a saúde financeira pessoal e faz recomendações")]
    public async Task<object> AnalisarSaudeFinanceiraAsync(
    [Description("Renda mensal líquida")] decimal renda,
    [Description("Despesas mensais totais")] decimal despesas,
    [Description("Objetivo financeiro (ex: valor para comprar algo)")] decimal objetivo = 0)
    {
        await Task.Delay(150); // Simula análise complexa

        var economiaMensal = renda - despesas;
        var taxaEconomia = renda > 0 ? (economiaMensal / renda) * 100 : 0;
        var mesesParaObjetivo = economiaMensal > 0 && objetivo > 0 ? objetivo / economiaMensal : -1;

        // CORREÇÃO AQUI: Converter decimal para double
        var (situacao, recomendacao, icone) = AnalisarSituacao(economiaMensal, (double)taxaEconomia);

        var metricas = new
        {
            RendaMensal = renda,
            DespesasMensais = despesas,
            EconomiaMensal = economiaMensal,
            TaxaEconomia = Math.Round(taxaEconomia, 1),
            ObjetivoFinanceiro = objetivo,
            MesesParaObjetivo = mesesParaObjetivo > 0 ? Math.Ceiling(mesesParaObjetivo) : 0,
            Situacao = situacao,
            Icone = icone,
            Recomendacao = recomendacao,
            Alertas = GerarAlertas(renda, despesas, economiaMensal)
        };

        return metricas;
    }

    [KernelFunction]
    [Description("Simula diferentes cenários de investimento")]
    public async Task<object> SimularCenariosInvestimentoAsync(
        [Description("Valor inicial para investir")] decimal valorInicial,
        [Description("Valor mensal adicional")] decimal aporteMensal = 0,
        [Description("Prazo total em anos")] int anos = 10)
    {
        await Task.Delay(200);

        var cenarios = new[]
        {
            new { Nome = "Conservador", Juros = 6.0, Risco = "Baixo" },
            new { Nome = "Moderado", Juros = 10.0, Risco = "Médio" },
            new { Nome = "Agressivo", Juros = 15.0, Risco = "Alto" }
        };

        var resultados = new List<object>();

        foreach (var cenario in cenarios)
        {
            var meses = anos * 12;
            var taxaMensal = Math.Pow(1 + cenario.Juros / 100, 1.0 / 12) - 1;
            var montante = valorInicial * (decimal)Math.Pow(1 + taxaMensal, meses);

            // Adiciona aportes mensais
            for (int i = 0; i < meses; i++)
            {
                montante += aporteMensal * (decimal)Math.Pow(1 + taxaMensal, meses - i - 1);
            }

            resultados.Add(new
            {
                Cenario = cenario.Nome,
                TaxaJurosAnual = cenario.Juros,
                NivelRisco = cenario.Risco,
                MontanteFinal = Math.Round(montante, 2),
                TotalInvestido = Math.Round(valorInicial + (aporteMensal * meses), 2),
                Rendimento = Math.Round(montante - (valorInicial + (aporteMensal * meses)), 2)
            });
        }

        return new
        {
            ValorInicial = valorInicial,
            AporteMensal = aporteMensal,
            PrazoAnos = anos,
            Cenarios = resultados
        };
    }

    private (string Situacao, string Recomendacao, string Icone) AnalisarSituacao(decimal economia, double taxaEconomia)
    {
        if (economia <= 0)
            return ("Crítica", "⚠️ Suas despesas são maiores que sua renda. Recomendo revisar urgentemente seus gastos e cortar despesas não essenciais.", "🔴");

        if (taxaEconomia < 10)
            return ("Atenção", "💡 Você está economizando pouco. Tente aumentar sua renda ou reduzir despesas para alcançar pelo menos 20% de economia.", "🟡");

        if (taxaEconomia < 20)
            return ("Boa", "📈 Você está no caminho certo! Continue com sua disciplina financeira e considere investir o excedente.", "🟢");

        return ("Excelente", "🎉 Excelente! Sua saúde financeira é muito boa. Considere diversificar seus investimentos para maximizar retornos.", "💎");
    }

    private string[] GerarAlertas(decimal renda, decimal despesas, decimal economia)
    {
        var alertas = new List<string>();

        if (despesas > renda * 0.5m)
            alertas.Add("Mais de 50% da renda comprometida com despesas");

        if (economia < renda * 0.1m)
            alertas.Add("Economia abaixo do recomendado (10% da renda)");

        if (despesas > renda * 0.7m)
            alertas.Add("Alto comprometimento de renda - risco financeiro");

        return alertas.ToArray();
    }

    private string GetMensagemInvestimento(decimal rendimento, int meses)
    {
        if (rendimento <= 0)
            return "💰 Investimento preservou o capital";

        return rendimento switch
        {
            < 100 => $"💸 Pequeno rendimento de R$ {rendimento:F2} em {meses} meses",
            < 1000 => $"📊 Rendimento sólido de R$ {rendimento:F2} em {meses} meses",
            _ => $"🎯 Excelente rendimento de R$ {rendimento:F2} em {meses} meses!"
        };
    }
}