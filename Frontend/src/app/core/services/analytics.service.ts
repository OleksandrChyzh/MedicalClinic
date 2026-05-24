import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface SummaryDto {
  totalRevenue: number;
  totalAppointments: number;
  completedAppointments: number;
  cancelledAppointments: number;
  noShowRate: number;
}

export interface ChartItemDto {
  label: string;
  value: number;
}

export interface DoctorKpiDto {
  doctorName: string;
  completedCount: number;
  totalCount: number;
}

export interface DashboardAnalyticsDto {
  summary: SummaryDto;
  revenueByDirection: ChartItemDto[];
  appointmentStatuses: ChartItemDto[];
  topDirections: ChartItemDto[];
  doctorKpi: DoctorKpiDto[];
  trafficByMonth: ChartItemDto[];
}

@Injectable({ providedIn: 'root' })
export class AnalyticsService {
  private http = inject(HttpClient);
  private readonly base = 'https://localhost:7272/api/analytics';

  getDashboard(startDate?: string, endDate?: string): Observable<DashboardAnalyticsDto> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    return this.http.get<DashboardAnalyticsDto>(`${this.base}/dashboard`, { params });
  }
}
