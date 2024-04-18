using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
  [Authorize]
  public class MessageHub : Hub
  {
    private readonly IMapper _mapper;
    private readonly IHubContext<PresenceHub> _presenceHub;
    private readonly IUnitOfWork _uow;
    public MessageHub(IUnitOfWork uow, IMapper mapper, IHubContext<PresenceHub> presenceHub)
    {
      _uow = uow;
      _presenceHub = presenceHub;
      _mapper = mapper;
    }

    public override async Task OnConnectedAsync()
    {
      var httpContext = Context.GetHttpContext();
      var otherUser = httpContext.Request.Query["user"];
      var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
      await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
      var group = await AddToGroup(groupName); //save to DB for tracking

      await Clients.Group(groupName).SendAsync("UpdatedGroup", group); //update upon calling

      var messages = await _uow.MessageRepository
        .GetMessageThread(Context.User.GetUsername(), otherUser);

      if(_uow.HasChanges()) await _uow.Complete(); //save to db if there are changes

      //Send through SignalR
      await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
      var group = await RemoveFromMessageGroup(); //Remove tracking of entity from db
      await Clients.Group(group.Name).SendAsync("UpdatedGroup"); //only one user left
      await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
      var username = Context.User.GetUsername(); //current logged in 
      //constraint
      if (username == createMessageDto.RecipientUsername.ToLower())
      {
        throw new HubException("You cannot send messages to yourself");
      }
      //gather vars
      var sender = await _uow.UserRepository.GetUserByUsernameAsync(username);
      var recipient = await _uow.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

      if (recipient == null) throw new HubException("Not found user"); //error

      //db object
      var message = new Message()
      {
        Sender = sender,
        Recipient = recipient,
        SenderUsername = sender.UserName,
        RecipientUsername = recipient.UserName,
        Content = createMessageDto.Content,
      };
      _uow.MessageRepository.AddMessage(message); //insert on db

      var groupName = GetGroupName(sender.UserName, recipient.UserName);

      var group = await _uow.MessageRepository.GetMessageGroup(groupName);

      if (group.Connections.Any(x => x.Username == recipient.UserName))
      {
        message.DateRead = DateTime.UtcNow; //update read/unread feature 
      }
      else //message notification
      {
        var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
        if (connections != null)
        {
          //SignalR API response
          await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", new { username = sender.UserName, knownAs = sender.KnownAs });
        }
      }

      _uow.MessageRepository.AddMessage(message);

      if (await _uow.Complete())
      {
        //Send through SignalR
        await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));

      }
    }

    //helper method
    private string GetGroupName(string caller, string other)
    {
      var stringCompare = string.CompareOrdinal(caller, other) < 0;
      return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

    private async Task<Group> AddToGroup(string groupName)
    {
      var group = await _uow.MessageRepository.GetMessageGroup(groupName);
      var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

      if (group == null)
      {
        group = new Group(groupName);
        _uow.MessageRepository.AddGroup(group);
      }

      group.Connections.Add(connection); //save hub connection id for tracking
      if (await _uow.Complete()) return group;

      throw new HubException("Failed to add to group");
    }

    private async Task<Group> RemoveFromMessageGroup()
    {
      var group = await _uow.MessageRepository.GetGroupForConnection(Context.ConnectionId);
      var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
      _uow.MessageRepository.RemoveConnection(connection); //remove connection from tracking

      if (await _uow.Complete()) return group;

      throw new HubException("Failed to remove the group");
    }
  }
}