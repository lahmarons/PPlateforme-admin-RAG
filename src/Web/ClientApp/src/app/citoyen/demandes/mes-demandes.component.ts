import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { MessageModule } from 'primeng/message';
import { ButtonModule } from 'primeng/button';
import { firstValueFrom } from 'rxjs';
import { DemandesClient, DemandeDto } from '../../web-api-client';

@Component({
  selector: 'app-citoyen-mes-demandes',
  standalone: true,
  imports: [CommonModule, RouterModule, CardModule, TableModule, TagModule, MessageModule, ButtonModule],
  templateUrl: './mes-demandes.component.html'
})
export class MesDemandesComponent implements OnInit {
  loading = true;
  demandes: DemandeDto[] = [];
  errorMessage: string | null = null;

  constructor(
    private readonly demandesClient: DemandesClient
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      const page = await firstValueFrom(this.demandesClient.getDemandes(1, 100));
      this.demandes = page?.items ?? [];
    } catch {
      this.errorMessage = 'Impossible de charger les demandes.';
    } finally {
      this.loading = false;
    }
  }
}
