﻿using System.Collections.Generic;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Globalization;
using GTRevo.Infrastructure.Globalization.Messages.Database;
using Newtonsoft.Json.Linq;

namespace GTRevo.Infrastructure.Web.JSBridge
{
    public class JsonMessageExportCache
    {
        private readonly LocaleManager localeManager;
        private readonly IMessageRepository messageRepository;
        private Dictionary<string, string> localeDictionaries;

        public JsonMessageExportCache(LocaleManager localeManager,
            IMessageRepository messageRepository)
        {
            this.localeManager = localeManager;
            this.messageRepository = messageRepository;

            Refresh();
        }

        public string GetExportedMessages(string localeCode)
        {
            string dictionary;
            if (localeDictionaries.TryGetValue(localeCode, out dictionary))
            {
                return dictionary;
            }

            return new JObject().ToString();
        }

        public void Refresh()
        {
            Dictionary<string, string> locales = new Dictionary<string, string>();
            JsonMessageExporter exporter = new JsonMessageExporter();

            foreach (var locale in localeManager.Locales)
            {
                JObject dictionary = exporter.ExportMessages(messageRepository.GetMessagesForLocale(locale.Value));
                locales.Add(locale.Key, dictionary.ToString());
            }

            localeDictionaries = locales;
        }
    }
}