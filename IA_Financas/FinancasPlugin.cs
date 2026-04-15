using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace IA_Financas;

public class FinancasPlugin
{
    private readonly Random _random = new();

    [KernelFunction]
    [Description("Obtém o preço atual e variação de uma ação da B3")]
    public async Task<object> GetPrecosAcoesAsync(
        [Description("Símbolo da ação (ex: PETR4, VALE3, ITUB4)")] string acao)
    {
        // Simulação de dados de mercado em tempo real
        var basePrices = new Dictionary<string, decimal>
        {
            ["PETR4"] = 35.67m,
            ["VALE3"] = 72.41m,
            ["ITUB4"] = 34.15m,
            ["BBDC4"] = 25.30m,
            ["WEGE3"] = 36.80m,
            ["MGLU3"] = 2.15m,
            ["BBAS3"] = 56.90m
        };

        var variacao = (decimal)(_random.NextDouble() * 4 - 2); // -2% a +2%

        await Task.Delay(100); // Simula latência de API

        if (basePrices.ContainsKey(acao.ToUpper()))
        {
            var precoBase = basePrices[acao.ToUpper()];
            var precoAtual = precoBase * (1 + variacao / 100);
            var tendencia = variacao > 0 ? "alta" : variacao < 0 ? "baixa" : "estável";

            return new
            {
                Acao = acao.ToUpper(),
                Preco = Math.Round(precoAtual, 2),
                Variacao = variacao > 0 ? $"+{Math.Round(variacao, 2)}%" : $"{Math.Round(variacao, 2)}%",
                Tendencia = tendencia,
                Atualizado = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                Recomendacao = GetRecomendacaoAcao(tendencia)
            };
        }
        else
        {
            return new
            {
                Erro = $"Ação {acao} não encontrada",
                AcoesDisponiveis = basePrices.Keys.ToArray()
            };
        }
    }

    [KernelFunction]
    [Description("Obtém a cotação atual de moedas e criptomoedas")]
    public async Task<object> GetTaxaCambioAsync(
        [Description("Código da moeda (ex: USD, EUR, BTC, GBP)")] string moeda)
    {
        var cotacoes = new Dictionary<string, object>
        {
            ["USD"] = new
            {
                Valor = 5.45m,
                Tendencia = "estável",
                Nome = "Dólar Americano",
                Variacao24h = "+0.3%"
            },
            ["EUR"] = new
            {
                Valor = 6.12m,
                Tendencia = "alta",
                Nome = "Euro",
                Variacao24h = "+0.8%"
            },
            ["BTC"] = new
            {
                Valor = 185000.00m,
                Tendencia = "volátil",
                Nome = "Bitcoin",
                Variacao24h = "-2.1%"
            },
            ["GBP"] = new
            {
                Valor = 7.20m,
                Tendencia = "estável",
                Nome = "Libra Esterlina",
                Variacao24h = "+0.2%"
            },
            ["ETH"] = new
            {
                Valor = 10500.00m,
                Tendencia = "volátil",
                Nome = "Ethereum",
                Variacao24h = "-1.5%"
            }
        };

        await Task.Delay(150); // Simula latência de API

        var moedaUpper = moeda.ToUpper();
        if (cotacoes.ContainsKey(moedaUpper))
        {
            return cotacoes[moedaUpper];
        }
        else
        {
            return new
            {
                Erro = $"Cotação para {moeda} não disponível",
                MoedasDisponiveis = cotacoes.Keys.ToArray()
            };
        }
    }

    [KernelFunction]
    [Description("Analisa o desempenho histórico de uma ação")]
    public async Task<object> AnalisarAcaoAsync(
        [Description("Símbolo da ação")] string acao,
        [Description("Período em dias para análise")] int periodoDias = 30)
    {
        await Task.Delay(200); // Simula processamento

        var analises = new Dictionary<string, object>
        {
            ["PETR4"] = new
            {
                Volatilidade = "Alta",
                Recomendacao = "COMPRAR",
                AlvoPreco = 38.50m,
                StopLoss = 33.20m,
                Fundamentos = new
                {
                    DividendYield = "6.8%",
                    P_L = 8.2m,
                    ROE = "15.3%"
                }
            },
            ["VALE3"] = new
            {
                Volatilidade = "Média",
                Recomendacao = "MANTER",
                AlvoPreco = 75.00m,
                StopLoss = 68.50m,
                Fundamentos = new
                {
                    DividendYield = "8.2%",
                    P_L = 6.5m,
                    ROE = "22.1%"
                }
            }
        };

        if (analises.ContainsKey(acao.ToUpper()))
        {
            return analises[acao.ToUpper()];
        }
        else
        {
            return new
            {
                Erro = $"Análise não disponível para {acao}",
                AcoesComAnalise = analises.Keys.ToArray()
            };
        }
    }

    private string GetRecomendacaoAcao(string tendencia)
    {
        return tendencia switch
        {
            "alta" => "📈 Momento favorável para compra",
            "baixa" => "📉 Cautela recomendada",
            "estável" => "⚡ Estabilidade de preços",
            _ => "🔍 Analisar fundamentos"
        };
    }
}
