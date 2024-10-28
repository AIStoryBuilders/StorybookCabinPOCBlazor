using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using OpenAI;
using OpenAI.Models;
using StorybookCabinPOCBlazor.Models;
using System.Text;
using System.Text.Json;

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
            var userName = userData.UserName;
            var hTTPToken = userData.HttpToken;
            var userText = userData.UserText;
            var gameBoard = userData.GameBoard;

            var user = _storybookCabinPOCBlazorContext.Users
                .Where(u => u.Email == userName && u.Objectidentifier == hTTPToken)
                .FirstOrDefault();

            string openAIResponse = "";

            if (user != null)
            {
                // Call GetOpenAIResponse to get a response from OpenAI
                var OpenAIReponse = GetOpenAIResponse(userData);

                if(OpenAIReponse.Result != null)
                {
                    openAIResponse = OpenAIReponse.Result;
                }
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
        private async Task<string> GetOpenAIResponse(UserData userData)
        {
            // Get the API key from the appsettings.json file
            var ApiKey = _openAIServiceOptions.ApiKey;

            // Create a new instance of OpenAIClient using the ApiKey
            var api = new OpenAIClient(new OpenAIAuthentication(ApiKey));

            // Serialize 
            string jsonGameBoard = JsonSerializer.Serialize(userData.GameBoard);
            string jsonCharacterInfo = JsonSerializer.Serialize(userData.CharacterInfo);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Please examine the following JSON that represents the #CharacterInfo and their coordinates on the gameboard:");
            sb.AppendLine(jsonCharacterInfo);
            sb.AppendLine("");
            sb.AppendLine("Please examine the following JSON that represents the gameboard:");
            sb.AppendLine(jsonGameBoard);
            sb.AppendLine("");
            sb.AppendLine("Please do the following based on the #PlayerInstructions.:");
            sb.AppendLine("");
            sb.AppendLine("#1 - Only output an updated #CharacterInfo json object that updates the character coordinates on the gameboard ");
            sb.AppendLine("based on the #PlayerInstructions. ");
            sb.AppendLine("");
            sb.AppendLine("#2 - Only allow #CharacterInfo to put a character in a gameboard coordinate thay is Type empty.");
            sb.AppendLine("");
            sb.AppendLine("#3 - A #CharacterInfo coordinate cannot occupy the same coordinate as another character.");
            sb.AppendLine("");
            sb.AppendLine("#4 - Only update #CharacterInfo coordinates");
            sb.AppendLine("");
            sb.AppendLine("#Player Instructions: ");
            sb.AppendLine(userData.UserText);

            var messages = new List<OpenAI.Chat.Message>
            {
                new OpenAI.Chat.Message(Role.System, "You are a helpful assistant that responds in JSON format."),
                new OpenAI.Chat.Message(Role.User, sb.ToString())
            };

            // Create a new instance of the ChatRequest class, ensuring we ask for JSON format
            var chatRequest = new OpenAI.Chat.ChatRequest(
                messages,
                temperature: 0.1,
                model: Model.GPT4o
            );

            var chatResponse = await api.ChatEndpoint.GetCompletionAsync(chatRequest);

            // Return the JSON response
            return chatResponse.FirstChoice.Message.Content;
        }

    }

    public class GameBoardCell
    {
        public string? Type { get; set; }
        public string? description { get; set; }
    }

    public class CharacterInfoCell
    {
        public int age { get; set; }
        public string? description { get; set; }
        public string? name { get; set; }
        public string? gender { get; set; }
    }

    public class UserData
    {
        public string? UserName { get; set; }
        public string? HttpToken { get; set; }
        public string? UserText { get; set; }
        public Dictionary<string, GameBoardCell>? GameBoard { get; set; }
        public Dictionary<string, CharacterInfoCell>? CharacterInfo { get; set; }
    }

    public class MessageData
    {
        public string? message { get; set; }
    }



}
