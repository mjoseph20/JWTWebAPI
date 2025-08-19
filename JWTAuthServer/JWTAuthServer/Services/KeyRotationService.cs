﻿using JWTAuthServer.Data;
using JWTAuthServer.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace JWTAuthServer.Services
{
    public class KeyRotationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider; // creates a scoped service lifetime
        private readonly TimeSpan _rotationalInterval = TimeSpan.FromDays(7); // sets how frequently keys should be rotated

        public KeyRotationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await RotateKeysAsync();
                await Task.Delay(_rotationalInterval, stoppingToken);
            }
        }

        private async Task RotateKeysAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var activeKey = await context.SigningKeys.FirstOrDefaultAsync(k => k.IsActive);
            if (activeKey == null || activeKey.ExpiredAt <= DateTime.UtcNow.AddDays(10))
            {
                if (activeKey != null)
                { 
                    activeKey.IsActive = false;
                    context.SigningKeys.Update(activeKey);
                }

                using var rsa = RSA.Create(2048);
                var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
                var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                var newKeyId = Guid.NewGuid().ToString();
                var newKey = new SigningKey
                {
                    KeyId = newKeyId,
                    PrivateKey = privateKey,
                    PublicKey = publicKey,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ExpiredAt = DateTime.UtcNow.AddYears(1),
                };

                await context.SigningKeys.AddAsync(newKey);
                await context.SaveChangesAsync();
            }
        }
    }
}
