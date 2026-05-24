import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  AnalyticsService,
  DashboardAnalyticsDto,
  ChartItemDto,
  DoctorKpiDto
} from '../../../../core/services/analytics.service';

type PresetKey = '7' | '30' | '90' | 'custom';

@Component({
  selector: 'app-admin-statistics',
  standalone: true,
  imports: [CommonModule, DecimalPipe, FormsModule],
  template: `
    <section class="dash">
      <!-- HEADER -->
      <div class="dash-header">
        <div>
          <p class="eyebrow">Адмін-панель</p>
          <h1>Аналітика та статистика</h1>
        </div>
        <!-- DATE FILTER -->
        <div class="filter-bar">
          @for (p of presets; track p.key) {
            <button class="preset-btn" [class.active]="activePreset() === p.key" (click)="applyPreset(p.key)">
              {{ p.label }}
            </button>
          }
          <div class="custom-range">
            <input type="date" [(ngModel)]="customStart" [max]="customEnd" />
            <span>—</span>
            <input type="date" [(ngModel)]="customEnd" [min]="customStart" />
            <button class="preset-btn" (click)="applyCustom()">Застосувати</button>
          </div>
        </div>
      </div>

      <!-- LOADING / ERROR -->
      @if (isLoading()) {
        <div class="state-box"><div class="spinner"></div><p>Завантаження...</p></div>
      } @else if (error()) {
        <div class="state-box error"><p>{{ error() }}</p></div>
      } @else if (data(); as d) {

        <!-- SUMMARY CARDS -->
        <div class="summary-grid">
          <div class="sum-card">
            <div class="sum-icon" style="background:#dbeafe">💰</div>
            <div>
              <p class="sum-label">Прибуток</p>
              <p class="sum-value">{{ d.summary.totalRevenue | number:'1.0-0' }} грн</p>
            </div>
          </div>
          <div class="sum-card">
            <div class="sum-icon" style="background:#dcfce7">📋</div>
            <div>
              <p class="sum-label">Всього записів</p>
              <p class="sum-value">{{ d.summary.totalAppointments }}</p>
            </div>
          </div>
          <div class="sum-card">
            <div class="sum-icon" style="background:#d1fae5">✅</div>
            <div>
              <p class="sum-label">Завершено</p>
              <p class="sum-value">{{ d.summary.completedAppointments }}</p>
            </div>
          </div>
          <div class="sum-card">
            <div class="sum-icon" style="background:#fee2e2">❌</div>
            <div>
              <p class="sum-label">Скасовано</p>
              <p class="sum-value">{{ d.summary.cancelledAppointments }}</p>
            </div>
          </div>
          <div class="sum-card">
            <div class="sum-icon" style="background:#fef3c7">📉</div>
            <div>
              <p class="sum-label">Відмови</p>
              <p class="sum-value">{{ d.summary.noShowRate }}%</p>
            </div>
          </div>
        </div>

        <!-- CHARTS GRID -->
        <div class="charts-grid">

          <!-- 1. Revenue by direction (Horizontal Bar) -->
          <div class="chart-card wide">
            <h3 class="chart-title">💵 Дохід по напрямках (грн)</h3>
            @if (d.revenueByDirection.length === 0) {
              <p class="no-data">Немає даних</p>
            } @else {
              <div class="hbar-list">
                @for (item of d.revenueByDirection; track item.label) {
                  <div class="hbar-row">
                    <span class="hbar-label">{{ item.label }}</span>
                    <div class="hbar-track">
                      <div class="hbar-fill" [style.width.%]="pct(item.value, maxRevenue(d.revenueByDirection))"
                           style="background: linear-gradient(90deg,#0ea5e9,#38bdf8)"></div>
                    </div>
                    <span class="hbar-val">{{ item.value | number:'1.0-0' }}</span>
                  </div>
                }
              </div>
            }
          </div>

          <!-- 2. Appointment statuses (Donut SVG) -->
          <div class="chart-card">
            <h3 class="chart-title">📊 Статуси записів</h3>
            @if (d.appointmentStatuses.length === 0) {
              <p class="no-data">Немає даних</p>
            } @else {
              <div class="donut-wrap">
                <svg viewBox="0 0 140 140" class="donut-svg">
                  @for (seg of donutSegments(d.appointmentStatuses); track seg.label; let i = $index) {
                    <circle
                      cx="70" cy="70" r="50"
                      fill="none"
                      [attr.stroke]="statusColors[i % statusColors.length]"
                      stroke-width="28"
                      [attr.stroke-dasharray]="seg.dash + ' ' + seg.gap"
                      [attr.stroke-dashoffset]="seg.offset"
                      transform="rotate(-90 70 70)">
                    </circle>
                  }
                  <text x="70" y="68" text-anchor="middle" font-size="12" font-weight="700" fill="#0f172a">{{ d.summary.totalAppointments }}</text>
                  <text x="70" y="82" text-anchor="middle" font-size="8" fill="#64748b">записів</text>
                </svg>
                <div class="donut-legend">
                  @for (item of d.appointmentStatuses; track item.label; let i = $index) {
                    <div class="legend-row">
                      <span class="legend-dot" [style.background]="statusColors[i % statusColors.length]"></span>
                      <span class="legend-label">{{ statusLabels[item.label] ?? item.label }}</span>
                      <span class="legend-val">{{ item.value }}</span>
                    </div>
                  }
                </div>
              </div>
            }
          </div>

          <!-- 3. Top directions (Vertical Bar) -->
          <div class="chart-card">
            <h3 class="chart-title">🏆 Топ напрямків (записів)</h3>
            @if (d.topDirections.length === 0) {
              <p class="no-data">Немає даних</p>
            } @else {
              <div class="vbar-list">
                @for (item of d.topDirections; track item.label; let i = $index) {
                  <div class="vbar-col">
                    <span class="vbar-val">{{ item.value }}</span>
                    <div class="vbar-track">
                      <div class="vbar-fill"
                           [style.height.%]="pct(item.value, maxRevenue(d.topDirections))"
                           [style.background]="barColors[i % barColors.length]">
                      </div>
                    </div>
                    <span class="vbar-label">{{ item.label }}</span>
                  </div>
                }
              </div>
            }
          </div>

          <!-- 4. Doctor KPI (Horizontal Bar) -->
          <div class="chart-card wide">
            <h3 class="chart-title">👨‍⚕️ KPI лікарів (завершені / всього)</h3>
            @if (d.doctorKpi.length === 0) {
              <p class="no-data">Немає даних</p>
            } @else {
              <div class="hbar-list">
                @for (doc of d.doctorKpi; track doc.doctorName) {
                  <div class="hbar-row">
                    <span class="hbar-label">{{ doc.doctorName }}</span>
                    <div class="hbar-track">
                      <div class="hbar-fill hbar-bg-total" [style.width.%]="pct(doc.totalCount, maxDoctorTotal(d.doctorKpi))"></div>
                      <div class="hbar-fill hbar-fg-complete" [style.width.%]="pct(doc.completedCount, maxDoctorTotal(d.doctorKpi))"></div>
                    </div>
                    <span class="hbar-val">{{ doc.completedCount }}/{{ doc.totalCount }}</span>
                  </div>
                }
              </div>
              <div class="kpi-legend">
                <span class="legend-dot" style="background:#0ea5e9"></span><span>Завершено</span>
                <span class="legend-dot" style="background:#e2e8f0; margin-left:12px"></span><span>Всього</span>
              </div>
            }
          </div>

          <!-- 5. Traffic by month (Line SVG) -->
          <div class="chart-card wide">
            <h3 class="chart-title">📈 Трафік записів по місяцях</h3>
            @if (d.trafficByMonth.length === 0) {
              <p class="no-data">Немає даних</p>
            } @else {
              <div class="line-chart-wrap">
                <svg [attr.viewBox]="'0 0 ' + lineW + ' ' + lineH" class="line-svg" preserveAspectRatio="none">
                  <defs>
                    <linearGradient id="lineGrad" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stop-color="#0ea5e9" stop-opacity="0.3"/>
                      <stop offset="100%" stop-color="#0ea5e9" stop-opacity="0"/>
                    </linearGradient>
                  </defs>
                  <path [attr.d]="lineFillPath(d.trafficByMonth)" fill="url(#lineGrad)"/>
                  <path [attr.d]="linePath(d.trafficByMonth)" fill="none" stroke="#0ea5e9" stroke-width="2.5" stroke-linejoin="round" stroke-linecap="round"/>
                  @for (pt of linePoints(d.trafficByMonth); track pt.x) {
                    <circle [attr.cx]="pt.x" [attr.cy]="pt.y" r="4" fill="#0ea5e9" stroke="#fff" stroke-width="2"/>
                  }
                </svg>
                <div class="line-labels">
                  @for (item of d.trafficByMonth; track item.label) {
                    <span>{{ item.label }}</span>
                  }
                </div>
              </div>
            }
          </div>

        </div>
      }
    </section>
  `,
  styles: [`
    .dash { display:flex; flex-direction:column; gap:24px; padding:0 4px; }
    .dash-header { display:flex; justify-content:space-between; align-items:flex-start; flex-wrap:wrap; gap:16px; }
    .eyebrow { color:#0284c7; font-weight:800; text-transform:uppercase; font-size:.78rem; margin:0; }
    h1 { margin:0; color:#0f172a; font-size:1.8rem; }
    h3 { margin:0; }

    /* Filter */
    .filter-bar { display:flex; flex-wrap:wrap; align-items:center; gap:8px; }
    .preset-btn { padding:7px 14px; border-radius:8px; border:1px solid #cbd5e1; background:#f8fafc; color:#334155; font-weight:600; font-size:.85rem; cursor:pointer; transition:.15s; }
    .preset-btn:hover { background:#e0f2fe; border-color:#0ea5e9; color:#0284c7; }
    .preset-btn.active { background:#0ea5e9; color:#fff; border-color:#0ea5e9; }
    .custom-range { display:flex; align-items:center; gap:6px; }
    .custom-range input { padding:6px 10px; border:1px solid #cbd5e1; border-radius:8px; font-size:.85rem; }

    /* State */
    .state-box { display:flex; flex-direction:column; align-items:center; justify-content:center; gap:12px; min-height:200px; background:#fff; border-radius:18px; border:1px solid #e2e8f0; color:#64748b; font-weight:500; }
    .state-box.error { background:#fff5f5; color:#b91c1c; }
    .spinner { width:36px; height:36px; border:4px solid #e2e8f0; border-top-color:#0ea5e9; border-radius:50%; animation:spin .7s linear infinite; }
    @keyframes spin { to { transform:rotate(360deg); } }

    /* Summary */
    .summary-grid { display:grid; grid-template-columns:repeat(auto-fill,minmax(200px,1fr)); gap:16px; }
    .sum-card { background:#fff; border:1px solid #e2e8f0; border-radius:16px; padding:16px 20px; display:flex; align-items:center; gap:14px; box-shadow:0 4px 12px rgba(15,23,42,.05); }
    .sum-icon { width:44px; height:44px; border-radius:12px; display:flex; align-items:center; justify-content:center; font-size:1.3rem; flex-shrink:0; }
    .sum-label { margin:0; font-size:.8rem; color:#64748b; font-weight:600; text-transform:uppercase; }
    .sum-value { margin:4px 0 0; font-size:1.35rem; font-weight:800; color:#0f172a; }

    /* Charts grid */
    .charts-grid { display:grid; grid-template-columns:repeat(2,1fr); gap:20px; }
    .chart-card { background:#fff; border:1px solid #e2e8f0; border-radius:18px; padding:20px; box-shadow:0 4px 12px rgba(15,23,42,.05); }
    .chart-card.wide { grid-column:1/-1; }
    .chart-title { font-size:1rem; font-weight:700; color:#0f172a; margin-bottom:16px !important; }
    .no-data { text-align:center; color:#94a3b8; padding:32px 0; }

    /* Horizontal Bar */
    .hbar-list { display:flex; flex-direction:column; gap:10px; }
    .hbar-row { display:grid; grid-template-columns:160px 1fr 80px; align-items:center; gap:12px; }
    .hbar-label { font-size:.85rem; color:#334155; font-weight:600; white-space:nowrap; overflow:hidden; text-overflow:ellipsis; }
    .hbar-track { position:relative; height:16px; background:#f1f5f9; border-radius:8px; overflow:hidden; }
    .hbar-fill { position:absolute; top:0; left:0; height:100%; border-radius:8px; transition:width .4s ease; }
    .hbar-bg-total { background:#e2e8f0; width:100%; }
    .hbar-fg-complete { background:linear-gradient(90deg,#0ea5e9,#38bdf8); }
    .hbar-val { font-size:.85rem; font-weight:700; color:#0284c7; text-align:right; }
    .kpi-legend { display:flex; align-items:center; gap:6px; margin-top:12px; font-size:.82rem; color:#64748b; }

    /* Donut */
    .donut-wrap { display:flex; align-items:center; gap:24px; flex-wrap:wrap; }
    .donut-svg { width:140px; height:140px; flex-shrink:0; }
    .donut-legend { display:flex; flex-direction:column; gap:8px; }
    .legend-row { display:flex; align-items:center; gap:8px; font-size:.85rem; }
    .legend-dot { width:12px; height:12px; border-radius:3px; flex-shrink:0; }
    .legend-label { color:#334155; font-weight:600; flex:1; }
    .legend-val { color:#0f172a; font-weight:700; }

    /* Vertical Bar */
    .vbar-list { display:flex; align-items:flex-end; gap:8px; height:160px; padding-top:24px; }
    .vbar-col { display:flex; flex-direction:column; align-items:center; gap:4px; flex:1; height:100%; }
    .vbar-val { font-size:.7rem; font-weight:700; color:#334155; }
    .vbar-track { flex:1; width:100%; background:#f1f5f9; border-radius:6px 6px 0 0; display:flex; align-items:flex-end; overflow:hidden; }
    .vbar-fill { width:100%; border-radius:6px 6px 0 0; transition:height .4s ease; min-height:4px; }
    .vbar-label { font-size:.65rem; color:#64748b; text-align:center; line-height:1.2; max-width:64px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }

    /* Line chart */
    .line-chart-wrap { display:flex; flex-direction:column; gap:6px; }
    .line-svg { width:100%; height:160px; }
    .line-labels { display:flex; justify-content:space-between; padding:0 4px; }
    .line-labels span { font-size:.72rem; color:#94a3b8; text-align:center; }

    @media(max-width:900px) {
      .charts-grid { grid-template-columns:1fr; }
      .chart-card.wide { grid-column:1; }
      .hbar-row { grid-template-columns:120px 1fr 60px; }
    }
  `]
})
export class AdminStatisticsComponent implements OnInit {
  private analyticsService = inject(AnalyticsService);

