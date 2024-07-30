using AutoMapper;
using Hangfire;
using MediatR;
using Para.Base.Response;
using Para.Bussiness.Cqrs;
using Para.Bussiness.Job;
using Para.Bussiness.Notification;
using Para.Bussiness.RabbitMq;
using Para.Data.Domain;
using Para.Data.UnitOfWork;
using Para.Schema;

namespace Para.Bussiness.Command
{
    public class AccountCommandHandler :
        IRequestHandler<CreateAccountCommand, ApiResponse<AccountResponse>>,
        IRequestHandler<UpdateAccountCommand, ApiResponse>,
        IRequestHandler<DeleteAccountCommand, ApiResponse>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly INotificationService notificationService;
        private readonly IRabbitMQService rabbitMQService;

        public AccountCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, IRabbitMQService rabbitMQService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.notificationService = notificationService;
            this.rabbitMQService = rabbitMQService;
        }

        public async Task<ApiResponse<AccountResponse>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            var mapped = mapper.Map<AccountRequest, Account>(request.Request);
            mapped.OpenDate = DateTime.Now;
            mapped.Balance = 0;
            mapped.AccountNumber = new Random().Next(1000000, 9999999);
            mapped.IBAN = $"TR{mapped.AccountNumber}97925786{mapped.AccountNumber}01";
        
            var saved = await unitOfWork.AccountRepository.Insert(mapped);
            await unitOfWork.Complete();

            var customer = await unitOfWork.CustomerRepository.GetById(request.Request.CustomerId);
            if (customer == null)
            {
                throw new Exception("Customer retrieval failed");
            }

            BackgroundJob.Schedule(() => 
                    SendEmail(customer.Email, $"{customer.FirstName} {customer.LastName}", request.Request.CurrencyCode),
                    TimeSpan.FromSeconds(30));

            var message = new EmailMessage
            {
                Email = customer.Email,
                Subject = "Welcome!",
                Body = "Thank you for registering."
            };

            await rabbitMQService.PublishToQueue(message, "emailQueue");

            var response = mapper.Map<AccountResponse>(saved);
            return new ApiResponse<AccountResponse>(response);
            
        }


        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new []{ 10, 15, 18 }, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public void SendEmail(string email, string name, string currencyCode)
        {
            notificationService.SendEmail(email, "Hesap Acilisi", $"Merhaba, {name}, Adiniza {currencyCode} doviz cinsi ile hesabiniz acilmistir.").Wait();
        }

        public async Task<ApiResponse> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
        {
            var mapped = mapper.Map<AccountRequest, Account>(request.Request);
            mapped.Id = request.AccountId;
            unitOfWork.AccountRepository.Update(mapped);
            await unitOfWork.Complete();
            return new ApiResponse();
        }

        public async Task<ApiResponse> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            await unitOfWork.AccountRepository.Delete(request.AccountId);
            await unitOfWork.Complete();
            return new ApiResponse();
        }
    }
}
