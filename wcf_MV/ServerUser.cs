﻿using System.ServiceModel;

namespace wcf_MV
{
    internal class ServerUser
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public OperationContext operationContext { get; set; }

    }
}