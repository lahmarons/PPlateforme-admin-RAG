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
import { CitoyenDemandesStoreService } from '../services/citoyen-demandes-store.service';

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
    private readonly demandesClient: DemandesClient,
    private readonly store: CitoyenDemandesStoreService
  ) {}

  async ngOnInit(): Promise<void> {
    const ids = this.store.demandeIds();

    if (!ids.length) {
      this.loading = false;
      return;
    }

    try {
      const results = await Promise.all(ids.map((id) => firstValueFrom(this.demandesClient.getDemandeById(id))));
      this.demandes = results.filter(Boolean) as DemandeDto[];
    } catch {
      this.errorMessage = 'Impossible de charger les demandes.';
    } finally {
      this.loading = false;
    }
  }
}
