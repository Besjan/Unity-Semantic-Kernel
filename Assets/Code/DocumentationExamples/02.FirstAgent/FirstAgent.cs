// Copyright (c) Microsoft. All rights reserved.

using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using UnityEngine;

/// <summary>
/// https://learn.microsoft.com/en-us/semantic-kernel/agents/
/// </summary>
public class FirstAgent : MonoBehaviour
{
	[SerializeField] Chat chatUI;

	Kernel kernel;
	IChatCompletionService chatCompletionService;
	ChatHistory chatMessages;

	private void Awake()
	{
		chatUI.OnMessageSent.AddListener((string message) => UserRequest(message));

		var kernelBuilder = Kernel.CreateBuilder()
			.AddOpenAIChatCompletion(modelId: "_", apiKey: "_", httpClient: new HttpClient(new LMStudio()));
		kernelBuilder.Plugins.AddFromType<AuthorEmailPlanner>();
		kernelBuilder.Plugins.AddFromType<EmailPlugin>();
		kernel = kernelBuilder.Build();

		// Retrieve the chat completion service from the kernel
		chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

		// Create the chat history
		chatMessages = new ChatHistory(@"""
You are a friendly assistant who likes to follow the rules. You will complete required steps
and request approval before taking any consequential actions. If the user doesn't provide
enough information for you to complete a task, you will keep asking questions until you have
enough information to complete the task.
""");
	}

	public async void UserRequest(string userMessage)
	{
		// Create assistant chatHistory entry
		chatUI.Container.AddMessage(new Message(chatUI.Members[1], string.Empty));
		var assistantMessage = chatUI.Container.ContainerObject.GetComponentsInChildren<MessagePresenter>().Last().Content;

		chatMessages.AddUserMessage(userMessage);

		// Get the chat completions
		OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
		{
			ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
		};

		var response = chatCompletionService.GetStreamingChatMessageContentsAsync(
			chatMessages,
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
		chatMessages.AddAssistantMessage(fullMessage);
	}
}