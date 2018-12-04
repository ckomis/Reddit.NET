﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Reddit.NET;
using Reddit.NET.Exceptions;
using Reddit.NET.Models.Structures;
using RestSharp;
using System;
using System.Collections.Generic;

namespace Reddit.NETTests.ModelTests.WorkflowTests
{
    [TestClass]
    public class ModmailTests : BaseTests
    {
        public ModmailTests() : base() { }

        [TestMethod]
        public void GetConversations()
        {
            ConversationContainer conversationContainer = reddit.Models.Modmail.GetConversations("", "", "user", "all");

            Validate(conversationContainer);

            conversationContainer = reddit.Models.Modmail.GetConversations("", reddit.Models.Subreddits.About(testData["Subreddit"]).Data.Name, "recent", "all");

            Validate(conversationContainer);
        }

        [TestMethod]
        public void Conversation()
        {
            // If a non-moderator tries to specify a "to" username for NewConversation, the API will return 403 Forbidden.  --Kris
            Validate(reddit.Models.Modmail.NewConversation("This is a test.", false, testData["Subreddit"], "Test Message", reddit.Models.Account.Me().Name));

            // If user is already muted, we can't continue because Unmute requires the ID of the conversation in which the user was originally muted.  --Kris
            ModmailConversationContainer modmailConversationContainer = null;
            try
            {
                modmailConversationContainer = reddit2.Models.Modmail.NewConversation("This is a test with no target user.", false, testData["Subreddit"], "Test Message", "");
            }
            catch (RedditBadRequestException ex)
            {
                CheckBadRequest("USER_MUTED", "Target user cannot be muted when the test begins.", ex);
            }

            Validate(modmailConversationContainer);
            Assert.IsTrue(modmailConversationContainer.Messages.Count == 1);

            ModmailConversationContainer modmailConversationContainer2 = reddit.Models.Modmail.GetConversation(modmailConversationContainer.Conversation.Id, false);

            Validate(modmailConversationContainer2);
            Assert.AreEqual(modmailConversationContainer.Conversation.Id, modmailConversationContainer2.Conversation.Id);

            modmailConversationContainer2 = reddit.Models.Modmail.NewMessage(modmailConversationContainer.Conversation.Id, "This is a test reply.", false, false);

            Validate(modmailConversationContainer2);
            Assert.AreEqual(modmailConversationContainer.Conversation.Id, modmailConversationContainer2.Conversation.Id);
            Assert.IsTrue(modmailConversationContainer2.Messages.Count == 2);

            modmailConversationContainer2 = reddit.Models.Modmail.MarkHighlighted(modmailConversationContainer.Conversation.Id);

            Validate(modmailConversationContainer2);
            Assert.AreEqual(modmailConversationContainer.Conversation.Id, modmailConversationContainer2.Conversation.Id);
            Assert.IsTrue(modmailConversationContainer2.Conversation.IsHighlighted);

            modmailConversationContainer2 = reddit.Models.Modmail.RemoveHighlight(modmailConversationContainer.Conversation.Id);

            Validate(modmailConversationContainer2);
            Assert.AreEqual(modmailConversationContainer.Conversation.Id, modmailConversationContainer2.Conversation.Id);
            Assert.IsFalse(modmailConversationContainer2.Conversation.IsHighlighted);

            modmailConversationContainer2 = reddit.Models.Modmail.Mute(modmailConversationContainer.Conversation.Id);

            Validate(modmailConversationContainer2);
            Assert.AreEqual(modmailConversationContainer.Conversation.Id, modmailConversationContainer2.Conversation.Id);

            modmailConversationContainer2 = reddit.Models.Modmail.UnMute(modmailConversationContainer.Conversation.Id);

            Validate(modmailConversationContainer2);
            Assert.AreEqual(modmailConversationContainer.Conversation.Id, modmailConversationContainer2.Conversation.Id);

            ModmailUser modmailUser = reddit.Models.Modmail.User(modmailConversationContainer.Conversation.Id);

            Validate(modmailUser);
            Assert.AreEqual(GetTargetUserModel().Name, modmailUser.Name);

            reddit.Models.Modmail.MarkRead(modmailConversationContainer.Conversation.Id);
            reddit.Models.Modmail.MarkUnread(modmailConversationContainer.Conversation.Id);
        }
    }
}
