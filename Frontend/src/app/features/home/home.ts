import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import {LoginComponent} from '../auth/login/login';

@Component({
  selector: 'app-home',
  imports: [RouterLink], // ДОДАНО: імпорт для роботи маршрутизації в HTML
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class HomeComponent {}
