// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using UnityEngine;

/// <summary>
/// https://learn.microsoft.com/en-us/semantic-kernel/prompts/configure-prompts?tabs=Csharp
/// </summary>
public class PromptConfiguration : MonoBehaviour
{
	[SerializeField] Chat chatUI;

	private Kernel kernel;
	KernelFunction chat;
	ChatHistory history;

	private void Awake()
	{
		chatUI.OnMessageSent.AddListener((string message) => UserRequest(message));

		var endpoint = "http://localhost:1234/v1/chat/completions";
		var client = new HttpClient(new LocalLLMs(endpoint));

		var kernelBuilder = Kernel.CreateBuilder()
			//.AddOpenAIChatCompletion(modelId: "_", apiKey: "_", httpClient: new HttpClient(new LMStudio()));
			.AddAzureOpenAIChatCompletion(deploymentName: "local-model", endpoint: endpoint, apiKey: "Is not required", httpClient: client);

		kernelBuilder.Plugins.AddFromType<ConversationSummaryPlugin>();

		kernel = kernelBuilder.Build();

		// Create a template for chatHistory with settings
		chat = kernel.CreateFunctionFromPrompt(
			new PromptTemplateConfig()
			{
				Name = "Chat",
				Description = "Chat with the assistant.",
				Template = @"{{ConversationSummaryPlugin.SummarizeConversation $history}}
                    User: {{$request}}
                    Assistant: ",
				TemplateFormat = "semantic-kernel",
				InputVariables = new List<InputVariable>()
				{
					new() { Name = "history", Description = "The history of the conversation.", IsRequired = false, Default = "" },
					new() { Name = "request", Description = "The user's request.", IsRequired = true }
				},
				ExecutionSettings =
				{
					{
						"default",
						new OpenAIPromptExecutionSettings()
						{
							MaxTokens = 1000,
							Temperature = 0
						}
					}
				}
			}
		);

		ChatHistory history = new ChatHistory();
	}

	public async void UserRequest(string request)
	{
		// Create assistant chatHistory entry
		chatUI.Container.AddMessage(new Message(chatUI.Members[1], string.Empty));
		var message = chatUI.Container.ContainerObject.GetComponentsInChildren<MessagePresenter>().Last().Content;

		// Get chatHistory response
		var response = kernel.InvokeStreamingAsync<StreamingChatMessageContent>(
			chat,
			new()
			{
					{ "request", request },
					{ "history", string.Join("\n", history.Select(x => x.Role + ": " + x.Content)) }
			}
		);

		// Stream the response
		await foreach (var chunk in response)
		{
			message.text += chunk;
			//await Task.Yield();
		}

		// Append to history
		history.AddUserMessage(request!);
		history.AddAssistantMessage(message.text);
	}
}