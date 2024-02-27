// Copyright (c) Microsoft. All rights reserved.

using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Codeblaze.SemanticKernel.Connectors.Ollama;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using UnityEngine;

/// <summary>
/// https://learn.microsoft.com/en-us/semantic-kernel/overview/
/// </summary>
public class LightAgentOllama : MonoBehaviour
{
	[SerializeField] Chat chatUI;

	Kernel kernel;
	IChatCompletionService chatCompletionService;
	ChatHistory chatHistory;

	private void Awake()
	{
		chatUI.OnMessageSent.AddListener((string message) => UserRequest(message));

		var kernelBuilder = Kernel.CreateBuilder();

		// provide the HTTP client used to interact with Ollama API
		kernelBuilder.Services.AddTransient<HttpClient>();

		kernelBuilder.AddOllamaChatCompletion(
			"llama2", // Ollama model Id
			"http://localhost:11434" // Ollama endpoint,
		);

		kernelBuilder.Plugins.AddFromType<LightPlugin>();
		kernel = kernelBuilder.Build();

		// Retrieve the chat completion service from the kernel
		chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

		// Create the chat history
		chatHistory = new ChatHistory();
	}

	public async void UserRequest(string userMessage)
	{
		// Create assistant chatHistory entry
		chatUI.Container.AddMessage(new Message(chatUI.Members[1], string.Empty));
		var assistantMessage = chatUI.Container.ContainerObject.GetComponentsInChildren<MessagePresenter>().Last().Content;

		chatHistory.AddUserMessage($"{userMessage}");

		// Enable auto function calling
		OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
		{
			ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
			Temperature = 0.0f,
		};

		//// Get the response from the AI
		//var response = chatCompletionService.GetStreamingChatMessageContentsAsync(
		//	chatHistory,
		//	executionSettings: openAIPromptExecutionSettings,
		//	kernel: kernel);

		//// Stream the response
		//string fullMessage = "";
		//await foreach (var content in response)
		//{
		//	assistantMessage.text += content.Content;
		//	await Task.Yield();

		//	fullMessage += content.Content;
		//}

		var result = await chatCompletionService.GetChatMessageContentsAsync(
			chatHistory: chatHistory,
			executionSettings: openAIPromptExecutionSettings);

		string fullMessage = result[^1].Content;
		assistantMessage.text = fullMessage;

		// Add the message from the agent to the chat history
		chatHistory.AddAssistantMessage(fullMessage);
	}
}