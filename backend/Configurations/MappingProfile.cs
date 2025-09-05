using AutoMapper;
using backend.Models;
using backend.DTOs;

namespace backend.Configurations
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Employee mappings
            CreateMap<CreateEmployeeDto, Employee>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Department, opt => opt.Ignore())
                .ForMember(dest => dest.Position, opt => opt.Ignore())
                .ForMember(dest => dest.DirectManager, opt => opt.Ignore())
                .ForMember(dest => dest.Subordinates, opt => opt.Ignore())
                .ForMember(dest => dest.Contracts, opt => opt.Ignore())
                .ForMember(dest => dest.WorkHistories, opt => opt.Ignore())
                .ForMember(dest => dest.Rewards, opt => opt.Ignore())
                .ForMember(dest => dest.Disciplines, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => EmployeeStatus.Active))
                .ForMember(dest => dest.TerminationDate, opt => opt.Ignore());

            CreateMap<UpdateEmployeeDto, Employee>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Department, opt => opt.Ignore())
                .ForMember(dest => dest.Position, opt => opt.Ignore())
                .ForMember(dest => dest.DirectManager, opt => opt.Ignore())
                .ForMember(dest => dest.Subordinates, opt => opt.Ignore())
                .ForMember(dest => dest.Contracts, opt => opt.Ignore())
                .ForMember(dest => dest.WorkHistories, opt => opt.Ignore())
                .ForMember(dest => dest.Rewards, opt => opt.Ignore())
                .ForMember(dest => dest.Disciplines, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore())
                .ForMember(dest => dest.EmployeeCode, opt => opt.Ignore())
                .ForMember(dest => dest.HireDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position))
                .ForMember(dest => dest.DirectManager, opt => opt.MapFrom(src => src.DirectManager));

            // Work Schedule mappings
            CreateMap<CreateWorkShiftDto, WorkShift>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ShiftStatus.Active))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.EmployeeShiftAssignments, opt => opt.Ignore())
                .ForMember(dest => dest.WorkSchedules, opt => opt.Ignore());

            CreateMap<WorkShift, WorkShiftDto>();

            CreateMap<CreateEmployeeShiftAssignmentDto, EmployeeShiftAssignment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Employee, opt => opt.Ignore())
                .ForMember(dest => dest.WorkShift, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<CreateWorkScheduleDto, WorkSchedule>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Employee, opt => opt.Ignore())
                .ForMember(dest => dest.WorkShift, opt => opt.Ignore())
                .ForMember(dest => dest.ActualStartTime, opt => opt.Ignore())
                .ForMember(dest => dest.ActualEndTime, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // Attendance mappings
            CreateMap<AttendanceDetail, AttendanceDetailDto>();

            // Leave Management mappings
            CreateMap<CreateLeavePolicyDto, LeavePolicy>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Department, opt => opt.Ignore())
                .ForMember(dest => dest.Position, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.EmployeeLeaveBalances, opt => opt.Ignore())
                .ForMember(dest => dest.LeaveRequests, opt => opt.Ignore());

            CreateMap<CreateLeaveRequestDto, LeaveRequest>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
                .ForMember(dest => dest.Employee, opt => opt.Ignore())
                .ForMember(dest => dest.LeavePolicy, opt => opt.Ignore())
                .ForMember(dest => dest.CoverEmployee, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedBy, opt => opt.Ignore())
                .ForMember(dest => dest.RequestedDays, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.ManagerComments, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedById, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LeaveApprovalWorkflows, opt => opt.Ignore());

            CreateMap<CreatePublicHolidayDto, PublicHoliday>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Department, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<Employee, EmployeeBasicDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.MiddleName} {src.LastName}".Trim()))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null))
                .ForMember(dest => dest.PositionTitle, opt => opt.MapFrom(src => src.Position != null ? src.Position.Title : null))
                .ForMember(dest => dest.EmployeeCode, opt => opt.MapFrom(src => src.EmployeeCode ?? ""));

            // Department mappings
            CreateMap<CreateDepartmentDto, Department>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ParentDepartment, opt => opt.Ignore())
                .ForMember(dest => dest.Manager, opt => opt.Ignore())
                .ForMember(dest => dest.SubDepartments, opt => opt.Ignore())
                .ForMember(dest => dest.Employees, opt => opt.Ignore())
                .ForMember(dest => dest.Positions, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<UpdateDepartmentDto, Department>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ParentDepartment, opt => opt.Ignore())
                .ForMember(dest => dest.Manager, opt => opt.Ignore())
                .ForMember(dest => dest.SubDepartments, opt => opt.Ignore())
                .ForMember(dest => dest.Employees, opt => opt.Ignore())
                .ForMember(dest => dest.Positions, opt => opt.Ignore());

            CreateMap<Department, DepartmentDto>()
                .ForMember(dest => dest.ParentDepartmentName, opt => opt.MapFrom(src => src.ParentDepartment != null ? src.ParentDepartment.Name : null))
                .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src => src.Manager != null ? $"{src.Manager.FirstName} {src.Manager.LastName}".Trim() : null))
                .ForMember(dest => dest.EmployeeCount, opt => opt.MapFrom(src => src.Employees.Count))
                .ForMember(dest => dest.SubDepartments, opt => opt.MapFrom(src => src.SubDepartments));

            // Position mappings
            CreateMap<CreatePositionDto, Position>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Department, opt => opt.Ignore())
                .ForMember(dest => dest.Employees, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<UpdatePositionDto, Position>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Department, opt => opt.Ignore())
                .ForMember(dest => dest.Employees, opt => opt.Ignore());

            CreateMap<Position, PositionDto>()
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null))
                .ForMember(dest => dest.EmployeeCount, opt => opt.MapFrom(src => src.Employees.Count));

            // Contract mappings
            CreateMap<CreateContractDto, EmployeeContract>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Employee, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ContractStatus.Active))
                .ForMember(dest => dest.DocumentPath, opt => opt.Ignore())
                .ForMember(dest => dest.TerminationReason, opt => opt.Ignore())
                .ForMember(dest => dest.TerminationDate, opt => opt.Ignore());

            CreateMap<UpdateContractDto, EmployeeContract>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Employee, opt => opt.Ignore())
                .ForMember(dest => dest.DocumentPath, opt => opt.Ignore());

            CreateMap<EmployeeContract, ContractDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => $"{src.Employee.FirstName} {src.Employee.LastName}".Trim()))
                .ForMember(dest => dest.EmployeeCode, opt => opt.MapFrom(src => src.Employee.EmployeeCode ?? ""));

            // Work History mappings
            CreateMap<CreateWorkHistoryDto, WorkHistory>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Employee, opt => opt.Ignore())
                .ForMember(dest => dest.OldDepartment, opt => opt.Ignore())
                .ForMember(dest => dest.NewDepartment, opt => opt.Ignore())
                .ForMember(dest => dest.OldPosition, opt => opt.Ignore())
                .ForMember(dest => dest.NewPosition, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedById, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedDate, opt => opt.Ignore());

            CreateMap<WorkHistory, WorkHistoryDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => $"{src.Employee.FirstName} {src.Employee.LastName}".Trim()))
                .ForMember(dest => dest.EmployeeCode, opt => opt.MapFrom(src => src.Employee.EmployeeCode ?? ""))
                .ForMember(dest => dest.OldDepartmentName, opt => opt.MapFrom(src => src.OldDepartment != null ? src.OldDepartment.Name : null))
                .ForMember(dest => dest.NewDepartmentName, opt => opt.MapFrom(src => src.NewDepartment != null ? src.NewDepartment.Name : null))
                .ForMember(dest => dest.OldPositionTitle, opt => opt.MapFrom(src => src.OldPosition != null ? src.OldPosition.Title : null))
                .ForMember(dest => dest.NewPositionTitle, opt => opt.MapFrom(src => src.NewPosition != null ? src.NewPosition.Title : null))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedBy != null ? src.ApprovedBy.FullName : null));

            // Reward mappings
            CreateMap<CreateRewardDto, Reward>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Employee, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedById, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedDate, opt => opt.Ignore())
                .ForMember(dest => dest.DocumentPath, opt => opt.Ignore());

            CreateMap<Reward, RewardDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => $"{src.Employee.FirstName} {src.Employee.LastName}".Trim()))
                .ForMember(dest => dest.EmployeeCode, opt => opt.MapFrom(src => src.Employee.EmployeeCode ?? ""))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedBy != null ? src.ApprovedBy.FullName : null));

            // Discipline mappings
            CreateMap<CreateDisciplineDto, Discipline>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Employee, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedById, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedDate, opt => opt.Ignore())
                .ForMember(dest => dest.DocumentPath, opt => opt.Ignore());

            CreateMap<Discipline, DisciplineDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => $"{src.Employee.FirstName} {src.Employee.LastName}".Trim()))
                .ForMember(dest => dest.EmployeeCode, opt => opt.MapFrom(src => src.Employee.EmployeeCode ?? ""))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedBy != null ? src.ApprovedBy.FullName : null));

            // Document mappings
            CreateMap<EmployeeDocument, DocumentDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => $"{src.Employee.FirstName} {src.Employee.LastName}".Trim()))
                .ForMember(dest => dest.EmployeeCode, opt => opt.MapFrom(src => src.Employee.EmployeeCode ?? ""))
                .ForMember(dest => dest.UploadedByName, opt => opt.MapFrom(src => src.UploadedBy != null ? src.UploadedBy.FullName : null));
        }
    }
}
