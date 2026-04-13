import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { CitoyenSessionService } from '../services/citoyen-session.service';

@Component({
  selector: 'app-citoyen-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, CardModule, ButtonModule, MessageModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent {
  constructor(public readonly session: CitoyenSessionService) {}
}
