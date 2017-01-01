# CommandPump
CommandPump is a message processing framework that attempts to make the processing, sending and receiving of messages as seamless
as possible by handling the orchestration between all of these elements.

## Getting Started
The quickest way to get started is to install MSMQ by running the following PowerShell command as administator:
```powershell
Enable-WindowsOptionalFeature -Online -FeatureName "MSMQ-Server", "MSMQ-Container" 
#Disable-WindowsOptionalFeature -Online -FeatureName "MSMQ-Server", "MSMQ-Container" 
```

Example
```c#
// The MSMQ implementation will create the queue if it does not already exist
string msmqQueueName = System.Environment.MachineName + @"\Private$\CommandPump"; 

MsmqMessageSender msmqSender = new MsmqMessageSender(msmqQueueName);
MsmqMessageReceiver msmqReceiver = new MsmqMessageReceiver(msmqQueueName);
            
CommandPumpSender commandSender = new CommandPumpSender(msmqSender);
CommandPumpReceiver commandReceiver = new CommandPumpReceiver(msmqReceiver);

// Register the class instance that will handle the incoming commands
// The class instance will be cached so do not dispose
commandReceiver.Dispatch.RegisterHandler(new ExampleCommandHandler());

// Create the command
ICommand cmd = new WaitCommand()
{
  Id = "1",
  WaitTime = 100,
  Name = "WaitCommand"
};

// Send the command
commandSender.Send(cmd);

// Start the command pump
commandReceiver.Start();

// Simulate running time
System.Threading.Thread.Sleep(5000);

// Stop receiving further messages and block untill completion
commandReceiver.Stop();
commandReceiver.WaitForCompletion();
```
Example command with handler
```c#
public class ExampleCommand : ICommand
{
    public string Id { get; set; }

    public int WaitTime { get; set; }

    public string Name { get; set; }
}

public class ExampleCommandHandler : ICommandHandler<ExampleCommand>
{
    public void Handle(ExampleCommand Command)
    {
        System.Threading.Thread.Sleep(Command.WaitTime);
    }
}
```

## Motivation
Heavily inspiried by the Microsoft CQRS Journey project, CommandPump seeks to improve it by supporting other message brokers.

## Features

- Asynchronous processing of a variable number of messages
- Injectable service broker support

## Supported Message Brokers
- MSMQ
- Windows Service Bus
