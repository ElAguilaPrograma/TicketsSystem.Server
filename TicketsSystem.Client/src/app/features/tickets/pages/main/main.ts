import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from "../../../../shared/components/button/button.component";
import { CardComponent } from "../../../../shared/components/card/card.component";
import { Table, TableColumn, TablePagination } from "../../../../shared/components/table/table";
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroExclamationCircle,
  heroEllipsisHorizontal,
  heroCheckCircle,
  heroPlus,
  heroEllipsisVertical
} from '@ng-icons/heroicons/outline';

export interface Ticket {
  id: string;
  subject: string;
  reportedBy: string;
  priority: 'CRITICAL' | 'HIGH' | 'MEDIUM' | 'LOW';
  status: 'Open' | 'In Progress' | 'Resolved';
  createdDate: string;
}

@Component({
  selector: 'app-main',
  imports: [CommonModule, ButtonComponent, CardComponent, NgIcon, Table],
  viewProviders: [
    provideIcons({
      heroExclamationCircle,
      heroEllipsisHorizontal,
      heroCheckCircle,
      heroPlus,
      heroEllipsisVertical
    })
  ],
  templateUrl: './main.html',
  styleUrl: './main.css',
})
export class Main {
  columns: TableColumn[] = [
    { key: 'id', header: 'Ticket ID', width: '120px' },
    { key: 'subject', header: 'Subject' },
    { key: 'priority', header: 'Priority', width: '100px' },
    { key: 'status', header: 'Status', width: '120px' },
    { key: 'createdDate', header: 'Created Date', width: '180px' },
    { key: 'actions', header: 'Actions', width: '80px' }
  ];

  tickets: Ticket[] = [
    {
      id: 'INC-1024',
      subject: 'Server Latency in US-East-1 Region',
      reportedBy: 'Infrastructure Team',
      priority: 'CRITICAL',
      status: 'Open',
      createdDate: 'Oct 24, 2023 · 14:22'
    },
    {
      id: 'INC-1023',
      subject: 'VPN Authentication Failure',
      reportedBy: 'Sarah J. (Accounting)',
      priority: 'HIGH',
      status: 'In Progress',
      createdDate: 'Oct 24, 2023 · 12:45'
    },
    {
      id: 'INC-1022',
      subject: 'Printer Connection Issues - Floor 3',
      reportedBy: 'Facility Support',
      priority: 'LOW',
      status: 'Open',
      createdDate: 'Oct 24, 2023 · 09:12'
    },
    {
      id: 'INC-1021',
      subject: 'SaaS Integration Timeout',
      reportedBy: 'Automatic System Alert',
      priority: 'MEDIUM',
      status: 'Resolved',
      createdDate: 'Oct 23, 2023 · 16:55'
    }
  ];

  pagination: TablePagination = {
    currentPage: 1,
    pageSize: 4,
    totalItems: 196
  };

  onPageChange(page: number): void {
    this.pagination = { ...this.pagination, currentPage: page };
  }

  getPriorityClass(priority: string): string {
    const classes: Record<string, string> = {
      'CRITICAL': 'bg-red-500/20 text-red-500 border-red-500/30',
      'HIGH': 'bg-orange-500/20 text-orange-500 border-orange-500/30',
      'MEDIUM': 'bg-yellow-500/20 text-yellow-500 border-yellow-500/30',
      'LOW': 'bg-slate-500/20 text-slate-400 border-slate-500/30'
    };
    return classes[priority] || '';
  }

  getStatusClass(status: string): string {
    const classes: Record<string, string> = {
      'Open': 'bg-brand-primary',
      'In Progress': 'bg-amber-500',
      'Resolved': 'bg-green-500'
    };
    return classes[status] || '';
  }
}
