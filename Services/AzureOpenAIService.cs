using System.Text.Json;
using Azure.AI.OpenAI;
using FacturApp.Config;
using FacturApp.DTOs;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace FacturApp.Services;

public record ChatTurn(string Role, string Content);

public enum IntentType { CreateInvoice, AddClient, TextReply }

public record AppIntentResult(
    IntentType Type,
    string TextReply = "",
    InvoiceIntent? Invoice = null,
    ClientIntent? Client = null
);

public record InvoiceIntent(string ClienteNombre, List<InvoiceItemIntent> Items);
public record InvoiceItemIntent(string ProductoNombre, int Cantidad);
public record ClientIntent(string Nombre, string Nif, string Direccion, string Email);

public interface IAzureOpenAIService
{
    Task<AppIntentResult> DetectUserIntentAsync(List<ChatTurn> history, CancellationToken ct = default);
}

public class AzureOpenAIService(ILogger<AzureOpenAIService> logger, AppSettingsService settings) : IAzureOpenAIService
{
    private static readonly ChatTool InvoiceTool = ChatTool.CreateFunctionTool(
        functionName: "crear_factura",
        functionDescription: "Extrae los parámetros para crear una factura a partir de texto en español",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "cliente_nombre": { "type": "string", "description": "Nombre del cliente" },
                "items": {
                    "type": "array",
                    "items": {
                        "type": "object",
                        "properties": {
                            "producto_nombre": { "type": "string" },
                            "cantidad": { "type": "integer" }
                        },
                        "required": ["producto_nombre", "cantidad"]
                    }
                }
            },
            "required": ["cliente_nombre", "items"]
        }
        """)
    );

    private static readonly ChatTool AddClientTool = ChatTool.CreateFunctionTool(
        functionName: "agregar_cliente",
        functionDescription: "Sirve para dar de alta o guardar a un cliente nuevo en nuestro sistema. Usa esta función SOLAMENTE si tienes sus datos completos o si el usuario te pidió explícitamente agregarlo. Pide primero los datos faltantes si no los tienes.",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "nombre": { "type": "string", "description": "Nombre de la empresa o cliente" },
                "nif": { "type": "string", "description": "NIF, CIF o DNI del cliente" },
                "direccion": { "type": "string", "description": "Dirección completa o ciudad" },
                "email": { "type": "string", "description": "Correo de contacto del cliente" }
            },
            "required": ["nombre", "nif", "direccion", "email"]
        }
        """)
    );

    public async Task<AppIntentResult> DetectUserIntentAsync(List<ChatTurn> history, CancellationToken ct = default)
    {
        logger.LogInformation("Analizando historial de conversación con IA...");

        var client = new AzureOpenAIClient(
            new Uri(settings.OpenAIEndpoint),
            new System.ClientModel.ApiKeyCredential(settings.OpenAIKey));
        var chat = client.GetChatClient(settings.OpenAIDeployment);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("Eres 'Asistente IA', un asistente inteligente dentro de FacturApp diseñado para ayudar a crear facturas y gestionar clientes. Tienes herramientas ('crear_factura' y 'agregar_cliente'). Si el usuario quiere guardar un cliente, asegúrate de tener Nombre, NIF, Dirección e Email y llama a 'agregar_cliente'. Si te pides crear una factura, asegúrate extrae los datos e invoca 'crear_factura'. Si te falta algún dato para llamar a tus herramientas o el usuario te hace una pregunta, respóndele normalmente pidiendo más información.")
        };

        foreach (var turn in history)
        {
            if (turn.Role == "User") messages.Add(new UserChatMessage(turn.Content));
            else messages.Add(new AssistantChatMessage(turn.Content));
        }

        var options = new ChatCompletionOptions
        {
            Tools = { InvoiceTool, AddClientTool },
            ToolChoice = ChatToolChoice.CreateAutoChoice()
        };

        var response = await chat.CompleteChatAsync(messages, options, ct);
        var completion = response.Value;

        if (completion.FinishReason == ChatFinishReason.ToolCalls)
        {
            var toolCall = completion.ToolCalls.FirstOrDefault();
            if (toolCall == null) return new AppIntentResult(IntentType.TextReply, "Hubo un error interpretando las herramientas.");

            var json = JsonSerializer.Deserialize<JsonElement>(toolCall.FunctionArguments.ToString());

            if (toolCall.FunctionName == "crear_factura")
            {
                var clienteNombre = json.GetProperty("cliente_nombre").GetString() ?? string.Empty;
                var items = new List<InvoiceItemIntent>();

                if (json.TryGetProperty("items", out var itemsEl))
                {
                    foreach (var item in itemsEl.EnumerateArray())
                    {
                        items.Add(new InvoiceItemIntent(
                            item.GetProperty("producto_nombre").GetString() ?? string.Empty,
                            item.GetProperty("cantidad").GetInt32()
                        ));
                    }
                }

                return new AppIntentResult(IntentType.CreateInvoice, Invoice: new InvoiceIntent(clienteNombre, items));
            }

            if (toolCall.FunctionName == "agregar_cliente")
            {
                return new AppIntentResult(IntentType.AddClient, Client: new ClientIntent(
                    json.GetProperty("nombre").GetString() ?? string.Empty,
                    json.GetProperty("nif").GetString() ?? string.Empty,
                    json.GetProperty("direccion").GetString() ?? string.Empty,
                    json.GetProperty("email").GetString() ?? string.Empty
                ));
            }
        }

        // Si no hizo tool calls, es texto normal
        return new AppIntentResult(IntentType.TextReply, TextReply: completion.Content[0].Text);
    }
}
