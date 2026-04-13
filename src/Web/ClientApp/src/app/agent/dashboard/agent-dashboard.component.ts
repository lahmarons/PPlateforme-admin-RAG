import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { TagModule } from 'primeng/tag';
import { catchError, firstValueFrom, of } from 'rxjs';
import { DemandeDto, DemandesClient } from '../../web-api-client';
import { AgentSessionService } from '../services/agent-session.service';
import { CitoyenDemandesStoreService } from '../../citoyen/services/citoyen-demandes-store.service';

@Component({
  selector: 'app-agent-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, ButtonModule, MessageModule, TagModule],
  templateUrl: './agent-dashboard.component.html'
})
export class AgentDashboardComponent implements OnInit {
  loading = true;
  errorMessage: string | null = null;
  infoMessage: string | null = null;
  demandesEnAttente = 0;
  demandesTraitees = 0;

  constructor(
    public readonly session: AgentSessionService,
    private readonly demandesClient: DemandesClient,
    private readonly demandesStore: CitoyenDemandesStoreService
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      const ids = this.demandesStore.demandeIds();

      if (!ids.length) {
        this.infoMessage = 'Aucune demande connue côté frontend. Ajoutez un endpoint GET /api/Demandes pour un tableau agent complet.';
        return;
      }

      const demandes = (await Promise.all(
        ids.map((id) => firstValueFrom(this.demandesClient.getDemandeById(id).pipe(catchError(() => of(null)))))
      )).filter(Boolean) as DemandeDto[];

      this.demandesEnAttente = demandes.filter(d => d.statut === 0 || d.statut === 1).length;
      this.demandesTraitees = demandes.filter(d => d.statut === 2).length;
    } catch {
      this.errorMessage = 'Impossible de charger les indicateurs du dashboard agent.';
    } finally {
      this.loading = false;
    }
  }
}
