using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.Services;
using backend.DTOs;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkScheduleController : ControllerBase
    {
        private readonly IWorkScheduleService _workScheduleService;

        public WorkScheduleController(IWorkScheduleService workScheduleService)
        {
            _workScheduleService = workScheduleService;
        }

        // Work Shift Management
        [HttpGet("shifts")]
        public async Task<ActionResult<IEnumerable<WorkShiftDto>>> GetWorkShifts()
        {
            var shifts = await _workScheduleService.GetAllWorkShiftsAsync();
            return Ok(shifts);
        }

        [HttpGet("shifts/{id}")]
        public async Task<ActionResult<WorkShiftDto>> GetWorkShift(int id)
        {
            var shift = await _workScheduleService.GetWorkShiftByIdAsync(id);
            if (shift == null)
                return NotFound();

            return Ok(shift);
        }

        [HttpPost("shifts")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<ActionResult<WorkShiftDto>> CreateWorkShift([FromBody] CreateWorkShiftDto createDto)
        {
            try
            {
                var shift = await _workScheduleService.CreateWorkShiftAsync(createDto);
                return CreatedAtAction(nameof(GetWorkShift), new { id = shift.Id }, shift);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("shifts/{id}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<ActionResult<WorkShiftDto>> UpdateWorkShift(int id, [FromBody] CreateWorkShiftDto updateDto)
        {
            try
            {
                var shift = await _workScheduleService.UpdateWorkShiftAsync(id, updateDto);
                if (shift == null)
                    return NotFound();

                return Ok(shift);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("shifts/{id}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> DeleteWorkShift(int id)
        {
            var result = await _workScheduleService.DeleteWorkShiftAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        // Employee Shift Assignment
        [HttpGet("shift-assignments")]
        public async Task<ActionResult<IEnumerable<EmployeeShiftAssignmentDto>>> GetShiftAssignments([FromQuery] int? employeeId)
        {
            var assignments = await _workScheduleService.GetEmployeeShiftAssignmentsAsync(employeeId);
            return Ok(assignments);
        }

        [HttpGet("shift-assignments/{id}")]
        public async Task<ActionResult<EmployeeShiftAssignmentDto>> GetShiftAssignment(int id)
        {
            var assignment = await _workScheduleService.GetEmployeeShiftAssignmentByIdAsync(id);
            if (assignment == null)
                return NotFound();

            return Ok(assignment);
        }

        [HttpPost("shift-assignments")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<ActionResult<EmployeeShiftAssignmentDto>> CreateShiftAssignment([FromBody] CreateEmployeeShiftAssignmentDto createDto)
        {
            try
            {
                var assignment = await _workScheduleService.CreateEmployeeShiftAssignmentAsync(createDto);
                return CreatedAtAction(nameof(GetShiftAssignment), new { id = assignment.Id }, assignment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("shift-assignments/{id}")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<ActionResult<EmployeeShiftAssignmentDto>> UpdateShiftAssignment(int id, [FromBody] CreateEmployeeShiftAssignmentDto updateDto)
        {
            try
            {
                var assignment = await _workScheduleService.UpdateEmployeeShiftAssignmentAsync(id, updateDto);
                if (assignment == null)
                    return NotFound();

                return Ok(assignment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("shift-assignments/{id}")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<IActionResult> DeleteShiftAssignment(int id)
        {
            var result = await _workScheduleService.DeleteEmployeeShiftAssignmentAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        // Work Schedule Management
        [HttpGet("schedules")]
        public async Task<ActionResult<IEnumerable<WorkScheduleDto>>> GetWorkSchedules(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? employeeId)
        {
            var schedules = await _workScheduleService.GetWorkSchedulesAsync(startDate, endDate, employeeId);
            return Ok(schedules);
        }

        [HttpGet("schedules/{id}")]
        public async Task<ActionResult<WorkScheduleDto>> GetWorkSchedule(int id)
        {
            var schedule = await _workScheduleService.GetWorkScheduleByIdAsync(id);
            if (schedule == null)
                return NotFound();

            return Ok(schedule);
        }

        [HttpPost("schedules")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<ActionResult<WorkScheduleDto>> CreateWorkSchedule([FromBody] CreateWorkScheduleDto createDto)
        {
            try
            {
                var schedule = await _workScheduleService.CreateWorkScheduleAsync(createDto);
                return CreatedAtAction(nameof(GetWorkSchedule), new { id = schedule.Id }, schedule);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("schedules/bulk")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<ActionResult<IEnumerable<WorkScheduleDto>>> CreateBulkWorkSchedule([FromBody] BulkScheduleCreateDto bulkCreateDto)
        {
            try
            {
                var schedules = await _workScheduleService.CreateBulkWorkScheduleAsync(bulkCreateDto);
                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("schedules/{id}")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<ActionResult<WorkScheduleDto>> UpdateWorkSchedule(int id, [FromBody] CreateWorkScheduleDto updateDto)
        {
            try
            {
                var schedule = await _workScheduleService.UpdateWorkScheduleAsync(id, updateDto);
                if (schedule == null)
                    return NotFound();

                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("schedules/{id}")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<IActionResult> DeleteWorkSchedule(int id)
        {
            var result = await _workScheduleService.DeleteWorkScheduleAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        // Helper Endpoints
        [HttpGet("employees/{employeeId}/schedule")]
        public async Task<ActionResult<IEnumerable<WorkScheduleDto>>> GetEmployeeSchedule(
            int employeeId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var schedule = await _workScheduleService.GetEmployeeScheduleAsync(employeeId, startDate, endDate);
            return Ok(schedule);
        }

        [HttpGet("employees/{employeeId}/applicable-shifts")]
        public async Task<ActionResult<IEnumerable<WorkShiftDto>>> GetApplicableShiftsForEmployee(int employeeId)
        {
            var shifts = await _workScheduleService.GetApplicableShiftsForEmployeeAsync(employeeId);
            return Ok(shifts);
        }

        [HttpGet("check-conflict")]
        public async Task<ActionResult<bool>> CheckScheduleConflict(
            [FromQuery] int employeeId,
            [FromQuery] DateTime workDate,
            [FromQuery] int? excludeScheduleId)
        {
            var hasConflict = await _workScheduleService.HasScheduleConflictAsync(employeeId, workDate, excludeScheduleId);
            return Ok(hasConflict);
        }
    }
}
