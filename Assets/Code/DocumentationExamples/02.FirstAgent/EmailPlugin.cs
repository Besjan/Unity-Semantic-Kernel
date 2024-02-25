using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Threading.Tasks;

public class EmailPlugin
{
	[KernelFunction]
	[Description("Sends an email to a recipient.")]
	public async Task SendEmailAsync(
		Kernel kernel,
		[Description("Semicolon delimitated list of emails of the recipients")] string recipientEmails,
		string subject,
		string body
	)
	{
		// Add logic to send an email using the recipientEmails, subject, and body
		// For now, we'll just print out a success message to the console
		UnityEngine.Debug.Log("Email sent!");
		UnityEngine.Debug.Log($"Subject: {subject}");
		UnityEngine.Debug.Log($"Body: {body}");
	}
}