using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
  public class MessagesController : BaseApiController
  {
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;

    public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
    {
      _messageRepository = messageRepository;
      _userRepository = userRepository;
      _mapper = mapper;
    }

    [HttpPost()]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
      var username = User.GetUsername(); //current logged in 
      //constraint
      if (username == createMessageDto.RecipientUsername.ToLower())
      {
        return BadRequest("You cannot send messages to yourself");
      }
      //gather vars
      var sender = await _userRepository.GetUserByUsernameAsync(username);
      var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
      if (recipient == null) return NotFound(); //error
      //db object
      var message = new Message()
      {
        Sender = sender,
        Recipient = recipient,
        SenderUsername = sender.UserName,
        RecipientUsername = recipient.UserName,
        Content = createMessageDto.Content
      };
      _messageRepository.AddMessage(message); //insert on db
      //success
      if (await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));
      return BadRequest("Failed to send message"); //error
    }

    [HttpGet()]
    public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
      messageParams.Username = User.GetUsername();

      var messages = await _messageRepository.GetMessageForUser(messageParams);

      Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));

      return messages;
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
      var currentUserName = User.GetUsername();

      return Ok(await _messageRepository.GetMessageThread(currentUserName, username));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
      //local vars
      var username = User.GetUsername();
      var message = await _messageRepository.GetMessage(id);
      //at least sender or recipient is triggering the deletion
      if (message.SenderUsername != username && message.RecipientUsername != username) return Unauthorized();
      //check which is deleting, one party deletion preserves data in db
      if(message.SenderUsername == username) message.SenderDeleted = true;
      if(message.RecipientUsername == username) message.RecipientDeleted = true; 
      //both deleted in their thread, deleted in db
      if(message.SenderDeleted && message.RecipientDeleted)
      {
        _messageRepository.DeleteMessage(message);
      }
      //update db
      if(await _messageRepository.SaveAllAsync()) return Ok();
      //error
      return BadRequest("Problem deleting the message");
    }
  }

}