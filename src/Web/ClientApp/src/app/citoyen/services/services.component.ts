import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ServicesClient, ServiceDto } from '../../web-api-client';

@Component({
  selector: 'app-citoyen-services',
  standalone: true,
  imports: [CommonModule, RouterModule, CardModule, ButtonModule, MessageModule, ProgressSpinnerModule],
  templateUrl: './services.component.html'
})
export class ServicesComponent implements OnInit {
  loading = true;
  errorMessage: string | null = null;
  services: ServiceDto[] = [];

  constructor(private readonly servicesClient: ServicesClient) {}

  ngOnInit(): void {
    this.servicesClient.getServices().subscribe({
      next: (data) => {
        this.services = data || [];
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Impossible de charger les services.';
        this.loading = false;
      }
    });
  }
}
