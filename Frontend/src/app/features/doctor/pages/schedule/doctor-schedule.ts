import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScheduleService } from '../../../../core/services/schedule.service';
import { Schedule } from '../../../../models/schedule.model';

@Component({
  selector: 'app-doctor-schedule',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './doctor-schedule.html',
  styleUrl: './doctor-schedule.scss'
})
export class DoctorScheduleComponent implements OnInit {
  private scheduleService = inject(ScheduleService);

  schedules = signal<Schedule[]>([]);
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);

  orderedSchedules = computed(() => {
    return [...this.schedules()].sort((a, b) => this.getDayOrder(a.weekDay) - this.getDayOrder(b.weekDay));
  });

  ngOnInit(): void {
    this.loadSchedule();
  }

  loadSchedule(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.scheduleService.getMySchedule().subscribe({
      next: data => {
        this.schedules.set(data);
        this.isLoading.set(false);
      },
      error: err => {
        console.error('Помилка завантаження розкладу лікаря', err);
        this.errorMessage.set('Не вдалося завантажити розклад. Спробуйте оновити сторінку.');
        this.isLoading.set(false);
      }
    });
  }

  getDuration(schedule: Schedule): string {
    const start = this.parseTime(schedule.startTime);
    const end = this.parseTime(schedule.endTime);
    const diffMinutes = end - start;

    if (diffMinutes <= 0) {
      return '—';
    }

    const hours = Math.floor(diffMinutes / 60);
    const minutes = diffMinutes % 60;

    if (hours > 0 && minutes > 0) {
      return `${hours} год ${minutes} хв`;
    }

    if (hours > 0) {
      return `${hours} год`;
    }

    return `${minutes} хв`;
  }

  private parseTime(time: string): number {
    const [hours, minutes] = time.split(':').map(Number);
    return hours * 60 + minutes;
  }

  private getDayOrder(day: string): number {
    const normalizedDay = day.toLowerCase();
    const order: Record<string, number> = {
      monday: 1,
      понеділок: 1,
      вівторок: 2,
      tuesday: 2,
      середа: 3,
      wednesday: 3,
      четвер: 4,
      thursday: 4,
      пʼятниця: 5,
      "п'ятниця": 5,
      friday: 5,
      субота: 6,
      saturday: 6,
      неділя: 7,
      sunday: 7
    };

    return order[normalizedDay] ?? 99;
  }
}
