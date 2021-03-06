﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Host
{
    [ServiceContract]
    public interface IChatClient
    {
        [OperationContract(IsOneWay =true)]
        void RecieveMessage(string user, string message);

    }


    [ServiceContract(CallbackContract =typeof(IChatClient))]
    public interface IChatService
    {

        [OperationContract(IsOneWay =true)]
        void Join(string username);
        [OperationContract(IsOneWay = true)]
        void SendMessage(string message);
    }




    [ServiceBehavior(ConcurrencyMode=ConcurrencyMode.Single,InstanceContextMode =InstanceContextMode.Single)]
    public class ChatService : IChatService
    {
        Dictionary<IChatClient, string> _users = new Dictionary<IChatClient, string>();
        public void Join(string username)
        {
            var connection = OperationContext.Current.GetCallbackChannel<IChatClient>();
            _users[connection] = username;
        }

        public void SendMessage(string message)
        {
            var connnection = OperationContext.Current.GetCallbackChannel<IChatClient>();
            string user;
            if (!_users.TryGetValue(connnection, out user))
                return;
            foreach (var other in _users.Keys)
            {
                if (other == connnection)
                    continue;
                other.RecieveMessage(user, message);
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(ChatService));
            host.Open();
            Console.WriteLine("Service is ready");
            Console.ReadLine();
            host.Close();
        }
    }
}
