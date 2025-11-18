using System;
using System.ClientModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConnectionMgr.Properties;
using OpenAI;
using OpenAI.Chat;

public class OpenAiChatService
{
	private readonly OpenAIClient _client;
	private readonly string _model;
	private readonly string _systemPrompt = @"You are an IT Security Professional that knows everything about MS Windows prozecess, networking and security threads.
Do not include any follow-up questions like for example ""Do you want me to ..."", just give the summary.";

	/// <summary>
	/// Create a client that talks to Ollama.
	/// </summary>
	/// <param name="systemPrompt">
	/// Optional system message that will be sent on every request.
	/// If null, the class will try to read Settings.Default.SystemPrompt.
	/// </param>
	public OpenAiChatService(string systemPrompt = null)
	{
		var serverUrl = Settings.Default.OpenAiServerUrl;
		var apiKey = Settings.Default.OpenAiApiKey;
		_model = "gemma3:12b"; // Settings.Default.OpenAiModel;

		var options = new OpenAIClientOptions
		{
			Endpoint = new Uri(serverUrl)
		};

		_client = new OpenAIClient(new ApiKeyCredential(apiKey), options);
		_systemPrompt = systemPrompt ?? _systemPrompt;
	}

	/// <summary>
	/// Call the chat endpoint and return the assistant’s reply.
	/// </summary>
	/// <param name="userPrompt">User’s message.</param>
	/// <param name="systemPrompt">
	/// Optional system message for this particular call. If null, the instance‑level
	/// system prompt (or none) is used.
	/// </param>
	/// <returns>Assistant reply.</returns>
	public async Task<string> GetChatResponseAsync(string userPrompt, string systemPrompt = null)
	{
		// Build the message list: System → User
		var messages = new ChatMessage[]
		{
		          // Only add system if we actually have one
		          !string.IsNullOrWhiteSpace(systemPrompt ?? _systemPrompt)
				? ChatMessage.CreateSystemMessage(systemPrompt ?? _systemPrompt)
				: null,
			ChatMessage.CreateUserMessage(userPrompt)
		}
		.Where(m => m != null)        // filter out the null when no system prompt
		.ToArray();

		var chatClient = _client.GetChatClient(_model);
		var response = await chatClient.CompleteChatAsync(messages);

		if (response?.Value?.Content != null)
		{
			if (response.Value.Content.Count == 0)
				return response.Value.Content[0].Text;
			else
			{
				StringBuilder sb = new StringBuilder();
				foreach(var c in response.Value.Content)
					sb.Append(c.Text);
				return sb.ToString();
			}
		}
		return string.Empty;
	}
}