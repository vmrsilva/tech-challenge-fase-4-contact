using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechChallange.Contact.Domain.Contact.Messaging
{
    public class ContactCreateMessageDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }
        public Guid RegionId { get; set; }
    }
}
