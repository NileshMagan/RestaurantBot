using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using RestaurantBot.Models;
using System.Collections.Generic; //Lists
using Microsoft.WindowsAzure.MobileServices;
//using Microsoft.Bot.Builder.FormFlow;

namespace RestaurantBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                //MobileServiceClient client = AzureManager.AzureManagerInstance.AzureClient;

                //GET BOT DATA
                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

                var userMessage = activity.Text;

                string endOutput = "Hi there! Tell me a location and I will show you some good food!";
                bool notFood = false;


                //////////
                bool loggedIn = userData.GetProperty<bool>("loggedIn");
                bool verified = userData.GetProperty<bool>("verified");
                if (loggedIn == false)
                {
                    endOutput += " Please login by entering your name";
                    userData.SetProperty<bool>("loggedIn", true);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);

                    //return
                    Activity infoReply = activity.CreateReply(endOutput);
                    await connector.Conversations.ReplyToActivityAsync(infoReply);
                }
                else if ((loggedIn == true) && (verified == false))
                {
                    endOutput = "Thank you! ask me something about restaurants now ";
                    userData.SetProperty<bool>("verified", true);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);

                    //Get user Name
                    userData.SetProperty<string>("currentUser", userMessage);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    string currentUser = userData.GetProperty<string>("currentUser");

                    endOutput += currentUser;
                    Activity infoReply = activity.CreateReply(endOutput);
                    await connector.Conversations.ReplyToActivityAsync(infoReply);
                    //endOutput = homecity;
                }
                else
                {
                    ///////////////

                    string currentUser = userData.GetProperty<string>("currentUser");

                    //[Prompt("What is your first name? {||}")];

                    //if (userMessage.Length != 0) //Irrelevant
                    //{
                    if (userMessage.Length > 3)
                    {
                        if (userMessage.ToLower().Equals("home"))
                        {

                            notFood = true;
                            string homecity = userData.GetProperty<string>("HomeCity");
                            if (homecity == null)
                            {
                                endOutput = "Home City not assigned";
                            }
                            else
                            {
                                endOutput = homecity;
                            }
                        }
                        else if (userMessage.ToLower().Substring(0, 5).Equals("hello"))
                        {
                            notFood = true;
                            // calculate something for us to return
                            if (userData.GetProperty<bool>("SentWelcome"))
                            {
                                endOutput = "Nice to see you again!";
                            }
                            else
                            {
                                endOutput = "Hi " + currentUser + "! Tell me a location and I will show you some good food!";
                                userData.SetProperty<bool>("SentWelcome", true);
                                await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                            }

                            // return our reply to the user
                            //Activity infoReply = activity.CreateReply(endOutput);
                            //await connector.Conversations.ReplyToActivityAsync(infoReply);

                            // }
                        }
                        else if (userMessage.ToLower().Contains("clear"))
                        {

                            endOutput = "Your bot data has been cleared";
                            await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                            notFood = true;

                        }
                        /*else if (userMessage.ToLower().Equals("home"))
                        {

                            notFood = true;
                            string homecity = userData.GetProperty<string>("HomeCity");
                            if (homecity == null)
                            {
                                endOutput = "Home City not assigned";
                            }
                            else
                            {
                                endOutput = homecity;
                            }
                        }*/
                        else if (userMessage.ToLower().Substring(0, 8).Equals("i ate at"))
                        {

                            string tempName = userMessage.Substring(9);

                            myNewTable temp = new myNewTable()
                            {
                                //updatedAt = DateTime.Now.ToString(),
                                //version = "AAAAAAAAB9k",
                                //id = "8b0e5117",
                                //deleted = false,
                                username = currentUser,
                                restaurantName = tempName
                                // createdAt = DateTime.Now.ToString()
                            };

                            await AzureManager.AzureManagerInstance.AddTable(temp);
                            //await AzureManager.AzureManagerInstance.DeleteTable(temp);

                            notFood = true;

                            endOutput = "New restaurant added [" + temp.createdAt + "]";
                        }
                        else if (userMessage.ToLower().Substring(0, 8).Equals("set home"))
                        {

                            string homeCity = userMessage.Substring(9);
                            userData.SetProperty<string>("HomeCity", homeCity);
                            await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                            endOutput = homeCity;
                            notFood = true;
                        }
                        else if (userMessage.ToLower().Equals("get table"))
                        {
                            List<myNewTable> fromServer = await AzureManager.AzureManagerInstance.GetMyTables();
                            endOutput = "";
                            //"Timeline t" is just the object 't'which is of Timeline type in timelines
                            foreach (myNewTable Tab in fromServer)
                            {
                                if (Tab.username == currentUser)
                                {
                                    endOutput += "[" + Tab.createdAt + "] was the date you ate at " + Tab.restaurantName + "\n\n";
                                }
                            }

                            if (endOutput == "") { endOutput += "No data added yet! \n Add restaurants you have eaten at by typing: 'I ate at (restaurant_name)'"; }
                            notFood = true;

                        }
                        else if (userMessage.ToLower().Equals("delete history"))
                        {
                            //List<string> ListID;
                            //List<string> ListID = new List<string
                            int i = 0;
                            int newLength = 0;
                            List<myNewTable> fromServer = await AzureManager.AzureManagerInstance.GetMyTables();

                            string[] ListID = new string[fromServer.Count];

                            //endOutput = "";
                            foreach (myNewTable Tab in fromServer)
                            {
                                if (Tab.username == currentUser)
                                {
                                    //endOutput += "[" + Tab.createdAt + "] was the date you ate at " + Tab.restaurantName + "\n\n";
                                    ListID[i] = Tab.id;
                                    newLength++;
                                }
                                i++;
                            }

                            //if (endOutput == "") { endOutput += "No data added yet! \n Add restaurants you have eaten at by typing: 'I ate at (restaurant_name)'"; }
                            //notFood = true;
                            int pos = 0;
                            string[] deleteArray = new string[newLength];
                            for (int q = 0; q < (i); q++)
                            {
                                if (ListID[q] != null)
                                {
                                    deleteArray[pos] = ListID[q];
                                    pos++;
                                }
                            }

                            for (int p = 0; p < newLength; p++)
                            {
                                myNewTable temp = new myNewTable()
                                {
                                    //updatedAt = DateTime.Now.ToString(),
                                    //version = "AAAAAAAAB9k",
                                    id = deleteArray[p],
                                    //deleted = false,
                                    //username = currentUser,
                                    //restaurantName = tempName
                                    // createdAt = DateTime.Now.ToString()
                                };

                                //await AzureManager.AzureManagerInstance.AddTable(temp);
                                await AzureManager.AzureManagerInstance.DeleteTable(temp);
                            }



                            notFood = true;

                            endOutput = "Restaurants deleted";


                        }



                        }




                    if (notFood)
                    {
                        Activity infoReply = activity.CreateReply(endOutput);
                        await connector.Conversations.ReplyToActivityAsync(infoReply);
                    }
                    else
                    {
                        //var accessStatus = await Geolocator.RequestAccessAsync();
                        // calculate something for us to return
                        int length = (activity.Text ?? string.Empty).Length;


                        CategoryObject.RootObject rootObject;
                        //    Console.WriteLine(activity.Attachments[0].ContentUrl);

                        HttpClient client = new HttpClient();
                        client.DefaultRequestHeaders.Add("user-key", "2bf65b2604dac6d629213e12e26ebc42");
                        //client.DefaultRequestHeaders.Add("Authorization", "Bearer " + "2bf65b2604dac6d629213e12e26ebc42");
                        string x = await client.GetStringAsync(new Uri("https://developers.zomato.com/api/v2.1/categories"));

                        rootObject = JsonConvert.DeserializeObject<CategoryObject.RootObject>(x);

                        Activity reply = activity.CreateReply($"Received was: {rootObject.categories[0].categories.name}");
                        //await connector.Conversations.ReplyToActivityAsync(reply);


                        //Google

                        GoogleObject.RootObject rootObject2;
                        HttpClient googleClient = new HttpClient();
                        //client.DefaultRequestHeaders.Add("user-key", "2bf65b2604dac6d629213e12e26ebc42");
                        //client.DefaultRequestHeaders.Add("Authorization", "Bearer " + "2bf65b2604dac6d629213e12e26ebc42");
                        string y = await googleClient.GetStringAsync(new Uri("https://maps.googleapis.com/maps/api/geocode/json?address=" + activity.Text + "&key=AIzaSyB9Q-_V9ZY6fZCInmMaZNySUxIIPnk0TY8"));
                        rootObject2 = JsonConvert.DeserializeObject<GoogleObject.RootObject>(y);

                        //reply = activity.CreateReply($"lat is: {rootObject2.results[0].geometry.location.lat} and long is: {rootObject2.results[0].geometry.location.lng}");
                        //await connector.Conversations.ReplyToActivityAsync(reply);



                        //Geo zomato API to get nearby resteraunts
                        GeoObject.RootObject rootObject3;
                        HttpClient GeoClient = new HttpClient();
                        GeoClient.DefaultRequestHeaders.Add("user-key", "2bf65b2604dac6d629213e12e26ebc42");
                        //client.DefaultRequestHeaders.Add("lat", (rootObject2.results[0].geometry.location.lat).ToString());
                        //client.DefaultRequestHeaders.Add("lon", (rootObject2.results[0].geometry.location.lng).ToString());
                        string z = await GeoClient.GetStringAsync(new Uri("https://developers.zomato.com/api/v2.1/geocode?lat=" + rootObject2.results[0].geometry.location.lat + "&lon=" + rootObject2.results[0].geometry.location.lng));
                        rootObject3 = JsonConvert.DeserializeObject<GeoObject.RootObject>(z);

                        reply = activity.CreateReply($"RESTAURANTS NEAR YOU:");
                        await connector.Conversations.ReplyToActivityAsync(reply);

                        for (int i = 0; i < rootObject3.popularity.nearby_res.Count; i++)
                        {
                            //Was here to output res id's
                            reply = activity.CreateReply($"{rootObject3.popularity.nearby_res[i]}");


                            //RES_ID to Retaurants
                            ResObject.RootObject rootObject4;
                            HttpClient ResClient = new HttpClient();
                            ResClient.DefaultRequestHeaders.Add("user-key", "2bf65b2604dac6d629213e12e26ebc42");
                            string w = await ResClient.GetStringAsync(new Uri("https://developers.zomato.com/api/v2.1/restaurant?res_id=" + rootObject3.popularity.nearby_res[i]));
                            rootObject4 = JsonConvert.DeserializeObject<ResObject.RootObject>(w);
                            reply = activity.CreateReply($"{rootObject4.name}");
                            //await connector.Conversations.ReplyToActivityAsync(reply);
                            //END RES_ID



                            //await connector.Conversations.ReplyToActivityAsync(reply);


                            //Cards

                            //Activity replyToConversation = activity.CreateReply("MSA information");
                            reply.Recipient = activity.From;
                            reply.Type = "message";
                            reply.Attachments = new List<Attachment>();
                            List<CardImage> cardImages = new List<CardImage>();
                            cardImages.Add(new CardImage(url: rootObject4.featured_image));
                            List<CardAction> cardButtons = new List<CardAction>();
                            //Buttons
                            CardAction plButton = new CardAction()
                            {
                                Value = rootObject4.url,
                                Type = "openUrl",
                                Title = "More info"
                            };
                            cardButtons.Add(plButton);
                            CardAction plButton1 = new CardAction()
                            {
                                Value = rootObject4.menu_url,
                                Type = "openUrl",
                                Title = "Menu"
                            };
                            cardButtons.Add(plButton1);
                            /*CardAction plButton2 = new CardAction()
                            {
                                Value = rootObject4.featured_image,
                                Type = "openUrl",
                                Title = "Menu"
                            };
                            cardButtons.Add(plButton2);*/

                            ThumbnailCard plCard = new ThumbnailCard()
                            {
                                Title = "Visit " + rootObject4.name,
                                Subtitle = "Rating: " + rootObject4.user_rating.rating_text + " " + rootObject4.user_rating.aggregate_rating + "\n Average cost for two: $" + rootObject4.average_cost_for_two,
                                Images = cardImages,
                                Buttons = cardButtons
                            };
                            Attachment plAttachment = plCard.ToAttachment();
                            reply.Attachments.Add(plAttachment);
                            await connector.Conversations.SendToConversationAsync(reply);

                            //return Request.CreateResponse(HttpStatusCode.OK);

                        }

                    }

                }

                // return our reply to the user
                //reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
                //await connector.Conversations.ReplyToActivityAsync(reply);
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