// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using UnityEngine;

/// <summary>
/// This example demonstrates how to templatize prompts as described at
/// https://learn.microsoft.com/semantic-kernel/prompts/templatizing-prompts
/// </summary>
public class Templates : MonoBehaviour
{
	[SerializeField] Chat chatUI;

	private Kernel kernel;
	KernelFunction chat;
	ChatHistory history;

	List<string> choices;
	List<ChatHistory> fewShotExamples;
	KernelFunction getIntent;

	private void Awake()
	{
		chatUI.OnMessageSent.AddListener(async (string message) => await UserRequest(message));

		kernel = Kernel.CreateBuilder()
			.AddOpenAIChatCompletion(modelId: "_", apiKey: "_", httpClient: new HttpClient(new LMStudio()))
			.Build();

		// Create a Semantic Kernel template for chat
		chat = kernel.CreateFunctionFromPrompt(
			@"{{$history}}
            User: {{$request}}
            Assistant: ");

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

{{#each chatHistory}}
    <message role=""{{role}}"">{{content}}</message>
{{/each}}

<message role=""user"">{{request}}</message>
<message role=""system"">Intent:</message>",
				TemplateFormat = "handlebars"
			},
			new HandlebarsPromptTemplateFactory()
		);

		history = new();
	}

	public async Task UserRequest(string request)
	{
		// Create assistant chat entry
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

		// Get chat response
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