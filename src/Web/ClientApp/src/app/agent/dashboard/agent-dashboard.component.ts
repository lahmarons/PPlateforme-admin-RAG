import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { TagModule } from 'primeng/tag';
import { catchError, firstValueFrom, of } from 'rxjs';
import { AuthService } from '../../shared/auth/auth.service';
import { DemandeDto, DemandesClient } from '../../web-api-client';
import { AgentSessionService } from '../services/agent-session.service';

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
  demandes: DemandeDto[] = [];

  constructor(
    public readonly session: AgentSessionService,
    private readonly authService: AuthService,
    private readonly demandesClient: DemandesClient
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      const currentUser = await firstValueFrom(this.authService.me().pipe(catchError(() => of(null))));

      if (!currentUser?.roles?.includes('Agent')) {
        this.infoMessage = 'Session agent non confirmée. Connectez-vous avec un compte agent.';
        return;
      }

      this.session.markConnected();

      const demandesPage = await firstValueFrom(this.demandesClient.getDemandes(1, 200));
      const demandes = (demandesPage as any)?.items ?? [];
      this.demandes = demandes;
      this.demandesEnAttente = demandes.filter((d: DemandeDto) => d.statut === 0 || d.statut === 1).length;
      this.demandesTraitees = demandes.filter((d: DemandeDto) => d.statut === 2).length;

      if (!demandes.length) {
        this.infoMessage = 'Aucune demande disponible pour le moment.';
      }
    } catch {
      this.errorMessage = 'Impossible de charger les demandes du tableau agent.';
    } finally {
      this.loading = false;
    }
  }

  statusSeverity(statut: number): 'warn' | 'success' | 'danger' | 'info' {
    switch (statut) {
      case 2:
        return 'success';
      case 3:
        return 'danger';
      case 1:
        return 'info';
      default:
        return 'warn';
    }
  }
}
