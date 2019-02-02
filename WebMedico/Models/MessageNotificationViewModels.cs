using System.Collections.Generic;

namespace WebMedico.Models
{
    public sealed class MessageNotificationViewModel
    {
        public IEnumerable<MessageNotificationItemViewModel> Items { get; set; }

        public int TotalUnreadMessagesCount { get; set; }

        public MessageNotificationViewModel()
        {
            Items = new List<MessageNotificationItemViewModel>();
        }
    }

    public sealed class MessageNotificationItemViewModel
    {
        public string ChannelId { get; set; }

        public int UnreadMessagesCount { get; set; }

        public string NumberCircle { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
    }
}