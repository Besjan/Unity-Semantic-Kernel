using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Use LM Studio to server LLMs: https://lmstudio.ai/
/// </summary>
public class LMStudio : HttpClientHandler
{
	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		request.RequestUri = new Uri($"http://localhost:1234{request.RequestUri.PathAndQuery}");
		return base.SendAsync(request, cancellationToken);
	}
}
