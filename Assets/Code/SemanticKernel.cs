using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Net.Http;
using TMPro;
using UnityEngine;
using System.Threading.Tasks;

// https://www.youtube.com/watch?v=9Bl7aZ_s9nE&t=1s&ab_channel=LearnMicrosoftAI
public class SemanticKernel : MonoBehaviour
{
	[SerializeField] string promptTemplate = "";

	[SerializeField] TextMeshProUGUI log;

	private Kernel kernel;
	private OpenAIPromptExecutionSettings openAIPromptExecutionSettings;

	void Awake()
	{
		kernel = Kernel.CreateBuilder()
			.AddOpenAIChatCompletion(modelId: "_", apiKey: "_", httpClient: new HttpClient(new LMStudio()))
			.Build();

		openAIPromptExecutionSettings = new()
		{
			MaxTokens = 100,
			Temperature = 1.0,
		};
	}

	private async void OnEnable()
	{
		var kernelFunction = KernelFunctionFactory.CreateFromPrompt(promptTemplate, openAIPromptExecutionSettings);

		var response = kernel.InvokeStreamingAsync(kernelFunction);

		await foreach (var item in response)
		{
			log.text += item;
			await Task.Yield();
		}
	}
}
