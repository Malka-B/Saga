using NServiceBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messages
{
    public class PlaceOrder : ICommand
    {
        public string OrderId { get; set; }
    }
}
