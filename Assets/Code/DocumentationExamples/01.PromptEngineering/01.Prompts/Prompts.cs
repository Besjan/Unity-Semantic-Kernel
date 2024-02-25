// Copyright (c) Microsoft. All rights reserved.

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using TMPro;
using UnityEngine;

/// <summary>
/// https://learn.microsoft.com/semantic-kernel/prompts/your-first-prompt
/// </summary>
public class Prompts : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI log;

	private Kernel kernel;

	private void Awake()
	{
		kernel = Kernel.CreateBuilder()
			.AddOpenAIChatCompletion(modelId: "_", apiKey: "_", httpClient: new HttpClient(new LMStudio()))
			.Build();

		_ = RunAsync();
	}

	public async Task RunAsync()
	{
		// </KernelCreation>

		// 0.0 Initial prompt
		//////////////////////////////////////////////////////////////////////////////////
		string request = "I want to send an email to the marketing team celebrating their recent milestone.";
		log.text  = request;
		string prompt = $"What is the intent of this request? {request}";
		log.text += "\n\n" + prompt;

		/* Uncomment this code to make this example interactive
		// <InitialPrompt>
		Write("Your request: ");
		string request = ReadLine()!;
		string prompt = $"What is the intent of this request? {request}";
		// </InitialPrompt>
		*/

		log.text += "\n\n" + "0.0 Initial prompt";
		// <InvokeInitialPrompt>
		log.text += "\n\n" + await kernel.InvokePromptAsync(prompt);
		// </InvokeInitialPrompt>

		// 1.0 Make the prompt more specific
		//////////////////////////////////////////////////////////////////////////////////
		// <MoreSpecificPrompt>
		prompt = @$"What is the intent of this request? {request}
        You can choose between SendEmail, SendMessage, CompleteTask, CreateDocument.";
		// </MoreSpecificPrompt>

		log.text += "\n\n" + "1.0 Make the prompt more specific";
		log.text += "\n\n" + await kernel.InvokePromptAsync(prompt);

		// 2.0 Add structure to the output with formatting
		//////////////////////////////////////////////////////////////////////////////////
		// <StructuredPrompt>
		prompt = @$"Instructions: What is the intent of this request?
        Choices: SendEmail, SendMessage, CompleteTask, CreateDocument.
        User Input: {request}
        Intent: ";
		// </StructuredPrompt>

		log.text += "\n\n" + "2.0 Add structure to the output with formatting";
		log.text += "\n\n" + await kernel.InvokePromptAsync(prompt);

		// 2.1 Add structure to the output with formatting (using Markdown and JSON)
		//////////////////////////////////////////////////////////////////////////////////
		// <FormattedPrompt>
		prompt = @$"## Instructions
Provide the intent of the request using the following format:

```json
{{
    ""intent"": {{intent}}
}}
```

## Choices
You can choose between the following intents:

```json
[""SendEmail"", ""SendMessage"", ""CompleteTask"", ""CreateDocument""]
```

## User Input
The user input is:

```json
{{
    ""request"": ""{request}""
}}
```

## Intent";
		// </FormattedPrompt>

		log.text += "\n\n" + "2.1 Add structure to the output with formatting (using Markdown and JSON)";
		log.text += "\n\n" + await kernel.InvokePromptAsync(prompt);

		// 3.0 Provide examples with few-shot prompting
		//////////////////////////////////////////////////////////////////////////////////
		// <FewShotPrompt>
		prompt = @$"Instructions: What is the intent of this request?
Choices: SendEmail, SendMessage, CompleteTask, CreateDocument.

User Input: Can you send a very quick approval to the marketing team?
Intent: SendMessage

User Input: Can you send the full update to the marketing team?
Intent: SendEmail

User Input: {request}
Intent: ";
		// </FewShotPrompt>

		log.text += "\n\n" + "3.0 Provide examples with few-shot prompting";
		log.text += "\n\n" + await kernel.InvokePromptAsync(prompt);

		// 4.0 Tell the AI what to do to avoid doing something wrong
		//////////////////////////////////////////////////////////////////////////////////
		// <AvoidPrompt>
		prompt = @$"Instructions: What is the intent of this request?
If you don't know the intent, don't guess; instead respond with ""Unknown"".
Choices: SendEmail, SendMessage, CompleteTask, CreateDocument, Unknown.

User Input: Can you send a very quick approval to the marketing team?
Intent: SendMessage

User Input: Can you send the full update to the marketing team?
Intent: SendEmail

User Input: {request}
Intent: ";
		// </AvoidPrompt>

		log.text += "\n\n" + "4.0 Tell the AI what to do to avoid doing something wrong";
		log.text += "\n\n" + await kernel.InvokePromptAsync(prompt);

		// 5.0 Provide context to the AI
		//////////////////////////////////////////////////////////////////////////////////
		// <ContextPrompt>
		string history = @"User input: I hate sending emails, no one ever reads them.
AI response: I'm sorry to hear that. Messages may be a better way to communicate.";

		prompt = @$"Instructions: What is the intent of this request?
If you don't know the intent, don't guess; instead respond with ""Unknown"".
Choices: SendEmail, SendMessage, CompleteTask, CreateDocument, Unknown.

User Input: Can you send a very quick approval to the marketing team?
Intent: SendMessage

User Input: Can you send the full update to the marketing team?
Intent: SendEmail

{history}
User Input: {request}
Intent: ";
		// </ContextPrompt>

		log.text += "\n\n" + "5.0 Provide context to the AI";
		log.text += "\n\n" + await kernel.InvokePromptAsync(prompt);

		// 6.0 Using message roles in chat completion prompts
		//////////////////////////////////////////////////////////////////////////////////
		// <RolePrompt>
		history = @"<message role=""user"">I hate sending emails, no one ever reads them.</message>
<message role=""assistant"">I'm sorry to hear that. Messages may be a better way to communicate.</message>";

		prompt = @$"<message role=""system"">Instructions: What is the intent of this request?
If you don't know the intent, don't guess; instead respond with ""Unknown"".
Choices: SendEmail, SendMessage, CompleteTask, CreateDocument, Unknown.</message>

<message role=""user"">Can you send a very quick approval to the marketing team?</message>
<message role=""system"">Intent:</message>
<message role=""assistant"">SendMessage</message>

<message role=""user"">Can you send the full update to the marketing team?</message>
<message role=""system"">Intent:</message>
<message role=""assistant"">SendEmail</message>

{history}
<message role=""user"">{request}</message>
<message role=""system"">Intent:</message>";
		// </RolePrompt>

		log.text += "\n\n" + "6.0 Using message roles in chat completion prompts";
		log.text += "\n\n" + await kernel.InvokePromptAsync(prompt);

		// 7.0 Give your AI words of encouragement
		//////////////////////////////////////////////////////////////////////////////////
		// <BonusPrompt>
		history = @"<message role=""user"">I hate sending emails, no one ever reads them.</message>
<message role=""assistant"">I'm sorry to hear that. Messages may be a better way to communicate.</message>";

		prompt = @$"<message role=""system"">Instructions: What is the intent of this request?
If you don't know the intent, don't guess; instead respond with ""Unknown"".
Choices: SendEmail, SendMessage, CompleteTask, CreateDocument, Unknown.
Bonus: You'll get $20 if you get this right.</message>

<message role=""user"">Can you send a very quick approval to the marketing team?</message>
<message role=""system"">Intent:</message>
<message role=""assistant"">SendMessage</message>

<message role=""user"">Can you send the full update to the marketing team?</message>
<message role=""system"">Intent:</message>
<message role=""assistant"">SendEmail</message>

{history}
<message role=""user"">{request}</message>
<message role=""system"">Intent:</message>";
		// </BonusPrompt>

		log.text += "\n\n" + "7.0 Give your AI words of encouragement";
		log.text += "\n\n" + await kernel.InvokePromptAsync(prompt);
	}
}
