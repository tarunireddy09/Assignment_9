using Assignment_9.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Assignment_9.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BloodBankController : ControllerBase
    {
        private static List<BloodBankEntry> BloodBankEntries = new List<BloodBankEntry>();
        private static int CurrentId = 1;

        [HttpPost]
        public IActionResult Create([FromBody] BloodBankEntry entry)
        {
            if (entry == null) return BadRequest("Invalid data.");

            entry.Id = CurrentId++;
            entry.CollectionDate = DateTime.Now; 
            entry.ExpirationDate = entry.CollectionDate.AddMonths(6); 
            BloodBankEntries.Add(entry);

            return CreatedAtAction(nameof(GetById), new { id = entry.Id }, entry);
        }

        [HttpGet]
        public IActionResult GetAll(int page = 1, int size = 10, string sortBy = "Id", string order = "asc")
        {
            var entries = BloodBankEntries.AsQueryable();

           
            entries = sortBy.ToLower() switch
            {
                "bloodtype" => order.ToLower() == "desc" ? entries.OrderByDescending(e => e.BloodType) : entries.OrderBy(e => e.BloodType),
                "collectiondate" => order.ToLower() == "desc" ? entries.OrderByDescending(e => e.CollectionDate) : entries.OrderBy(e => e.CollectionDate),
                _ => order.ToLower() == "desc" ? entries.OrderByDescending(e => e.Id) : entries.OrderBy(e => e.Id),
            };

            var paginatedEntries = entries.Skip((page - 1) * size).Take(size).ToList();
            return Ok(paginatedEntries);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var entry = BloodBankEntries.FirstOrDefault(e => e.Id == id);
            if (entry == null) return NotFound("Entry not found.");
            return Ok(entry);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] BloodBankEntry updatedEntry)
        {
            var existingEntry = BloodBankEntries.FirstOrDefault(e => e.Id == id);
            if (existingEntry == null) return NotFound("Entry not found.");

            existingEntry.DonorName = updatedEntry.DonorName;
            existingEntry.Age = updatedEntry.Age;
            existingEntry.BloodType = updatedEntry.BloodType;
            existingEntry.ContactInfo = updatedEntry.ContactInfo;
            existingEntry.Quantity = updatedEntry.Quantity;
            if (updatedEntry.CollectionDate != default)
            {
                existingEntry.CollectionDate = updatedEntry.CollectionDate;
                
                existingEntry.ExpirationDate = updatedEntry.CollectionDate.AddMonths(6);
            }

            existingEntry.Status = updatedEntry.Status;

            return NoContent();
        }


        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var entry = BloodBankEntries.FirstOrDefault(e => e.Id == id);
            if (entry == null) return NotFound("Entry not found.");

            BloodBankEntries.Remove(entry);
            return NoContent();
        }

        [HttpGet("search")]
        public IActionResult Search(string? bloodType = null, string? status = null, string? donorName = null)
        {
            var results = BloodBankEntries.AsQueryable();

            if (!string.IsNullOrWhiteSpace(bloodType))
                results = results.Where(e => e.BloodType.Equals(bloodType, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(status))
                results = results.Where(e => e.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(donorName))
                results = results.Where(e => e.DonorName.Contains(donorName, StringComparison.OrdinalIgnoreCase));

            return Ok(results.ToList());
        }
    }
}
