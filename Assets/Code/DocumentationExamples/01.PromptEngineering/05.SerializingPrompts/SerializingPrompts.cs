// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using UnityEngine;

/// <summary>
/// https://learn.microsoft.com/en-us/semantic-kernel/prompts/saving-prompts-as-files?tabs=Csharp
/// </summary>
public class SerializingPrompts : MonoBehaviour
{
	[SerializeField] Chat chatUI;

	Kernel kernel;
	KernelPlugin prompts;
	ChatHistory history;

	List<string> choices;
	List<ChatHistory> fewShotExamples;
	KernelFunction getIntent;

	private void Awake()
	{
		chatUI.OnMessageSent.AddListener((string message) => UserRequest(message));

		var kernelBuilder = Kernel.CreateBuilder()
			.AddOpenAIChatCompletion(modelId: "_", apiKey: "_", httpClient: new HttpClient(new LMStudio()));

		kernelBuilder.Plugins.AddFromType<ConversationSummaryPlugin>();

		kernel = kernelBuilder.Build();

		// Load prompts
		prompts = kernel.CreatePluginFromPromptDirectory(Path.Combine(Application.streamingAssetsPath, "Prompts"));

		// Create a template for chatHistory with settings
		history = new ChatHistory();

		// Create choices
		choices = new() { "ContinueConversation", "EndConversation" };

		// Create few-shot examples
		fewShotExamples = new List<ChatHistory> {
				new ChatHistory()
			{
				new ChatMessageContent(AuthorRole.User, "Can you send a very quick approval to the marketing team?"),
				new ChatMessageContent(AuthorRole.System, "Intent:"),
				new ChatMessageContent(AuthorRole.Assistant, "ContinueConversation")
			},
			new ChatHistory()
			{
				new ChatMessageContent(AuthorRole.User, "Thanks, I'm done for now"),
				new ChatMessageContent(AuthorRole.System, "Intent:"),
				new ChatMessageContent(AuthorRole.Assistant, "EndConversation")
			}};

		// Create handlebars template for intent
		getIntent = kernel.CreateFunctionFromPrompt(
			new()
			{
				Template = @"
<message role=""system"">Instructions: What is the intent of this request?
Do not explain the reasoning, just reply back with the intent. If you are unsure, reply with {{choices[0]}}.
Choices: {{choices}}.</message>

{{#each fewShotExamples}}
    {{#each this}}
        <message role=""{{role}}"">{{content}}</message>
    {{/each}}
{{/each}}

{{ConversationSummaryPlugin-SummarizeConversation history}}

<message role=""user"">{{request}}</message>
<message role=""system"">Intent:</message>",
				TemplateFormat = "handlebars"
			},
			new HandlebarsPromptTemplateFactory()
		);
	}

	public async void UserRequest(string request)
	{
		// Create assistant chatHistory entry
		chatUI.Container.AddMessage(new Message(chatUI.Members[1], string.Empty));
		var message = chatUI.Container.ContainerObject.GetComponentsInChildren<MessagePresenter>().Last().Content;

		// Invoke prompt
		var intent = await kernel.InvokeAsync(
			getIntent,
			new()
			{
				{ "request", request },
				{ "choices", choices },
				{ "history", history },
				{ "fewShotExamples", fewShotExamples }
			}
		);

		if (intent.ToString() == "EndConversation")
		{
			message.text = "Glad to help!";
			await Task.Delay(3000);
			Application.Quit();
		}

		// Get chatHistory response
		var response = kernel.InvokeStreamingAsync<StreamingChatMessageContent>(
			prompts["chat"],
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