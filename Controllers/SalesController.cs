using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using SalesService;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SalesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly SalesDbContext _context;

        public SalesController(SalesDbContext context)
        {
            _context = context;
        }

        [HttpPost("add-sale")]
        [SwaggerOperation(
        Summary = "Добавление или обновление продажи",
        Description = "Этот метод добавляет новую продажу, если она найдена по ID.")]
        public async Task<IActionResult> AddSale([FromBody] Sale sale)
        {
            var existingSale = await _context.Sales
                .FirstOrDefaultAsync(s => s.Id == sale.Id);

            if (existingSale != null)
            {
                if (!string.Equals(existingSale.ProductName, sale.ProductName, StringComparison.OrdinalIgnoreCase))
                {
                    return Conflict(new
                    {
                        message = "Conflict: Product with the same ID but different name already exists.",
                        existingSale,
                        conflictingSale = sale
                    });
                }

                existingSale.Quantity += sale.Quantity;
                existingSale.Price = sale.Price;
                existingSale.Date = sale.Date;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Sale updated successfully", updatedSale = existingSale });
            }

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Sale added successfully", sale });
        }

        [HttpPost("update-sale")]
        [SwaggerOperation(
        Summary = "Обновление продажи",
        Description = "Этот метод обновляет продажу, если она найдена по ID.")]
        public async Task<IActionResult> UpdateSale([FromBody] Sale updatedSale)
        {
            // Проверяем, существует ли запись с таким Id
            var sale = await _context.Sales
                .FirstOrDefaultAsync(s => s.Id == updatedSale.Id);

            if (sale == null)
            {
                // Если записи нет, возвращаем ошибку 404
                return NotFound(new { message = $"Sale with ID {updatedSale.Id} not found" });
            }

            // Обновляем данные только если запись найдена
            sale.ProductName = updatedSale.ProductName;
            sale.ProductCategory = updatedSale.ProductCategory;
            sale.Quantity = updatedSale.Quantity;
            sale.Price = updatedSale.Price;
            sale.Date = updatedSale.Date;

            // Сохраняем изменения в базе данных
            await _context.SaveChangesAsync();

            // Возвращаем успешный ответ с обновленными данными
            return Ok(new { message = "Sale updated successfully", updatedSale });
        }


        // GET: Генерация прогноза
        [HttpGet("generate-forecast")]
        [SwaggerOperation(
        Summary = "Генерация прогноза",
        Description = "Этот метод делает анализ за последние N дней и на основе его делает предсказание.")]
        public async Task<IActionResult> GenerateForecast([FromQuery] int daysToAnalyze = 30)
        {
            if (!await _context.Sales.AnyAsync())
                return NotFound("Нет данных о продажах для анализа.");

            var analysisStartDate = DateTime.UtcNow.AddDays(-daysToAnalyze);
            var recentSales = await _context.Sales
                .Where(s => s.Date >= analysisStartDate)
                .ToListAsync();

            if (!recentSales.Any())
                return NotFound($"Нет данных о продажах за последние {daysToAnalyze} дня(ей).");

            var forecasts = recentSales
                .GroupBy(s => s.ProductName)
                .Select(g => new DemandForecast
                {
                    ProductName = g.Key,
                    ForecastDate = DateTime.UtcNow.AddDays(7),
                    PredictedDemand = (int)Math.Ceiling(g.Average(s => s.Quantity))
                })
                .ToList();

            foreach (var forecast in forecasts)
            {
                if (forecast.PredictedDemand > 100)
                {
                    forecast.Recommendation = "Рекомендуется увеличить запасы.";
                }
                else if (forecast.PredictedDemand < 20)
                {
                    forecast.Recommendation = "Рекомендуется снизить запасы.";
                }
                else
                {
                    forecast.Recommendation = "Уровень запасов достаточен.";
                }
                _context.DemandForecasts.Add(forecast);
                await _context.SaveChangesAsync();
            }
            

            return Ok(forecasts);
        }

        // GET: Получение аналитики
        [HttpGet("get-analytics")]
        [SwaggerOperation(
        Summary = "Получение аналитики",
        Description = "Этот метод делает выводит аналитику в указанном диапазоне времени.")]
        public async Task<IActionResult> GetAnalytics([FromQuery] string startDate, [FromQuery] string endDate)
        {
            if (!DateTime.TryParse(startDate, out var parsedStartDate) || !DateTime.TryParse(endDate, out var parsedEndDate))
            {
                return BadRequest("Неверный формат даты.");
            }

            // Преобразуем в UTC
            var startDateUtc = TimeZoneInfo.ConvertTimeToUtc(parsedStartDate);
            var endDateUtc = TimeZoneInfo.ConvertTimeToUtc(parsedEndDate);

            var sales = await _context.Sales
                .Where(s => s.Date >= startDateUtc && s.Date <= endDateUtc)
                .ToListAsync();

            var totalRevenue = sales.Sum(s => s.Price * s.Quantity);
            var averageCheck = sales.Any() ? totalRevenue / sales.Count() : 0;
            var totalSales = sales.Count();

            return Ok(new
            {
                TotalRevenue = totalRevenue,
                AverageCheck = averageCheck,
                TotalSales = totalSales
            });
        }


        // GET: Получение трендов
        [HttpGet("get-trends")]
        [SwaggerOperation(
        Summary = "Получение трендов",
        Description = "Этот метод получает продажи по указанной категории")]
        public async Task<IActionResult> GetTrends([FromQuery] string productCategory)
        {
            var trends = await _context.Sales
                .Where(s => s.ProductCategory == productCategory)
                .GroupBy(s => s.Date.Date)
                .Select(g => new { Date = g.Key, TotalSales = g.Sum(s => s.Quantity) })
                .OrderBy(t => t.Date)
                .ToListAsync();

            return Ok(trends);
        }

        // GET: Получение всех продаж
        [HttpGet("get-all-sales")]
        [SwaggerOperation(
        Summary = "Получение всех продаж",
        Description = "Этот метод выводит список все существующие продажи.")]
        public async Task<IActionResult> GetAllSales()
        {
            var sales = await _context.Sales.ToListAsync();
            return Ok(sales);
        }
    }
}
