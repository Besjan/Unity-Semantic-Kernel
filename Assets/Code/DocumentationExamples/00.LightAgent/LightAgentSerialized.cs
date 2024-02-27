// Copyright (c) Microsoft. All rights reserved.

using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using UnityEngine;

/// <summary>
/// https://learn.microsoft.com/en-us/semantic-kernel/overview/
/// </summary>
public class LightAgentSerialized : MonoBehaviour
{
	[SerializeField] Chat chatUI;

	Kernel kernel;
	KernelPlugin prompts;
	IChatCompletionService chatCompletionService;
	ChatHistory chatHistory;

	private void Awake()
	{
		chatUI.OnMessageSent.AddListener((string message) => UserRequest(message));

		//var endpoint = "http://localhost:1234/v1/chat/completions";
		//var client = new HttpClient(new LocalLLMs(endpoint));

		var kernelBuilder = Kernel.CreateBuilder()
			.AddOpenAIChatCompletion(modelId: "_", apiKey: "_", httpClient: new HttpClient(new LMStudio()));
		//.AddAzureOpenAIChatCompletion(deploymentName: "local-model", endpoint: endpoint, apiKey: "Is not required", httpClient: client);

		kernelBuilder.Plugins.AddFromType<LightPlugin>();
		kernel = kernelBuilder.Build();

		// Load prompts
		prompts = kernel.CreatePluginFromPromptDirectory(Path.Combine(Application.streamingAssetsPath, "Prompts"));

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
			//Temperature = 0.0f,
		};

		// Get the response from the AI
		var response = chatCompletionService.GetStreamingChatMessageContentsAsync(
			chatHistory,
			executionSettings: openAIPromptExecutionSettings,
			kernel: kernel);

		// Stream the response
		string fullMessage = "";
		await foreach (var content in response)
		{
			assistantMessage.text += content.Content;
			await Task.Yield();

			fullMessage += content.Content;
		}

		// Add the message from the agent to the chat history
		chatHistory.AddAssistantMessage(fullMessage);
	}
}