using EmployeeManagementApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Runtime.CompilerServices;

namespace EmployeeManagementApp.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly string _connectionString;

        // ASP.NET Core automatically injects IConfiguration here
        public EmployeeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("EmployeeDb");
        }

        public IActionResult Index()
        {
            //Exercise 2 - Razor
            ViewBag.PageTitle = "Viewbag Page Title";
            ViewBag.CurrentDate = DateTime.Now;

            //Create an Employee object and return it to a View
            //Employee obj = new Employee();
            //obj.EmployeeId = 1;
            //obj.FirstName = "Test";
            //obj.LastName = "Doe";
            //obj.Email = "Test@mail.com";
            //obj.Department = "IT";
            //return View(obj);

            //---------------------------------------------------------

            //Exercise 4 - Lists: Create employees list and return it to a View
            //List<Employee> employees = new List<Employee>()
            //{
            //    new Employee()
            //    {
            //        EmployeeId = 1,
            //        FirstName = "Test",
            //        LastName = "Doe",
            //        Email = "Test@mail.com",
            //        Department = "IT"
            //    },
            //    new Employee()
            //    {
            //        EmployeeId = 2,
            //        FirstName = "John",
            //        LastName = "Doe",
            //        Email = "John@mail.com",
            //        Department = "HR"
            //    },
            //    new Employee()
            //    {
            //        EmployeeId = 3,
            //        FirstName = "Jane",
            //        LastName = "Doe",
            //        Email = "Jane@mail.com",
            //        Department = "Finance"
            //    }
            //};
            //return View(employees);

            //---------------------------------------------------------

            List<Employee> employees = new List<Employee>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"SELECT EmployeeId, FirstName, LastName, Email, Department FROM Employees";

                using (SqlCommand command = new SqlCommand(sql, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        employees.Add(new Employee
                        {
                            EmployeeId = Convert.ToInt32(reader["EmployeeId"]),
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            Email = reader["Email"].ToString(),
                            Department = reader["Department"].ToString()
                        });
                    }
                }
            }

            return View(employees);
        }

        [HttpGet]
        public IActionResult SearchView()
        {
            List<string> departments = new List<string>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Load distinct departments for the dropdown
                string deptSql = "SELECT DISTINCT Department FROM Employees WHERE Department IS NOT NULL ORDER BY Department";
                using (SqlCommand deptCommand = new SqlCommand(deptSql, connection))
                using (SqlDataReader deptReader = deptCommand.ExecuteReader())
                {
                    while (deptReader.Read())
                    {
                        departments.Add(deptReader["Department"].ToString());
                    }
                }
            }

            ViewBag.Departments = new SelectList(departments);

            return View();
        }

        [HttpGet]
        public JsonResult Search(string term, string? department)
        {
            List<Employee> employees = new List<Employee>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"SELECT EmployeeId, FirstName, LastName, Email, Department 
                               FROM Employees 
                               WHERE FirstName LIKE @Term OR LastName LIKE @Term AND Department = @Department";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Term", "%" + term + "%");
                    command.Parameters.AddWithValue("@Department", "%" + department + "%");

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(new Employee
                            {
                                EmployeeId = Convert.ToInt32(reader["EmployeeId"]),
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Email = reader["Email"].ToString(),
                                Department = reader["Department"].ToString()
                            });
                        }
                    }
                }

            }

            return Json(employees);
            //return Json(new
            //{
            //    page = 1,
            //    total = 1,
            //    records = employees.Count,
            //    rows = employees
            //});
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("FirstName,LastName,Email,Department")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = @"INSERT INTO Employees (FirstName, LastName, Email, Department) 
                                   VALUES (@FirstName, @LastName, @Email, @Department)";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                        command.Parameters.AddWithValue("@LastName", employee.LastName);
                        command.Parameters.AddWithValue("@Email", employee.Email);
                        command.Parameters.AddWithValue("@Department", employee.Department);
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            return View(employee);
        }

        [HttpGet]
        public IActionResult Edit (int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Employee employee = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"SELECT EmployeeId, FirstName, LastName, Email, Department 
                               FROM Employees 
                               WHERE EmployeeId = @id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@id", SqlDbType.Int).Value = id.Value;
                    
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            employee = new Employee()
                            {
                                EmployeeId = Convert.ToInt32(reader["EmployeeId"]),
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Email = reader["Email"].ToString(),
                                Department = reader["Department"].ToString()
                            };
                        } 
                    }
                }
            }

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int? id, [Bind("FirstName,LastName,Email,Department")] Employee employee)
        {
            if (id == null)
            {
                return NotFound();
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"UPDATE Employees SET FirstName = @FirstName, LastName = @LastName, Email = @Email, Department = @Department
                               WHERE EmployeeId = @id";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@id", SqlDbType.Int).Value = id.Value;
                    command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                    command.Parameters.AddWithValue("@LastName", employee.LastName);
                    command.Parameters.AddWithValue("@Email", employee.Email);
                    command.Parameters.AddWithValue("@Department", employee.Department);
                    command.ExecuteNonQueryAsync();
                }
            }
            if (employee == null)
            {
                return NotFound();
            }

            //return View(employee);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"DELETE FROM Employees WHERE EmployeeId = @id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@id", SqlDbType.Int).Value = id.Value;
                    await command.ExecuteNonQueryAsync();
                }
            }

            return Ok();
        }


    }
}
