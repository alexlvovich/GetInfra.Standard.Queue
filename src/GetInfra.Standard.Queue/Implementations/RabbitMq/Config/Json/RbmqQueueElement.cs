﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GetInfra.Standard.Queue.Implementations.RabbitMq.Config.Json
{
    public class RbmqQueueElement
    {
        public string Host { get; set; }
        
        public string Username { get; set; }
        
        public string Password { get; set; }

      
        public string Exchange { get; set; }

       
        public string Queue { get; set; }

      
        public string RoutingKey { get; set; }

    
        public ushort QoS { get; set; }

  
        public string ExchangeType { get; set; }

       
        public bool IsDurable { get; set; }

       
        public bool AutoDelete { get; set; }

     
        public ushort MessageLimit { get; set; }

       
        public string Vhost { get; set; }

     
        public bool Bind { get; set; }

     
        public bool DeadLetters { get; set; }

     
        public int Port { get; set; }

      
        public string Name { get; set; }
    }
}
