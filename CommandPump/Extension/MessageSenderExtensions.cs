﻿// ==============================================================================================================
// Shamelessly taken from the Microsoft patterns & practices CQRS Journey project
// https://github.com/MicrosoftArchive/cqrs-journey
// https://msdn.microsoft.com/en-us/library/jj554200.aspx
// ==============================================================================================================
// Microsoft patterns & practices
// CQRS Journey project
// ==============================================================================================================
// ©2012 Microsoft. All rights reserved. Certain content used with permission from contributors
// http://go.microsoft.com/fwlink/p/?LinkID=258575
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance 
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and limitations under the License.
// ==============================================================================================================

using CommandPump.Common;
using CommandPump.Contract;
using System.Collections.Generic;
using System.Linq;

namespace CommandPump.Extension
{

    /// <summary>
    /// Provides usability overloads for <see cref="IMessageSender"/>
    /// </summary>
    public static class MessageSenderExtensions
    {
        public static void Send<T>(this IMessageSender sender, T command)
        {
            sender.Send(new Envelope<T>(command));
        }

        public static void Send<T>(this IMessageSender sender, IEnumerable<T> commands)
        {
            sender.Send(commands.Select(x => new Envelope<T>(x)));
        }

        public static void SendBatch<T>(this IMessageSender sender, IEnumerable<T> commands)
        {
            sender.SendBatch(commands.Select(x => new Envelope<T>(x)));
        }
    }
}

