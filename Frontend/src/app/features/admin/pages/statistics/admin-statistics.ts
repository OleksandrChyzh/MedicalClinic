import { Component } from '@angular/core';

@Component({
  selector: 'app-admin-statistics',
  standalone: true,
  template: `
    <section class="admin-page">
      <div class="page-header"><div><p class="eyebrow">Адмін-панель</p><h1>Статистика</h1></div></div>
      <div class="panel placeholder">
        <h2>Сторінка статистики</h2>
        <p>Тут буде реалізовано функціонал аналітики та звітів.</p>
        <p>Ця сторінка буде додана пізніше.</p>
      </div>
    </section>
  `,
  styles: [`.admin-page{display:flex;flex-direction:column;gap:20px}.page-header{display:flex;justify-content:space-between}.eyebrow{color:#0284c7;font-weight:800;text-transform:uppercase;font-size:.78rem}h1,h2{margin:0;color:#0f172a}.panel{background:#fff;border:1px solid #e2e8f0;border-radius:18px;padding:18px;box-shadow:0 10px 25px rgba(15,23,42,.05)}.placeholder{min-height:200px;display:flex;flex-direction:column;align-items:center;justify-content:center;text-align:center;color:#64748b}`]
})
export class AdminStatisticsComponent {}