  data = signal<DashboardAnalyticsDto | null>(null);
  isLoading = signal(false);
  error = signal<string | null>(null);
  activePreset = signal<PresetKey>('30');

  customStart = '';
  customEnd = '';

  readonly lineW = 800;
  readonly lineH = 160;
  readonly PAD = 20;

  readonly presets: { key: PresetKey; label: string }[] = [
    { key: '7', label: 'Тиждень' },
    { key: '30', label: '30 днів' },
    { key: '90', label: '3 місяці' },
    { key: 'custom', label: 'Власний' },
  ];

  readonly statusColors = ['#0ea5e9','#22c55e','#f59e0b','#ef4444','#a78bfa'];
  readonly barColors   = ['#0ea5e9','#38bdf8','#7dd3fc','#bae6fd','#e0f2fe','#0284c7','#0369a1','#075985','#0c4a6e','#082f49'];

  readonly statusLabels: Record<string, string> = {
    CREATED: 'Очікує',
    CONFIRMED: 'Підтверджено',
    CANCELLED: 'Скасовано',
    COMPLETED: 'Завершено',
  };

  ngOnInit(): void {
    this.applyPreset('30');
  }

  applyPreset(key: PresetKey): void {
    if (key === 'custom') { this.activePreset.set('custom'); return; }
    this.activePreset.set(key);
    const end = new Date();
    const start = new Date();
    start.setDate(end.getDate() - parseInt(key, 10));
    this.load(this.fmt(start), this.fmt(end));
  }

