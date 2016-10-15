using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;
namespace DeadlinerBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        public static MobileServiceClient MobileService =
new MobileServiceClient(
    "https://deadlinerhse.azurewebsites.net"
);
        static int stage = -2;
        static String title = "";
        static String text = "";
        static String UserLogin = null;
        TodoItem item = null;
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                // calculate something for us to return
                int length = (activity.Text ?? string.Empty).Length;

                string message = "";
                int i;
                if (stage==-2)
                {
                    message = "Пришли мне свой логин";
                    stage = -1;
                }
                else
                {
                    switch (stage)
                    {
                        case -1: item = item ?? new TodoItem(); UserLogin = activity.Text; message = "Пришли мне название задачи"; stage = 1; break;
                        case 0: item = item ?? new TodoItem(); message = "Пришли мне название задачи"; stage++; break;
                        case 1: item = item ?? new TodoItem(); title = activity.Text; message = "Пришли мне описание задачи"; stage++; break;
                        case 2: item = item ?? new TodoItem(); text = activity.Text; message = "Введите количество дней до дедлайна"; stage++; break;
                        case 3:
                            if (int.TryParse(activity.Text, out i)) { item = item ?? new TodoItem(); item.UserName = UserLogin;  item.Title = title; item.Text = text; item.DueTo = ((DateTime)activity.Timestamp).AddDays(i); await MessagesController.MobileService.GetTable<TodoItem>().InsertAsync(item); message = $"Задача {item.Title}:{item.Text} добавлена с дедлайном на {item.DueTo}."; stage = 0; break; }
                            else { message = "Пришли целое число дней"; break; }
                    }
                }

                // return our reply to the user
                Activity reply = activity.CreateReply(message);//$"You sent {activity.Text} which was {length} characters"
                await connector.Conversations.ReplyToActivityAsync(reply);


            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}