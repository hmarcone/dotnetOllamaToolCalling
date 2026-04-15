using IA_Financas;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;

var builder = Kernel.CreateBuilder();

// Configura o conector OpenAI apontando para o Ollama local 
builder.AddOpenAIChatCompletion(
    modelId: "llama3.1",  // Modelo Ollama
    apiKey: "not-needed-for-local",  // Ignorado para local
    endpoint: new Uri("http://localhost:11434/v1"));  // Endpoint do Ollama

Kernel kernel = builder.Build();

// Adicionar plugins
kernel.Plugins.AddFromType<FinancasPlugin>("mercado");
kernel.Plugins.AddFromType<FinancasPessoalPlugin>("pessoal");

// Interface do usuário
Console.Clear();
Console.WriteLine("================================================");
Console.WriteLine("🤖 ASSISTENTE FINANCEIRO IA - TOOL CALLING");
Console.WriteLine("================================================");
Console.WriteLine("Modelo: llama3.1:latest (Ollama Local)");
Console.WriteLine("Digite 'sair' para encerrar ou 'ajuda' para ver exemplos");
Console.WriteLine("Digite 'ferramentas' para ver os recursos usados");
Console.WriteLine("Digite 'limpar' para limpar o histórico da conversa");
Console.WriteLine("================================================\n");

// Histórico da conversa
var chatHistory = new ChatHistory();

// Prompt do sistema
chatHistory.AddSystemMessage("""
Você é um assistente financeiro inteligente e prestativo.
Use as ferramentas disponíveis para fornecer informações precisas sobre investimentos, 
cotações e análises financeiras.

SEJA DIRETO E CLARO NAS RESPOSTAS. Use os dados das ferramentas para enriquecer suas respostas.

Ferramentas disponíveis:
- mercado.GetPrecosAcoesAsync: Consulta preços de ações
- mercado.GetTaxaCambioAsync: Consulta cotações de moedas  
- pessoal.CalcularInvestimento: Calcula rendimentos
- pessoal.AnalisarSaudeFinanceira: Analisa finanças pessoais

Sempre que possível, use as ferramentas para obter dados atualizados!
""");

while (true)
{
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.Write("💬 Você: ");
    Console.ResetColor();

    var pergunta = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(pergunta))
        continue;

    if (pergunta.ToLower() == "sair")
        break;

    if (pergunta.ToLower() == "ajuda")
    {
        MostrarAjuda();
        continue;
    }

    if (pergunta.ToLower() == "ferramentas")
    {
        MostrarFerramentas(kernel);
        continue;
    }

    if (pergunta.ToLower() == "limpar")
    {
        chatHistory.Clear();
        chatHistory.AddSystemMessage("Histórico limpo. Continue como assistente financeiro.");
        Console.WriteLine("Histórico da conversa foi limpo.\n");
        continue;
    }

    try
    {
        // Adicionar pergunta ao histórico
        chatHistory.AddUserMessage(pergunta);

        // Mostrar indicador de pensando...
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("🤔 Pensando");
        for (int i = 0; i < 5; i++)
        {
            Console.Write(".");
            await Task.Delay(300);
        }
        Console.WriteLine();
        Console.ResetColor();

        // Obter resposta do assistente
        // Pega o 'serviço de chat' que configuramos (no caso, Ollama)
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        // Enviamos todo o histórico para o modelo
        // O modelo analisa e decide sozinho se precisa usar alguma ferramenta
        // Se decidir usar, ele 'chama' a ferramenta, nós executamos e retornamos o resultado
        var response = await chatService.GetChatMessageContentAsync(chatHistory);

        // O modelo então formata a resposta final com os dados obtidos
        // Adicionar resposta ao histórico
        chatHistory.AddAssistantMessage(response.Content!);

        // Mostrar resposta formatada
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"🤖 Assistente: {response.Content}");
        Console.ResetColor();
        Console.WriteLine();

        // Mostrar dica sobre tool calling
        MostrarDicaToolCalling(kernel, pergunta);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Erro: {ex.Message}");
        Console.ResetColor();
        Console.WriteLine();
    }
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("👋 Até logo!");
Console.ResetColor();

