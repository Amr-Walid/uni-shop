using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using FTD.Application.Mappers;
using FTD.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FTD.Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly IAppDbContext _db;
        private readonly IEmailService _email;

        public MessageService(IAppDbContext db, IEmailService email)
        {
            _db = db;
            _email = email;
        }

        public async Task SaveMessageAsync(ContactMessageDto dto)
        {
            var msg = new ContactMessage
            {
                Name = dto.Name?.Trim(),
                Email = dto.Email?.Trim(),
                Phone = dto.Phone?.Trim(),
                Message = dto.Message?.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            _db.ContactMessages.Add(msg);
            await _db.SaveChangesAsync();

            // Send email notification (async, won't block if it fails)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _email.SendContactNotificationAsync(
                        dto.Name ?? "", dto.Email ?? "", dto.Phone ?? "", dto.Message ?? "");
                }
                catch
                {
                    // Ignore background email failure
                }
            });
        }

        public async Task<List<ContactMessageDto>> GetAllMessagesAsync()
        {
            var entities = await _db.ContactMessages
                .AsNoTracking()
                .OrderByDescending(m => m.IsRead)
                .ThenByDescending(m => m.CreatedAt)
                .ToListAsync();

            return entities.Select(m => m.ToDto()).Where(dto => dto != null).Select(dto => dto!).ToList();
        }

        public async Task MarkReadAsync(int id)
        {
            var msg = await _db.ContactMessages.FindAsync(id);
            if (msg != null)
            {
                msg.IsRead = true;
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var msg = await _db.ContactMessages.FindAsync(id);
            if (msg != null)
            {
                _db.ContactMessages.Remove(msg);
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteReadAsync()
        {
            var read = await _db.ContactMessages.Where(m => m.IsRead).ToListAsync();
            if (read.Any())
            {
                _db.ContactMessages.RemoveRange(read);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<int> GetUnreadCountAsync()
        {
            return await _db.ContactMessages.CountAsync(m => !m.IsRead);
        }
    }
}
