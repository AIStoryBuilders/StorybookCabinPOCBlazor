using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using OpenAI;
using OpenAI.Models;
using StorybookCabinPOCBlazor.Models;

namespace StorybookCabinPOCBlazor.Api
{
    [ApiController]
    [Route("[controller]")]
    public class ReceiveCallFromGodot : ControllerBase
    {
        private readonly OpenAIServiceOptions _openAIServiceOptions;
        private StorybookCabinPOCBlazorContext _storybookCabinPOCBlazorContext;

        public ReceiveCallFromGodot(
            IOptions<OpenAIServiceOptions> openAIServiceOptions,
            StorybookCabinPOCBlazorContext storybookCabinPOCBlazorContext)
        {
            _openAIServiceOptions = openAIServiceOptions.Value;
            _storybookCabinPOCBlazorContext = storybookCabinPOCBlazorContext;
        }

        [HttpPost]
        public IActionResult Post([FromBody] UserData userData)
        {
            // Verify the user's identity by checking the database
            var userName = userData.userName;
            var hTTPToken = userData.hTTPToken;
            var gameBoard = userData.gameBoard;

            var user = _storybookCabinPOCBlazorContext.Users
                .Where(u => u.Email == userName && u.Objectidentifier == hTTPToken)
                .FirstOrDefault();

            string openAIResponse = "";

            if (user != null)
            {
                // Call GetOpenAIResponse to get a response from OpenAI
                openAIResponse = GetOpenAIResponse(userData.userText).Result;
            }
            else
            {
                openAIResponse = "You are not authorized to use this service.";
            }

            MessageData objMessageData = new MessageData();
            objMessageData.message = $"{openAIResponse}";

            // Return objMessageData as JSON
            return Ok(objMessageData);
        }

        [HttpGet]
        public IActionResult Get()
        {
            // Return a response
            return Ok(new { Message = "Data received successfully." });
        }

        // OpenAI
        private async Task<string> GetOpenAIResponse(string? userText)
        {
            // Get the API key from the appsettings.json file
            var ApiKey = _openAIServiceOptions.ApiKey;

            // Create a new instance of OpenAIClient using
            // the ApiKey and Organization
            var api =
            new OpenAIClient(new OpenAIAuthentication(ApiKey));

            var messages = new List<OpenAI.Chat.Message>
            {
                new OpenAI.Chat.Message(Role.System, "You are a helpful assistant."),
                new OpenAI.Chat.Message(Role.User, userText),
            };

            // Create a new instance of the ChatRequest class, passing in the
            // messages list, and other parameters

            var chatRequest = new OpenAI.Chat.ChatRequest(
                messages, temperature: 0.1, model: Model.GPT3_5_Turbo);

            var ChatResponse =
            await api.ChatEndpoint.GetCompletionAsync(chatRequest);

            return ChatResponse.FirstChoice.Message;
        }
    }

    public class UserData
    {
        public string? userName { get; set; }
        public string? hTTPToken { get; set; }
        public string? userText { get; set; }
		public string? gameBoard { get; set; }
	}

    public class MessageData
    {
        public string? message { get; set; }
    }
}
