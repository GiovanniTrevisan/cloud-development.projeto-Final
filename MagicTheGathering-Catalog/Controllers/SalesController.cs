using Azure.Messaging.ServiceBus;
using MagicTheGathering_Catalog.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repository.Exceptions;
using Repository.Interfaces;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MagicTheGathering_Catalog.Controllers
{
    public class SalesController : Controller
    {
        private readonly ISalesRecordRepository _salesRepository;
        private readonly ISellerRepository _sellerRepository;
        private readonly IConfiguration _configuration;

        public SalesController(ISalesRecordRepository salesRepository, ISellerRepository sellerRepository, IConfiguration configuration)
        {
            _sellerRepository = sellerRepository;
            _salesRepository = salesRepository;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? page, DateTime? minDate, DateTime? maxDate)
        {
            if (!minDate.HasValue)
                minDate = DateTime.Parse("01/01/1960");

            if (!maxDate.HasValue)
                maxDate = DateTime.Now;

            List<SalesRecord> sales = await _salesRepository.GetAllPaginatedByDateAsync(minDate, maxDate, page ?? 1);

            ViewBag.ParamName = "page";
            ViewBag.ItemsPerPage = _configuration.GetValue<int>("Pagination:ItemsPerPage");
            ViewBag.MaxLinksPerPage = _configuration.GetValue<int>("Pagination:MaxLinksPerPage");
            ViewBag.TotalItems = await _salesRepository.TotalItemsByDateAsync(minDate, maxDate);
            return View(sales);
        }

        public async Task<IActionResult> Create()
        {
            SalesRecordFormViewModel viewModel = new SalesRecordFormViewModel { Sellers = await _sellerRepository.FindAllAsync() };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SalesRecordFormViewModel salesRecordForm)
        {

            var sale = salesRecordForm.SalesRecord;
            sale.Seller = await _sellerRepository.FindByIdAsync(sale.Seller.Id);

            try
            {
                await _salesRepository.InsertAsync(sale);

                await SendSaleEmail(sale);

                return RedirectToAction(nameof(Index));
            }
            catch (IntegrityException e)
            {
                SalesRecordFormViewModel viewModel = new SalesRecordFormViewModel { Sellers = await _sellerRepository.FindAllAsync(), SalesRecord = sale };
                return View(viewModel);
            }


        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(Error), new { message = "Id not provided" });

            SalesRecord sale = await _salesRepository.FindByIdAsync(id.Value);
            if (sale == null)
                return RedirectToAction(nameof(Error), new { message = "Id not founded" });

            return View(sale);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _salesRepository.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (IntegrityException e)
            {
                return RedirectToAction(nameof(Error), new { message = e.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, SalesRecord sale)
        {
            if (id != sale.Id)
                return RedirectToAction(nameof(Error), new { message = "Id missmatch" });

            try
            {
                await _salesRepository.UpdateAsync(sale);
                return RedirectToAction(nameof(Index));
            }
            catch (ApplicationException e)
            {
                return RedirectToAction(nameof(Index), new { message = e.Message });
            }
        }

        public IActionResult Error(string message)
        {
            ErrorViewModel viewModel = new ErrorViewModel() { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Message = message };
            return View(viewModel);
        }

        private async Task<bool> SendSaleEmail(SalesRecord salesRecord)
        {

            try
            {

                await using var client = new ServiceBusClient(_configuration.GetConnectionString("ServiceBus"));
                ServiceBusSender sender = client.CreateSender(_configuration["QueueAzureFunc"]);

                var serializedSale = JsonSerializer.Serialize(salesRecord, new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.Preserve });

                ServiceBusMessage message = new ServiceBusMessage(serializedSale);

                await sender.SendMessageAsync(message);

                return true;

            }
            catch
            {
                return false;
            }
        }
    }
}
