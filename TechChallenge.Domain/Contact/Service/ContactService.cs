﻿using Microsoft.Extensions.Configuration;
using TechChallange.Common.MessagingService;
using TechChallange.Contact.Domain.Contact.Messaging;
using TechChallange.Contact.Domain.Region.Exception;
using TechChallange.Contact.Integration.Region;
using TechChallange.Contact.Integration.Region.Dto;
using TechChallange.Contact.Integration.Response;
using TechChallange.Contact.Integration.Service;
using TechChallange.Domain.Cache;
using TechChallange.Domain.Contact.Entity;
using TechChallange.Domain.Contact.Exception;
using TechChallange.Domain.Contact.Repository;

namespace TechChallange.Domain.Contact.Service
{
    public class ContactService : IContactService
    {
        private readonly IContactRepository _contactRepository;
        private readonly ICacheRepository _cacheRepository;
        private readonly IIntegrationService _integrationService;
        private readonly IRegionIntegration _regionIntegration;
        private readonly IMessagingService _messagingService;
        private readonly IConfiguration _configuration;


        public ContactService(IContactRepository contactRepository,
                              ICacheRepository cacheRepository,
                              IIntegrationService integrationService,
                              IRegionIntegration regionIntegration,
                              IMessagingService messagingService,
                              IConfiguration configuration)
        {
            _contactRepository = contactRepository;
            _cacheRepository = cacheRepository;
            _integrationService = integrationService;
            _regionIntegration = regionIntegration;
            _messagingService = messagingService;
            _configuration = configuration;

        }

        public async Task CreateAsync(ContactEntity contactEntity)
        {
            var region = await GetRegionById(contactEntity.RegionId).ConfigureAwait(false);

            if (region == null)
                throw new RegionNotFoundException();

            var queueName = _configuration["MassTransit:QueueCreateContact"] ?? string.Empty;

            var wasMessageSent = await _messagingService.SendMessage(queueName, new ContactCreateMessageDto { Email = contactEntity.Email, Name = contactEntity.Name, Phone = contactEntity.Phone, RegionId = contactEntity.RegionId, Id = contactEntity.Id }).ConfigureAwait(false);

        }

        public async Task<IEnumerable<ContactEntity>> GetAllPagedAsync(int pageSize, int page)
        {
            return await _contactRepository.GetAllPagedAsync(c => !c.IsDeleted, pageSize, page, c => c.Name).ConfigureAwait(false);
        }

        public async Task<IEnumerable<ContactEntity>> GetByDddAsync(string ddd)
        {
            Func<Task<IntegrationBaseResponseDto<RegionGetDto>>> callApi = () => { return _regionIntegration.GetByDDD(ddd); };
            var regionResponse = await _integrationService.SendResilientRequest(callApi);

            if (regionResponse == null)
                throw new RegionNotFoundException();

            if (!regionResponse?.Success ?? false)
                throw new RegionNotFoundException();

            var region = regionResponse.Data;

            return await _cacheRepository.GetAsync(ddd, async () => await _contactRepository.GetByDddAsync(region.Id).ConfigureAwait(false));
        }

        public async Task<ContactEntity> GetByIdAsync(Guid id)
        {
            var contactDb = await _cacheRepository.GetAsync(id.ToString(), async () => await _contactRepository.GetByIdAsync(id).ConfigureAwait(false));

            if (contactDb == null)
                throw new ContactNotFoundException();

            return contactDb;
        }

        public async Task<int> GetCountAsync()
        {
            return await _contactRepository.GetCountAsync(c => !c.IsDeleted);
        }

        public async Task RemoveByIdAsync(Guid id)
        {
            var contactDb = await _contactRepository.GetByIdAsync(id).ConfigureAwait(false);

            if (contactDb == null)
                throw new ContactNotFoundException();

            contactDb.MarkAsDeleted();

            await _contactRepository.UpdateAsync(contactDb).ConfigureAwait(false);
        }

        public async Task UpdateAsync(ContactEntity contact)
        {
            var contactDb = await _contactRepository.GetByIdAsync(contact.Id).ConfigureAwait(false);

            if (contactDb == null)
                throw new ContactNotFoundException();

            var region = await GetRegionById(contact.RegionId).ConfigureAwait(false);

            if (region == null)
                throw new RegionNotFoundException();

            contactDb.Name = contact.Name;
            contactDb.Phone = contact.Phone;
            contactDb.Email = contact.Email;
            contactDb.RegionId = contact.RegionId;

            await _contactRepository.UpdateAsync(contact).ConfigureAwait(false);
        }

        #region Private Methods

        private async Task<RegionGetDto> GetRegionById(Guid regionId)
        {
            Func<Task<IntegrationBaseResponseDto<RegionGetDto>>> callApi = () => { return _regionIntegration.GetById(regionId); };
            var regionResponse = await _integrationService.SendResilientRequest(callApi);

            if (regionResponse?.Success ?? false)
                return regionResponse.Data;
            else
                throw new RegionNotFoundException();
        }
        #endregion
    }
}
