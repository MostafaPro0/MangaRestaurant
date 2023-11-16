using MangaRestaurant.APIs.Dtos;
using MangaRestaurant.APIs.Errors;
using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.RepositoriesContract;
using MangaRestaurant.Core.Specifications;
using MangaRestaurant.Core.Specifications.EmployeeSpecs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MangaRestaurant.APIs.Controllers
{
    public class EmployeeController : BaseApiController
    {
        private readonly IGenericRepository<Employee> _employeeRepor;

        public EmployeeController(IGenericRepository<Employee> employeeRepor) {
            _employeeRepor = employeeRepor;
        }

        //GET : api/Employees
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Employee>>> GetAllEmployees()
        {
            var spec = new EmployeeWithDepartmentSpecs();
            var employees = await _employeeRepor.GetAllAsyncWithSpecAsync(spec);

            return Ok(employees);
        }

        [ProducesResponseType(typeof(ProductToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        // GET : api/Employees/1
        [HttpGet("{Id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var spec = new EmployeeWithDepartmentSpecs(id);
            var employee = await _employeeRepor.GetEntityWithSpecAsync(spec);

            if(employee is null)
            return NotFound(new ApiResponse(404, "Employee Not Found"));

            return Ok(employee);
        }
    }
}
