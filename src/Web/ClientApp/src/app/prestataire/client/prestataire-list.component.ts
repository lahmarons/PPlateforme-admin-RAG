import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { PrestataireDto } from '../../web-api-client';
import { PrestataireApiService } from '../services/prestataire-api.service';

@Component({
  selector: 'app-prestataire-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ButtonModule, CardModule, InputTextModule, MessageModule],
  templateUrl: './prestataire-list.component.html'
})
export class PrestataireListComponent implements OnInit {
  loading = true;
  errorMessage: string | null = null;
  prestataires: PrestataireDto[] = [];
  recherche = '';

  constructor(private readonly api: PrestataireApiService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.errorMessage = null;

    this.api.getPrestataires(this.recherche).subscribe({
      next: (data) => {
        this.prestataires = data ?? [];
        this.loading = false;
      },
      error: (err: Error) => {
        this.errorMessage = err.message;
        this.loading = false;
      }
    });
  }

  applyFilters(): void {
    this.load();
  }

  clearFilters(): void {
    this.recherche = '';
    this.load();
  }
}
