import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { TagModule } from 'primeng/tag';
import { firstValueFrom } from 'rxjs';
import { AdminsClient, AgentsClient, DemandesClient, ServicesClient } from '../../web-api-client';
import { AdminSessionService } from '../services/admin-session.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, ButtonModule, CardModule, MessageModule, TagModule],
  templateUrl: './admin-dashboard.component.html'
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

  constructor(
    public readonly session: AdminSessionService,
    private readonly adminsClient: AdminsClient,
    private readonly agentsClient: AgentsClient,
    private readonly servicesClient: ServicesClient,
    private readonly demandesClient: DemandesClient
  ) {}

  async ngOnInit(): Promise<void> {
    try {
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

      if (!this.session.isConnected()) {
        this.infoMessage = 'Connectez-vous pour administrer les services et les comptes agents.';
      }
    } catch {
      this.errorMessage = 'Impossible de charger les indicateurs du dashboard administrateur.';
    } finally {
      this.loading = false;
    }
  }
}