  applyCustom(): void {
    if (!this.customStart || !this.customEnd) return;
    this.activePreset.set('custom');
    this.load(this.customStart, this.customEnd);
  }

  private load(start?: string, end?: string): void {
    this.isLoading.set(true);
    this.error.set(null);
    this.analyticsService.getDashboard(start, end).subscribe({
      next: d => { this.data.set(d); this.isLoading.set(false); },
      error: () => { this.error.set('Не вдалося завантажити аналітику.'); this.isLoading.set(false); }
    });
  }

  private fmt(d: Date): string {
    return d.toISOString().split('T')[0];
  }

  pct(val: number, max: number): number {
    return max > 0 ? Math.round((val / max) * 100) : 0;
  }

  maxRevenue(items: ChartItemDto[]): number {
    return items.reduce((m, i) => Math.max(m, i.value), 0);
  }

  maxDoctorTotal(items: DoctorKpiDto[]): number {
    return items.reduce((m, i) => Math.max(m, i.totalCount), 0);
  }

  donutSegments(items: ChartItemDto[]): { label: string; dash: number; gap: number; offset: number }[] {
    const circ = 2 * Math.PI * 50;
    const total = items.reduce((s, i) => s + i.value, 0);
    let offset = 0;
    return items.map(item => {
      const dash = total > 0 ? (item.value / total) * circ : 0;
      const seg = { label: item.label, dash, gap: circ - dash, offset: -offset };
      offset += dash;
      return seg;
    });
  }

  linePoints(items: ChartItemDto[]): { x: number; y: number }[] {
    if (items.length === 0) return [];
    const max = Math.max(...items.map(i => i.value), 1);
    const w = this.lineW - this.PAD * 2;
    const h = this.lineH - this.PAD * 2;
    return items.map((item, idx) => ({
      x: this.PAD + (idx / Math.max(items.length - 1, 1)) * w,
      y: this.PAD + (1 - item.value / max) * h
    }));
  }

  linePath(items: ChartItemDto[]): string {
    const pts = this.linePoints(items);
    if (pts.length === 0) return '';
    return pts.map((p, i) => `${i === 0 ? 'M' : 'L'}${p.x},${p.y}`).join(' ');
  }

  lineFillPath(items: ChartItemDto[]): string {
    const pts = this.linePoints(items);
    if (pts.length === 0) return '';
    const line = pts.map((p, i) => `${i === 0 ? 'M' : 'L'}${p.x},${p.y}`).join(' ');
    const last = pts[pts.length - 1];
    const first = pts[0];
    return `${line} L${last.x},${this.lineH} L${first.x},${this.lineH} Z`;
  }
}
