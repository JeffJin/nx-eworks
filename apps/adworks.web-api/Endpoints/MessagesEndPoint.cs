//// Copyright (c) .NET Foundation. All rights reserved.
//// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
//
//using System.Collections.Generic;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;
//using adworks.media_common;
//using adworks.message_common;
//using Microsoft.AspNetCore.Connections;
//
//namespace adworks.media_web_api
//{s
//    public class MessagesEndPoint : EndPoint
//    {
//        public ConnectionList Connections { get; } = new ConnectionList();
//
//        public override async Task OnConnectedAsync(ConnectionContext connection)
//        {
//            Connections.Add(connection);
//            var connectMessage = new Message()
//            {
//                Topic = "MessageEndPointConnected",
//                Body = $"{connection.ConnectionId} connected ({connection.Metadata[ConnectionMetadataNames.Transport]})"
//            };
//            await Broadcast(SerializeHelper.Serialize(connectMessage));
//
//            try
//            {
//                while (await connection.Transport.In.WaitToReadAsync())
//                {
//                    if (connection.Transport.In.TryRead(out var buffer))
//                    {
//                        await Broadcast(buffer);
//                    }
//                }
//            }
//            finally
//            {
//                Connections.Remove(connection);
//
//                var closeMessage = new Message()
//                {
//                    Topic = "MessageEndPointDisconnected",
//                    Body = $"{connection.ConnectionId} disconnected ({connection.Metadata[ConnectionMetadataNames.Transport]})"
//                };
//                await Broadcast(SerializeHelper.Serialize(closeMessage));
//            }
//        }
//
//        private Task Broadcast(object payload)
//        {
//            return Broadcast(SerializeHelper.Serialize(payload));
//        }
//
//        private Task Broadcast(byte[] payload)
//        {
//            var tasks = new List<Task>(Connections.Count);
//
//            foreach (var c in Connections)
//            {
//                tasks.Add(c.Transport.Out.WriteAsync(payload));
//            }
//
//            return Task.WhenAll(tasks);
//        }
//    }
//}
