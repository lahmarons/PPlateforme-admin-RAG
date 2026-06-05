import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { ProgressBarModule } from 'primeng/progressbar';
import { TagModule } from 'primeng/tag';
import { catchError, firstValueFrom, of } from 'rxjs';
import { AuthService } from '../../shared/auth/auth.service';
import { AdminsClient, AgentsClient, DemandesClient, ServicesClient } from '../../web-api-client';
import { AdminSessionService } from '../services/admin-session.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, ButtonModule, CardModule, MessageModule, ProgressBarModule, TagModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss'
})
export class AdminDashboardComponent implements OnInit {
  loading = true;
  errorMessage: string | null = null;
  infoMessage: string | null = null;
  totalAdmins = 0;
  totalAgents = 0;
  activeAgents = 0;
  totalServices = 0;
  activeServices = 0;
  totalDemandes = 0;
  demandesEnAttente = 0;
  demandesTraitees = 0;
  demandesRefusees = 0;

  constructor(
    public readonly session: AdminSessionService,
    private readonly authService: AuthService,
    private readonly adminsClient: AdminsClient,
    private readonly agentsClient: AgentsClient,
    private readonly servicesClient: ServicesClient,
    private readonly demandesClient: DemandesClient
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      const currentUser = await firstValueFrom(this.authService.me().pipe(catchError(() => of(null))));

      if (!currentUser?.roles?.includes('Administrator')) {
        this.infoMessage = 'Session administrateur non confirmée. Connectez-vous avec un compte administrateur.';
        return;
      }

      this.session.markConnected();

      const [admins, agents, services, demandesPage] = await Promise.all([
        firstValueFrom(this.adminsClient.getAdmins()),
        firstValueFrom(this.agentsClient.getAgents()),
        firstValueFrom(this.servicesClient.getServices()),
        firstValueFrom(this.demandesClient.getDemandes(1, 200))
      ]);

      this.totalAdmins = admins.length;
      this.totalAgents = agents.length;
      this.activeAgents = agents.filter(a => (a as any).isActive !== false).length;
      this.totalServices = services.length;
      this.activeServices = services.filter(s => (s as any).isActive !== false).length;

      const demandes = (demandesPage as any)?.items ?? [];
      this.totalDemandes = demandes.length;
      this.demandesEnAttente = demandes.filter((d: any) => d.statut === 0 || d.statut === 1).length;
      this.demandesTraitees = demandes.filter((d: any) => d.statut === 2).length;
      this.demandesRefusees = demandes.filter((d: any) => d.statut === 3).length;
    } catch {
      this.errorMessage = 'Impossible de charger les indicateurs du dashboard administrateur.';
    } finally {
      this.loading = false;
    }
  }

  get agentActivationRate(): number {
    return this.totalAgents ? Math.round((this.activeAgents / this.totalAgents) * 100) : 0;
  }

  get serviceActivationRate(): number {
    return this.totalServices ? Math.round((this.activeServices / this.totalServices) * 100) : 0;
  }

  get demandePendingRate(): number {
    return this.totalDemandes ? Math.round((this.demandesEnAttente / this.totalDemandes) * 100) : 0;
  }

  get demandeProcessedRate(): number {
    return this.totalDemandes ? Math.round((this.demandesTraitees / this.totalDemandes) * 100) : 0;
  }

  get demandeRefusedRate(): number {
    return this.totalDemandes ? Math.round((this.demandesRefusees / this.totalDemandes) * 100) : 0;
  }

  get maxEntityCount(): number {
    return Math.max(1, this.totalAdmins, this.totalAgents, this.totalServices, this.totalDemandes);
  }

  entityBarWidth(count: number): string {
    return `${Math.max(8, Math.round((count / this.maxEntityCount) * 100))}%`;
  }
}