// Métodos auxiliares
static void MostrarAjuda()
{
    Console.WriteLine("\n📚 EXEMPLOS DE PERGUNTAS:");
    Console.WriteLine("==================================================");

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("💰 AÇÕES:");
    Console.ResetColor();
    Console.WriteLine("  • \"Qual o preço da PETR4?\"");
    Console.WriteLine("  • \"Me mostre a cotação da VALE3\"");
    Console.WriteLine("  → Ferramenta: mercado.GetStockPrice");

    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine("\n💱 MOEDAS:");
    Console.ResetColor();
    Console.WriteLine("  • \"Qual a cotação do dólar?\"");
    Console.WriteLine("  • \"Quanto vale 1 euro em reais?\"");
    Console.WriteLine("  → Ferramenta: mercado.GetExchangeRate");

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\n📈 INVESTIMENTOS:");
    Console.ResetColor();
    Console.WriteLine("  • \"Quanto renderia R$ 1000 em 12 meses a 10% ao ano?\"");
    Console.WriteLine("  • \"Calcule o rendimento de R$ 5000 em 24 meses a 8%\"");
    Console.WriteLine("  → Ferramenta: pessoal.CalcularInvestimento");

    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine("\n🏦 ANÁLISE FINANCEIRA:");
    Console.ResetColor();
    Console.WriteLine("  • \"Analise minha saúde financeira com renda R$ 5000 e despesas R$ 3500\"");
    Console.WriteLine("  • \"Tenho renda de R$ 3000 e gastos de R$ 2800, estou bem?\"");
    Console.WriteLine("  → Ferramenta: pessoal.AnalisarSaudeFinanceira");

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\n🔗 PERGUNTAS COMPLEXAS (Múltiplas ferramentas):");
    Console.ResetColor();
    Console.WriteLine("  • \"Me dê o preço do dólar e calcule quanto custaria importar um produto de US$ 1000\"");
    Console.WriteLine("  • \"Qual o preço da PETR4 e quanto eu precisaria investir para ter R$ 10.000 em 2 anos?\"");

    Console.WriteLine("\n==================================================\n");
}

static void MostrarFerramentas(Kernel kernel)
{
    Console.WriteLine("\n🛠️ FERRAMENTAS DISPONÍVEIS (TOOL CALLING):");
    Console.WriteLine("==================================================");

    foreach (var plugin in kernel.Plugins)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n📦 PLUGIN: {plugin.Name}");
        Console.ResetColor();

        foreach (var function in plugin)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"  🔧 {function.Name}");
            Console.ResetColor();
            Console.WriteLine($"     Descrição: {function.Description ?? "Sem descrição"}");

            if (function.Metadata.Parameters.Any())
            {
                Console.WriteLine("     Parâmetros:");
                foreach (var param in function.Metadata.Parameters)
                {
                    Console.WriteLine($"       - {param.Name}: {param.ParameterType?.Name ?? "string"}");
                    if (!string.IsNullOrEmpty(param.Description))
                        Console.WriteLine($"         {param.Description}");
                }
            }
            Console.WriteLine();
        }
    }

    Console.WriteLine("==================================================\n");
}

static void MostrarDicaToolCalling(Kernel kernel, string pergunta)
{
    // Detecta palavras-chave que geralmente acionam tool calling
    var palavrasChave = new[]
    {
        "preço", "cotação", "valor", "calcule", "calcular", "quanto",
        "analise", "analisar", "rendimento", "investimento", "ação",
        "dólar", "euro", "bitcoin", "saúde financeira", "despesas", "renda"
    };

    if (palavrasChave.Any(palavra => pergunta.ToLower().Contains(palavra)))
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("💡 Dica: Esta pergunta provavelmente usou Tool Calling!");
        Console.WriteLine("   O LLM identificou a necessidade de dados externos e chamou as ferramentas automaticamente.");
        Console.ResetColor();
        Console.WriteLine();
    }
}