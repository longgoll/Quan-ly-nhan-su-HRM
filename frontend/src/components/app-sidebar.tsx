"use client"

import * as React from "react"
import {
  Users,
  UserCheck,
  Calendar,
  FileText,
  Award,
  Briefcase,
  Clock,
  Settings2,
  Home,
  Building2,
} from "lucide-react"

import { NavMain } from "@/components/nav-main"
import { NavProjects } from "@/components/nav-projects"
import { NavUser } from "@/components/nav-user"
import { TeamSwitcher } from "@/components/team-switcher"
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
  SidebarRail,
} from "@/components/ui/sidebar"

// This is sample data.
const data = {
  teams: [
    {
      name: "HRM System",
      logo: Building2,
      plan: "Enterprise",
    },
  ],
  navMain: [
    {
      title: "Dashboard",
      url: "/dashboard",
      icon: Home,
      isActive: true,
    },
    {
      title: "Quản lý nhân viên",
      url: "/employees",
      icon: Users,
      items: [
        {
          title: "Danh sách nhân viên",
          url: "/employees",
        },
        {
          title: "Thêm nhân viên",
          url: "/employees/create",
        },
        {
          title: "Phòng ban",
          url: "/departments",
        },
        {
          title: "Chức vụ",
          url: "/positions",
        },
      ],
    },
    {
      title: "Chấm công",
      url: "/attendance",
      icon: UserCheck,
      items: [
        {
          title: "Chấm công hôm nay",
          url: "/attendance/today",
        },
        {
          title: "Lịch sử chấm công",
          url: "/attendance/history",
        },
        {
          title: "Lịch làm việc",
          url: "/work-schedule",
        },
      ],
    },
    {
      title: "Nghỉ phép",
      url: "/leave",
      icon: Calendar,
      items: [
        {
          title: "Đơn nghỉ phép",
          url: "/leave/requests",
        },
        {
          title: "Duyệt nghỉ phép",
          url: "/leave/approvals",
        },
        {
          title: "Lịch nghỉ phép",
          url: "/leave/calendar",
        },
      ],
    },
    {
      title: "Hợp đồng & Tài liệu",
      url: "/documents",
      icon: FileText,
      items: [
        {
          title: "Hợp đồng lao động",
          url: "/contracts",
        },
        {
          title: "Tài liệu nhân sự",
          url: "/documents",
        },
        {
          title: "Lịch sử công tác",
          url: "/work-history",
        },
      ],
    },
    {
      title: "Khen thưởng & Kỷ luật",
      url: "/rewards-disciplines",
      icon: Award,
      items: [
        {
          title: "Khen thưởng",
          url: "/rewards",
        },
        {
          title: "Kỷ luật",
          url: "/disciplines",
        },
      ],
    },
    {
      title: "Cài đặt",
      url: "/settings",
      icon: Settings2,
      items: [
        {
          title: "Cài đặt chung",
          url: "/settings/general",
        },
        {
          title: "Người dùng",
          url: "/settings/users",
        },
        {
          title: "Bảo mật",
          url: "/settings/security",
        },
      ],
    },
  ],
  projects: [
    {
      name: "Báo cáo tháng",
      url: "/reports/monthly",
      icon: Clock,
    },
    {
      name: "Báo cáo năm",
      url: "/reports/yearly",
      icon: Briefcase,
    },
  ],
}

export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
  return (
    <Sidebar collapsible="icon" {...props}>
      <SidebarHeader>
        <TeamSwitcher teams={data.teams} />
      </SidebarHeader>
      <SidebarContent>
        <NavMain items={data.navMain} />
        <NavProjects projects={data.projects} />
      </SidebarContent>
      <SidebarFooter>
        <NavUser />
      </SidebarFooter>
      <SidebarRail />
    </Sidebar>
  )
}
