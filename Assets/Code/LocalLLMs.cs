using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Use local server LLMs: https://lmstudio.ai/
/// </summary>
public class LocalLLMs : HttpClientHandler
{
	Uri endpoint;

	public LocalLLMs(string endpoint) => this.endpoint = new Uri(endpoint);

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		request.RequestUri = endpoint;
		return base.SendAsync(request, cancellationToken);
	}
}
