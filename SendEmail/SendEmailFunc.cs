using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using Models;
using System.Threading.Tasks;
using Repository;
using Repository.Interfaces;
using System.Threading;

namespace SendEmail
{
    public class SendEmailFunc
    {
        
        private readonly ISalesRecordRepository repository;

        public SendEmailFunc(ISalesRecordRepository repository)
        {
            this.repository = repository;
        }
        
        [FunctionName("Function1")]
        public async Task Run([ServiceBusTrigger("Sales", Connection = "SalesConnectionString")] string salesRecord, ILogger log)
        {
            //Thread.Sleep(60000);

            var deserializedSale = JsonSerializer.Deserialize<SalesRecord>(salesRecord, new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.Preserve });

            deserializedSale.Status = Models.Enums.SaleStatus.Billed;

            await repository.UpdateAsync(deserializedSale);

            log.LogInformation($"Sale # {deserializedSale.Id} has been Billed!");
        }
    }
}
